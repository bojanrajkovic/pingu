using System;
using System.IO;
using System.Linq;
using System.Net.Http;

using Microsoft.Extensions.CommandLineUtils;
using Newtonsoft.Json;

namespace Pingu.BenchmarkUploader
{
    class Program
    {
        static void Main(string[] args)
        {
            var cla = new CommandLineApplication(throwOnUnexpectedArg: true) {
                Description = "Pingu benchmark uploader.",
                FullName = "Pingu.BenchmarkUploader",
                Name = "BenchmarkUploader",
                ExtendedHelpText = Environment.NewLine + "Upload benchmark results to storage service."
            };

            var projectId = cla.Option("-i|--project-id", "The project ID to upload to.", CommandOptionType.SingleValue);
            var revision = cla.Option("-r|--revision", "The revision hash to create.", CommandOptionType.SingleValue);
            var branch = cla.Option("-b|--branch", "The branch of the revision.", CommandOptionType.SingleValue);
            var apiKey = cla.Option("-k|--api-key", "The API key of the project.", CommandOptionType.SingleValue);
            var dataFileFormat = cla.Option("-d|--data-format", "The data file format.", CommandOptionType.SingleValue);
            var dataFile = cla.Option("-f|--file", "The data file.", CommandOptionType.SingleValue);
            cla.HelpOption("-?|-h|--help");

            cla.OnExecute(() => {
                if (cla.Options.Any(o => !o.HasValue())) {
                    cla.ShowHelp();
                    return 1;
                }

                var jsonObject = new {
                    hash = revision.Value(),
                    branch = branch.Value(),
                    apiKey = apiKey.Value(),
                    dataFileFormat = dataFileFormat.Value(),
                    dataFile = Convert.ToBase64String(File.ReadAllBytes(dataFile.Value()))
                };

                var json = JsonConvert.SerializeObject(jsonObject);
                var content = new StringContent(json);
                content.Headers.Add("Content-Type", "application/json");

                var client = new HttpClient();

                var resp = client.PostAsync($"https://benchmarkly-dev.azurewebsites.net/api/v1.0/projects/{projectId.Value()}/revisions", content)
                                 .GetAwaiter()
                                 .GetResult();

                if (!resp.IsSuccessStatusCode) {
                    Console.WriteLine($"Failed: {resp.StatusCode}");
                    return 1;
                }

                return 0;
            });

            cla.Execute(args);
        }
    }
}
