$(document).ready(function () {
    // 1. XỬ LÝ UPLOAD ẢNH QUA AJAX
    $("#fileHinhAjax").change(function () {
        let formData = new FormData();
        let file = this.files[0];

        // Kiểm tra nếu có file mới xử lý
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
                        // Hiển thị ảnh vừa upload lên khung xem trước
                        $("#imgPreview").attr("src", "/images/" + res.fileName);

                        // QUAN TRỌNG NHẤT: Gán tên file vào input ẩn để gửi về DB khi bấm Save
                        $("#hinhPath").val(res.fileName);

                        console.log("Đã cập nhật hinhPath: " + res.fileName);
                    } else {
                        alert("Lỗi upload ảnh!");
                    }
                },
                error: function () {
                    alert("Không thể kết nối đến server để upload ảnh!");
                }
            });
        }
    });

    // 2. KIỂM TRA TRÙNG TÊN QUA AJAX
    $("#txtTenHh").on("input", function () {
        let tenSp = $(this).val();
        if (tenSp.length < 3) return; // Chỉ kiểm tra khi nhập trên 3 ký tự

        $.ajax({
            url: "/HangHoa/KiemTraTenAjax",
            type: "GET",
            data: { tenSp: tenSp },
            success: function (res) {
                if (res.isExisted) {
                    $("#tenFeedback").text("⚠️ Tên này đã có trong hệ thống!").css("color", "red");
                    $("#btnSubmit").prop("disabled", true); // Khóa nút lưu nếu trùng tên
                } else {
                    $("#tenFeedback").text("✅ Tên hợp lệ.").css("color", "green");
                    $("#btnSubmit").prop("disabled", false); // Mở lại nút lưu
                }
            }
        });
    });
});