module.exports = {
	bundle: {
		vendor: {
			scripts: [
				'./wwwroot/scripts/vendor/jquery.js',
				'./wwwroot/scripts/vendor/jquery-maskedinput.js',
				'./wwwroot/scripts/vendor/sweetalert.js',
				'./wwwroot/scripts/vendor/spin.js',
				'./wwwroot/scripts/vendor/ladda.js'
			],
			styles: [
				'./wwwroot/styles/vendor/bootstrap.css',
				'./wwwroot/styles/vendor/flaticon.css',
				'./wwwroot/styles/vendor/ladda-themeless.css',
				'./wwwroot/styles/vendor/font-awesome.css'
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