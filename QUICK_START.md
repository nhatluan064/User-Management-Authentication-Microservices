# Quick Start Guide

Hướng dẫn nhanh để chạy Authentication Service.

## Yêu cầu

- Docker và Docker Compose đã cài đặt
- Hoặc .NET 8.0 SDK và Node.js 20+ (để chạy local)

## Chạy với Docker (Khuyến nghị)

### Bước 1: Cấu hình

Chỉnh sửa file `AuthenticationService/appsettings.json`:

```json
{
  "MicrosoftAuth": {
    "TenantId": "your-tenant-id",  // Để trống nếu không dùng M365
    "ClientId": "your-client-id",  // Để trống nếu không dùng M365
    "ClientSecret": "your-client-secret",  // Để trống nếu không dùng M365
    "Domain": "your-domain.com"  // Domain của AD
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "your-email@gmail.com",
    "FromName": "Authentication Service"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyForJWTTokenGenerationMustBeAtLeast32CharactersLong",
    "Issuer": "AuthenticationService",
    "Audience": "AuthenticationService",
    "ExpirationMinutes": 60
  }
}
```

### Bước 2: Chạy Docker Compose

```bash
docker-compose up -d
```

### Bước 3: Khởi tạo Database

Chờ vài giây để SQL Server khởi động, sau đó chạy:

```bash
# Tạo migration (nếu chưa có)
docker exec -it auth-api dotnet ef migrations add InitialCreate --project /src

# Apply migration
docker exec -it auth-api dotnet ef database update --project /src
```

### Bước 4: Truy cập

- **Frontend**: http://localhost:3000
- **Backend API**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger

## Chạy Local (Development)

### Backend

```bash
cd AuthenticationService

# Restore packages
dotnet restore

# Tạo migration (nếu chưa có)
dotnet ef migrations add InitialCreate

# Apply migration
dotnet ef database update

# Chạy
dotnet run
```

Backend sẽ chạy tại: http://localhost:5000

### Frontend

```bash
cd frontend

# Install dependencies
npm install

# Chạy dev server
npm run dev
```

Frontend sẽ chạy tại: http://localhost:3000

## Tạo user đầu tiên

### Cách 1: Qua Frontend

1. Truy cập http://localhost:3000
2. Đăng nhập với tài khoản AD (nếu đã cấu hình)
3. Vào mục "Người dùng" > "Tạo user local"

### Cách 2: Qua API

```bash
# Đăng nhập với AD user trước (hoặc tạo local user qua API nếu có quyền)
curl -X POST http://localhost:5000/api/users/local \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "username": "admin",
    "password": "Admin123!",
    "email": "admin@example.com",
    "fullName": "Administrator"
  }'
```

## Tạo phòng ban và chức vụ

1. Đăng nhập vào Frontend
2. Vào mục "Phòng ban"
3. Tạo phòng ban mới
4. Mở rộng phòng ban để thêm chức vụ

## Phân user vào phòng ban

1. Vào mục "Người dùng"
2. Tìm user cần phân
3. Sử dụng API để phân vào phòng ban:

```bash
curl -X POST http://localhost:5000/api/users/1/departments/1?roleId=1 \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## Tạo ủy quyền

1. Đăng nhập vào Frontend
2. Vào mục "Ủy quyền"
3. Click "Tạo ủy quyền"
4. Chọn người được ủy quyền, thời gian và lý do
5. Hệ thống sẽ tự động:
   - Tạo PDF giấy ủy quyền
   - Gửi email cho người được ủy quyền

## Troubleshooting

### Database không kết nối được

```bash
# Kiểm tra container
docker ps

# Xem logs
docker logs auth-db
docker logs auth-api

# Restart
docker-compose restart
```

### Migration lỗi

```bash
# Xóa migration cũ và tạo lại
docker exec -it auth-api dotnet ef migrations remove --project /src
docker exec -it auth-api dotnet ef migrations add InitialCreate --project /src
docker exec -it auth-api dotnet ef database update --project /src
```

### Frontend không kết nối được API

Kiểm tra `frontend/vite.config.ts`:
```typescript
proxy: {
  '/api': {
    target: 'http://localhost:5000',  // Đảm bảo đúng port
    changeOrigin: true
  }
}
```

## Next Steps

- Đọc [README.md](README.md) để biết thêm chi tiết
- Đọc [API_INTEGRATION.md](API_INTEGRATION.md) để tích hợp với app khác
- Cấu hình Microsoft AD/M365 nếu cần
- Cấu hình email server để gửi email ủy quyền

