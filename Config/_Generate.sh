#!/bin/bash

if [ $# -eq 0 ]; then
  echo "不能直接调用，请通过Unity菜单Icy/Generate Config来生成"
  exit 1
fi

WORKSPACE=.
LUBAN_DLL=$WORKSPACE/Luban/Luban.dll
CONF_ROOT=.

CODE_DIR=$1
BIN_DIR=$2
JSON_DIR=$3

dotnet $LUBAN_DLL \
    -t all \
    -c cs-bin \
    -d json \
    -d bin \
    --conf $CONF_ROOT/luban.conf \
    -x outputCodeDir=$CODE_DIR \
    -x json.outputDataDir=$JSON_DIR \
    -x bin.outputDataDir=$BIN_DIR