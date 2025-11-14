# Hướng dẫn sử dụng Authentication Service

## Bước 1: Kiểm tra containers đang chạy

Mở PowerShell và chạy:
```bash
docker ps
```

Bạn sẽ thấy 3 containers đang chạy:
- `auth-db` (SQL Server)
- `auth-api` (Backend)
- `auth-frontend` (Frontend)

## Bước 2: Truy cập ứng dụng

### Option 1: Truy cập Frontend (Giao diện Web)
Mở trình duyệt và vào:
```
http://localhost:3000
```

### Option 2: Truy cập Swagger API (Để test API)
Mở trình duyệt và vào:
```
http://localhost:5000/swagger
```

## Bước 3: Tạo User đầu tiên

Vì chưa có user nào, bạn cần tạo user đầu tiên qua Swagger:

### Cách 1: Qua Swagger UI (Dễ nhất)

1. Truy cập: http://localhost:5000/swagger
2. Tìm endpoint `POST /api/users/local`
3. Click vào endpoint đó
4. Click nút **"Try it out"**
5. Nhập thông tin:
```json
{
  "username": "admin",
  "password": "Admin123!",
  "email": "admin@example.com",
  "fullName": "Administrator"
}
```
6. Click **"Execute"**
7. Nếu thành công, bạn sẽ thấy response 200 với thông tin user

**Lưu ý:** Nếu bị lỗi 401 (Unauthorized), bạn cần tạm thời comment dòng `[Authorize]` trong file `AuthenticationService/Controllers/UsersController.cs` dòng 13.

### Cách 2: Qua cURL (Command line)

Mở PowerShell và chạy:
```powershell
curl -X POST http://localhost:5000/api/users/local `
  -H "Content-Type: application/json" `
  -d '{\"username\":\"admin\",\"password\":\"Admin123!\",\"email\":\"admin@example.com\",\"fullName\":\"Administrator\"}'
```

### Cách 3: Tạm thời bỏ [Authorize] để tạo user

1. Mở file `AuthenticationService/Controllers/UsersController.cs`
2. Tìm dòng 13: `[Authorize]`
3. Comment lại: `// [Authorize]`
4. Restart container: `docker-compose restart api`
5. Tạo user qua Swagger
6. Sau đó uncomment lại `[Authorize]` và restart

## Bước 4: Đăng nhập vào Frontend

1. Truy cập: http://localhost:3000
2. Bạn sẽ thấy trang đăng nhập
3. Nhập thông tin:
   - **Username:** `admin`
   - **Password:** `Admin123!`
   - **Check:** "Đăng nhập bằng tài khoản local"
4. Click **"Đăng nhập"**

## Bước 5: Sử dụng ứng dụng

Sau khi đăng nhập, bạn có thể:

1. **Dashboard** - Xem thông tin user và ủy quyền
2. **Người dùng** - Quản lý users
3. **Phòng ban** - Quản lý phòng ban và chức vụ
4. **Ủy quyền** - Tạo và quản lý ủy quyền

## Các lệnh hữu ích

### Xem logs
```bash
# Xem logs của tất cả services
docker-compose logs -f

# Xem logs của một service cụ thể
docker logs auth-api -f
docker logs auth-db -f
docker logs auth-frontend -f
```

### Restart services
```bash
# Restart tất cả
docker-compose restart

# Restart một service
docker-compose restart api
```

### Dừng ứng dụng
```bash
docker-compose down
```

### Khởi động lại
```bash
docker-compose up -d
```

## Troubleshooting

### Không truy cập được Frontend
- Kiểm tra container: `docker ps`
- Xem logs: `docker logs auth-frontend`

### Không truy cập được API
- Kiểm tra container: `docker ps`
- Xem logs: `docker logs auth-api`
- Kiểm tra Swagger: http://localhost:5000/swagger

### Lỗi đăng nhập
- Kiểm tra user đã được tạo chưa
- Kiểm tra password có đúng không
- Xem logs API: `docker logs auth-api`

