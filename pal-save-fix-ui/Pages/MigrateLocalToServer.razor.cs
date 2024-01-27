using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using pal_save_fix_ui.Data.Processors;
using System;
using System.Diagnostics;
using System.IO;

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
        }

        private int progress = 0;

        private async Task ProcessFile(InputFileChangeEventArgs e)
        {
            isLoading = true;
            loadedFiles.Clear();

            foreach (var file in e.GetMultipleFiles(maxAllowedFiles))
            {
                try
                {
                    Log("Uploading files");
                    using var processor = new MigrateLocalToServerProcessor(file.OpenReadStream(maxFileSize, _cts.Token));
                    progress = 50;
                    Log("Extracting files...");
                    processor.ExtractAllFiles();
                    Log("Files extracted!");
                    progress = 60;
                    int each = 30 / GuidMapping.Count;
                    await foreach(string log in processor.Migrate(GuidMapping, _cts.Token))
                    {
                        Log(log);
                        progress += each;
                    }
                    progress = 90;
                    Log("Operation completed. Archiving procceed files");
                    using var procceedArchiveFileStream = processor.ArchiveFiles();

                    progress = 95;
                    Log("Downloading files...");
                    var fileName = $"{Path.GetFileNameWithoutExtension(file.Name)}-to-dedicated-save.zip";

                    using var streamRef = new DotNetStreamReference(stream: procceedArchiveFileStream);

                    await JS.InvokeVoidAsync("downloadFileFromStream", _cts.Token, fileName, streamRef);
                    Log("Done");
                    progress = 100;
                }
                catch (Exception ex)
                {
                    Logger.LogError("File: {Filename} Error: {Error}",
                        file.Name, ex.Message);
                }
            }

            isLoading = false;
        }
    }
}
