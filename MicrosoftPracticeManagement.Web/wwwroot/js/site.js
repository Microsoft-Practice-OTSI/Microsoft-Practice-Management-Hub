// Microsoft Practice Hub - Global Site Interactions

document.addEventListener("DOMContentLoaded", function () {
    // 1. Setup Global Search keyboard shortcut (Ctrl+/ or Cmd+/)
    const globalSearchInput = document.getElementById("globalSearchInput");
    document.addEventListener("keydown", function (e) {
        if ((e.ctrlKey || e.metaKey) && e.key === '/') {
            e.preventDefault();
            if (globalSearchInput) {
                globalSearchInput.focus();
                globalSearchInput.select();
            }
        }
    });

    if (globalSearchInput) {
        globalSearchInput.addEventListener("keydown", function (e) {
            if (e.key === "Enter") {
                const term = encodeURIComponent(globalSearchInput.value.trim());
                if (term) {
                    window.location.href = `/Resources?search=${term}`;
                }
            }
        });
    }

    // 2. Quick View Modal Trigger
    const quickViewModalEl = document.getElementById('quickViewModal');
    let quickViewModal = null;
    if (quickViewModalEl && typeof bootstrap !== 'undefined') {
        quickViewModal = new bootstrap.Modal(quickViewModalEl);
    }

    document.querySelectorAll('.btn-quick-view').forEach(btn => {
        btn.addEventListener('click', function (e) {
            e.preventDefault();
            const empId = this.getAttribute('data-emp-id');
            if (!empId) return;

            const modalBody = document.getElementById('quickViewModalBody');
            if (modalBody) {
                modalBody.innerHTML = `
                    <div class="text-center py-5">
                        <div class="spinner-border text-primary" role="status">
                            <span class="visually-hidden">Loading...</span>
                        </div>
                        <p class="mt-2 text-muted">Retrieving consultant profile...</p>
                    </div>`;
            }

            if (quickViewModal) {
                quickViewModal.show();
            }

            fetch(`/Resources/QuickView?id=${encodeURIComponent(empId)}`)
                .then(res => {
                    if (!res.ok) throw new Error('Resource not found');
                    return res.text();
                })
                .then(html => {
                    if (modalBody) {
                        modalBody.innerHTML = html;
                    }
                })
                .catch(err => {
                    if (modalBody) {
                        modalBody.innerHTML = `<div class="alert alert-danger m-3">Could not load consultant profile: ${err.message}</div>`;
                    }
                });
        });
    });

    // 3. Practice Switcher Handler
    const practiceSelector = document.getElementById('practiceSelector');
    if (practiceSelector) {
        practiceSelector.addEventListener('change', function () {
            const selected = this.value;
            // Notify user of practice switch
            showToast(`Switched Practice Area to: ${selected}`);
        });
    }
});

function showToast(message) {
    const toastEl = document.getElementById('hubGlobalToast');
    if (toastEl && typeof bootstrap !== 'undefined') {
        const toastBody = toastEl.querySelector('.toast-body');
        if (toastBody) toastBody.textContent = message;
        const toast = new bootstrap.Toast(toastEl);
        toast.show();
    }
}
