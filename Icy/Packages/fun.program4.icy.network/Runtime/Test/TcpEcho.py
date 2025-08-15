import socketserver
import time

HOST = "localhost"
PORT = 12321
BUFFER_SIZE = 1024

# this server uses ThreadingMixIn - one thread per connection
# replace with ForkMixIn to spawn a new process per connection

count = 0

class EchoServer(socketserver.ThreadingMixIn, socketserver.TCPServer):
    # no need to override anything - default behavior is just fine
    pass

class EchoRequestHandler(socketserver.StreamRequestHandler):
    """
    Handles one connection to the client.
    """
    def handle(self):
        print ("connection from %s" % self.client_address[0])
        count = 0
        while True:
            # line = self.rfile.readline()
            line = self.request.recv(BUFFER_SIZE)
            if not line:
                break
            count = count + 1
            print ("Echo back to %s : [%d] %s" % (self.client_address[0], count, line))
            time.sleep(0.01)
            self.wfile.write(line)
        print ("%s disconnected" % self.client_address[0])


# Create the server
server = EchoServer((HOST, PORT), EchoRequestHandler)

# Activate the server; this will keep running until you
# interrupt the program with Ctrl-C
print ("server listening on %s:%s" % server.server_address)
server.serve_forever()
