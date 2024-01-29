using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using pal_save_fix_ui.Data.Processors;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.IO.Pipes;
using PipeOptions = System.IO.Pipelines.PipeOptions;

namespace pal_save_fix_ui.Pages
{
    public partial class MigrateLocalToServer : IDisposable
    {
        private readonly CancellationTokenSource _cts = new();

        private readonly Dictionary<string, string> GuidMapping = [];

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            using var __cts = _cts;
        }

        private string LastLog = string.Empty;

        private void Log(string message)
        {
            ProcessLogs.Add(message);
            LastLog = message;
            StateHasChanged();
        }

        private int progress = 0;

        private async Task ProcessFile(InputFileChangeEventArgs e)
        {
            isLoading = true;
            ProcessLogs.Clear();
            StateHasChanged();
            loadedFiles.Clear();

            foreach (var file in e.GetMultipleFiles(maxAllowedFiles))
            {
                try
                {
                    progress = 10;
                    Log("Uploading files");
                    await using var fileStream = file.OpenReadStream(maxFileSize, _cts.Token);
                    // var pipe = new Pipe(new PipeOptions(useSynchronizationContext: false));
                    // await fileStream.CopyToAsync(pipe.Writer, _cts.Token);
                    // await using var readStream = pipe.Reader.AsStream();
                    await using var memoryStream = new MemoryStream();
                    await fileStream.CopyToAsync(memoryStream, _cts.Token);
                    using var processor = new MigrateLocalToServerProcessor(memoryStream);
                    
                    progress = 30;
                    Log("Extracting files...");
                    processor.ExtractAllFiles();
                    Log("Files extracted!");
                    progress = 40;
                    var each = 50 / GuidMapping.Count;
                    await foreach(var log in processor.Migrate(GuidMapping, _cts.Token))
                    {
                        Log(log);
                        progress += each;
                    }
                    progress = 90;
                    
                    Log("Operation completed. Archiving proceed files");

                    progress = 95;
                    Log("Downloading files...");
                    var fileName = $"{Path.GetFileNameWithoutExtension(file.Name)}-to-dedicated-save.zip";

                    using var streamRef = new DotNetStreamReference(stream: processor.ArchiveFiles());

                    await JS.InvokeVoidAsync("downloadFileFromStream", _cts.Token, fileName, streamRef);
                    Log("Done");
                    progress = 100;
                }
                catch (Exception ex)
                {
                    Log($"File: {file.Name} Error: {ex.Message}");
                }
            }

            isLoading = false;
        }
    }
}
