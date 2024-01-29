using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using PalworldToolset.Data.Processors;

namespace PalworldToolset.Pages
{
    public partial class MigrateLocalToServer : IDisposable
    {
        private readonly CancellationTokenSource _cts = new();

        private readonly Dictionary<string, string> _guidMapping = [];

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            using var cts = _cts;
        }

        private string _lastLog = string.Empty;

        private void Log(string message)
        {
            _processLogs.Add(message);
            _lastLog = message;
            StateHasChanged();
        }

        private int _progress;

        private async Task ProcessFile(InputFileChangeEventArgs e)
        {
            _isLoading = true;
            _processLogs.Clear();
            StateHasChanged();

            foreach (var file in e.GetMultipleFiles(MaxAllowedFiles))
            {
                try
                {
                    _progress = 10;
                    Log("Uploading files");
                    await using var fileStream = file.OpenReadStream(MaxFileSize, _cts.Token);
                    // var pipe = new Pipe(new PipeOptions(useSynchronizationContext: false));
                    // await fileStream.CopyToAsync(pipe.Writer, _cts.Token);
                    // await using var readStream = pipe.Reader.AsStream();
                    await using var memoryStream = new MemoryStream();
                    await fileStream.CopyToAsync(memoryStream, _cts.Token);
                    using var processor = new MigrateLocalToServerProcessor(memoryStream);
                    
                    _progress = 30;
                    Log("Extracting files...");
                    processor.ExtractAllFiles();
                    Log("Files extracted!");
                    _progress = 40;
                    var each = 50 / _guidMapping.Count;
                    await foreach(var log in processor.Migrate(_guidMapping, _cts.Token))
                    {
                        Log(log);
                        _progress += each;
                    }
                    _progress = 90;
                    
                    Log("Operation completed. Archiving processed files");

                    _progress = 95;
                    Log("Downloading files...");
                    var fileName = $"{Path.GetFileNameWithoutExtension(file.Name)}-to-dedicated-save.zip";

                    using var streamRef = new DotNetStreamReference(stream: processor.ArchiveFiles());

                    await JS.InvokeVoidAsync("downloadFileFromStream", _cts.Token, fileName, streamRef);
                    Log("Done");
                    _progress = 100;
                }
                catch (Exception ex)
                {
                    Log($"File: {file.Name} Error: {ex.Message}\n{ex.StackTrace}");
                }
            }

            _isLoading = false;
        }
    }
}
