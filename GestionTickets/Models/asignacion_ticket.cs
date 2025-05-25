using System.ComponentModel.DataAnnotations;

namespace GestionTickets.Models
{
    public class asignacion_ticket
    {
        [Key]
        public int id_asignacion { get; set; }
        public int id_ticket { get; set; }
        public int id_tecnico { get; set; }
        public DateTime fecha_asignacion { get; set; }
        public char estado_ticket { get; set; } // 'P'=Pendiente, 'E'=En proceso, 'R'=Resuelto
    }
}
