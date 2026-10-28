param([string]$GamePath = '')
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'common.ps1')
$root = Resolve-WorldBoxRoot -GamePath $GamePath
$env:WORLD_BOX_PATH=$root
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw 'dotnet nao instalado. Instale .NET SDK (8 ou superior) ou use instalacao por fontes sem compilacao local.'
}
$project = Join-Path $PSScriptRoot '..\ArcaneMartialAscension.Dev.csproj'
Write-Host "Compilando contra as DLLs locais em: $root"
& dotnet build $project --configuration Debug --nologo
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host 'Compilacao concluida. Nao copie a DLL .Dev para Mods: o NML carrega as fontes Code/*.cs.'
