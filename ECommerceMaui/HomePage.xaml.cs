using ECommerceMaui.Models; //Necesitamos acceso a nuestra clase Product
using System.Collections.ObjectModel; // Necesario para ObservableCollection
using ECommerceMaui.Servicios;
using System.Net.Http;
using System.IO;
using Microsoft.Maui.Controls;

namespace ECommerceMaui;

public partial class HomePage : ContentPage
{
    // 1. Variable para nuestro servicio de base de datos
    private readonly DatabaseService _dbService;
    private static readonly HttpClient _httpClient = new HttpClient();
    // 2. La lista de productos (ya no la inicializamos aquí)
    public ObservableCollection<Product> Products { get; set; }

    public HomePage()
    {
        InitializeComponent();

        // 3. Creamos la instancia del servicio
        _dbService = new DatabaseService();


        Products = new ObservableCollection<Product>();


        ProductsCollectionView.ItemsSource = Products;
    }

    /// <summary>
    /// Este método se ejecuta CADA VEZ que la página aparece en pantalla.
    /// Es el lugar perfecto para cargar datos frescos.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadProductsFromDbAsync();
    }

    /// <summary>
    /// Método (reemplazo del 'LoadMockProducts')
    /// que llama al servicio de base de datos.
    /// </summary>
    private async Task LoadProductsFromDbAsync()
    {
        Products.Clear();

        var productsFromDb = await _dbService.GetProductsAsync();

        if (productsFromDb != null)
        {
            foreach (var product in productsFromDb)
            {
                // 1. Añadimos el producto (con su texto) a la lista.
                // La UI lo mostrará inmediatamente, pero sin imagen.
                Products.Add(product);

                // 2. Iniciamos la descarga de la imagen para este producto
                //    en segundo plano. No esperamos (no usamos 'await' aquí)
                //    para que la UI no se bloquee.
                _ = LoadProductImageAsync(product);
            }
        }
    }
/// <summary>
/// NUEVO MÉTODO: Carga la imagen para un producto específico
/// usando la lógica de HttpClient (la solución de Copilot).
/// </summary>
    private async Task LoadProductImageAsync(Product product)
    {
        // Forzamos PNG
        string imageUrl = product.ImageUrl.EndsWith(".png") ? product.ImageUrl : $"{product.ImageUrl}.png";

        try
        {
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("image/png"));

            var imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);

            // 3. Cuando se descarga, la asignamos a la propiedad 'ProductImageSource'
            product.ProductImageSource = ImageSource.FromStream(() => new MemoryStream(imageBytes));

            // 4. Gracias a INotifyPropertyChanged, la UI se
            //    actualizará automáticamente en este momento.
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error cargando imagen para {product.Name}: {ex.Message}");
            product.ProductImageSource = "dotnet_bot.png"; // Imagen de fallback
        }
    }


    /// <summary>
    /// Se ejecuta cuando el usuario selecciona un producto de la lista.
    /// (¡Ahora también registra la vista!)
    /// </summary>
    private async void OnProductSelected(object sender, SelectionChangedEventArgs e)
    {
        var product = e.CurrentSelection.FirstOrDefault() as Product;
        if (product == null)
            return;

        // --- ¡NUEVA LÓGICA DE HISTORIAL! ---
        var currentUser = Services.AuthService.Instance.CurrentUser;
        if (currentUser != null)
        {
            // Llamamos al método de la BBDD en segundo plano
            // No usamos "await" para no retrasar la navegación
            _ = _dbService.LogProductViewAsync(currentUser.UserId, product.Id);
        }
        // --- FIN DE LÓGICA DE HISTORIAL ---

        // Navegamos a la página de detalle (sin cambios)
        await Navigation.PushAsync(new ProductDetailPage(product));

        // Limpiamos la selección (sin cambios)
        ((CollectionView)sender).SelectedItem = null;
    }
}
