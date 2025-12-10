using ECommerceMaui.Models;
using ECommerceMaui.Servicios; // ¡Importante!

namespace ECommerceMaui;

public partial class LoginPage : ContentPage
{
    private readonly DatabaseService _dbService;

    public LoginPage()
    {
        InitializeComponent();
        _dbService = new DatabaseService();
    }

    /// <summary>
    /// Se ejecuta al presionar "Ingresar".
    /// (Lógica actualizada para guardar la sesión)
    /// </summary>
    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text;
        var password = PasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Error", "Por favor, ingresa correo y contraseña.", "OK");
            return;
        }

        // 3. No esperamos un 'bool', esperamos un 'User'
        User loggedInUser = await _dbService.LoginUserAsync(email, password);

        // 4. Verificamos si el resultado es nulo
        if (loggedInUser != null)
        {
            // ¡Login Exitoso!

            // 5. Guardamos al usuario en nuestro servicio Singleton global
            Services.AuthService.Instance.Login(loggedInUser);

            // Reemplazamos la página actual (Login) por la tienda (AppShell).
            Application.Current.MainPage = new AppShell();
        }
        else
        {
            // Error
            await DisplayAlert("Login Fallido", "Correo o contraseña incorrectos. Por favor, intenta de nuevo.", "OK");
            PasswordEntry.Text = string.Empty;
        }
    }

    /// <summary>
    /// Se ejecuta al presionar "Regístrate".
    /// (Esta no cambia)
    /// </summary>
    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage());
    }
}