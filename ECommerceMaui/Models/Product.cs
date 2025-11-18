using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ECommerceMaui.Models
{
    // Implementamos la interfaz para notificar cambios
    public class Product : INotifyPropertyChanged
    {
        // --- Propiedades de la Base de Datos ---
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public string ImageUrl { get; set; }
        public int Stock { get; set; }

        // --- Propiedad para la UI ---

        private ImageSource _productImageSource;
        public ImageSource ProductImageSource
        {
            get { return _productImageSource; }
            set
            {
                _productImageSource = value;
                OnPropertyChanged(); // ¡Avisa a la UI que esta propiedad cambió!
            }
        }

        // --- Lógica de INotifyPropertyChanged ---
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}