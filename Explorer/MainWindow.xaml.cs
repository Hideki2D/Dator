using Explorer.Services;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Explorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //MainViewModel newViewModel = new MainViewModel();
        public MainWindow()
        {
            InitializeComponent();
            //newViewModel.LoadDrives();
            //DataContext = newViewModel;
        }

        //private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        //{
        //    if (ItemsList.SelectedItem is not FileSystemItem item)
        //        return;

        //    if (item.IsDrive || item.IsDirectory)
        //        newViewModel.LoadFolder(item.FullPath);
        //}
    }
}