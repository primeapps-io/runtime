"use strict";angular.module("primeapps").directive("focus",["$timeout",function(e){return function(t,a,n){t.$watch(n.focus,function(t){t&&e(function(){a[0].focus()},0,!1)})}}]).directive("ngPriorityNav",["$timeout","$window","PriorityNavService","$interpolate",function(e,t,a){return{restrict:"A",priority:-999,link:function(n,l,r){function o(t,n,l,r,o){t.append(n).addClass("go-away"),o&&(l.children().remove(),t.children().remove()),e(function(){e(function(){o&&(u[0].style.cssText="line-height:1.5",a.addIds(t.children())),a.calculatebreakPoint(t,n,l,r),e(function(){t.removeClass("go-away")},200)})})}var i=angular.element('<div class="vertical-nav"><a href data-toggle="dropdown" aria-haspopup="true" aria-expanded="false" class="more-link btn btn-secondary btn-sm"><span class="bubble"></span></a><ul class="vertical-nav-dropdown dropdown-menu dropdowlist shadow"></ul></div>'),u=angular.element(i[0].querySelector(".more-link")),s=angular.element(i[0].querySelector(".vertical-nav-dropdown")),d=angular.element(i[0].querySelector(".bubble"));l.addClass("priority-nav"),l.addClass(r.ngPriorityNavClass),u.addClass(r.ngPriorityNavMoreLinkClass),s.addClass(r.ngPriorityNavDropDownClass),d.addClass(r.ngPriorityNavBubbleClass);var c=a.debounce(function(){o(l,i,s,d,!1)},500);r.ngPriorityNav?r.$observe("ngPriorityNav",function(e){e&&o(l,i,s,d,!0)},!0):o(l,i,s,d,!0),angular.element(t).on("resize",c).on("orientationchange",c)}}}]).directive("blur",function(){return function(e,t,a){t.bind("blur",function(){e.$apply(a.blur)})}}).directive("languageClass",function(){return{restrict:"A",controller:["$rootScope","$scope","$element",function(e,t,a){var n=function(){return e.language},l=function(e,t){return t&&a.removeClass(t),a.addClass(e)};return l(e.language),t.$watch(n,function(e,t){return e!==t?l(e,t):void 0})}]}}).directive("customOnChange",function(){return{require:"ngModel",link:function(e,t,a,n){t.bind("change",function(){e.$apply(function(){t.length>0&&(n.$setViewValue(t[0].files[0].name),n.$render())})})}}}).directive("autoGrow",function(){return function(e,t,a){var n=function(){t.css("height","auto");var e=t[0].offsetHeight,a=t[0].scrollHeight;a>e&&t.css("height",a+"px")};e.$watch(a.ngModel,function(){n()}),t.bind("focus",function(){n()}),a.$set("ngTrim","false")}}).directive("resetField",["$compile","$timeout",function(e,t){return{require:"ngModel",scope:{},link:function(a,n,l,r){var o=/text|search|tel|url|email|password/i;if("INPUT"!==n[0].nodeName)throw new Error("resetField is limited to input elements");if(!o.test(l.type))throw new Error("Invalid input type for resetField: "+l.type);var i=e('<i ng-show="enabled" ng-mousedown="reset()" class="fa fa-times-circle"></i>')(a);n.after(i),a.reset=function(){r.$setViewValue(null),r.$render(),t(function(){n[0].focus()},0,!1)},n.bind("input",function(){a.enabled=!r.$isEmpty(n.val())}).bind("focus",function(){a.enabled=!r.$isEmpty(n.val()),a.$apply()}).bind("blur",function(){a.enabled=!1,a.$apply()})}}}]).directive("compareTo",function(){return{require:"ngModel",scope:{otherModelValue:"=compareTo"},link:function(e,t,a,n){n.$validators.compareTo=function(t){return t==e.otherModelValue},e.$watch("otherModelValue",function(){n.$validate()})}}}).directive("ngThumb",["$window",function(e){var t={support:!(!e.FileReader||!e.CanvasRenderingContext2D),isFile:function(t){return angular.isObject(t)&&t instanceof e.File},isImage:function(e){var t="|"+e.type.slice(e.type.lastIndexOf("/")+1)+"|";return"|jpg|png|jpeg|bmp|gif|".indexOf(t)>-1}};return{restrict:"A",template:"<canvas/>",link:function(e,a,n){function l(e){var t=new Image;t.onload=r,t.src=e.target.result}function r(){var e=o.width||this.width/this.height*o.height,t=o.height||this.height/this.width*o.width;i.attr({width:e,height:t}),i[0].getContext("2d").drawImage(this,0,0,e,t)}if(t.support){var o=e.$eval(n.ngThumb);if(t.isFile(o.file)&&t.isImage(o.file)){var i=a.find("canvas"),u=new FileReader;u.onload=l,u.readAsDataURL(o.file)}}}}}]).directive("numeric",function(){return{require:"ngModel",scope:{min:"=minValue",max:"=maxValue",ngRequired:"=ngRequired"},link:function(e,t,a,n){n.$parsers.push(function(t){if(void 0===t||t.indexOf(" ").length>-1)return"";var a=t.replace(/[^0-9]/g,"");return a!=t&&(n.$setViewValue(a),n.$render()),n.$validators.min=function(t){return!e.ngRequired&&isNaN(t)?!0:"undefined"!=typeof e.min?t>=parseInt(e.min):!0},e.$watch("min",function(){n.$validate()}),n.$validators.max=function(t){return!e.ngRequired&&isNaN(t)?!0:"undefined"!=typeof e.max?t<=parseInt(e.max):!0},e.$watch("max",function(){n.$validate()}),a})}}}).directive("restrict",["$parse",function(e){return{restrict:"A",require:"ngModel",link:function(t,a,n){t.$watch(n.ngModel,function(a){a&&e(n.ngModel).assign(t,a.replace(new RegExp(n.restrict,"g"),""))})}}}]).directive("placeholder",["$timeout",function(e){var t=document.createElement("input");return"placeholder"in t?{}:{link:function(t,a,n){e(function(){a.val(n.placeholder),a.bind("focus",function(){a.val()==n.placeholder&&a.val("")}).bind("blur",function(){""==a.val()&&a.val(n.placeholder)})})}}}]).directive("ngEnter",function(){return function(e,t,a){t.bind("keydown keypress",function(t){13===t.which&&(e.$apply(function(){e.$eval(a.ngEnter)}),t.preventDefault())})}}).directive("subTable",["$rootScope","ngTableParams","blockUI","$filter","$cache","helper","exportFile","operations","ModuleService","components","mdToast",function(e,t,a,n,l,r,o,i,u,s,d){return{restrict:"EA",scope:{relatedModule:"=",parentModule:"=",reload:"=",showFilter:"=",isSelectable:"@",disableSelectAll:"@",disableLinks:"@"},templateUrl:"view/common/subtable.html?v="+version,controller:["$scope",function(t){t.loading=!0,t.relatedModule.loading=!0,t.module=n("filter")(e.modules,{name:t.relatedModule.related_module},!0)[0],t.type=t.relatedModule.related_module,t.readonly=t.relatedModule.readonly||!1,t.parentType=t.relatedModule.relation_field,t.parentId=t.$parent.id,t.language=e.language,t.operations=i,t.hasPermission=r.hasPermission,t.lookupUser=r.lookupUser,t.relatedModuleInModal=t.$parent.selectedRelatedModule&&t.$parent.selectedRelatedModule.relatedModuleInModal?!0:!1,t.previousParentType=t.$parent.previousParentType,t.previousParentId=t.$parent.previousParentId,t.previousReturnTab=t.$parent.previousReturnTab,t.isAdmin=e.user.profile.has_admin_rights,t.hideDeleteAll=n("filter")(e.deleteAllHiddenModules,t.parentModule+"|"+t.type,!0)[0];var c=[10,25,50,100],p=t.relatedModule.display_fields,f=t.parentType+(t.isSelectable?"":t.parentId);t.cacheKey=f+"_"+t.module.name,t.$parent["selectedRows"+t.type]=[];var m="many_to_many"===t.relatedModule.relation_type,g=[];if((void 0===t.isSelectable||null===t.isSelectable)&&(t.isSelectable=!1),!t.isSelectable){var v=t.parentType;if(m&&(v=t.parentModule!=t.module.name?t.parentModule+"_id":t.parentModule+"1_id"),g.push({field:v,operator:"equals",value:t.parentId,no:1}),"related_to"===t.parentType){var h=n("filter")(e.modules,{name:t.parentModule},!0)[0];t.parentType=t.parentModule,g.push({field:"related_module",operator:"is",value:h["label_"+e.user.tenant_language+"_singular"],no:1})}}t.isSelectable=!0,m&&(t.cacheKey=f+"_"+t.relatedModule.relation_field+"_"+t.relatedModule.related_module);var y=a.instances.get("tableBlockUISubTable"+t.cacheKey);u.setTable(t,y,c,10,angular.copy(g),f,t.type,t.isSelectable,t.disableLinks?null:t.parentId,t.disableLinks?null:t.parentType,p,t.relatedModule,t.parentModule,t.relatedModule.id,t.previousParentType,t.previousParentId,t.previousReturnTab,t.$parent),t.tableParams&&(t.tableParams.disableSelectAll=t.disableSelectAll),t.isManyToManyModal=!0,t.refresh=function(e){l.remove(t.cacheKey),e&&(t.tableParams.filterList=g,t.tableParams.refreshing=!0),t.tableParams.reloading=!0,t.tableParams.reload()},t.$watch("reload",function(e){e&&t.refresh(!1)}),t.$watch("showFilter",function(e){e&&(t.tableParams.showFilter=!t.tableParams.showFilter)}),t["delete"]=function(e){u.getRecord(t.module.name,e).then(function(a){var n=u.processRecordSingle(a.data,t.$parent.$parent.module,t.$parent.$parent.picklistsModule);t.executeCode=!1,s.run("BeforeDelete","Script",t,n),t.executeCode||u.deleteRecord(t.module.name,e).then(function(){setTimeout(function(){u.getRecord(t.parentModule,t.parentId).then(function(e){var a=u.processRecordSingle(e.data,t.$parent.$parent.module,t.$parent.$parent.picklistsModule);u.formatRecordFieldValues(a,t.$parent.$parent.module,t.$parent.$parent.picklistsModule),s.run("AfterDelete","Script",t,a),t.$parent.$parent.$parent.record=a;var n=t.parentModule+"_"+t.parentModule;l.remove(n),t.tableParams.reload()})},1e3)})})},t.deleteRelation=function(e){var a={};a[t.parentModule+"_id"]=parseInt(t.parentId),a[t.relatedModule.related_module+"_id"]=e,u.deleteRelation(t.parentModule,t.relatedModule.related_module,a).then(function(){l.remove(t.cacheKey),t.tableParams.reload()})},t.multiselect=function(e,a){var n=[];return angular.forEach(t.tableParams.picklists[a.picklist_id],function(t){t.inactive||t.labelStr.toLowerCase().indexOf(e)>-1&&n.push(t)}),n},t.selectAllModal=function(e,a){t.$parent["selectedRows"+t.type]=[],t.isAllSelectedModal?t.isAllSelectedModal=!1:(t.isAllSelectedModal=!0,angular.forEach(a,function(e){e.fields.forEach(function(a){1==a.primary&&t.$parent["selectedRows"+t.type].push({id:e.id,displayName:a.valueFormatted})})}))},t.selectRow=function(e,a){e.target.checked?a.fields.forEach(function(e){1!=e.primary||e.isJoin||t.$parent["selectedRows"+t.type].push({id:a.id,displayName:e.valueFormatted})}):t.$parent["selectedRows"+t.type]=t.$parent["selectedRows"+t.type].filter(function(e){return e.id!=a.id}),t.isAllSelectedModal=!1},t.isRowSelected=function(e){return t.$parent["selectedRows"+t.type].filter(function(t){return t.id==e}).length>0},t.$parent.$parent.$parent.isManyToMany=m,t.deleteSelectedsSubTable=function(){if(!t.relatedModuleInModal){if(!t.$parent["selectedRows"+t.type]||!t.$parent["selectedRows"+t.type].length)return;var e=[];t.$parent["selectedRows"+t.type].filter(function(t){e.push(t.id)}),u.deleteRecordBulk(t.module.name,e).then(function(){l.remove(t.cacheKey),t.tableParams.reloading=!0,t.tableParams.reload(),t.$parent["selectedRows"+t.type]=[],t.isAllSelectedModal=!1;var a={};a.data=e.length})}},t["export"]=function(){if(!t.relatedModuleInModal){if(t.tableParams.total()<1)return;var a=!1;try{a=!!new Blob}catch(l){}if(!a)return void d.warning({content:n("translate")("Module.ExportUnsupported"),timeout:8e3});if(t.tableParams.total()>3e3)return void d.warning({content:n("translate")("Module.ExportWarning"),timeout:8e3});var r=t.module["label_"+e.language+"_plural"]+"-"+n("date")(new Date,"dd-MM-yyyy")+".xls";t.exporting=!0,u.getCSVData(t,t.type).then(function(e){d.success({content:n("translate")("Module.ExcelExportSuccess"),timeout:5e3}),o.excel(e,r),t.exporting=!1})}}}]}}]).directive("numberCurrency",["$rootScope","$filter","$locale","helper",function(e,t,a,n){return{restrict:"A",require:"ngModel",scope:{minValue:"=",maxValue:"=",currencySymbol:"=",ngRequired:"=",places:"=",rounding:"="},link:function(l,r,o,i){function u(e){return RegExp("\\d|\\-|\\"+e,"g")}function s(e){return RegExp("\\-{0,1}((\\"+e+")|([0-9]{1,}\\"+e+"?))&?[0-9]{0,100}","g")}function d(e){e=String(e);var r=a.NUMBER_FORMATS.DECIMAL_SEP,o=null,i=t("currency")("-1",c(),l.places),d=i.indexOf("1"),p=i.substring(0,d);if(e=e.replace(p,"-"),RegExp("^-[\\s]*$","g").test(e)&&(e="-0"),u(r).test(e)){var g=e.match(u(r)).join("").match(s(r));if(g&&(g=g[0].replace(r,".")),!g)return null;switch(o=parseFloat(g),m){case"off":o=n.roundBy(Math.round,o,f);break;case"down":o=n.roundBy(Math.floor,o,f);break;case"up":o=n.roundBy(Math.ceil,o,f)}}return o}function c(){return l.currencySymbol?("false"===l.currencySymbol&&(l.currencySymbol=""),l.currencySymbol):e.currencySymbol?e.currencySymbol:("tr"===e.language?a.NUMBER_FORMATS.CURRENCY_SYM="₺":"en"===e.language&&(a.NUMBER_FORMATS.CURRENCY_SYM="$"),a.NUMBER_FORMATS.CURRENCY_SYM)}function p(){for(var e=i.$formatters,t=e.length,a=i.$$rawModelValue;t--;)a=e[t](a);i.$setViewValue(a),i.$render()}if("false"!==o.numberCurrency){var f="undefined"!=typeof l.places&&null!=l.places?l.places:2,m="undefined"!=typeof l.rounding&&null!=l.rounding?l.rounding:"none";i.$parsers.push(function(e){var t=d(e);return("."==t||"-."==t)&&(t=".0"),parseFloat(t)}),r.on("blur",function(){i.$commitViewValue(),p()}),i.$formatters.unshift(function(e){return t("currency")(e,c(),l.places)}),i.$validators.min=function(e){return!l.ngRequired&&isNaN(e)?!0:"undefined"!=typeof l.minValue?e>=parseFloat(l.minValue):!0},i.$validators.max=function(e){return!l.ngRequired&&isNaN(e)?!0:"undefined"!=typeof l.maxValue?e<=parseFloat(l.maxValue):!0},l.$watch("maxValue",function(){i.$validate()}),l.$watch("minValue",function(){i.$validate()}),l.$watch("currencySymbol",function(){i.$commitViewValue(),p()}),i.$validators.places=function(e){return e&&isNaN(e)?!1:!0}}}}}]).directive("numberDecimal",["$rootScope","$filter","$locale","helper",function(e,t,a,n){return{restrict:"A",require:"ngModel",scope:{min:"=minValue",max:"=maxValue",ngRequired:"=",places:"=",rounding:"="},link:function(e,l,r,o){function i(e){return RegExp("\\d|\\-|\\"+e,"g")}function u(e){return RegExp("\\-{0,1}((\\"+e+")|([0-9]{1,}\\"+e+"?))&?[0-9]{0,100}","g")}function s(l){l=String(l);var r=a.NUMBER_FORMATS.DECIMAL_SEP,o=null,s=t("number")("-1",e.places),d=s.indexOf("1"),f=s.substring(0,d);if(l=l.replace(f,"-"),RegExp("^-[\\s]*$","g").test(l)&&(l="-0"),i(r).test(l)){var m=l.match(i(r)).join("").match(u(r));if(m&&(m=m[0].replace(r,".")),!m)return null;switch(o=parseFloat(m),p){case"off":o=n.roundBy(Math.round,o,c);break;case"down":o=n.roundBy(Math.floor,o,c);break;case"up":o=n.roundBy(Math.ceil,o,c)}}return o}function d(){for(var e=o.$formatters,t=e.length,a=o.$$rawModelValue;t--;)a=e[t](a);o.$setViewValue(a),o.$render()}if("false"!==r.numberDecimal){var c="undefined"!=typeof e.places&&null!=e.places?e.places:2,p="undefined"!=typeof e.rounding&&null!=e.rounding?e.rounding:"none";o.$parsers.push(function(e){var t=s(e);return("."==t||"-."==t)&&(t=".0"),parseFloat(t)}),l.on("blur",function(){o.$commitViewValue(),d()}),o.$formatters.unshift(function(a){return t("number")(a,e.places)}),o.$validators.min=function(t){return!e.ngRequired&&isNaN(t)?!0:"undefined"!=typeof e.min?t>=parseFloat(e.min):!0},e.$watch("min",function(){o.$validate()}),o.$validators.max=function(t){return!e.ngRequired&&isNaN(t)?!0:"undefined"!=typeof e.max?t<=parseFloat(e.max):!0},e.$watch("max",function(){o.$validate()}),o.$validators.places=function(e){return e&&isNaN(e)?!1:!0}}}}}]).directive("tooltip",function(){return{restrict:"A",link:function(e,t){$(t).hover(function(){$(t).tooltip("show")},function(){$(t).tooltip("hide")})}}}).directive("webHook",["$http","$filter","mdToast",function(e,t,a){return{restrict:"A",link:function(n,l,r){l.bind("click",function(){var l=angular.fromJson(r.webHook);if(l.template&&l.url){n.loading=!0;var o=l.template.split(","),i={},u=n.$parent.$parent.record;angular.forEach(o,function(e){var t=u[e];t&&(i[e]=t.length>0?t:t.labelStr)}),e.post(l.url,i).then(function(){a.success(t("translate")("Common.ProcessTriggerSuccess"))}).error(function(){a.error(t("translate")("Common.Error")),n.loading=!1})}})}}}]).directive("customModalFrame",[function(){return{restrict:"A",link:function(e,t,a){t.bind("click",function(){angular.fromJson(a.customModalFrame)})}}}]).directive("uiTinymceMulti",["$rootScope","uiTinymceConfig",function(e,t){t=t||{};var a=0;return{require:"ngModel",link:function(n,l,r,o){var i,u,s;r.id||r.$set("id","uiTinymce"+a++),u={setup:function(e){e.on("init",function(){o.$render()}),e.on("ExecCommand",function(){e.save(),o.$setViewValue(l.val()),n.$$phase||n.$apply()}),e.on("KeyUp",function(){e.save(),o.$setViewValue(l.val()),n.$$phase||n.$apply()})},style_formats:[{title:"tr"===e.language?"Yazı Boyutu":"Font Size",items:[{title:"tr"===e.language?"Çok Büyük":"Very Big",block:"h2",styles:{fontWeight:"normal"}},{title:"tr"===e.language?"Büyük":"Big",block:"h3",styles:{fontWeight:"normal"}},{title:("tr"===e.language,"Normal"),block:"h4",styles:{fontWeight:"normal"}},{title:"tr"===e.language?"Küçük":"Small",block:"h5",styles:{fontWeight:"normal"}},{title:"tr"===e.language?"Çok Küçük":"Very Small",block:"h6",styles:{fontWeight:"normal"}}]}],mode:"exact",elements:r.id,language:e.language,menubar:!1,statusbar:!1,plugins:"fullscreen paste",paste_as_text:!0,toolbar:"bold italic bullist numlist | styleselect | fullscreen",skin:"lightgray",theme:"modern",height:"200"},i=r.uiTinymce?n.$eval(r.uiTinymce):{},angular.extend(u,t,i),setTimeout(function(){tinymce.init(u)}),o.$render=function(){s||(s=tinymce.get(r.id)),s&&s.setContent(o.$viewValue||"")}}}}]).directive("location",["$rootScope","config","$filter","$timeout",function(e,t,a,n){return{restrict:"E",require:"^?ngModel",link:function(e,t,a,l){function r(e){s.latitude=e.coords.latitude,s.longitude=e.coords.longitude,s.zoom=10,i()}function o(){i()}function i(){var a=t[0];if(e.addres&&!e.location){var l=new google.maps.Geocoder;l.geocode({address:e.addres},function(e,t){n(function(){"OK"===t?(s.latitude=e[0].geometry.location.lat(),s.longitude=e[0].geometry.location.lng(),u(a)):u(a)})})}else u(a)}function u(t){if(e.location){var a=e.location.split(",");s.latitude=a[0],s.longitude=a[1],s.zoom=17}var n=new google.maps.LatLng(s.latitude,s.longitude),r={center:n,zoom:s.zoom},o=new google.maps.Map(t,r),i=new google.maps.Marker({draggable:!0,animation:google.maps.Animation.DROP,position:n});l.$setViewValue(s.latitude+","+s.longitude);var u=new google.maps.InfoWindow;o.addListener("mouseup",function(e){var t=e.latLng;l.$setViewValue(t.lat()+","+t.lng()),u.setContent(t.lat()+","+t.lng()),u.open(o,i)}),o.addListener("click",function(t){var a=t.latLng,a=t.latLng;e.ngModel=a.lat()+","+a.lng(),u.setContent(a.lat()+","+a.lng()),i.setPosition(a),u.open(o,i),l.$setViewValue(a.lat()+","+a.lng())}),i.setMap(o)}var s={latitude:39.93948807471046,longitude:32.85907745361328,zoom:5};navigator.geolocation.getCurrentPosition(r,o)}}}]).directive("helpPage",["$rootScope","$http","config","$filter","HelpService","$sce","$cache","$localStorage",function(e,t,a,n,l,r,o,i){return{restrict:"EA",scope:{moduleId:"=",route:"="},controller:["$scope",function(t){if(e.isMobile())return!1;if(e.firtScreenShow&&!t.moduleId)return e.helpPageFirstScreen?(e.helpTemplatesModal=e.helpPageFirstScreen,angular.isObject(e.helpTemplatesModal.template)||(e.helpTemplatesModal.template=r.trustAsHtml(e.helpTemplatesModal.template)),e.show=!0,void(e.firtScreenShow=!1)):(e.helpTemplatesModal=void 0,void(e.show=!1));e.selectedClose=!0,e.show=!1,t.selectedCloseModalForRoute=2,t.selectedCloseModalForModule=!0,e.helpTemplatesModal=void 0;var a=void 0,u=void 0;if(i.read("startPage")&&(t.startPage=JSON.parse(i.read("startPage")),i.read("routeShow")&&(t.selectedCloseStartPage=JSON.parse(i.read("routeShow")),a=n("filter")(t.selectedCloseStartPage,{name:t.route})[0]),u=n("filter")(t.startPage,{name:t.route})[0],a?t.selectedCloseModalForRoute=a.value:u&&(t.selectedCloseModalForRoute=u.value)),i.read("moduleShow")){t.selectedCloseModal=JSON.parse(i.read("moduleShow"));var s=n("filter")(t.selectedCloseModal,{name:t.moduleId})[0];s&&(t.selectedCloseModalForModule=s.value,e.show=t.selectedCloseModalForModule)}i.read("routeShow")&&(t.selectedCloseRoute=JSON.parse(i.read("routeShow")),a=n("filter")(t.selectedCloseRoute,{name:t.route})[0],a?(t.selectedCloseModalForRoute=a.value,e.show=!1):t.selectedCloseModalForRoute=2),t.openModal=function(){if(e.helpTemplatesModal&&"publish"===e.helpTemplatesModal.show_type&&(angular.isObject(e.helpTemplatesModal.template)||(e.helpTemplatesModal.template=r.trustAsHtml(e.helpTemplatesModal.template)),i.read("ModalShow")&&(e.selectedClose=JSON.parse(i.read("ModalShow")),e.show=!1),e.selectedClose===!0)){if(t.moduleId&&t.selectedCloseModalForModule&&(e.show=!0),t.route&&1===t.selectedCloseModalForRoute){if(i.read("startPage")){t.startPage=JSON.parse(i.read("startPage"));var a=n("filter")(t.startPage,{name:t.route})[0];if(a&&1===a.value){var l=[],o={name:e.currentPath,value:2};l.push(o),i.write("startPage",JSON.stringify(l))}}e.show=!0}t.route&&2===t.selectedCloseModalForRoute&&(e.show=!0)}};var d="help-";if(t.moduleId&&(d+="-"+t.moduleId),t.route&&(t.route.replace("/","--"),d+=t.route),o.get(d)&&(e.helpTemplatesModal=o.get(d),e.selectedClose&&t.openModal()),!e.helpTemplatesModal&&!e.dashboardHelpTemplate){if(t.moduleId){var c=n("filter")(e.modules,{id:t.moduleId},!0)[0],p=n("filter")(c.helps,{modal_type:"modal",module_type:"module_list"},!0)[0];if(!p)return;e.helpTemplatesModal=p,o.put(d,p)}else l.getByType("modal",null,t.route).then(function(a){a.data&&(a.data.template=r.trustAsHtml(a.data.template),e.helpTemplatesModal=a.data,o.put(d,a.data),e.selectedClose&&t.openModal())});e.selectedClose&&t.openModal()}t.showModal=function(){(t.moduleId||t.route)&&i.write("ModalShow",!1)}}]}}]).directive("queryBuilder",["$compile","$rootScope","$filter",function(e,t,a){return{restrict:"E",scope:{group:"=",fieldskey:"=",module:"=",viewfilter:"=",allfields:"="},templateUrl:"view/app/module/filters.html",compile:function(n){var l,r;return l=n.contents().remove(),function(n,o){n.language=t.language,n.globalization=t.globalization;var i={transport:{read:function(e){e.success(n.allfields)}},group:{field:"lookuplabel"}};n.filterFieldsOption={dataSource:new kendo.data.DataSource(i),dataTextField:"label",dataValueField:"name",optionLabel:a("translate")("Common.Select"),filter:"contains",autoWidth:!0,height:300,autoBind:!1,filterable:{multi:!0,search:!0,ignoreCase:!0}},n.filterOperatorsOption={dataTextField:"label",dataValueField:"name",optionLabel:a("translate")("Module.SelectOperator"),filter:"contains",autoWidth:!0,autoBind:!1,height:300},n.addCondition=function(){n.group.filters.unshift({key:"key".generateRandomKey(20),operator:"",field:"",value:""})},n.changeOpertor=function(e){"not_empty"!==e.operator.name&&"empty"!==e.operator.name||null==e.field?e.value="":(e.value="-",n.viewfilter())},n.viewFiltere=function(e){e?null!=e.value&&""!==e.operator.name&&n.viewfilter():n.viewfilter()},n.fieldChange=function(e){e.value=null,e.operator=null,e.disabled=!1,e.costumeDate=null},n.dateFormat=[{label:a("translate")("View.Second"),value:"s"},{label:a("translate")("View.Minute"),value:"m"},{label:a("translate")("View.Hour"),value:"h"},{label:a("translate")("View.Day"),value:"D"},{label:a("translate")("View.Week"),value:"W"},{label:a("translate")("View.Month"),value:"M"},{label:a("translate")("View.Year"),value:"Y"}],n.numberOptionsFilter={format:"{0:n0}",decimals:0},n.costumeDateFilter=[{name:"thisNow",label:a("translate")("View.Now"),value:"now()"},{name:"thisToday",label:a("translate")("View.StartOfTheDay"),value:"today()"},{name:"thisWeek",label:a("translate")("View.StartOfThisWeek"),value:"this_week()"},{name:"thisMonth",label:a("translate")("View.StartOfThisMonth"),value:"this_month()"},{name:"thisYear",label:a("translate")("View.StartOfThisYear"),value:"this_year()"},{name:"year",label:a("translate")("View.NowYear"),value:"year()"},{name:"month",label:a("translate")("View.NowMonth"),value:"month()"},{name:"day",label:a("translate")("View.NowDay"),value:"day()"},{name:"costume",label:a("translate")("View.CustomDate"),value:"costume"},{name:"todayNextPrev",label:a("translate")("View.FromTheBeginningOfTheDay"),value:"costumeN",nextprevdatetype:"D"},{name:"weekNextPrev",label:a("translate")("View.FromTheBeginningOfTheWeek"),value:"costumeW",nextprevdatetype:"M"},{name:"monthNextPrev",label:a("translate")("View.FromTheBeginningOfTheMonth"),value:"costumeM",nextprevdatetype:"M"},{name:"yearNextPrev",label:a("translate")("View.FromTheBeginningOfTheYear"),value:"costumeY",nextprevdatetype:"Y"}],n.removeCondition=function(e){n.group.filters.splice(e,1),n.viewfilter()},n.addGroup=function(){n.group.filters.push({group:{level:n.group.level+1,logic:"and",filters:[]}})},n.removeGroup=function(){"group"in n.$parent&&n.$parent.group.filters.splice(n.$parent.$index,1),n.viewfilter()},r||(r=e(l)),o.append(r(n,function(e){return e}))}}}}]).directive("inputStars",[function(){function e(e){return Number(e)===e&&e%1!==0}function t(t,a,n,l){function r(e,t){return e.pageX<t.getBoundingClientRect().left+t.offsetWidth/2}var o={get allowHalf(){return"string"==typeof n.allowHalf&&"false"!=n.allowHalf},get readonly(){return"false"!=n.readonly&&(n.readonly||""===n.readonly)},get fullIcon(){return n.iconFull||"fa-star"},get halfIcon(){return n.iconHalf||"fa-star-half-o"},get emptyIcon(){return n.iconEmpty||"fa-star-o"},get iconBase(){return n.iconBase||"fa fa-fw"},get iconHover(){return n.iconHover||"angular-input-stars-hover"}};t.items=new Array(+n.max),t.listClass=n.listClass||"angular-input-stars",l.$render=function(){t.lastValue=e(l.$viewValue)?Math.round(2*parseFloat(l.$viewValue))/2:parseFloat(l.$viewValue)||0},t.getClass=function(e){var a;if(e>=t.lastValue)a=o.iconBase+" "+o.emptyIcon;else{var n=e+.5;a=o.allowHalf&&n===t.lastValue?o.iconBase+" "+o.halfIcon+" active ":o.iconBase+" "+o.fullIcon+" active "}return o.readonly?a+" readonly":a},t.unpaintStars=function(e,a){t.paintStars(t.lastValue-1,a)},t.paintStars=function(e,t){if(!o.readonly){for(var n=a.find("li").find("i"),l=0;l<n.length;l++){var r,i,u=angular.element(n[l]);e>=l?(r=[o.emptyIcon,o.halfIcon],i=[o.iconHover,o.fullIcon,"active"]):(r=[o.fullIcon,o.iconHover,o.halfIcon,"active"],i=o.allowHalf&&e+.5===l?[o.halfIcon,"active"]:[o.emptyIcon]),u.removeClass(r.join(" ")),u.addClass(i.join(" "))}t||n.removeClass(o.iconHover)}},t.setValue=function(e,a){if(!o.readonly){var i,u=a.target;if(i=o.allowHalf&&r(a,u)?e+.5:e+1,i===t.lastValue&&(i=0),t.lastValue=i,l.$setViewValue(i),l.$render(),n.onStarClick)try{t.$parent.$eval(n.onStarClick,{$event:a})}catch(a){}}}}var a={restrict:"EA",replace:!0,template:'<ul ng-class="listClass"><li ng-touch="paintStars($index)" ng-mouseenter="paintStars($index, true, $event)" ng-mouseleave="unpaintStars($index, false)" ng-repeat="item in items track by $index"><i  ng-class="getClass($index)" ng-click="setValue($index, $event)"></i></li></ul>',require:"ngModel",scope:{bindModel:"=ngModel"},link:t};return a}]);