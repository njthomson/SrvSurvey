# Runs all tests (journal-replay E2E + InaraTests) against a Release build (the DEBUG build skips Game's static init outside SrvSurvey.exe)
# and leaves per-test SrvSurvey logs plus the console output in the artifacts folder.
param([string]$Artifacts = (Join-Path $PSScriptRoot "..\..\..\e2e-artifacts"))

$Artifacts = [IO.Path]::GetFullPath($Artifacts)
New-Item -ItemType Directory -Force $Artifacts | Out-Null
$env:SRVSURVEY_E2E_ARTIFACTS = $Artifacts

$project = Join-Path $PSScriptRoot "..\SrvSurvey.Tests.csproj"
dotnet test $project -c Release -p:Platform=x64 --logger "console;verbosity=normal" 2>&1 |
    Where-Object { $_ -notmatch "Sorting:" } |
    Tee-Object -FilePath (Join-Path $Artifacts "run-e2e.txt")
exit $LASTEXITCODE
