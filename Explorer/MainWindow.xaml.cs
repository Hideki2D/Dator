using Explorer.ViewModels;
using System.Windows;

namespace Explorer
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel = new();

        public MainWindow()
        {
            InitializeComponent();

            DataContext = _viewModel;
            _viewModel.LoadDrives();
        }
    }
}