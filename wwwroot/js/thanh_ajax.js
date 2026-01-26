$(document).ready(function () {
    // ==========================================
    // PHẦN 1: QUẢN LÝ SẢN PHẨM (Code cũ giữ nguyên)
    // ==========================================

    // 1.1 Upload hình ảnh qua Ajax
    $("#fileHinhAjax").change(function () {
        let formData = new FormData();
        let file = this.files[0];
        if (file) {
            formData.append("fHinh", file);
            $.ajax({
                url: "/HangHoa/UploadHinhAjax",
                type: "POST",
                data: formData,
                contentType: false,
                processData: false,
                success: function (res) {
                    if (res.success) {
                        $("#imgPreview").attr("src", "/images/" + res.fileName);
                        $("#hinhPath").val(res.fileName);
                    }
                }
            });
        }
    });

    // 1.2 Kiểm tra trùng tên Sản phẩm
    $("#txtTenHh").on("input", function () {
        let tenSp = $(this).val();
        if (tenSp.length < 3) return;
        $.ajax({
            url: "/HangHoa/KiemTraTenAjax",
            type: "GET",
            data: { tenSp: tenSp },
            success: function (res) {
                if (res.isExisted) {
                    $("#tenFeedback").text("⚠️ Tên sản phẩm đã tồn tại!").css("color", "red");
                } else {
                    $("#tenFeedback").text("✅ Tên hợp lệ.").css("color", "green");
                }
            }
        });
    });

    // ==========================================
    // PHẦN 2: TÀI KHOẢN (CODE MỚI THÊM VÀO)
    // ==========================================

    // 2.1 Kiểm tra trùng Email khi Đăng ký
    $("#InputEmail").on("blur", function () { // Sự kiện blur: khi người dùng nhập xong và click ra ngoài
        var email = $(this).val().trim();
        var feedback = $("#emailFeedback");
        var btn = $("#btnRegister");

        if (email.length < 5 || !email.includes("@")) {
            feedback.text("");
            return;
        }

        $.ajax({
            url: '/Account/CheckEmail', // Gọi Action vừa viết trong AccountController
            type: 'GET',
            data: { email: email },
            success: function (response) {
                if (response.isExists) {
                    // Nếu Email đã có trong DB
                    feedback.text("⚠️ Email này đã được đăng ký!").css("color", "red");
                    // Khóa nút đăng ký không cho bấm
                    btn.prop("disabled", true);
                } else {
                    // Nếu Email chưa có
                    feedback.text("✅ Bạn có thể sử dụng Email này.").css("color", "green");
                    // Mở lại nút đăng ký
                    btn.prop("disabled", false);
                }
            },
            error: function () {
                console.log("Lỗi kết nối kiểm tra Email");
            }
        });
    });
});