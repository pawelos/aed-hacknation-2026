using System.Diagnostics;
using System.Text;

internal sealed record DockerResult(int ExitCode, string Stdout, string Stderr)
{
    public bool Succeeded => ExitCode == 0;

    public DockerResult EnsureSuccess(string arguments)
    {
        if (!Succeeded)
        {
            throw new InvalidOperationException(
                $"docker {arguments} failed (exit {ExitCode}).{Environment.NewLine}{Stderr}{Stdout}");
        }

        return this;
    }
}

internal static class DockerCli
{
    public static DockerResult Run(string arguments, string? workingDirectory = null)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = arguments,
            WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using Process process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Failed to start 'docker {arguments}'.");

        // Read stderr asynchronously so a full stderr pipe cannot block the stdout read.
        Task<string> stderrTask = process.StandardError.ReadToEndAsync();
        string stdout = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        return new DockerResult(process.ExitCode, stdout, stderrTask.Result);
    }
}
