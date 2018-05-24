module.exports = {
    bundle: {
        auth: {
            scripts: [
                '/wwwroot/scripts/vendor/jquery.js',
                '/wwwroot/scripts/vendor/jquery-maskedinput.js',
                '/wwwroot/scripts/vendor/sweetalert.js',
                '/wwwroot/scripts/vendor/spin.js',
                '/wwwroot/scripts/vendor/ladda.js'
            ],
            styles: [
                '/wwwroot/styles/vendor/bootstrap.css',
                '/wwwroot/styles/vendor/flaticon.css',
                '/wwwroot/styles/vendor/ladda-themeless.css',
                '/wwwroot/styles/vendor/font-awesome.css'
            ],
            options: {
                maps: false,
                uglify: true,
                minCSS: true,
                rev: false
            }
        },
        vendor: {
            scripts: [
                '/wwwroot/scripts/vendor/angular.js',
                '/wwwroot/scripts/vendor/angular-ui-router.js',
                '/wwwroot/scripts/vendor/ocLazyLoad.js',
                '/wwwroot/scripts/vendor/angular-cookies.js',
                '/wwwroot/scripts/vendor/angular-translate.js',
                '/wwwroot/scripts/vendor/angular-animate.js',
                '/wwwroot/scripts/vendor/angular-sanitize.js',
                '/wwwroot/scripts/vendor/angular-strap.js',
                '/wwwroot/scripts/vendor/angular-strap.tpl.js',
                '/wwwroot/scripts/vendor/angular-ui-bootstrap-custom.js',
                '/wwwroot/scripts/vendor/angular-ui-bootstrap-custom-tpls.js',
                '/wwwroot/scripts/vendor/angular-xeditable.js',
                '/wwwroot/scripts/vendor/angular-ladda.js',
                '/wwwroot/scripts/vendor/angular-ui-utils.js',
                '/wwwroot/scripts/vendor/angular-ui-tinymce.js',
                '/wwwroot/scripts/vendor/mentio/mentio.js',
                '/wwwroot/scripts/vendor/ng-table.js',
                '/wwwroot/scripts/vendor/spin.js',
                '/wwwroot/scripts/vendor/ladda.js',
                '/wwwroot/scripts/vendor/moment.js',
                '/wwwroot/scripts/vendor/es5-shim.js',
                '/wwwroot/scripts/vendor/es5-sham.js',
                '/wwwroot/scripts/vendor/angular-file-upload.js',
                '/wwwroot/scripts/vendor/angular-bootstrap-show-errors.js',
                '/wwwroot/scripts/vendor/ngToast.js',
                '/wwwroot/scripts/vendor/angular-block-ui.js',
                '/wwwroot/scripts/vendor/angular-touch.js',
                '/wwwroot/scripts/vendor/ng-sortable.js',
                '/wwwroot/scripts/vendor/file-saver.js',
                '/wwwroot/scripts/vendor/ng-img-crop.js',
                '/wwwroot/scripts/vendor/angular-images-resizer.js',
                '/wwwroot/scripts/vendor/angular-ui-tree.js',
                '/wwwroot/scripts/vendor/plupload.full.js',
                '/wwwroot/scripts/vendor/angular-plupload.js',
                '/wwwroot/scripts/vendor/ng-tags-input.js',
                '/wwwroot/scripts/vendor/angular-ui-mask.js',
                '/wwwroot/scripts/vendor/powerbi.js',
                '/wwwroot/scripts/vendor/ace/ace.js',
                '/wwwroot/scripts/vendor/angular-ui-ace.js',
                '/wwwroot/scripts/vendor/ace/ext-language_tools.js',
                '/wwwroot/scripts/vendor/angular-ui-select.js',
                '/wwwroot/scripts/vendor/angular-resizable.js',
                '/wwwroot/scripts/vendor/clipboard.js',
                '/wwwroot/scripts/vendor/angular-translate-extentions.js',
                '/wwwroot/scripts/vendor/angular-dynamic-locale.js',
                '/wwwroot/scripts/vendor/locales/moment-locales.js',
                '/wwwroot/scripts/vendor/angular-slider.js',
                '/wwwroot/scripts/vendor/angular-bootstrap-calendar-tpls.js',
                '/wwwroot/scripts/vendor/dragular.js',
                '/wwwroot/scripts/vendor/angucomplete-alt-custom.js',
                '/wwwroot/scripts/vendor/ngclipboard.js',
                '/wwwroot/scripts/vendor/moment-business-days.js',
                '/wwwroot/scripts/vendor/moment-weekdaysin.js'
            ],
            styles: [
                '/wwwroot/styles/vendor/angular-block-ui.css',
                '/wwwroot/styles/vendor/angular-bootstrap-calendar.css',
                '/wwwroot/styles/vendor/angular-motion.css',
                '/wwwroot/styles/vendor/angular-resizable.css',
                '/wwwroot/styles/vendor/angular-ui-tree.css',
                '/wwwroot/styles/vendor/bootstrap-additions.css',
                '/wwwroot/styles/vendor/dragular.css',
                '/wwwroot/styles/vendor/flaticon.css',
                '/wwwroot/styles/vendor/font-awesome.css',
                '/wwwroot/styles/vendor/ladda-themeless.css',
                '/wwwroot/styles/vendor/ng-table.css',
                '/wwwroot/styles/vendor/ng-tags-input.bootstrap.css',
                '/wwwroot/styles/vendor/ng-tags-input.css',
                '/wwwroot/styles/vendor/ngToast.css',
                '/wwwroot/styles/vendor/select.css',
                '/wwwroot/styles/vendor/xeditable.css',
                '/wwwroot/styles/ui.css'
            ],
            options: {
                maps: false,
                uglify: true,
                minCSS: true,
                rev: false
            }
        },
        app: {
            scripts: [
                '/wwwroot/scripts/app.js',
                '/wwwroot/scripts/interceptors.js',
                '/wwwroot/scripts/routes.js',
                '/wwwroot/scripts/config.js',
                '/wwwroot/scripts/constants.js',
                '/wwwroot/scripts/utils.js',
                '/wwwroot/scripts/directives.js',
                '/wwwroot/scripts/filters.js',
                '/wwwroot/views/app/authService.js',
                '/wwwroot/views/app/appController.js',
                '/wwwroot/views/app/appService.js',
                '/wwwroot/views/app/crmController.js',
                '/wwwroot/views/app/note/noteService.js',
                '/wwwroot/views/app/note/noteDirective.js',
                '/wwwroot/views/app/documents/documentService.js',
                '/wwwroot/views/app/documents/documentDirective.js',
                '/wwwroot/views/app/module/moduleService.js',
                '/wwwroot/views/app/setup/help/helpService.js',
                '/wwwroot/views/app/setup/setupController.js',
                '/wwwroot/views/app/setup/payment/paymentService.js',
                '/wwwroot/views/app/setup/payment/paymentDirective.js',
                '/wwwroot/views/app/setup/workgroups/workgroupService.js',
                '/wwwroot/views/app/setup/messaging/messagingService.js',
                '/wwwroot/views/app/payment/paymentFormController.js',
                '/wwwroot/views/app/join/joinController.js',
                '/wwwroot/views/app/phone/sipPhoneController.js'


            ],
			styles: '/wwwroot/styles/app.css',
            options: {
                maps: false,
                uglify: true,
                minCSS: true,
                rev: false
            }
        }
    }
};