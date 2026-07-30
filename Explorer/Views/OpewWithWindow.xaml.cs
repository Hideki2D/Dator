using Explorer.Models;
using Explorer.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace Explorer.Views;

public partial class OpenWithWindow : Window
{
    private readonly string _extension;
    private readonly string _filePath;

    public ObservableCollection<AppCandidate> Apps { get; } = new();

    public OpenWithWindow(string filePath)
    {
        InitializeComponent();

        _filePath = filePath;
        _extension = Path.GetExtension(filePath);

        DataContext = this;

        foreach (var app in FileAssociationService.GetCandidateApps(_extension))
            Apps.Add(app);

        AppsList.SelectedIndex = Apps.Count > 0 ? 0 : -1;
    }

    private void Browse_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Выбор приложения",
            Filter = "Программы (*.exe)|*.exe|Все файлы (*.*)|*.*"
        };

        if (dialog.ShowDialog(this) != true)
            return;

        var custom = new AppCandidate
        {
            DisplayName = Path.GetFileNameWithoutExtension(dialog.FileName),
            ExecutablePath = dialog.FileName,
            CommandLineTemplate = $"\"{dialog.FileName}\" \"%1\"",
            Icon = ShellIconProvider.GetIcon(dialog.FileName)
        };

        Apps.Insert(0, custom);
        AppsList.SelectedIndex = 0;
    }

    private void Open_Click(object sender, RoutedEventArgs e)
    {
        if (AppsList.SelectedItem is not AppCandidate app)
            return;

        FileAssociationService.Launch(app, _filePath);

        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}