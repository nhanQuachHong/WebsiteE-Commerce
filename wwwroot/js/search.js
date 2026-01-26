document.addEventListener("DOMContentLoaded", function () {
    const searchInput = document.getElementById("searchInput");
    const clearBtn = document.getElementById("clearBtn");
    const searchBtn = document.getElementById("searchBtn");
    const categoryFilter = document.getElementById("categoryFilter");
    const priceSort = document.getElementById("priceSort");
    const productList = document.getElementById("productList");

    // Lấy danh sách tất cả thẻ sản phẩm
    const products = Array.from(document.querySelectorAll(".product-card"));

    // --- 1. HÀM LỌC SẢN PHẨM ---
    function filterProducts() {
        const searchText = searchInput.value.toLowerCase().trim();
        const selectedCategory = categoryFilter.value;

        products.forEach(card => {
            const name = card.getAttribute("data-name"); // Lấy tên từ HTML
            const category = card.getAttribute("data-category"); // Lấy mã loại

            // Kiểm tra điều kiện: (Tên chứa từ khóa) VÀ (Đúng loại hoặc chọn Tất cả)
            const matchesSearch = name.includes(searchText);
            const matchesCategory = selectedCategory === "" || category === selectedCategory;

            // Ẩn/Hiện sản phẩm
            if (matchesSearch && matchesCategory) {
                card.style.display = "block";
            } else {
                card.style.display = "none";
            }
        });

        // Sau khi lọc xong thì sắp xếp lại nếu đang chọn sort
        sortProducts();
    }

    // --- 2. HÀM SẮP XẾP SẢN PHẨM ---
    function sortProducts() {
        const sortValue = priceSort.value;

        // Chỉ sắp xếp các sản phẩm đang hiển thị (display != none)
        // Tuy nhiên để đơn giản, ta sắp xếp lại DOM của toàn bộ list

        let sortedProducts = [...products];

        if (sortValue === "asc") {
            sortedProducts.sort((a, b) => {
                return parseFloat(a.getAttribute("data-price")) - parseFloat(b.getAttribute("data-price"));
            });
        } else if (sortValue === "desc") {
            sortedProducts.sort((a, b) => {
                return parseFloat(b.getAttribute("data-price")) - parseFloat(a.getAttribute("data-price"));
            });
        }
        // Nếu chọn "newest" hoặc "mặc định", ta có thể reload lại thứ tự gốc (nếu cần xử lý phức tạp hơn)

        // Xóa danh sách cũ và thêm lại theo thứ tự mới
        productList.innerHTML = "";
        sortedProducts.forEach(product => productList.appendChild(product));

        // Gọi lại filter để ẩn những cái không khớp search
        // (Lưu ý: Logic này hơi vòng vo, nhưng đảm bảo DOM được sắp xếp đúng)
        const searchText = searchInput.value.toLowerCase().trim();
        const selectedCategory = categoryFilter.value;

        sortedProducts.forEach(card => {
            const name = card.getAttribute("data-name");
            const category = card.getAttribute("data-category");
            const matchesSearch = name.includes(searchText);
            const matchesCategory = selectedCategory === "" || category === selectedCategory;

            if (matchesSearch && matchesCategory) {
                card.style.display = "block";
            } else {
                card.style.display = "none";
            }
        });
    }

    // --- 3. GẮN SỰ KIỆN (EVENT LISTENERS) ---

    // Khi gõ phím tìm kiếm
    searchInput.addEventListener("input", function () {
        if (this.value.length > 0) clearBtn.style.display = "block";
        else clearBtn.style.display = "none";
        filterProducts();
    });

    // Nút xóa tìm kiếm
    clearBtn.addEventListener("click", function () {
        searchInput.value = "";
        this.style.display = "none";
        filterProducts();
    });

    // Thay đổi danh mục
    categoryFilter.addEventListener("change", filterProducts);

    // Thay đổi sắp xếp
    priceSort.addEventListener("change", sortProducts);
});