using System.ComponentModel.DataAnnotations;

namespace GestionTickets.Models
{
    public class tecnico
    {
        [Key]
        public int id_tecnico { get; set; }
        public int id_usuario { get; set; }
        public int id_usuarioid_categoria { get; set; }

    }
}
