window.loginLogo = {
    async removeWhiteBackground(imageId) {
        const image = document.getElementById(imageId);
        if (!image ||
            image.dataset.backgroundProcessed === "true" ||
            image.dataset.backgroundProcessing === "true") return;

        image.dataset.backgroundProcessing = "true";
        try {
            await image.decode();

            const scale = Math.min(1, 1000 / Math.max(image.naturalWidth, image.naturalHeight));
            const canvas = document.createElement("canvas");
            canvas.width = Math.max(1, Math.round(image.naturalWidth * scale));
            canvas.height = Math.max(1, Math.round(image.naturalHeight * scale));

            const context = canvas.getContext("2d", { willReadFrequently: true });
            if (!context) return;

            context.drawImage(image, 0, 0, canvas.width, canvas.height);
            const imageData = context.getImageData(0, 0, canvas.width, canvas.height);
            const pixels = imageData.data;
            const pixelCount = canvas.width * canvas.height;
            const visited = new Uint8Array(pixelCount);
            const queue = new Uint32Array(pixelCount);
            let queueEnd = 0;

            const addIfBackground = (pixel) => {
                if (visited[pixel]) return;
                visited[pixel] = 1;

                const offset = pixel * 4;
                if (pixels[offset + 3] > 0 &&
                    pixels[offset] >= 232 &&
                    pixels[offset + 1] >= 232 &&
                    pixels[offset + 2] >= 232) {
                    queue[queueEnd++] = pixel;
                }
            };

            for (let x = 0; x < canvas.width; x++) {
                addIfBackground(x);
                addIfBackground((canvas.height - 1) * canvas.width + x);
            }
            for (let y = 0; y < canvas.height; y++) {
                addIfBackground(y * canvas.width);
                addIfBackground(y * canvas.width + canvas.width - 1);
            }

            for (let queueStart = 0; queueStart < queueEnd; queueStart++) {
                const pixel = queue[queueStart];
                const x = pixel % canvas.width;
                const y = Math.floor(pixel / canvas.width);
                pixels[pixel * 4 + 3] = 0;

                if (x > 0) addIfBackground(pixel - 1);
                if (x + 1 < canvas.width) addIfBackground(pixel + 1);
                if (y > 0) addIfBackground(pixel - canvas.width);
                if (y + 1 < canvas.height) addIfBackground(pixel + canvas.width);
            }

            if (queueEnd === 0) return;

            context.putImageData(imageData, 0, 0);
            image.dataset.backgroundProcessed = "true";
            image.src = canvas.toDataURL("image/png");
        } catch (error) {
            console.warn("No se pudo adaptar el fondo del logo de acceso.", error);
        } finally {
            delete image.dataset.backgroundProcessing;
        }
    }
};

const processLoginLogo = () => {
    const image = document.getElementById("login-brand-logo");
    if (!image || image.dataset.backgroundProcessed === "true") return;
    if (!image.complete || image.naturalWidth === 0) {
        image.addEventListener("load", processLoginLogo, { once: true });
        return;
    }

    window.loginLogo.removeWhiteBackground("login-brand-logo");
};

new MutationObserver(processLoginLogo).observe(document.body, {
    childList: true,
    subtree: true
});

processLoginLogo();