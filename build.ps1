<#
.SYNOPSIS
    로컬과 CI가 똑같이 실행하는 빌드 스크립트.

.DESCRIPTION
    WHY: CI(YAML)에 명령을 직접 쓰면 "CI에서만 깨지는" 문제를 로컬에서 재현할 수 없다.
         모든 단계를 이 파일에 두고 CI는 이 스크립트를 호출만 한다 → 푸시 전에 .\build.ps1 로 CI를 미리 돌려 볼 수 있다.

    단계: tool restore → restore → format 검사 → build → test(+coverage) → 커버리지 리포트 → 산출물 정리
    산출물: artifacts\ (test-results, coverage, lib, demo)

.EXAMPLE
    .\build.ps1                 # CI와 동일
    .\build.ps1 -SkipFormat     # 작업 중 빠르게 빌드/테스트만
#>
[CmdletBinding()]
param(
    [string] $Configuration = 'Release',
    [switch] $SkipFormat
)

$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

$Solution  = 'shComponent.sln'
$Artifacts = Join-Path $PSScriptRoot 'artifacts'

# WHY: dotnet 은 실패해도 PowerShell 예외를 던지지 않는다. 종료 코드를 직접 검사해야 CI가 실패를 알아챈다.
function Invoke-Step([string] $Name, [scriptblock] $Command) {
    Write-Host "`n=== $Name ===" -ForegroundColor Cyan
    # WHY: Windows PowerShell 5.1 은 stderr 가 리디렉션(2>&1)된 상태에서 네이티브 명령이 stderr 에 한 줄만 써도
    #      Stop 설정이면 예외로 중단한다(예: dotnet format 의 "작업 영역 로드 경고"). 성공/실패는 종료 코드로만 판단한다.
    $ErrorActionPreference = 'Continue'
    $sw = [Diagnostics.Stopwatch]::StartNew()
    & $Command
    if ($LASTEXITCODE -ne 0) { throw "$Name 실패 (exit $LASTEXITCODE)" }
    Write-Host "--- $Name 완료 ($([int]$sw.Elapsed.TotalSeconds)s)" -ForegroundColor DarkGray
}

# WHY: 이전 실행의 결과 파일이 섞이면 커버리지/테스트 결과가 부풀려진다.
if (Test-Path $Artifacts) { Remove-Item $Artifacts -Recurse -Force }

# WHY: 첫 줄 로그로 어떤 SDK가 쓰였는지 남긴다(global.json 과 다르면 여기서 바로 보임).
Invoke-Step 'SDK 확인' { dotnet --version }

Invoke-Step 'Tool restore' { dotnet tool restore }

# WHY: restore 를 한 번만 하고 이후 단계는 --no-restore / --no-build 로 재사용 → 시간 단축, 단계별 실패 원인이 분명해짐.
Invoke-Step 'Restore' { dotnet restore $Solution }

if (-not $SkipFormat) {
    # WHY: .editorconfig 규칙과 다른 코드가 있으면 실패. 고치려면: dotnet format shComponent.sln
    Invoke-Step 'Format 검사' { dotnet format $Solution --verify-no-changes --no-restore }
}

Invoke-Step 'Build' { dotnet build $Solution -c $Configuration --no-restore }

# WHY: global.json 의 test.runner = Microsoft.Testing.Platform 이라 옵션을 그대로 넘긴다.
#      --coverage: 커버리지 수집, --report-xunit-trx: VS/CI에서 열 수 있는 결과 파일.
Invoke-Step 'Test' {
    dotnet test --solution $Solution -c $Configuration --no-build `
        --coverage --coverage-output-format cobertura `
        --report-xunit-trx `
        --results-directory (Join-Path $Artifacts 'test-results')
}

# WHY: net48/net8 두 번 수집된 커버리지를 하나로 합치고, 테스트 코드·xUnit 어셈블리는 빼고 shComponent 만 집계한다.
#      현재는 "측정만" 한다(최소 기준 없음). 기준을 두려면 아래 $MinLineCoverage 를 0보다 크게.
$MinLineCoverage = 0
Invoke-Step 'Coverage 리포트' {
    dotnet tool run reportgenerator `
        "-reports:$Artifacts/test-results/*.cobertura.xml" `
        "-targetdir:$Artifacts/coverage" `
        '-reporttypes:Cobertura;HtmlInline;MarkdownSummaryGithub;TextSummary' `
        '-assemblyfilters:+shComponent'
}
Get-Content (Join-Path $Artifacts 'coverage/Summary.txt') | Select-Object -First 12 | Write-Host

$lineRate = [double]([xml](Get-Content (Join-Path $Artifacts 'coverage/Cobertura.xml'))).coverage.'line-rate' * 100
if ($lineRate -lt $MinLineCoverage) { throw ("라인 커버리지 {0:N1}% < 기준 {1}%" -f $lineRate, $MinLineCoverage) }

# WHY: GitHub Actions 실행 화면(Summary 탭)에 커버리지 표를 바로 보여 준다. 로컬에서는 변수가 없어 건너뜀.
if ($env:GITHUB_STEP_SUMMARY) {
    Get-Content (Join-Path $Artifacts 'coverage/SummaryGithub.md') | Add-Content $env:GITHUB_STEP_SUMMARY
}

# WHY: 업로드할 산출물을 한 폴더에 모아 CI YAML이 경로를 몰라도 되게 한다.
Copy-Item "shComponent/bin/$Configuration"      (Join-Path $Artifacts 'lib')  -Recurse
Copy-Item "shComponent.Demo/bin/$Configuration" (Join-Path $Artifacts 'demo') -Recurse

Write-Host "`n빌드 성공. 산출물: $Artifacts" -ForegroundColor Green
