"use strict";angular.module("primeapps").controller("AnalyticsFormController",["$rootScope","$scope","$cookies","$state","$location","$localStorage","ngToast","config","$window","$timeout","$filter","blockUI","helper","FileUploader","AnalyticsService","ModuleService",function(e,t,r,a,o,n,i,l,c,p,s,u,d,f,h,g){t.id=o.search().id,t.title=s("translate")("Setup.Report.NewReport"),t.lookupUser=d.lookupUser;var m=g.getIcons();if(t.icons=s("orderBy")(m,"chart"),!e.user.has_analytics)return i.create({content:s("translate")("Common.Forbidden"),className:"warning"}),void a.go("app.dashboard");h.getReports().then(function(e){t.analyticsReports=e.data}),t.id?(t.title=s("translate")("Setup.Report.EditReport"),h.get(t.id).then(function(e){t.reportModel=e.data;var r=t.reportModel.pbix_url;t.reportFileName=r.slice(r.indexOf("--")+2)})):(t.reportModel={},t.reportModel.sharing_type="everybody",t.reportModel.icon="fa fa-bar-chart");var v=function(e){t.saving=!1,a.go("app.analytics",{id:e})},F=t.uploader=new f({url:l.apiUrl+"analytics/save_pbix",headers:{Authorization:"Bearer "+n.read("access_token"),"X-Tenant-Id":r.get("tenant_id"),Accept:"application/json"},queueLimit:1});F.onCompleteItem=function(e,r,a){200===a&&(t.report.pbix_url=r,t.id?h.update(t.report).then(function(){v(t.report.id)})["catch"](function(){t.saving=!1}):h.create(t.report).then(function(e){v(e.data.id)})["catch"](function(){t.saving=!1}))},F.onWhenAddingFileFailed=function(e,t){switch(t.name){case"pbixFilter":i.create({content:s("translate")("Setup.Report.FormatError"),className:"warning"});break;case"sizeFilter":i.create({content:s("translate")("Setup.Report.SizeError"),className:"warning"})}},F.filters.push({name:"pbixFilter",fn:function(e){var t=d.getFileExtension(e.name);return"pbix"===t}}),F.filters.push({name:"sizeFilter",fn:function(e){return e.size<2097152}}),t.clearReportFile=function(){F.clearQueue(),t.reportFileName=void 0,t.reportFileCleared=!0},t.save=function(){if(t.analyticsForm.$valid&&(t.id||F.queue.length)&&(!t.id||!t.reportFileCleared||F.queue.length)){if(t.saving=!0,t.report=angular.copy(t.reportModel),t.report.shares&&t.report.shares.length){var e=angular.copy(t.report.shares);t.report.shares=[],angular.forEach(e,function(e){t.report.shares.push(e.id)})}!t.id||t.reportFileCleared?F.uploadAll():h.update(t.report).then(function(){v(t.report.id)})["catch"](function(){t.saving=!1})}},t.cancel=function(){a.go("app.analytics")}}]);