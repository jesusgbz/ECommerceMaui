namespace ECommerceMaui
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            
            // En lugar de cargar AppShell o solo LoginPage, cargamos
            // una NavigationPage que contiene LoginPage.
            MainPage = new NavigationPage(new LoginPage());
        }
    }
}