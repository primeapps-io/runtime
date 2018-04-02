module.exports = {
    bundle: {
        auth: {
            scripts: [
                './web/scripts/vendor/jquery.js',
                './web/scripts/vendor/jquery-maskedinput.js',
                './web/scripts/vendor/sweetalert.js',
                './web/scripts/vendor/spin.js',
                './web/scripts/vendor/ladda.js'
            ],
            styles: [
                './web/styles/vendor/bootstrap.css',
                './web/styles/vendor/flaticon.css',
                './web/styles/vendor/ladda-themeless.css',
                './web/styles/vendor/font-awesome.css'
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
                './web/scripts/vendor/angular.js',
                './web/scripts/vendor/angular-ui-router.js',
                './web/scripts/vendor/ocLazyLoad.js',
                './web/scripts/vendor/angular-cookies.js',
                './web/scripts/vendor/angular-translate.js',
                './web/scripts/vendor/angular-animate.js',
                './web/scripts/vendor/angular-sanitize.js',
                './web/scripts/vendor/angular-strap.js',
                './web/scripts/vendor/angular-strap.tpl.js',
                './web/scripts/vendor/angular-ui-bootstrap-custom.js',
                './web/scripts/vendor/angular-ui-bootstrap-custom-tpls.js',
                './web/scripts/vendor/angular-xeditable.js',
                './web/scripts/vendor/angular-ladda.js',
                './web/scripts/vendor/angular-ui-utils.js',
                './web/scripts/vendor/angular-ui-tinymce.js',
                './web/scripts/vendor/mentio/mentio.js',
                './web/scripts/vendor/ng-table.js',
                './web/scripts/vendor/spin.js',
                './web/scripts/vendor/ladda.js',
                './web/scripts/vendor/moment.js',
                './web/scripts/vendor/es5-shim.js',
                './web/scripts/vendor/es5-sham.js',
                './web/scripts/vendor/angular-file-upload.js',
                './web/scripts/vendor/angular-bootstrap-show-errors.js',
                './web/scripts/vendor/ngToast.js',
                './web/scripts/vendor/angular-block-ui.js',
                './web/scripts/vendor/angular-touch.js',
                './web/scripts/vendor/ng-sortable.js',
                './web/scripts/vendor/file-saver.js',
                './web/scripts/vendor/ng-img-crop.js',
                './web/scripts/vendor/angular-images-resizer.js',
                './web/scripts/vendor/angular-ui-tree.js',
                './web/scripts/vendor/plupload.full.js',
                './web/scripts/vendor/angular-plupload.js',
                './web/scripts/vendor/ng-tags-input.js',
                './web/scripts/vendor/angular-ui-mask.js',
                './web/scripts/vendor/powerbi.js',
                './web/scripts/vendor/ace/ace.js',
                './web/scripts/vendor/angular-ui-ace.js',
                './web/scripts/vendor/ace/ext-language_tools.js',
                './web/scripts/vendor/angular-ui-select.js',
                './web/scripts/vendor/angular-resizable.js',
                './web/scripts/vendor/clipboard.js',
                './web/scripts/vendor/angular-translate-extentions.js',
                './web/scripts/vendor/angular-dynamic-locale.js',
                './web/scripts/vendor/locales/moment-locales.js',
                './web/scripts/vendor/angular-slider.js',
                './web/scripts/vendor/angular-bootstrap-calendar-tpls.js',
                './web/scripts/vendor/dragular.js',
                './web/scripts/vendor/angucomplete-alt-custom.js',
                './web/scripts/vendor/ngclipboard.js',
                './web/scripts/vendor/moment-business-days.js',
                './web/scripts/vendor/moment-weekdaysin.js'
            ],
            styles: [
                './web/styles/vendor/angular-block-ui.css',
                './web/styles/vendor/angular-bootstrap-calendar.css',
                './web/styles/vendor/angular-motion.css',
                './web/styles/vendor/angular-resizable.css',
                './web/styles/vendor/angular-ui-tree.css',
                './web/styles/vendor/bootstrap-additions.css',
                './web/styles/vendor/dragular.css',
                './web/styles/vendor/flaticon.css',
                './web/styles/vendor/font-awesome.css',
                './web/styles/vendor/ladda-themeless.css',
                './web/styles/vendor/ng-table.css',
                './web/styles/vendor/ng-tags-input.bootstrap.css',
                './web/styles/vendor/ng-tags-input.css',
                './web/styles/vendor/ngToast.css',
                './web/styles/vendor/select.css',
                './web/styles/vendor/xeditable.css',
                './web/styles/ui.css'
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
                './web/scripts/app.js',
                './web/scripts/interceptors.js',
                './web/scripts/routes.js',
                './web/scripts/config.js',
                './web/scripts/constants.js',
                './web/scripts/utils.js',
                './web/scripts/directives.js',
                './web/scripts/filters.js',
                './web/views/app/authService.js',
                './web/views/app/appController.js',
                './web/views/app/appService.js',
                './web/views/app/crm/crmController.js',
                './web/views/app/crm/note/noteService.js',
                './web/views/app/crm/note/noteDirective.js',
                './web/views/app/crm/documents/documentService.js',
                './web/views/app/crm/documents/documentDirective.js',
                './web/views/app/crm/module/moduleService.js',
                './web/views/app/setup/crm/help/helpService.js',
                './web/views/app/setup/setupController.js',
                './web/views/app/setup/crm/payment/paymentService.js',
                './web/views/app/setup/crm/payment/paymentDirective.js',
                './web/views/app/setup/crm/workgroups/workgroupService.js',
                './web/views/app/setup/crm/messaging/messagingService.js',
                './web/views/app/crm/payment/paymentFormController.js',
                './web/views/app/crm/join/joinController.js',
                './web/views/app/crm/phone/sipPhoneController.js'


            ],
			styles: './web/styles/app.css',
            options: {
                maps: false,
                uglify: true,
                minCSS: true,
                rev: false
            }
        }
    }
};