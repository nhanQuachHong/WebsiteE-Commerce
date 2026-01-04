// ===== SEARCH BAR SCRIPT =====
const searchInput = document.getElementById('searchInput');
const clearBtn = document.getElementById('clearBtn');
const searchBtn = document.getElementById('searchBtn');
const suggestions = document.getElementById('suggestions');
const categoryFilter = document.getElementById('categoryFilter');
const productTypeFilter = document.getElementById('productTypeFilter');
const priceSort = document.getElementById('priceSort');

let debounceTimer;

// Dữ liệu nội thất mẫu (mô phỏng database)
const furnitureData = [
    { name: 'Sofa Da Cao Cấp Milano', price: 15900000, category: 'living-room', type: 'sofa' },
    { name: 'Sofa Vải Bố Hiện Đại', price: 12500000, category: 'living-room', type: 'sofa' },
    { name: 'Bàn Ăn Gỗ Sồi 6 Ghế', price: 18900000, category: 'dining-room', type: 'table' },
    { name: 'Bàn Ăn Mặt Đá Marble', price: 25000000, category: 'dining-room', type: 'table' },
    { name: 'Giường Ngủ Gỗ Óc Chó', price: 22000000, category: 'bedroom', type: 'bed' },
    { name: 'Giường Bọc Nỉ Cao Cấp', price: 16500000, category: 'bedroom', type: 'bed' },
    { name: 'Tủ Quần Áo 3 Cánh', price: 9800000, category: 'bedroom', type: 'wardrobe' },
    { name: 'Tủ Quần Áo Cửa Lùa', price: 14200000, category: 'bedroom', type: 'wardrobe' },
    { name: 'Bàn Làm Việc Gỗ Công Nghiệp', price: 3500000, category: 'office', type: 'table' },
    { name: 'Bàn Giám Đốc Gỗ Tự Nhiên', price: 8900000, category: 'office', type: 'table' },
    { name: 'Ghế Văn Phòng Ergonomic', price: 2800000, category: 'office', type: 'chair' },
    { name: 'Ghế Gaming Pro Series', price: 4500000, category: 'office', type: 'chair' },
    { name: 'Kệ Tivi Gỗ Hiện Đại', price: 5200000, category: 'living-room', type: 'shelf' },
    { name: 'Kệ Sách Đứng 5 Tầng', price: 3800000, category: 'office', type: 'shelf' },
    { name: 'Tủ Bếp Gỗ Acrylic', price: 32000000, category: 'kitchen', type: 'cabinet' },
    { name: 'Bàn Bar Mini Hiện Đại', price: 4200000, category: 'kitchen', type: 'table' },
    { name: 'Ghế Ăn Bọc Nỉ Cao Cấp', price: 1800000, category: 'dining-room', type: 'chair' },
    { name: 'Gương Trang Trí Khung Vàng', price: 2500000, category: 'decoration', type: 'decoration' }
];

// Format giá VND
function formatPrice(price) {
    return new Intl.NumberFormat('vi-VN').format(price) + ' ₫';
}

// Hiện/ẩn nút xóa
searchInput.addEventListener('input', function() {
    clearBtn.style.display = this.value ? 'block' : 'none';
    
    clearTimeout(debounceTimer);
    debounceTimer = setTimeout(() => {
        fetchSuggestions(this.value);
    }, 300);
});

// Lọc khi thay đổi filter
categoryFilter.addEventListener('change', function() {
    if (searchInput.value) {
        fetchSuggestions(searchInput.value);
    }
});

productTypeFilter.addEventListener('change', function() {
    if (searchInput.value) {
        fetchSuggestions(searchInput.value);
    }
});

priceSort.addEventListener('change', function() {
    if (searchInput.value) {
        fetchSuggestions(searchInput.value);
    }
});

// Xóa nội dung
clearBtn.addEventListener('click', function() {
    searchInput.value = '';
    clearBtn.style.display = 'none';
    suggestions.style.display = 'none';
    searchInput.focus();
});

// Ẩn suggestions khi click bên ngoài
document.addEventListener('click', function(e) {
    if (!e.target.closest('.search-container')) {
        suggestions.style.display = 'none';
    }
});

