namespace AcademiaDoZe.Presentation.AppMaui
{
    // "Application" conflita com o nome da nossa camada de aplicação (AcademiaDoZe.Application).
    // Por isso herdamos explicitamente de Microsoft.Maui.Controls.Application, direcionando
    // para a classe Application do MAUI e evitando o conflito de nomes.
    public partial class App : Microsoft.Maui.Controls.Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}
