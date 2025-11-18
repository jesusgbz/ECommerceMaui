using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerceMaui.Models
{
    /// <summary>
    /// Representa una reseña en la UI.
    /// Incluye el nombre del usuario, no solo su ID.
    /// </summary>
    public class Review
    {
        public int ReviewId { get; set; }
        public int ProductId { get; set; }
        public string AuthorName { get; set; } // ¡Importante!
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}