// Focus vào input hiện lại suggestions
searchInput.addEventListener('focus', function() {
    if (this.value && suggestions.innerHTML) {
        suggestions.style.display = 'block';
    }
});

// Hàm gọi AJAX để lấy gợi ý sản phẩm
function fetchSuggestions(query) {
    if (!query.trim()) {
        suggestions.style.display = 'none';
        return;
    }

    suggestions.innerHTML = '<div class="loading">Đang tìm kiếm...</div>';
    suggestions.style.display = 'block';

    // ===== KHI KẾT NỐI VỚI ASP.NET, THAY ĐOẠN setTimeout BÊN DƯỚI BẰNG ĐOẠN NÀY: =====
    /*
    const params = new URLSearchParams({
        searchString: query,
        category: categoryFilter.value,
        productType: productTypeFilter.value,
        sort: priceSort.value
    });
    
    fetch(`/Furniture/SearchSuggestions?${params}`)
        .then(response => response.json())
        .then(data => {
            displaySuggestions(data, query);
        })
        .catch(error => {
            console.error('Error:', error);
            suggestions.innerHTML = '<div class="loading">Lỗi kết nối</div>';
        });
    */

    // Mô phỏng lọc và sắp xếp (XÓA PHẦN NÀY KHI DÙNG API THẬT)
    setTimeout(() => {
        let results = furnitureData.filter(item => 
            item.name.toLowerCase().includes(query.toLowerCase())
        );

        // Lọc theo category
        if (categoryFilter.value) {
            results = results.filter(p => p.category === categoryFilter.value);
        }

        // Lọc theo product type
        if (productTypeFilter.value) {
            results = results.filter(p => p.type === productTypeFilter.value);
        }

        // Sắp xếp
        if (priceSort.value === 'asc') {
            results.sort((a, b) => a.price - b.price);
        } else if (priceSort.value === 'desc') {
            results.sort((a, b) => b.price - a.price);
        }
        
        displaySuggestions(results, query);
    }, 300);
}

// Hiển thị gợi ý
function displaySuggestions(results, query) {
    if (results.length === 0) {
        suggestions.innerHTML = '<div class="loading">Không tìm thấy sản phẩm</div>';
        return;
    }

    const html = results.map(product => {
        const regex = new RegExp(`(${query})`, 'gi');
        const highlighted = product.name.replace(regex, '<span class="highlight">$1</span>');
        
        return `
            <div class="suggestion-item" onclick='selectProduct(${JSON.stringify(product)})'>
                <span class="suggestion-icon">🪑</span>
                <span class="suggestion-text">
                    ${highlighted}
                    <div style="font-size: 12px; color: #8B4513; margin-top: 2px; font-weight: 600;">
                        ${formatPrice(product.price)}
                    </div>
                </span>
            </div>
        `;
    }).join('');

    suggestions.innerHTML = html;
    suggestions.style.display = 'block';
}

// Chọn sản phẩm từ gợi ý
function selectProduct(product) {
    searchInput.value = product.name;
    suggestions.style.display = 'none';
    clearBtn.style.display = 'block';
    console.log('Đã chọn:', product);
    // Có thể redirect đến trang chi tiết sản phẩm hoặc làm gì đó với sản phẩm
}

// Tìm kiếm khi nhấn nút
searchBtn.addEventListener('click', function() {
    const query = searchInput.value.trim();
    if (query) {
        console.log('Tìm kiếm:', {
            query: query,
            category: categoryFilter.value,
            productType: productTypeFilter.value,
            sort: priceSort.value
        });
        suggestions.style.display = 'none';
        
        // Chuyển đến trang kết quả tìm kiếm (nếu dùng ASP.NET)
        // window.location.href = `/Furniture/Index?searchString=${encodeURIComponent(query)}&category=${categoryFilter.value}&productType=${productTypeFilter.value}&sort=${priceSort.value}`;
        
        // Hoặc hiển thị alert (để test)
        alert('Tìm kiếm: ' + query);
    }
});

// Enter để tìm kiếm
searchInput.addEventListener('keypress', function(e) {
    if (e.key === 'Enter') {
        searchBtn.click();
    }
});