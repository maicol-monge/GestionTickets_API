using System.ComponentModel.DataAnnotations;

namespace GestionTickets.Models
{
    public class empresa
    {
        [Key]
        public int id_empresa { get; set; } // id_empresa
        public string nombre_empresa { get; set; } // nombre_empresa
        public string direccion { get; set; } // direccion
        public string nombre_contacto_principal { get; set; } // nombre_contacto_principal
        public string correo { get; set; } // correo
        public string telefono { get; set; } // telefono
    }
}
