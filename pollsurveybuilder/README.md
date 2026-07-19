# Poll & Survey Builder (Topic 01 — AMD201)

Dự án **Poll & Survey Builder** (tương tự Strawpoll / Slido) được thiết kế và phát triển theo kiến trúc **Clean Architecture** sử dụng **ASP.NET Core (.NET 10)**, **SignalR WebSocket**, **Redis Cache**, **SQL Server Database**, **Docker / Docker Compose**, và giao diện Web SPA thời gian thực với biểu đồ **Chart.js**.

Dự án sẵn sàng khởi chạy trực tiếp trên **Visual Studio** thông qua tệp giải pháp **`PollSurveyBuilder.slnx`** hoặc **`PollSurveyBuilder.sln`**.

---

## 🏛️ Kiến Trúc Hệ Thống (Clean Architecture)

Dự án được phân chia thành 6 dự án thành phần theo nguyên lý Clean Architecture:

```
pollsurveybuilder/
├── PollSurveyBuilder.Domain/              # Lớp Domain: Chứa Entities (Poll, PollOption, Vote, QnAQuestion), Enums
├── PollSurveyBuilder.Application/         # Lớp Application: DTOs, Interfaces, Service Logic, Repositories Contracts
├── PollSurveyBuilder.Infrastructure/      # Lớp Infrastructure: DbContext, EF Core Repositories, Redis Cache, SignalR Proxy
├── PollSurveyBuilder.Common/              # Lớp Common: ShortCodeGenerator, AppConstants, CacheKeys
├── PollSurveyBuilder.API/                 # Lớp API: Controllers, SignalR Hub, Scalar OpenAPI, Exception Middleware
├── PollSurveyBuilder.Web/                 # Lớp Web UI: SPA Frontend (HTML5/CSS3/JS, Chart.js, Bootstrap 5, SignalR JS)
├── PollSurveyBuilder.Tests/               # Lớp Tests: xUnit & FluentAssertions Unit Tests
├── docker-compose.yml                     # Docker Compose Orchestration (API, Web, SQL Server, Redis)
├── Dockerfile                             # Multi-stage Docker build
└── PollSurveyBuilder.slnx                 # File giải pháp Visual Studio
```

---

## ✨ Danh Sách Tính Năng Theo Tiêu Chí Đánh Giá

### 🟢 1. Cốt Lõi (Pass Grade)
- **Tạo Poll Nhanh**: Người dùng nhập câu hỏi và tùy chọn trả lời -> nhận ngay mã ngắn rút gọn độc nhất (`7fGh2`).
- **Bỏ Phiếu Đơn Giản**: Bất kỳ ai có liên kết đều có thể mở trang bỏ phiếu và gửi bình chọn.
- **Ràng Buộc Bỏ Phiếu 1 Lần**: Kiểm tra và chặn bỏ phiếu lặp lại thông qua Voter Token (Session Cookie / Browser Fingerprint + Redis Cache check).
- **Kết Quả Live Thời Gian Thực**: Trang kết quả cập nhật số liệu và biểu đồ thanh động ngay lập tức qua **SignalR WebSocket** mà không cần reload lại trang.
- **Đóng Poll**: Người tạo có thể chủ động đóng poll để ngừng nhận bình chọn mới.
- **RESTful API**: `POST /api/polls`, `GET /api/polls/{code}`, `POST /api/polls/{code}/vote`, `GET /api/polls/{code}/results`.

### 🟡 2. Bổ Sung Bằng Khen (Merit Grade)
- **Thời Gian Hết Hạn (Poll Expiry)**: Đặt thời gian tự động hết hạn cho poll. Khi quá hạn, hệ thống tự khóa và hiển thị thông báo kết thúc.
- **Đa Dạng Loại Câu Hỏi**:
  - `MultipleChoice`: Trắc nghiệm tùy chỉnh.
  - `YesNo`: Có / Không.
  - `Rating`: Thang điểm 1 đến 5 sao.
  - `OpenText`: Phản hồi văn bản tự do.

### 🔴 3. Bổ Sung Xuất Sắc (Distinction Grade)
- **Bảng Điều Khiển Phân Tích (Analytics Dashboard)**:
  - Biểu đồ đường (Line Chart) xu hướng bỏ phiếu theo thời gian.
  - Thống kê phút bỏ phiếu cao điểm nhất (*Peak Voting Minute*).
  - Thống kê tùy chọn dẫn đầu (*Top Option*).
- **Hỏi & Đáp Ẩn Danh (Live Anonymous Q&A Mode)**:
  - Khán giả gửi câu hỏi văn bản ẩn danh song song với bình chọn.
  - Tính năng **Upvote** và **Ghim (Pin)** câu hỏi quan trọng thời gian thực qua SignalR WebSocket.

---

## 🛠️ Hướng Dẫn Cài Đặt & Khởi Chạy

### Cách 1: Chạy trực tiếp từ Visual Studio / .NET CLI (Khuyên dùng)

1. **Khởi chạy Redis bằng Docker** (Nếu sử dụng Redis cache):
   ```bash
   docker run --name pollsurvey-redis -p 6379:6379 -d redis:7.2-alpine
   ```

2. **Mở dự án trong Visual Studio**:
   - Mở file **`PollSurveyBuilder.slnx`** trong Visual Studio 2022 (hoặc `PollSurveyBuilder.sln`).
   - Đặt **`PollSurveyBuilder.API`** và **`PollSurveyBuilder.Web`** làm Multiple Startup Projects (hoặc chạy lệnh terminal).

3. **Chạy qua CLI**:
   ```bash
   # Terminal 1: API (Backend)
   cd PollSurveyBuilder.API
   dotnet run --urls http://localhost:5000

   # Terminal 2: Web UI (Frontend)
   cd PollSurveyBuilder.Web
   dotnet run --urls http://localhost:8080
   ```

4. **Truy cập ứng dụng**:
   - **Giao diện Web UI**: [http://localhost:8080](http://localhost:8080)
   - **Tài liệu API Scalar / OpenAPI**: [http://localhost:5000/scalar/v1](http://localhost:5000/scalar/v1)

---

### Cách 2: Chạy toàn bộ hệ thống bằng Docker Compose

Dịch vụ bao gồm 4 container: Web API, Web UI, SQL Server 2022, và Redis.

```bash
docker-compose up --build
```

- **Web UI App**: `http://localhost:8080`
- **REST API & SignalR Hub**: `http://localhost:5000`
- **Scalar API Docs**: `http://localhost:5000/scalar/v1`

---

## 🧪 Chạy Kiểm Thử Đơn Vị (Unit Tests)

Dự án bao gồm bộ kiểm thử tự động xUnit bao phủ logic nghiệp vụ cốt lõi:

```bash
dotnet test PollSurveyBuilder.Tests/PollSurveyBuilder.Tests.csproj
```

---

## ⚙️ DevOps & CI/CD Pipeline

Dự án tích hợp quy trình **GitHub Actions CI/CD** tại tệp `.github/workflows/ci-cd.yml` tự động kiểm tra mã nguồn, chạy unit tests, và đóng gói Docker Images khi có bản đẩy mới (Push) lên nhánh `main`.
