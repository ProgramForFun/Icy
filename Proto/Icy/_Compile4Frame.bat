chcp  65001


:: 1、清除旧文件
@echo off

:: 输出目录
set "target_dir=./Target"
:: 记录当前目录
set "cur_dir=%~dp0"
:: 切换到输出目录
cd /d "%target_dir%"

:: 删除除指定文件外的所有文件
for %%f in (*.*) do (
    del /q "%%f"
)

:: 删除除指定文件外的所有子目录
for /d %%d in (*) do (
    rd /s /q "%%d"
)

::echo 目录已清空，保留了文件 %keep_file% 和 所有后缀为 %keep_extension% 的文件
cd /d "%cur_dir%"


:: 2、开始编译
@echo.
@echo compile proto to C#

@echo.
@echo 正在编译proto文件，请稍候...
@echo.

@call ..\protoc\bin\protoc.exe  --csharp_out "%target_dir%" *.proto

if %ERRORLEVEL%==0 (
	@echo ====================
	@echo ===== 编译完成 =====
	@echo ====================
)

pause