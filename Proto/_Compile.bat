chcp  65001
@echo off

IF "%~1"=="" (
	echo 不能直接调用，请通过Unity菜单Icy/Compile Proto来编译
	pause
	exit /b 1
)

:: 输出目录
set "target_dir=%~1"
:: asmdef和Reset Extension文件要保留
set "keep_file=Protos.asmdef"
set "keep_file2=ResetMethodExtension.cs"
set "keep_extension=.meta"
:: 记录当前目录
set "cur_dir=%~dp0"
:: 切换到输出目录
cd /d "%target_dir%"

:: 1、清除旧文件
:: 删除除指定文件外的所有文件
for %%f in (*.*) do (
    if not "%%f"=="%keep_file%" (
        if not "%%f"=="%keep_file2%" (
            if not "%%~xf"=="%keep_extension%" (
                del /q "%%f"
            )
        )
    )
)

:: 删除除指定文件外的所有子目录
for /d %%d in (*) do (
    rd /s /q "%%d"
)

cd /d "%cur_dir%"


:: 2、开始编译
@echo.
@echo compile proto to C#

@echo.
@echo 正在编译proto文件，请稍候...
@echo.

@call protoc\bin\protoc.exe  --csharp_out "%target_dir%" *.proto

if %ERRORLEVEL%==0 (
	@echo ====================
	@echo ===== 编译完成 =====
	@echo ====================
)

pause