@echo off
chcp 65001 > nul
echo ====================================================
echo     .NET项目编译缓存清理工具
echo ====================================================
echo.
echo [信息] 此脚本将清理:
echo   - 所有 bin 和 obj 文件夹
echo   - .vs 文件夹
echo   - node_modules 文件夹（如果存在）
echo   - bin 和 obj 文件夹
echo.
echo [警告] 这不会影响您的源代码
echo.

set /p confirm=确认清理? (Y/N):
if /i not "%confirm%"=="Y" (
    echo 已取消。
    pause
    exit /b
)

echo.
echo ====================================================
echo 开始清理...
echo ====================================================
echo.

echo [1/4] 清理 bin 和 obj 文件夹...
for /d /r . %%d in (bin obj) do (
    if exist "%%d" (
        echo  删除: %%d
        rd /s /q "%%d" 2>nul
    )
)
echo  [完成]

echo [2/4] 清理 .vs 文件夹...
if exist .vs (
    echo  删除: .vs
    rd /s /q ".vs" 2>nul
)
echo  [完成]

echo [3/4] 清理 node_modules 文件夹...
for /d %%m in (node_modules) do (
    if exist "%%m" (
        echo  删除: %%m
        rd /s /q "%%m" 2>nul
    )
)
echo  [完成]

echo [4/4] 清理 package-lock.json...
if exist package-lock.json (
    echo  删除: package-lock.json
    del /q "package-lock.json" 2>nul
)
echo  [完成]

echo.
echo ====================================================
echo            清理完成！
echo ====================================================
echo.
echo [建议] 执行以下命令重新编译:
echo.
echo   dotnet restore
echo   dotnet build
echo.
echo ====================================================
pause
