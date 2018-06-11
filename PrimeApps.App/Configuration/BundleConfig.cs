﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PrimeApps.App
{
    public partial class Startup
    {
	    public static void RegisterBundle(IServiceCollection services)
	    {
			services.AddWebOptimizer(pipeline =>
			{
				pipeline.AddJavaScriptBundle("/scripts/bundles-js/auth.js",
					"/scripts/vendor/jquery.js",
					"/scripts/vendor/jquery-maskedinput.js",
					"/scripts/vendor/sweetalert.js",
					"/scripts/vendor/spin.js",
					"/scripts/vendor/ladda.js");

				/*pipeline.AddBundle("/scripts/bundles-js/app.js", "text/javascript; charset=UTF-8",*/
				pipeline.AddJavaScriptBundle("/scripts/bundles-js/app.js",
					"scripts/app.js",
					"scripts/interceptors.js",
					"scripts/routes.js",
					"scripts/config.js",
					"scripts/constants.js",
					"scripts/utils.js",
					"scripts/directives.js",
					"scripts/filters.js",
					"views/authService.js",
					"views/appController.js",
					"views/appService.js",
					"views/app/crmController.js",
					"views/app/note/noteService.js",
					"views/app/note/noteDirective.js",
					"views/app/documents/documentService.js",
					"views/app/documents/documentDirective.js",
					"views/app/module/moduleService.js",
					"views/setup/help/helpService.js",
					"views/setup/setupController.js",
					"views/setup/payment/paymentService.js",
					"views/setup/payment/paymentDirective.js",
					"views/setup/workgroups/workgroupService.js",
					"views/setup/messaging/messagingService.js",
					"views/app/payment/paymentFormController.js",
					"views/app/join/joinController.js",
					"views/app/phone/sipPhoneController.js");

				pipeline.AddJavaScriptBundle("/scripts/bundles-js/vendor.js",
					"scripts/vendor/angular.js",
					"scripts/vendor/angular-ui-router.js",
					"scripts/vendor/ocLazyLoad.js",
					"scripts/vendor/angular-cookies.js",
					"scripts/vendor/angular-translate.js",
					"scripts/vendor/angular-animate.js",
					"scripts/vendor/angular-sanitize.js",
					"scripts/vendor/angular-strap.js",
					"scripts/vendor/angular-strap.tpl.js",
					"scripts/vendor/angular-ui-bootstrap-custom.js",
					"scripts/vendor/angular-ui-bootstrap-custom-tpls.js",
					"scripts/vendor/angular-xeditable.js",
					"scripts/vendor/angular-ladda.js",
					"scripts/vendor/angular-ui-utils.js",
					"scripts/vendor/angular-ui-tinymce.js",
					"scripts/vendor/mentio/mentio.js",
					"scripts/vendor/ng-table.js",
					"scripts/vendor/spin.js",
					"scripts/vendor/ladda.js",
					"scripts/vendor/moment.js",
					"scripts/vendor/es5-shim.js",
					"scripts/vendor/es5-sham.js",
					"scripts/vendor/angular-file-upload.js",
					"scripts/vendor/angular-bootstrap-show-errors.js",
					"scripts/vendor/ngToast.js",
					"scripts/vendor/angular-block-ui.js",
					"scripts/vendor/angular-touch.js",
					"scripts/vendor/ng-sortable.js",
					"scripts/vendor/file-saver.js",
					"scripts/vendor/ng-img-crop.js",
					"scripts/vendor/angular-images-resizer.js",
					"scripts/vendor/angular-ui-tree.js",
					"scripts/vendor/plupload.full.js",
					"scripts/vendor/angular-plupload.js",
					"scripts/vendor/ng-tags-input.js",
					"scripts/vendor/angular-ui-mask.js",
					"scripts/vendor/powerbi.js",
					"scripts/vendor/ace/ace.js",
					"scripts/vendor/angular-ui-ace.js",
					"scripts/vendor/ace/ext-language_tools.js",
					"scripts/vendor/angular-ui-select.js",
					"scripts/vendor/angular-resizable.js",
					"scripts/vendor/clipboard.js",
					"scripts/vendor/angular-translate-extentions.js",
					"scripts/vendor/angular-dynamic-locale.js",
					"scripts/vendor/locales/moment-locales.js",
					"scripts/vendor/angular-slider.js",
					"scripts/vendor/angular-bootstrap-calendar-tpls.js",
					"scripts/vendor/dragular.js",
					"scripts/vendor/angucomplete-alt-custom.js",
					"scripts/vendor/ngclipboard.js",
					"scripts/vendor/moment-business-days.js",
					"scripts/vendor/moment-weekdaysin.js");

				pipeline.AddCssBundle("/styles/bundles-css/auth.css",
					"styles/vendor/bootstrap.css",
					"styles/vendor/flaticon.css",
					"styles/vendor/ladda-themeless.css",
					"styles/vendor/font-awesome.css");

				pipeline.AddCssBundle("/styles/bundles-css/vendor.css",
					"styles/vendor/angular-block-ui.css",
					"styles/vendor/angular-bootstrap-calendar.css",
					"styles/vendor/angular-motion.css",
					"styles/vendor/angular-resizable.css",
					"styles/vendor/angular-ui-tree.css",
					"styles/vendor/bootstrap-additions.css",
					"styles/vendor/dragular.css",
					"styles/vendor/flaticon.css",
					"styles/vendor/font-awesome.css",
					"styles/vendor/ladda-themeless.css",
					"styles/vendor/ng-table.css",
					"styles/vendor/ng-tags-input.bootstrap.css",
					"styles/vendor/ng-tags-input.css",
					"styles/vendor/ngToast.css",
					"styles/vendor/select.css",
					"styles/vendor/xeditable.css",
					"styles/ui.css");

				pipeline.AddCssBundle("/styles/bundles-css/app.css",
					"styles/app.css");
			});
		}
    }
}
