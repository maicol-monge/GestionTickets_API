using System.ComponentModel.DataAnnotations;

namespace GestionTickets.Models
{
    public class ticket
    {
        [Key]
        public int id_ticket { get; set; } // id_ticket
        public string titulo { get; set; } // titulo
        public string tipo_ticket { get; set; } // tipo_ticket
        public string descripcion { get; set; } // descripcion
        public string prioridad { get; set; } // prioridad
        public DateTime fecha_creacion { get; set; } // fecha_creacion
        public DateTime? fecha_cierre { get; set; } // fecha_cierre
        public int id_usuario { get; set; } // id_usuario
        public int id_categoria { get; set; } // id_categoria
        public string estado { get; set; } // estado

    }
}
