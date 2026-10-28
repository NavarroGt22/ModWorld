param([string]$LogFile = '', [int]$Tail = 180)
$ErrorActionPreference='Stop'
if ([string]::IsNullOrWhiteSpace($LogFile)) {
    $low = Join-Path $env:USERPROFILE 'AppData\LocalLow'
    $candidates = @(
      (Join-Path $low 'mkarpenko\worldbox\Player.log'),
      (Join-Path $low 'mkarpenko\WorldBox\Player.log'),
      (Join-Path $low 'Maxim Karpenko\WorldBox\Player.log')
    )
    $LogFile = $candidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
    if ([string]::IsNullOrWhiteSpace($LogFile)) {
       $found = Get-ChildItem -LiteralPath $low -Filter 'Player.log' -File -Recurse -ErrorAction SilentlyContinue |
          Where-Object { $_.FullName -match 'worldbox' } | Select-Object -First 1
       if ($found) { $LogFile = $found.FullName }
    }
}
if ([string]::IsNullOrWhiteSpace($LogFile) -or -not (Test-Path -LiteralPath $LogFile)) {
    throw 'Player.log nao encontrado. Passe -LogFile "C:\caminho\Player.log".'
}
Write-Host "Lendo: $LogFile"
$lines = @(Get-Content -LiteralPath $LogFile -Tail $Tail)
$matches = @($lines | Where-Object { $_ -match 'AMA|ArcaneMartial|error CS[0-9]+|Failed to compile|Exception|[Ee]rror' })
if ($matches.Count -gt 0) { $matches | ForEach-Object { Write-Host $_ } }
else { Write-Host 'Nenhuma linha suspeita nas linhas finais. Trecho completo abaixo:'; $lines | ForEach-Object { Write-Host $_ } }
Write-Host 'Se faltarem erros antigos, aumente -Tail 500 ou envie os dois logs.'
