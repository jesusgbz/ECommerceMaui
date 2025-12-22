using ECommerceMaui.Models; //Necesitamos acceso a nuestra clase Product
using System.Collections.ObjectModel; // Necesario para ObservableCollection
using ECommerceMaui.Servicios;
using System.Net.Http;
using System.IO;
using Microsoft.Maui.Controls;

namespace ECommerceMaui;

public partial class HomePage : ContentPage
{
    //Variable para nuestro servicio de base de datos
    private readonly DatabaseService _dbService;
    private static readonly HttpClient _httpClient = new HttpClient();

    // 1. LISTA MAESTRA (Todos los productos descargados)
    private List<Product> _allProducts = new List<Product>();

    // 2. LISTA VISIBLE (Lo que ve el usuario, filtrada)
    public ObservableCollection<Product> DisplayProducts { get; set; } = new ObservableCollection<Product>();

    public HomePage()
    {
        InitializeComponent();

        // 3. Creamos la instancia del servicio
        _dbService = new DatabaseService();


        // Enlazamos la UI a la lista visible
        ProductsCollectionView.ItemsSource = DisplayProducts;
    }

    /// <summary>
    /// Este método se ejecuta CADA VEZ que la página aparece en pantalla.
    /// Es el lugar perfecto para cargar datos frescos.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDataAsync();
    }

    /// <summary>
    /// Método (reemplazo del 'LoadMockProducts')
    /// que llama al servicio de base de datos.
    /// </summary>
    private async Task LoadDataAsync()
    {
        // A. Cargar Categorías
        var categories = await _dbService.GetCategoriesAsync();
        // Agregamos una opción "Todas" al principio
        categories.Insert(0, new Category { Id = 0, Name = "Todas" });
        CategoriesCollectionView.ItemsSource = categories;

        // B. Cargar Productos
        DisplayProducts.Clear();
        var productsFromDb = await _dbService.GetProductsAsync();

        if (productsFromDb != null)
        {
            // Guardamos en la Lista Maestra
            _allProducts = productsFromDb.ToList();

            // Y mostramos todo inicialmente
            foreach (var p in _allProducts)
            {
                DisplayProducts.Add(p);
                _ = LoadProductImageAsync(p); // Carga imágenes en segundo plano
            }
        }
    }

    // --- LÓGICA DE FILTRADO ---

    private string _searchText = "";
    private int _selectedCategoryId = 0; // 0 = Todas

    /// <summary>
    /// Se ejecuta cada vez que escribes una letra
    /// </summary>
    private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
    {
        _searchText = e.NewTextValue;
        ApplyFilters();
    }

    /// <summary>
    /// Se ejecuta al tocar una categoría
    /// </summary>
    private void OnCategoryTapped(object sender, SelectionChangedEventArgs e)
    {
        var category = e.CurrentSelection.FirstOrDefault() as Category;
        if (category == null) return;

        _selectedCategoryId = category.Id;
        ApplyFilters();
    }

    /// <summary>
    /// Aplica ambos filtros (Texto Y Categoría) al mismo tiempo
    /// </summary>
    private void ApplyFilters()
    {
        // 1. Empezamos con la lista maestra completa
        var filtered = _allProducts.AsEnumerable();

        // 2. Aplicamos filtro de categoría (si no es "Todas")
        if (_selectedCategoryId != 0)
        {
            filtered = filtered.Where(p => p.CategoryId == _selectedCategoryId); // Necesitas añadir CategoryId al modelo Product
        }

        // 3. Aplicamos filtro de texto (si escribieron algo)
        if (!string.IsNullOrWhiteSpace(_searchText))
        {
            filtered = filtered.Where(p => p.Name.ToLower().Contains(_searchText.ToLower()));
        }

        // 4. Actualizamos la lista visible
        DisplayProducts.Clear();
        foreach (var p in filtered)
        {
            DisplayProducts.Add(p);
        }
    }

    /// <summary>
    ///  Carga la imagen para un producto específico
    /// usando la lógica de HttpClient.
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
    /// Método infalible para detectar clics en Android
    /// </summary>
    private async void OnProductTapped(object sender, TappedEventArgs e)
    {
        // 1. Obtenemos el producto desde el CommandParameter
        var product = e.Parameter as Product;
        if (product == null) return;

        // 2. Lógica de Historial (Igual que antes)
        var currentUser = Services.AuthService.Instance.CurrentUser;
        if (currentUser != null)
        {
            _ = _dbService.LogProductViewAsync(currentUser.UserId, product.Id);
        }

        // 3. Navegar
        await Navigation.PushAsync(new ProductDetailPage(product));
    }
}