using System.Windows.Media.Imaging;

public class FileSystemItem
{
    public required string Name { get; init; }

    public required string FullPath { get; init; }

    public required BitmapSource Icon { get; init; }

    public bool IsDirectory { get; init; }

    public bool IsDrive { get; init; }
}