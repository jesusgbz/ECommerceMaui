namespace ECommerceMaui; // Asegúrate que coincida con tu proyecto

public partial class RegisterPage : ContentPage
{
    public RegisterPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Método que se ejecuta cuando el usuario presiona "Registrarme"
    /// </summary>
    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        // --- Validación (La agregaremos después) ---
        // Aquí iría la lógica para verificar que los campos no estén vacíos
        // y que las contraseñas coincidan.

        // --- Simulación de Éxito ---
        // Mostramos una alerta al usuario.
        await DisplayAlert("Éxito", "Tu cuenta ha sido creada.", "Aceptar");

        // --- Regresar al Login ---
        // Usamos PopAsync para "quitar" esta página de la pila
        // y revelar la página anterior (LoginPage).
        await Navigation.PopAsync();
    }
}