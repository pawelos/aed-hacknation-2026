<#
.SYNOPSIS
  Generates the wizytowka pages (site/public/**/index.html + sitemap.xml) from site/src.

.DESCRIPTION
  Content lives in site/src:
    <lang>.json         language name, locale, slugs, menu, UI texts, titles, descriptions, topic, faq, og_title,
                        og_image_alt, leads
    <lang>/<page>.html  page body; placeholders: {href:<page>} {asset:hero.svg} {icon:<name>} {app_url} {faq}
  The generated files are committed: the server only serves site/public (no build step there).
  Every page shares the header (logo, menu, theme toggle, login, language picker with search) and footer.
  SEO in <head>: title, description, robots, canonical, hreflang, Open Graph + Twitter card (image from og.ps1)
  and JSON-LD (Organization, WebSite, WebPage, BreadcrumbList, FAQPage). sitemap.xml gets lastmod from git.
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
$ContactEmail = 'kontakt@hacknation-2026.pl'

# Search engine ownership tags (HTML tag method): paste the content value from Google Search Console /
# Bing Webmaster Tools and rebuild. Empty values are skipped. Not needed when the domain is verified by DNS.
$SiteVerification = [ordered]@{
    'google-site-verification' = ''
    'msvalidate.01'            = ''
}

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

# Share image rendered by og.ps1 (absolute URL: crawlers do not resolve relative og:image).
function Get-OgImage([string]$Lang) {
    return "$SiteUrl/assets/og-$Lang.png"
}

# FAQ from <lang>.json as <details> (works without JS); the same texts go to the FAQPage JSON-LD.
function Get-FaqHtml([string]$Lang) {
    $items = foreach ($item in $Langs[$Lang].faq) {
        "<details><summary><h3>$(ConvertTo-HtmlAttribute $item.q)</h3></summary><p>$(ConvertTo-HtmlAttribute $item.a)</p></details>"
    }
    return "<div class=`"faq`">`n      " + ($items -join "`n      ") + "`n    </div>"
}

# Structured data for search engines. A data block, not a script: the CSP script-src 'self' does not apply to it.
function Get-JsonLd([string]$Lang, [string]$Page, [bool]$HasFaq) {
    $L = $Langs[$Lang]
    $url = Get-Canonical (Get-Slug $Lang $Page)
    $siteId = "$SiteUrl/#website"
    $orgId = "$SiteUrl/#organization"

    $webPage = [ordered]@{
        '@type'            = 'WebPage'
        '@id'              = "$url#webpage"
        url                = $url
        name               = [string]$L.titles.$Page
        description        = [string]$L.descriptions.$Page
        inLanguage         = $Lang
        isPartOf           = @{ '@id' = $siteId }
        primaryImageOfPage = [ordered]@{ '@type' = 'ImageObject'; url = (Get-OgImage $Lang); width = 1200; height = 630 }
    }
    if ($HasFaq) {
        $webPage['@type'] = @('WebPage', 'FAQPage')
        $webPage.mainEntity = @(foreach ($item in $L.faq) {
                [ordered]@{
                    '@type'        = 'Question'
                    name           = [string]$item.q
                    acceptedAnswer = [ordered]@{ '@type' = 'Answer'; text = [string]$item.a }
                }
            })
    }
    if ($Page -ne 'home') {
        $webPage.breadcrumb = [ordered]@{
            '@type'         = 'BreadcrumbList'
            itemListElement = @(
                [ordered]@{ '@type' = 'ListItem'; position = 1; name = [string]$L.nav.home; item = (Get-Canonical (Get-Slug $Lang 'home')) },
                [ordered]@{ '@type' = 'ListItem'; position = 2; name = [string]$L.nav.$Page; item = $url }
            )
        }
    }

    $data = [ordered]@{
        '@context' = 'https://schema.org'
        '@graph'   = @(
            [ordered]@{
                '@type' = 'Organization'
                '@id'   = $orgId
                name    = $Brand
                url     = "$SiteUrl/"
                logo    = [ordered]@{ '@type' = 'ImageObject'; url = "$SiteUrl/assets/apple-touch-icon.png"; width = 180; height = 180 }
                email   = $ContactEmail
            },
            [ordered]@{
                '@type'     = 'WebSite'
                '@id'       = $siteId
                url         = "$SiteUrl/"
                name        = $Brand
                description = [string]$L.descriptions.home
                inLanguage  = @($LangCodes)
                about       = [ordered]@{
                    '@type' = 'Thing'
                    name    = [string]$L.topic
                    sameAs  = 'https://en.wikipedia.org/wiki/Automated_external_defibrillator'
                }
                publisher   = @{ '@id' = $orgId }
            },
            $webPage
        )
    }
    # Escape < so the text can never close the <script> element (PowerShell 7 does not escape it by default).
    return (ConvertTo-Json $data -Depth 10 -Compress).Replace('<', '<')
}

