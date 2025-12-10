using ECommerceMaui.Servicios; // Acceso al servicio
using System.Linq;

namespace ECommerceMaui;

public partial class PaymentPage : ContentPage
{
    private readonly DatabaseService _dbService;

    public PaymentPage()
    {
        InitializeComponent();
        _dbService = new DatabaseService(); // Inicializamos el servicio
    }

    private async void OnPayClicked(object sender, EventArgs e)
    {
        // 1. Validar Sesión (IGUAL QUE ANTES)
        var currentUser = Services.AuthService.Instance.CurrentUser;
        if (currentUser == null)
        {
            await DisplayAlert("Error", "Sesión no válida.", "OK");
            return;
        }

        // 2. Validar Campos de Dirección (IGUAL QUE ANTES)
        if (string.IsNullOrWhiteSpace(StreetEntry.Text) ||
            string.IsNullOrWhiteSpace(ColoniaEntry.Text) ||
            string.IsNullOrWhiteSpace(ZipEntry.Text) ||
            string.IsNullOrWhiteSpace(CityEntry.Text) ||
            string.IsNullOrWhiteSpace(StateEntry.Text))
        {
            await DisplayAlert("Faltan Datos", "Por favor, completa la dirección de envío.", "OK");
            return;
        }

        // 3. Validar Carrito (IGUAL QUE ANTES)
        var cartItems = ShoppingCartService.Instance.CartItems;
        if (cartItems.Count == 0)
        {
            await DisplayAlert("Carrito Vacío", "No tienes productos para comprar.", "OK");
            return;
        }

        // --- CAMBIO 1: Calcular el total ANTES de ir a la BD ---
        // Necesitamos este valor para guardarlo en la tabla 'orders'
        double total = cartItems.Sum(p => p.Price);

        // --- CAMBIO 2: Usar ProcessOrderAsync en lugar de UpdateStockAsync ---
        // Le pasamos el carrito, el ID del usuario y el Total calculado.
        bool updateSuccess = await _dbService.ProcessOrderAsync(cartItems, currentUser.UserId, total);

        if (updateSuccess)
        {
            // --- CONSTRUIR EL MENSAJE FINAL ---

            // Formatear dirección
            string fullAddress = $"{StreetEntry.Text}, {ColoniaEntry.Text}, CP {ZipEntry.Text}\n{CityEntry.Text}, {StateEntry.Text}";

            // Obtener nombres de productos (El total ya lo calculamos arriba)
            string productNames = string.Join(", ", cartItems.Select(p => p.Name));

            string mensaje =
                $"✅ ¡Compra Exitosa!\n\n" +
                $"📦 Artículos: {productNames}\n" +
                $"💰 Total pagado: ${total:F2}\n\n" + // Usamos la variable 'total' de arriba
                $"🚚 Enviando a:\n{fullAddress}\n\n" +
                $"ℹ️ Tipo de envío: Express (3 días)\n\n" +
                $"📧 Se ha enviado la confirmación a: {currentUser.Email}";

            await DisplayAlert("Pedido Confirmado", mensaje, "Genial");

            // Limpiar y Salir
            ShoppingCartService.Instance.ClearCart();
            await Navigation.PopToRootAsync();
        }
        else
        {
            // Si falla, es probable que no haya stock suficiente
            await DisplayAlert("Error de Pago",
                "No se pudo procesar el pedido. Es posible que el inventario se haya agotado.",
                "OK");
        }
    }
}