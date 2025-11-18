using ECommerceMaui.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerceMaui.Services
{
    public class AuthService
    {
        // --- El Patrón Singleton ---
        public static AuthService Instance { get; } = new AuthService();
        private AuthService() { }
        // -------------------------

        /// <summary>
        /// Almacena la información del usuario que ha iniciado sesión.
        /// Será 'null' si nadie ha iniciado sesión.
        /// </summary>
        public User CurrentUser { get; private set; }

        /// <summary>
        /// Método para "iniciar sesión" en la app
        /// </summary>
        public void Login(User user)
        {
            CurrentUser = user;
        }

        /// <summary>
        /// Método para "cerrar sesión" en la app
        /// </summary>
        public void Logout()
        {
            CurrentUser = null;
        }
    }
}
