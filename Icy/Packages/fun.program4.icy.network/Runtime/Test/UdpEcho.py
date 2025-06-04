import socket

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.setsockopt(socket.SOL_SOCKET, socket.SO_SNDBUF, 4096)
sock.setsockopt(socket.SOL_SOCKET, socket.SO_RCVBUF, 4096)

server_address = '0.0.0.0'
server_port = 12333

server = (server_address, server_port)
sock.bind(server)
print("Listening on " + server_address + ":" + str(server_port))

while True:
	payload, client_address = sock.recvfrom(4096)
	print("Echoing " + str(payload) + " back to " + str(client_address))
	sent = sock.sendto(payload, client_address)
