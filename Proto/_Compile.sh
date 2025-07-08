#!/bin/bash

# 检查参数是否为空
if [ -z "$1" ]; then
    echo "不能直接调用，请通过Unity菜单Icy/Compile Proto来编译"
    read -p "Press [Enter] to continue..."
    exit 1
fi

# 设置目标目录和保留文件
target_dir="$1"
keep_file1="Protos.asmdef"
keep_file2="ResetMethodExtension.cs"

# 记录当前目录
cur_dir="$(pwd)"

# 切换到输出目录
cd "$target_dir" || exit 1

# 1. 清除旧文件
# 处理根目录文件
for file in *; do
    if [ -f "$file" ]; then
        if [ "$file" != "$keep_file1" ] && [ "$file" != "$keep_file2" ] && [[ ! "$file" == *.meta ]]; then
            rm -f "$file"
        fi
    fi
done

# 删除所有子目录（包括隐藏目录）
find . -mindepth 1 -type d -exec rm -rf {} +

# 切回原目录
cd "$cur_dir" || exit 1

# 2. 开始编译
echo ""
echo "compile proto to C#"
echo ""
echo "正在编译proto文件，请稍候..."
echo ""

# 执行protoc编译
./protoc/bin/protoc --csharp_out "$target_dir" ./*.proto

# 检查编译结果
if [ $? -eq 0 ]; then
    echo "===================="
    echo "===== 编译完成 ====="
    echo "===================="
fi

# 暂停
read -p "Press [Enter] to continue..."