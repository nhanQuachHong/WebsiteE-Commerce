// ===== SEARCH BAR SCRIPT =====
const searchInput = document.getElementById('searchInput');
const clearBtn = document.getElementById('clearBtn');
const searchBtn = document.getElementById('searchBtn');
const suggestions = document.getElementById('suggestions');
const categoryFilter = document.getElementById('categoryFilter');
const priceSort = document.getElementById('priceSort');

let debounceTimer;

// ===== ĐỒNG BỘ DROPDOWN VỚI URL NGAY KHI LOAD =====
document.addEventListener('DOMContentLoaded', function() {
    const urlParams = new URLSearchParams(window.location.search);
    
    console.log('URL params:', urlParams.toString());
    
    if (urlParams.has('category') && categoryFilter) {
        categoryFilter.value = urlParams.get('category');
        console.log('Category filter set to:', urlParams.get('category'));
    }
    
    if (urlParams.has('sort') && priceSort) {
        priceSort.value = urlParams.get('sort');
        console.log('Sort filter set to:', urlParams.get('sort'));
    }
    
    if (urlParams.has('search') && searchInput) {
        searchInput.value = urlParams.get('search');
        if (clearBtn) {
            clearBtn.style.display = 'block';
        }
        console.log('Search input set to:', urlParams.get('search'));
    }
});

// Format giá
function formatPrice(price) {
    return '$' + new Intl.NumberFormat('en-US').format(price);
}

// ===== ĐỒNG BỘ DROPDOWN VỚI URL (THÊM VÀO ĐẦU) =====
window.addEventListener('DOMContentLoaded', function() {
    const urlParams = new URLSearchParams(window.location.search);

    if (urlParams.has('category')) {
        categoryFilter.value = urlParams.get('category');
    }

    if (urlParams.has('sort')) {
        priceSort.value = urlParams.get('sort');
    }

    if (urlParams.has('search')) {
        searchInput.value = urlParams.get('search');
        clearBtn.style.display = 'block';
    }
});

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
    const params = new URLSearchParams();
    
    if (searchInput.value) {
        params.set('search', searchInput.value);
    }
    if (this.value) {
        params.set('category', this.value);
    }
    if (priceSort.value) {
        params.set('sort', priceSort.value);
    }

    window.location.href = `/Shop${params.toString() ? '?' + params.toString() : ''}`;
});

priceSort.addEventListener('change', function() {
    const params = new URLSearchParams();
    
    if (searchInput.value) {
        params.set('search', searchInput.value);
    }
    if (categoryFilter.value) {
        params.set('category', categoryFilter.value);
    }
    if (this.value) {
        params.set('sort', this.value);
    }

    window.location.href = `/Shop${params.toString() ? '?' + params.toString() : ''}`;
});

// Xóa nội dung
clearBtn.addEventListener('click', function() {
    searchInput.value = '';
    categoryFilter.value = '';
    priceSort.value = '';
    clearBtn.style.display = 'none';
    suggestions.style.display = 'none';
    searchInput.focus();
    window.location.href = '/Shop';
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

    suggestions.innerHTML = '<div class="loading">Searching...</div>';
    suggestions.style.display = 'block';

    // Gọi API ASP.NET Core
    fetch(`/Shop/SearchSuggestions?term=${encodeURIComponent(query)}`)
        .then(response => response.json())
        .then(data => {
            displaySuggestions(data, query);
        })
        .catch(error => {
            console.error('Error:', error);
            suggestions.innerHTML = '<div class="loading">Connection error</div>';
        });
}

// Hiển thị gợi ý
function displaySuggestions(results, query) {
    if (results.length === 0) {
        suggestions.innerHTML = '<div class="loading">No products found</div>';
        return;
    }

    const html = results.map(product => {
        const regex = new RegExp(`(${query})`, 'gi');
        const highlighted = product.name.replace(regex, '<span class="highlight">$1</span>');
        
        return `
            <div class="suggestion-item" onclick='selectProduct(${product.id}, "${product.name.replace(/'/g, "\\'")}")'>
                <span class="suggestion-icon">🛒</span>
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
    window.location.href = `/Shop/Details/${productId}`;
}

// Tìm kiếm khi nhấn nút
searchBtn.addEventListener('click', function() {
    const query = searchInput.value.trim();
    if (query) {
        suggestions.style.display = 'none';
        
        const params = new URLSearchParams();
        params.set('search', query);
        
        if (categoryFilter.value) {
            params.set('category', categoryFilter.value);
        }
        if (priceSort.value) {
            params.set('sort', priceSort.value);
        }

        window.location.href = `/Shop?${params.toString()}`;
    }
});

// Enter để tìm kiếm
searchInput.addEventListener('keypress', function(e) {
    if (e.key === 'Enter') {
        searchBtn.click();
    }
});