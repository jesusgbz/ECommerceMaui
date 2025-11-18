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
}