using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerceMaui.Models
{
    /// <summary>
    /// Representa al usuario que ha iniciado sesión en la app.
    /// (No contiene información sensible como el hash de la contraseña).
    /// </summary>
    public class User
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
    }
}
