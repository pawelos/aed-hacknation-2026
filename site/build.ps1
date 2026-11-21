<#
.SYNOPSIS
  Generates the wizytowka pages (site/public/**/index.html + sitemap.xml) from site/src.

.DESCRIPTION
  Content lives in site/src:
    <lang>.json         language name, locale, slugs, menu, UI texts, titles, description, og_title, leads
    <lang>/<page>.html  page body; placeholders: {href:<page>} {asset:hero.svg} {icon:<name>} {app_url}
  The generated files are committed: the server only serves site/public (no build step there).
  Every page shares the header (logo, menu, theme toggle, login, language picker with search) and footer.
  Links and assets are relative, so the pages also work opened from disk (file://).

  This file is ASCII only on purpose: Windows PowerShell 5.1 reads a .ps1 without BOM as ANSI, so Polish
  texts stay in the UTF-8 files under site/src, read and written explicitly as UTF-8 (no BOM, LF).

.EXAMPLE
  powershell -NoProfile -ExecutionPolicy Bypass -File site/build.ps1
  pwsh site/build.ps1
#>

Set-StrictMode -Version 2.0
$ErrorActionPreference = 'Stop'

$Root = $PSScriptRoot
$Src = Join-Path $Root 'src'
$Public = Join-Path $Root 'public'

$SiteUrl = 'https://hacknation-2026.pl'
$AppUrl = 'https://aed.hacknation-2026.pl'
$LoginUrl = "$AppUrl/sign-in"
$Brand = 'AED Radar'

# Page keys in menu order; languages in picker order (the first one is x-default).
$Pages = @('home', 'solution', 'audience', 'materials', 'team', 'contact')
$LangCodes = @('pl', 'en')

$Icons = @{
    sun      = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" aria-hidden="true"><circle cx="12" cy="12" r="4"/><path d="M12 2v2M12 20v2M4.9 4.9l1.4 1.4M17.7 17.7l1.4 1.4M2 12h2M20 12h2M4.9 19.1l1.4-1.4M17.7 6.3l1.4-1.4"/></svg>'
    moon     = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M20 14.5A8 8 0 0 1 9.5 4a8 8 0 1 0 10.5 10.5Z"/></svg>'
    menu     = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" aria-hidden="true"><path d="M4 7h16M4 12h16M4 17h16"/></svg>'
    chevron  = '<svg class="chevron" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="m6 9 6 6 6-6"/></svg>'
    search   = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><circle cx="11" cy="11" r="7"/><path d="m20 20-3.5-3.5"/></svg>'
    tick     = '<svg class="tick" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M20 6 9 17l-5-5"/></svg>'
    login    = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M15 3h4a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2h-4"/><path d="m10 17 5-5-5-5"/><path d="M15 12H3"/></svg>'
    pin      = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M12 21s-7-6.2-7-11.5A7 7 0 0 1 19 9.5C19 14.8 12 21 12 21Z"/><circle cx="12" cy="9.5" r="2.5"/></svg>'
    upload   = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M12 15V3"/><path d="m7 8 5-5 5 5"/><path d="M5 21h14"/></svg>'
    check    = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M20 6 9 17l-5-5"/></svg>'
    mail     = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><rect x="3" y="5" width="18" height="14" rx="2"/><path d="m3 7 9 6 9-6"/></svg>'
    code     = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="m8 7-5 5 5 5M16 7l5 5-5 5"/></svg>'
    calendar = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><rect x="3" y="5" width="18" height="16" rx="2"/><path d="M16 3v4M8 3v4M3 10h18"/></svg>'
    profile  = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><rect x="3" y="4" width="18" height="16" rx="2"/><circle cx="9" cy="10" r="2"/><path d="M5.5 16a3.5 3.5 0 0 1 7 0M15 9h3M15 13h3"/></svg>'
}

$Flags = @{
    pl = '<svg viewBox="0 0 20 20"><rect width="20" height="10" fill="#ffffff"/><rect y="10" width="20" height="10" fill="#dc143c"/></svg>'
    en = '<svg viewBox="0 0 60 60"><rect width="60" height="60" fill="#012169"/><path d="M0 0l60 60M60 0 0 60" stroke="#fff" stroke-width="12"/><path d="M0 0l60 60M60 0 0 60" stroke="#c8102e" stroke-width="5"/><path d="M30 0v60M0 30h60" stroke="#fff" stroke-width="18"/><path d="M30 0v60M0 30h60" stroke="#c8102e" stroke-width="10"/></svg>'
}

$Utf8 = New-Object System.Text.UTF8Encoding($false)

function Read-Utf8([string]$Path) {
    return [System.IO.File]::ReadAllText($Path, [System.Text.Encoding]::UTF8)
}

function Write-Utf8([string]$Path, [string]$Text) {
    $dir = Split-Path -Parent $Path
    if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }
    # LF only, whatever the line endings of this script in the checkout.
    [System.IO.File]::WriteAllText($Path, ($Text -replace "`r`n", "`n"), $Utf8)
}

