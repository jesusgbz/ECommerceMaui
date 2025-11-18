using ECommerceMaui.Servicios; // ¡Importante! Acceso al servicio

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
        // 1. ¡Intentamos actualizar el stock primero!
        //    Obtenemos la lista de ítems desde nuestro carrito global
        var cartItems = ShoppingCartService.Instance.CartItems;

        bool updateSuccess = await _dbService.UpdateStockAsync(cartItems);

        if (updateSuccess)
        {
            // --- SIMULACIÓN DE PAGO EXITOSO ---

            // 2. Mostrar alerta de éxito
            await DisplayAlert("Pago Exitoso", "Tu pago ha sido procesado y el inventario ha sido actualizado.", "OK");

            // 3. Vaciar el carrito de compras (local)
            ShoppingCartService.Instance.ClearCart();

            // 4. Regresar al usuario a la página de inicio
            await Navigation.PopToRootAsync();
        }
        else
        {
            // --- FALLO LA ACTUALIZACIÓN DE STOCK ---
            await DisplayAlert("Error de Pago",
                "No se pudo procesar el pedido. No hay suficiente inventario para uno o más productos. Por favor, revisa tu carrito.",
                "OK");

            // No limpiamos el carrito y no regresamos,
            // para que el usuario pueda arreglar su pedido.
        }
    }
}