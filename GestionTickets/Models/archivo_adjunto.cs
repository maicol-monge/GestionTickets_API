using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GestionTickets.Models
{
    public class archivo_adjunto
    {
        [Key]
        public int id_archivo { get; set; }
        public string nombre_archivo { get; set; }
        public string ruta_archivo { get; set; }
        public int id_ticket { get; set; }

        [ForeignKey("id_ticket")]
        public ticket Ticket { get; set; }
    }
}
