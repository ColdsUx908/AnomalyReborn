param([switch]$IncludeConsumers)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path

foreach ($project in @('Transoceanic', 'TransoceanicCodeAssist')) {
    $projectRoot = Join-Path $repoRoot $project
    foreach ($file in Get-ChildItem -LiteralPath $projectRoot -Recurse -File -Filter '*.cs') {
        $relativePath = [IO.Path]::GetRelativePath($projectRoot, $file.FullName)
        if ($relativePath -match '(^|[\\/])(bin|obj)[\\/]') { continue }

        $source = [IO.File]::ReadAllText($file.FullName)
        $namespace = [regex]::Match($source, '(?m)^namespace\s+([\w.]+)\s*[;{]')
        if (!$namespace.Success) {
            if ($file.Name -eq 'GlobalUsings.cs') { continue }
            throw "Missing namespace: $($file.FullName)"
        }

        $directory = [IO.Path]::GetDirectoryName($relativePath)
        $expected = if ($directory) { "$project.$($directory -replace '[\\/]', '.')" } else { $project }
        if ($namespace.Groups[1].Value -ne $expected) {
            throw "Namespace mismatch: $relativePath (expected $expected)"
        }
    }
}

Push-Location $repoRoot
try {
    dotnet build Transoceanic/Transoceanic.csproj -p:BuildMod=false -m:1
    if ($LASTEXITCODE -ne 0) { throw 'Transoceanic build failed.' }

    if ($IncludeConsumers) {
        dotnet build Anomalies/Anomalies.csproj -p:BuildMod=false -m:1
        if ($LASTEXITCODE -ne 0) { throw 'Anomalies build failed.' }
    }

    foreach ($test in @('AutoHosting/AutoHosting.Tests.csproj', 'Primitives/Primitives.Tests.csproj')) {
        dotnet run --project "Tests/Transoceanic/$test"
        if ($LASTEXITCODE -ne 0) { throw "Test failed: $test" }
    }
}
finally {
    Pop-Location
}
