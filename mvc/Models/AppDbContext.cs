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
    public DbSet<Inquilino> Inquilinos { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Reserva> Reservas { get; set; }
    public DbSet<Pago> Pagos { get; set; }

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

      // INQUILINO

      modelBuilder.Entity<Inquilino>().ToTable("inquilino");

      modelBuilder.Entity<Inquilino>()
          .Property(i => i.Id)
          .HasColumnName("id");

      modelBuilder.Entity<Inquilino>()
          .Property(i => i.Nombre)
          .HasColumnName("nombre");

      modelBuilder.Entity<Inquilino>()
          .Property(i => i.Apellido)
          .HasColumnName("apellido");

      modelBuilder.Entity<Inquilino>()
          .Property(i => i.Dni)
          .HasColumnName("dni");

      modelBuilder.Entity<Inquilino>()
          .Property(i => i.Telefono)
          .HasColumnName("telefono");

      modelBuilder.Entity<Inquilino>()
          .Property(i => i.Email)
          .HasColumnName("email");

      // USUARIO

      modelBuilder.Entity<Usuario>().ToTable("usuario");

      modelBuilder.Entity<Usuario>()
          .Property(u => u.Id)
          .HasColumnName("id_usuario");

      modelBuilder.Entity<Usuario>()
          .Property(u => u.Nombre)
          .HasColumnName("nombre");

      modelBuilder.Entity<Usuario>()
          .Property(u => u.Apellido)
          .HasColumnName("apellido");

      modelBuilder.Entity<Usuario>()
          .Property(u => u.Email)
          .HasColumnName("email");

      modelBuilder.Entity<Usuario>()
          .Property(u => u.Clave)
          .HasColumnName("clave");

      modelBuilder.Entity<Usuario>()
          .Property(u => u.Avatar)
          .HasColumnName("avatar");

      modelBuilder.Entity<Usuario>()
          .Property(u => u.Rol)
          .HasColumnName("rol");


      // RESERVA

      modelBuilder.Entity<Reserva>().ToTable("reserva");

      modelBuilder.Entity<Reserva>()
          .Property(r => r.Id)
          .HasColumnName("id");

      modelBuilder.Entity<Reserva>()
          .Property(r => r.InquilinoId)
          .HasColumnName("inquilino_id");

      modelBuilder.Entity<Reserva>()
          .Property(r => r.InmuebleId)
          .HasColumnName("inmueble_id");
      modelBuilder.Entity<Reserva>()
          .Property(r => r.MontoDiario)
          .HasColumnName("monto_diario");

      modelBuilder.Entity<Reserva>()
          .Property(r => r.FechaDesde)
          .HasColumnName("fecha_desde")
          .HasColumnType("date");

      modelBuilder.Entity<Reserva>()
          .Property(r => r.FechaHasta)
          .HasColumnName("fecha_hasta")
          .HasColumnType("date");

      modelBuilder.Entity<Reserva>()
          .Property(r => r.FechaTerminacion)
          .HasColumnName("fecha_terminacion")
          .HasColumnType("date");

      modelBuilder.Entity<Reserva>()
          .Property(r => r.UsuarioCreadorId)
          .HasColumnName("usuario_creador_id");

      modelBuilder.Entity<Reserva>()
          .Property(r => r.UsuarioTerminadorId)
          .HasColumnName("usuario_terminador_id");

      modelBuilder.Entity<Reserva>()
          .HasOne<Usuario>()
          .WithMany()
          .HasForeignKey(r => r.UsuarioCreadorId);

      modelBuilder.Entity<Reserva>()
          .HasOne<Usuario>()
          .WithMany()
          .HasForeignKey(r => r.UsuarioTerminadorId);

      modelBuilder.Entity<Reserva>()
          .HasOne<Inquilino>()
          .WithMany()
          .HasForeignKey(r => r.InquilinoId);

      modelBuilder.Entity<Reserva>()
          .HasOne<Inmueble>()
          .WithMany()
          .HasForeignKey(r => r.InmuebleId);

      // PAGO

      modelBuilder.Entity<Pago>().ToTable("pago");

      modelBuilder.Entity<Pago>()
          .Property(p => p.Id)
          .HasColumnName("id");

      modelBuilder.Entity<Pago>()
          .Property(p => p.Fecha)
          .HasColumnName("fecha")
          .HasColumnType("date");

      modelBuilder.Entity<Pago>()
          .Property(p => p.Concepto)
          .HasColumnName("concepto");

      modelBuilder.Entity<Pago>()
          .Property(p => p.Importe)
          .HasColumnName("importe");

      modelBuilder.Entity<Pago>()
          .Property(p => p.ReservaId)
          .HasColumnName("reserva_id");

      modelBuilder.Entity<Pago>()
          .Property(p => p.Anulado)
          .HasColumnName("anulado");

      modelBuilder.Entity<Pago>()
          .Property(p => p.UsuarioCreadorId)
          .HasColumnName("usuario_creador_id");

      modelBuilder.Entity<Pago>()
          .Property(p => p.UsuarioAnuladorId)
          .HasColumnName("usuario_anulador_id");

      modelBuilder.Entity<Pago>()
          .HasOne<Reserva>()
          .WithMany()
          .HasForeignKey(p => p.ReservaId);


      // Relacion: un propietario puede tener varios inmuebles
      modelBuilder.Entity<Inmueble>()
          .HasOne(i => i.Propietario)
          .WithMany()
          .HasForeignKey(i => i.PropietarioId);
    }
  }
}