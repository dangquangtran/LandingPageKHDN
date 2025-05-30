# 🌐 KienlongBank - Landing Page Đăng Ký Tài Khoản Doanh Nghiệp

## 📝 Mô tả dự án

Đây là ứng dụng web giúp Khách hàng Doanh nghiệp (KHDN) đăng ký mở tài khoản tại Ngân hàng Kiên Long (KienlongBank) thông qua một trang landing page hiện đại và thân thiện. Ứng dụng cho phép nhập thông tin, upload tài liệu, xác thực người dùng và gửi email thông báo thành công.

> 📅 **Thời gian phát triển:** 2 tuần  
> 👨‍💻 **Số lượng thành viên:** 2 thực tập sinh  
> 🧪 **Mục tiêu:** Đánh giá kỹ năng thực tế về lập trình, teamwork, UI/UX và bảo mật

---

## 🛠️ Công nghệ sử dụng

| Thành phần        | Công nghệ                                |
|-------------------|------------------------------------------|
| Ngôn ngữ           | C#                                       |
| Backend            | ASP.NET MVC (.NET 6 trở lên)             |
| Frontend           | Razor View, Bootstrap 5, jQuery          |
| CSDL               | SQL Server Express / LocalDB             |
| Gửi email          | SMTP (MailKit hoặc Gmail)                |
| Bảo mật            | Google reCAPTCHA v2, Validate form, lọc file |
| Quản lý mã nguồn   | Git + GitHub (hoặc GitLab)               |
| Ghi log            | ILogger (file text hoặc console log)     |

---

## 🚀 Tính năng nổi bật

### 📄 1. Landing Page
- Hiển thị thông tin dịch vụ mở tài khoản doanh nghiệp
- Thiết kế chuyên nghiệp theo phong cách ngân hàng
- Giao diện responsive (PC, máy tính bảng, điện thoại)

### 🧾 2. Form đăng ký tài khoản doanh nghiệp
#### ✔️ Thông tin cơ bản:
- Tên doanh nghiệp  
- Mã số thuế  
- Địa chỉ  
- Số điện thoại  
- Email  

#### ✔️ Người đại diện pháp luật:
- Họ tên  
- CMND/CCCD  
- Chức vụ  

#### 📎 Tài liệu đính kèm:
- Giấy phép đăng ký kinh doanh (PDF, PNG, JPG)  
- CMND/CCCD người đại diện pháp luật  

#### 🔒 Bảo mật:
- Validate dữ liệu đầu vào (client & server)
- Tích hợp Google reCAPTCHA v2 chống spam
- Hạn chế upload file nguy hiểm (.exe, .dll…)

#### 📧 Gửi email:
- Xác nhận đăng ký thành công qua email sau khi nộp form

### 🛡 3. Trang quản trị (Admin)
- Xem danh sách doanh nghiệp đã đăng ký
- Tìm kiếm theo tên doanh nghiệp hoặc mã số thuế
- Chỉ cho phép truy cập nội bộ (nội dung demo, không bảo mật nâng cao)

---

