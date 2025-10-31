using ECommerceMaui.Models; // ¡Importante! Necesitamos acceso a nuestra clase Product
using System.Collections.ObjectModel; // Necesario para ObservableCollection
using System.Net.Http;
using System.IO;
using Microsoft.Maui.Controls;

namespace ECommerceMaui;

public partial class HomePage : ContentPage
{
    // Una lista "observable" notifica a la UI (el XAML) si se agregan o quitan elementos.
    public ObservableCollection<Product> Products { get; set; }

    public HomePage()
    {
        InitializeComponent();

        Products = new ObservableCollection<Product>();
        ProductsCollectionView.ItemsSource = Products;

        LoadMockProducts(); // inicia la carga y descarga de imágenes
    }

    /// <summary>
    /// Crea una lista de productos ficticios para mostrar en la UI y descarga las imágenes.
    /// </summary>
    private async void LoadMockProducts()
    {
        // --- INICIO PRODUCTOS FICTICIOS ---
        var mock = new[]
        {
            new Product
            {
                Id = 1,
                Name = "Laptop Ejecutivo",
                Description = "Laptop potente para negocios.",
                Price = 1499.99,
                ImageUrl = "https://placehold.co/170x120.png?text=Laptop", // force PNG
                Stock = 10
            },
            new Product
            {
                Id = 2,
                Name = "Smartphone Premium",
                Description = "El último smartphone del mercado.",
                Price = 999.50,
                ImageUrl = "https://placehold.co/170x120.png?text=Smartphone", // force PNG
                Stock = 25
            },
            new Product
            {
                Id = 3,
                Name = "Audífonos Pro",
                Description = "Cancelación de ruido activa.",
                Price = 249.00,
                ImageUrl = "https://placehold.co/170x120.png?text=Audifonos", // force PNG
                Stock = 50
            },
            new Product
            {
                Id = 4,
                Name = "Smartwatch Elite",
                Description = "Monitoreo de salud avanzado.",
                Price = 399.00,
                ImageUrl = "https://placehold.co/170x120.png?text=Smartwatch", // force PNG
                Stock = 15
            }
        };
        // --- FIN PRODUCTOS FICTICIOS ---

        // Add items to the existing ObservableCollection so bindings remain valid
        foreach (var p in mock)
            Products.Add(p);

        // Download images
        using var httpClient = new HttpClient();
        // Prefer PNG to avoid servers sending WebP (which WinUI may not decode)
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.ParseAdd("image/png");
        httpClient.DefaultRequestHeaders.Accept.ParseAdd("image/*;q=0.8");

        foreach (var product in Products)
        {
            if (string.IsNullOrWhiteSpace(product.ImageUrl))
                continue;

            try
            {
                var bytes = await httpClient.GetByteArrayAsync(product.ImageUrl);
                // ImageSource.FromStream must return a new stream each time it's requested.
                product.ImageSource = ImageSource.FromStream(() => new MemoryStream(bytes));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error downloading image '{product.ImageUrl}': {ex.Message}");
                // Optional: set a local fallback image if you include one in the project
                // product.ImageSource = ImageSource.FromFile("Assets/Images/fallback.png");
            }
        }
    }

    /// <summary>
    /// Se ejecuta cuando el usuario selecciona un producto de la lista.
    /// </summary>
    
    private async void OnProductSelected(object sender, SelectionChangedEventArgs e)
    {
        // 1. Obtenemos el producto que fue seleccionado
        // e.CurrentSelection es una lista, tomamos el primer (y único) ítem
        var product = e.CurrentSelection.FirstOrDefault() as Product;

        // 2. Verificamos que no sea nulo (por seguridad)
        if (product == null)
            return;

        // 3. ¡Navegamos! "Empujamos" la nueva página de detalle
        //    Pasamos el 'product' al constructor que creamos en el paso anterior.
        await Navigation.PushAsync(new ProductDetailPage(product));

        // 4. Limpiamos la selección
        //    (Para que el ítem no se quede "marcado" y se pueda volver a presionar)
        ((CollectionView)sender).SelectedItem = null;
    }
}
