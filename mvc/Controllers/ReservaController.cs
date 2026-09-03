using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mvc.Models;

namespace mvc.Controllers
{
    [Authorize]
    public class ReservaController : Controller
    {
        private readonly AppDbContext _context;

        public ReservaController(AppDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // DETALLES DE UNA RESERVA (con auditoría)
        // =====================================================
        public async Task<IActionResult> Detalles(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);

            if (reserva == null)
            {
                return NotFound();
            }

            // Cargamos nombres de usuarios para la auditoría
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
    }
}
