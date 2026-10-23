param([string]$Configuration = 'Debug')
$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$project = Join-Path $repoRoot 'Anomalies/Anomalies.csproj'
$targets = Join-Path $PSScriptRoot 'ExcludeBoss.targets'
$bosses = Get-ChildItem -LiteralPath (Join-Path $repoRoot 'Anomalies/Bosses') -Directory
foreach ($boss in $bosses) {
    Write-Host "Compiling without $($boss.Name)"
    & dotnet build $project --no-restore -m:1 -v quiet "-p:Configuration=$Configuration" '-p:BuildMod=false' "-p:CustomAfterMicrosoftCommonTargets=$targets" "-p:ExcludedBoss=$($boss.Name)"
    if ($LASTEXITCODE -ne 0) { throw "Boss isolation failed: $($boss.Name)" }
    $assemblyPath = Join-Path $repoRoot "Anomalies/bin/$Configuration/net8.0/Anomalies.dll"
    $stream = [System.IO.File]::OpenRead($assemblyPath)
    $pe = [System.Reflection.PortableExecutable.PEReader]::new($stream)
    try {
        $metadata = [System.Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($pe)
        $prefix = "Anomalies.Bosses.$($boss.Name)"
        foreach ($handle in $metadata.TypeDefinitions) {
            $type = $metadata.GetTypeDefinition($handle)
            $namespace = $metadata.GetString($type.Namespace)
            if ($namespace -eq $prefix -or $namespace.StartsWith("$prefix.")) {
                throw "Excluded module still present in output: $namespace"
            }
        }
    }
    finally {
        $pe.Dispose()
        $stream.Dispose()
    }
    Write-Host "Verified $($boss.Name) is absent from assembly metadata"
}
# Restore the normal output after the exclusion checks.
& dotnet build $project --no-restore -m:1 -v quiet "-p:Configuration=$Configuration" '-p:BuildMod=false'
if ($LASTEXITCODE -ne 0) { throw 'Full build failed' }
