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

    // Constante que define el tamaño de pagina para la paginación de reservas

    private const int TamanioPagina = 3;

    public async Task<IActionResult> Index(int pagina = 1)
    {
      if (pagina < 1) pagina = 1;

      var query =
          from r in _context.Reservas
          join i in _context.Inquilinos on r.InquilinoId equals i.Id
          join im in _context.Inmuebles on r.InmuebleId equals im.Id
          orderby r.Id
          select new
          {
            Reserva = r,
            NombreInquilino = i.Nombre + " " + i.Apellido,
            DireccionInmueble = im.Direccion
          };

      var totalReservas = await query.CountAsync();
      var totalPaginas = (int)Math.Ceiling(totalReservas / (double)TamanioPagina);

      var datos = await query
          .Skip((pagina - 1) * TamanioPagina)
          .Take(TamanioPagina)
          .ToListAsync();

      var reservas = datos.Select(d =>
      {
        d.Reserva.NombreInquilino = d.NombreInquilino;
        d.Reserva.DireccionInmueble = d.DireccionInmueble;
        return d.Reserva;
      }).ToList();

      ViewBag.PaginaActual = pagina;
      ViewBag.TotalPaginas = totalPaginas;

      return View(reservas);
    }
    // Recupera los nombres del inquilino y del inmueble asociados a una reserva
    // Completa NombreInquilino y DireccionInmueble de una reserva
    // para mostrarlos en las vistas sin duplicar la consulta en cada accion.
    private async Task CargarNombresDeReserva(Reserva reserva)
    {
      var inquilino = await _context.Inquilinos.FindAsync(reserva.InquilinoId);
      var inmueble = await _context.Inmuebles
          .Where(i => i.Id == reserva.InmuebleId)
          .Select(i => new { i.Id, i.Direccion })
          .FirstOrDefaultAsync();

      reserva.NombreInquilino = inquilino != null ? $"{inquilino.Nombre} {inquilino.Apellido}" : "-";
      reserva.DireccionInmueble = inmueble != null ? inmueble.Direccion : "-";
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

      await CargarNombresDeReserva(reserva);

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
    public async Task<IActionResult> Create(int? inmuebleId, DateTime? desde, DateTime? hasta)
    {
      await CargarListas();

      var reserva = new Reserva();

      if (inmuebleId.HasValue)
      {
        reserva.InmuebleId = inmuebleId.Value;
      }

      if (desde.HasValue)
      {
        reserva.FechaDesde = desde.Value;
      }

      if (hasta.HasValue)
      {
        reserva.FechaHasta = hasta.Value;
      }

      return View(reserva);
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
          r.Estado != EstadoReserva.Cancelada &&
          r.FechaDesde < fechaHasta &&
          r.FechaHasta > fechaDesde);

      if (reservaIdAExcluir.HasValue)
      {
        query = query.Where(r => r.Id != reservaIdAExcluir.Value);
      }

      return await query.AnyAsync();
    }
    // Informe: dado un rango de fechas, lista los inmuebles disponibles
    // (Disponible == true) que no tienen ninguna reserva activa solapada.
    public async Task<IActionResult> InmueblesLibres(DateTime? desde, DateTime? hasta)
    {
      if (desde == null || hasta == null)
      {
        return View(new List<Inmueble>());
      }

      var fechaDesde = desde.Value.Date;
      var fechaHasta = hasta.Value.Date;

      if (fechaHasta <= fechaDesde)
      {
        ViewBag.Error = "La fecha hasta debe ser posterior a la fecha desde";
        return View(new List<Inmueble>());
      }

      var libres = await _context.Inmuebles
          .Include(im => im.TipoInmueble)
          .Include(im => im.Propietario)
          .Where(im =>
              im.Disponible &&
              !_context.Reservas.Any(r =>
                  r.InmuebleId == im.Id &&
                  r.Estado != EstadoReserva.Cancelada &&
                  r.FechaDesde < fechaHasta &&
                  r.FechaHasta > fechaDesde))
          .OrderBy(im => im.Direccion)
          .ToListAsync();

      ViewBag.Desde = fechaDesde;
      ViewBag.Hasta = fechaHasta;

      return View(libres);
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
          .ToListAsync();

      ViewBag.Inmuebles = inmuebles;
    }
    // Informe: lista las reservas actualmente vigentes (Estado == Confirmada)
    // cuya fecha de fin todavia no llego.
    public async Task<IActionResult> Vigentes()
    {
      var hoy = DateTime.Today;

      var datos = await (
          from r in _context.Reservas
          join i in _context.Inquilinos on r.InquilinoId equals i.Id
          join im in _context.Inmuebles on r.InmuebleId equals im.Id
          where r.Estado == EstadoReserva.Confirmada && r.FechaHasta >= hoy
          orderby r.FechaDesde
          select new
          {
            Reserva = r,
            NombreInquilino = i.Nombre + " " + i.Apellido,
            DireccionInmueble = im.Direccion
          }
      ).ToListAsync();

      var reservas = datos.Select(d =>
      {
        d.Reserva.NombreInquilino = d.NombreInquilino;
        d.Reserva.DireccionInmueble = d.DireccionInmueble;
        return d.Reserva;
      }).ToList();

      return View(reservas);
    }

    public async Task<IActionResult> TerminanEn(int dias = 30)
    {
      var hoy = DateTime.Today;
      var fechaLimite = hoy.AddDays(dias);

      var datos = await (
          from r in _context.Reservas
          join i in _context.Inquilinos on r.InquilinoId equals i.Id
          join im in _context.Inmuebles on r.InmuebleId equals im.Id
          where r.Estado == EstadoReserva.Confirmada
             && r.FechaHasta >= hoy
             && r.FechaHasta <= fechaLimite
          orderby r.FechaHasta
          select new
          {
            Reserva = r,
            NombreInquilino = i.Nombre + " " + i.Apellido,
            DireccionInmueble = im.Direccion
          }
      ).ToListAsync();

      var reservas = datos.Select(d =>
      {
        d.Reserva.NombreInquilino = d.NombreInquilino;
        d.Reserva.DireccionInmueble = d.DireccionInmueble;
        return d.Reserva;
      }).ToList();

      ViewBag.Dias = dias;

      return View(reservas);
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

      await CargarNombresDeReserva(reserva);

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
    // Presenta el formulario para renovar una reserva existente
    public async Task<IActionResult> Renovar(int? id)
    {
      if (id == null) return NotFound();

      var original = await _context.Reservas.FindAsync(id);

      if (original == null) return NotFound();

      if (original.Estado != EstadoReserva.Confirmada)
      {
        return RedirectToAction(nameof(Index));
      }

      var nueva = new Reserva
      {
        InquilinoId = original.InquilinoId,
        InmuebleId = original.InmuebleId,
        FechaDesde = original.FechaHasta,
        MontoDiario = original.MontoDiario,
        CantidadPersonas = original.CantidadPersonas
      };

      ViewBag.ReservaOriginalId = original.Id;

      return View(nueva);
    }
    // Procesa la renovación de una reserva existente, validando fechas y disponibilidad
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Renovar(int reservaOriginalId, Reserva reserva)
    {
      var original = await _context.Reservas.FindAsync(reservaOriginalId);

      if (original == null) return NotFound();

      if (original.Estado != EstadoReserva.Confirmada)
      {
        return RedirectToAction(nameof(Index));
      }

      if (reserva.FechaHasta <= reserva.FechaDesde)
      {
        ModelState.AddModelError("FechaHasta", "La fecha hasta debe ser posterior a la fecha desde");
      }

      if (reserva.FechaDesde < original.FechaHasta)
      {
        ModelState.AddModelError("FechaDesde", "La nueva reserva no puede empezar antes de que termine la original");
      }

      if (ModelState.IsValid && await InmuebleOcupado(original.InmuebleId, reserva.FechaDesde, reserva.FechaHasta, null))
      {
        ModelState.AddModelError("FechaHasta", "El inmueble ya esta reservado en esas fechas");
      }

      if (!ModelState.IsValid)
      {
        ViewBag.ReservaOriginalId = reservaOriginalId;
        return View(reserva);
      }

      var nueva = new Reserva
      {
        InquilinoId = original.InquilinoId,
        InmuebleId = original.InmuebleId,
        FechaDesde = reserva.FechaDesde,
        FechaHasta = reserva.FechaHasta,
        MontoDiario = reserva.MontoDiario,
        CantidadPersonas = reserva.CantidadPersonas,
        Estado = EstadoReserva.Confirmada,
        UsuarioCreadorId = ObtenerIdUsuarioActual()
      };

      _context.Reservas.Add(nueva);
      await _context.SaveChangesAsync();

      return RedirectToAction(nameof(Index));
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