"use strict";angular.module("primeapps").controller("ExportDataController",["$rootScope","$scope","$filter","$window","$mdDialog","mdToast",function(e,t,a,l,o,i){t.getDownloadViewUrlExcel=function(){var o=t.module.name,n=t.activeView.id,d=e.user.profile.id,s=t["export"].moduleAllColumn,c=Object.assign({},t.findRequest);delete c.aggregations,delete c.group,delete c.groupDate,l.open("/attach/export_excel_view?module="+o+"&viewId="+n+"&profileId="+d+"&listFindRequestJson="+JSON.stringify(c)+"&isViewFields="+!s+"&locale="+e.locale+(e.preview?"&appId="+e.user.app_id:""),"_blank"),i.success(a("translate")("Module.ExcelDesktop"))},t.excelNoData=function(){var o=t.module.name,n=t.quoteTemplate.id,d=t.quoteTemplate.name,s=e.activeView.id;l.open("/attach/export_excel_no_data?module="+o+"&viewId="+s+"&templateId="+n+"&templateName="+d+"&locale="+e.locale+"&listFindRequestJson="+JSON.stringify(t.findRequest),"_blank"),i.success(a("translate")("Module.ExcelDesktop"))},t.excelData=function(){var o=t.module.name,n=t.quoteTemplate.id,d=t.quoteTemplate.name,s=e.activeView.id;l.open("/attach/export_excel_data?module="+o+"&viewId="+s+"&templateId="+n+"&templateName="+d+"&locale="+e.locale+"&listFindRequestJson="+JSON.stringify(t.findRequest),"_blank"),i.success(a("translate")("Module.ExcelDesktop"))},t.cancel=function(){o.cancel()},t.quoteTemplatesOptions={dataSource:a("filter")(t.quoteTemplates,{active:!0,isShown:!0}),dataTextField:"name",dataValueField:"id"}}]);