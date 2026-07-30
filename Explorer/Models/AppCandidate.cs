using System.Windows.Media.Imaging;

namespace Explorer.Models;

public class AppCandidate
{
    public required string DisplayName { get; init; }

    public required string ExecutablePath { get; init; }

    public string? CommandLineTemplate { get; set; }

    public string? ProgId { get; set; }

    public BitmapSource? Icon { get; init; }
}