# Same as Python html.escape: & < > " ' (WebUtility.HtmlEncode would also encode Polish letters on .NET Framework).
function ConvertTo-HtmlAttribute([string]$Text) {
    return $Text.Replace('&', '&amp;').Replace('<', '&lt;').Replace('>', '&gt;').Replace('"', '&quot;').Replace("'", '&#x27;')
}

$Langs = [ordered]@{}
foreach ($code in $LangCodes) {
    $Langs[$code] = (Read-Utf8 (Join-Path $Src "$code.json")) | ConvertFrom-Json
}

function Get-Slug([string]$Lang, [string]$Page) {
    return [string]$Langs[$Lang].slugs.$Page
}

function Get-Depth([string]$Slug) {
    if ($Slug -eq '') { return 0 }
    return ($Slug.Split('/')).Count
}

# Relative link from one page folder to another page (index.html, so it also works from disk).
function Get-Rel([string]$FromSlug, [string]$ToSlug) {
    $prefix = '../' * (Get-Depth $FromSlug)
    if ($ToSlug -ne '') { return "$prefix$ToSlug/index.html" }
    return "${prefix}index.html"
}

function Get-Asset([string]$FromSlug, [string]$Name) {
    return ('../' * (Get-Depth $FromSlug)) + $Name
}

function Get-Canonical([string]$Slug) {
    if ($Slug -ne '') { return "$SiteUrl/$Slug/" }
    return "$SiteUrl/"
}

function Get-Header([string]$Lang, [string]$Page) {
    $L = $Langs[$Lang]
    $slug = Get-Slug $Lang $Page
    $ui = $L.ui

    $navItems = ''
    foreach ($p in $Pages) {
        $current = if ($p -eq $Page) { ' aria-current="page"' } else { '' }
        $navItems += "<li><a href=`"$(Get-Rel $slug (Get-Slug $Lang $p))`"$current>$($L.nav.$p)</a></li>"
    }

    $langItems = ''
    foreach ($code in $LangCodes) {
        $other = $Langs[$code]
        $current = if ($code -eq $Lang) { ' aria-current="true"' } else { '' }
        $tick = if ($code -eq $Lang) { $Icons.tick } else { '' }
        $dataName = ConvertTo-HtmlAttribute ([string]$other.name).ToLowerInvariant()
        $langItems += "<li data-name=`"$dataName $code`"><a href=`"$(Get-Rel $slug (Get-Slug $code $Page))`" hreflang=`"$code`" lang=`"$code`"$current>" +
            "<span class=`"flag flag-sm`" aria-hidden=`"true`">$($Flags[$code])</span><span>$($other.name)</span>" +
            "$tick</a></li>"
    }

    return @"
<header class="site-header">
  <div class="header-inner">
    <a class="logo" href="$(Get-Rel $slug (Get-Slug $Lang 'home'))" aria-label="$($ui.home_label)">
      <img src="$(Get-Asset $slug 'assets/logo.svg')" alt="" width="40" height="40">
      <span class="logo-text">AED <span>Radar</span></span>
    </a>
    <nav class="main-nav" aria-label="$($ui.main_nav)">
      <ul>$navItems</ul>
    </nav>
    <div class="header-actions">
      <button class="icon-button theme-toggle" type="button" aria-label="$($ui.theme)" title="$($ui.theme)" aria-pressed="false" hidden>
        <span class="icon-sun">$($Icons.sun)</span><span class="icon-moon">$($Icons.moon)</span>
      </button>
      <a class="button button-primary button-login" href="$LoginUrl" aria-label="$($ui.login_label)">$($Icons.login)<span>$($ui.login)</span></a>
      <details class="dropdown lang">
        <summary aria-label="$($ui.lang_label): $($L.name)" title="$($ui.lang_label)">
          <span class="flag" aria-hidden="true">$($Flags[$Lang])</span>
          <span class="lang-name">$($L.name)</span>$($Icons.chevron)
        </summary>
        <div class="dropdown-panel">
          <label class="lang-search" hidden>
            $($Icons.search)
            <input type="search" placeholder="$($ui.search)" aria-label="$($ui.search_label)" autocomplete="off">
          </label>
          <ul class="lang-list">$langItems</ul>
          <p class="lang-empty" hidden>$($ui.no_matches)</p>
        </div>
      </details>
      <details class="dropdown nav-mobile">
        <summary class="icon-button" aria-label="$($ui.menu)" title="$($ui.menu)">$($Icons.menu)</summary>
        <div class="dropdown-panel">
          <ul>$navItems</ul>
        </div>
      </details>
    </div>
  </div>
</header>
"@
}

