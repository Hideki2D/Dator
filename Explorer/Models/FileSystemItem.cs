using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Explorer.Models;

public class FileSystemItem
{
    public required string Name { get; init; }

    public required string FullPath { get; init; }

    public required BitmapSource Icon { get; init; }

    public bool IsDirectory { get; init; }

    public bool IsDrive { get; init; }

    /// <summary>Двойной клик без модификаторов.</summary>
    public ICommand? OpenCommand { get; set; }

    /// <summary>Ctrl + двойной клик — открыть окно выбора приложения.</summary>
    public ICommand? OpenWithCommand { get; set; }
}