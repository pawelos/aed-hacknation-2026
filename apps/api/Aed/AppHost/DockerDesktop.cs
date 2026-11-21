using System.Diagnostics;

internal static class DockerDesktop
{
    private static readonly TimeSpan StartTimeout = TimeSpan.FromMinutes(3);
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);

    // Starts Docker Desktop when the daemon is down and waits until "docker info" succeeds.
    public static void EnsureRunning()
    {
        if (DockerCli.Run("info").Succeeded)
        {
            return;
        }

        Console.WriteLine("Docker is not running - starting Docker Desktop...");
        Start();
        WaitUntilReady();
    }

    private static void Start()
    {
        // "docker desktop start" exists since Docker Desktop 4.37; older versions need the exe.
        if (DockerCli.Run("desktop start").Succeeded)
        {
            return;
        }

        string exe = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            "Docker", "Docker", "Docker Desktop.exe");
        if (!File.Exists(exe))
        {
            throw new InvalidOperationException(
                $"Docker Desktop not found at '{exe}'. Start Docker manually.");
        }

        Process.Start(new ProcessStartInfo(exe) { UseShellExecute = true });
    }

    private static void WaitUntilReady()
    {
        var stopwatch = Stopwatch.StartNew();
        while (true)
        {
            DockerResult info = DockerCli.Run("info");
            if (info.Succeeded)
            {
                Console.WriteLine("Docker is ready.");
                return;
            }

            if (stopwatch.Elapsed > StartTimeout)
            {
                throw new InvalidOperationException(
                    $"Docker did not start within {StartTimeout.TotalMinutes} min.{Environment.NewLine}{info.Stderr}");
            }

            Thread.Sleep(PollInterval);
        }
    }
}
