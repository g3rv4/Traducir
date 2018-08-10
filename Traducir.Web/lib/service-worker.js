self.addEventListener('push', function (event) {
    const data = event.data.json();

    const options = {
        body: data.content,
        icon: '/lib/unicorn.png',
        actions: data.actions.map((a, idx) => {
            return {
                action: idx,
                title: a.title
            }
        }),
        requireInteraction: data.requireInteraction,
        data: data,
        tag: data.topic
    };

    const promiseChain = self.registration.showNotification(data.title, options);
    event.waitUntil(promiseChain);
});

self.addEventListener('notificationclick', function (event) {
    const notification = event.notification;

    let destinationUrl = "";
    if (!event.action) {
        destinationUrl = notification.data.url;
    } else {
        const actionId = parseInt(event.action);
        destinationUrl = notification.data.actions[actionId].url;
    }
    destinationUrl = new URL(destinationUrl).href;

    const promiseChain = clients.matchAll({
        type: 'window',
        includeUncontrolled: true
    })
        .then((windowClients) => {
            let matchingClient = null;

            for (let i = 0; i < windowClients.length; i++) {
                const windowClient = windowClients[i];
                if (windowClient.url === destinationUrl) {
                    matchingClient = windowClient;
                    break;
                }
            }

            if (matchingClient) {
                return matchingClient.focus();
            } else {
                return clients.openWindow(destinationUrl);
            }
        });

    event.waitUntil(promiseChain);
    notification.close();
});
