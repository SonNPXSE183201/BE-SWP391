param(
    [string]$BaseUrl = "http://localhost:5010/api",
    [switch]$UseGateway
)

if ($UseGateway) {
    $BaseUrl = "http://localhost:5000/api/v1"
}

$passed = 0
$failed = 0
$ts = [DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds()

function Test-Step($name, $scriptBlock) {
    try {
        $r = & $scriptBlock
        Write-Host "[OK] $name"
        if ($r.Message) { Write-Host "      $($r.Message)" }
        $script:passed++
        return $r
    }
    catch {
        $status = 0
        $body = $_.ErrorDetails.Message
        if ($_.Exception.Response) { $status = [int]$_.Exception.Response.StatusCode }
        if (-not $body -and $_.Exception.Response) {
            $reader = [System.IO.StreamReader]::new($_.Exception.Response.GetResponseStream())
            $body = $reader.ReadToEnd()
        }
        Write-Host "[FAIL $status] $name"
        Write-Host "      $body"
        $script:failed++
        return $null
    }
}

Write-Host "========== ADMIN FE API TEST ($BaseUrl) =========="

$sqlService = Get-Service "MSSQL`$SQL2022" -ErrorAction SilentlyContinue
if ($sqlService -and $sqlService.Status -ne "Running") {
    Write-Host "[BLOCKED] SQL Server (SQL2022) dang tat."
    Write-Host "          Mo PowerShell (Run as Administrator) va chay:"
    Write-Host "          net start MSSQL`$SQL2022"
    Write-Host "          Sau do chay lai run-be.bat roi chay script test nay."
    exit 2
}

$login = Test-Step "POST /api/auth/login" {
    Invoke-RestMethod -Uri "$BaseUrl/auth/login" -Method POST -ContentType "application/json" -Body '{"Identifier":"admin","Password":"12345"}'
}
if (-not $login -or -not $login.Data.Token) { exit 1 }

$h = @{ Authorization = "Bearer $($login.Data.Token)" }

$dashboard = Test-Step "GET /api/dashboard/admin" {
    Invoke-RestMethod -Uri "$BaseUrl/dashboard/admin" -Method GET -Headers $h
}
if ($dashboard -and $dashboard.Data) {
    Write-Host "      stats.users=$($dashboard.Data.stats.users) approvals=$($dashboard.Data.stats.approvals)"
}

$users = Test-Step "GET /api/admin/users" {
    Invoke-RestMethod -Uri "$BaseUrl/admin/users?role=Assistant&status=Pending" -Method GET -Headers $h
}
if ($users -and $users.Data) {
    Write-Host "      user count=$($users.Data.Count)"
}

$series = Test-Step "GET /api/admin/contracts/series" {
    Invoke-RestMethod -Uri "$BaseUrl/admin/contracts/series" -Method GET -Headers $h
}
if ($series -and $series.Data) {
    Write-Host "      series count=$($series.Data.Count)"
}

$contractSeriesId = $null
if ($series -and $series.Data.Count -gt 0) {
    $noContract = $series.Data | Where-Object { -not $_.hasContract } | Select-Object -First 1
    if ($noContract) { $contractSeriesId = $noContract.id }
}

if ($contractSeriesId) {
    Test-Step "POST /api/admin/contracts" {
        $body = @{ seriesId = $contractSeriesId; baseGenkouryoPrice = 150000 } | ConvertTo-Json
        Invoke-RestMethod -Uri "$BaseUrl/admin/contracts" -Method POST -ContentType "application/json" -Headers $h -Body $body
    } | Out-Null
} else {
    Write-Host "[SKIP] POST /api/admin/contracts - no Board_Approved series without contract in DB"
}

$recon = Test-Step "GET /api/admin/reconciliation" {
    Invoke-RestMethod -Uri "$BaseUrl/admin/reconciliation?status=All" -Method GET -Headers $h
}
if ($recon -and $recon.Data) {
    Write-Host "      records=$($recon.Data.records.Count) matched=$($recon.Data.summary.matchedCount)"
}

$pendingAssistants = Test-Step "GET /api/admin/users/pending" {
    Invoke-RestMethod -Uri "$BaseUrl/admin/users/pending" -Method GET -Headers $h
}

$approveId = $null
if ($pendingAssistants -and $pendingAssistants.Data.Count -ge 1) {
    $approveId = $pendingAssistants.Data[0].Id
}

if ($approveId) {
    Test-Step "PUT /api/admin/users/$approveId/approve (approved=true)" {
        $body = @{ userId = "$approveId"; approved = $true } | ConvertTo-Json
        Invoke-RestMethod -Uri "$BaseUrl/admin/users/$approveId/approve" -Method PUT -ContentType "application/json" -Headers $h -Body $body
    } | Out-Null
} else {
    Write-Host "[SKIP] PUT approve - no pending assistant (register via /api/auth/register first)"
}

# Legacy APIs still supported
Test-Step "POST /api/admin/users (Mangaka)" {
    $b = "{`"RoleId`":4,`"UserName`":`"mangaka_$ts`",`"Email`":`"mangaka.$ts@gmail.com`",`"FullName`":`"Test Mangaka`",`"PenName`":`"Pen$ts`"}"
    Invoke-RestMethod -Uri "$BaseUrl/admin/users" -Method POST -ContentType "application/json; charset=utf-8" -Headers $h -Body ([System.Text.Encoding]::UTF8.GetBytes($b))
} | Out-Null

Write-Host "=========================================="
Write-Host "PASSED: $passed | FAILED: $failed"
if ($failed -gt 0) { exit 1 }
