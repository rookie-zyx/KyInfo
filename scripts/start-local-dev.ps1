# KyInfo 本地开发：启动 Docker PostgreSQL
# 用法: powershell -ExecutionPolicy Bypass -File scripts\start-local-dev.ps1

$ErrorActionPreference = "Stop"
$dockerBin = "$env:LOCALAPPDATA\Programs\DockerDesktop\resources\bin"
if (Test-Path $dockerBin) {
    $env:PATH = "$dockerBin;$env:PATH"
}

$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

Write-Host ">>> 启动 PostgreSQL (Docker)..." -ForegroundColor Cyan
docker compose up -d postgres

Write-Host ">>> 等待数据库就绪..." -ForegroundColor Cyan
for ($i = 0; $i -lt 30; $i++) {
    $h = docker inspect --format='{{.State.Health.Status}}' kyinfo-postgres 2>$null
    if ($h -eq "healthy") { break }
    Start-Sleep -Seconds 2
}

Write-Host ">>> 编译解决方案..." -ForegroundColor Cyan
dotnet build KyInfo.sln -c Debug

Write-Host ""
Write-Host "请在两个终端分别执行:" -ForegroundColor Yellow
Write-Host "  终端1: dotnet run --project KyInfo.Api --urls http://127.0.0.1:5028"
Write-Host "  终端2: dotnet run --project KyInfo.Blazor"
Write-Host ""
Write-Host "访问: http://localhost:5179  管理员: admin / Admin123!" -ForegroundColor Green
