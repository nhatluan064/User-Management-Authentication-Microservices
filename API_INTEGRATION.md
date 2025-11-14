# Hướng dẫn tích hợp Authentication Service với ứng dụng khác

Tài liệu này hướng dẫn cách tích hợp Authentication Service với các ứng dụng khác thông qua REST API.

## Tổng quan

Authentication Service cung cấp các API endpoints để:
- Xác thực người dùng (Microsoft AD, Microsoft 365, hoặc Local)
- Lấy thông tin người dùng
- Validate JWT tokens
- Lấy thông tin ủy quyền (nếu có)

## Base URL

- Development: `http://localhost:5000/api`
- Production: `https://your-domain.com/api`

## Authentication Flow

### 1. Đăng nhập và lấy JWT Token

**Endpoint:** `POST /auth/login`

**Request Body:**
```json
{
  "username": "user@domain.com",
  "password": "password",
  "useLocalAuth": false
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userInfo": {
    "id": 1,
    "username": "user@domain.com",
    "email": "user@domain.com",
    "fullName": "Nguyễn Văn A",
    "isLocalUser": false,
    "departments": [
      {
        "id": 1,
        "name": "Phòng IT",
        "code": "IT",
        "role": {
          "id": 1,
          "name": "Trưởng phòng"
        }
      }
    ]
  },
  "delegationInfo": {
    "delegationId": 5,
    "delegator": {
      "id": 2,
      "username": "manager@domain.com",
      "fullName": "Trần Văn B"
    },
    "startDate": "2024-01-01T00:00:00Z",
    "endDate": "2024-01-10T00:00:00Z",
    "reason": "Nghỉ phép"
  }
}
```

**Lưu ý:**
- `useLocalAuth: false` - Sử dụng Microsoft AD/M365
- `useLocalAuth: true` - Sử dụng local user
- `delegationInfo` chỉ có khi user đang có ủy quyền hoạt động

### 2. Validate Token

**Endpoint:** `POST /auth/validate`

**Request Body:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Response:**
```json
{
  "id": 1,
  "username": "user@domain.com",
  "email": "user@domain.com",
  "fullName": "Nguyễn Văn A",
  "isLocalUser": false,
  "departments": [...]
}
```

**Status Codes:**
- `200 OK` - Token hợp lệ
- `401 Unauthorized` - Token không hợp lệ hoặc đã hết hạn

### 3. Lấy thông tin user hiện tại

**Endpoint:** `GET /auth/user-info`

**Headers:**
```
Authorization: Bearer {token}
```

**Response:**
```json
{
  "id": 1,
  "username": "user@domain.com",
  "email": "user@domain.com",
  "fullName": "Nguyễn Văn A",
  "isLocalUser": false,
  "departments": [...]
}
```

## Sử dụng JWT Token

### Cấu trúc JWT Token

JWT token chứa các claims sau:

```json
{
  "sub": "1",  // User ID
  "name": "user@domain.com",
  "email": "user@domain.com",
  "fullName": "Nguyễn Văn A",
  "isLocalUser": "False",
  "department": ["1", "2"],  // Department IDs
  "role": ["1:1", "2:3"],  // "departmentId:roleId"
  "delegationId": "5",  // Nếu có ủy quyền
  "delegatorId": "2",
  "delegatorName": "Trần Văn B",
  "exp": 1234567890,
  "iss": "AuthenticationService",
  "aud": "AuthenticationService"
}
```

### Decode JWT Token (JavaScript)

```javascript
function parseJwt(token) {
  const base64Url = token.split('.')[1];
  const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
  const jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
    return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
  }).join(''));
  return JSON.parse(jsonPayload);
}

const token = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...';
const claims = parseJwt(token);
console.log('User ID:', claims.sub);
console.log('Departments:', claims.department);
console.log('Delegation:', claims.delegationId);
```

## Ví dụ tích hợp

### JavaScript/TypeScript (React/Vue/Angular)

