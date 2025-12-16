import http.server
import socketserver
import os

PORT = 12666
DIRECTORY = "C:/Users/dd/Desktop/AssetCDN"
#完整路径应为：C:/Users/dd/Desktop/AssetCDN/CDN/Android/v1.0
#v1.0目录内，为YooAsset单次打包出来的那个以日期命名的目录

os.chdir(DIRECTORY)
Handler = http.server.SimpleHTTPRequestHandler
Handler.extensions_map.update({
    '.bundle': 'application/octet-stream',
})

with socketserver.TCPServer(("", PORT), Handler) as httpd:
    print(f"资源服务器运行在: http://localhost:{PORT}")
    httpd.serve_forever()
