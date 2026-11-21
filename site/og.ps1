<#
.SYNOPSIS
  Renders the share images (public/assets/og-pl.png, og-en.png, 1200x630) and public/assets/apple-touch-icon.png
  (180x180) from the templates in site/src/og with headless Microsoft Edge.

.DESCRIPTION
  Run it only after changing src/og/*.html, logo.svg or hero.svg; the PNGs are committed like the pages.
  ASCII only, like build.ps1.

.EXAMPLE
  powershell -NoProfile -ExecutionPolicy Bypass -File site/og.ps1
#>

Set-StrictMode -Version 2.0
$ErrorActionPreference = 'Stop'

$Root = $PSScriptRoot
$OgSrc = Join-Path $Root 'src/og'
$Assets = Join-Path $Root 'public/assets'

$Edge = @(
    "${env:ProgramFiles(x86)}\Microsoft\Edge\Application\msedge.exe",
    "$env:ProgramFiles\Microsoft\Edge\Application\msedge.exe"
) | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $Edge) { throw 'Microsoft Edge not found.' }

function Invoke-Screenshot([string]$Template, [string]$Query, [string]$Out, [int]$Width, [int]$Height) {
    $url = ([System.Uri](Join-Path $OgSrc $Template)).AbsoluteUri + $Query
    $profileDir = Join-Path ([System.IO.Path]::GetTempPath()) ('og-edge-' + [guid]::NewGuid())
    $edgeArgs = @('--headless=new', '--disable-gpu', '--hide-scrollbars', '--force-device-scale-factor=1',
        '--allow-file-access-from-files', "--user-data-dir=$profileDir",
        "--window-size=$Width,$Height", "--screenshot=$Out", $url)
    $p = Start-Process -FilePath $Edge -ArgumentList $edgeArgs -Wait -PassThru -WindowStyle Hidden
    Remove-Item -Recurse -Force $profileDir -ErrorAction SilentlyContinue
    if ($p.ExitCode -ne 0 -or -not (Test-Path $Out)) { throw "Edge failed to render $Template$Query" }
    Write-Output "public/assets/$(Split-Path -Leaf $Out)"
}

Invoke-Screenshot 'card.html' '?lang=pl' (Join-Path $Assets 'og-pl.png') 1200 630
Invoke-Screenshot 'card.html' '?lang=en' (Join-Path $Assets 'og-en.png') 1200 630
Invoke-Screenshot 'icon.html' '' (Join-Path $Assets 'apple-touch-icon.png') 180 180
