/* DriverRent Interactive Enhancements */
(() => {
    'use strict';

    // 1. Toast Notification Helper
    window.driverRentToast = function (message, variant = 'success') {
        let container = document.getElementById('drGlobalToastContainer');
        if (!container) {
            container = document.createElement('div');
            container.id = 'drGlobalToastContainer';
            container.className = 'toast-container position-fixed top-0 end-0 p-3';
            container.style.zIndex = '1095';
            document.body.appendChild(container);
        }

        const iconMap = {
            success: 'fa-circle-check text-success',
            info: 'fa-circle-info text-primary',
            warning: 'fa-triangle-exclamation text-warning',
            danger: 'fa-circle-xmark text-danger'
        };

        const toastEl = document.createElement('div');
        toastEl.className = 'toast align-items-center border-0 shadow-lg rounded-4 bg-white';
        toastEl.setAttribute('role', 'status');
        toastEl.setAttribute('aria-live', 'polite');
        toastEl.innerHTML = `
            <div class="d-flex align-items-center p-2">
                <div class="toast-body d-flex align-items-center gap-2 fw-medium text-dark">
                    <i class="fa-solid ${iconMap[variant] || iconMap.info} fs-5"></i>
                    <span></span>
                </div>
                <button type="button" class="btn-close me-2 m-auto" data-bs-dismiss="toast" aria-label="ปิด"></button>
            </div>
        `;
        toastEl.querySelector('.toast-body span').textContent = message;
        container.appendChild(toastEl);

        if (window.bootstrap?.Toast) {
            const instance = new bootstrap.Toast(toastEl, { delay: 2800 });
            toastEl.addEventListener('hidden.bs.toast', () => toastEl.remove());
            instance.show();
        }
    };

    // 2. Copy Promo Code
    document.addEventListener('click', async (event) => {
        const trigger = event.target.closest('[data-copy-code]');
        if (!trigger) return;
        const code = trigger.getAttribute('data-copy-code');
        if (!code) return;

        try {
            await navigator.clipboard.writeText(code);
            window.driverRentToast(`คัดลอกโค้ด "${code}" เรียบร้อยแล้ว`, 'success');
        } catch {
            window.driverRentToast(`โค้ดโปรโมชัน: ${code}`, 'info');
        }
    });

    // 3. Scroll Reveal
    const revealItems = document.querySelectorAll('[data-reveal]');
    if (revealItems.length > 0) {
        const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
        if (reducedMotion || !('IntersectionObserver' in window)) {
            revealItems.forEach(el => el.classList.add('is-revealed'));
        } else {
            const observer = new IntersectionObserver((entries, obs) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        entry.target.classList.add('is-revealed');
                        obs.unobserve(entry.target);
                    }
                });
            }, { threshold: 0.08, rootMargin: '0px 0px -30px 0px' });
            revealItems.forEach(el => observer.observe(el));
        }
    }

    // 4. KPI Count-Up Animation
    document.querySelectorAll('[data-countup]').forEach(el => {
        const raw = el.getAttribute('data-countup') || '';
        const target = parseFloat(raw.replace(/,/g, ''));
        if (Number.isNaN(target)) return;

        const prefix = el.getAttribute('data-countup-prefix') || '';
        const suffix = el.getAttribute('data-countup-suffix') || '';
        const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
        if (reducedMotion || target === 0) {
            el.textContent = `${prefix}${Math.round(target).toLocaleString('en-US')}${suffix}`;
            return;
        }

        const duration = 700;
        const start = performance.now();
        function step(now) {
            const progress = Math.min((now - start) / duration, 1);
            const eased = 1 - Math.pow(1 - progress, 3);
            const current = Math.round(target * eased);
            el.textContent = `${prefix}${current.toLocaleString('en-US')}${suffix}`;
            if (progress < 1) requestAnimationFrame(step);
        }
        requestAnimationFrame(step);
    });

    // 5. Custom Confirm Modal for forms with data-confirm
    let pendingConfirmForm = null;
    document.addEventListener('submit', (event) => {
        const form = event.target.closest('form[data-confirm]');
        if (!form || form.dataset.confirmed === 'true') return;

        event.preventDefault();
        pendingConfirmForm = form;
        const message = form.getAttribute('data-confirm') || 'ยืนยันการทำรายการนี้หรือไม่?';

        let modalEl = document.getElementById('drConfirmModal');
        if (!modalEl) {
            modalEl = document.createElement('div');
            modalEl.id = 'drConfirmModal';
            modalEl.className = 'modal fade';
            modalEl.tabIndex = -1;
            modalEl.setAttribute('aria-hidden', 'true');
            modalEl.innerHTML = `
                <div class="modal-dialog modal-dialog-centered">
                    <div class="modal-content">
                        <div class="modal-header border-0 pb-0">
                            <h5 class="modal-title fw-bold text-dark">
                                <i class="fa-solid fa-triangle-exclamation text-warning me-2"></i>ยืนยันการทำรายการ
                            </h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="ปิด"></button>
                        </div>
                        <div class="modal-body py-3 text-secondary" id="drConfirmModalBody"></div>
                        <div class="modal-footer border-0 pt-0">
                            <button type="button" class="btn btn-light border px-4" data-bs-dismiss="modal">ยกเลิก</button>
                            <button type="button" class="btn btn-danger px-4 fw-semibold" id="drConfirmModalApprove">ยืนยัน</button>
                        </div>
                    </div>
                </div>
            `;
            document.body.appendChild(modalEl);

            modalEl.querySelector('#drConfirmModalApprove').addEventListener('click', () => {
                if (pendingConfirmForm) {
                    const targetForm = pendingConfirmForm;
                    pendingConfirmForm = null;
                    targetForm.dataset.confirmed = 'true';
                    bootstrap.Modal.getInstance(modalEl)?.hide();
                    targetForm.submit();
                }
            });
        }

        modalEl.querySelector('#drConfirmModalBody').textContent = message;
        if (window.bootstrap?.Modal) {
            bootstrap.Modal.getOrCreateInstance(modalEl).show();
        }
    });

    // 6. Lightbox Image Modal for [data-lightbox-src]
    document.addEventListener('click', (event) => {
        const trigger = event.target.closest('[data-lightbox-src]');
        if (!trigger) return;
        event.preventDefault();

        const src = trigger.getAttribute('data-lightbox-src');
        const title = trigger.getAttribute('data-lightbox-title') || 'ดูรูปภาพหลักฐาน';
        if (!src) return;

        let modalEl = document.getElementById('drLightboxModal');
        if (!modalEl) {
            modalEl = document.createElement('div');
            modalEl.id = 'drLightboxModal';
            modalEl.className = 'modal fade';
            modalEl.tabIndex = -1;
            modalEl.setAttribute('aria-hidden', 'true');
            modalEl.innerHTML = `
                <div class="modal-dialog modal-dialog-centered modal-lg">
                    <div class="modal-content bg-dark text-white border-0">
                        <div class="modal-header border-secondary bg-dark text-white">
                            <h6 class="modal-title fw-bold mb-0" id="drLightboxTitle"></h6>
                            <div class="d-flex align-items-center gap-2">
                                <a id="drLightboxExternal" href="#" target="_blank" rel="noopener" class="btn btn-sm btn-outline-light">
                                    <i class="fa-solid fa-arrow-up-right-from-square me-1"></i>เปิดแท็บใหม่
                                </a>
                                <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="ปิด"></button>
                            </div>
                        </div>
                        <div class="modal-body text-center p-3 bg-black">
                            <img id="drLightboxImg" src="" alt="" class="img-fluid rounded-3" style="max-height: 75vh; object-fit: contain;" />
                        </div>
                    </div>
                </div>
            `;
            document.body.appendChild(modalEl);
        }

        modalEl.querySelector('#drLightboxTitle').textContent = title;
        modalEl.querySelector('#drLightboxExternal').setAttribute('href', src);
        const img = modalEl.querySelector('#drLightboxImg');
        img.src = src;
        img.alt = title;

        if (window.bootstrap?.Modal) {
            bootstrap.Modal.getOrCreateInstance(modalEl).show();
        }
    });

    // 7. Drag & Drop Image Upload Preview
    document.querySelectorAll('[data-dropzone]').forEach(zone => {
        const input = zone.querySelector('input[type="file"]');
        const previewWrap = zone.querySelector('[data-dropzone-preview]');
        const previewImg = zone.querySelector('[data-dropzone-img]');
        const fileNameEl = zone.querySelector('[data-dropzone-filename]');
        if (!input) return;

        function handleFiles(files) {
            if (!files || files.length === 0) return;
            const file = files[0];
            if (fileNameEl) {
                const sizeKb = Math.max(1, Math.round(file.size / 1024));
                fileNameEl.textContent = `${file.name} (${sizeKb} KB)`;
            }
            if (previewWrap && previewImg && file.type.startsWith('image/')) {
                const reader = new FileReader();
                reader.onload = (e) => {
                    previewImg.src = e.target.result;
                    previewWrap.classList.remove('d-none');
                };
                reader.readAsDataURL(file);
            }
        }

        input.addEventListener('change', () => handleFiles(input.files));

        ['dragenter', 'dragover'].forEach(evt => {
            zone.addEventListener(evt, (e) => {
                e.preventDefault();
                zone.classList.add('is-dragover');
            });
        });

        ['dragleave', 'drop'].forEach(evt => {
            zone.addEventListener(evt, (e) => {
                e.preventDefault();
                zone.classList.remove('is-dragover');
            });
        });

        zone.addEventListener('drop', (e) => {
            if (e.dataTransfer?.files?.length) {
                input.files = e.dataTransfer.files;
                handleFiles(input.files);
            }
        });
    });
})();
