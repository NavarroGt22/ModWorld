function Resolve-WorldBoxRoot {
    param([string]$GamePath = '')
    $steam = Join-Path ([Environment]::GetEnvironmentVariable('ProgramFiles(x86)')) 'Steam\steamapps\common\worldbox'
    $candidates = @($GamePath, $env:WORLD_BOX_PATH, $steam,
       'C:\Program Files (x86)\Steam\steamapps\common\worldbox',
       'C:\Program Files\Steam\steamapps\common\worldbox')
    foreach ($candidate in $candidates) {
        if ([string]::IsNullOrWhiteSpace($candidate)) { continue }
        $root = $candidate.Trim().Trim('"')
        if ((Test-Path -LiteralPath (Join-Path $root 'worldbox_Data\Managed') -PathType Container)) {
            return (Resolve-Path -LiteralPath $root).Path
        }
    }
    throw "WorldBox nao encontrado. Execute o script com -GamePath 'D:\\SteamLibrary\\steamapps\\common\\worldbox' ou configure WORLD_BOX_PATH."
}
function Resolve-ModsFolder {
    param([string]$Root)
    $assets = Join-Path $Root 'worldbox_Data\StreamingAssets'
    if (-not (Test-Path -LiteralPath $assets -PathType Container)) {
        throw "Pasta StreamingAssets nao encontrada: $assets"
    }
    $found = Get-ChildItem -LiteralPath $assets -Directory |
        Where-Object { $_.Name -ieq 'mods' } | Select-Object -First 1
    if ($null -ne $found) { return $found.FullName }
    $newFolder = Join-Path $assets 'mods'
    New-Item -ItemType Directory -Path $newFolder -Force | Out-Null
    return $newFolder
}
