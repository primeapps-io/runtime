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
        },
        vendor: {
            scripts: [
                './scripts/vendor/angular.js',
                './scripts/vendor/angular-ui-router.js',
                './scripts/vendor/ocLazyLoad.js',
                './scripts/vendor/angular-cookies.js',
                './scripts/vendor/angular-translate.js',
                './scripts/vendor/angular-animate.js',
                './scripts/vendor/angular-sanitize.js',
                './scripts/vendor/angular-strap.js',
                './scripts/vendor/angular-strap.tpl.js',
                './scripts/vendor/angular-ui-bootstrap-custom.js',
                './scripts/vendor/angular-ui-bootstrap-custom-tpls.js',
                './scripts/vendor/angular-xeditable.js',
                './scripts/vendor/angular-ladda.js',
                './scripts/vendor/angular-filter.js',
                './scripts/vendor/angular-ui-utils.js',
                './scripts/vendor/angular-ui-tinymce.js',
                './scripts/vendor/mentio/mentio.js',
                './scripts/vendor/ng-table.js',
                './scripts/vendor/spin.js',
                './scripts/vendor/ladda.js',
                './scripts/vendor/moment.js',
                './scripts/vendor/es5-shim.js',
                './scripts/vendor/es5-sham.js',
                './scripts/vendor/angular-file-upload.js',
                './scripts/vendor/angular-bootstrap-show-errors.js',
                './scripts/vendor/ngToast.js',
                './scripts/vendor/angular-block-ui.js',
                './scripts/vendor/angular-touch.js',
                './scripts/vendor/ng-sortable.js',
                './scripts/vendor/file-saver.js',
                './scripts/vendor/ng-img-crop.js',
                './scripts/vendor/angular-images-resizer.js',
                './scripts/vendor/angular-ui-tree.js',
                './scripts/vendor/plupload.full.js',
                './scripts/vendor/angular-plupload.js',
                './scripts/vendor/ng-tags-input.js',
                './scripts/vendor/angular-ui-mask.js',
                './scripts/vendor/powerbi.js',
                './scripts/vendor/ace/ace.js',
                './scripts/vendor/angular-ui-ace.js',
                './scripts/vendor/ace/ext-language_tools.js',
                './scripts/vendor/angular-ui-select.js',
                './scripts/vendor/angular-resizable.js',
                './scripts/vendor/clipboard.js',
                './scripts/vendor/angular-translate-extentions.js',
                './scripts/vendor/angular-dynamic-locale.js',
                './scripts/vendor/locales/moment-locales.js',
                './scripts/vendor/angular-slider.js',
                './scripts/vendor/angular-bootstrap-calendar-tpls.js',
                './scripts/vendor/dragular.js',
                './scripts/vendor/angucomplete-alt-custom.js',
                './scripts/vendor/ngclipboard.js',
                './scripts/vendor/moment-business-days.js',
                './scripts/vendor/moment-weekdaysin.js'
            ],
            styles: [
                './styles/vendor/angular-block-ui.css',
                './styles/vendor/angular-bootstrap-calendar.css',
                './styles/vendor/angular-motion.css',
                './styles/vendor/angular-resizable.css',
                './styles/vendor/angular-ui-tree.css',
                './styles/vendor/bootstrap-additions.css',
                './styles/vendor/dragular.css',
                './styles/vendor/flaticon.css',
                './styles/vendor/font-awesome.css',
                './styles/vendor/ladda-themeless.css',
                './styles/vendor/ng-table.css',
                './styles/vendor/ng-tags-input.bootstrap.css',
                './styles/vendor/ng-tags-input.css',
                './styles/vendor/ngToast.css',
                './styles/vendor/select.css',
                './styles/vendor/xeditable.css',
                './styles/ui.css'
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
                './scripts/app.js',
                './scripts/interceptors.js',
                './scripts/routes.js',
                './scripts/config.js',
                './scripts/constants.js',
                './scripts/utils.js',
                './scripts/directives.js',
                './scripts/filters.js',
                './views/app/authService.js',
                './views/app/appController.js',
                './views/app/appService.js',
                './views/app/crm/crmController.js',
                './views/app/crm/note/noteService.js',
                './views/app/crm/note/noteDirective.js',
                './views/app/crm/documents/documentService.js',
                './views/app/crm/documents/documentDirective.js',
                './views/app/crm/module/moduleService.js',
                './views/app/setup/crm/help/helpService.js',
                './views/app/setup/setupController.js',
                './views/app/setup/crm/payment/paymentService.js',
                './views/app/setup/crm/payment/paymentDirective.js',
                './views/app/setup/crm/workgroups/workgroupService.js',
                './views/app/setup/crm/messaging/messagingService.js',
                './views/app/crm/payment/paymentFormController.js',
                './views/app/crm/join/joinController.js',
                './views/app/crm/phone/sipPhoneController.js'


            ],
            styles: './styles/app.css',
            options: {
                maps: false,
                uglify: true,
                minCSS: true,
                rev: false
            }
        }
    }
};