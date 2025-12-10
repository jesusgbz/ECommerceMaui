using ECommerceMaui.Models;
using System.Net.Http; // Puede que este ya esté o no, pero añádelo por si acaso
using System.Diagnostics;
// --- ¡AÑADE ESTAS DOS LÍNEAS! ---
using ECommerceMaui.Servicios;
using System.Collections.ObjectModel;
using ECommerceMaui.Services; // Necesario para GetReviewsForProductAsync

namespace ECommerceMaui;

public partial class ProductDetailPage : ContentPage
{
    private Product _product; // Variable para guardar el producto que recibimos

    // --- Usaremos la misma lógica de HttpClient para cargar la imagen ---
    private static readonly HttpClient _httpClient = new();

    private readonly DatabaseService _dbService;
    private readonly AuthService _authService;

    /// <summary>
    /// Constructor que acepta un producto
    /// </summary>
    public ProductDetailPage(Product product)
    {
        InitializeComponent();

        _product = product; // Guardamos el producto
        _dbService = new DatabaseService();
        _authService = AuthService.Instance;

        LoadProductDetails(); // Cargamos los detalles en la UI
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadReviewsAsync();
    }

    private async Task LoadReviewsAsync()
    {
        // Obtiene las reseñas desde la BBDD
        var reviews = await _dbService.GetReviewsForProductAsync(_product.Id);
        // Asigna la lista al CollectionView
        ReviewsCollectionView.ItemsSource = reviews;
    }

    /// <summary>
    /// Rellena la UI con los datos del producto
    /// </summary>
    private async void LoadProductDetails()
    {
        // Asignamos los textos
        ProductNameLabel.Text = _product.Name;
        ProductPriceLabel.Text = $"${_product.Price:F2}";
        ProductDescriptionLabel.Text = _product.Description;

        // --- Lógica de carga de imagen (solución de Copilot) ---
        // Forzamos PNG
        string imageUrl = _product.ImageUrl.EndsWith(".png") ? _product.ImageUrl : $"{_product.ImageUrl}.png";

        try
        {
            // Preferimos PNG
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("image/png"));

            var imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);
            var stream = new MemoryStream(imageBytes);

            // Asignamos la imagen desde el stream
            ProductImage.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));
        }
        catch (Exception ex)
        {
            // Si falla, mostramos una alerta y/o una imagen local
            System.Diagnostics.Debug.WriteLine($"Error cargando imagen: {ex.Message}");
            ProductImage.Source = "dotnet_bot.png"; // Imagen de fallback
        }
    }

    /// <summary>
    /// Se ejecuta al presionar "Agregar al Carrito".
    /// (Por ahora, solo simulación)
    /// </summary>
    private async void OnAddToCartClicked(object sender, EventArgs e)
    {
        // 1. Agregar al Singleton del Carrito
        Servicios.ShoppingCartService.Instance.AddItem(_product);

        // 2. Mostrar menú de opciones (Action Sheet)
        // Parámetros: Título, Botón Cancelar, Botón Destructivo (null), Opciones...
        string action = await DisplayActionSheet(
            "✅ Producto Agregado",
            "Seguir Comprando", 
            null, 
            "Ir al Carrito", 
            "Pagar Ahora");

        // 3. Manejar la decisión del usuario
        switch (action)
        {
            case "Ir al Carrito":
                // Cambiamos la pestaña activa del Shell para ir al Carrito
                // Usamos la ruta absoluta "//ShoppingCartPage"
                await Shell.Current.GoToAsync($"//{nameof(ShoppingCartPage)}");
                
                // Opcional: "Limpiamos" la navegación de la pestaña de Inicio para que al volver no siga en el detalle
                await Navigation.PopToRootAsync(); 
                break;

            case "Pagar Ahora":
                // Navegamos directamente a la pantalla de pago (sin pasar por la vista del carrito)
                await Navigation.PushAsync(new PaymentPage());
                break;

            case "Seguir Comprando":
                // No hacemos nada, el usuario se queda en el detalle del producto
                break;
        }
    }

    private async void OnSubmitReviewClicked(object sender, EventArgs e)
    {
        var currentUser = _authService.CurrentUser;
        if (currentUser == null)
        {
            await DisplayAlert("Error", "Debes iniciar sesión para dejar una reseña.", "OK");
            return;
        }

        // 1. Obtener datos del formulario
        var rating = (int)RatingSlider.Value;
        var comment = CommentEditor.Text;

        if (string.IsNullOrWhiteSpace(comment))
        {
            await DisplayAlert("Error", "Por favor, escribe un comentario.", "OK");
            return;
        }

        // 2. Enviar a la BBDD
        bool isSuccess = await _dbService.SubmitReviewAsync(_product.Id, currentUser.UserId, rating, comment);

        if (isSuccess)
        {
            await DisplayAlert("¡Gracias!", "Tu reseña ha sido publicada.", "OK");

            // 3. Limpiar formulario
            RatingSlider.Value = 5;
            CommentEditor.Text = string.Empty;

            // 4. Recargar la lista de reseñas
            await LoadReviewsAsync();
        }
        else
        {
            await DisplayAlert("Error", "No se pudo publicar tu reseña. Intenta más tarde.", "OK");
        }
    }
}