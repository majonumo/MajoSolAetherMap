[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$ZipPath,

    [Parameter(Mandatory)]
    [string]$ExpectedVersion,

    [Parameter(Mandatory)]
    [string]$SourceArtworkPath
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem

$requiredEntries = @(
    'MajoSolAetherMap.deps.json',
    'MajoSolAetherMap.dll',
    'MajoSolAetherMap.json',
    'assets/majo-and-sol.png'
)

$archive = [System.IO.Compression.ZipFile]::OpenRead((Resolve-Path -LiteralPath $ZipPath))
try {
    $entries = @{}
    foreach ($entry in $archive.Entries) {
        $entries[$entry.FullName.Replace('\', '/')] = $entry
    }

    foreach ($requiredEntry in $requiredEntries) {
        if (-not $entries.ContainsKey($requiredEntry)) {
            throw "Package is missing $requiredEntry."
        }
    }

    $manifestReader = [System.IO.StreamReader]::new($entries['MajoSolAetherMap.json'].Open())
    try {
        $manifest = $manifestReader.ReadToEnd() | ConvertFrom-Json
    }
    finally {
        $manifestReader.Dispose()
    }

    if ($manifest.InternalName -ne 'MajoSolAetherMap') {
        throw "Unexpected packaged InternalName: $($manifest.InternalName)"
    }

    if ([string]$manifest.AssemblyVersion -ne $ExpectedVersion) {
        throw "Packaged AssemblyVersion $($manifest.AssemblyVersion) does not match $ExpectedVersion."
    }

    if ([int]$manifest.DalamudApiLevel -ne 15) {
        throw "Packaged manifest is not for Dalamud API 15."
    }

    $sha256 = [System.Security.Cryptography.SHA256]::Create()
    try {
        $artworkStream = $entries['assets/majo-and-sol.png'].Open()
        try {
            $packagedArtworkHash = ([BitConverter]::ToString($sha256.ComputeHash($artworkStream))).Replace('-', '')
        }
        finally {
            $artworkStream.Dispose()
        }
    }
    finally {
        $sha256.Dispose()
    }

    $sourceArtworkHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourceArtworkPath).Hash
    if ($packagedArtworkHash -ne $sourceArtworkHash) {
        throw 'Packaged artwork differs from assets/majo-and-sol.png.'
    }
}
finally {
    $archive.Dispose()
}

Write-Host "Validated package $ZipPath for MajoSolAetherMap $ExpectedVersion."
