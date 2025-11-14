# Hướng dẫn chạy Authentication Service

## Cách 1: Chạy với Docker (Khuyến nghị - Dễ nhất)

### Bước 1: Kiểm tra Docker đã cài đặt

Mở PowerShell hoặc Command Prompt và chạy:
```bash
docker --version
docker-compose --version
```

Nếu chưa có, tải Docker Desktop tại: https://www.docker.com/products/docker-desktop

### Bước 2: Mở thư mục dự án

```bash
cd D:\Developer\HTML-CSS-JS-REACT
```

### Bước 3: Chạy Docker Compose

```bash
docker-compose up -d
```

Lệnh này sẽ:
- Tải và chạy SQL Server container
- Build và chạy Backend API container
- Build và chạy Frontend container

**Lưu ý:** Lần đầu chạy sẽ mất vài phút để download images và build.

### Bước 4: Kiểm tra containers đang chạy

```bash
docker ps
```

Bạn sẽ thấy 3 containers:
- `auth-db` (SQL Server)
- `auth-api` (Backend API)
- `auth-frontend` (Frontend)

### Bước 5: Khởi tạo Database

Chờ khoảng 30 giây để SQL Server khởi động hoàn toàn, sau đó chạy:

```bash
# Tạo thư mục Migrations nếu chưa có
docker exec -it auth-api mkdir -p /src/Migrations

# Tạo migration
docker exec -it auth-api dotnet ef migrations add InitialCreate --project /src

# Apply migration để tạo database và tables
docker exec -it auth-api dotnet ef database update --project /src
```

**Nếu gặp lỗi "dotnet ef not found", chạy:**
```bash
# Cài đặt EF Core tools trong container
docker exec -it auth-api dotnet tool install --global dotnet-ef
docker exec -it auth-api dotnet ef migrations add InitialCreate --project /src
docker exec -it auth-api dotnet ef database update --project /src
```

### Bước 6: Truy cập ứng dụng

- **Frontend (Web UI)**: http://localhost:3000
- **Backend API**: http://localhost:5000
- **Swagger API Docs**: http://localhost:5000/swagger

### Bước 7: Tạo user đầu tiên

Vì chưa có user nào, bạn cần tạo user local đầu tiên qua API:

**Cách 1: Dùng Swagger UI**
1. Truy cập http://localhost:5000/swagger
2. Tìm endpoint `POST /api/users/local`
3. Click "Try it out"
4. Nhập thông tin:
```json
{
  "username": "admin",
  "password": "Admin123!",
  "email": "admin@example.com",
  "fullName": "Administrator"
}
```
5. Click "Execute"

**Cách 2: Dùng cURL hoặc Postman**
```bash
curl -X POST http://localhost:5000/api/users/local \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "Admin123!",
    "email": "admin@example.com",
    "fullName": "Administrator"
  }'
```

**Lưu ý:** Endpoint này yêu cầu authentication. Nếu bị lỗi 401, bạn có thể tạm thời comment `[Authorize]` trong `UsersController.cs` để tạo user đầu tiên.

### Bước 8: Đăng nhập

1. Truy cập http://localhost:3000
2. Nhập:
   - Username: `admin`
   - Password: `Admin123!`
   - Check "Đăng nhập bằng tài khoản local"
3. Click "Đăng nhập"

---

## Cách 2: Chạy Local (Development)

### Yêu cầu
- .NET 8.0 SDK: https://dotnet.microsoft.com/download
- Node.js 20+: https://nodejs.org/
- SQL Server (Local hoặc Docker)

### Backend

#### Bước 1: Mở terminal trong thư mục AuthenticationService

```bash
cd D:\Developer\HTML-CSS-JS-REACT\AuthenticationService
```

#### Bước 2: Restore packages

```bash
dotnet restore
```

#### Bước 3: Cài đặt EF Core tools (nếu chưa có)

```bash
dotnet tool install --global dotnet-ef
```

#### Bước 4: Tạo migration

```bash
dotnet ef migrations add InitialCreate
```

#### Bước 5: Tạo database

Đảm bảo SQL Server đang chạy, sau đó:

```bash
dotnet ef database update
```

#### Bước 6: Chạy Backend

```bash
dotnet run
```

Backend sẽ chạy tại: http://localhost:5000

### Frontend

#### Bước 1: Mở terminal mới trong thư mục frontend

