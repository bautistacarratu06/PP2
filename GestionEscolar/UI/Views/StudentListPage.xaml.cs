using UI.ViewModels;

namespace UI.Views
{
    public partial class StudentListPage : ContentPage
    {
        public StudentListPage()
        {
            InitializeComponent();

            BindingContext = new StudentListViewModel();
        }
    }
}