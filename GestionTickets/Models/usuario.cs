using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GestionTickets.Models
{
    public class usuario
    {
        [Key]
        public int id_usuario { get; set; } // id_usuario
        public string nombre { get; set; } // nombre
        public string apellido { get; set; } // apellido
        public string correo { get; set; } // correo
        public string contrasena { get; set; } // contrasena
        public string telefono { get; set; } // telefono
        public string? tipo_usuario { get; set; } // tipo_usuario
        public string? rol { get; set; } // rol
        public int id_empresa { get; set; } // id_empresa

        [NotMapped]
        public int? id_categoria { get; set; }

    }
}
