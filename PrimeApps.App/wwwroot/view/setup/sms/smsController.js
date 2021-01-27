"use strict";angular.module("primeapps").controller("SmsController",["$rootScope","$scope","$filter","SmsService","$mdDialog","mdToast","$stateParams","helper","$state","AppService",function(s,e,t,o,i,a,n,r,l,m){e.loading=!1,s.globalLoading=!1,m.checkPermission().then(function(n){if(n&&n.data){var r=JSON.parse(n.data.profile),m=void 0;if(n.data.customProfilePermissions&&(m=JSON.parse(n.data.customProfilePermissions)),!r.HasAdminRights){var S=void 0;m&&(S=m.permissions.indexOf("sms")>-1),S||l.go("app.setup.email")}}s.breadcrumblist=[{title:t("translate")("Setup.Nav.System"),link:"#/app/setup/sms"},{title:t("translate")("Setup.Messaging.SMS.Title")}],e.smsModel=angular.copy(s.system.messaging.SMS)||{},e.goUrl=function(s){window.location=s},e.editSMS=function(){e.systemForm.$valid&&(e.loading=!0,s.globalLoading=!0,o.updateSMSSettings(e.smsModel).then(function(){a.success(t("translate")("Setup.Settings.UpdateSuccess")),s.system.messaging.SMS||(s.system.messaging.SMS={}),s.system.messaging.SMS.provider=e.smsModel.provider,s.system.messaging.SMS.user_name=e.smsModel.user_name,s.system.messaging.SMS.alias=e.smsModel.alias,e.loading=!1,s.globalLoading=!1})["catch"](function(){e.loading=!1,s.globalLoading=!1,e.systemForm.$submitted=!1,a.error(t("translate")("Common.Error"))}))},e.resetSMSForm=function(){e.smsModel=angular.copy(s.system.messaging.SMS)},e.removeSMSSettings=function(){o.removeSMSSettings(e.smsModel).then(function(){e.smsModel=null,s.system.messaging.SMS=null})},e.close=function(){i.hide()},e.submitGeneral=function(){return e.systemForm.$valid?void e.editSMS(e.smsModel):void a.error(t("translate")("Module.RequiredError"))}})}]);