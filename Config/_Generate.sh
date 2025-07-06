#!/bin/bash

if [ $# -eq 0 ]; then
  echo "不能直接调用，请通过Unity菜单Icy/Generate Config来生成"
  exit 1
fi

WORKSPACE=.
LUBAN_DLL=$WORKSPACE/Luban/Luban.dll
CONF_ROOT=.

dotnet $LUBAN_DLL \
    -t all \
    -c cs-bin \
    -d json \
    -d bin \
    --conf $CONF_ROOT/luban.conf \
    -x outputCodeDir=../Icy/Assets/Example/Configs/code \
    -x json.outputDataDir=../Icy/Assets/Example/Configs/json \
    -x bin.outputDataDir=../Icy/Assets/Example/Configs/bin