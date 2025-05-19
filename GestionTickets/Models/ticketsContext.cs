using Microsoft.EntityFrameworkCore;

namespace GestionTickets.Models
{
    public class ticketsContext: DbContext
    {
        public ticketsContext(DbContextOptions<ticketsContext> options) : base(options)
        {

        }

        public DbSet<empresa> empresa { get; set; }

    }
}
