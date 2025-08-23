# pip install kcp

from kcp.server import Connection
from kcp.server import KCPServerAsync

count = 0

# Create the initial server instance.
server = KCPServerAsync(
    "127.0.0.1",
    12333,
    conv_id=1,
    no_delay=True,
)

# Ability to set performance options after initialisation.
server.set_performance_options(
    update_interval=10,
)


# Ran when the server starts.
@server.on_start
async def on_start() -> None:
    print("Server started!")


# Ran when a connection is made.
@server.on_data
async def on_data(connection: Connection, data: bytes) -> None:
    global count
    connection.enqueue(bytes(data))
    count = count + 1
    print(f"[Kcp] Echo back to {connection.address}: [{count}]{data}")


server.start()
