namespace Pingu.Tests
{
    public class ProcessResult
    {
        public int ExitCode { get; internal set; }
        public string StandardError { get; internal set; }
        public string StandardOutput { get; internal set; }
    }
}