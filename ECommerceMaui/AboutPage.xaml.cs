using System.Collections.ObjectModel;

namespace ECommerceMaui;

public partial class AboutPage : ContentPage
{
    // Clase sencilla para representar a un miembro (Solo vive aquí)
    public class TeamMember
    {
        public string Name { get; set; }
        public string Role { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
    }

    public ObservableCollection<TeamMember> Team { get; set; }

    public AboutPage()
    {
        InitializeComponent();
        LoadTeam();
        TeamCollectionView.ItemsSource = Team;
    }

    private void LoadTeam()
    {
        Team = new ObservableCollection<TeamMember>
        {
            new TeamMember
            {
                Name = "Zoe",
                Role = "CEO / Director General",
                Description = "DarkPX nació de su mente a los 8 años. Lidera la visión estratégica y expansión. Aún conserva su primer Chevy.",
                // Usamos un avatar de mujer (puedes poner su foto real después)
                ImageUrl = "https://i.imgur.com/GvLJihB.jpeg"
            },
            new TeamMember
            {
                Name = "Jesús",
                Role = "Gerente de Sistemas",
                Description = "Arquitecto de software y desarrollador Full-Stack del proyecto. Le gustan las varitas de pescado.",
                // Tu icono de Hacker
                ImageUrl = "https://i.imgur.com/aVuKkaQ.jpeg"
            },
            new TeamMember
            {
                Name = "Victoria",
                Role = "Gerente de Marketing",
                Description = "Encargada de la identidad visual y posicionamiento de la marca. Fan de la Navidad.",
                ImageUrl = "https://i.imgur.com/VWYSMmk.jpeg"
            },
            new TeamMember
            {
                Name = "Eithan",
                Role = "Gerente de Operaciones",
                Description = "Asegura que la logística y el inventario funcionen sin fallos. Habla con las ardillas. Jefe de tropa scout los fines de semana.",
                ImageUrl = "https://i.imgur.com/XyjFcMT.jpeg"
            }
            
        };
    }

    /// <summary>
    /// Se ejecuta al tocar la foto de un miembro.
    /// Crea una página modal temporal para ver la foto en grande.
    /// </summary>
    private async void OnMemberImageTapped(object sender, TappedEventArgs e)
    {
        // 1. Obtenemos los datos del miembro que fue tocado
        var member = e.Parameter as TeamMember;
        if (member == null) return;

        // 2. Creamos una página nueva "en el aire" (sin archivo XAML extra)
        var zoomPage = new ContentPage
        {
            BackgroundColor = Color.FromArgb("#1C1C1E"), // Fondo DarkPX
            Title = member.Name
        };

        // 3. Construimos el contenido de esa página (Grid con Botón Cerrar e Imagen)
        var layout = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto }, // Fila 0: Botón Cerrar
                new RowDefinition { Height = GridLength.Star }, // Fila 1: Imagen
                new RowDefinition { Height = GridLength.Auto }  // Fila 2: Nombre
            }
        };

        // Botón de Cerrar (X)
        var closeButton = new Button
        {
            Text = "Cerrar [X]",
            TextColor = Colors.White,
            BackgroundColor = Colors.Transparent,
            HorizontalOptions = LayoutOptions.End,
            Margin = new Thickness(0, 10, 10, 0)
        };
        closeButton.Clicked += async (s, args) => await Navigation.PopModalAsync(); // Acción de cerrar

        // La Imagen Grande
        var bigImage = new Image
        {
            Source = member.ImageUrl,
            Aspect = Aspect.AspectFit, // Para que se vea completa sin cortes
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Margin = 20
        };

        // El Nombre y Puesto (Opcional, para que se vea elegante)
        var infoLabel = new Label
        {
            Text = $"{member.Name}\n{member.Role}",
            TextColor = Colors.White,
            HorizontalTextAlignment = TextAlignment.Center,
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            Margin = new Thickness(0, 0, 0, 40)
        };

        // 4. Armamos el rompecabezas
        layout.Add(closeButton, 0, 0); // Fila 0
        layout.Add(bigImage, 0, 1);    // Fila 1
        layout.Add(infoLabel, 0, 2);   // Fila 2

        zoomPage.Content = layout;

        // 5. ¡Mostramos la página! (Modo Modal = Cubre toda la pantalla)
        await Navigation.PushModalAsync(zoomPage);
    }
}