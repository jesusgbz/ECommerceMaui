using ECommerceMaui.Helpers; // ¡Importante! Acceso al Hasher
using ECommerceMaui.Servicios; // ¡Importante! Acceso a la BBDD

namespace ECommerceMaui;

public partial class RegisterPage : ContentPage
{
    private readonly DatabaseService _dbService;

    public RegisterPage()
    {
        InitializeComponent();
        _dbService = new DatabaseService();
    }

    /// <summary>
    /// Se ejecuta al presionar "Crear Cuenta".
    /// ¡Ahora con lógica real!
    /// </summary>
    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        // 1. Obtener datos de la UI
        var fullName = FullNameEntry.Text;
        var email = EmailEntry.Text;
        var password = PasswordEntry.Text;
        var confirmPassword = ConfirmPasswordEntry.Text;

        // 2. Validar entradas
        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
        {
            await DisplayAlert("Error", "Todos los campos son obligatorios.", "OK");
            return;
        }

        if (password != confirmPassword)
        {
            await DisplayAlert("Error", "Las contraseñas no coinciden.", "OK");
            return;
        }

        // 3. Cifrar la contraseña
        var passwordHash = PasswordHelper.HashPassword(password);

        // 4. Intentar registrar en la BBDD
        bool isSuccess = await _dbService.RegisterUserAsync(email, fullName, passwordHash);

        // 5. Mostrar resultado
        if (isSuccess)
        {
            await DisplayAlert("Éxito", "Usuario registrado exitosamente. Ahora puedes iniciar sesión.", "OK");
            // Regresar al Login
            await Navigation.PopAsync();
        }
        else
        {
            // El error (ej. email duplicado) ya se imprimió en la consola
            // desde el DatabaseService.
            await DisplayAlert("Error", "No se pudo completar el registro. Es posible que el correo ya esté en uso.", "OK");
        }
    }

    /// <summary>
    /// Se ejecuta al presionar "Ya tengo cuenta (Volver)".
    /// (Esta no cambia)
    /// </summary>
    private async void OnBackClicked(object sender, EventArgs e)
    {
        // Simplemente regresamos a la página anterior (Login).
        await Navigation.PopAsync();
    }
}