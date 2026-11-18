[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$ManifestPath,

    [Parameter(Mandatory)]
    [string]$VersionTag,

    [Parameter(Mandatory)]
    [long]$LastUpdateEpoch,

    [Parameter(Mandatory)]
    [string]$OutputPath
)

$ErrorActionPreference = 'Stop'

$manifest = Get-Content -LiteralPath $ManifestPath -Raw -Encoding UTF8 | ConvertFrom-Json
if ($manifest.InternalName -ne 'MajoSolAetherMap') {
    throw "Unexpected InternalName: $($manifest.InternalName)"
}

if ([int]$manifest.DalamudApiLevel -ne 15) {
    throw "Expected Dalamud API 15, found $($manifest.DalamudApiLevel)."
}

$tagVersion = $VersionTag.TrimStart('v')
$assemblyVersion = [string]$manifest.AssemblyVersion
$shortAssemblyVersion = $assemblyVersion -replace '\.0$', ''
if ($tagVersion -ne $assemblyVersion -and $tagVersion -ne $shortAssemblyVersion) {
    throw "Tag $VersionTag does not match AssemblyVersion $assemblyVersion."
}

$downloadUrl = "https://github.com/majonumo/MajoSolAetherMap/releases/download/$VersionTag/latest.zip"
$remoteManifest = [ordered]@{
    Author                 = $manifest.Author
    Name                   = $manifest.Name
    InternalName           = $manifest.InternalName
    AssemblyVersion        = $assemblyVersion
    Description            = $manifest.Description
    ApplicableVersion      = $manifest.ApplicableVersion
    RepoUrl                = $manifest.RepoUrl
    Tags                   = @($manifest.Tags)
    CategoryTags           = @($manifest.CategoryTags)
    DalamudApiLevel        = [int]$manifest.DalamudApiLevel
    LoadRequiredState      = [int]$manifest.LoadRequiredState
    LoadSync               = [bool]$manifest.LoadSync
    CanUnloadAsync         = [bool]$manifest.CanUnloadAsync
    LoadPriority           = [int]$manifest.LoadPriority
    Punchline              = $manifest.Punchline
    AcceptsFeedback        = [bool]$manifest.AcceptsFeedback
    FeedbackMessage        = $manifest.FeedbackMessage
    IsHide                 = $false
    IsTestingExclusive     = $false
    DownloadCount          = 0
    LastUpdate             = [long]$LastUpdateEpoch
    DownloadLinkInstall    = $downloadUrl
    DownloadLinkUpdate     = $downloadUrl
    DownloadLinkTesting    = $downloadUrl
}

$outputDirectory = Split-Path -Parent $OutputPath
if ($outputDirectory -and -not (Test-Path -LiteralPath $outputDirectory)) {
    New-Item -ItemType Directory -Path $outputDirectory | Out-Null
}

ConvertTo-Json -InputObject @([pscustomobject]$remoteManifest) -Depth 10 |
    Set-Content -LiteralPath $OutputPath -Encoding UTF8
