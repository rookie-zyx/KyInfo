@echo off
chcp 65001 > nul
echo ====================================================
echo     C盘一键深度清理工具 v3.0
echo ====================================================
echo.
echo [警告] 此脚本将执行以下操作:
echo   - 清理所有临时文件
echo   - 清空回收站
echo   - 清理系统日志
echo   - 清理Windows更新缓存
echo   - 清理预读取文件
echo.
set /p confirm=确认执行清理? (Y/N):
if /i not "%confirm%"=="Y" (
    echo 已取消。
    pause
    exit /b
)

echo.
echo ====================================================
echo 开始深度清理...
echo ====================================================
echo.

echo [步骤1/8] 停止Windows更新服务...
net stop wuauserv > nul 2>&1
echo  [完成]

echo [步骤2/8] 清理Windows临时文件...
if exist C:\Windows\Temp\ (
    del /s /f /q C:\Windows\Temp\*.* > nul 2>&1
    for /d %%i in (C:\Windows\Temp\*) do rd /s /q "%%i" > nul 2>&1
)
echo  [完成]

echo [步骤3/8] 清理用户临时文件夹...
if exist %TEMP%\ (
    del /s /f /q %TEMP%\*.* > nul 2>&1
    for /d %%i in (%TEMP%\*) do rd /s /q "%%i" > nul 2>&1
)
echo  [完成]

echo [步骤4/8] 清理Windows更新缓存...
if exist C:\Windows\SoftwareDistribution\Download\ (
    del /s /f /q C:\Windows\SoftwareDistribution\Download\*.* > nul 2>&1
)
echo  [完成]

echo [步骤5/8] 清理预读取文件...
if exist C:\Windows\Prefetch\ (
    del /s /f /q C:\Windows\Prefetch\*.* > nul 2>&1
)
echo  [完成]

echo [步骤6/8] 清理系统日志...
if exist C:\Windows\Logs\ (
    del /s /f /q C:\Windows\Logs\*.* > nul 2>&1
)
echo  [完成]

echo [步骤7/8] 清理Recent文档记录...
if exist %APPDATA%\Microsoft\Windows\Recent\ (
    del /s /f /q %APPDATA%\Microsoft\Windows\Recent\*.* > nul 2>&1
)
echo  [完成]

echo [步骤8/8] 清空回收站...
rd /s /q %SystemDrive%\$Recycle.Bin > nul 2>&1
echo  [完成]

echo.
echo [启动] 重新启动Windows更新服务...
net start wuauserv > nul 2>&1
echo  [完成]

echo.
echo ====================================================
echo            清理完成！
echo ====================================================
echo.

echo [信息] 建议重启电脑以完成清理
set /p restart=是否立即重启? (Y/N):
if /i "%restart%"=="Y" (
    shutdown /r /t 5 /c "系统将在5秒后重启以完成清理"
) else (
    echo 请手动重启电脑以完成清理
)

echo.
echo ====================================================
echo 后续清理建议:
echo ====================================================
echo.
echo 1. 运行磁盘清理工具: cleanmgr
echo.
echo 2. 压缩历史版本: 
echo    Dism.exe /online /Cleanup-Image /StartComponentCleanup /ResetBase
echo.
echo 3. 清理项目编译缓存:
echo    在项目目录运行: for /d /r . %%d in (bin obj) do @if exist "%%d" rd /s /q "%%d"
echo.
echo 4. 清理浏览器缓存:
echo    Chrome: 删除 %LOCALAPPDATA%\Google\Chrome\User Data\Default\Cache
echo    Edge: 删除 %LOCALAPPDATA%\Microsoft\Edge\User Data\Default\Cache
echo.
echo 5. 清理开发工具缓存:
echo    NuGet: dotnet nuget locals all --clear
echo    npm: npm cache clean --force
echo    pip: pip cache purge
echo.
echo ====================================================
pause
