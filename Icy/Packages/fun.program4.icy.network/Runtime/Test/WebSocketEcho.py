# pip install websockets

import asyncio
import websockets

async def echo_handler(websocket):
    """
    处理 WebSocket 连接，将接收到的所有消息原样返回
    """
    try:
        async for message in websocket:
            await websocket.send(message)
            print(f"Echo back to: {message}")
    except websockets.exceptions.ConnectionClosedOK:
        print("客户端正常断开连接")
    except websockets.exceptions.ConnectionClosedError as e:
        print(f"连接异常关闭: {e}")

async def main():
    """
    启动 WebSocket 服务器
    """
    # 设置服务器配置
    host = "localhost"
    port = 12888
    server = await websockets.serve(
        echo_handler,
        host,
        port
    )
    
    print(f"Echo Server 正在运行 ws://{host}:{port}")
    print("按 Ctrl+C 停止服务器")
    
    # 永久运行服务器
    await server.wait_closed()

if __name__ == "__main__":
    try:
        asyncio.run(main())
    except KeyboardInterrupt:
        print("\n服务器已停止")
