chcp  65001


:: 1、清除旧文件
@echo off

:: 输出目录
set "target_dir=../Icy/Assets/Scripts/Protos"
:: asmdef文件要保留
set "keep_file1=Protos.asmdef"
set "keep_file2=Protos.asmdef.meta"
:: 记录当前目录
set "cur_dir=%~dp0"
:: 切换到输出目录
cd /d "%target_dir%"

:: 删除除指定文件外的所有文件
for %%f in (*) do (
    if not "%%f"=="%keep_file1%" (
        if not "%%f"=="%keep_file2%" (
            del /q "%%f"
        )
    )
)

:: 删除除指定文件外的所有子目录
for /d %%d in (*) do (
    rd /s /q "%%d"
)

::echo 目录已清空，保留了文件 %keep_file1% 和 %keep_file2%
cd /d "%cur_dir%"


:: 2、开始编译
@echo.
@echo compile proto to C#

@echo.
@echo 正在编译proto文件，请稍候...

@call protoc\bin\protoc.exe  --csharp_out "%target_dir%" *.proto

@echo.
@echo ====================
@echo ===== 编译完成 =====
@echo ====================

pause