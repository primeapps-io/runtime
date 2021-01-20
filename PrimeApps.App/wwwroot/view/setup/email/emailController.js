"use strict";angular.module("primeapps").controller("EmailController",["$rootScope","$scope","$filter","EmailService","$mdDialog","mdToast","$stateParams","helper","$state","AppService",function(e,l,s,a,i,n,t,o,r,d){d.checkPermission().then(function(t){if(t&&t.data){var o=JSON.parse(t.data.profile),d=void 0;if(t.data.customProfilePermissions&&(d=JSON.parse(t.data.customProfilePermissions)),!o.HasAdminRights){var m=void 0;d&&(m=d.permissions.indexOf("email")>-1),m||(n.error(s("translate")("Common.Forbidden")),r.go("app.dashboard"))}}e.breadcrumblist=[{title:s("translate")("Layout.Menu.Dashboard"),link:"#/app/dashboard"},{title:s("translate")("Setup.Nav.System"),link:"#/app/setup/sms"},{title:s("translate")("Setup.Messaging.EMail.Title")}],l.emailModel=angular.copy(e.system.messaging.SystemEMail)||{},l.newSender={},l.loading=!1,l.goUrl=function(e){window.location=e},null!=l.emailModel&&l.emailModel.hasOwnProperty("provider")||(l.emailModel={provider:"",user_name:"",password:"",senders:[],enable_ssl:!0,dont_send_bulk_email_result:!1}),l.showNewSenderForm=function(){l.alias=null,l.email=null;var e=angular.element(document.body);i.show({parent:e,templateUrl:"view/setup/email/senderAdd.html",clickOutsideToClose:!0,scope:l,preserveScope:!0})},l.addNewSender=function(e,a){return this.senderForm.alias.$valid&&this.senderForm.email.$valid?(null==l.emailModel.senders&&(l.emailModel.senders=[]),l.emailModel.senders.push({alias:e,email:a}),l.close(),void l.systemForm.$setValidity("noSender",!0)):void n.error(s("translate")("Module.RequiredError"))},l.removeSender=function(e){if(null!=l.emailModel.senders){var s=l.emailModel.senders.indexOf(e);l.emailModel.senders.splice(s,1)}},l.editEMail=function(){0==l.emailModel.senders.length&&l.systemForm.$setValidity("noSender",!1),l.systemForm.$valid&&(l.loading=!0,l.emailModel.host.indexOf("yandex")>-1?(l.emailModel.host="smtp.yandex.ru",l.emailModel.port="587",l.emailModel.enable_ssl=!0):l.emailModel.provider="smtp",a.updateEMailSettings(l.emailModel).then(function(){n.success(s("translate")("Setup.Settings.UpdateSuccess")),a.getSetting().then(function(l){var s=l.data;s.SystemEMail&&(s.SystemEMail.enable_ssl="True"===s.SystemEMail.enable_ssl),s.SystemEMail&&s.SystemEMail.dont_send_bulk_email_result&&(s.SystemEMail.dont_send_bulk_email_result="True"===s.SystemEMail.dont_send_bulk_email_result),s.PersonalEMail&&(s.PersonalEMail.enable_ssl="True"===s.PersonalEMail.enable_ssl),s.PersonalEMail&&s.PersonalEMail.dont_send_bulk_email_result&&(s.PersonalEMail.dont_send_bulk_email_result="True"===s.PersonalEMail.dont_send_bulk_email_result),e.system.messaging=s}),l.loading=!1}))},l.resetEMailForm=function(){l.emailModel=angular.copy(e.system.messaging.SystemEMail)},l.removeEMailSettings=function(){a.removeEMailSettings(l.emailModel).then(function(){l.emailModel=null,e.system.messaging.SystemEMail=null})},l.close=function(){i.hide()},l.submitGeneral=function(){return l.systemForm.$valid?void l.editEMail(l.emailModel):void n.error(s("translate")("Module.RequiredError"))}})}]);