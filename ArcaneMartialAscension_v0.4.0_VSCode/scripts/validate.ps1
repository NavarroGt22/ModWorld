param()
$ErrorActionPreference='Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$mod = Join-Path $projectRoot 'ArcaneMartialAscension'
$paths = @('mod.json','Code\Main.cs','Code\Spells\SpellCatalog.cs',
 'Code\GameIntegration\ActorAbilities.cs','Code\VisualEffects\ScreenEffects.cs',
 'Config\catalogo_magias_160.json','Locales\br.json','Locales\en.json')
foreach ($p in $paths) {
    $f = Join-Path $mod $p
    if (-not (Test-Path -LiteralPath $f)) { throw "Falta arquivo: $p" }
}
$manifest = Get-Content (Join-Path $mod 'mod.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$catalog = Get-Content (Join-Path $mod 'Config\catalogo_magias_160.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$null=Get-Content (Join-Path $mod 'Locales\br.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$null=Get-Content (Join-Path $mod 'Locales\en.json') -Raw -Encoding UTF8 | ConvertFrom-Json
if ($manifest.version -ne '0.4.0') { throw 'Esperado mod.json version 0.4.0' }
if (@($catalog.magias).Count -ne 160) { throw "Esperadas 160 magias em JSON" }
$groups = @($catalog.magias | Group-Object escola_id)
if ($groups.Count -ne 8) { throw "Esperadas 8 escolas; encontradas $($groups.Count)" }
foreach ($g in $groups) {
    if ($g.Count -ne 20) { throw "Escola $($g.Name) deve ter 20 magias" }
    foreach ($c in 0..9) {
        $atCircle = @($g.Group | Where-Object { $_.circulo -eq $c })
        if ($atCircle.Count -ne 2) { throw "Escola $($g.Name), circulo $c: esperado 2" }
    }
}
$ids = @($catalog.magias | Select-Object -ExpandProperty id)
if (@($ids | Select-Object -Unique).Count -ne 160) { throw 'Existem IDs de magia repetidos' }
$source = Get-Content (Join-Path $mod 'Code\Spells\SpellCatalog.cs') -Raw -Encoding UTF8
$sourceIds = [regex]::Matches($source, 'Add\(new SpellEntry\("([^"]+)"') |
    ForEach-Object { $_.Groups[1].Value }
if (@($sourceIds).Count -ne 160) { throw "SpellCatalog.cs: encontrado $(@($sourceIds).Count), esperado 160" }
$missing = @($ids | Where-Object { $_ -notin $sourceIds })
if ($missing.Count -gt 0) { throw "Magias no JSON mas ausentes no C#: $($missing -join ', ')" }
$icons = Get-ChildItem (Join-Path $mod 'GameResources\ui\icons\ama') -Filter '*.png' -File
if ($icons.Count -lt 16) { throw 'Icones de escolas/armas faltando' }
Write-Host "OK: manifesto v$($manifest.version), 8 escolas, 160 magias sincronizadas com C#, $($icons.Count) icones."
Write-Host 'ATENCAO: validacao estatica. NAO compila C# nem executa o WorldBox.'
