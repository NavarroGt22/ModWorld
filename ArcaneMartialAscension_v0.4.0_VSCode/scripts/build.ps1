param([string]$GamePath = '')
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'common.ps1')
$root = Resolve-WorldBoxRoot -GamePath $GamePath
$env:WORLD_BOX_PATH=$root
$project = Join-Path $PSScriptRoot '..\ArcaneMartialAscension.Dev.csproj'
Write-Host "Compilando contra as DLLs locais em: $root"
$sdkAvailable = $false
if (Get-Command dotnet -ErrorAction SilentlyContinue) {
    $sdks = @(& dotnet --list-sdks)
    $sdkAvailable = $LASTEXITCODE -eq 0 -and $sdks.Count -gt 0
}
if ($sdkAvailable) {
    & dotnet build $project --configuration Debug --nologo
} else {
    # Use the installed loader's compiler in an isolated .NET Framework process.
    $windowsPowerShell = Join-Path $env:SystemRoot 'System32\WindowsPowerShell\v1.0\powershell.exe'
    Write-Host 'SDK ausente; usando Roslyn do NeoModLoader (Windows PowerShell).'
    & $windowsPowerShell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'build-nml.ps1') -GamePath $root
}
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host 'Compilacao concluida. Nao copie a DLL .Dev para Mods: o NML carrega as fontes Code/*.cs.'
