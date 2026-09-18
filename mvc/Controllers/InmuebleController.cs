using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using mvc.Models;
using mvc.Models.ViewModels;

namespace mvc.Controllers
{
    
    public class InmuebleController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public InmuebleController(
            AppDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // LISTAR INMUEBLES

        public async Task<IActionResult> Index(
            string? buscar,
            int? propietarioId,
            bool? disponible,
            int pagina = 1)
        {
            // Cantidad de registros por pagina
            int registrosPorPagina = 5;

            // Consulta base
            var consulta = _context.Inmuebles
                .Include(i => i.Propietario)
                .Include(i => i.TipoInmueble)
                .Include(i => i.Imagenes)
                .AsQueryable();

            // BUSQUEDA

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                buscar = buscar.Trim();

                consulta = consulta.Where(i =>
                    i.Direccion.Contains(buscar) ||
                    i.Propietario!.Nombre.Contains(buscar) ||
                    i.Propietario.Apellido.Contains(buscar));
            }

            // FILTRO POR PROPIETARIO

            if (propietarioId.HasValue)
            {
                consulta = consulta.Where(i =>
                    i.PropietarioId == propietarioId.Value);
            }

            // FILTRO POR DISPONIBILIDAD

            if (disponible.HasValue)
            {
                consulta = consulta.Where(i =>
                    i.Disponible == disponible.Value);
            }

            // TOTAL DE REGISTROS

            int totalRegistros = await consulta.CountAsync();

            // Cantidad total de paginas
            int totalPaginas = (int)Math.Ceiling(
                totalRegistros / (double)registrosPorPagina
            );

            // Evitamos paginas invalidas
            if (pagina < 1)
            {
                pagina = 1;
            }

            if (totalPaginas > 0 && pagina > totalPaginas)
            {
                pagina = totalPaginas;
            }

            // PAGINADO

            var inmuebles = await consulta
                .OrderBy(i => i.Id)
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToListAsync();

            // DATOS PARA LA VISTA

            await CargarPropietarios();

            ViewBag.Buscar = buscar;
            ViewBag.PropietarioId = propietarioId;
            ViewBag.Disponible = disponible;

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;

            return View(inmuebles);
        }

        // CREAR INMUEBLE

