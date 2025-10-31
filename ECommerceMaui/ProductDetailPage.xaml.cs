using ECommerceMaui.Models; // ¡Importante! Necesitamos el modelo

namespace ECommerceMaui;

public partial class ProductDetailPage : ContentPage
{
    private Product _product; // Variable para guardar el producto que recibimos

    // --- Usaremos la misma lógica de HttpClient para cargar la imagen ---
    private static readonly HttpClient _httpClient = new HttpClient();

    /// <summary>
    /// Constructor que acepta un producto
    /// </summary>
    public ProductDetailPage(Product product)
    {
        InitializeComponent();

        _product = product; // Guardamos el producto
        LoadProductDetails(); // Cargamos los detalles en la UI
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
        // Lógica futura: Agregar _product al carrito de compras

        await DisplayAlert("Carrito", $"{_product.Name} ha sido agregado al carrito (Simulación).", "OK");
    }
}