// ── Cart Helper ──────────────────────────────────────────────────────────────
// Manages cart badge count via session API

document.addEventListener('DOMContentLoaded', () => {
    updateCartBadge();
});

async function updateCartBadge() {
    try {
        const res = await fetch('/Cart/Count');
        if (res.ok) {
            const count = await res.json();
            const badge = document.getElementById('cart-count');
            if (badge) {
                badge.textContent = count;
                badge.style.display = count > 0 ? 'block' : 'none';
            }
        }
    } catch { /* ignore */ }
}

async function addToCart(productId, quantity) {
    const res = await fetch('/Cart/Add', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('[name="__RequestVerificationToken"]')?.value ?? ''
        },
        body: JSON.stringify({ productId, quantity })
    });

    if (res.ok) {
        await updateCartBadge();
        showToast('Ürün sepete eklendi!', 'success');
    } else {
        const msg = await res.text();
        showToast(msg || 'Hata oluştu.', 'danger');
    }
}

function showToast(message, type = 'success') {
    const container = document.getElementById('toast-container') ?? createToastContainer();
    const id = 'toast-' + Date.now();
    const html = `
        <div id="${id}" class="toast align-items-center text-white bg-${type} border-0" role="alert">
          <div class="d-flex">
            <div class="toast-body fw-semibold">${message}</div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
          </div>
        </div>`;
    container.insertAdjacentHTML('beforeend', html);
    const el = document.getElementById(id);
    new bootstrap.Toast(el, { delay: 3000 }).show();
    el.addEventListener('hidden.bs.toast', () => el.remove());
}

function createToastContainer() {
    const div = document.createElement('div');
    div.id = 'toast-container';
    div.style.cssText = 'position:fixed;bottom:1.5rem;right:1.5rem;z-index:9999;display:flex;flex-direction:column;gap:.5rem;';
    document.body.appendChild(div);
    return div;
}
