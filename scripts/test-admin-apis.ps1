$base = "http://localhost:5010/api"
$passed = 0
$failed = 0

function Test-Step($name, $scriptBlock) {
    try {
        $r = & $scriptBlock
        Write-Host "[200] $name"
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

Write-Host "========== MCWPMS ADMIN API TEST =========="

# Chuẩn bị data pending assistant trong DB
$patch = "d:\FPTU\SWP391\BE-SWP391\Database\patch_test_pending_assistants.sql"
cmd /c "sqlcmd -S localhost\SQL2022 -U sa -P 12345 -i `"$patch`" -C" | Out-Host

$login = Test-Step "POST /api/auth/login" {
    Invoke-RestMethod -Uri "$base/auth/login" -Method POST -ContentType "application/json" -Body '{"Identifier":"admin","Password":"12345"}'
}
if (-not $login) { exit 1 }

$h = @{ Authorization = "Bearer $($login.Data.Token)" }
$ts = [DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds()

$mangaka = Test-Step "POST /api/admin/users (Mangaka)" {
    $b = "{`"RoleId`":4,`"UserName`":`"mangaka_$ts`",`"Email`":`"mangaka.$ts@gmail.com`",`"FullName`":`"Tran Van Nam`",`"PenName`":`"NamArt_$ts`"}"
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($b)
    Invoke-RestMethod -Uri "$base/admin/users" -Method POST -ContentType "application/json; charset=utf-8" -Headers $h -Body $bytes
}

Test-Step "POST /api/admin/users (Editor)" {
    $b = "{`"RoleId`":2,`"UserName`":`"editor_$ts`",`"Email`":`"editor.$ts@gmail.com`",`"FullName`":`"Le Thi Bien Tap`"}"
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($b)
    Invoke-RestMethod -Uri "$base/admin/users" -Method POST -ContentType "application/json; charset=utf-8" -Headers $h -Body $bytes
} | Out-Null

Test-Step "POST /api/admin/users (Board)" {
    $b = "{`"RoleId`":3,`"UserName`":`"board_$ts`",`"Email`":`"board.$ts@gmail.com`",`"FullName`":`"Pham Van Hoi Dong`"}"
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($b)
    Invoke-RestMethod -Uri "$base/admin/users" -Method POST -ContentType "application/json; charset=utf-8" -Headers $h -Body $bytes
} | Out-Null

$pending = Test-Step "GET /api/admin/users/pending" {
    Invoke-RestMethod -Uri "$base/admin/users/pending" -Method GET -Headers $h
}
$approveId = $null
$rejectId = $null
if ($pending -and $pending.Data.Count -ge 1) {
    $approveId = $pending.Data[0].Id
    Write-Host "      Pending count: $($pending.Data.Count), approve id: $approveId"
}
if ($pending -and $pending.Data.Count -ge 2) {
    $rejectId = $pending.Data[1].Id
    Write-Host "      reject id: $rejectId"
}

if ($approveId) {
    Test-Step "POST /api/admin/users/$approveId/approve" {
        Invoke-RestMethod -Uri "$base/admin/users/$approveId/approve" -Method POST -Headers $h
    } | Out-Null
} else {
    Write-Host "[FAIL] POST /api/admin/users/approve - no pending assistant"
    $failed++
}

if ($rejectId) {
    Test-Step "POST /api/admin/users/$rejectId/reject" {
        Invoke-RestMethod -Uri "$base/admin/users/$rejectId/reject" -Method POST -Headers $h
    } | Out-Null
} else {
    Write-Host "[FAIL] POST /api/admin/users/reject - need 2 pending assistants"
    $failed++
}

$lockId = if ($mangaka) { $mangaka.Data.Id } else { 2 }
Test-Step "POST /api/admin/users/$lockId/lock" {
    Invoke-RestMethod -Uri "$base/admin/users/$lockId/lock" -Method POST -Headers $h
} | Out-Null

Write-Host "=========================================="
Write-Host "PASSED: $passed | FAILED: $failed"
if ($failed -gt 0) { exit 1 }
