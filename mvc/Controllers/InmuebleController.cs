using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using mvc.Models;

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

        public async Task<IActionResult> Index()
        {
            var inmuebles = await _context.Inmuebles
                .Include(i => i.Propietario)
                .Include(i => i.TipoInmueble)
                .Include(i => i.Imagenes)
                .ToListAsync();

            return View(inmuebles);
        }

        // CREAR INMUEBLE

        public async Task<IActionResult> Create()
        {
            await CargarPropietarios();
            await CargarTipos();

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

                    // Si la carpeta no existe, la creamos
                    if (!Directory.Exists(carpeta))
                    {
                        Directory.CreateDirectory(carpeta);
                    }

                    bool esPrimeraImagen = true;

                    foreach (var imagen in imagenes)
                    {
                        // Ignorar archivos vacios
                        if (imagen.Length == 0)
                        {
                            continue;
                        }

                        // Generar nombre unico
                        string extension = Path.GetExtension(imagen.FileName);
                        string nombreArchivo =
                            Guid.NewGuid().ToString() + extension;

                        string rutaFisica = Path.Combine(
                            carpeta,
                            nombreArchivo
                        );

                        // Guardar archivo fisicamente
                        using (var stream = new FileStream(
                            rutaFisica,
                            FileMode.Create))
                        {
                            await imagen.CopyToAsync(stream);
                        }

                        // Guardar informacion en la base de datos
                        var nuevaImagen = new Imagen
                        {
                            Url = "/uploads/inmuebles/" + nombreArchivo,
                            Descripcion = imagen.FileName,
                            EsPrincipal = esPrimeraImagen,
                            InmuebleId = inmueble.Id
                        };

                        _context.Imagenes.Add(nuevaImagen);

                        // Solo la primera es principal
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

                TempData["Ok"] = "Inmueble eliminado correctamente";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "No se puede eliminar el inmueble porque tiene reservas o imagenes asociadas";
            }

            return RedirectToAction(nameof(Index));
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

        private async Task CargarTipos()
        {
            var tipos = await _context.TiposInmueble
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