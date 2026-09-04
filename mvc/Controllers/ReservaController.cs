using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using mvc.Models;

namespace mvc.Controllers
{
  public class ReservaController : Controller
  {
    private readonly AppDbContext _context;

    public ReservaController(AppDbContext context)
    {
      _context = context;
    }

    // LISTAR RESERVAS
    public async Task<IActionResult> Index()
    {
      var reservas = await _context.Reservas.ToListAsync();

      foreach (var reserva in reservas)
      {
        var inquilino = await _context.Inquilinos.FindAsync(reserva.InquilinoId);
        var inmueble = await _context.Inmuebles
            .Where(i => i.Id == reserva.InmuebleId)
            .Select(i => new { i.Id, i.Direccion })
            .FirstOrDefaultAsync();

        reserva.NombreInquilino = inquilino != null ? $"{inquilino.Nombre} {inquilino.Apellido}" : "-";
        reserva.DireccionInmueble = inmueble != null ? inmueble.Direccion : "-";
      }

      return View(reservas);
    }

    // DETALLES DE UNA RESERVA (con auditoria)
    public async Task<IActionResult> Detalles(int id)
    {
      var reserva = await _context.Reservas.FindAsync(id);

      if (reserva == null)
      {
        return NotFound();
      }

      var idsUsuarios = new List<int> { reserva.UsuarioCreadorId };
      if (reserva.UsuarioTerminadorId.HasValue)
      {
        idsUsuarios.Add(reserva.UsuarioTerminadorId.Value);
      }

      var nombres = await _context.Usuarios
          .Where(u => idsUsuarios.Contains(u.Id))
          .ToDictionaryAsync(u => u.Id, u => u.NombreCompleto);

      reserva.NombreUsuarioCreador = nombres.GetValueOrDefault(reserva.UsuarioCreadorId);
      reserva.NombreUsuarioTerminador = reserva.UsuarioTerminadorId.HasValue
          ? nombres.GetValueOrDefault(reserva.UsuarioTerminadorId.Value)
          : null;

      return View(reserva);
    }

    // CREAR RESERVA

    public async Task<IActionResult> Create()
    {
      await CargarListas();

      return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Reserva reserva)
    {
      if (reserva.FechaHasta <= reserva.FechaDesde)
      {
        ModelState.AddModelError("FechaHasta", "La fecha hasta debe ser posterior a la fecha desde");
      }

      if (ModelState.IsValid && await InmuebleOcupado(reserva.InmuebleId, reserva.FechaDesde, reserva.FechaHasta, null))
      {
        ModelState.AddModelError("InmuebleId", "El inmueble ya esta reservado en esas fechas");
      }

      if (ModelState.IsValid)
      {
        reserva.UsuarioCreadorId = 1;

        _context.Reservas.Add(reserva);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
      }

      await CargarListas();

      return View(reserva);
    }


    private async Task<bool> InmuebleOcupado(int inmuebleId, DateTime fechaDesde, DateTime fechaHasta, int? reservaIdAExcluir)
    {
      var query = _context.Reservas.Where(r =>
          r.InmuebleId == inmuebleId &&
          r.FechaDesde < fechaHasta &&
          r.FechaHasta > fechaDesde);

      if (reservaIdAExcluir.HasValue)
      {
        query = query.Where(r => r.Id != reservaIdAExcluir.Value);
      }

      return await query.AnyAsync();
    }


    private async Task CargarListas()
    {
      var inquilinos = await _context.Inquilinos
          .OrderBy(i => i.Apellido)
          .ThenBy(i => i.Nombre)
          .ToListAsync();

      ViewBag.Inquilinos = new SelectList(inquilinos, "Id", "Apellido");

      var inmuebles = await _context.Inmuebles
          .OrderBy(i => i.Direccion)
          .Select(i => new { i.Id, i.Direccion })
          .ToListAsync();

      ViewBag.Inmuebles = new SelectList(inmuebles, "Id", "Direccion");

      ViewBag.Usuarios = new List<Usuario>
      {
        new Usuario { Id = 1, Nombre = "Provisorio", Apellido = "Provisorio" }
      };
    }

    public async Task<IActionResult> Terminar(int? id)
    {
      if (id == null) return NotFound();

      var reserva = await _context.Reservas.FindAsync(id);

      if (reserva == null) return NotFound();

      if (reserva.FechaTerminacion != null)
      {
        return RedirectToAction(nameof(Index));
      }

      var (multa, porcentaje) = CalcularMulta(reserva, DateTime.Today);

      ViewBag.Multa = multa;
      ViewBag.Porcentaje = porcentaje;

      return View(reserva);
    }

    [HttpPost, ActionName("Terminar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TerminarConfirmado(int id)
    {
      var reserva = await _context.Reservas.FindAsync(id);

      if (reserva == null) return NotFound();

      if (reserva.FechaTerminacion != null)
      {
        return RedirectToAction(nameof(Index));
      }

      var fechaTerminacion = DateTime.Today;
      var (multa, _) = CalcularMulta(reserva, fechaTerminacion);

      var pago = new Pago
      {
        Fecha = fechaTerminacion,
        Concepto = "Multa por terminacion anticipada de reserva",
        Importe = multa,
        ReservaId = reserva.Id,
        Anulado = false,
        Metodo = MetodoPago.Efectivo,
        UsuarioCreadorId = 1
      };

      _context.Pagos.Add(pago);

      reserva.FechaTerminacion = fechaTerminacion;
      reserva.UsuarioTerminadorId = 1;

      await _context.SaveChangesAsync();

      return RedirectToAction(nameof(Index));
    }

    private (decimal multa, int porcentaje) CalcularMulta(Reserva reserva, DateTime fechaTerminacion)
    {
      double diasTotales = (reserva.FechaHasta - reserva.FechaDesde).TotalDays;
      DateTime mitadDelPeriodo = reserva.FechaDesde.AddDays(diasTotales / 2);

      int porcentaje = fechaTerminacion < mitadDelPeriodo ? 50 : 25;

      double diasRestantes = (reserva.FechaHasta - fechaTerminacion).TotalDays;
      if (diasRestantes < 0) diasRestantes = 0;

      decimal montoRestante = (decimal)diasRestantes * reserva.MontoDiario;
      decimal multa = montoRestante * (porcentaje / 100m);

      return (multa, porcentaje);
    }
  }
}