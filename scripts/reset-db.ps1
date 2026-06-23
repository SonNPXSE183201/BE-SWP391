$connStringMaster = "Server=.;User Id=sa;Password=12345;TrustServerCertificate=True;Encrypt=False"
$connStringDb = "Server=.;Database=MangaPublishing;User Id=sa;Password=12345;TrustServerCertificate=True;Encrypt=False"

$basePath = (Get-Item -Path $PSScriptRoot).Parent.FullName
$schemaFile = Join-Path $basePath "Database\schema.sql"
$seedFile = Join-Path $basePath "Database\seed.sql"

# 1. Run schema.sql (uses master to drop/recreate db)
$schemaText = Get-Content -Raw -Path $schemaFile
$schemaBatches = $schemaText -split "(?m)^\s*GO\s*$"

$connection = New-Object System.Data.SqlClient.SqlConnection($connStringMaster)
try {
    $connection.Open()
    Write-Host "Đang tạo lại database schema..."
    foreach ($batch in $schemaBatches) {
        $trimmedBatch = $batch.Trim()
        if (-not [string]::IsNullOrWhiteSpace($trimmedBatch)) {
            $command = $connection.CreateCommand()
            $command.CommandText = $trimmedBatch
            $command.ExecuteNonQuery() > $null
        }
    }
    Write-Host "Tạo database schema thành công!" -ForegroundColor Green
}
catch {
    Write-Error "Lỗi khi tạo schema: $_"
}
finally {
    if ($connection.State -eq [System.Data.ConnectionState]::Open) {
        $connection.Close()
    }
}

# 2. Run seed.sql
$seedText = Get-Content -Raw -Path $seedFile
$seedBatches = $seedText -split "(?m)^\s*GO\s*$"

$connection = New-Object System.Data.SqlClient.SqlConnection($connStringDb)
try {
    $connection.Open()
    Write-Host "Đang seed dữ liệu..."
    foreach ($batch in $seedBatches) {
        $trimmedBatch = $batch.Trim()
        if (-not [string]::IsNullOrWhiteSpace($trimmedBatch)) {
            $command = $connection.CreateCommand()
            $command.CommandText = $trimmedBatch
            $command.ExecuteNonQuery() > $null
        }
    }
    Write-Host "Seed dữ liệu database thành công!" -ForegroundColor Green
}
catch {
    Write-Error "Lỗi khi seed dữ liệu: $_"
}
finally {
    if ($connection.State -eq [System.Data.ConnectionState]::Open) {
        $connection.Close()
    }
}
