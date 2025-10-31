using System;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using System.Runtime.CompilerServices;

namespace ECommerceMaui.Models
{
    public class Product : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int Stock { get; set; }

        // New: ImageSource property to hold the downloaded image
        private ImageSource? _imageSource;
        public ImageSource? ImageSource
        {
            get => _imageSource;
            set
            {
                if (!Equals(_imageSource, value))
                {
                    _imageSource = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}