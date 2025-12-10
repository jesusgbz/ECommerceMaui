using ECommerceMaui.Services;
using ECommerceMaui.Servicios; // ¡Importante!

namespace ECommerceMaui;

public partial class ProfilePage : ContentPage
{
    // Necesitamos ambos servicios
    private readonly AuthService _authService;
    private readonly DatabaseService _dbService;

    public ProfilePage()
    {
        InitializeComponent();

        // Obtenemos las instancias de nuestros Singletons
        _authService = AuthService.Instance;
        _dbService = new DatabaseService();
    }

    /// <summary>
    /// Se ejecuta cada vez que la página de perfil aparece
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadUserProfile();
    }

    /// <summary>
    /// Carga los datos del usuario logueado en la UI
    /// </summary>
    private void LoadUserProfile()
    {
        var currentUser = _authService.CurrentUser;

        if (currentUser != null)
        {
            FullNameLabel.Text = currentUser.FullName;
            EmailLabel.Text = currentUser.Email;

            // Cargar el avatar
            UserAvatarImage.Source = currentUser.AvatarUrl;
        }
    }

    /// <summary>
    /// Se ejecuta al presionar "Cerrar Sesión"
    /// </summary>
    private void OnLogoutClicked(object sender, EventArgs e)
    {
        // 1. Limpiamos la sesión global
        _authService.Logout();

        // 2. Limpiamos el carrito de compras (por si acaso)
        ShoppingCartService.Instance.ClearCart();

        // 3. Regresamos al Login
        Application.Current.MainPage = new NavigationPage(new LoginPage());
    }

    /// <summary>
    /// Se ejecuta al presionar "Eliminar Mi Cuenta"
    /// </summary>
    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        var currentUser = _authService.CurrentUser;
        if (currentUser == null) return;

        // 1. ¡Pedir confirmación! Esta acción es irreversible.
        bool answer = await DisplayAlert(
            "Confirmación Requerida",
            "¿Estás seguro de que quieres eliminar tu cuenta permanentemente? Esta acción no se puede deshacer.",
            "SÍ, ELIMINAR",
            "Cancelar");

        if (answer) // Si el usuario presionó "SÍ, ELIMINAR"
        {
            // 2. Intentar eliminar de la BBDD
            bool isSuccess = await _dbService.DeleteUserAsync(currentUser.UserId);

            if (isSuccess)
            {
                await DisplayAlert("Cuenta Eliminada", "Tu cuenta ha sido eliminada exitosamente.", "OK");

                // 3. Cerrar sesión y regresar al Login
                OnLogoutClicked(null, null); // Reutilizamos la lógica de cerrar sesión
            }
            else
            {
                await DisplayAlert("Error", "No se pudo eliminar tu cuenta. Por favor, intenta más tarde.", "OK");
            }
        }
    }
}