using Microsoft.Extensions.DependencyInjection;

namespace UI
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell())
            {
                Title = "Gestión Escolar",
                Width = 1360,
                Height = 900
            };
        }
    }
}