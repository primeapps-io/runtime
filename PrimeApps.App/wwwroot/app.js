if('serviceWorker' in navigator){
  navigator.serviceWorker.register('/pwa-sw.js')
    .then(reg => console.log('service worker registered'))
    .catch(err => console.log('service worker not registered', err));
}