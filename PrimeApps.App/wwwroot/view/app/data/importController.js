"use strict";angular.module("primeapps").controller("ImportController",["$rootScope","$scope","$stateParams","$state","config","$q","$localStorage","$filter","helper","FileUploader","$timeout","$cache","emailRegex","ModuleService","ImportService","$cookies","components","$mdDialog","mdToast",function(e,a,t,r,i,l,n,o,d,u,s,p,m,c,f,g,v,h,x){function M(e){return new Date(86400*(e-25569)*1e3)}function k(e,t,r){const i=a.rows[t];return i?i[r].toString():void 0}function y(e){return o("filter")(a.cells,{column:e},!0)[0]}function _(e){var a={};for(var t in e)a[t]=e[t].column;return a}function b(e,a){if(e&&e.length>0){var t=o("filter")(e,{field:a.name},!0)[0];!t||"number"!==a.data_type&&"number_auto"!==a.data_type||(t.operator=t.operator.startsWith("starts")||"contains"===t.operator?"equals":t.operator)}}function w(e,a){if(angular.isNumber(e)||"droplist"===a)return e;if("date_time"!==a&&"date"!==a&&"time"!==a){var t=JSON.parse(e);return angular.isObject(t)?void 0:e}return e?new Date:void 0}a.type=t.type,a.wizardStep=0,a.fieldMap={},a.fixedValue={},a.importMapping={},a.mappingField={},a.selectedMapping={},a.mappingArray=[];var F=!1;if(a.loading=!1,a.isShowTryButton=!1,a.module=o("filter")(e.modules,{name:a.type},!0)[0],!a.module)return x.warning(o("translate")("Common.NotFound")),void r.go("app.dashboard");a.module=angular.copy(a.module),angular.forEach(a.module.fields,function(e){e.validation.required=!1,("created_by"===e.name||"updated_by"===e.name||"created_at"===e.name||"updated_at"===e.name)&&(d.hasAdminRights()||(e.deleted=!0))}),angular.forEach(a.module.sections,function(e){var t=o("filter")(a.module.fields,{section:e.name,deleted:"!true"});t.length||(e.deleted=!0),"component"===e.type&&(e.deleted=!0)}),f.getAllMapping(a.module.id).then(function(e){if(e){var t=e.data;a.mappingArray=t}});var D=function(e,t){var r=new FileReader;r.onload=function(e){if(!e||e.target.error)return void s(function(){x.warning({content:o("translate")("Module.ExportUnsupported"),timeout:1e4})});try{a.workbook=XLS.read(e.target.result,{type:"binary"}),a.sheets=a.workbook.SheetNames,s(function(){a.selectedSheet=a.selectedSheet||a.sheets[0],a.selectSheet(t),a.getSampleDate()})}catch(r){s(function(){x.warning(o("translate")("Data.Import.InvalidExcel")),a.loading=!1})}},r.readAsBinaryString(e)},S={column:"fixed",name:o("translate")("Data.Import.FixedValue")+" -->"},V={column:"currentUser",name:e.user.full_name};a.selectSheet=function(t,r){return a.selectedSheet=r||a.selectedSheet,a.rows=XLSX.utils.sheet_to_json(a.workbook.Sheets[a.selectedSheet],{raw:!0,header:"A"}),a.headerRow=angular.copy(a.rows[0]),a.rows.shift(),a.cells=[],a.rows.length>3e3?(x.warning(o("translate")("Data.Import.CountError")),void(a.loading=!1)):t?void a.submit(!0):(a.selectedMapping.name||(a.fieldMap={},a.fixedValue={},a.fixedValueFormatted={},angular.forEach(a.headerRow,function(t,r){var i=t+o("translate")("Data.Import.ColumnIndex",{index:r}),l={column:r,name:i,used:!1};a.cells.push(l);for(var n=0;n<a.module.fields.length;n++){var d=a.module.fields[n];if(!d.deleted){var u=e.getLanguageValue(d.languages,"label");if("owner"===d.name)a.fieldMap[d.name]=V;else if(u&&u.toLowerCase()===t.trim().toLowerCase())a.fieldMap[d.name]=y(r);else if(d&&d.default_value&&"checkbox"!==d.data_type){var s=w(d.default_value,d.data_type);if("picklist"===d.data_type){a.fieldMap[d.name]=S;var p=o("filter")(a.picklistsModule[d.picklist_id],{id:s})[0];a.fixedValue[d.name]=p,a.fixedValueFormatted[d.name]=e.getLanguageValue(p.languages,"label")}else if("droplist"===d.data_type){var m=o("filter")(window.droplist[d.droplist_id].items,{name:s},!0)[0];a.fieldMap[d.name]=S,a.fixedValue[d.name]=m,a.fixedValueFormatted[d.name]=m.label}else s?(a.fieldMap[d.name]=S,a.fixedValueFormatted[d.name]=a.fixedValue[d.name]=s):a.fieldMap[d.name]=y(r)}}}})),void(a.loading=!1))},u.FileSelect.prototype.isEmptyAfterSelection=function(){return!0};var L=a.uploader=new u;a.fileName=null,L.onAfterAddingFile=function(e){a.selectedSheet=void 0,a.loading=!0,null===a.fileName||a.fileName===e._file.name||0===a.wizardStep?(D(e._file),a.fileName=e._file.name):a.fileName!==e._file.name&&L.clearQueue()},L.onWhenAddingFileFailed=function(e,a){switch(a.name){case"excelFilter":x.warning(o("translate")("Data.Import.FormatError"));break;case"sizeFilter":x.warning(o("translate")("Data.Import.SizeError"))}},L.onBeforeUploadItem=function(e){e.url="storage/upload_import_excel?import_id="+a.importResponse.id},L.filters.push({name:"excelFilter",fn:function(e){var a=d.getFileExtension(e.name);return"xls"===a||"xlsx"===a}}),L.filters.push({name:"sizeFilter",fn:function(e){return e.size<2097152}}),c.getPicklists(a.module).then(function(t){e.processPicklistLanguages(t),a.picklistsModule=t}),a.multiselect=function(t,r){var i=[];return angular.forEach(a.picklistsModule[r.picklist_id],function(a){if(!a.inactive&&!a.hidden){var r=e.getLanguageValue(a.languages,"label");r&&r.toLowerCase().indexOf(t)>-1&&i.push(a),i.push(a)}}),i},a.clear=function(){L.clearQueue(),a.rows=null,a.cells=null,a.sheets=null,a.fieldMap=null,a.fixedValue=null,a.fixedValueFormatted=null,a.showAdvancedOptions=!1},a.cellChanged=function(e,t){return"fixed"===a.fieldMap[e.name]?void a.openFixedValueModal(e,t):(!a.fieldMap[e.name]&&a.fixedValue&&a.fixedValue[e.name]&&delete a.fixedValue[e.name],void("related_module"===e.name&&delete a.fixedValue.related_to))},a.fixedValueChanged=function(e){"related_module"===e.name&&delete a.fixedValue.related_to},a.openFixedValueModal=function(e,t){a.fixedValue=a.fixedValue||{},a.fixedValueState=angular.copy(a.fixedValue),a.fixedField=e;var r=angular.element(document.body);h.show({parent:r,templateUrl:"view/app/data/fixedValue.html",clickOutsideToClose:!0,targetEvent:t,scope:a,preserveScope:!0})},a.closeDialog=function(){h.hide()},a.modalSubmit=function(e){e.validate()&&(a.fixedValue[a.fixedField.name]||delete a.fieldMap[a.fixedField.name],a.fixedValueFormatted=angular.copy(a.fixedValue),c.formatRecordFieldValues(a.fixedValueFormatted,a.module,a.picklistsModule),angular.forEach(a.fixedValueFormatted,function(e,t){var r=o("filter")(a.module.fields,{name:t},!0)[0];r&&r.valueFormatted&&(a.fixedValueFormatted[t]=r.valueFormatted)}),a.excelCellOptions(a.fixedField,!0),a.closeDialog())},a.modalCancel=function(){!a.fixedValueState[a.fixedField.name]&&a.fixedValue[a.fixedField.name]&&delete a.fixedValue[a.fixedField.name],a.fixedValue[a.fixedField.name]||delete a.fieldMap[a.fixedField.name]},a.loadMappingList=function(){f.getAllMapping(a.module.id).then(function(e){e.data&&(a.mappingArray=e.data)}),a.mappingDropDown2.dataSource.read()},a.mappingSave=function(e){a.savingTemplate=!0,a.importMapping.module_id=a.module.id,a.fixedValue&&(a.fieldMap.fixed=angular.copy(a.fixedValue)),a.fixedValueFormatted&&(a.fieldMap.fixedFormat=angular.copy(a.fixedValueFormatted));var t=angular.copy(a.fieldMap);return t?a.importMapping.mapping=JSON.stringify(t):x.warning({content:o("translate")("Data.Import.MappingNotFound"),timeout:6e3}),a.importMapping.skip=a.importMapping.skip?a.importMapping.skip:!1,e.$valid?a.importMapping.name||a.selectedMapping?void(a.selectedMapping.id?f.updateMapping(a.selectedMapping.id,a.importMapping).then(function(e){var t=e.data;t?(a.loadMappingList(),s(function(){x.success({content:o("translate")("Data.Import.MappingSaveSucces"),timeout:6e3}),a.savingTemplate=!1,a.cancel()},500)):a.savingTemplate=!1})["catch"](function(){x.error({content:o("translate")("Common.Error"),timeout:6e3}),a.savingTemplate=!1,a.cancel()}):f.getMapping(a.importMapping).then(function(e){var t=e.data;t?(x.warning({content:o("translate")("Data.Import.TryMappingName"),timeout:6e3}),a.importMapping={},a.savingTemplate=!1,a.cancel()):f.saveMapping(a.importMapping).then(function(e){var t=e.data;t?s(function(){a.loadMappingList(),x.success({content:o("translate")("Data.Import.MappingSaveSucces"),timeout:6e3}),a.savingTemplate=!1,a.cancel(),f.getAllMapping(a.module.id).then(function(e){e.data&&(a.mappingArray=e.data,a.selectedMapping=t,a.mappingDropDown2.dataSource.read(),a.mappingSelectedChange(t))})},500):a.savingTemplate=!1})["catch"](function(){x.error({content:o("translate")("Common.Error"),timeout:6e3}),a.savingTemplate=!1,a.cancel()})})):(a.importMapping={},void(a.savingTemplate=!1)):void(a.savingTemplate=!1)},a.deleteMapping=function(e){a.selectedMapping=e,v.run("BeforeDelete","Script",a,a.selectedMapping),f.deleteMapping(a.selectedMapping).then(function(){a.loadMappingList(),x.success({content:o("translate")("Data.Import.DeletedMapping"),timeout:6e3})})["catch"](function(){x.error({content:o("translate")("Common.Error"),timeout:6e3})})},a.showConfirm=function(e,t){var r=h.confirm().title(o("translate")("Common.AreYouSure")).targetEvent(t).ok(o("translate")("Common.Yes")).cancel(o("translate")("Common.No"));h.show(r).then(function(){a.deleteMapping(e)},function(){})},a.mappingModalCancel=function(){a.importMapping={}},a.openMappingModal=function(){if(!a.importForm.validate())return void x.warning(o("translate")("Module.RequiredError"));var e=angular.element(document.body);h.show({parent:e,templateUrl:"view/app/data/importMappingSave.html",clickOutsideToClose:!1,scope:a,preserveScope:!0})},a.mappingSelectedChange=function(e){if(e&&e.id){a.selectedMapping=e;var t=o("filter")(a.mappingArray,{id:e.id},!0)[0];if(t){a.importMapping.name=t.name,a.importMapping.skip=t.skip;var r=angular.fromJson(t.mapping);r&&(r.fixed&&(a.fixedValue=r.fixed,delete r.fixed),r.fixedFormat&&(a.fixedValueFormatted=r.fixedFormat,delete r.fixedFormat),a.fieldMap=r)}}else a.selectedMapping={},a.importMapping={}},a.checkWizard=function(){var e=a.user.profile.has_admin_rights;a.fieldMapCopy=angular.copy(a.fieldMap),a.selectedMapping.skip&&!e?(a.wizardStep=2,F=!0,a.submit(!0)):(F=!1,a.wizardStep=1)},a.getDateFormat=function(){var e=angular.copy(a.dateOrder);return e=e.replace(/\//g,a.dateDelimiter)},a.getDateTimeFormat=function(){var e=a.getDateFormat();return e+=" "+a.timeFormat},a.getSampleDate=function(){a.dateOrder||(a.dateOrder="en"===e.locale?"M/D/YYYY":"DD/MM/YYYY"),a.dateDelimiter||(a.dateDelimiter="en"===e.locale?"/":"."),a.timeFormat||(a.timeFormat="en"===e.locale?"H:mm a":"HH:mm");var t=new Date("2010-01-31 16:20:00"),r=a.getDateTimeFormat();a.sampleDate=moment(t).format(r)},a.prepareFixedValue=function(){var t=angular.copy(a.fixedValue);return angular.forEach(t,function(r,i){var l=o("filter")(a.module.fields,{name:i},!0)[0];switch(l.data_type){case"number":t[i]=parseInt(t[i]);break;case"number_decimal":case"currency":t[i]=parseFloat(t[i]);break;case"picklist":t[i]=null!=t[i]?t[i].labelStr:null;break;case"droplist":t[i]=null!=t[i]?t[i].name:null;break;case"tag":var n=t[i].split("|");t[i]="{";for(var d=0;d<n.length;d++)t[i]+='"'+n[d]+'",';t[i]&&(t[i]=t[i].slice(0,-1)+"}");break;case"multiselect_droplist":var u="{{";angular.forEach(t[i],function(a){u+='"'+e.getLanguageValue(a.languages,"label")+'",'}),t[i]=u.slice(0,-1)+"}}";break;case"multiselect":var u="{{";angular.forEach(t[i],function(a){u+='"'+e.getLanguageValue(a.languages,"label")+'",'}),t[i]=u.slice(0,-1)+"}}";break;case"lookup":t[i]=t[i].id}}),t},a.picklistLang=o("filter")(e.globalizations,function(e){return e.Culture.split("-")[0]===tenantLanguage?e:void 0})[0],a.prepareRecords=function(){for(var t=[],r=function(t,r,i,l){var n="";switch(t&&(n=t.toString().trim()),a.error={},a.error.rowNo=i,a.error.cellName=l,a.error.cellValue=t,a.error.fieldLabel=e.getLanguageValue(r.languages,"label"),r.data_type){case"number":n=parseInt(n),isNaN(n)&&(a.error.message=o("translate")("Data.Import.Error.InvalidNumber"),n=0);break;case"number_decimal":n=parseFloat(n),isNaN(n)&&(a.error.message=o("translate")("Data.Import.Error.InvalidNumber"),n=0);break;case"currency":n=n.replace(",","."),n=parseFloat(n),isNaN(n)&&(a.error.message=o("translate")("Data.Import.Error.InvalidDecimal"),n=null);break;case"date":var d=M(n);n="Invalid Date"===d.toString()?n.split(" ")[0]:d;var u=a.getDateFormat(),s="en"===a.locale?"D/M/YYYY":"DD/MM/YYYY",p=moment(n,u,!0);if(p.isValid())n=p.format();else if(p=moment(n,s,!0),!p.isValid()){var c=a.dateDelimiter,f=n.split(dateDelimiter),g=u.split(dateDelimiter);if(f.length>1){var v=f[g.indexOf("YYYY")],h=f[g.indexOf("MM")],x=f[g.indexOf("DD")],y=Date.parse(h+c+x+dateDelimiter+v);p=moment(new Date(y),u,!0),p.isValid()?n=p.format():(a.error.message=o("translate")("Data.Import.Error.InvalidDate"),n=null)}}break;case"date_time":var _=angular.copy(n),d=M(n);n="Invalid Date"===d.toString()?n.split(" ")[0]:d;var b=a.getDateTimeFormat(),w="en"===a.locale?"D/M/YYYY":"DD/MM/YYYY";w+=" "+a.timeFormat;var F=moment(n,b,!0);F.isValid()?n=F.toDate():(F=moment(_,b,!0),F.isValid()?n=F.toDate():(F=moment(n,w,!0),F.isValid()?n=F.toDate():(a.error.message=o("translate")("Data.Import.Error.InvalidDateTime"),n=null)));break;case"time":var D,S=k(r,i-1,l),V=[];S?(V=S.split(":"),V[1]="en"===a.locale?V[1].split(" ")[0]:V[1],D=moment(new Date(null,null,null,V[0],V[1]),a.timeFormat,!0)):(n=M(n),D=moment(n.toUTCString(),a.timeFormat,!0)),D.isValid()?n=D.format():(a.error.message=o("translate")("Data.Import.Error.InvalidTime"),n=null);break;case"email":m.test(n)||(a.error.message=o("translate")("Data.Import.Error.InvalidEmail"),n=null);break;case"picklist":var L={languages:{}};L.languages[e.globalization.Label]={},L.languages[e.globalization.Label].label=n;var I=o("filter")(a.picklistsModule[r.picklist_id],L,!0)[0];if(!I)return a.isShowTryButton=!0,a.error.message=o("translate")("Data.Import.Error.PicklistItemNotFound"),void(n=null);n=I.languages[a.picklistLang.Label].label;break;case"droplist":var T=o("filter")(window.droplist[r.droplist_id].items,{label:n},!0)[0];if(!T)return a.isShowTryButton=!0,a.error.message=o("translate")("Data.Import.Error.PicklistItemNotFound"),void(n=null);n=T.name;break;case"multiselect":var C=n.split("|");n="{{";for(var E=0;E<C.length;E++){var N=C[E],O={languages:{}};O.languages[e.globalization.Label]={},O.languages[e.globalization.Label].label=C[E];var Y=o("filter")(a.picklistsModule[r.picklist_id],O)[0];if(!Y)return a.isShowTryButton=!0,a.error.message=o("translate")("Data.Import.Error.MultiselectItemNotFound",{item:N}),void(n=null);n+='"'+Y.languages[a.picklistLang.Label].label+'",'}n&&(n=n.slice(0,-1)+"}}");break;case"multiselect_droplist":var P=n.split("|");n=[];for(var E=0;E<P.length;E++){var A=P[E],B=o("filter")(window.droplist[r.droplist_id].items,{label:P[E]})[0];if(!B)return a.isShowTryButton=!0,a.error.message=o("translate")("Data.Import.Error.MultiselectItemNotFound",{item:A}),void(n=null);n.push(B.name)}n="{"+n.join(",")+"}";break;case"lookup":"relation"===r.lookup_type&&(r.lookup_type=a.fixedValue.related_module.value);var z=!0;r.lookupModulePrimaryField&&"number"===r.lookupModulePrimaryField.data_type&&(z=!1);var R=[];if(a.lookupIds[r.lookup_type])for(var U=0;U<a.lookupIds[r.lookup_type].length;U++){var q=a.lookupIds[r.lookup_type][U],$=n%1===0;z&&$&&q.id===parseInt(n)?R.push(q):q.value==n&&R.push(q)}var j=o("filter")(e.modules,{name:r.lookup_type})[0];if("users"===r.lookup_type&&(j={languages:{}},j.languages[e.globalization.Label]={label:{singular:"User"}}),R.length>1)return a.isShowTryButton=!0,a.error.message=o("translate")("Data.Import.Error.LookupMoreThanOne",{module:e.getLanguageValue(j.languages,"label","singular")}),void(n=null);var J=R[0];if(!J)return a.isShowTryButton=!0,a.error.message=o("translate")("Data.Import.Error.LookupNotFound",{module:e.getLanguageValue(j.languages,"label","singular")}),void(n=null);n=J.id;break;case"checkbox":"yes"===n.toLowerCase()||"evet"===n.toLowerCase()||"true"===n.toLowerCase()?n="true":"no"===n.toLowerCase()||"hayır"===n.toLowerCaseTurkish()||"false"===n.toLowerCase()?n="false":(a.error.message=o("translate")("Data.Import.Error.InvalidCheckbox"),n=null)}if(null!==n&&void 0!==n){if(r.validation||(r.validation={}),!r.validation.max_length)if(r.combination)r.validation.max_length=250;else switch(r.data_type){case"text_single":r.validation.max_length=50;break;case"text_multi":r.validation.max_length=32720;break;case"number":r.validation.max_length=15;break;case"number_decimal":r.validation.max_length=19;break;case"currency":r.validation.max_length=21;break;case"email":r.validation.max_length=100}if(r.validation.max||"number"!==r.data_type&&"number_decimal"!==r.data_type&&"currency"!==r.data_type||(r.validation.max=Number.MAX_VALUE),r.validation.max_length&&n.toString().length>r.validation.max_length&&(a.error.message=o("translate")("Data.Import.Validation.MaxLength",{maxLength:r.validation.max_length}),n=null),r.validation.min_length&&n.toString().length<r.validation.min_length&&(a.error.message=o("translate")("Data.Import.Validation.MinLength",{minLength:r.validation.min_length}),n=null),r.validation.max&&n>r.validation.max&&(a.error.message=o("translate")("Data.Import.Validation.Max",{max:r.validation.max}),n=null),r.validation.min&&n<r.validation.min&&(a.error.message=o("translate")("Data.Import.Validation.Min",{min:r.validation.min}),n=null),r.validation.pattern){var Q=new RegExp(r.validation.pattern);Q.test(n)||(a.error.message=o("translate")("Data.Import.Validation.Pattern"),n=null)}}return null!=n&&void 0!=n&&(a.error=null),n},i=a.prepareFixedValue(),l=0;l<a.rows.length;l++){var n={},d=a.rows[l];a.error=null;var u=_(a.fieldMap);for(var s in u)if("fixed"!==s&&"fixedFormat"!==s&&u.hasOwnProperty(s)){var p=u[s];if("fixed"===p)n[s]=i[s];else{var c=angular.copy(o("filter")(a.module.fields,{name:s},!0)[0]),f=d[p];if(c.validation&&c.validation.required&&!f){a.error={},a.error.rowNo=l+2,a.error.cellName=p,a.error.cellValue=f,a.error.fieldLabel=e.getLanguageValue(c.languages,"label"),a.error.message=o("translate")("Data.Import.Error.Required"),a.isShowTryButton=!0;break}if(!f&&0!==f)continue;var g=r(f,c,l+2,p);if(angular.isUndefined(g))break;n[s]=g}}if(a.error)break;t.push(n)}return a.preparing=!1,t},a.getLookupIds=function(){var t=l.defer(),r=[],i=_(a.fieldMap);if(!a.processing)for(var n=0;n<a.rows.length;n++){var d=a.rows[n];if(d[i.owner]){var u=o("filter")(a.users,function(e){var a=e.first_name+" "+e.last_name;return a.trim().toLowerCaseTurkish()===d[i.owner].toString().trim().toLowerCaseTurkish()},!0)[0];u||(d[a.fieldMap.owner]=e.user.full_name)}else d[i.owner]=e.user.full_name;for(var s in d)if(d.hasOwnProperty(s)){var p=d[s].toString().trim();for(var m in i)if(i.hasOwnProperty(m)){var c=i[m];if("fixed"!==m&&s===c){var g=angular.copy(o("filter")(a.module.fields,{name:m},!0)[0]);if("lookup"!==g.data_type)break;"relation"===g.lookup_type&&(g.lookup_type=a.fixedValue.related_module.value);var v=o("filter")(r,{type:g.lookup_type},!0)[0];if(!v){if(v={},v.type=g.lookup_type,v.values=[],"users"!==g.lookup_type){var h=o("filter")(e.modules,{name:g.lookup_type},!0)[0];if(h){var g=o("filter")(h.fields,{primary_lookup:!0},!0)[0];g?v.field=g.name:(g=o("filter")(h.fields,{primary:!0},!0)[0],g&&(v.field=g.name))}}else v.field=p.indexOf("@")>-1?"email":"full_name";p&&v.values.indexOf(p)<0&&r.push(v)}p&&v.values.indexOf(p)<0&&v.values.push(p)}}}}return r.length?a.processing||(a.processing=!0,f.getLookupIds(r).then(function(e){t.resolve(e.data),a.processing=!1})["catch"](function(e){t.reject(e.data),a.processing=!1})):t.resolve([]),t.promise},a.submitLock=!1,a.submit=function(e){if(!a.submitLock){if(a.submitLock=!0,a.error=null,a.errorUnique=null,a.processing=!1,!a.importForm.validate())return void x.warning(o("translate")("Module.RequiredError"));if(F)return void(F=!1);e||(a.wizardStep=2),a.preparing=!0,a.trying=!1,e?a.fieldMap=a.fieldMapOld:a.fieldMapOld=a.fieldMap,a.saving=!0,s(function(){a.getLookupIds().then(function(e){a.lookupIds=e,a.records=a.prepareRecords(),a.error&&null!=a.error.message&&L.clearQueue(),v.run("BeforeImport","Script",a),a.submitLock=!1,a.saving=!1})["catch"](function(){a.submitLock=!1,a.saving=!1})},200)}},a.tryAgain=function(){a.isShowTryButton=!1,a.trying=!0,a.error=null,a.errorUnique=null;var e=L.queue[0]._file;D(e,!0)},a.saveLoaded=function(){a.getLookupIds().then(function(e){a.lookupIds=e,a.records=a.prepareRecords(),a.save()})},a.save=function(){return!a.records&&a.selectedMapping.id?void a.saveLoaded():(a.saving=!0,s(function(){a.saving&&(a.longProcessing=!0)},4e3),void f["import"](a.records,a.module.name).then(function(e){var t=a.module.name+"_"+a.module.name;p.remove(t),a.importResponse=e.data,a.fileName=null,s(function(){a.saving=!1,a.longProcessing=!1,a.isShowTryButton=!1,L.uploadAll(),x.success({content:o("translate")("Data.Import.Success"),timeout:6e3}),r.go("app.moduleList",{type:a.type})},500)})["catch"](function(e){409===e.status&&(a.errorUnique={},a.errorUnique.field=e.data.field,a.isShowTryButton=!0,a.saving=!1,e.data.field2&&(a.errorUnique.field=e.data.field2),L.clearQueue()),a.saving=!1,a.longProcessing=!1}))},a.combinationFilter=function(e){return!e.combination},a.mappingControl=function(){a.importForm.validate()||(a.wizardStep=2)},a.cancel=function(){h.cancel()};var I=[];angular.element(document).ready(function(){a.excelCellOptions=function(e,t){if(I=[],"related_module"!==e.name&&(I=angular.copy(a.cells)),S.name=a.fixedValue[e.name]?a.fixedValueFormatted[e.name]:o("translate")("Data.Import.FixedValue")+" -->",I.push(S),I.push(V),!t)return{dataSource:I,autoBind:!0,dataTextField:"name",dataValueField:"column"};var r=angular.element("#"+e.name).data("kendoDropDownList");r.setDataSource(I)},a.fieldMap=angular.copy(a.fieldMapCopy)}),a.mappingOptions={dataSource:{transport:{read:{url:"/api/data/import_get_all_mappings/"+a.module.id,type:"GET",dataType:"json",beforeSend:e.beforeSend(),cache:!1}}},dataTextField:"name",dataValueField:"id",autoBind:!0},a.dateMinValidationControl=function(e){return e.validation.min?"today"===e.validation.min?a.currentDayMin:e.validation.min:new Date(2e3,0,1,0,0,0)},a.dateMaxValidationControl=function(e){return e.validation.max?"today"===e.validation.max?a.currentDayMax:e.validation.max:new Date(2099,0,1,0,0,0)},a.fixedSelectListOption=function(t){var r={};if("picklist"==t.data_type){var i=o("filter")(a.picklistsModule[t.picklist_id],{inactive:"!true",hidden:"!true"},!0);r={dataSource:i,dataTextField:"languages."+e.globalization.Label+".label",dataValueField:"id"}}else{var l=o("filter")(window.droplist[t.droplist_id].items,{inactive:"!true",hidden:"!true"},!0);r={dataSource:l,dataTextField:"label",dataValueField:"name"}}return r},a.fixedMultiSelect=function(t){var r=o("filter")(a.picklistsModule[t.picklist_id],{inactive:"!true",hidden:"!true"},!0);return{placeholder:o("translate")("Common.MultiselectPlaceholder"),dataTextField:"languages."+e.globalization.Label+".label",valuePrimitive:!0,autoBind:!1,dataSource:r}},a.fixedMultiSelectDroplist=function(e){var a=o("filter")(window.droplist[e.droplist_id].items,{inactive:"!true",hidden:"!true"},!0);return{placeholder:o("translate")("Common.MultiselectPlaceholder"),dataTextField:"label",valuePrimitive:!0,autoBind:!1,dataSource:a}},a.fixedLookupOptions=function(t){return{dataSource:new kendo.data.DataSource({serverFiltering:!0,transport:{read:function(r){const i={module:t.lookup_type,convert:!1};r.data.filter||(r.data.filter={filters:[]});var l=Object.assign({},r.data.filter.filters[0]);if(!r.data.filter||r.data.filter&&0===r.data.filter.filters.length){if(r.data.filter={},r.data.filter.filters=[],r.data.filter.logic="and",t&&t.filters.length>0)for(var n=0;n<t.filters.length;n++){const l=t.filters[n];r.data.filter.filters.push({field:l.filter_field,operator:l.operator,value:l.value})}var o={field:t.lookupModulePrimaryField.name,operator:t.lookup_search_type&&""!==t.lookup_search_type?t.lookup_search_type.replace("_",""):"startswith",value:""};("number"===t.lookupModulePrimaryField.data_type||"number_auto"===t.lookupModulePrimaryField.data_type)&&(o.operator="not_empty",o.value="not_empty"===o.value?"-":o.value)}if(r.data.filter.filters.length>0)if(b(r.data.filter.filters,t.lookupModulePrimaryField),r.data.filter.filters=c.fieldLookupFilters(t,a.record,r.data.filter.filters,i),i.fields.push(t.lookupModulePrimaryField.name),"number"===t.lookupModulePrimaryField.data_type||"number_auto"===t.lookupModulePrimaryField.data_type)r.data.filter.filters[0].operator||(r.data.filter.filters[0].operator="equals");else if(r.data.filter.filters.length>0){const d=r.data.filter.filters[0].operator;d.contains("_")&&(r.data.filter.filters[0].operator=""!==d?d.replace("_",""):"startswith")}$.ajax({url:"/api/record/find_custom",contentType:"application/json",dataType:"json",type:"POST",data:JSON.stringify(Object.assign(i,r.data)),success:function(e){r.success(e)},beforeSend:e.beforeSend()})}},schema:{data:"data",total:"total",model:{id:"id"}}}),optionLabel:o("translate")("Common.Select"),autoBind:!1,dataTextField:t.lookupModulePrimaryField.name,dataValueField:"id",filter:"startswith"}},a.getColumnName=function(e){var t=a.fieldMap[e];return t?t.column:void 0},a.getFields=function(e,t){var r=o("filter")(a.module.fields,function(a){return!a.deleted&&"number_auto"!==a.data_type&&a.section===e&&a.section_column===t&&!a.combination},!0);return o("orderBy")(r,"order")}}]);