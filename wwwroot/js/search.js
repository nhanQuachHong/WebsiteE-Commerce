// ===== SEARCH BAR SCRIPT =====
const searchInput = document.getElementById('searchInput');
const clearBtn = document.getElementById('clearBtn');
const searchBtn = document.getElementById('searchBtn');
const suggestions = document.getElementById('suggestions');
const priceSort = document.getElementById('priceSort');
const productList = document.getElementById('productList');
const loadingSpinner = document.getElementById('loadingSpinner');

let debounceTimer;
let currentCategory = '';
let currentSort = '';

// ===== ĐỒNG BỘ DROPDOWN VỚI URL NGAY KHI LOAD =====
document.addEventListener('DOMContentLoaded', function() {
    const urlParams = new URLSearchParams(window.location.search);
    
    console.log('URL params:', urlParams.toString());
    
    // Lấy category từ URL (nếu có)
    if (urlParams.has('category')) {
        currentCategory = urlParams.get('category');
    }
    
    if (urlParams.has('sort') && priceSort) {
        priceSort.value = urlParams.get('sort');
        currentSort = urlParams.get('sort');
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

// Load sản phẩm bằng AJAX
function loadProducts(searchTerm = '') {
    if (!productList) return;
    
    // Hiển thị loading
    productList.classList.add('loading');
    if (loadingSpinner) {
        loadingSpinner.style.display = 'block';
    }
    
    const params = new URLSearchParams();
    
    // Thêm search term
    if (searchTerm) {
        params.append('search', searchTerm);
    }
    
    // Thêm category (lấy từ tabs active hoặc currentCategory)
    const activeTab = document.querySelector('.category-tab.active');
    if (activeTab) {
        const category = activeTab.getAttribute('data-category');
        if (category) {
            params.append('category', category);
            currentCategory = category;
        }
    } else if (currentCategory) {
        params.append('category', currentCategory);
    }
    
    // Thêm sort
    if (currentSort) {
        params.append('sort', currentSort);
    }
    
    // Gọi AJAX
    fetch(`/Shop/GetProducts?${params.toString()}`)
        .then(response => {
            if (!response.ok) throw new Error('Network response was not ok');
            return response.text();
        })
        .then(html => {
            productList.innerHTML = html;
            productList.classList.remove('loading');
            if (loadingSpinner) {
                loadingSpinner.style.display = 'none';
            }
            
            // Cập nhật URL không reload
            updateUrlParams(searchTerm);
        })
        .catch(error => {
            console.error('Error loading products:', error);
            productList.innerHTML = '<div class="col-12 text-center py-5"><p class="text-muted fs-5">Error loading products.</p></div>';
            productList.classList.remove('loading');
            if (loadingSpinner) {
                loadingSpinner.style.display = 'none';
            }
        });
}

// Cập nhật URL không reload
function updateUrlParams(searchTerm = '') {
    const params = new URLSearchParams();
    
    if (searchTerm) {
        params.set('search', searchTerm);
    }
    
    if (currentCategory) {
        params.set('category', currentCategory);
    }
    
    if (currentSort) {
        params.set('sort', currentSort);
    }
    
    const newUrl = window.location.pathname + (params.toString() ? '?' + params.toString() : '');
    window.history.pushState({}, '', newUrl);
}

// Hiện/ẩn nút xóa
if (searchInput && clearBtn) {
    searchInput.addEventListener('input', function() {
        clearBtn.style.display = this.value ? 'block' : 'none';
        
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(() => {
            fetchSuggestions(this.value);
        }, 300);
    });
}

// Xử lý khi click vào category tabs (nếu có trên trang)
document.addEventListener('click', function(e) {
    if (e.target.classList.contains('category-tab')) {
        // Lấy category từ tab được click
        const category = e.target.getAttribute('data-category');
        currentCategory = category;
        
        // Load sản phẩm với category mới
        loadProducts(searchInput ? searchInput.value.trim() : '');
    }
});

// Xử lý sort (AJAX)
if (priceSort) {
    priceSort.addEventListener('change', function() {
        currentSort = this.value;
        loadProducts(searchInput ? searchInput.value.trim() : '');
    });
}

// Xóa nội dung search
if (clearBtn) {
    clearBtn.addEventListener('click', function() {
        searchInput.value = '';
        clearBtn.style.display = 'none';
        suggestions.style.display = 'none';
        searchInput.focus();
        
        // Load lại sản phẩm (không có search term)
        loadProducts();
    });
}

// Ẩn suggestions khi click bên ngoài
document.addEventListener('click', function(e) {
    if (!e.target.closest('.search-container')) {
        if (suggestions) {
            suggestions.style.display = 'none';
        }
    }
});

// Focus vào input hiện lại suggestions
if (searchInput) {
    searchInput.addEventListener('focus', function() {
        if (this.value && suggestions && suggestions.innerHTML) {
            suggestions.style.display = 'block';
        }
    });
}

// Hàm gọi AJAX để lấy gợi ý sản phẩm
function fetchSuggestions(query) {
    if (!suggestions) return;
    
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
            if (suggestions) {
                suggestions.innerHTML = '<div class="loading">Connection error</div>';
            }
        });
}

// Hiển thị gợi ý
function displaySuggestions(results, query) {
    if (!suggestions) return;
    
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

// Tìm kiếm khi nhấn nút (AJAX - không reload)
if (searchBtn && searchInput) {
    searchBtn.addEventListener('click', function() {
        const query = searchInput.value.trim();
        if (suggestions) {
            suggestions.style.display = 'none';
        }
        
        // Load sản phẩm bằng AJAX
        loadProducts(query);
    });
}

// Enter để tìm kiếm (AJAX - không reload)
if (searchInput) {
    searchInput.addEventListener('keypress', function(e) {
        if (e.key === 'Enter') {
            e.preventDefault(); // Ngăn form submit (nếu có)
            
            const query = searchInput.value.trim();
            if (suggestions) {
                suggestions.style.display = 'none';
            }
            
            // Load sản phẩm bằng AJAX
            loadProducts(query);
        }
    });
}

// Xử lý khi nhấn nút back/forward
window.addEventListener('popstate', function() {
    const urlParams = new URLSearchParams(window.location.search);
    
    // Cập nhật các biến từ URL
    const search = urlParams.get('search') || '';
    const category = urlParams.get('category') || '';
    const sort = urlParams.get('sort') || '';
    
    // Cập nhật UI
    if (searchInput) searchInput.value = search;
    if (clearBtn) clearBtn.style.display = search ? 'block' : 'none';
    
    // Cập nhật tabs active
    const categoryTabs = document.querySelectorAll('.category-tab');
    if (categoryTabs) {
        categoryTabs.forEach(tab => {
            const tabCategory = tab.getAttribute('data-category');
            if (tabCategory === category) {
                tab.classList.add('active');
            } else {
                tab.classList.remove('active');
            }
        });
    }
    
    if (priceSort && sort) {
        priceSort.value = sort;
    }
    
    // Cập nhật biến
    currentCategory = category;
    currentSort = sort;
    
    // Load sản phẩm
    loadProducts(search);
});