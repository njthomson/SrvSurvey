# Proves the E2E tests catch the bugs: runs the CURRENT tests against the product code of -BaseRef
# (default: the upstream commit these fixes started from) in a temporary git worktree.
# Expected: every fix-test fails there. Regression-guard-only tests (StatusRace, Threading) may pass.
param(
    [string]$BaseRef = "1a734c14",
    [string]$Artifacts = (Join-Path $PSScriptRoot "..\..\..\e2e-artifacts")
)

$repo = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$Artifacts = [IO.Path]::GetFullPath($Artifacts)
New-Item -ItemType Directory -Force $Artifacts | Out-Null
$wt = Join-Path ([IO.Path]::GetTempPath()) ("srvsurvey-red-" + [Guid]::NewGuid().ToString("N").Substring(0, 8))

git -C $repo worktree add --detach $wt $BaseRef | Out-Null
try {
    # current tests + harness, old product code
    Remove-Item -Recurse -Force (Join-Path $wt "SrvSurvey.Tests")
    Copy-Item -Recurse (Join-Path $repo "SrvSurvey.Tests") (Join-Path $wt "SrvSurvey.Tests") -Exclude bin, obj
    Get-ChildItem (Join-Path $wt "SrvSurvey.Tests") -Directory -Recurse -Include bin, obj | Remove-Item -Recurse -Force

    $env:SRVSURVEY_E2E_ARTIFACTS = Join-Path $Artifacts "red"
    dotnet test (Join-Path $wt "SrvSurvey.Tests\SrvSurvey.Tests.csproj") -c Release -p:Platform=x64 --filter "FullyQualifiedName~SrvSurvey.Tests.E2E" --logger "console;verbosity=normal" 2>&1 |
        Where-Object { $_ -notmatch "Sorting:" } |
        Tee-Object -FilePath (Join-Path $Artifacts "red-run.txt")
}
finally {
    git -C $repo worktree remove --force $wt
}
