using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerceMaui.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }

        // El ID que guardamos en la base de datos
        public int AvatarId { get; set; }

        // Propiedad calculada: Convierte el ID en una URL de imagen automáticamente
        // Propiedad calculada: Avatares Gamer (Estilo Plano/Color)
        public string AvatarUrl => AvatarId switch
        {
            1 => "https://cdn-icons-png.flaticon.com/512/4526/4526315.png",

            2 => "https://cdn-icons-png.flaticon.com/512/4526/4526323.png",

            3 => "https://cdn-icons-png.flaticon.com/512/4526/4526442.png",

            4 => "https://cdn-icons-png.flaticon.com/512/4526/4526446.png",

            // Default
            _ => "https://cdn-icons-png.flaticon.com/512/4526/4526315.png"
        };
    }
}
