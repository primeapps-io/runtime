module.exports = {
    bundle: {
        auth: {
            scripts: [
                './scripts/vendor/jquery.js',
                './scripts/vendor/jquery-maskedinput.js',
                './scripts/vendor/sweetalert.js',
                './scripts/vendor/spin.js',
                './scripts/vendor/ladda.js'
            ],
            styles: [
                './styles/vendor/bootstrap.css',
                './styles/vendor/flaticon.css',
                './styles/vendor/ladda-themeless.css',
                './styles/vendor/font-awesome.css'
            ],
            options: {
                maps: false,
                uglify: true,
                minCSS: true,
                rev: false
            }
        }
    }
};