using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using mvc.Models;

namespace mvc.Controllers
{
    [Authorize]
    public class PagosController : Controller
    {
        private readonly AppDbContext _context;

        public PagosController(AppDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // LISTAR PAGOS POR RESERVA
        // =====================================================
        public async Task<IActionResult> Index(int idReserva)
        {
            var pagos = await _context.Pagos
                .Where(p => p.ReservaId == idReserva)
                .OrderByDescending(p => p.Fecha)
                .ThenByDescending(p => p.Id)
                .ToListAsync();

            // Cargamos nombres de usuarios para la auditoría
            var idsUsuarios = pagos.Select(p => p.UsuarioCreadorId)
                .Union(pagos.Where(p => p.UsuarioAnuladorId.HasValue).Select(p => p.UsuarioAnuladorId!.Value))
                .Distinct();

            var nombres = await _context.Usuarios
                .Where(u => idsUsuarios.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.NombreCompleto);

            foreach (var pago in pagos)
            {
                pago.NombreUsuarioCreador = nombres.GetValueOrDefault(pago.UsuarioCreadorId);
                pago.NombreUsuarioAnulador = pago.UsuarioAnuladorId.HasValue
                    ? nombres.GetValueOrDefault(pago.UsuarioAnuladorId.Value)
                    : null;
            }

            ViewBag.ReservaId = idReserva;
            return View(pagos);
        }

        // =====================================================
        // DETALLES DE UN PAGO (con auditoría)
        // =====================================================
        public async Task<IActionResult> Detalles(int id)
        {
            var pago = await _context.Pagos.FindAsync(id);

            if (pago == null)
            {
                return NotFound();
            }

            // Cargamos nombres de usuarios para la auditoría
            var idsUsuarios = new List<int> { pago.UsuarioCreadorId };
            if (pago.UsuarioAnuladorId.HasValue)
            {
                idsUsuarios.Add(pago.UsuarioAnuladorId.Value);
            }

            var nombres = await _context.Usuarios
                .Where(u => idsUsuarios.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.NombreCompleto);

            pago.NombreUsuarioCreador = nombres.GetValueOrDefault(pago.UsuarioCreadorId);
            pago.NombreUsuarioAnulador = pago.UsuarioAnuladorId.HasValue
                ? nombres.GetValueOrDefault(pago.UsuarioAnuladorId.Value)
                : null;

            return View(pago);
        }

        // =====================================================
        // CREAR PAGO
        // =====================================================
        public IActionResult Crear(int reservaId)
        {
            var pago = new Pago
            {
                ReservaId = reservaId,
                Fecha = DateTime.Today,
                Estado = EstadoPago.Pendiente
            };

            return View(pago);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Pago pago)
        {
            // Asignamos el usuario logueado como creador
            pago.UsuarioCreadorId = ObtenerIdUsuarioActual();

            if (!ModelState.IsValid)
            {
                return View(pago);
            }

            _context.Pagos.Add(pago);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { idReserva = pago.ReservaId });
        }

        // =====================================================
        // EDITAR SOLO EL CONCEPTO
        // =====================================================
        public async Task<IActionResult> EditarConcepto(int id)
        {
            var pago = await _context.Pagos.FindAsync(id);

            if (pago == null)
            {
                return NotFound();
            }

            // No se edita el concepto de un pago anulado
            if (pago.Anulado)
            {
                TempData["Error"] = "No se puede editar el concepto de un pago anulado.";
                return RedirectToAction(nameof(Index), new { idReserva = pago.ReservaId });
            }

            var vm = new EditarConceptoPagoViewModel
            {
                Id = pago.Id,
                ReservaId = pago.ReservaId,
                Concepto = pago.Concepto
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarConcepto(EditarConceptoPagoViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var pago = await _context.Pagos.FindAsync(vm.Id);
            if (pago == null)
            {
                return NotFound();
            }

            // No se edita el concepto de un pago anulado
            if (pago.Anulado)
            {
                TempData["Error"] = "No se puede editar el concepto de un pago anulado.";
                return RedirectToAction(nameof(Index), new { idReserva = pago.ReservaId });
            }

            // Solo se actualiza el concepto (importe y fecha quedan intactos)
            pago.Concepto = vm.Concepto;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { idReserva = vm.ReservaId });
        }

        // =====================================================
        // ANULAR PAGO (baja lógica)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Anular(int id, int reservaId)
        {
            var pago = await _context.Pagos.FindAsync(id);

            if (pago == null)
            {
                return NotFound();
            }

            // Si ya está anulado, no hacemos nada
            if (pago.Anulado)
            {
                TempData["Error"] = "El pago ya estaba anulado.";
                return RedirectToAction(nameof(Index), new { idReserva = reservaId });
            }

            // Baja lógica: no se borra el registro
            pago.Anulado = true;
            pago.Estado = EstadoPago.Anulado;
            pago.UsuarioAnuladorId = ObtenerIdUsuarioActual();
            await _context.SaveChangesAsync();

            TempData["Ok"] = "Pago anulado correctamente.";
            return RedirectToAction(nameof(Index), new { idReserva = reservaId });
        }

        // =====================================================
        // AUXILIAR
        // =====================================================

        // Devuelve el ID del usuario logueado
        private int ObtenerIdUsuarioActual()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out int id) ? id : 0;
        }
    }
}
