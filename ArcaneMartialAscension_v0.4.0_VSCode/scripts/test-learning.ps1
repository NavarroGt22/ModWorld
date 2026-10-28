param([string]$GamePath='')
$ErrorActionPreference='Stop'
if ($PSVersionTable.PSEdition -eq 'Core') {
    $windowsPowerShell=Join-Path $env:SystemRoot 'System32\WindowsPowerShell\v1.0\powershell.exe'
    & $windowsPowerShell -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath -GamePath $GamePath
    exit $LASTEXITCODE
}
# Reuse the installed NML compiler and the same real reference set as the mod build.
. (Join-Path $PSScriptRoot 'build-nml.ps1') -GamePath $GamePath
$testPath=Join-Path $projectRoot 'tests\LearningRulesTests.cs'
$testTree=[Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText(
    [IO.File]::ReadAllText($testPath),$options,$testPath,[Text.Encoding]::UTF8,[Threading.CancellationToken]::None)
$references.Add([Microsoft.CodeAnalysis.MetadataReference]::CreateFromFile(
    $outputFile,[Microsoft.CodeAnalysis.MetadataReferenceProperties]::Assembly,$null))
$testOptions=$compilationOptions.WithOutputKind([Microsoft.CodeAnalysis.OutputKind]::ConsoleApplication)
$testCompilation=[Microsoft.CodeAnalysis.CSharp.CSharpCompilation]::Create(
    'AMA.LearningTests',[Microsoft.CodeAnalysis.SyntaxTree[]]@($testTree),$references,$testOptions)
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
