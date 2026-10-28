param([string]$GamePath = '', [switch]$NoBackup)
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'common.ps1')
$root = Resolve-WorldBoxRoot -GamePath $GamePath
if (Get-Process -Name worldbox -ErrorAction SilentlyContinue) {
    throw 'Feche o WorldBox antes de instalar para evitar trocar fontes durante o carregamento.'
}
$mods = Resolve-ModsFolder -Root $root
$source = Join-Path $PSScriptRoot '..\ArcaneMartialAscension'
$destination = [IO.Path]::GetFullPath((Join-Path $mods 'ArcaneMartialAscension'))
$resolvedMods = [IO.Path]::GetFullPath($mods).TrimEnd('\')
if ([IO.Path]::GetDirectoryName($destination) -ine $resolvedMods -or
    [IO.Path]::GetFileName($destination) -ne 'ArcaneMartialAscension' -or
    $destination -ieq [IO.Path]::GetFullPath($source)) {
    throw "Destino de instalacao inseguro: $destination"
}
if (-not (Test-Path -LiteralPath (Join-Path $source 'mod.json'))) { throw 'Arquivo mod.json ausente na origem' }
Write-Host "Destino: $destination"
if (Test-Path -LiteralPath $destination) {
   if (-not $NoBackup) {
      $backupRoot = Join-Path $root 'AMA_Backups'
      New-Item -ItemType Directory -Path $backupRoot -Force | Out-Null
      $timestamp = Get-Date -Format 'yyyyMMdd_HHmmss'
      $backup = Join-Path $backupRoot "ArcaneMartialAscension_$timestamp"
      Copy-Item -LiteralPath $destination -Destination $backup -Recurse -Force
      Write-Host "Backup fora de Mods: $backup"
   }
   Remove-Item -LiteralPath $destination -Recurse -Force
}
Copy-Item -LiteralPath $source -Destination $destination -Recurse -Force
if (-not (Test-Path -LiteralPath (Join-Path $destination 'mod.json'))) { throw 'Instalacao falhou: mod.json nao encontrado' }
Write-Host 'Instalado. Feche o jogo antes de atualizar e reabra-o para o NML recompilar as fontes.'
