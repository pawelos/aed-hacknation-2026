using System.Text.Json;

internal static class DockerCompose
{
    // Starts the infra from docker-compose.yml (fixed ports); containers survive AppHost stop.
    public static void EnsureUp(string projectDir)
    {
        string composeFile = Path.Combine(projectDir, "docker-compose.yml");
        if (!File.Exists(composeFile))
        {
            throw new FileNotFoundException(
                $"docker-compose.yml not found at '{composeFile}'.");
        }

        DockerDesktop.EnsureRunning();
        RemoveConflictingContainers(projectDir);

        DockerResult up = DockerCli.Run("compose up -d", projectDir).EnsureSuccess("compose up -d");

        Console.WriteLine($"Infra containers ready ({projectDir}).");
        if (!string.IsNullOrWhiteSpace(up.Stdout))
        {
            Console.WriteLine(up.Stdout.Trim());
        }
    }

    // Removes containers that use our container_name but belong to another compose project
    // (e.g. created before the repo folder was renamed); otherwise "compose up" fails with "Conflict".
    // "docker rm" keeps volumes, so the other project's data is not lost.
    private static void RemoveConflictingContainers(string projectDir)
    {
        const string configArguments = "compose config --format json";
        DockerResult config = DockerCli.Run(configArguments, projectDir).EnsureSuccess(configArguments);

        using JsonDocument configJson = JsonDocument.Parse(config.Stdout);
        string projectName = configJson.RootElement.GetProperty("name").GetString()!;

        foreach (string containerName in GetContainerNames(configJson.RootElement))
        {
            if (!TryGetComposeProject(containerName, out string? owner) || owner == projectName)
            {
                continue;
            }

            Console.WriteLine(
                $"Removing container '{containerName}' from compose project '{owner ?? "(none)"}' (name conflict).");
            string rmArguments = $"rm -f {containerName}";
            DockerCli.Run(rmArguments).EnsureSuccess(rmArguments);
        }
    }

    private static IEnumerable<string> GetContainerNames(JsonElement config)
    {
        foreach (JsonProperty service in config.GetProperty("services").EnumerateObject())
        {
            if (service.Value.TryGetProperty("container_name", out JsonElement name))
            {
                yield return name.GetString()!;
            }
        }
    }

    // False when the container does not exist; project is null when it has no compose label.
    private static bool TryGetComposeProject(string containerName, out string? project)
    {
        project = null;

        DockerResult inspect = DockerCli.Run($"inspect --format \"{{{{json .Config.Labels}}}}\" {containerName}");
        if (!inspect.Succeeded)
        {
            return false;
        }

        using JsonDocument labels = JsonDocument.Parse(inspect.Stdout);
        if (labels.RootElement.ValueKind == JsonValueKind.Object
            && labels.RootElement.TryGetProperty("com.docker.compose.project", out JsonElement owner))
        {
            project = owner.GetString();
        }

        return true;
    }
}
