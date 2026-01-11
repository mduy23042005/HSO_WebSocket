--------------------------------------------------------Cách chạy được WebSocket------------------------------------------------------

Bước 1: Mở Project HSO_WebSocket.

Bước 2: Cấu hình Port:

	1. Setup Port kết nối WebAPI:
	- Chỉ định port kết nối tới WebAPI trong file WebAPIManager.cs dòng private const string apiUrl = "http://localhost:55555"; theo 	port cho đúng với bên WebAPI.

	2. Cấu hình port cho WebSocket:
	- Mở PowerShell Run by Administrator -> gõ netsh http add urlacl url=http://localhost:55556/ user=Everyone. Có thể chọn port khác 	nếu port 55556 bị chiếm.
	- Nếu port bị chặn? Mở Window Defender Firewall with Advanced Security -> click phải vào Inbound Rules -> khai báo port và để quyền 	truy cập mở.
	- Chỉ định port kết nối tới WebSocket trong file WebSocketServerManager.cs dòng listener.Prefixes.Add("http://+:55556/");

Bước 3: Run Server:
Mở file WebSocketServerManager.cs và Run