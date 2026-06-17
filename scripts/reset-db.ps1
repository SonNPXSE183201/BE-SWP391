$connString = "Server=.;Database=MangaPublishing;User Id=sa;Password=12345;TrustServerCertificate=True;Encrypt=False"
$sqlFile = Join-Path (Get-Item -Path $PSScriptRoot).Parent.FullName "Database\seed.sql"
$sqlText = Get-Content -Raw -Path $sqlFile

# Phân tách script theo từ khóa GO (đứng độc lập trên một dòng)
$batches = $sqlText -split "(?m)^\s*GO\s*$"

$connection = New-Object System.Data.SqlClient.SqlConnection($connString)
try {
    $connection.Open()
    Write-Host "Đang kết nối tới database..."
    
    foreach ($batch in $batches) {
        $trimmedBatch = $batch.Trim()
        if (-not [string]::IsNullOrWhiteSpace($trimmedBatch)) {
            $command = $connection.CreateCommand()
            $command.CommandText = $trimmedBatch
            $command.ExecuteNonQuery() > $null
        }
    }
    Write-Host "Reset và seed dữ liệu database thành công!" -ForegroundColor Green
}
catch {
    Write-Error $_.Exception.Message
}
finally {
    if ($connection.State -eq [System.Data.ConnectionState]::Open) {
        $connection.Close()
    }
}
