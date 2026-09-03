using Microsoft.EntityFrameworkCore;

namespace mvc.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Propietario> Propietarios { get; set; }

        public DbSet<Inmueble> Inmuebles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Tabla Propietario
            modelBuilder.Entity<Propietario>()
                .ToTable("propietario");

            modelBuilder.Entity<Propietario>()
                .Property(p => p.Id)
                .HasColumnName("id");

            modelBuilder.Entity<Propietario>()
                .Property(p => p.Nombre)
                .HasColumnName("nombre");

            modelBuilder.Entity<Propietario>()
                .Property(p => p.Apellido)
                .HasColumnName("apellido");

            modelBuilder.Entity<Propietario>()
                .Property(p => p.Dni)
                .HasColumnName("dni");

            modelBuilder.Entity<Propietario>()
                .Property(p => p.Telefono)
                .HasColumnName("telefono");

            modelBuilder.Entity<Propietario>()
                .Property(p => p.Email)
                .HasColumnName("email");


            // INMUEBLE

            modelBuilder.Entity<Inmueble>().ToTable("inmueble");

            modelBuilder.Entity<Inmueble>()
                .Property(i => i.Id)
                .HasColumnName("id");

            modelBuilder.Entity<Inmueble>()
                .Property(i => i.Direccion)
                .HasColumnName("direccion");

            modelBuilder.Entity<Inmueble>()
                .Property(i => i.Cupo)
                .HasColumnName("cupo");

            modelBuilder.Entity<Inmueble>()
                .Property(i => i.Tipo)
                .HasColumnName("tipo");

            modelBuilder.Entity<Inmueble>()
                .Property(i => i.Latitud)
                .HasColumnName("latitud");

            modelBuilder.Entity<Inmueble>()
                .Property(i => i.Longitud)
                .HasColumnName("longitud");

            modelBuilder.Entity<Inmueble>()
                .Property(i => i.PrecioPorDia)
                .HasColumnName("precio_por_dia");

            modelBuilder.Entity<Inmueble>()
                .Property(i => i.PorcentajeReserva)
                .HasColumnName("porcentaje_reserva");

            modelBuilder.Entity<Inmueble>()
                .Property(i => i.Disponible)
                .HasColumnName("disponible");

            modelBuilder.Entity<Inmueble>()
                .Property(i => i.PropietarioId)
                .HasColumnName("propietario_id");

            // Relacion: un propietario puede tener varios inmuebles
            modelBuilder.Entity<Inmueble>()
                .HasOne(i => i.Propietario)
                .WithMany()
                .HasForeignKey(i => i.PropietarioId);
        }
    }
}
