// ===== SEARCH BAR SCRIPT =====
const searchInput = document.getElementById('searchInput');
const clearBtn = document.getElementById('clearBtn');
const searchBtn = document.getElementById('searchBtn');
const suggestions = document.getElementById('suggestions');
const categoryFilter = document.getElementById('categoryFilter');
const priceSort = document.getElementById('priceSort');

let debounceTimer;

// Format giá VND
function formatPrice(price) {
    return '$' + new Intl.NumberFormat('en-US').format(price);
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
    filterProducts();
});

priceSort.addEventListener('change', function() {
    filterProducts();
});

// Xóa nội dung
clearBtn.addEventListener('click', function() {
    searchInput.value = '';
    categoryFilter.value = '';
    priceSort.value = '';
    clearBtn.style.display = 'none';
    suggestions.style.display = 'none';
    searchInput.focus();
    filterProducts();
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

    // Gọi API ASP.NET Core
    fetch(`/Shop/SearchSuggestions?term=${encodeURIComponent(query)}`)
        .then(response => response.json())
        .then(data => {
            displaySuggestions(data, query);
        })
        .catch(error => {
            console.error('Error:', error);
            suggestions.innerHTML = '<div class="loading">Lỗi kết nối</div>';
        });
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
            <div class="suggestion-item" onclick='selectProduct(${product.id}, "${product.name.replace(/'/g, "\\'")}")'>
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
function selectProduct(productId, productName) {
    // Chuyển đến trang chi tiết sản phẩm
    window.location.href = `/Shop/Details/${productId}`;
}

// Tìm kiếm khi nhấn nút
searchBtn.addEventListener('click', function() {
    const query = searchInput.value.trim();
    if (query) {
        suggestions.style.display = 'none';
        
        // Chuyển đến trang kết quả tìm kiếm
        const params = new URLSearchParams({
            search: query,
            category: categoryFilter.value,
            sort: priceSort.value
        });
        window.location.href = `/Shop?${params.toString()}`;
    }
});

// Enter để tìm kiếm
searchInput.addEventListener('keypress', function(e) {
    if (e.key === 'Enter') {
        searchBtn.click();
    }
});

// Lọc sản phẩm trên trang hiện tại (không reload)
function filterProducts() {
    const searchTerm = searchInput.value.toLowerCase();
    const selectedCategory = categoryFilter.value;
    const selectedSort = priceSort.value;
    const productCards = document.querySelectorAll('.product-card');

    let visibleProducts = Array.from(productCards);

    // Lọc theo tìm kiếm
    if (searchTerm) {
        visibleProducts = visibleProducts.filter(card => {
            const name = card.dataset.name;
            return name.includes(searchTerm);
        });
    }

    // Lọc theo danh mục
    if (selectedCategory) {
        visibleProducts = visibleProducts.filter(card => {
            return card.dataset.category === selectedCategory;
        });
    }

    // Ẩn/hiện sản phẩm
    productCards.forEach(card => {
        if (visibleProducts.includes(card)) {
            card.style.display = 'block';
        } else {
            card.style.display = 'none';
        }
    });

    // Sắp xếp
    if (selectedSort && visibleProducts.length > 0) {
        const container = document.getElementById('productList');
        visibleProducts.sort((a, b) => {
            const priceA = parseFloat(a.dataset.price) || 0;
            const priceB = parseFloat(b.dataset.price) || 0;

            if (selectedSort === 'asc') return priceA - priceB;
            if (selectedSort === 'desc') return priceB - priceA;
            return 0;
        });

        visibleProducts.forEach(card => container.appendChild(card));
    }
}