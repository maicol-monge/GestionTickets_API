using System.ComponentModel.DataAnnotations;

namespace GestionTickets.Models
{
    public class comentario
    {
        [Key]
        public int id_comentario { get; set; }
        public int id_ticket { get; set; }
        public int id_usuario { get; set; }
        public DateTime fecha_comentario { get; set; }
        public string contenido { get; set; }
    }
}