function Get-PageBanner([string]$Lang, [string]$Page) {
    $L = $Langs[$Lang]
    $slug = Get-Slug $Lang $Page
    $homeLink = Get-Rel $slug (Get-Slug $Lang 'home')
    return @"
<section class="page-banner" aria-labelledby="page-title">
  <div class="container">
    <nav class="breadcrumb" aria-label="$($L.ui.breadcrumb)">
      <ol><li><a href="$homeLink">$($L.nav.home)</a></li><li aria-current="page">$($L.nav.$Page)</li></ol>
    </nav>
    <h1 id="page-title">$($L.nav.$Page)</h1>
    <p class="section-intro">$($L.leads.$Page)</p>
  </div>
</section>
"@
}

function Get-Page([string]$Lang, [string]$Page) {
    $L = $Langs[$Lang]
    $slug = Get-Slug $Lang $Page

    $body = Read-Utf8 (Join-Path $Src "$Lang/$Page.html")
    foreach ($p in $Pages) {
        $body = $body.Replace("{href:$p}", (Get-Rel $slug (Get-Slug $Lang $p)))
    }
    $body = $body.Replace('{asset:hero.svg}', (Get-Asset $slug 'assets/hero.svg'))
    foreach ($name in $Icons.Keys) {
        $body = $body.Replace("{icon:$name}", $Icons[$name])
    }
    $body = $body.Replace('{app_url}', $AppUrl)
    if ($body -match '\{(href|asset|icon):[^}]*\}|\{app_url\}') {
        throw "Unknown placeholder $($Matches[0]) in src/$Lang/$Page.html"
    }

    $alternates = ($LangCodes | ForEach-Object {
            "  <link rel=`"alternate`" hreflang=`"$_`" href=`"$(Get-Canonical (Get-Slug $_ $Page))`">"
        }) -join "`n"
    $banner = if ($Page -eq 'home') { '' } else { Get-PageBanner $Lang $Page }
    $description = ConvertTo-HtmlAttribute $L.description
    $ogTitle = ConvertTo-HtmlAttribute $L.og_title

    return @"
<!doctype html>
<!-- Generated by site/build.ps1 - edit site/src, not here. -->
<html lang="$Lang">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>$($L.titles.$Page)</title>
  <meta name="description" content="$description">
  <meta name="theme-color" content="#00843a">
  <link rel="canonical" href="$(Get-Canonical $slug)">
$alternates
  <link rel="alternate" hreflang="x-default" href="$(Get-Canonical (Get-Slug $LangCodes[0] $Page))">
  <link rel="icon" href="$(Get-Asset $slug 'assets/favicon.svg')" type="image/svg+xml">
  <meta property="og:type" content="website">
  <meta property="og:locale" content="$($L.locale)">
  <meta property="og:site_name" content="$Brand">
  <meta property="og:title" content="$ogTitle">
  <meta property="og:description" content="$description">
  <meta property="og:url" content="$(Get-Canonical $slug)">
  <link rel="stylesheet" href="$(Get-Asset $slug 'styles.css')">
  <script src="$(Get-Asset $slug 'assets/site.js')"></script>
</head>
<body>
  <a class="skip-link" href="#main">$($L.ui.skip)</a>
$(Get-Header $Lang $Page)
  <main id="main">
$banner
$($body.Trim())
  </main>

  <footer class="site-footer">
    <div class="container">
      <p>$($L.ui.footer)</p>
    </div>
  </footer>
</body>
</html>

"@
}

function Get-Sitemap {
    $urls = @()
    foreach ($page in $Pages) {
        foreach ($lang in $LangCodes) {
            $links = ($LangCodes | ForEach-Object {
                    "    <xhtml:link rel=`"alternate`" hreflang=`"$_`" href=`"$(Get-Canonical (Get-Slug $_ $page))`"/>"
                }) -join "`n"
            $urls += "  <url>`n    <loc>$(Get-Canonical (Get-Slug $lang $page))</loc>`n$links`n  </url>"
        }
    }
    return "<?xml version=`"1.0`" encoding=`"UTF-8`"?>`n" +
        "<urlset xmlns=`"http://www.sitemaps.org/schemas/sitemap/0.9`" xmlns:xhtml=`"http://www.w3.org/1999/xhtml`">`n" +
        ($urls -join "`n") + "`n</urlset>`n"
}

foreach ($lang in $LangCodes) {
    foreach ($page in $Pages) {
        $slug = Get-Slug $lang $page
        $rel = if ($slug -ne '') { "$slug/index.html" } else { 'index.html' }
        Write-Utf8 (Join-Path $Public $rel) (Get-Page $lang $page)
        Write-Output "public/$rel"
    }
}
Write-Utf8 (Join-Path $Public 'sitemap.xml') (Get-Sitemap)
Write-Output 'public/sitemap.xml'
