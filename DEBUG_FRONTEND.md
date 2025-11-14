# Debug Frontend - Trang trắng

## Các bước kiểm tra

### 1. Mở Browser Console
- Nhấn **F12** hoặc **Ctrl+Shift+I**
- Vào tab **Console**
- Xem có lỗi màu đỏ không

### 2. Kiểm tra Network
- Vào tab **Network**
- Refresh trang (F5)
- Xem các request có thành công không (status 200)

### 3. Kiểm tra Elements
- Vào tab **Elements**
- Tìm thẻ `<div id="root">`
- Xem có nội dung gì bên trong không

## Các lỗi thường gặp

### Lỗi: "Failed to fetch" hoặc "Network Error"
**Nguyên nhân:** API không kết nối được

**Giải pháp:**
```bash
# Kiểm tra API đang chạy
curl http://localhost:5000/swagger

# Restart API
docker-compose restart api
```

### Lỗi: "Cannot read property of undefined"
**Nguyên nhân:** Code JavaScript có lỗi

**Giải pháp:** Xem chi tiết lỗi trong console và báo lại

### Trang vẫn trắng nhưng không có lỗi
**Nguyên nhân:** Có thể là vấn đề với routing

**Giải pháp:**
1. Thử truy cập trực tiếp: http://localhost:3000/login
2. Xem có hiển thị trang login không

## Test nhanh

Mở PowerShell và chạy:
```powershell
# Test API
curl http://localhost:5000/swagger

# Test Frontend
curl http://localhost:3000

# Xem logs
docker logs auth-frontend --tail 20
docker logs auth-api --tail 20
```

## Nếu vẫn không được

1. **Hard refresh:** Ctrl+Shift+R hoặc Ctrl+F5
2. **Xóa cache:** Ctrl+Shift+Delete
3. **Thử browser khác:** Chrome, Firefox, Edge
4. **Kiểm tra firewall:** Đảm bảo ports 3000 và 5000 không bị block


