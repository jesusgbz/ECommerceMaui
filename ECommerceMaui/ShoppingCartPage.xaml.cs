using ECommerceMaui.Models;
using ECommerceMaui.Servicios; // ¡Importante! Acceso al servicio

namespace ECommerceMaui;

public partial class ShoppingCartPage : ContentPage
{
    public ShoppingCartPage()
    {
        InitializeComponent();

        // --- ¡LA MAGIA! ---
        // Conectamos el ItemsSource del CollectionView
        // a la lista ObservableCollection de nuestro Singleton.
        CartCollectionView.ItemsSource = ShoppingCartService.Instance.CartItems;
    }

    // Este método es para el botón "Proceder al Pago"
    private async void OnCheckoutClicked(object sender, EventArgs e)
    {
        // En un futuro, iríamos a la página de pago.
        // Por ahora, solo mostramos una alerta.
        await Navigation.PushAsync(new PaymentPage());

        // (Aquí es donde crearíamos la página de pago que pediste)
    }

    /// <summary>
    /// Elimina un solo producto del carrito
    /// </summary>
    private void OnRemoveItemClicked(object sender, EventArgs e)
    {
        var button = sender as ImageButton;
        var product = button.CommandParameter as Product;

        // Llamamos al servicio para quitarlo
        ShoppingCartService.Instance.RemoveItem(product);
    }

    /// <summary>
    /// Vacía todo el carrito
    /// </summary>
    private async void OnClearCartClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Vaciar Carrito", "¿Estás seguro de que quieres eliminar todos los productos?", "Sí", "No");
        if (confirm)
        {
            ShoppingCartService.Instance.ClearCart();
        }
    }
}