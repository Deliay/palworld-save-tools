
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace PalworldToolset.Data.Processors;

public class MigrateLocalToServerProcessor(Stream archive) : SaveArchiverProcessor(archive)
{

    private IEnumerable<string> Args(string from, string to)
    {
        yield return ScriptPath;
        yield return UnrealEngineSaveTools;
        yield return SaveFolder;
        yield return to;
        yield return from;
    }

    public async IAsyncEnumerable<string> Migrate(Dictionary<string, string> userIdMapping, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var (from, to) in userIdMapping)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo("python", Args(from, to))
                {
                    WorkingDirectory = ScriptFolder,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                },
            };
            process.Start();
            yield return $"Start processing {from} to {to}, pid={process.Id}...";
            yield return $"Run: {process.StartInfo.FileName} {string.Join(' ', process.StartInfo.ArgumentList)}";
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
            yield return $"{from} to {to} proceed, pid={process.Id} exited with {process.ExitCode}";
        }
    }

}