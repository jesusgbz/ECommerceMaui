namespace ECommerceMaui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // --- INICIO DE CÓDIGO A AGREGAR ---

        // Registramos las rutas para poder navegar a ellas
        Routing.RegisterRoute(nameof(HomePage), typeof(HomePage));
        Routing.RegisterRoute(nameof(ShoppingCartPage), typeof(ShoppingCartPage));
        Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));

        // --- FIN DE CÓDIGO A AGREGAR ---
    }
}