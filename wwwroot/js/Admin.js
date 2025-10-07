(function () {
    'use strict';

    const el = id => document.getElementById(id);

    // 🟢 Mostrar mensajes TempData
    function showTempMessages() {
        if (window.tempData?.message) {
            Swal.fire({
                icon: 'success',
                title: 'Éxito',
                text: window.tempData.message,
                timer: 2000,
                showConfirmButton: false
            });
        } else if (window.tempData?.error) {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: window.tempData.error,
                confirmButtonText: 'Aceptar'
            });
        }
    }

    // 🟠 Confirmación de eliminación
    function initDeleteConfirmation() {
        document.querySelectorAll('.delete-form').forEach(form => {
            form.addEventListener('submit', e => {
                e.preventDefault();
                Swal.fire({
                    title: '¿Estás seguro?',
                    text: "No podrás revertir esta acción.",
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonColor: '#d33',
                    cancelButtonColor: '#3085d6',
                    confirmButtonText: 'Sí, eliminar',
                    cancelButtonText: 'Cancelar'
                }).then(result => {
                    if (result.isConfirmed) form.submit();
                });
            });
        });
    }

    // 🟣 Zoom elegante sin Bootstrap Modal
    function initZoomOverlay() {
        const overlay = document.createElement("div");
        overlay.classList.add("zoom-overlay");
        overlay.innerHTML = '<img src="" alt="Imagen ampliada">';
        document.body.appendChild(overlay);

        const zoomImg = overlay.querySelector("img");

        document.querySelectorAll(".zoomable-photo").forEach(img => {
            img.addEventListener("click", () => {
                zoomImg.src = img.src;
                overlay.classList.add("active");
            });
        });

        overlay.addEventListener("click", () => {
            overlay.classList.remove("active");
        });
    }

    // 🔵 Captura de foto en Create (permite varias tomas)
    async function initCreateCapture() {
        const video = el('video');
        const canvas = el('canvas');
        const captureBtn = el('capture');
        const retakeBtn = el('retake');
        const photoInput = el('photoBase64');
        const preview = el('preview');

        if (!video || !canvas || !captureBtn || !photoInput || !preview) return;

        let stream = null;

        try {
            stream = await navigator.mediaDevices.getUserMedia({ video: true });
            video.srcObject = stream;
            await video.play();
            preview.style.display = 'none';
            retakeBtn.style.display = 'none';
        } catch (err) {
            console.error("Error al iniciar cámara (Create):", err);
            Swal.fire({
                icon: 'error',
                title: 'Error cámara',
                text: 'No se pudo acceder a la cámara. Verifica permisos y que estés en HTTPS/localhost.'
            });
            return;
        }

        captureBtn.addEventListener('click', () => {
            try {
                canvas.width = video.videoWidth || 320;
                canvas.height = video.videoHeight || 240;
                const ctx = canvas.getContext('2d');
                ctx.drawImage(video, 0, 0, canvas.width, canvas.height);
                const dataUrl = canvas.toDataURL('image/png');

                photoInput.value = dataUrl;
                preview.src = dataUrl;
                preview.style.display = 'block';
                retakeBtn.style.display = 'inline-block';
            } catch (err) {
                console.error("Error capturando foto:", err);
            }
        });

        retakeBtn.addEventListener('click', () => {
            preview.style.display = 'none';
            photoInput.value = '';
        });

        window.addEventListener('beforeunload', () => {
            if (stream) stream.getTracks().forEach(t => t.stop());
        });
    }

    // 🟢 Cámara editable (Edit)
    function initEditCamera() {
        const video = el('video');
        const canvas = el('canvas');
        const startCamera = el('startCamera');
        const takePhoto = el('takePhoto');
        const cancelPhoto = el('cancelPhoto');
        const photoBase64 = el('photoBase64');
        const currentPhoto = el('currentPhoto');

        if (!video || !canvas || !startCamera || !takePhoto || !cancelPhoto || !photoBase64 || !currentPhoto) return;

        let stream = null;

        startCamera.addEventListener('click', async () => {
            try {
                stream = await navigator.mediaDevices.getUserMedia({ video: true });
                video.srcObject = stream;
                await video.play();
                video.style.display = 'block';
                startCamera.style.display = 'none';
                takePhoto.style.display = 'inline-block';
                cancelPhoto.style.display = 'inline-block';
                currentPhoto.style.display = 'none';
            } catch (err) {
                console.error("Error cámara (Edit):", err);
                Swal.fire({
                    icon: 'error',
                    title: 'Error cámara',
                    text: 'No se pudo acceder a la cámara. Verifica permisos y que estés en HTTPS/localhost.'
                });
            }
        });

        takePhoto.addEventListener('click', () => {
            try {
                canvas.width = video.videoWidth || 320;
                canvas.height = video.videoHeight || 240;
                const ctx = canvas.getContext('2d');
                ctx.drawImage(video, 0, 0, canvas.width, canvas.height);
                const dataURL = canvas.toDataURL('image/png');
                photoBase64.value = dataURL;
                currentPhoto.src = dataURL;
                currentPhoto.style.display = 'block';
                video.style.display = 'none';
                takePhoto.style.display = 'none';
                cancelPhoto.style.display = 'none';
                startCamera.style.display = 'inline-block';

                if (stream) {
                    stream.getTracks().forEach(track => track.stop());
                    stream = null;
                }
            } catch (err) {
                console.error("Error capturando (Edit):", err);
            }
        });

        cancelPhoto.addEventListener('click', () => {
            if (stream) {
                stream.getTracks().forEach(track => track.stop());
                stream = null;
            }
            video.style.display = 'none';
            startCamera.style.display = 'inline-block';
            takePhoto.style.display = 'none';
            cancelPhoto.style.display = 'none';
            currentPhoto.style.display = 'block';
        });
    }

    // 🟡 Inicialización general
    document.addEventListener('DOMContentLoaded', () => {
        showTempMessages();
        initDeleteConfirmation();
        initZoomOverlay();
        initCreateCapture();
        initEditCamera();
        console.log("✅ Admin.js cargado correctamente con zoom funcional");
    });
})();
