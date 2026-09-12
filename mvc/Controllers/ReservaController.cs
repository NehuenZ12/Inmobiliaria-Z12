using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using mvc.Models;

namespace mvc.Controllers
{
  // Controlador de reservas que gestiona el listado, detalle, creación,
  // validación de disponibilidad y terminación de una reserva asociada a
  // un inquilino, un inmueble y un usuario creador/terminador.
  [Authorize]
  public class ReservaController : Controller
  {
    private readonly AppDbContext _context;

    // Constructor del controlador: recibe el contexto de base de datos
    // para consultar inquilinos, inmuebles, reservas, pagos y usuarios.
    public ReservaController(AppDbContext context)
    {
      _context = context;
    }

    // Muestra el listado principal de reservas con datos derivados como
    // nombre del inquilino y dirección del inmueble para facilitar la visualización.
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

    // Recupera una reserva específica y complementa su vista con los nombres
    // de usuario creador y terminador para mantener un registro de auditoría.
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

    // Presenta el formulario para crear una nueva reserva y precarga
    // las listas desplegables de inquilinos e inmuebles disponibles.
    public async Task<IActionResult> Create()
    {
      await CargarListas();

      return View();
    }

    // Procesa el envío del formulario de creación, validando fechas,
    // disponibilidad del inmueble y registrando el usuario creador autenticado.
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
        reserva.UsuarioCreadorId = ObtenerIdUsuarioActual();
        reserva.Estado = EstadoReserva.Pendiente;

        _context.Reservas.Add(reserva);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
      }

      await CargarListas();

      return View(reserva);
    }


    // Verifica si el inmueble ya tiene otra reserva solapada en el rango de fechas.
    // La reserva actual puede excluirse de la consulta al editar para evitar falsos positivos.
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


    // Carga las listas para los selectores de inquilinos e inmuebles
    // y las deja disponibles en la vista por medio de ViewBag.
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
    }

    // Muestra la pantalla de finalización de una reserva, calculando
    // la multa posible de acuerdo con el tiempo restante y la fecha de corte.
    public async Task<IActionResult> Cancelar(int? id)
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
    public async Task<IActionResult> CheckIn(int? id)
    {
      if (id == null) return NotFound();

      var reserva = await _context.Reservas.FindAsync(id);

      if (reserva == null) return NotFound();

      if (reserva.Estado != EstadoReserva.Pendiente)
      {
        return RedirectToAction(nameof(Index));
      }

      return View(reserva);
    }

    [HttpPost, ActionName("CheckIn")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckInConfirmado(int id)
    {
      var reserva = await _context.Reservas.FindAsync(id);

      if (reserva == null) return NotFound();

      if (reserva.Estado != EstadoReserva.Pendiente)
      {
        return RedirectToAction(nameof(Index));
      }

      reserva.Estado = EstadoReserva.Confirmada;

      await _context.SaveChangesAsync();

      return RedirectToAction(nameof(Index));
    }

    // Confirma la terminación de la reserva, genera el pago por multa
    // si aplica y registra al usuario que finaliza la operación.
    [HttpPost, ActionName("Cancelar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelarConfirmado(int id)
    {
      var reserva = await _context.Reservas.FindAsync(id);

      if (reserva == null) return NotFound();

      if (reserva.FechaTerminacion != null)
      {
        return RedirectToAction(nameof(Index));
      }

      var fechaTerminacion = DateTime.Today;
      var (multa, _) = CalcularMulta(reserva, fechaTerminacion);
      var usuarioActual = ObtenerIdUsuarioActual();

      var pago = new Pago
      {
        Fecha = fechaTerminacion,
        Concepto = "Multa por terminacion anticipada de reserva",
        Importe = multa,
        ReservaId = reserva.Id,
        Metodo = MetodoPago.Efectivo,
        UsuarioCreadorId = usuarioActual
      };

      _context.Pagos.Add(pago);

      reserva.FechaTerminacion = fechaTerminacion;
      reserva.UsuarioTerminadorId = usuarioActual;
      reserva.Estado = EstadoReserva.Cancelada;

      await _context.SaveChangesAsync();
      return RedirectToAction(nameof(Index));
    }

    // Calcula la multa por terminación anticipada usando el porcentaje
    // y el monto restante según la fracción del período reservada.
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

    // Sección auxiliar del controlador.

    // Recupera el identificador del usuario autenticado desde el claim
    // de identidad del sistema para registrar auditoría en reservas y pagos.
    private int ObtenerIdUsuarioActual()
    {
      var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      return int.TryParse(idClaim, out int id) ? id : 0;
    }
  }
}