import http.server
import socketserver
import os

PORT = 12666
DIRECTORY = "C:/Users/dd/Desktop/AssetCDN"
#完整路径应为：C:/Users/dd/Desktop/AssetCDN/CDN/Android/v1.0
#其中的1.0应该为底包中的版本号，也就UnityEngine.Application.version
#v1.0目录内，为打Bundle后，StreamingAssets/yoo/DefaultPackage目录中的内容，直接copy覆盖进来

os.chdir(DIRECTORY)
Handler = http.server.SimpleHTTPRequestHandler
Handler.extensions_map.update({
    '.bundle': 'application/octet-stream', #添加.bundle文件的MIME类型，指定为二进制流
})

with socketserver.TCPServer(("", PORT), Handler) as httpd:
    print(f"资源服务器运行在: http://localhost:{PORT}")
    httpd.serve_forever()
