"use strict";function getCode(e,a){var t=new XMLHttpRequest;t.open("GET",e,!1),t.onload=function(){return 200===this.status?a(t.responseText):void 0},t.send({})}var runController=function(code,callback){return angular.isObject(code)?(eval(code),callback(!0)):void getCode(code.url,function(result){return eval(result),callback(result)})};angular.module("primeapps").controller("DashboardController",["$rootScope","$scope","helper","$filter","$cache","DashboardService","ModuleService","$window","$state","$mdDialog","$timeout","HelpService","$sce","$mdSidenav","mdToast","operations",function(e,a,t,r,n,s,o,l,i,d,c,h,u,g,b,p){if(e.$on("error",function(e,a){b.warning(r("translate")(a))}),a.currentPath.indexOf("/dashboard")<0)if(a.menu){var f=r("filter")(a.menu,{route:"dashboard"},!0)[0];a.openSubMenu(f,a.menu)}else a.$parent.currentPath="/app/dashboard";a.hasPermission=t.hasPermission,a.isFullScreen=!0,a.loading=!0,a.loadingchanges=!1,a.disableSaveBtn=!0,a.showNewDashboardSetting=r("filter")(e.moduleSettings,{key:"new_dashboard"},!0)[0],e.sideinclude=!1,e.breadcrumblist=[],e.dashboardHelpSide||h.getByType("sidemodal",null,"/app/dashboard").then(function(a){e.dashboardHelpSide=a.data}),l.scrollTo(0,0);var m=e.user.profile.start_page.toLowerCase();if("dashboard"!==m)return void(window.location="#/app/"+m);a.colorPaletOptions={columns:6,palette:["#D24D57","#BE90D4","#5AABE3","#87D37C","#F4D03E","#B8BEC2","#DC3023","#8E44AD","#19B5FE","#25A65B","#FFB61E","#959EA4","#C3272B","#763668","#1F4688","#006442","#CA6924","#4D5C66"]},a.sideModalLeft=function(){e.buildToggler("sideModal","view/app/dashboard/dashboardOrderChange.html",a)},a.showConfirm=function(e,t){var n=d.confirm().title(r("translate")("Common.AreYouSure")).targetEvent(e).ok(r("translate")("Common.Yes")).cancel(r("translate")("Common.No"));d.show(n).then(function(){a.dashletDelete(t)},function(){a.status="You decided to keep your debt."})},a.endHandler=function(e){e.sender;e.sender.draggable.dropped=!0,e.preventDefault(),c(function(){a.$apply(function(){a.dashlets.splice(e.newIndex,0,a.dashlets.splice(e.oldIndex,1)[0])})}),a.disableSaveBtn=!1},e.sortableOptions={placeholder:function(e){return e.clone().addClass("sortable-list-placeholder").text(e.innerText)},hint:function(e){return e.clone().addClass("sortable-list-hint")},cursorOffset:{top:-10,left:20}},a.showFullScreen=function(e){a.fullScreenDashlet=a.fullScreenDashlet?null:e},a.openMenu=function(){},a.DashletTypes=[{label:r("translate")("Dashboard.Chart"),name:"chart"},{label:r("translate")("Dashboard.Widget"),name:"widget"}];var D=r("translate")("Dashboard.Column");a.dashletWidths=[{label:"1 "+D,value:3},{label:"2 "+D,value:6},{label:"3 "+D,value:9},{label:"4 "+D,value:12}],a.dashletHeights=[{label:"300 px",value:330},{label:"400 px",value:430},{label:"500 px",value:530},{label:"600 px",value:630}],a.getUser=function(a){var t=r("filter")(e.users,{id:a},!0)[0];return t.full_name?t.full_name:a},a.showComponent=[];var v=[];if(components)for(var _=JSON.parse(components),w=0;w<_.length;w++)v["component-"+_[w].Id]=_[w];a.loadDashboard=function(){var t=function(){a.activeDashboard=n.get("activeDashboard"),s.getDashlets(a.activeDashboard.id).then(function(t){t=t.data,e.processLanguages(t);for(var s=0;s<t.length;s++){var o=t[s];if("component"===o.dashlet_type&&(n.get("component-"+o.id)?runController(n.get(a.showComponent[o.id]),function(){a.componetiGoster=!0}):runController(!1,function(e){a.showComponent[o.id]=!0,n.put(n.get("component-"+id),e)})),"chart"===o.dashlet_type){o.chart_item.config={dataEmptyMessage:r("translate")("Dashboard.ChartEmptyMessage")},o.chart_item.chart.showPercentValues="1",o.chart_item.chart.showPercentInTooltip="0",o.chart_item.chart.animateClockwise="1",o.chart_item.chart.enableMultiSlicing="0",o.chart_item.chart.isHollow="0",o.chart_item.chart.is2D="0",o.chart_item.chart.formatNumberScale="0",o.chart_item.chart.xaxisname=o.chart_item.chart.languages[e.globalization.Label].xaxis_name,o.chart_item.chart.yaxisname=o.chart_item.chart.languages[e.globalization.Label].yaxis_name,o.chart_item.chart.caption=o.chart_item.chart.languages[e.globalization.Label].caption,e.languageStringify(o.chart_item.chart),"tr"===a.locale&&(o.chart_item.chart.decimalSeparator=",",o.chart_item.chart.thousandSeparator=".");var l=r("filter")(e.modules,{id:o.chart_item.chart.report_module_id},!0)[0];if(l){var i;if(o.chart_item.chart.report_aggregation_field.indexOf(".")<0)i=r("filter")(l.fields,{name:o.chart_item.chart.report_aggregation_field},!0)[0];else{var d=o.chart_item.chart.report_aggregation_field.split("."),c=r("filter")(e.modules,{name:d[1]},!0)[0];i=r("filter")(c.fields,{name:d[2]},!0)[0]}i&&"currency"===i.data_type&&(o.chart_item.chart.numberPrefix=e.currencySymbol),!i||"currency"!==i.data_type&&"number_decimal"!==i.data_type||(o.chart_item.chart.forceDecimals="1")}}}a.dashlets=t,n.put("dashlets",t)})["finally"](function(){a.loading=!1})};void 0===a.showNewDashboardSetting||null===a.showNewDashboardSetting||"true"===a.showNewDashboardSetting.value?(a.showNewDashboard=!0,t()):a.getSummaryJsonValue=function(e){var a=angular.fromJson(e);return a.x};for(var l=0;l<e.modules.length;l++)o.getPicklists(e.modules[l]);a.$on("sample-data-removed",function(){t()}),a.goDetail=function(e,a){0!==e&&(window.location="#/app/modules/"+a+"?viewid="+e)}};var y=n.get("dashlets"),S=n.get("activeDashboard"),C=n.get("dashboards"),L=n.get("dashboardprofile");y&&(a.loading=!1,a.dashlets=y),S&&C?(a.dashboards=C,e.processLanguages(a.dashboards),a.activeDashboard=S,a.dashboardprofile=L,a.loadDashboard()):s.getDashboards().then(function(t){a.dashboards=t.data,e.processLanguages(a.dashboards),a.activeDashboard=r("filter")(a.dashboards,{sharing_type:"me",user_id:e.user.id},!0)[0],a.activeDashboard||(a.activeDashboard=e.user.profile.has_admin_rights?r("filter")(a.dashboards,{sharing_type:"everybody"},!0)[0]:r("filter")(a.dashboards,{sharing_type:"profile",profile_id:e.user.profile.id},!0)[0],a.activeDashboard||(a.activeDashboard=r("filter")(a.dashboards,{sharing_type:"everybody"},!0)[0])),a.dashboardprofile=[],angular.forEach(e.profiles,function(t){var n=r("filter")(a.dashboards,{sharing_type:"profile",profile_id:t.id},!0)[0];n||(t.name=e.getLanguageValue(t.languages,"name"),a.dashboardprofile.push(t))}),n.put("activeDashboard",a.activeDashboard),n.put("dashboards",a.dashboards),n.put("dashboardprofile",a.dashboardprofile),a.loadDashboard()}),a.changeDashboard=function(e){a.loading=!0,n.put("activeDashboard",e),a.loadDashboard()},a.hide=function(){d.hide()},a.cancel=function(){d.cancel()},a.dashboardformModal=function(t,r){var n={};n[e.globalization.Label]={name:"",description:""},a.currentDashboard=r?angular.copy(r):{languages:n};var s=angular.element(document.body);d.show({parent:s,templateUrl:"view/app/dashboard/formModal.html",clickOutsideToClose:!0,targetEvent:t,scope:a,preserveScope:!0})},a.cancelChangeOrder=function(){a.dashlets=a.currentDashlet},a.saveDashboard=function(t,o){if(o.preventDefault(),t.$submitted&&t.$valid){var l={languages:{}};if(l.languages[e.globalization.Label]={name:e.getLanguageValue(a.currentDashboard.languages,"name"),description:e.getLanguageValue(a.currentDashboard.languages,"description")},e.languageStringify(l),a.currentDashboard.id)l.id=a.currentDashboard.id,s.updateDashboard(l).then(function(t){for(var n=0;n<a.dashboards.length;n++)a.dashboards[n].id===t.data.id&&(e.processLanguage(t.data),a.dashboards[n]=t.data);d.cancel(),b.success(r("translate")("Dashboard.DashboardSaveSucces"))});else{l.profile_id=a.currentDashboard.profile_id,l.sharing_type=3,a.loading=!0;{angular.copy(a.activeDashboard)}s.createDashbord(l).then(function(){a.hide(),n.remove("dashlets"),n.remove("activeDashboard"),n.remove("dashboards"),n.remove("dashboardprofile"),i.reload(),d.cancel(),b.success(r("translate")("Dashboard.DashboardSaveSucces"))})}}else t.dash.$valid||b.error(r("translate")("Dashboard.ProfileRequired"))},a.saveDashlet=function(t,n){if(n.preventDefault(),a.validator.validate()){var o={dashlet_type:a.currentDashlet.dashlet_type,dashboard_id:a.activeDashboard.id,y_tile_length:a.currentDashlet.y_tile_length,x_tile_height:a.currentDashlet.x_tile_height,languages:{}};if(o.languages[e.globalization.Label]={name:e.getLanguageValue(a.currentDashlet.languages,"name")},"chart"===a.currentDashlet.dashlet_type)o.chart_id=a.currentDashlet.board;else{if(!a.currentDashlet.dataSource)return void b.error(r("translate")("Module.RequiredError"));o.widget_id=a.currentDashlet.board,o.y_tile_length=3,o.x_tile_height=150,o.view_id=a.currentDashlet.view_id,o.color=a.currentDashlet.color,o.icon=a.currentDashlet.icon}a.hide(),a.loading=!0,e.languageStringify(o),a.currentDashlet.id?s.dashletUpdate(a.currentDashlet.id,o).then(function(){a.loadDashboard(),b.success(r("translate")("Dashboard.DashletUpdateSucces"))}):(o.order=a.dashlets?a.dashlets.length:0,s.createDashlet(o).then(function(){a.loadDashboard(),b.success(r("translate")("Dashboard.DashletSaveSucces"))}))}else b.error(r("translate")("Module.RequiredError"))},a.dashletOrderSave=function(){a.loadingchanges=!0,a.sideModalLeft(),s.dashletOrderChange(a.dashlets).then(function(){a.loadDashboard(),a.loadingchanges=!1,b.success(r("translate")("Dashboard.DashletUpdateSucces")),e.closeSide("sideModal")}),a.disableSaveBtn=!0},a.openNewDashlet=function(t,r){a.currentDashlet={languages:{}},a.currentDashlet.languages[e.globalization.Label]={name:"",description:""},r&&(a.currentDashlet=angular.copy(r),r.chart_item?(a.currentDashlet.name=a.currentDashlet.chart_item.chart.caption,a.currentDashlet.board=a.currentDashlet.chart_item.chart.id):(a.currentDashlet.board=a.currentDashlet.widget.id,a.currentDashlet.widget.view_id?(a.currentDashlet.dataSource="view",s.getView(a.currentDashlet.widget.view_id).then(function(e){a.currentDashlet.module_id=e.data.module_id,a.setViews(),a.currentDashlet.view_id=a.currentDashlet.widget.view_id,a.currentDashlet.color=a.currentDashlet.widget.color,a.currentDashlet.icon=a.currentDashlet.widget.icon})):a.currentDashlet.dataSource="report"),a.changeDashletType());var n=angular.element(document.body);d.show({parent:n,templateUrl:"view/app/dashboard/formModalDashlet.html",clickOutsideToClose:!0,targetEvent:t,scope:a,preserveScope:!0})},a.changeDashletType=function(){!a.currentDashlet.dashlet_type||"chart"!==a.currentDashlet.dashlet_type.name&&"chart"!==a.currentDashlet.dashlet_type?s.getWidgets().then(function(t){a.boards=t.data,e.processLanguages(a.boards),a.boardLabel=r("translate")("Report.Single")}):s.getCharts().then(function(t){a.boards=t.data,e.processLanguages(a.boards),a.boardLabel=a.DashletTypes[0].label})},a.selectModule=function(){a.setViews()},a.setViews=function(){s.getViews(a.currentDashlet.module_id).then(function(t){a.views=t.data,e.processLanguages(a.views)})},a.dashletDelete=function(e){s.dashletDelete(e).then(function(){a.loadDashboard(),b.success(r("translate")("Dashboard.DashletDeletedSucces"))})},a.changeDashletMode=function(){a.dashletMode=a.dashletMode!==!0},a.changeView=function(){const t=r("filter")(a.views,{id:a.currentDashlet.view_id},!0)[0];a.currentDashlet.languages[e.globalization.Label].name=t?e.getLanguageValue(t.languages,"label"):void 0},a.changeBoard=function(){const t=r("filter")(a.boards,{id:a.currentDashlet.board},!0)[0];a.currentDashlet.languages[e.globalization.Label].name=t?e.getLanguageValue(t.languages,"name"):void 0},"undefined"!=typeof Tawk_API&&(Tawk_API.visitor={name:e.user.full_name,email:e.user.email}),a.modulesOpt=r("filter")(a.modules,function(e){return"roles"!==e.name&&"users"!==e.name&&"profiles"!==e.name&&"component"!==e.system_type&&t.hasPermission(e.name,p.read)})}]);