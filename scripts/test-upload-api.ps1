param(
    [string]$BaseUrl = "http://localhost:5010/api",
    [switch]$UseGateway
)

if ($UseGateway) {
    $BaseUrl = "http://localhost:5000/api/v1"
}

Write-Host "========== CLOUD/LOCAL STORAGE UPLOAD API TEST ($BaseUrl) =========="

# 1. Tạo file tạm thời để test upload
$tempFilePath = [System.IO.Path]::GetTempFileName()
[System.IO.File]::WriteAllText($tempFilePath, "Hello this is a mock test image file content to test API upload functionality.")

# Đổi tên file tạm thành .txt hoặc .png để gửi qua API
$testFilePath = "$tempFilePath.png"
Rename-Item $tempFilePath $testFilePath

Write-Host "File test tạm thời đã được tạo: $testFilePath"

try {
    # 2. Xây dựng Multipart Form-Data Request
    $fileBytes = [System.IO.File]::ReadAllBytes($testFilePath)
    $fileName = [System.IO.Path]::GetFileName($testFilePath)
    
    $LF = "`r`n"
    $boundary = [System.Guid]::NewGuid().ToString()
    
    $bodyLines = (
        "--$boundary",
        'Content-Disposition: form-data; name="file"; filename="' + $fileName + '"',
        "Content-Type: image/png",
        "",
        [System.Text.Encoding]::GetEncoding("iso-8859-1").GetString($fileBytes),
        "--$boundary--",
        ""
    ) -join $LF

    $bodyBytes = [System.Text.Encoding]::GetEncoding("iso-8859-1").GetBytes($bodyLines)
    
    # 3. Gửi request POST tới /api/upload
    Write-Host "Đang gửi file upload tới: $BaseUrl/upload..."
    $response = Invoke-RestMethod -Uri "$BaseUrl/upload" `
                                  -Method POST `
                                  -ContentType "multipart/form-data; boundary=$boundary" `
                                  -Body $bodyBytes
    
    # 4. Kiểm tra phản hồi
    if ($response -and ($response.success -eq $true -or $response.IsSuccess -eq $true)) {
        Write-Host "[OK] Tải file lên thành công!" -ForegroundColor Green
        Write-Host "     Phản hồi từ API: $($response.message)"
        Write-Host "     URL file: $($response.data)" -ForegroundColor Cyan
    } else {
        Write-Host "[FAIL] API phản hồi không thành công." -ForegroundColor Red
        Write-Host "       Phản hồi chi tiết: $response"
        exit 1
    }
}
catch {
    Write-Host "[FAIL] Gặp lỗi khi gọi API upload!" -ForegroundColor Red
    $status = 0
    if ($_.Exception.Response) { $status = [int]$_.Exception.Response.StatusCode }
    Write-Host "       Status Code: $status"
    
    $body = $_.ErrorDetails.Message
    if (-not $body -and $_.Exception.Response) {
        $reader = [System.IO.StreamReader]::new($_.Exception.Response.GetResponseStream())
        $body = $reader.ReadToEnd()
    }
    Write-Host "       Chi tiết lỗi: $body"
    exit 1
}
finally {
    # 5. Dọn dẹp file test tạm
    if (Test-Path $testFilePath) {
        Remove-Item $testFilePath -Force
        Write-Host "Đã dọn dẹp file test tạm thời."
    }
}

Write-Host "=========================================="
