@echo off
chcp 65001 > nul
echo ====================================================
echo            C盘清理工具 v2.0
echo ====================================================
echo.

echo [信息] 正在检查C盘空间...
for /f "tokens=3" %%a in ('dir /-c ^| findstr /C:"可用字节"') do set free=%%a
set /a freeMB=%free:~0,-6% / 1024
echo [信息] C盘当前剩余空间: %freeMB% MB
echo.

echo ====================================================
echo 正在清理临时文件...
echo ====================================================

echo [1/5] 清理Windows临时文件...
if exist C:\Windows\Temp\ (
    del /s /q C:\Windows\Temp\*.* > nul 2>&1
    echo  [完成] Windows临时文件
)

echo [2/5] 清理用户临时文件夹...
if exist %TEMP%\ (
    del /s /q %TEMP%\*.* > nul 2>&1
    echo  [完成] 用户临时文件
)

echo [3/5] 清理系统日志文件...
if exist C:\Windows\Logs\ (
    del /s /q C:\Windows\Logs\*.* > nul 2>&1
    echo  [完成] 系统日志
)

echo [4/5] 清理预读取文件...
if exist C:\Windows\Prefetch\ (
    del /s /q C:\Windows\Prefetch\*.* > nul 2>&1
    echo  [完成] 预读取文件
)

echo [5/5] 清空回收站...
rd /s /q %SystemDrive%\$Recycle.Bin > nul 2>&1
echo  [完成] 回收站

echo.
echo ====================================================
echo            清理完成！
echo ====================================================
echo.

echo [信息] 正在重新检查C盘空间...
for /f "tokens=3" %%a in ('dir /-c ^| findstr /C:"可用字节"') do set free2=%%a
set /a freeMB2=%free2:~0,-6% / 1024
set /a freed=%freeMB2% - %freeMB%
echo 清理前剩余: %freeMB% MB
echo 清理后剩余: %freeMB2% MB
if %freed% gtr 0 (
    echo 释放空间: + %freed% MB
) else (
    echo 变化: %freed% MB
)

echo.
echo ====================================================
echo 额外清理建议（建议手动执行）:
echo ====================================================
echo.
echo 1. [推荐] 使用Windows磁盘清理工具
echo    按 Win+R 输入: cleanmgr
echo.
echo 2. [推荐] 清理Windows更新缓存
echo    以管理员身份运行命令提示符，执行:
echo    net stop wuauserv
echo    rd /s /q C:\Windows\SoftwareDistribution\Download
echo    net start wuauserv
echo.
echo 3. [进阶] 清理WinSxS组件存储
echo    以管理员身份运行命令提示符，执行:
echo    Dism.exe /online /Cleanup-Image /StartComponentCleanup /ResetBase
echo.
echo 4. [进阶] 清理.NET编译缓存
echo    在项目根目录执行:
echo    for /d /r . %%d in (bin obj) do @if exist "%%d" rd /s /q "%%d"
echo.
echo 5. [进阶] 清理NuGet缓存
echo    dotnet nuget locals all --clear
echo.
echo 6. [进阶] 清理npm缓存
echo    npm cache clean --force
echo.
echo ====================================================
pause
