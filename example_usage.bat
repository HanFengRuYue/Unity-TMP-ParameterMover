@echo off
echo Unity TMP Parameter Mover - 批处理示例
echo.

rem 设置示例文件路径
set ORIGIN_FILE=original_font.json
set FROM_FILE=source_font.json

rem 检查程序是否存在
if not exist UnityTMPParameterMover.exe (
    echo 错误: 找不到 UnityTMPParameterMover.exe
    echo 请确保程序在当前目录中
    pause
    exit /b 1
)

rem 检查原始文件是否存在
if not exist "%ORIGIN_FILE%" (
    echo 错误: 找不到原始文件: %ORIGIN_FILE%
    echo 请确保文件存在或修改脚本中的文件路径
    pause
    exit /b 1
)

rem 检查来源文件是否存在
if not exist "%FROM_FILE%" (
    echo 错误: 找不到来源文件: %FROM_FILE%
    echo 请确保文件存在或修改脚本中的文件路径
    pause
    exit /b 1
)

echo 正在处理文件:
echo 原始文件: %ORIGIN_FILE%
echo 来源文件: %FROM_FILE%
echo.

rem 执行参数移动
UnityTMPParameterMover.exe -o "%ORIGIN_FILE%" -f "%FROM_FILE%"

echo.
echo 处理完成!
pause 