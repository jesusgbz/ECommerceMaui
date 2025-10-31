namespace ECommerceMaui
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Esta es la línea clave que modificamos en el paso 3.1
            // En lugar de cargar AppShell o solo LoginPage, cargamos
            // una NavigationPage que contiene LoginPage.
            MainPage = new NavigationPage(new LoginPage());
        }
    }
}