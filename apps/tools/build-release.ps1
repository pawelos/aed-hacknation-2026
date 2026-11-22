# Builds the release images for the VPS: the server never builds and has no source code.
# Runs on Windows (Docker Desktop) and on the Azure DevOps Linux agent (deploy/azure-pipelines.yml).
# Pattern: C:\car-reminder\apps\tools\build-release.ps1.
#
#   .\tools\build-release.ps1
#
# Result in release\<version>\ (git-ignored):
#   images.tar.gz  aed/api and aed/web images
#   version.txt    the version (read by the deploy pipeline)
# Deploying them (deploy/WDROZENIE.md): ./deploy/scripts/deploy.sh app load <images.tar.gz> && ./deploy/scripts/deploy.sh app up
$ErrorActionPreference = 'Stop'

# Forward slashes: the same script runs on the Linux agent (pwsh), where \ is not a separator.
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
Set-Location $root

function Invoke-Native {
    param([string] $Description, [scriptblock] $Command)
    Write-Host "==> $Description" -ForegroundColor Cyan
    & $Command
    if ($LASTEXITCODE -ne 0) {
        throw "$Description failed (exit code $LASTEXITCODE)"
    }
}

$commit = (git rev-parse --short HEAD).Trim()
if ($LASTEXITCODE -ne 0) {
    throw 'git rev-parse failed'
}
$version = '{0}-{1}' -f (Get-Date -Format 'yyyy-MM-dd-HHmm'), $commit
$dirty = git status --porcelain -- .
if ($dirty) {
    Write-Warning 'Uncommitted changes: the release contains code that is not in any commit.'
    $version = "$version-dirty"
}

$releaseDir = Join-Path $root "release/$version"
if (Test-Path $releaseDir) {
    Remove-Item $releaseDir -Recurse -Force
}
New-Item -ItemType Directory -Force $releaseDir | Out-Null
Write-Host "Release $version -> $releaseDir"

# Same platform as the VPS.
$images = @(
    @{ Name = 'api'; Context = 'api/Aed'; Dockerfile = 'api/Aed/Api/Dockerfile' },
    @{ Name = 'web'; Context = 'web';     Dockerfile = 'web/Dockerfile' }
)
$tags = @()
foreach ($image in $images) {
    $tag = "aed/$($image.Name):$version"
    $tags += $tag
    Invoke-Native "docker build $tag" {
        docker build --platform linux/amd64 -t $tag -f $image.Dockerfile $image.Context
    }
}

$tarPath = Join-Path $releaseDir 'images.tar'
Invoke-Native 'docker save' { docker save -o $tarPath $tags }

# docker load reads a gzip-compressed tar directly.
Write-Host '==> Compressing images.tar.gz' -ForegroundColor Cyan
$source = [System.IO.File]::OpenRead($tarPath)
try {
    $target = [System.IO.File]::Create("$tarPath.gz")
    try {
        $gzip = New-Object System.IO.Compression.GZipStream($target, [System.IO.Compression.CompressionLevel]::Optimal)
        try {
            $source.CopyTo($gzip)
        }
        finally {
            $gzip.Dispose()
        }
    }
    finally {
        $target.Dispose()
    }
}
finally {
    $source.Dispose()
}
Remove-Item $tarPath

$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText((Join-Path $releaseDir 'version.txt'), "$version`n", $utf8NoBom)

$size = [math]::Round((Get-Item "$tarPath.gz").Length / 1MB)
Write-Host ''
Write-Host "Release $version is ready ($size MB):" -ForegroundColor Green
Write-Host "  WinSCP $tarPath.gz  ->  /home/ubuntu/deploy-in/"
Write-Host '  VPS:   cd ~/hacknation-2026'
Write-Host '         ./deploy/scripts/deploy.sh app load ~/deploy-in/images.tar.gz && ./deploy/scripts/deploy.sh app up'
Write-Host '         (deploy/WDROZENIE.md)'
