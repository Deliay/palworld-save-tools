
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace pal_save_fix_ui.Data.Processors;

public class MigrateLocalToServerProcessor(Stream archive) : SaveArchiverProcessor(archive)
{

    private IEnumerable<string> Args(string oldGuid, string newGuid)
    {
        yield return ScriptPath;
        yield return UnrealEngineSaveTools;
        yield return SaveFolder;
        yield return newGuid;
        yield return oldGuid;
    }

    public async IAsyncEnumerable<string> Migrate(Dictionary<string, string> userIdMapping, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var (oldGuid, newGuid) in userIdMapping)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo("python", Args(oldGuid, newGuid))
                {
                    WorkingDirectory = ScriptFolder,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                },
            };
            process.Start();

            yield return $"Start processing {oldGuid} to {newGuid}, pid={process.Id}...";
            yield return $"This will take about 2 minutes...";
            do
            {
                var log = await process.StandardOutput.ReadLineAsync(cancellationToken);
                if (log != null)
                {
                    yield return log;
                }
            } while (!cancellationToken.IsCancellationRequested && !process.HasExited);
            
            var errLog = await process.StandardError.ReadToEndAsync(cancellationToken);
            yield return errLog;

            await process.WaitForExitAsync(cancellationToken);
            yield return $"{oldGuid} to {newGuid} proceed, pid={process.Id} exited with {process.ExitCode}";
        }
    }

}