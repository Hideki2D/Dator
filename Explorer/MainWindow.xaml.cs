using Explorer.ViewModels;
using Explorer.Views;
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
            _viewModel.OpenWithRequested += OnOpenWithRequested;
            _viewModel.LoadDrives();
        }

        private void OnOpenWithRequested(string filePath)
        {
            var window = new OpenWithWindow(filePath) { Owner = this };
            window.ShowDialog();
        }
    }
}