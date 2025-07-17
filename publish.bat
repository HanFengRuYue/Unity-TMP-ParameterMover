@echo off
chcp 65001 > nul
echo.
echo ========================================
echo 正在编译 Unity TMP Parameter Mover...
echo ========================================
echo.

REM 清理之前的构建
if exist ".\bin\Release" (
    rd /s /q ".\bin\Release"
    echo 清理旧的构建文件
)

if exist ".\publish" (
    rd /s /q ".\publish"
    echo 清理旧的发布文件
)

echo.
echo 正在发布独立exe程序...
dotnet publish UnityTMPParameterMover.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:DebugType=none -p:DebugSymbols=false -o .\publish

if %ERRORLEVEL% EQU 0 (
    echo.
    echo 发布成功！
    echo ========================================
    echo.
    echo exe文件位置: %CD%\publish\UnityTMPParameterMover.exe
    if exist ".\publish\UnityTMPParameterMover.exe" (
        echo exe程序生成成功，可以独立运行
    )
    echo.
    echo 发布完成！您可以将 UnityTMPParameterMover.exe 复制到任何Windows电脑上运行。
) else (
    echo.
    echo 发布失败！请检查错误信息。
)

echo.
pause 