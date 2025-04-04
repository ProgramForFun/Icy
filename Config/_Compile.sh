#!/bin/bash

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