```typescript
// api.ts
import axios from 'axios';

const API_BASE_URL = 'http://localhost:5000/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add token to requests
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Login
export const login = async (username: string, password: string, useLocalAuth: boolean) => {
  const response = await api.post('/auth/login', {
    username,
    password,
    useLocalAuth,
  });
  localStorage.setItem('token', response.data.token);
  return response.data;
};

// Get current user
export const getCurrentUser = async () => {
  const response = await api.get('/auth/user-info');
  return response.data;
};

// Validate token
export const validateToken = async (token: string) => {
  const response = await api.post('/auth/validate', { token });
  return response.data;
};
```

### C# (.NET)

```csharp
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "http://localhost:5000/api";

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<LoginResponse> LoginAsync(string username, string password, bool useLocalAuth)
    {
        var request = new
        {
            username,
            password,
            useLocalAuth
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_baseUrl}/auth/login", content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<LoginResponse>(responseJson);
    }

    public async Task<UserInfo> GetUserInfoAsync(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.GetAsync($"{_baseUrl}/auth/user-info");
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<UserInfo>(responseJson);
    }
}
```

### Python

```python
import requests
import json

class AuthService:
    def __init__(self, base_url="http://localhost:5000/api"):
        self.base_url = base_url
        self.token = None

    def login(self, username, password, use_local_auth=False):
        url = f"{self.base_url}/auth/login"
        data = {
            "username": username,
            "password": password,
            "useLocalAuth": use_local_auth
        }
        response = requests.post(url, json=data)
        response.raise_for_status()
        result = response.json()
        self.token = result["token"]
        return result

    def get_user_info(self):
        if not self.token:
            raise ValueError("Not logged in")
        
        url = f"{self.base_url}/auth/user-info"
        headers = {"Authorization": f"Bearer {self.token}"}
        response = requests.get(url, headers=headers)
        response.raise_for_status()
        return response.json()

    def validate_token(self, token):
        url = f"{self.base_url}/auth/validate"
        data = {"token": token}
        response = requests.post(url, json=data)
        if response.status_code == 200:
            return response.json()
        return None
```

## Xử lý ủy quyền (Delegation)

Khi user có ủy quyền, token sẽ chứa thông tin về người ủy quyền:

```javascript
const claims = parseJwt(token);

if (claims.delegationId) {
  console.log('User đang được ủy quyền bởi:', claims.delegatorName);
  console.log('Delegation ID:', claims.delegationId);
  
  // Trong ứng dụng của bạn, bạn có thể:
  // 1. Hiển thị thông tin ủy quyền cho user
  // 2. Áp dụng quyền của người ủy quyền
  // 3. Log các hành động với thông tin ủy quyền
}
```

## Error Handling

### 401 Unauthorized
Token không hợp lệ hoặc đã hết hạn. Cần đăng nhập lại.

```javascript
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);
```

### 400 Bad Request
Request không hợp lệ. Kiểm tra format của request body.

### 500 Internal Server Error
Lỗi server. Thử lại sau hoặc liên hệ admin.

## Best Practices

1. **Lưu trữ token an toàn**
   - Sử dụng `localStorage` cho web apps
   - Sử dụng secure storage cho mobile apps
   - Không commit token vào git

2. **Refresh token**
   - Token có thời hạn (mặc định 60 phút)
   - Implement auto-refresh trước khi token hết hạn
   - Hoặc yêu cầu user đăng nhập lại

3. **Validate token ở server-side**
   - Luôn validate token ở backend của ứng dụng
   - Không chỉ dựa vào client-side validation

4. **Error handling**
   - Xử lý tất cả các trường hợp lỗi
   - Hiển thị thông báo rõ ràng cho user

5. **Security**
   - Sử dụng HTTPS trong production
   - Không log token vào console
   - Implement CSRF protection nếu cần

## Testing

### Test với cURL

```bash
# Login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "user@domain.com",
    "password": "password",
    "useLocalAuth": false
  }'

# Get user info
curl -X GET http://localhost:5000/api/auth/user-info \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"

# Validate token
curl -X POST http://localhost:5000/api/auth/validate \
  -H "Content-Type: application/json" \
  -d '{"token": "YOUR_TOKEN_HERE"}'
```

## Support

Nếu có vấn đề khi tích hợp, vui lòng:
1. Kiểm tra Swagger UI: `http://localhost:5000/swagger`
2. Kiểm tra logs của Authentication Service
3. Liên hệ team phát triển

