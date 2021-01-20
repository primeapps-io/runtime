"use strict";angular.module("primeapps").factory("$localStorage",["$window",function(e){return{set:function(t,o){e.localStorage[t]=angular.toJson(o)},get:function(e){var t=this.read(e);return t?angular.fromJson(t):null},write:function(t,o){e.localStorage[t]=o},read:function(t){return e.localStorage[t]},remove:function(t){e.localStorage.removeItem(t)}}}]).factory("$sessionStorage",["$window",function(e){return{set:function(t,o){e.sessionStorage[t]=angular.toJson(o)},get:function(e){var t=this.read(e);return t?angular.fromJson(t):null},write:function(t,o){e.sessionStorage[t]=o},read:function(t){return e.sessionStorage[t]},remove:function(t){e.sessionStorage.removeItem(t)},clear:function(){e.sessionStorage.clear()}}}]).factory("$cache",["$cacheFactory",function(e){return e("primeapps")}]).factory("mdToast",["$mdToast",function(e){function t(e){n.push(e),1===n.length&&o()}function o(){if(n.length){var t=n[0];e.show(t).then(function(){n.shift(),o()})}}function r(t){return i.template?{template:i.template,textContent:i.content,action:i.actionTxt,position:i.position,actionKey:i.actionKey,theme:"toast-"+t,hideDelay:i.actionTxt?0:i.timeout,scope:i.scope}:e.simple().textContent(i.content).action(i.actionTxt).position(i.position).actionKey(i.actionKey).theme("toast-"+t).hideDelay(i.actionTxt?0:i.timeout)}var n=[],i={content:null,actionTxt:null,position:"bottom right",actionKey:"z",timeout:4e3},a=function(e){i={content:null,actionTxt:null,position:"bottom right",actionKey:"z",timeout:4e3},"string"==typeof e?i.content=e:angular.extend(i,e)};return{success:function(e){a(e),t(r("success"))},warning:function(e){a(e),t(r("warning"))},error:function(e){a(e),t(r("error"))},info:function(e){a(e),t(r("info"))}}}]).factory("helper",["$rootScope","$timeout","$filter","$localStorage","$sessionStorage","$q","$http","config","$cache",function(e,t,o,r,n,i,a,s,l){return{hasCustomProfilePermission:function(t){if(!(e.customProfilePermissions&&e.customProfilePermissions.length>0))return!1;for(var o=0;o<e.customProfilePermissions.length;o++){var r=e.customProfilePermissions[o];if(r.profileId==e.user.profile.id){for(var n=!1,i=0;i<r.permissions.length;i++){var a=r.permissions[i];a==t&&(n=!0)}return n?!0:!1}}return!1},SnakeToCamel:function(e,t){function o(e,t,r){if(0===r||!angular.isObject(e))return e;for(var n={},i=Object.keys(e),a=0;a<i.length;a++)n[t(i[a])]=o(e[i[a]],t,r-1);return n}function r(e){var t="_",o=/(?=[A-Z])/;return e.split(o).join(t).toLowerCase()}return angular.isObject(e)?("undefined"==typeof t&&(t=1),o(e,r,t)):r(e)},getTime:function(e){if(!e)return"";var t=new Date(e);return t.getTime()},dateDiff:function(e){var t=new Date;t.setHours(0,0,0,0);var o=Math.floor((e>t?e-t:t-e)/864e5);return o},hideLoader:function(){-1===document.body.className.indexOf("loaded")&&(document.body.className+=" loaded"),t(function(){var e=document.getElementById("loader");e&&e.parentNode.removeChild(e)},300)},getFileExtension:function(e){var t=e.split(".");return t.length<2?"":t=t[t.length-1].toLowerCase()||""},arrayObjectIndexOf:function(e,t){for(var o=0;o<e.length;o++)if(angular.equals(e[o],t))return o;return-1},hasPermission:function(t,r){var n=o("filter")(e.modules,{name:t},!0)[0];if(!n)return!1;var i=null!=profileConfigs&&null!=profileConfigs.config?profileConfigs.config[n.name]:null;return i?"modify"===r?i.actions.update.enable:"remove"===r?i.actions["delete"].enable:"write"===r?i.actions.create.enable:"read"===r?void 0===i.actions.read&&e.user.profile.has_admin_rights?!0:void 0===i.actions.read?!1:i.actions.read.enable:!1:!1},hasDocumentsPermission:function(t){var r=o("filter")(e.user.profile.permissions,{type:1})[0];return r?r[t]:!1},hasAdminRights:function(){return e.user.profile.has_admin_rights},getCulture:function(){var e=r.read("NG_TRANSLATE_LANG_KEY")||"tr";switch(e){case"tr":return"tr-TR";case"en":return"en-US"}},getCurrency:function(){var e=r.read("NG_TRANSLATE_LANG_KEY")||"tr";switch(e){case"tr":return"TRY";case"en":return"USD"}},getCurrentDateMin:function(){var e=new Date;return e.setHours(0),e.setMinutes(0),e.setSeconds(0),e.setMilliseconds(0),e},getCurrentDateMax:function(){var e=new Date;return e.setHours(23),e.setMinutes(59),e.setSeconds(59),e.setMilliseconds(0),e},floorMinutes:function(e){var t=3e5;return new Date(Math.floor(e.getTime()/t)*t)},lookupProfile:function(e,t){var o=i.defer();if(!e&&!t)return o.resolve([]),o.promise;var r={fields:["id","name"],filters:[{field:"name",operator:"starts_with",value:e,no:1},{field:"deleted",operator:"equals",value:!1,no:2}],limit:20,sort_field:"name",sort_direction:"asc"};return e||(r.filters.shift(),r.filters[0].no=1),a.post(s.apiUrl+"record/find/profiles",r).then(function(e){if(e=e.data,!e)return o.resolve([]),o.promise;for(var t=[],r=0;r<e.length;r++){var n=e[r],i={};i.id=n.id,i.name=n.name,t.push(i)}o.resolve(t)})["catch"](function(e){o.reject(e.data)}),o.promise},lookupRole:function(t,o){var r=i.defer();if(!t&&!o)return r.resolve([]),r.promise;var n={fields:["id","label_en","label_tr"],filters:[{field:"label_"+e.user.tenant_language,operator:"starts_with",value:t,no:1},{field:"deleted",operator:"equals",value:!1,no:2}],limit:20,sort_field:"label_"+e.user.tenant_language,sort_direction:"asc"};return t||(n.filters.shift(),n.filters[0].no=1),a.post(s.apiUrl+"record/find/roles",n).then(function(t){if(t=t.data,!t)return r.resolve([]),r.promise;for(var o=[],n=0;n<t.length;n++){var i=t[n],a={};a.id=i.id,a.name=i["label_"+e.user.tenant_language],o.push(a)}r.resolve(o)})["catch"](function(e){r.reject(e.data)}),r.promise},lookupUser:function(e,t,r){var n=i.defer();if(!e&&!t)return n.resolve([]),n.promise;var l={fields:["id","full_name","email","is_active"],filters:[{field:"full_name",operator:"starts_with",value:e,no:1},{field:"is_active",operator:"equals",value:!0,no:2}],limit:20,sort_field:"full_name",sort_direction:"asc"};if(r&&1==r)for(var c=0;c<l.filters.length;c++){var u=l.filters[c];if("is_active"==u.field){var f=l.filters.indexOf(u);l.filters.splice(f,1)}}return e||(l.filters.shift(),l.filters[0].no=1),a.post(s.apiUrl+"record/find/users",l).then(function(r){if(r=r.data,!r)return n.resolve([]),n.promise;var i=[];if(t&&!e){var a={};switch(a.id=0,t){case"record_owner":a.email="[owner]",a.full_name=o("translate")("Common.RecordOwner");break;case"logged_in_user":a.email="[me]",a.full_name=o("translate")("Common.LoggedInUser")}i.push(a)}for(var s=0;s<r.length;s++){var l=r[s],c={};c.id=l.id,c.email=l.email,c.full_name=l.full_name,i.push(c)}n.resolve(i)})["catch"](function(e){n.reject(e.data)}),n.promise},lookupUserAndGroup:function(e,t,o){var r=i.defer();if(!o)return r.resolve([]),r.promise;var n={module_id:e,is_readonly:t,search_term:o};return a.post(s.apiUrl+"record/lookup_user",n).then(function(e){return e.data?void r.resolve(e.data):(r.resolve([]),r.promise)})["catch"](function(e){r.reject(e.data)}),r.promise},getPicklists:function(t,r){for(var n=i.defer(),c={},u=[],f=this,m=0;m<t.length;m++){var p=t[m],d=l.get("picklist_"+p);if(0!==p)!d||r?u.push(p):c[p]=d;else{if(d&&!r){c[p]=d;break}for(var g=[],h=0;h<e.modules.length;h++){var v=e.modules[h];if(0!=v.order&&"users"!==v.name){var y={};y.id=parseInt(v.id)+9e5,y.type=9e5,y.order=v.order,y.value=v.name,g.push(y)}}g=o("orderBy")(g,"order"),c[9e5]=g,l.put("picklist_"+9e5,g)}}return u.length<=0?(n.resolve(c),n.promise):(a.post(s.apiUrl+"picklist/find",u).then(function(r){if(!r.data)return n.resolve(c),n.promise;for(var i=0;i<t.length;i++){var a=t[i];if(!(u.indexOf(a)<0)){var s=f.mergePicklists(r.data);s=o("orderBy")(s,"label_"+e.language),c[a]=s,l.put("picklist_"+a,c[a])}}n.resolve(c)})["catch"](function(e){n.reject(e.data)}),n.promise)},mergePicklists:function(t){var r=[];const n=o("filter")(e.globalizations,function(e){return e.Culture.contains(tenantLanguage+"-")},!0)[0];if(t)for(var i=0;i<t.length;i++)for(var a=t[i],s=0;s<a.items.length;s++){const l=a.items[s];l.languages&&!angular.isObject(l.languages)&&(l.languages=JSON.parse(l.languages));const c=e.getLanguageValue(l.languages,"label");var u={};u.type=a.id,u.id=l.id,u.label=c,u.value=l.value,u.value2=l.value2,u.value3=l.value3,u.system_code=l.system_code,u.order=l.order,u.inactive=l.inactive,l.languages&&l.languages[n.Label]&&(u.labelStr=l.languages[n.Label].label),u.languages=l.languages,r.push(u)}return r},getSlug:function(e,t){if(!e)return"";t||(t="_");for(var o,r,n={" ":" ","¡":"!","¢":"c","£":"lb","¥":"yen","¦":"|","§":"SS","¨":'"',"©":"(c)","ª":"a","«":"<<","¬":"not","­":"-","®":"(R)","°":"^0","±":"+/-","²":"^2","³":"^3","´":"'","µ":"u","¶":"P","·":".","¸":",","¹":"^1","º":"o","»":">>","¼":" 1/4 ","½":" 1/2 ","¾":" 3/4 ","¿":"?","À":"`A","Á":"'A","Â":"^A","Ã":"~A","Ä":'"A',"Å":"A","Æ":"AE","Ç":"C","È":"`E","É":"'E","Ê":"^E","Ë":'"E',"Ì":"`I","Í":"'I","Î":"^I","Ï":'"I',"Ð":"D","Ñ":"~N","Ò":"`O","Ó":"'O","Ô":"^O","Õ":"~O","Ö":'"O',"×":"x","Ø":"O","Ù":"`U","Ú":"'U","Û":"^U","Ü":'"U',"Ý":"'Y","Þ":"Th","ß":"ss","à":"`a","á":"'a","â":"^a","ã":"~a","ä":'"a',"å":"a","æ":"ae","ç":"c","è":"`e","é":"'e","ê":"^e","ë":'"e',"ì":"`i","í":"'i","î":"^i","ï":'"i',"ð":"d","ñ":"~n","ò":"`o","ó":"'o","ô":"^o","õ":"~o","ö":'"o',"÷":":","ø":"o","ù":"`u","ú":"'u","û":"^u","ü":'"u',"ý":"'y","þ":"th","ÿ":'"y',"Ā":"A","ā":"a","Ă":"A","ă":"a","Ą":"A","ą":"a","Ć":"'C","ć":"'c","Ĉ":"^C","ĉ":"^c","Ċ":"C","ċ":"c","Č":"C","č":"c","Ď":"D","ď":"d","Đ":"D","đ":"d","Ē":"E","ē":"e","Ĕ":"E","ĕ":"e","Ė":"E","ė":"e","Ę":"E","ę":"e","Ě":"E","ě":"e","Ĝ":"^G","ĝ":"^g","Ğ":"G","ğ":"g","Ġ":"G","ġ":"g","Ģ":"G","ģ":"g","Ĥ":"^H","ĥ":"^h","Ħ":"H","ħ":"h","Ĩ":"~I","ĩ":"~i","Ī":"I","ī":"i","Ĭ":"I","ĭ":"i","Į":"I","į":"i","İ":"I","ı":"i","Ĳ":"IJ","ĳ":"ij","Ĵ":"^J","ĵ":"^j","Ķ":"K","ķ":"k","Ĺ":"L","ĺ":"l","Ļ":"L","ļ":"l","Ľ":"L","ľ":"l","Ŀ":"L","ŀ":"l","Ł":"L","ł":"l","Ń":"'N","ń":"'n","Ņ":"N","ņ":"n","Ň":"N","ň":"n","ŉ":"'n","Ō":"O","ō":"o","Ŏ":"O","ŏ":"o","Ő":'"O',"ő":'"o',"Œ":"OE","œ":"oe","Ŕ":"'R","ŕ":"'r","Ŗ":"R","ŗ":"r","Ř":"R","ř":"r","Ś":"'S","ś":"'s","Ŝ":"^S","ŝ":"^s","Ş":"S","ş":"s","Š":"S","š":"s","Ţ":"T","ţ":"t","Ť":"T","ť":"t","Ŧ":"T","ŧ":"t","Ũ":"~U","ũ":"~u","Ū":"U","ū":"u","Ŭ":"U","ŭ":"u","Ů":"U","ů":"u","Ű":'"U',"ű":'"u',"Ų":"U","ų":"u","Ŵ":"^W","ŵ":"^w","Ŷ":"^Y","ŷ":"^y","Ÿ":'"Y',"Ź":"'Z","ź":"'z","Ż":"Z","ż":"z","Ž":"Z","ž":"z","ſ":"s"},i=[],a=0;a<e.length;a++)(r=e.charCodeAt(a))<384&&(o=String.fromCharCode(r),i.push(n[o]||o));return e=i.join(""),e=e.replace(/[^\w\s-]/g,"").trim().toLowerCase(),e.replace(/[-\s]+/g,t)},roundBy:function(e,t,o){var r=t*Math.pow(10,o);return r=e(r),r/Math.pow(10,o)},parseQueryString:function(e){var t,o,r,n,i,a,s,l={};if(null===e)return l;t=e.split("&");for(var c=0;c<t.length;c++)o=t[c],r=o.indexOf("="),-1===r?(n=o,i=null):(n=o.substr(0,r),i=o.substr(r+1)),a=decodeURIComponent(n),s=decodeURIComponent(i),l[a]=s;return l},replaceDynamicValues:function(e){var t=e.split("{appConfigs.");if(t.length>1)for(var o in t)if(t.hasOwnProperty(o)){if(!t[o])continue;var r=t[o].split("}")[0];e=e.replace("{appConfigs."+r+"}",appConfigs[r])}return e}}}]).factory("convert",["helper",function(e){return{fromMsDate:function(t){return t=e.getTime(t),new Date(t)},toMsDate:function(e){return"/Date("+e.getTime()+")/"}}}]).factory("exportFile",function(){return{excel:function(e,t){for(var o=e[0].length-1,r=0,n={columns:"",rows:""},i={excel:'<html xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:x="urn:schemas-microsoft-com:office:excel" xmlns="http://www.w3.org/TR/REC-html40"><head><meta http-equiv="Content-Type" content="text/html; charset=UTF-8"><!--[if gte mso 9]><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet><x:Name>{worksheet}</x:Name><x:WorksheetOptions><x:DisplayGridlines/></x:WorksheetOptions></x:ExcelWorksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml><![endif]--></head><body><table>{table}</table></body></html>',excelML:'<?xml version="1.0"?><?mso-application progid="Excel.Sheet"?><ss:Workbook xmlns:="urn:schemas-microsoft-com:office:spreadsheet" xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:x="urn:schemas-microsoft-com:office:excel" xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet" xmlns:html="http://www.w3.org/TR/REC-html40"><ss:Styles><ss:Style ss:ID="1"><ss:Font ss:Bold="1"/></ss:Style></ss:Styles><ss:Worksheet ss:Name="Sheet1"><ss:Table>{columns}{rows}</ss:Table></ss:Worksheet></ss:Workbook>',rowOpen:"<ss:Row>",rowClose:"</ss:Row>",dataOpenString:'<ss:Data ss:Type="String">',dataOpenNumber:'<ss:Data ss:Type="Number">',dataClose:"</ss:Data>",column:'<ss:Column ss:Width="80"/>',cellOpen:"<ss:Cell>",cellClose:"</ss:Cell>"},a=function(e){if(null===e||void 0===e)return"";var t="<![CDATA["+e+"]]>",o=e.toString(),r=o.indexOf(",")>-1||o.indexOf("\r")>-1||o.indexOf("\n")>-1,n=o.indexOf('"')>-1;return n&&(t=t.replace(/"/g,'""')),(r||n)&&(t='"'+t+'"'),t},s=function(e){return window.btoa(unescape(encodeURIComponent(e)))},l=function(e,t){return e.replace(/{(\w+)}/g,function(e,o){return t[o]})},c=0;o>c;c++)n.columns+=i.column;for(var u,f=0;u=e[f];f++){n.rows+=i.rowOpen,r=e[f].length;for(var m=0;r>m;m++){var p=u[m];n.rows+=i.cellOpen,n.rows+="number"==typeof p?i.dataOpenNumber:i.dataOpenString,n.rows+=a(p),n.rows+=i.dataClose,n.rows+=i.cellClose}n.rows+=i.rowClose}for(var d=s(l(i.excelML,n)),g=atob(d),h=new Array(g.length),v=0;v<g.length;v++)h[v]=g.charCodeAt(v);var y=new Uint8Array(h),w=new Blob([y],{type:"application/octet-stream"});saveAs(w,t)}}}).factory("officeHelper",["$http","config",function(e,t){return{officeTenantInfo:function(){return e.get(t.apiUrl+"User/ActiveDirectoryInfo")}}}]).factory("customScripting",["$timeout","ModuleService","$http","config","$filter","blockUI","mdToast","$mdDialog",function($timeout,ModuleService,$http,config,$filter,blockUI,mdToast,$mdDialog){return{run:function(scope,customScript,ev){scope.toast=function(e,t,o){$timeout(function(){mdToast[t]({content:e,timeout:o||5e3})})};try{$timeout(function(){eval(customScript)})}catch(e){return scope.$parent.$parent.scriptRunning[scope.$parent.custombutton.id]=!1,null}}}}]).factory("components",["$rootScope","$timeout","$filter","$localStorage","$sessionStorage","$q","$http","config","$cache","$injector","$state","$stateParams","helper","mdToast","$mdDialog","blockUI",function($rootScope,$timeout,$filter,$localStorage,$sessionStorage,$q,$http,config,$cache,$injector,$state,$stateParams,helper,mdToast,$mdDialog,blockUI){return{run:function(place,type,scope,record,field,moduleName){place=place.split(/(?=[A-Z])/).join("_").toLowerCase(),type=type.split(/(?=[A-Z])/).join("_").toLowerCase();var ModuleService=$injector.get("ModuleService");if(moduleName&&$rootScope.modulus[moduleName]&&$rootScope.modulus[moduleName].components)var components=$filter("filter")($rootScope.modulus[moduleName].components,function(e){return e.place===place&&e.type===type&&(e.module_id===$rootScope.modulus[moduleName].id||0===e.module_id)&&!e.deleted},!0);else var components=$filter("filter")(scope.module.components,function(e){return e.place===place&&e.type===type&&(e.module_id===scope.module.id||0===e.module_id)&&!e.deleted},!0);if(components=$filter("orderBy")(components,"order"),components&&components.length>0)for(var i=0;i<components.length;i++){var component=components[i];eval(component.content)}}}}]),String.prototype.toUpperCaseTurkish=function(){return this.replace(/i/g,"İ").toLocaleUpperCase()},String.prototype.toLowerCaseTurkish=function(){return this.replace(/I/g,"ı").toLocaleLowerCase()},String.prototype.replaceAll=function(e,t){function o(e){return e.replace(/([.*+?^=!:${}()|\[\]\/\\])/g,"\\$1")}return this.replace(new RegExp(o(e),"g"),t)},Array.prototype.getUnique=function(){for(var e={},t=[],o=0,r=this.length;r>o;++o)e.hasOwnProperty(this[o])||(t.push(this[o]),e[this[o]]=1);return t},String.prototype.generateRandomKey=function(e){for(var t="",o="ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789",r=o.length,n=0;e>n;n++)t+=o.charAt(Math.floor(Math.random()*r));return t};