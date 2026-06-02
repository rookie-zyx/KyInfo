# =====================================================
# C盘常用清理脚本
# 执行时间: 2026-06-01
# 注意事项: 请仔细阅读每个清理项的说明
# =====================================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "       C盘清理工具 v1.0" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 检查C盘空间
Write-Host "正在检查C盘空间..." -ForegroundColor Yellow
$drive = Get-PSDrive -Name C
$freeGB = [math]::Round($drive.Free/1GB, 2)
$usedGB = [math]::Round($drive.Used/1GB, 2)
$totalGB = [math]::Round(($drive.Free + $drive.Used)/1GB, 2)
Write-Host "C盘总空间: $totalGB GB" -ForegroundColor Green
Write-Host "已使用空间: $usedGB GB" -ForegroundColor Yellow
Write-Host "剩余空间: $freeGB GB" -ForegroundColor Green
Write-Host ""

# =====================================================
# 1. 清理Windows临时文件
# =====================================================
Write-Host "[1/6] 清理Windows临时文件..." -ForegroundColor Cyan
$windowsTemp = "C:\Windows\Temp"
if (Test-Path $windowsTemp) {
    try {
        Get-ChildItem -Path $windowsTemp -Recurse -Force -ErrorAction SilentlyContinue |
            Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
        Write-Host "  ✓ Windows临时文件已清理" -ForegroundColor Green
    } catch {
        Write-Host "  ⚠ 部分文件无法删除（需要管理员权限）" -ForegroundColor Yellow
    }
}

# =====================================================
# 2. 清理用户临时文件夹
# =====================================================
Write-Host "[2/6] 清理用户临时文件夹..." -ForegroundColor Cyan
$userTemp = $env:TEMP
if (Test-Path $userTemp) {
    try {
        Get-ChildItem -Path $userTemp -Recurse -Force -ErrorAction SilentlyContinue |
            Where-Object { $_.FullName -notmatch "Trojan|Virus|Malware" } |
            Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
        Write-Host "  ✓ 用户临时文件已清理" -ForegroundColor Green
    } catch {
        Write-Host "  ⚠ 部分文件无法删除" -ForegroundColor Yellow
    }
}

# =====================================================
# 3. 清理NuGet缓存
# =====================================================
Write-Host "[3/6] 清理NuGet缓存..." -ForegroundColor Cyan
$nuGetCache = "$env:USERPROFILE\.nuget\packages"
if (Test-Path $nuGetCache) {
    try {
        $nuGetSize = [math]::Round((Get-ChildItem -Path $nuGetCache -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum /1GB, 2)
        Write-Host "  ℹ NuGet缓存大小: $nuGetSize GB" -ForegroundColor Yellow
        Write-Host "  ℹ 如需清理，请手动执行: dotnet nuget locals all --clear" -ForegroundColor Cyan
    } catch {
        Write-Host "  ⚠ 无法检查NuGet缓存大小" -ForegroundColor Yellow
    }
}

# =====================================================
# 4. 清理npm缓存
# =====================================================
Write-Host "[4/6] 清理npm缓存..." -ForegroundColor Cyan
$npmCache = "$env:APPDATA\npm-cache"
if (Test-Path $npmCache) {
    try {
        $npmSize = [math]::Round((Get-ChildItem -Path $npmCache -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum /1GB, 2)
        Write-Host "  ℹ npm缓存大小: $npmSize GB" -ForegroundColor Yellow
        Write-Host "  ℹ 如需清理，请手动执行: npm cache clean --force" -ForegroundColor Cyan
    } catch {
        Write-Host "  ⚠ 无法检查npm缓存大小" -ForegroundColor Yellow
    }
}

# =====================================================
# 5. 清理回收站
# =====================================================
Write-Host "[5/6] 清理回收站..." -ForegroundColor Cyan
try {
    Clear-RecycleBin -Force -ErrorAction SilentlyContinue
    Write-Host "  ✓ 回收站已清空" -ForegroundColor Green
} catch {
    Write-Host "  ⚠ 回收站清理失败" -ForegroundColor Yellow
}

# =====================================================
# 6. 清理浏览器缓存（可选）
# =====================================================
Write-Host "[6/6] 检查浏览器缓存..." -ForegroundColor Cyan
$browserPaths = @(
    "$env:LOCALAPPDATA\Google\Chrome\User Data\Default\Cache",
    "$env:LOCALAPPDATA\Microsoft\Edge\User Data\Default\Cache",
    "$env:LOCALAPPDATA\Mozilla\Firefox\Profiles"
)

foreach ($path in $browserPaths) {
    if (Test-Path $path) {
        try {
            $size = [math]::Round((Get-ChildItem -Path $path -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum /1GB, 2)
            if ($size -gt 0) {
                Write-Host "  ℹ $path 大小: $size GB" -ForegroundColor Yellow
            }
        } catch {
        }
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "       清理完成！" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# 重新检查C盘空间
$drive = Get-PSDrive -Name C
$freeGBAfter = [math]::Round($drive.Free/1GB, 2)
$freedGB = [math]::Round($freeGBAfter - $freeGB, 2)
Write-Host ""
Write-Host "清理前剩余空间: $freeGB GB" -ForegroundColor Yellow
Write-Host "清理后剩余空间: $freeGBAfter GB" -ForegroundColor Green
if ($freedGB -gt 0) {
    Write-Host "释放空间: $freedGB GB" -ForegroundColor Green
}
Write-Host ""

# 额外建议
Write-Host "========================================" -ForegroundColor Yellow
Write-Host "       额外清理建议" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. 清理Windows更新缓存:" -ForegroundColor Cyan
Write-Host "   停止wuauserv服务后删除C:\Windows\SoftwareDistribution\Download" -ForegroundColor Gray
Write-Host ""
Write-Host "2. 清理WinSxS组件存储:" -ForegroundColor Cyan
Write-Host "   以管理员身份运行: Dism.exe /online /Cleanup-Image /StartComponentCleanup /ResetBase" -ForegroundColor Gray
Write-Host ""
Write-Host "3. 清理.NET编译缓存:" -ForegroundColor Cyan
Write-Host "   删除项目中的bin和obj文件夹" -ForegroundColor Gray
Write-Host ""
Write-Host "4. 使用Windows磁盘清理工具:" -ForegroundColor Cyan
Write-Host "   按Win+R输入: cleanmgr" -ForegroundColor Gray
Write-Host ""

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "如需执行上述清理，请手动操作或联系管理员" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan
