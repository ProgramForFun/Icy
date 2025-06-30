chcp  65001

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