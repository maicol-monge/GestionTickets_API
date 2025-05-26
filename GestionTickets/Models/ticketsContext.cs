using Microsoft.EntityFrameworkCore;

namespace GestionTickets.Models
{
    public class ticketsContext: DbContext
    {
        public ticketsContext(DbContextOptions<ticketsContext> options) : base(options)
        {

        }

        public DbSet<empresa> empresa { get; set; }
        public DbSet<usuario> usuario { get; set; }
        public DbSet<ticket> ticket { get; set; }
        public DbSet<categoria> categoria { get; set; }
        public DbSet<archivo_adjunto> archivo_adjunto { get; set; }
        public DbSet<asignacion_ticket> asignacion_ticket { get; set; }
        public DbSet<tecnico> tecnico { get; set; }
        public DbSet<tarea_ticket> tarea_ticket { get; set; }
        public DbSet<comentario> comentario { get; set; }




    }
}
