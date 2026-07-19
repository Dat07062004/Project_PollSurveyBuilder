// Shared Navigation & Authentication Component
(function () {
    const authKey = "pollsurvey_auth_token";
    const userKey = "pollsurvey_user_info";

    window.PollAuth = {
        getToken: () => localStorage.getItem(authKey),
        getUser: () => {
            const u = localStorage.getItem(userKey);
            return u ? JSON.parse(u) : null;
        },
        isLoggedIn: () => !!localStorage.getItem(authKey),
        setAuth: (token, user) => {
            localStorage.setItem(authKey, token);
            localStorage.setItem(userKey, JSON.stringify(user));
            window.location.reload();
        },
        logout: () => {
            localStorage.removeItem(authKey);
            localStorage.removeItem(userKey);
            window.location.href = "index.html";
        },
        getAuthHeaders: () => {
            const token = localStorage.getItem(authKey);
            return token ? { "Authorization": "Bearer " + token, "Content-Type": "application/json" } : { "Content-Type": "application/json" };
        }
    };

    document.addEventListener("DOMContentLoaded", () => {
        renderNavbar();
        renderAuthModal();
        bindAuthEvents();
    });

    function renderNavbar() {
        const navContainer = document.getElementById("main-nav");
        if (!navContainer) return;

        const user = window.PollAuth.getUser();
        const activePath = window.location.pathname.split("/").pop() || "index.html";

        navContainer.innerHTML = `
            <nav class="navbar navbar-expand-lg navbar-dark bg-primary shadow-sm mb-4">
                <div class="container">
                    <a class="navbar-brand fw-bold d-flex align-items-center gap-2" href="index.html">
                        <i class="bi bi-bar-chart-line-fill text-warning fs-4"></i>
                        <span>Poll & Survey Builder</span>
                    </a>
                    <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarContent">
                        <span class="navbar-toggler-icon"></span>
                    </button>
                    <div class="collapse navbar-collapse" id="navbarContent">
                        <ul class="navbar-nav me-auto mb-2 mb-lg-0">
                            <li class="nav-item">
                                <a class="nav-link ${activePath === 'index.html' ? 'active fw-bold' : ''}" href="index.html">
                                    <i class="bi bi-plus-circle me-1"></i>Tạo thăm dò
                                </a>
                            </li>
                            <li class="nav-item">
                                <a class="nav-link ${activePath === 'mypolls.html' ? 'active fw-bold' : ''}" href="mypolls.html" id="nav-mypolls-link">
                                    <i class="bi bi-collection-fill me-1"></i>Danh sách các cuộc thăm dò
                                </a>
                            </li>
                        </ul>
                        <div class="d-flex align-items-center gap-2">
                            ${user ? `
                                <div class="dropdown">
                                    <button class="btn btn-light dropdown-toggle d-flex align-items-center gap-2 fw-semibold shadow-sm" type="button" id="userMenu" data-bs-toggle="dropdown">
                                        <div class="rounded-circle bg-warning text-dark d-flex align-items-center justify-content-center fw-bold" style="width:32px; height:32px;">
                                            ${user.username.charAt(0).toUpperCase()}
                                        </div>
                                        <span>${user.username}</span>
                                    </button>
                                    <ul class="dropdown-menu dropdown-menu-end shadow">
                                        <li><span class="dropdown-item-text text-muted small"><i class="bi bi-envelope me-1"></i>${user.email}</span></li>
                                        <li><hr class="dropdown-divider"></li>
                                        <li><a class="dropdown-item" href="mypolls.html"><i class="bi bi-list-task me-2"></i>Polls của tôi</a></li>
                                        <li><button class="dropdown-item text-danger" id="btn-logout"><i class="bi bi-box-arrow-right me-2"></i>Đăng xuất</button></li>
                                    </ul>
                                </div>
                            ` : `
                                <button class="btn btn-warning fw-bold text-dark shadow-sm px-3" data-bs-toggle="modal" data-bs-target="#authModal">
                                    <i class="bi bi-person-fill me-1"></i>Đăng nhập / Đăng ký
                                </button>
                            `}
                        </div>
                    </div>
                </div>
            </nav>
        `;

        const btnLogout = document.getElementById("btn-logout");
        if (btnLogout) {
            btnLogout.addEventListener("click", () => window.PollAuth.logout());
        }

        const myPollsLink = document.getElementById("nav-mypolls-link");
        if (myPollsLink) {
            myPollsLink.addEventListener("click", (e) => {
                if (!window.PollAuth.isLoggedIn()) {
                    e.preventDefault();
                    showAuthRequiredModal("Vui lòng đăng nhập để xem danh sách các cuộc thăm dò của bạn.");
                }
            });
        }
    }

    function renderAuthModal() {
        if (document.getElementById("authModal")) return;

        const modalDiv = document.createElement("div");
        modalDiv.innerHTML = `
            <div class="modal fade" id="authModal" tabindex="-1" aria-hidden="true">
                <div class="modal-dialog modal-dialog-centered">
                    <div class="modal-content border-0 shadow-lg rounded-4">
                        <div class="modal-header border-0 bg-primary text-white rounded-top-4">
                            <h5 class="modal-title fw-bold" id="authModalLabel">
                                <i class="bi bi-shield-lock-fill me-2"></i>Tài khoản Poll & Survey
                            </h5>
                            <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body p-4">
                            <div id="auth-alert" class="alert alert-danger d-none"></div>

                            <ul class="nav nav-pills nav-justified mb-4" id="authTab" role="tablist">
                                <li class="nav-item">
                                    <button class="nav-link active fw-bold" id="login-tab" data-bs-toggle="tab" data-bs-target="#login-pane">
                                        <i class="bi bi-box-arrow-in-right me-1"></i>Đăng nhập
                                    </button>
                                </li>
                                <li class="nav-item">
                                    <button class="nav-link fw-bold" id="register-tab" data-bs-toggle="tab" data-bs-target="#register-pane">
                                        <i class="bi bi-person-plus-fill me-1"></i>Đăng ký
                                    </button>
                                </li>
                            </ul>

                            <div class="tab-content" id="authTabContent">
                                <!-- Tab Đăng nhập -->
                                <div class="tab-pane fade show active" id="login-pane">
                                    <form id="form-login">
                                        <div class="mb-3">
                                            <label class="form-label fw-semibold">Gmail hoặc Tên đăng nhập</label>
                                            <input type="text" id="login-username" class="form-control form-control-lg" placeholder="nhap@gmail.com hoặc username" required>
                                        </div>
                                        <div class="mb-3">
                                            <label class="form-label fw-semibold">Mật khẩu</label>
                                            <input type="password" id="login-password" class="form-control form-control-lg" placeholder="••••••••" required>
                                        </div>
                                        <button type="submit" class="btn btn-primary btn-lg w-100 fw-bold shadow-sm mb-3">
                                            Đăng nhập ngay
                                        </button>
                                    </form>

                                    <div class="text-center position-relative my-4">
                                        <hr>
                                        <span class="position-absolute top-50 start-50 translate-middle bg-white px-3 text-muted small">hoặc</span>
                                    </div>

                                    <button type="button" id="btn-google-login" class="btn btn-outline-danger btn-lg w-100 fw-bold d-flex align-items-center justify-content-center gap-2 shadow-sm">
                                        <i class="bi bi-google fs-5"></i>Đăng nhập bằng Gmail (Google)
                                    </button>
                                </div>

                                <!-- Tab Đăng ký -->
                                <div class="tab-pane fade" id="register-pane">
                                    <form id="form-register">
                                        <div class="mb-3">
                                            <label class="form-label fw-semibold">Gmail <span class="text-danger">*</span></label>
                                            <input type="email" id="reg-email" class="form-control" placeholder="vidu@gmail.com" required>
                                            <div class="form-text">Mỗi Gmail chỉ được sử dụng cho 1 tài khoản duy nhất.</div>
                                        </div>
                                        <div class="mb-3">
                                            <label class="form-label fw-semibold">Tên đăng nhập (Username) <span class="text-danger">*</span></label>
                                            <input type="text" id="reg-username" class="form-control" placeholder="nguyenvana" required>
                                        </div>
                                        <div class="mb-3">
                                            <label class="form-label fw-semibold">Mật khẩu <span class="text-danger">*</span></label>
                                            <input type="password" id="reg-password" class="form-control" placeholder="Tối thiểu 6 ký tự" required>
                                        </div>
                                        <div class="mb-3">
                                            <label class="form-label fw-semibold">Xác nhận Mật khẩu <span class="text-danger">*</span></label>
                                            <input type="password" id="reg-confirm" class="form-control" placeholder="Nhập lại mật khẩu" required>
                                        </div>
                                        <button type="submit" class="btn btn-success btn-lg w-100 fw-bold shadow-sm">
                                            Tạo tài khoản mới
                                        </button>
                                    </form>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        `;
        document.body.appendChild(modalDiv);
    }

    function bindAuthEvents() {
        const formLogin = document.getElementById("form-login");
        const formRegister = document.getElementById("form-register");
        const btnGoogle = document.getElementById("btn-google-login");
        const authAlert = document.getElementById("auth-alert");

        function showError(msg) {
            if (authAlert) {
                authAlert.innerText = msg;
                authAlert.classList.remove("d-none");
            }
        }

        async function parseResponse(res) {
            const text = await res.text();
            let data = {};
            try { data = text ? JSON.parse(text) : {}; } catch (e) { }
            return data;
        }

        if (formLogin) {
            formLogin.addEventListener("submit", async (e) => {
                e.preventDefault();
                authAlert.classList.add("d-none");
                const usernameOrEmail = document.getElementById("login-username").value.trim();
                const password = document.getElementById("login-password").value;

                try {
                    const res = await fetch("/api/auth/login", {
                        method: "POST",
                        headers: { "Content-Type": "application/json" },
                        body: JSON.stringify({ usernameOrEmail, password })
                    });
                    const data = await parseResponse(res);
                    if (!res.ok) throw new Error(data.message || `Đăng nhập thất bại (Mã lỗi ${res.status}). Vui lòng đảm bảo đã khởi chạy cả API và Web trong Visual Studio.`);

                    window.PollAuth.setAuth(data.token, { id: data.userId, username: data.username, email: data.email });
                } catch (err) {
                    showError(err.message);
                }
            });
        }

        if (formRegister) {
            formRegister.addEventListener("submit", async (e) => {
                e.preventDefault();
                authAlert.classList.add("d-none");
                const email = document.getElementById("reg-email").value.trim();
                const username = document.getElementById("reg-username").value.trim();
                const password = document.getElementById("reg-password").value;
                const confirmPassword = document.getElementById("reg-confirm").value;

                if (password !== confirmPassword) {
                    showError("Mật khẩu xác nhận không trùng khớp.");
                    return;
                }

                try {
                    const res = await fetch("/api/auth/register", {
                        method: "POST",
                        headers: { "Content-Type": "application/json" },
                        body: JSON.stringify({ email, username, password, confirmPassword })
                    });
                    const data = await parseResponse(res);
                    if (!res.ok) throw new Error(data.message || `Đăng ký thất bại (Mã lỗi ${res.status}). Vui lòng đảm bảo đã khởi chạy cả API và Web trong Visual Studio.`);

                    window.PollAuth.setAuth(data.token, { id: data.userId, username: data.username, email: data.email });
                } catch (err) {
                    showError(err.message);
                }
            });
        }

        if (btnGoogle) {
            btnGoogle.addEventListener("click", async () => {
                authAlert.classList.add("d-none");
                const emailPrompt = prompt("Nhập Gmail của bạn để đăng nhập nhanh bằng Google:", "user@gmail.com");
                if (!emailPrompt) return;

                if (!emailPrompt.includes("@") || (!emailPrompt.toLowerCase().endsWith("gmail.com") && !emailPrompt.includes("."))) {
                    showError("Vui lòng nhập định dạng Gmail hợp lệ.");
                    return;
                }

                try {
                    const res = await fetch("/api/auth/google", {
                        method: "POST",
                        headers: { "Content-Type": "application/json" },
                        body: JSON.stringify({ email: emailPrompt, name: emailPrompt.split("@")[0] })
                    });
                    const data = await parseResponse(res);
                    if (!res.ok) throw new Error(data.message || `Đăng nhập bằng Gmail thất bại (Mã lỗi ${res.status}).`);

                    window.PollAuth.setAuth(data.token, { id: data.userId, username: data.username, email: data.email });
                } catch (err) {
                    showError(err.message);
                }
            });
        }
    }

    window.showAuthRequiredModal = function (msg) {
        const authAlert = document.getElementById("auth-alert");
        if (authAlert) {
            authAlert.innerText = msg || "Vui lòng đăng nhập hoặc đăng ký tài khoản để tiếp tục.";
            authAlert.classList.remove("d-none");
        }
        const modalEl = document.getElementById("authModal");
        if (modalEl) {
            const bsModal = new bootstrap.Modal(modalEl);
            bsModal.show();
        }
    };
})();
