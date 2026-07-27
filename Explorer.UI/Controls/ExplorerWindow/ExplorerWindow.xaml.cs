using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Explorer.UI.Controls;

public partial class ExplorerWindow : UserControl
{
    public ExplorerWindow()
    {
        InitializeComponent();
    }

    private void FileListItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: { } item })
            return;

        var property = item.GetType().GetProperty("OpenCommand");

        if (property?.GetValue(item) is ICommand command && command.CanExecute(null))
            command.Execute(null);
    }
}