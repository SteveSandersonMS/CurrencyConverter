if (location.hostname !== 'localhost') {
    navigator.serviceWorker.register('service-worker.js', { updateViaCache: 'none' }).then(async registration => {
        checkIfReady();
        registration.onupdatefound = () => { registration.installing.onstatechange = checkIfReady; };

        function checkIfReady() {
            if (registration.active && registration.waiting && registration.waiting.state === 'installed') {
                document.body.classList.add('pwa-ready');
            }
        }
    });
}

async function triggerUpdate() {
    const registration = await navigator.serviceWorker.getRegistration();
    registration.waiting.postMessage('skipWaiting');
}

navigator.serviceWorker.addEventListener('message', event => {
    if (event.data === 'reload') {
        location.reload();
    }
});
