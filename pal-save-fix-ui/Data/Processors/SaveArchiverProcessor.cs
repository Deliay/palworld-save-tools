using System.IO.Compression;

namespace pal_save_fix_ui.Data.Processors;
public abstract class SaveArchiverProcessor : IDisposable
{
    protected static readonly string UnrealEngineSaveTools = Environment.GetEnvironmentVariable("UESAVE")
        ?? throw new InvalidOperationException("Missing ue-save tools");

    protected static readonly string ScriptPath = Environment.GetEnvironmentVariable("SCRIPT")
        ?? throw new InvalidOperationException("Missing script");

    protected static readonly string ScriptFolder = Path.GetDirectoryName(ScriptPath)
        ?? throw new InvalidOperationException("Missing script folder");

    protected SaveArchiverProcessor(Stream archive)
    {
        Archive = new ZipArchive(archive);

        var saveFiles = Archive.Entries.Where(entry => entry.FullName.EndsWith("Level.sav")).ToList();

        LevelEntry = saveFiles.Count switch
        {
            > 1 => throw new InvalidOperationException("There are more than one Level.sav in archive."),
            0 => throw new InvalidOperationException("No Level.sav in archive."),
            _ => saveFiles[0]
        };

        TempDirectory = Path.Combine(Path.GetTempPath(), $"pal-save-tools-{Guid.NewGuid().ToString()}");
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
        var ms = new MemoryStream();
        ZipFile.CreateFromDirectory(TempDirectory, ms);
        ms.Position = 0;
        return ms;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        using var archive = Archive;
        try
        {
            Directory.Delete(TempDirectory, true);
        }
        catch
        {
            // ignored
        }
    }
}