```bash
cd D:\Developer\HTML-CSS-JS-REACT\frontend
```

#### Bước 2: Install dependencies

```bash
npm install
```

#### Bước 3: Chạy dev server

```bash
npm run dev
```

Frontend sẽ chạy tại: http://localhost:3000 (hoặc port khác nếu 3000 đã được dùng)

---

## Xử lý lỗi thường gặp

### 1. Lỗi "Cannot connect to database"

**Nguyên nhân:** SQL Server chưa sẵn sàng hoặc connection string sai

**Giải pháp:**
```bash
# Kiểm tra SQL Server container
docker logs auth-db

# Restart containers
docker-compose restart

# Kiểm tra connection string trong appsettings.json
```

### 2. Lỗi "dotnet ef not found"

**Giải pháp:**
```bash
# Cài đặt EF Core tools
dotnet tool install --global dotnet-ef

# Hoặc trong Docker container
docker exec -it auth-api dotnet tool install --global dotnet-ef
```

### 3. Lỗi "Port already in use"

**Nguyên nhân:** Port 3000, 5000, hoặc 1433 đã được sử dụng

**Giải pháp:**
- Đổi port trong `docker-compose.yml`:
  ```yaml
  ports:
    - "3001:80"  # Thay vì 3000:80
  ```
- Hoặc dừng service đang dùng port đó

### 4. Frontend không kết nối được API

**Giải pháp:**
- Kiểm tra `frontend/vite.config.ts` có đúng port không
- Kiểm tra CORS trong `Program.cs`
- Kiểm tra API đang chạy: http://localhost:5000/swagger

### 5. Migration lỗi

**Giải pháp:**
```bash
# Xóa migration cũ
docker exec -it auth-api dotnet ef migrations remove --project /src

# Tạo lại
docker exec -it auth-api dotnet ef migrations add InitialCreate --project /src
docker exec -it auth-api dotnet ef database update --project /src
```

### 6. Lỗi build Docker

**Giải pháp:**
```bash
# Xóa images và build lại
docker-compose down
docker-compose build --no-cache
docker-compose up -d
```

---

## Dừng ứng dụng

### Với Docker:
```bash
docker-compose down
```

### Với Docker (xóa cả volumes):
```bash
docker-compose down -v
```

### Với Local:
- Nhấn `Ctrl+C` trong terminal đang chạy

---

## Xem logs

### Docker:
```bash
# Xem logs của tất cả services
docker-compose logs

# Xem logs của một service cụ thể
docker logs auth-api
docker logs auth-db
docker logs auth-frontend

# Xem logs real-time
docker-compose logs -f
```

### Local:
- Logs hiển thị trực tiếp trong terminal

---

## Kiểm tra ứng dụng hoạt động

1. **Kiểm tra Backend:**
   - Truy cập http://localhost:5000/swagger
   - Nếu thấy Swagger UI = Backend OK

2. **Kiểm tra Frontend:**
   - Truy cập http://localhost:3000
   - Nếu thấy trang login = Frontend OK

3. **Kiểm tra Database:**
   ```bash
   # Vào SQL Server container
   docker exec -it auth-db /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd -Q "SELECT name FROM sys.databases"
   ```

---

## Cấu hình bổ sung

### Cấu hình Microsoft AD (nếu cần)

Chỉnh sửa `AuthenticationService/appsettings.json`:
```json
"MicrosoftAuth": {
  "Domain": "your-domain.com"  // Domain của AD
}
```

### Cấu hình Email (nếu cần)

Chỉnh sửa `AuthenticationService/appsettings.json`:
```json
"EmailSettings": {
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": 587,
  "SmtpUsername": "your-email@gmail.com",
  "SmtpPassword": "your-app-password",  // App Password, không phải mật khẩu thường
  "FromEmail": "your-email@gmail.com"
}
```

**Lưu ý:** Với Gmail, cần tạo App Password:
1. Vào Google Account > Security
2. Enable 2-Step Verification
3. Tạo App Password
4. Dùng App Password thay vì mật khẩu thường

---

## Next Steps

Sau khi chạy thành công:
1. Tạo phòng ban và chức vụ
2. Phân user vào phòng ban
3. Test chức năng ủy quyền
4. Đọc [API_INTEGRATION.md](API_INTEGRATION.md) để tích hợp với app khác

