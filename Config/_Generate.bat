chcp  65001
@echo off

IF "%~1"=="" (
	echo 不能直接调用，请通过Unity菜单Icy/Generate Config来生成
	pause
	exit /b 1
)

set WORKSPACE=.
set LUBAN_DLL=%WORKSPACE%\Luban\Luban.dll
set CONF_ROOT=.

set "CODE_DIR=%~1"
set "BIN_DIR=%~2"
set "JSON_DIR=%~3"


dotnet %LUBAN_DLL% ^
    -t all ^
    -c cs-bin ^
    -d json ^
    -d bin ^
    --conf %CONF_ROOT%\luban.conf ^
    -x outputCodeDir=%CODE_DIR% ^
    -x json.outputDataDir=%JSON_DIR% ^
    -x bin.outputDataDir=%BIN_DIR%

pause