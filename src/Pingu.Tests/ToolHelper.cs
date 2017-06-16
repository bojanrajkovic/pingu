using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Pingu.Tests
{
    class ToolHelper
    {
        public static ProcessResult RunPngCheck(string path)
        {
            var asm = typeof(ToolHelper).GetTypeInfo().Assembly;
            var assemblyDir = Path.GetDirectoryName(asm.Location);

            // It'll be on the path on Linux/Mac, we ship it for Windows.
            var pngcheckPath = Path.DirectorySeparatorChar == '\\' ? Path.Combine(
                assemblyDir,
                "..",
                "..",
                "..",
                "..",
                "..",
                "tools",
                "pngcheck.exe") : "pngcheck";

            return Exe(pngcheckPath, "-v", path);
        }

        public static ProcessResult Exe(string command, params string[] args)
        {
            var startInfo = new ProcessStartInfo {
                FileName = command,
                Arguments = string.Join(
                    " ", 
                    args.Where(x => !string.IsNullOrWhiteSpace(x))
                        .Select(x => "\"" + x + "\"")
                ),
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };

            var proc = Process.Start(startInfo);

            var @out = proc.StandardOutput.ReadToEnd();
            var err = proc.StandardError.ReadToEnd();
            proc.WaitForExit(20000);

            return new ProcessResult {
                ExitCode = proc.ExitCode,
                StandardOutput = @out,
                StandardError = err
            };
        }
    }
}
