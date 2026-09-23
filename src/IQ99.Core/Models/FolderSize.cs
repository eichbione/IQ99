namespace IQ99.Core.Models;

public sealed class FolderSize
{
    public required string Path { get; init; }
    public long SizeBytes { get; init; }
    public string SizeText => Formatter.FormatBytes(SizeBytes);
    public double Percent { get; set; }
}