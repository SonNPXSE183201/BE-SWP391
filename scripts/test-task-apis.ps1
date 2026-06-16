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

Write-Host "========== MCWPMS TASK API TEST =========="

# 1. Login as Mangaka (Id=4)
$mangakaLogin = Test-Step "POST /api/auth/login (Mangaka)" {
    Invoke-RestMethod -Uri "$base/auth/login" -Method POST -ContentType "application/json" -Body '{"Identifier":"mangaka1","Password":"12345"}'
}
if (-not $mangakaLogin) { exit 1 }
$mangakaToken = $mangakaLogin.Data.Token
$mangakaAuth = @{ Authorization = "Bearer $mangakaToken" }

# 2. Login as Assistant (Id=5)
$assistantLogin = Test-Step "POST /api/auth/login (Assistant)" {
    Invoke-RestMethod -Uri "$base/auth/login" -Method POST -ContentType "application/json" -Body '{"Identifier":"assistant1","Password":"12345"}'
}
if (-not $assistantLogin) { exit 1 }
$assistantToken = $assistantLogin.Data.Token
$assistantAuth = @{ Authorization = "Bearer $assistantToken" }

# 3. Test Mangaka Creates a Task (No Assistant Assigned - Market Task)
$createTask = Test-Step "POST /api/tasks (Create Market Task)" {
    $deadline = [DateTime]::UtcNow.AddDays(7).ToString("o")
    $b = "{`"RegionId`":1,`"Description`":`"Test tao task moi tu script`",`"PaymentAmount`":100000,`"Deadline`":`"$deadline`",`"ZIndex_Order`":5}"
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($b)
    Invoke-RestMethod -Uri "$base/tasks" -Method POST -ContentType "application/json; charset=utf-8" -Headers $mangakaAuth -Body $bytes
}

# 4. Test Assistant Gets Available Tasks
$availableTasks = Test-Step "GET /api/tasks/available" {
    Invoke-RestMethod -Uri "$base/tasks/available?PageNumber=1&PageSize=10" -Method GET -Headers $assistantAuth
}
if ($availableTasks -and $availableTasks.Data.Items.Count -gt 0) {
    Write-Host "      Found $($availableTasks.Data.TotalItems) available task(s)."
}

# 5. Test Assistant Gets My Tasks (All)
$myTasksAll = Test-Step "GET /api/tasks/my-tasks (All Status)" {
    Invoke-RestMethod -Uri "$base/tasks/my-tasks?PageNumber=1&PageSize=10" -Method GET -Headers $assistantAuth
}
if ($myTasksAll -and $myTasksAll.Data.Items.Count -gt 0) {
    Write-Host "      Found $($myTasksAll.Data.TotalItems) task(s) assigned to Assistant."
}

# 6. Test Assistant Gets My Tasks (In_Progress only)
$myTasksInProgress = Test-Step "GET /api/tasks/my-tasks?Status=In_Progress" {
    Invoke-RestMethod -Uri "$base/tasks/my-tasks?Status=In_Progress&PageNumber=1&PageSize=10" -Method GET -Headers $assistantAuth
}
if ($myTasksInProgress) {
    Write-Host "      Found $($myTasksInProgress.Data.TotalItems) In_Progress task(s)."
}

Write-Host "=========================================="
Write-Host "PASSED: $passed | FAILED: $failed"
if ($failed -gt 0) { exit 1 }
