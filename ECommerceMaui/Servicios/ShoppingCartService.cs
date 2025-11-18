using ECommerceMaui.Models;
using System.Collections.ObjectModel; // ¡Muy importante!

namespace ECommerceMaui.Servicios
{
    public class ShoppingCartService
    {
        // -------------------------------------------------------------------
        // --- El Patrón Singleton ---
        // -------------------------------------------------------------------

        // 1. Una 'propiedad' estática (global) para guardar la única instancia
        public static ShoppingCartService Instance { get; } = new ShoppingCartService();

        // 2. Un 'constructor' privado para que nadie más pueda crear instancias
        private ShoppingCartService()
        {
            // El constructor está vacío, pero es privado.
        }

        /// <summary>
        /// --- Propiedades y Métodos del Carrito ---
        /// </summary>

        /// <summary>
        /// La lista de productos en el carrito.
        /// Usamos ObservableCollection para que la UI se actualice automáticamente
        /// cuando agregamos o quitamos elementos.
        /// </summary>
        public ObservableCollection<Product> CartItems { get; private set; } = new ObservableCollection<Product>();

        /// <summary>
        /// Método público para agregar un producto al carrito.
        /// </summary>
        public void AddItem(Product product)
        {
            // (Aquí podrías agregar lógica para ver si ya existe y sumar cantidad)
            CartItems.Add(product);
        }


        public void RemoveItem(Product product) {
            CartItems.Remove(product);
        }
        public void ClearCartItems() {
            CartItems.Clear();
        }
        public void ClearCart() {
            CartItems.Clear();
        }

        // (Más adelante podemos agregar métodos como RemoveItem, ClearCart, GetTotal, etc.)
    }
}