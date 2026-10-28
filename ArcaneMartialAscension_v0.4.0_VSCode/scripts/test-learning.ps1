param([string]$GamePath='')
$ErrorActionPreference='Stop'
if ($PSVersionTable.PSEdition -eq 'Core') {
    $windowsPowerShell=Join-Path $env:SystemRoot 'System32\WindowsPowerShell\v1.0\powershell.exe'
    & $windowsPowerShell -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath -GamePath $GamePath
    exit $LASTEXITCODE
}
# Reuse the installed NML compiler and the same real reference set as the mod build.
. (Join-Path $PSScriptRoot 'build-nml.ps1') -GamePath $GamePath
$testTrees=New-Object 'System.Collections.Generic.List[Microsoft.CodeAnalysis.SyntaxTree]'
foreach($testSource in Get-ChildItem -LiteralPath (Join-Path $projectRoot 'tests') -Filter '*.cs' -File) {
    $testTrees.Add([Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText(
        [IO.File]::ReadAllText($testSource.FullName),$options,$testSource.FullName,[Text.Encoding]::UTF8,[Threading.CancellationToken]::None))
}
$references.Add([Microsoft.CodeAnalysis.MetadataReference]::CreateFromFile(
    $outputFile,[Microsoft.CodeAnalysis.MetadataReferenceProperties]::Assembly,$null))
$testOptions=$compilationOptions.WithOutputKind([Microsoft.CodeAnalysis.OutputKind]::ConsoleApplication)
$testCompilation=[Microsoft.CodeAnalysis.CSharp.CSharpCompilation]::Create(
    'AMA.LearningTests',$testTrees,$references,$testOptions)
$testExe=Join-Path $outputFolder 'AMA.LearningTests.exe'
$testStream=[IO.File]::Create($testExe)
try { $testResult=$testCompilation.Emit($testStream,$null,$null,$null,$null,$null,[Threading.CancellationToken]::None) }
finally { $testStream.Dispose() }
foreach($diagnostic in $testResult.Diagnostics | Where-Object { $_.Severity -eq 'Error' -or $_.Severity -eq 'Warning' }) {
    Write-Host $diagnostic.ToString()
}
if(-not $testResult.Success){exit 1}
& $testExe
exit $LASTEXITCODE
