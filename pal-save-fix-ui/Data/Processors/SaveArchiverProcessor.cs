using System.IO.Compression;

namespace pal_save_fix_ui.Data.Processors;
public abstract class SaveArchiverProcessor : IDisposable
{
    private string _newArchiveName = Path.GetTempFileName();

    public static readonly string UnrealEngineSaveTools = Environment.GetEnvironmentVariable("UESAVE")
        ?? throw new InvalidOperationException("Missing uesave tools");

    public static readonly string ScriptPath = Environment.GetEnvironmentVariable("SCRIPT")
        ?? throw new InvalidOperationException("Missing script");

    public static readonly string ScriptFolder = Path.GetDirectoryName(ScriptPath)
        ?? throw new InvalidOperationException("Missing script folder");

    public SaveArchiverProcessor(Stream archive)
    {
        Archive = new ZipArchive(archive);

        LevelEntry = Archive.Entries.Where(entry => entry.FullName.EndsWith("Level.sav")).Single()
            ?? throw new InvalidDataException();

        TempDirectory = Path.GetTempPath();
    }

    public ZipArchive Archive { get; }
    public ZipArchiveEntry LevelEntry { get; }
    public string LevelEntryExtractedPath => Path.Combine(TempDirectory, LevelEntry.FullName);
    public string SaveFolder => Path.GetDirectoryName(LevelEntryExtractedPath) ?? throw new InvalidDataException("Can't find save folder in archive");
    public string TempDirectory { get; }

    public void ExtractAllFiles()
    {
        if (!Directory.Exists(TempDirectory)) Directory.CreateDirectory(TempDirectory);
        Archive.ExtractToDirectory(TempDirectory);
    }

    public Stream ArchiveFiles()
    {
        _newArchiveName = Path.GetTempFileName();
        ZipFile.CreateFromDirectory(TempDirectory, _newArchiveName);

        return File.OpenRead(_newArchiveName);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        using var _Archive = Archive;
        try
        {
            Directory.Delete(TempDirectory, true);
        }
        finally { }
        try
        {
            File.Delete(_newArchiveName);
        }
        finally { }
    }
}
