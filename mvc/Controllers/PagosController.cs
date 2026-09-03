using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using mvc.Models;

namespace mvc.Controllers
{
    [Authorize]
    public class PagosController : Controller
    {
        private readonly IRepositorioPago _repo;

        public PagosController(IRepositorioPago repo)
        {
            _repo = repo;
        }

        // =====================================================
        // LISTAR PAGOS POR RESERVA
        // =====================================================
        public async Task<IActionResult> Index(int idReserva)
        {
            var pagos = await _repo.ListarPorReserva(idReserva);
            ViewBag.ReservaId = idReserva;
            return View(pagos);
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

            await _repo.Crear(pago);

            return RedirectToAction(nameof(Index), new { idReserva = pago.ReservaId });
        }

        // =====================================================
        // EDITAR SOLO EL CONCEPTO
        // =====================================================
        public async Task<IActionResult> EditarConcepto(int id)
        {
            var pago = await _repo.ObtenerPorId(id);

            if (pago == null)
            {
                return NotFound();
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

            await _repo.EditarConcepto(vm.Id, vm.Concepto);

            return RedirectToAction(nameof(Index), new { idReserva = vm.ReservaId });
        }

        // =====================================================
        // ANULAR PAGO (baja lógica)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Anular(int id, int reservaId)
        {
            var pago = await _repo.ObtenerPorId(id);

            if (pago == null)
            {
                return NotFound();
            }

            await _repo.Anular(id, ObtenerIdUsuarioActual());

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