# Date of the last commit that changed the page texts; today when they have uncommitted changes or git is missing.
function Get-LastMod([string]$Lang, [string]$Page) {
    $today = (Get-Date).ToString('yyyy-MM-dd')
    if (-not (Get-Command git -ErrorAction SilentlyContinue)) { return $today }
    $files = @("src/$Lang/$Page.html", "src/$Lang.json")
    $dirty = & git -C $Root status --porcelain -- $files
    if ($LASTEXITCODE -ne 0 -or $dirty) { return $today }
    $date = & git -C $Root log -1 --format=%cs -- $files
    if ($LASTEXITCODE -ne 0 -or -not $date) { return $today }
    return [string]$date
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
      <a class="button button-primary button-login" href="$LoginUrl" target="_blank" rel="noopener" aria-label="$($ui.login_label)">$($Icons.login)<span>$($ui.login)</span></a>
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
    $hasFaq = $body.Contains('{faq}')
    $body = $body.Replace('{faq}', (Get-FaqHtml $Lang))
    if ($body -match '\{(href|asset|icon):[^}]*\}|\{app_url\}') {
        throw "Unknown placeholder $($Matches[0]) in src/$Lang/$Page.html"
    }

    $alternates = ($LangCodes | ForEach-Object {
            "  <link rel=`"alternate`" hreflang=`"$_`" href=`"$(Get-Canonical (Get-Slug $_ $Page))`">"
        }) -join "`n"
    $banner = if ($Page -eq 'home') { '' } else { Get-PageBanner $Lang $Page }
    $localeAlternates = ($LangCodes | Where-Object { $_ -ne $Lang } | ForEach-Object {
            "  <meta property=`"og:locale:alternate`" content=`"$($Langs[$_].locale)`">"
        }) -join "`n"
    $title = ConvertTo-HtmlAttribute $L.titles.$Page
    $description = ConvertTo-HtmlAttribute $L.descriptions.$Page
    $ogTitle = if ($Page -eq 'home') { ConvertTo-HtmlAttribute $L.og_title } else { $title }
    $ogImageAlt = ConvertTo-HtmlAttribute $L.og_image_alt
    $verification = ($SiteVerification.Keys | Where-Object { $SiteVerification[$_] } | ForEach-Object {
            "`n  <meta name=`"$_`" content=`"$(ConvertTo-HtmlAttribute $SiteVerification[$_])`">"
        }) -join ''

    return @"
<!doctype html>
<!-- Generated by site/build.ps1 - edit site/src, not here. -->
<html lang="$Lang">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>$title</title>
  <meta name="description" content="$description">
  <meta name="robots" content="index, follow, max-image-preview:large">$verification
  <meta name="theme-color" content="#00843a">
  <link rel="canonical" href="$(Get-Canonical $slug)">
$alternates
  <link rel="alternate" hreflang="x-default" href="$(Get-Canonical (Get-Slug $LangCodes[0] $Page))">
  <link rel="icon" href="$(Get-Asset $slug 'assets/favicon.svg')" type="image/svg+xml">
  <link rel="apple-touch-icon" href="$(Get-Asset $slug 'assets/apple-touch-icon.png')">
  <meta property="og:type" content="website">
  <meta property="og:locale" content="$($L.locale)">
$localeAlternates
  <meta property="og:site_name" content="$Brand">
  <meta property="og:title" content="$ogTitle">
  <meta property="og:description" content="$description">
  <meta property="og:url" content="$(Get-Canonical $slug)">
  <meta property="og:image" content="$(Get-OgImage $Lang)">
  <meta property="og:image:type" content="image/png">
  <meta property="og:image:width" content="1200">
  <meta property="og:image:height" content="630">
  <meta property="og:image:alt" content="$ogImageAlt">
  <meta name="twitter:card" content="summary_large_image">
  <script type="application/ld+json">$(Get-JsonLd $Lang $Page $hasFaq)</script>
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
            $urls += "  <url>`n    <loc>$(Get-Canonical (Get-Slug $lang $page))</loc>`n    <lastmod>$(Get-LastMod $lang $page)</lastmod>`n$links`n  </url>"
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
