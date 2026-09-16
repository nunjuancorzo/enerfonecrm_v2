window.publicSigning = {
    canvases: {},
    start: function (id) {
        const canvas = document.getElementById(id);
        if (!canvas) return;
        const context = canvas.getContext('2d');
        context.lineWidth = 2;
        context.lineCap = 'round';
        context.strokeStyle = '#111827';
        let drawing = false;
        const point = (event) => {
            const rect = canvas.getBoundingClientRect();
            const source = event.touches ? event.touches[0] : event;
            return { x: (source.clientX - rect.left) * canvas.width / rect.width, y: (source.clientY - rect.top) * canvas.height / rect.height };
        };
        const begin = (event) => { event.preventDefault(); drawing = true; const p = point(event); context.beginPath(); context.moveTo(p.x, p.y); };
        const move = (event) => { if (!drawing) return; event.preventDefault(); const p = point(event); context.lineTo(p.x, p.y); context.stroke(); };
        const end = () => { drawing = false; };
        canvas.addEventListener('mousedown', begin);
        canvas.addEventListener('mousemove', move);
        canvas.addEventListener('mouseup', end);
        canvas.addEventListener('mouseleave', end);
        canvas.addEventListener('touchstart', begin, { passive: false });
        canvas.addEventListener('touchmove', move, { passive: false });
        canvas.addEventListener('touchend', end);
        this.canvases[id] = canvas;
    },
    clear: function (id) {
        const canvas = document.getElementById(id);
        if (canvas) canvas.getContext('2d').clearRect(0, 0, canvas.width, canvas.height);
    },
    dataUrl: function (id) {
        const canvas = document.getElementById(id);
        return canvas ? canvas.toDataURL('image/png') : '';
    }
};