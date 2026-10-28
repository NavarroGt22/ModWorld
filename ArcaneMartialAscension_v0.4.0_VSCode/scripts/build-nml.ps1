param([string]$GamePath = '')
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'common.ps1')
$root = Resolve-WorldBoxRoot -GamePath $GamePath
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$managed = Join-Path $root 'worldbox_Data\Managed'
$mods = Join-Path $root 'worldbox_Data\StreamingAssets\mods'
$libraries = Join-Path $mods 'NML\Assemblies'
$gameAssembly = Join-Path $mods 'NML\Assembly-CSharp-Publicized.dll'
if (-not (Test-Path -LiteralPath $gameAssembly)) {
    $gameAssembly = Join-Path $managed 'Assembly-CSharp.dll'
}
$loader = Join-Path $mods 'NeoModLoader_memload.dll'
if (-not (Test-Path -LiteralPath $loader)) { $loader = Join-Path $mods 'NeoModLoader.dll' }
foreach ($required in @($gameAssembly, $loader, (Join-Path $libraries 'Microsoft.CodeAnalysis.CSharp.dll'))) {
    if (-not (Test-Path -LiteralPath $required -PathType Leaf)) { throw "DLL local ausente: $required" }
}

# Load compiler dependencies from the user's installation, without copying them.
$resolveLibrary = [ResolveEventHandler] {
    param($sender, $eventArgs)
    $simpleName = (New-Object Reflection.AssemblyName $eventArgs.Name).Name
    $candidate = Join-Path $libraries ($simpleName + '.dll')
    if (Test-Path -LiteralPath $candidate) { return [Reflection.Assembly]::LoadFrom($candidate) }
    return $null
}.GetNewClosure()
[AppDomain]::CurrentDomain.add_AssemblyResolve($resolveLibrary)
$compilerDependencies = @(
    (Join-Path $managed 'netstandard.dll'),
    (Join-Path $libraries 'System.Runtime.CompilerServices.Unsafe.dll'),
    (Join-Path $libraries 'System.Buffers.dll'),
    (Join-Path $libraries 'System.Memory.dll'),
    (Join-Path $libraries 'System.Threading.Tasks.Extensions.dll'),
    (Join-Path $libraries 'System.Collections.Immutable.dll'),
    (Join-Path $libraries 'System.Reflection.Metadata.dll'),
    (Join-Path $libraries 'Microsoft.CodeAnalysis.dll'),
    (Join-Path $libraries 'Microsoft.CodeAnalysis.CSharp.dll')
)
foreach ($dependency in $compilerDependencies) {
    if (Test-Path -LiteralPath $dependency) { [Reflection.Assembly]::LoadFrom($dependency) | Out-Null }
}
Write-Host "Compilador NML: $([Microsoft.CodeAnalysis.CSharp.CSharpCompilation].Assembly.FullName)"
Write-Host "Assembly do jogo: $gameAssembly"

$trees = New-Object 'System.Collections.Generic.List[Microsoft.CodeAnalysis.SyntaxTree]'
$options = [Microsoft.CodeAnalysis.CSharp.CSharpParseOptions]::Default.WithLanguageVersion(
    [Microsoft.CodeAnalysis.CSharp.LanguageVersion]::Latest)
$sources = @(Get-ChildItem -LiteralPath (Join-Path $projectRoot 'ArcaneMartialAscension\Code') -Filter '*.cs' -Recurse -File | Sort-Object FullName)
foreach ($source in $sources) {
    $trees.Add([Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText(
        [IO.File]::ReadAllText($source.FullName), $options, $source.FullName,
        [Text.Encoding]::UTF8, [Threading.CancellationToken]::None))
}
$references = New-Object 'System.Collections.Generic.List[Microsoft.CodeAnalysis.MetadataReference]'
$paths = @($gameAssembly, $loader) + @(
    Get-ChildItem -LiteralPath $managed, $libraries -Filter '*.dll' -File |
        Where-Object { $_.Name -ne 'Assembly-CSharp.dll' } | Select-Object -ExpandProperty FullName)
$seen = @{}
foreach ($path in $paths) {
    $name = [IO.Path]::GetFileNameWithoutExtension($path)
    if ($name -eq 'Assembly-CSharp-Publicized') { $name = 'Assembly-CSharp' }
    if ($seen.ContainsKey($name)) { continue }
    $seen[$name] = $true
    $references.Add([Microsoft.CodeAnalysis.MetadataReference]::CreateFromFile(
        $path, [Microsoft.CodeAnalysis.MetadataReferenceProperties]::Assembly, $null))
}

# Roslyn's options constructor uses optional arguments that PowerShell cannot omit.
$constructor = [Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions].GetConstructors() |
    Where-Object { @($_.GetParameters() | Select-Object -Skip 1 | Where-Object { -not $_.IsOptional }).Count -eq 0 } |
    Select-Object -First 1
if ($null -eq $constructor) { throw 'Construtor de opcoes Roslyn nao reconhecido.' }
$parameters = $constructor.GetParameters()
$arguments = New-Object 'object[]' $parameters.Length
for ($i = 0; $i -lt $parameters.Length; $i++) { $arguments[$i] = $parameters[$i].DefaultValue }
$arguments[0] = [Microsoft.CodeAnalysis.OutputKind]::DynamicallyLinkedLibrary
$compilationOptions = $constructor.Invoke($arguments)
$compilationOptions = $compilationOptions.WithConcurrentBuild($false)
$compilation = [Microsoft.CodeAnalysis.CSharp.CSharpCompilation]::Create(
    'ArcaneMartialAscension.Dev', $trees, $references, $compilationOptions)

$outputFolder = Join-Path $projectRoot 'bin\Debug\nml'
New-Item -ItemType Directory -Path $outputFolder -Force | Out-Null
$outputFile = Join-Path $outputFolder 'ArcaneMartialAscension.Dev.dll'
$stream = [IO.File]::Create($outputFile)
try {
    $result = $compilation.Emit($stream, $null, $null, $null, $null, $null, [Threading.CancellationToken]::None)
} finally { $stream.Dispose() }
$diagnostics = @($result.Diagnostics | Where-Object { $_.Severity -eq 'Error' -or $_.Severity -eq 'Warning' })
foreach ($diagnostic in $diagnostics) { Write-Host $diagnostic.ToString() }
if (-not $result.Success) {
    Remove-Item -LiteralPath $outputFile -Force
    exit 1
}
Write-Host "OK: $($sources.Count) arquivos C#, $($diagnostics.Count) avisos. Saida apenas para desenvolvimento: $outputFile"
Write-Host 'Esta compilacao nao executa o WorldBox nem confirma efeitos no jogo.'
