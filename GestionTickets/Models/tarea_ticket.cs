using System.ComponentModel.DataAnnotations;

namespace GestionTickets.Models
{
    public class tarea_ticket
    {
        [Key]
        public int id_tarea { get; set; }
        public int id_ticket { get; set; }
        public int id_usuario { get; set; }
        public DateTime fecha_tarea { get; set; }
        public string contenido { get; set; }
    }
}