        public async Task<IActionResult> Create(string? buscarTipo)
        {
            await CargarPropietarios();
            await CargarTipos();

            ViewBag.BuscarTipo = buscarTipo;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Inmueble inmueble,
            List<IFormFile>? imagenes)
        {
            if (ModelState.IsValid)
            {
                // Guardamos primero el inmueble para obtener su Id
                _context.Inmuebles.Add(inmueble);

                await _context.SaveChangesAsync();

                // GUARDAR IMAGENES

                if (imagenes != null && imagenes.Count > 0)
                {
                    string carpeta = Path.Combine(
                        _environment.WebRootPath,
                        "uploads",
                        "inmuebles"
                    );

                    if (!Directory.Exists(carpeta))
                    {
                        Directory.CreateDirectory(carpeta);
                    }

                    bool esPrimeraImagen = true;

                    foreach (var imagen in imagenes)
                    {
                        if (imagen.Length == 0)
                        {
                            continue;
                        }

                        string extension = Path.GetExtension(imagen.FileName);

                        string nombreArchivo =
                            Guid.NewGuid().ToString() + extension;

                        string rutaFisica = Path.Combine(
                            carpeta,
                            nombreArchivo
                        );

                        using (var stream = new FileStream(
                            rutaFisica,
                            FileMode.Create))
                        {
                            await imagen.CopyToAsync(stream);
                        }

                        var nuevaImagen = new Imagen
                        {
                            Url = "/uploads/inmuebles/" + nombreArchivo,
                            Descripcion = imagen.FileName,
                            EsPrincipal = esPrimeraImagen,
                            InmuebleId = inmueble.Id
                        };

                        _context.Imagenes.Add(nuevaImagen);

                        esPrimeraImagen = false;
                    }

                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            await CargarPropietarios();
            await CargarTipos();

            return View(inmueble);
        }

        // EDITAR INMUEBLE

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inmueble = await _context.Inmuebles.FindAsync(id);

            if (inmueble == null)
            {
                return NotFound();
            }

            await CargarPropietarios();
            await CargarTipos();

            return View(inmueble);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Inmueble inmueble)
        {
            if (id != inmueble.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var actual = await _context.Inmuebles.FindAsync(id);

                if (actual == null)
                {
                    return NotFound();
                }

                actual.Direccion = inmueble.Direccion;
                actual.Cupo = inmueble.Cupo;
                actual.Latitud = inmueble.Latitud;
                actual.Longitud = inmueble.Longitud;
                actual.PrecioPorDia = inmueble.PrecioPorDia;
                actual.PorcentajeReserva = inmueble.PorcentajeReserva;
                actual.Disponible = inmueble.Disponible;
                actual.PropietarioId = inmueble.PropietarioId;
                actual.TipoId = inmueble.TipoId;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            await CargarPropietarios();
            await CargarTipos();

            return View(inmueble);
        }


        // ELIMINAR INMUEBLE

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var inmueble = await _context.Inmuebles.FindAsync(id);

            if (inmueble == null)
            {
                return NotFound();
            }

            try
            {
                _context.Inmuebles.Remove(inmueble);

                await _context.SaveChangesAsync();

                TempData["Ok"] =
                    "Inmueble eliminado correctamente";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "No se puede eliminar el inmueble porque tiene reservas o imagenes asociadas";
            }

            return RedirectToAction(nameof(Index));
        }

        // INFORME: INMUEBLES MÁS RESERVADOS EN LOS ÚLTIMOS 365 DÍAS

        public async Task<IActionResult> MasReservados(int pagina = 1)
        {
            if (pagina < 1)
            {
                pagina = 1;
            }

            int registrosPorPagina = 5;

            var fechaLimite = DateTime.UtcNow.AddDays(-365);

            var consulta = _context.Inmuebles
                .Include(i => i.Propietario)
                .Select(i => new InmuebleReservaViewModel
                {
                    Id = i.Id,

                    Direccion = i.Direccion,

                    Propietario = i.Propietario != null
                        ? i.Propietario.Nombre + " " + i.Propietario.Apellido
                        : "",

                    CantidadReservas = _context.Reservas.Count(r =>
                        r.InmuebleId == i.Id &&
                        r.FechaCreacion >= fechaLimite &&
                        r.Estado != EstadoReserva.Cancelada)
                })
                .OrderByDescending(i => i.CantidadReservas)
                .ThenBy(i => i.Direccion);

            // Cantidad total de registros
            int totalRegistros = await consulta.CountAsync();

            // Cantidad total de paginas
            int totalPaginas = (int)Math.Ceiling(
                totalRegistros / (double)registrosPorPagina
            );

            // Evitamos paginas invalidas
            if (totalPaginas > 0 && pagina > totalPaginas)
            {
                pagina = totalPaginas;
            }

            // Traemos solamente los registros de la pagina actual
            var datos = await consulta
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToListAsync();

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;

            return View(datos);
        }

        // INFORME: INMUEBLES SIN RESERVAS EN LOS ULTIMOS X DIAS

        public async Task<IActionResult> SinReservas(
            int dias = 30,
            int pagina = 1)
        {
            if (dias < 1)
            {
                dias = 30;
            }

            if (pagina < 1)
            {
                pagina = 1;
            }

            int registrosPorPagina = 5;

            var fechaLimite = DateTime.UtcNow.AddDays(-dias);

            var consulta = _context.Inmuebles
                .Include(i => i.Propietario)
                .Where(i =>
                    !_context.Reservas.Any(r =>
                        r.InmuebleId == i.Id &&
                        r.FechaCreacion >= fechaLimite &&
                        r.Estado != EstadoReserva.Cancelada))
                .OrderBy(i => i.Direccion);

            // Cantidad total de registros
            int totalRegistros = await consulta.CountAsync();

            // Cantidad total de páginas
            int totalPaginas = (int)Math.Ceiling(
                totalRegistros / (double)registrosPorPagina
            );

            if (totalPaginas > 0 && pagina > totalPaginas)
            {
                pagina = totalPaginas;
            }

            // Registros de la página actual
            var inmuebles = await consulta
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToListAsync();

            ViewBag.Dias = dias;
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;

            return View(inmuebles);
        }


        // CARGAR PROPIETARIOS

        private async Task CargarPropietarios()
        {
            var propietarios = await _context.Propietarios
                .OrderBy(p => p.Apellido)
                .ThenBy(p => p.Nombre)
                .ToListAsync();

            ViewBag.Propietarios = new SelectList(
                propietarios,
                "Id",
                "Apellido"
            );
        }

        // CARGAR TIPOS

        private async Task CargarTipos(string? buscarTipo = null)
        {
            var consulta = _context.TiposInmueble.AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscarTipo))
            {
                buscarTipo = buscarTipo.Trim();

                consulta = consulta.Where(t =>
                    t.Nombre.Contains(buscarTipo));
            }

            var tipos = await consulta
                .OrderBy(t => t.Nombre)
                .ToListAsync();

            ViewBag.Tipos = new SelectList(
                tipos,
                "Id",
                "Nombre"
            );
        }
    }
}