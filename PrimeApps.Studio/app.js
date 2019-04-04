var workboxBuild = require('workbox-build');

// NOTE: This should be run *AFTER* all your assets are built
var buildSW =  function(){
    return workboxBuild.generateSW({
        globDirectory: 'PrimeApps.Studio/wwwroot',
        globPatterns:[
            '**\/*.{html,js,css}',
        ],
        swDest: `PrimeApps.Studio/wwwroot/sw.js`,
        cleanupOutdatedCaches:true,
        clientsClaim: false,
        skipWaiting: true,
        cleanUrls:true
        
    }).then(({warnings}) => {
        // In case there are any warnings from workbox-build, log them.
        for (const warning of warnings) {
        console.warn(warning);
    }
    console.info('Service worker generation completed.');
}).catch((error) => {
        console.warn('Service worker generation failed:', error);
});
}
// This will return a Promise
buildSW();   