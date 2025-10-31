namespace ECommerceMaui; // <-- ¡Nombre del proyecto actualizado!

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
	}

    /// <summary>
    /// Método que se ejecuta cuando el usuario presiona el botón "Regístrate"
    /// </summary>
    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        // Navegamos a la página de Registro
        // PushAsync "empuja" la nueva página encima de la actual
        await Navigation.PushAsync(new RegisterPage());
    }

    /// <summary>
    /// Se ejecuta al presionar "Ingresar".
    /// Simula un login exitoso y carga la página principal de la tienda (AppShell).
    /// </summary>
    private void OnLoginClicked(object sender, EventArgs e)
    {
        // --- SIMULACIÓN DE LOGIN ---
        // Aquí iría la lógica de validación contra la base de datos
        // (ej. if(email == "admin" && pass == "123"))

        // Si el login es exitoso, cambiamos la página principal de la aplicación.
        // Esto "descarta" la página de Login y carga la AppShell.
        Application.Current.MainPage = new AppShell();
    }
}