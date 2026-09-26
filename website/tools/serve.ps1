<#
.SYNOPSIS
    Local preview server for the EasyAkuru website (no Node.js or build step needed).

.DESCRIPTION
    Serves the website/ folder on http://localhost:<Port>/ the way Cloudflare Pages does,
    closely enough for previewing:
      * directories are served from their index.html (a directory without a trailing
        slash is redirected to the slash form);
      * static rules from website/_redirects are applied (exact paths and a trailing
        "*" splat with :splat in the destination);
      * headers from website/_headers are added (so the Content-Security-Policy is
        enforced locally too - inline styles/scripts will be blocked, as in production);
      * missing files get the nearest 404.html walking up the path, with status 404.
    Press Ctrl+C to stop.

.PARAMETER Port
    TCP port to listen on (default 8788).

.PARAMETER Root
    Folder to serve (default: the website/ folder that contains this script's folder).

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File website\tools\serve.ps1
    Then open http://localhost:8788/easyakuru/
#>
[CmdletBinding()]
param(
    [ValidateRange(1, 65535)]
    [int]$Port = 8788,
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$Root = [IO.Path]::GetFullPath($Root).TrimEnd('\', '/')
if (-not (Test-Path -LiteralPath $Root -PathType Container)) {
    throw "Root folder not found: $Root"
}

$MimeTypes = @{
    '.html' = 'text/html; charset=utf-8'
    '.htm' = 'text/html; charset=utf-8'
    '.css' = 'text/css; charset=utf-8'
    '.js' = 'text/javascript; charset=utf-8'
    '.mjs' = 'text/javascript; charset=utf-8'
    '.json' = 'application/json; charset=utf-8'
    '.webmanifest' = 'application/manifest+json; charset=utf-8'
    '.txt' = 'text/plain; charset=utf-8'
    '.xml' = 'application/xml; charset=utf-8'
    '.svg' = 'image/svg+xml'
    '.png' = 'image/png'
    '.jpg' = 'image/jpeg'
    '.jpeg' = 'image/jpeg'
    '.gif' = 'image/gif'
    '.webp' = 'image/webp'
    '.avif' = 'image/avif'
    '.ico' = 'image/x-icon'
    '.woff' = 'font/woff'
    '.woff2' = 'font/woff2'
    '.ttf' = 'font/ttf'
    '.otf' = 'font/otf'
    '.pdf' = 'application/pdf'
}

function Get-MimeType([string]$Path) {
    $ext = [IO.Path]::GetExtension($Path).ToLowerInvariant()
    if ($MimeTypes.ContainsKey($ext)) { return $MimeTypes[$ext] }
    return 'application/octet-stream'
}

# Converts a Pages URL pattern ("/a/*", "/:lang/x") to an anchored regex.
function ConvertTo-PatternRegex([string]$Pattern) {
    $parts = foreach ($token in [regex]::Split($Pattern, '(\*|:[A-Za-z]\w*)')) {
        if ($token -eq '*') { '(?<splat>.*)' }
        elseif ($token -match '^:[A-Za-z]\w*$') { '(?<' + $token.Substring(1) + '>[^/]+)' }
        else { [regex]::Escape($token) }
    }
    return [regex]('^' + ($parts -join '') + '$')
}

function Read-Redirects([string]$File) {
    $rules = New-Object System.Collections.Generic.List[object]
    if (-not (Test-Path -LiteralPath $File)) { return , $rules }
    foreach ($line in [IO.File]::ReadAllLines($File)) {
        $trimmed = $line.Trim()
        if ($trimmed -eq '' -or $trimmed.StartsWith('#')) { continue }
        $fields = $trimmed -split '\s+'
        if ($fields.Count -lt 2) { continue }
        $status = 302
        if ($fields.Count -ge 3) { $status = [int]$fields[2] }
        $rules.Add([pscustomobject]@{ Regex = ConvertTo-PatternRegex $fields[0]; To = $fields[1]; Status = $status })
    }
    return , $rules
}

function Read-Headers([string]$File) {
    $rules = New-Object System.Collections.Generic.List[object]
    if (-not (Test-Path -LiteralPath $File)) { return , $rules }
    $current = $null
    foreach ($line in [IO.File]::ReadAllLines($File)) {
        if ($line.Trim() -eq '' -or $line.Trim().StartsWith('#')) { continue }
        if ($line -match '^\S') {
            $current = [pscustomobject]@{ Regex = ConvertTo-PatternRegex $line.Trim(); Headers = New-Object System.Collections.Generic.List[object] }
            $rules.Add($current)
        }
        elseif ($null -ne $current) {
            $h = $line.Trim()
            if ($h.StartsWith('! ')) {
                $current.Headers.Add([pscustomobject]@{ Name = $h.Substring(2).Trim(); Value = $null })
            }
            else {
                $i = $h.IndexOf(':')
                if ($i -gt 0) {
                    $current.Headers.Add([pscustomobject]@{ Name = $h.Substring(0, $i).Trim(); Value = $h.Substring($i + 1).Trim() })
                }
            }
        }
    }
    return , $rules
}

function Add-SiteHeaders($Response, [string]$Path, $HeaderRules) {
    $values = [ordered]@{}
    foreach ($rule in $HeaderRules) {
        if (-not $rule.Regex.IsMatch($Path)) { continue }
        foreach ($h in $rule.Headers) {
            $key = $h.Name.ToLowerInvariant()
            if ($null -eq $h.Value) { $values.Remove($key); continue }
            if ($values.Contains($key)) { $values[$key].Value += ', ' + $h.Value }
            else { $values[$key] = [pscustomobject]@{ Name = $h.Name; Value = $h.Value } }
        }
    }
    foreach ($entry in $values.Values) { $Response.Headers[$entry.Name] = $entry.Value }
}

function Resolve-SitePath([string]$UrlPath) {
    # Returns the full file-system path for a URL path, or $null if it escapes the root.
    $relative = [Uri]::UnescapeDataString($UrlPath).TrimStart('/').Replace('/', '\')
    if ($relative.Contains(':') -or $relative.StartsWith('\')) { return $null }
    $full = [IO.Path]::GetFullPath([IO.Path]::Combine($Root, $relative))
    if ($full -ne $Root -and -not $full.StartsWith($Root + '\', [StringComparison]::OrdinalIgnoreCase)) { return $null }
    return $full
}

function Find-NotFoundPage([string]$UrlPath) {
    $dir = $UrlPath.Substring(0, $UrlPath.LastIndexOf('/') + 1)
    while ($true) {
        $candidate = Resolve-SitePath ($dir + '404.html')
        if ($null -ne $candidate -and (Test-Path -LiteralPath $candidate -PathType Leaf)) { return $candidate }
        if ($dir -eq '/') { return $null }
        $dir = $dir.Substring(0, $dir.TrimEnd('/').LastIndexOf('/') + 1)
    }
}

function Send-Bytes($Response, [int]$Status, [string]$ContentType, [byte[]]$Bytes, [bool]$HeadOnly) {
    $Response.StatusCode = $Status
    $Response.ContentType = $ContentType
    $Response.ContentLength64 = $Bytes.Length
    if (-not $HeadOnly) { $Response.OutputStream.Write($Bytes, 0, $Bytes.Length) }
}

function Send-Redirect($Response, [int]$Status, [string]$Location) {
    $Response.StatusCode = $Status
    $Response.RedirectLocation = $Location
    $Response.ContentLength64 = 0
}

function Invoke-Request($Context, $RedirectRules, $HeaderRules) {
    $request = $Context.Request
    $response = $Context.Response
    $path = $request.Url.AbsolutePath
    $query = $request.Url.Query
    $headOnly = $request.HttpMethod -eq 'HEAD'

    try {
        if ($request.HttpMethod -ne 'GET' -and -not $headOnly) {
            $response.Headers['Allow'] = 'GET, HEAD'
            Send-Bytes $response 405 'text/plain; charset=utf-8' ([Text.Encoding]::UTF8.GetBytes('405 Method Not Allowed')) $false
            return
        }

        Add-SiteHeaders $response $path $HeaderRules

        foreach ($rule in $RedirectRules) {
            $m = $rule.Regex.Match($path)
            if (-not $m.Success) { continue }
            $to = $rule.To
            if ($m.Groups['splat'].Success) { $to = $to.Replace(':splat', $m.Groups['splat'].Value) }
            if ($to -notmatch '\?' -and $query) { $to += $query }
            Send-Redirect $response $rule.Status $to
            return
        }

        $file = Resolve-SitePath $path
        if ($null -ne $file -and (Test-Path -LiteralPath $file -PathType Container)) {
            if (-not $path.EndsWith('/')) {
                Send-Redirect $response 308 ($path + '/' + $query)
                return
            }
            $file = Join-Path $file 'index.html'
        }

        # Never serve the Pages config files themselves.
        $leaf = if ($null -ne $file) { [IO.Path]::GetFileName($file) } else { '' }
        if ($null -ne $file -and $leaf -notin @('_redirects', '_headers') -and (Test-Path -LiteralPath $file -PathType Leaf)) {
            Send-Bytes $response 200 (Get-MimeType $file) ([IO.File]::ReadAllBytes($file)) $headOnly
            return
        }

        $notFound = Find-NotFoundPage $path
        if ($null -ne $notFound) {
            Send-Bytes $response 404 'text/html; charset=utf-8' ([IO.File]::ReadAllBytes($notFound)) $headOnly
        }
        else {
            Send-Bytes $response 404 'text/plain; charset=utf-8' ([Text.Encoding]::UTF8.GetBytes('404 Not Found')) $headOnly
        }
    }
    catch {
        Write-Warning "Error serving ${path}: $_"
        try { $response.StatusCode = 500 } catch { }
    }
    finally {
        Write-Host ('{0} {1} {2}' -f $response.StatusCode, $request.HttpMethod, $request.Url.PathAndQuery)
        $response.Close()
    }
}

$redirectRules = Read-Redirects (Join-Path $Root '_redirects')
$headerRules = Read-Headers (Join-Path $Root '_headers')

$listener = New-Object System.Net.HttpListener
$listener.Prefixes.Add("http://localhost:$Port/")
$listener.Start()
Write-Host "Serving $Root"
Write-Host "Open http://localhost:$Port/easyakuru/  (Ctrl+C to stop)"

try {
    while ($listener.IsListening) {
        # Poll with a timeout instead of blocking in GetContext() so Ctrl+C is honoured.
        $pending = $listener.BeginGetContext($null, $null)
        while (-not $pending.AsyncWaitHandle.WaitOne(250)) { }
        Invoke-Request ($listener.EndGetContext($pending)) $redirectRules $headerRules
    }
}
finally {
    $listener.Stop()
    $listener.Close()
    Write-Host 'Stopped.'
}
