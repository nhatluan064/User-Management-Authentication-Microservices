# Authentication Service

Dịch vụ xác thực tập trung cho các ứng dụng, hỗ trợ Microsoft Active Directory, Microsoft 365, và Local Users.

## Tính năng

- ✅ Xác thực với Microsoft Active Directory DC và Microsoft 365 Entity ID
- ✅ Quản lý user và phòng ban cùng các chức vụ trong phòng ban
- ✅ Tạo local user (user không thuộc domain)
- ✅ Phân phòng ban cho user đang đăng nhập
- ✅ Chức năng ủy quyền cho người khác thay thế vai trò trong một số ngày nhất định
- ✅ Xuất PDF giấy ủy quyền và gửi email sau khi ủy quyền
- ✅ RESTful API để các ứng dụng khác có thể tích hợp
- ✅ JWT Token-based authentication
- ✅ Frontend web với React và Material-UI

## Công nghệ sử dụng

### Backend
- .NET 8.0
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- JWT Authentication
- Microsoft Graph API (cho Microsoft 365)
- System.DirectoryServices (cho Active Directory)
- iTextSharp (cho PDF generation)
- MailKit (cho email)

### Frontend
- React 18
- TypeScript
- Vite
- Material-UI (MUI)
- React Router
- Axios

### Infrastructure
- Docker & Docker Compose
- SQL Server (containerized)

## Cấu trúc dự án

```
.
├── AuthenticationService/     # Backend .NET Core API
│   ├── Controllers/          # API Controllers
│   ├── Services/             # Business logic services
│   ├── Repositories/         # Data access layer
│   ├── Models/               # Domain models và DTOs
│   ├── Data/                 # DbContext
│   └── Dockerfile
├── frontend/                 # React Frontend
│   ├── src/
│   │   ├── pages/           # React pages
│   │   ├── components/      # React components
│   │   ├── contexts/        # React contexts
│   │   ├── services/        # API services
│   │   └── types/           # TypeScript types
│   └── Dockerfile
└── docker-compose.yml        # Docker Compose configuration
```

## Cài đặt và chạy

### Yêu cầu
- Docker và Docker Compose
- Hoặc .NET 8.0 SDK và Node.js 20+ (để chạy local)

### Chạy với Docker (Khuyến nghị)

1. Clone repository hoặc copy toàn bộ code

2. Cấu hình `appsettings.json` trong `AuthenticationService/`:
   ```json
   {
     "MicrosoftAuth": {
       "TenantId": "your-tenant-id",
       "ClientId": "your-client-id",
       "ClientSecret": "your-client-secret",
       "Domain": "your-domain.com"
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

3. Chạy Docker Compose:
   ```bash
   docker-compose up -d
   ```

4. Truy cập:
   - Frontend: http://localhost:3000
   - Backend API: http://localhost:5000
   - Swagger UI: http://localhost:5000/swagger

5. Khởi tạo database (chạy migration):
   ```bash
   docker exec -it auth-api dotnet ef database update
   ```
   Hoặc nếu chưa có migration:
   ```bash
   docker exec -it auth-api dotnet ef migrations add InitialCreate
   docker exec -it auth-api dotnet ef database update
   ```

### Chạy local (Development)

#### Backend
```bash
cd AuthenticationService
dotnet restore
dotnet ef database update  # Nếu chưa có database
dotnet run
```

#### Frontend
```bash
cd frontend
npm install
npm run dev
```

## API Endpoints

### Authentication
- `POST /api/auth/login` - Đăng nhập
- `POST /api/auth/validate` - Validate token
- `GET /api/auth/user-info` - Lấy thông tin user hiện tại
- `POST /api/auth/logout` - Đăng xuất

### Users
- `GET /api/users` - Lấy danh sách users
- `GET /api/users/{id}` - Lấy thông tin user
- `POST /api/users/local` - Tạo local user
- `PUT /api/users/{id}` - Cập nhật user
- `DELETE /api/users/{id}` - Xóa user
- `POST /api/users/{id}/departments/{departmentId}` - Phân user vào phòng ban

### Departments
- `GET /api/departments` - Lấy danh sách phòng ban
- `GET /api/departments/{id}` - Lấy thông tin phòng ban
- `POST /api/departments` - Tạo phòng ban
- `PUT /api/departments/{id}` - Cập nhật phòng ban
- `DELETE /api/departments/{id}` - Xóa phòng ban
- `POST /api/departments/{id}/roles` - Tạo chức vụ
- `GET /api/departments/{id}/roles` - Lấy danh sách chức vụ

### Delegations
- `GET /api/delegations/my-delegations` - Lấy danh sách ủy quyền
- `POST /api/delegations` - Tạo ủy quyền
- `POST /api/delegations/{id}/cancel` - Hủy ủy quyền
- `GET /api/delegations/{id}/pdf` - Tải PDF giấy ủy quyền

## Tích hợp với ứng dụng khác

### 1. Đăng nhập và lấy JWT Token

```javascript
const response = await fetch('http://localhost:5000/api/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    username: 'user@domain.com',
    password: 'password',
    useLocalAuth: false  // false cho AD, true cho local user
  })
});

