navigator.serviceWorker.register('service-worker.js', { updateViaCache: 'none' }).then(async registration => {
    // We check if there's a pending app update both immediately on startup and when the browser
    // tells us the service worker itself has updated
    checkIfReady();
    registration.onupdatefound = () => {
        registration.installing.onstatechange = checkIfReady;
    };

    function checkIfReady() {
        if (registration.active && registration.waiting && registration.waiting.state === 'installed') {
            // This causes the "update available" UI to get displayed
            document.body.classList.add('pwa-ready');
        }
    }
});

// This gets called from .NET when the user clicks "update now" in the UI
async function triggerUpdate() {
    const registration = await navigator.serviceWorker.getRegistration();
    registration.waiting.postMessage('skipWaiting');
}

// This gets called by the Service Worker when it's finished installing an update
navigator.serviceWorker.addEventListener('message', event => {
    if (event.data === 'reload') {
        location.reload();
    }
});
