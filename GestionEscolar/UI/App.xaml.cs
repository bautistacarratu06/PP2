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
                // * este es nuestro entrypoint que monta las shells
                // Shell muestra el primer ShellContent. tenemos q tenerlo en cuenta 
                // para la navegación. Investigar como hacer 1 sidebar
                Title = "Gestión Escolar PP2",
                Width = 1360,
                Height = 900
            };
        }
    }
}