const { token, userInfo, delegationInfo } = await response.json();
```

### 2. Sử dụng Token trong các request

```javascript
const response = await fetch('http://localhost:5000/api/auth/user-info', {
  headers: {
    'Authorization': `Bearer ${token}`
  }
});

const userInfo = await response.json();
```

### 3. Validate Token từ ứng dụng khác

```javascript
const response = await fetch('http://localhost:5000/api/auth/validate', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ token: 'your-jwt-token' })
});

const userInfo = await response.json();
```

### 4. Lấy thông tin ủy quyền (nếu có)

Token JWT chứa các claims về delegation:
- `delegationId`: ID của delegation
- `delegatorId`: ID của người ủy quyền
- `delegatorName`: Tên người ủy quyền

Bạn có thể decode JWT token để lấy thông tin này.

## Cấu hình

### Microsoft Active Directory
Cần cấu hình domain trong `appsettings.json`:
```json
"MicrosoftAuth": {
  "Domain": "your-domain.com"
}
```

### Microsoft 365
Cần tạo Azure AD App Registration và cấu hình:
```json
"MicrosoftAuth": {
  "TenantId": "your-tenant-id",
  "ClientId": "your-client-id",
  "ClientSecret": "your-client-secret"
}
```

### Email
Cấu hình SMTP server để gửi email:
```json
"EmailSettings": {
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": 587,
  "SmtpUsername": "your-email@gmail.com",
  "SmtpPassword": "your-app-password"
}
```

## Lưu ý bảo mật

1. **JWT Secret Key**: Thay đổi `SecretKey` trong `appsettings.json` bằng một key mạnh (ít nhất 32 ký tự)
2. **Database Password**: Thay đổi password SQL Server trong `docker-compose.yml` và `appsettings.json`
3. **HTTPS**: Trong production, sử dụng HTTPS thay vì HTTP
4. **CORS**: Cấu hình CORS phù hợp với domain của bạn
5. **Email Credentials**: Không commit thông tin email vào git

## Troubleshooting

### Database connection error
- Kiểm tra SQL Server container đã chạy: `docker ps`
- Kiểm tra connection string trong `appsettings.json`
- Đảm bảo database đã được tạo: `dotnet ef database update`

### AD Authentication không hoạt động
- Kiểm tra domain configuration
- Đảm bảo server có thể kết nối đến AD domain controller
- Kiểm tra firewall rules

### Email không gửi được
- Kiểm tra SMTP credentials
- Với Gmail, cần sử dụng App Password thay vì mật khẩu thường
- Kiểm tra firewall/network rules

## License

MIT

