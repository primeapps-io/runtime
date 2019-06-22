"use strict";angular.module("primeapps").controller("ImportController",["$rootScope","$scope","$stateParams","$state","config","$q","$localStorage","$filter","$popover","helper","FileUploader","ngToast","$modal","$timeout","$cache","emailRegex","ModuleService","ImportService","$cookies","components",function(e,a,t,r,i,n,l,o,d,p,u,s,m,c,f,g,v,h,x,M){function _(e){return new Date(86400*(e-25569)*1e3)}function k(e,t,r){var i=a.rowsBase[t],n=i[r];return n}a.type=t.type,a.wizardStep=0,a.fieldMap={},a.fixedValue={},a.importMapping={},a.mappingField={},a.selectedMapping={},a.mappingArray=[];var F=!1;if(a.module=o("filter")(e.modules,{name:a.type},!0)[0],!a.module)return s.create({content:o("translate")("Common.NotFound"),className:"warning"}),void r.go("app.dashboard");a.module=angular.copy(a.module),angular.forEach(a.module.fields,function(e){("created_by"===e.name||"updated_by"===e.name||"created_at"===e.name||"updated_at"===e.name)&&(e.validation.required=!1,p.hasAdminRights()||(e.deleted=!0)),"activities"===a.module.name&&("owner"!=e.name&&"activity_type"!=e.name&&"subject"!=e.name&&(e.validation.required=!1),("task_reminder"===e.name||"reminder_recurrence"===e.name||"task_notification"===e.name||"event_reminder"===e.name)&&(e.deleted=!0))}),angular.forEach(a.module.sections,function(e){var t=o("filter")(a.module.fields,{section:e.name,deleted:"!true"});t.length||(e.deleted=!0)}),h.getAllMapping(a.module.id).then(function(e){if(e){var t=e.data;a.mappingArray=t}});var y=function(e,t){var r=new FileReader;r.onload=function(e){if(!e||e.target.error)return void c(function(){s.create({content:o("translate")("Module.ExportUnsupported"),className:"warning",timeout:1e4})});try{a.workbook=XLS.read(e.target.result,{type:"binary"}),a.sheets=a.workbook.SheetNames,c(function(){a.selectedSheet=a.selectedSheet||a.sheets[0],a.selectSheet(t),a.getSampleDate()})}catch(r){c(function(){s.create({content:o("translate")("Data.Import.InvalidExcel"),className:"warning"})})}},r.readAsBinaryString(e)};a.selectSheet=function(t){return a.rowsBase=XLSX.utils.sheet_to_json(a.workbook.Sheets[a.selectedSheet],{header:"A"}),a.rows=XLSX.utils.sheet_to_json(a.workbook.Sheets[a.selectedSheet],{raw:!0,header:"A"}),a.headerRow=angular.copy(a.rows[0]),a.rows.shift(),a.cells=[],a.rows.length>3e3?void s.create({content:o("translate")("Data.Import.CountError"),className:"warning"}):(angular.forEach(a.headerRow,function(e,t){var r=e+o("translate")("Data.Import.ColumnIndex",{index:t});a.cells.push({column:t,name:r,used:!1})}),t?void a.submit(!0):void c(function(){a.selectedMapping.name||(a.fieldMap={},angular.forEach(a.headerRow,function(t,r){var i=!1;"tr"===e.language?angular.forEach(a.module.fields,function(e){e.deleted||e.label_tr.toLowerCaseTurkish()===t.trim().toLowerCaseTurkish()&&(i=!0,a.fieldMap[e.name]=r)}):angular.forEach(a.module.fields,function(e){e.deleted||e.label_tr.toLowerCase()===t.trim().toLowerCase()&&(i=!0,a.fieldMap[e.name]=r)});var n=o("filter")(a.cells,{column:r},!0)[0];n&&(n.used=i)}))}))};var b=a.uploader=new u({queueLimit:1});b.onAfterAddingFile=function(e){y(e._file)},b.onWhenAddingFileFailed=function(e,a){switch(a.name){case"excelFilter":s.create({content:o("translate")("Data.Import.FormatError"),className:"warning"});break;case"sizeFilter":s.create({content:o("translate")("Data.Import.SizeError"),className:"warning"})}},b.onBeforeUploadItem=function(e){e.url="storage/upload_import_excel?import_id="+a.importResponse.id},b.filters.push({name:"excelFilter",fn:function(e){var a=p.getFileExtension(e.name);return"xls"===a||"xlsx"===a}}),b.filters.push({name:"sizeFilter",fn:function(e){return e.size<2097152}}),v.getPicklists(a.module).then(function(e){a.picklistsModule=e}),a.lookup=function(t){if("users"===a.fixedField.lookup_type&&!a.fixedField.lookupModulePrimaryField){var r={};r.data_type="text_single",r.name="full_name",a.fixedField.lookupModulePrimaryField=r}if("relation"===a.fixedField.lookup_type){if(!a.fixedValue.related_module)return a.$broadcast("angucomplete-alt:clearInput",a.fixedField.name),n.defer().promise;var i=o("filter")(e.modules,{name:a.fixedValue.related_module.value},!0)[0];if(!i)return a.$broadcast("angucomplete-alt:clearInput",a.fixedField.name),n.defer().promise;a.fixedField.lookupModulePrimaryField=o("filter")(i.fields,{primary:!0},!0)[0]}return"number"!==a.fixedField.lookupModulePrimaryField.data_type&&"number_auto"!==a.fixedField.lookupModulePrimaryField.data_type||!isNaN(parseFloat(t))?v.lookup(t,a.fixedField,a.fixedValue):(a.$broadcast("angucomplete-alt:clearInput",a.fixedField.name),n.defer().promise)},a.multiselect=function(e,t){var r=[];return angular.forEach(a.picklistsModule[t.picklist_id],function(a){a.inactive||a.hidden||a.labelStr.toLowerCase().indexOf(e)>-1&&r.push(a)}),r},a.clear=function(){b.clearQueue(),a.rows=null,a.cells=null,a.sheets=null,a.fieldMap=null,a.fixedValue=null,a.fixedValueFormatted=null,a.importForm.$setPristine(),a.showAdvancedOptions=!1},a.cellChanged=function(e){return"fixed"===a.fieldMap[e.name]?void a.openFixedValueModal(e):(angular.forEach(a.cells,function(e){e.used=!1}),angular.forEach(a.fieldMap,function(e){var t=o("filter")(a.cells,{column:e},!0)[0];t&&(t.used=!0)}),!a.fieldMap[e.name]&&a.fixedValue&&a.fixedValue[e.name]&&delete a.fixedValue[e.name],void("related_module"===e.name&&delete a.fixedValue.related_to))},a.fixedValueChanged=function(e){"related_module"===e.name&&delete a.fixedValue.related_to},a.openFixedValueModal=function(e){a.fixedValue=a.fixedValue||{},a.fixedValueState=angular.copy(a.fixedValue),a.fixedField=e,a.fixedValueModal=a.fixedValueModal||m({scope:a,templateUrl:"view/app/data/fixedValue.html",animation:"",backdrop:"static",show:!1}),a.fixedValueModal.$promise.then(function(){a.fixedValueModal.show()})},a.modalSubmit=function(e){e.$valid&&(a.fixedValue[a.fixedField.name]||delete a.fieldMap[a.fixedField.name],a.fixedValueFormatted=angular.copy(a.fixedValue),v.formatRecordFieldValues(a.fixedValueFormatted,a.module,a.picklistsModule),angular.forEach(a.fixedValueFormatted,function(e,t){var r=o("filter")(a.module.fields,{name:t},!0)[0];r&&r.valueFormatted&&(a.fixedValueFormatted[t]=r.valueFormatted),"lookup"===r.data_type&&(a.fixedValueFormatted[t]=a.fixedValueFormatted[t].primary_value)}),a.fixedValueModal.hide())},a.modalCancel=function(){!a.fixedValueState[a.fixedField.name]&&a.fixedValue[a.fixedField.name]&&delete a.fixedValue[a.fixedField.name],a.fixedValue[a.fixedField.name]||delete a.fieldMap[a.fixedField.name]},a.mappingSave=function(e){a.savingTemplate=!0,a.importMapping.module_id=a.module.id,a.$parent.$parent.fixedValue&&(a.$parent.$parent.fieldMap.fixed=angular.copy(a.$parent.$parent.fixedValue)),a.fixedValueFormatted&&(a.$parent.$parent.fieldMap.fixedFormat=angular.copy(a.$parent.$parent.fixedValueFormatted));var t=angular.copy(a.$parent.$parent.fieldMap);return t?a.importMapping.mapping=JSON.stringify(t):(s.create({content:o("translate")("Data.Import.MappingNotFound"),className:"warning",timeout:6e3}),a.importMappingSaveModal.hide()),a.importMapping.skip=a.$parent.importMapping.skip?a.$parent.importMapping.skip:!1,e.$valid?a.$parent.importMapping.name||a.$parent.selectedMapping?(a.importMapping.name=a.$parent.importMapping.name,void(a.$parent.selectedMapping.id?h.updateMapping(a.$parent.selectedMapping.id,a.importMapping).then(function(e){var t=e.data;t?c(function(){s.create({content:o("translate")("Data.Import.MappingSaveSucces"),className:"success",timeout:6e3}),a.savingTemplate=!1,a.importMappingSaveModal.hide()},500):a.savingTemplate=!1})["catch"](function(){s.create({content:o("translate")("Common.Error"),className:"danger",timeout:6e3}),a.savingTemplate=!1,a.importMappingSaveModal.hide()}):h.getMapping(a.importMapping).then(function(e){var t=e.data;t?(s.create({content:o("translate")("Data.Import.TryMappingName"),className:"warning",timeout:6e3}),a.$parent.importMapping={},a.savingTemplate=!1,a.importMappingSaveModal.hide()):h.saveMapping(a.importMapping).then(function(e){var t=e.data;t?c(function(){s.create({content:o("translate")("Data.Import.MappingSaveSucces"),className:"success",timeout:6e3}),a.savingTemplate=!1,a.importMappingSaveModal.hide(),h.getAllMapping(a.module.id).then(function(e){e.data&&(a.$parent.$parent.$parent.mappingArray=e.data,a.$parent.$parent.$parent.selectedMapping=t,a.mappingSelectedChange())})},500):a.savingTemplate=!1})["catch"](function(){s.create({content:o("translate")("Common.Error"),className:"danger",timeout:6e3}),a.savingTemplate=!1,a.importMappingSaveModal.hide()})}))):(a.importMapping={},void(a.savingTemplate=!1)):void(a.savingTemplate=!1)},a.deleteMapping=function(){M.run("BeforeDelete","Script",a,a.selectedMapping),h.deleteMapping(a.selectedMapping).then(function(){s.create({content:o("translate")("Data.Import.DeletedMapping"),className:"success",timeout:6e3}),r.go("app.moduleList",{type:a.type})})["catch"](function(){s.create({content:o("translate")("Common.Error"),className:"danger",timeout:6e3})})},a.mappingModalCancel=function(){a.importMapping={}},a.openMappingModal=function(){F=!0,a.importForm.$valid&&(a.importMappingSaveModal=a.importMappingSaveModal||m({scope:a,templateUrl:"view/app/data/importMappingSave.html",animation:"",backdrop:"static",show:!1}),a.importMappingSaveModal.$promise.then(function(){a.importMappingSaveModal.show()}))},a.mappingSelectedChange=function(){if(a.selectedMapping.id){var e=o("filter")(a.mappingArray,{id:a.selectedMapping.id},!0)[0];if(e){a.importMapping.name=e.name,a.importMapping.skip=e.skip;var t=angular.fromJson(e.mapping);t&&(t.fixed&&(a.fixedValue=t.fixed,delete t.fixed),t.fixedFormat&&(a.fixedValueFormatted=t.fixedFormat,delete t.fixedFormat),a.fieldMap=t)}}},a.checkWizard=function(){var e=a.user.profile.has_admin_rights;a.selectedMapping.skip&&!e?(a.wizardStep=2,F=!0,a.submit(!0)):(F=!1,a.wizardStep=1)},a.getDateFormat=function(){var e=angular.copy(a.dateOrder);return e=e.replace(/\//g,a.dateDelimiter)},a.getDateTimeFormat=function(){var e=a.getDateFormat();return e+=" "+a.timeFormat},a.getSampleDate=function(){a.dateOrder||(a.dateOrder="en"===e.locale?"MM/DD/YYYY":"DD/MM/YYYY"),a.dateDelimiter||(a.dateDelimiter="en"===e.locale?"/":"."),a.timeFormat||(a.timeFormat="HH:mm");var t=new Date("2010-01-31 16:20:00"),r=a.getDateTimeFormat();a.sampleDate=moment(t).format(r)},a.prepareFixedValue=function(){var t=angular.copy(a.fixedValue);return angular.forEach(t,function(r,i){var n=o("filter")(a.module.fields,{name:i},!0)[0];switch(n.data_type){case"number":t[i]=parseInt(t[i]);break;case"number_decimal":case"currency":t[i]=parseFloat(t[i]);break;case"picklist":t[i]=null!=t[i]?t[i].label[e.user.tenant_language]:null;break;case"tag":var l=recordValue.split("|");recordValue="{";for(var d=0;d<l.length;d++)recordValue+='"'+l[d]+'",';recordValue&&(recordValue=recordValue.slice(0,-1)+"}");break;case"multiselect":var p="{";angular.forEach(t[i],function(a){p+='"'+a["label_"+e.user.tenant_language]+'",'}),t[i]=p.slice(0,-1)+"}";break;case"lookup":t[i]=t[i].id}}),t},a.prepareRecords=function(){for(var t=[],r=function(t,r,i,n){var l="";switch(t&&(l=t.toString().trim()),a.error={},a.error.rowNo=i,a.error.cellName=n,a.error.cellValue=t,a.error.fieldLabel=r["label_"+e.language],r.data_type){case"number":l=parseInt(l),isNaN(l)&&(a.error.message=o("translate")("Data.Import.Error.InvalidNumber"),l=0);break;case"number_decimal":l=parseFloat(l),isNaN(l)&&(a.error.message=o("translate")("Data.Import.Error.InvalidNumber"),l=0);break;case"currency":l=l.replace(",","."),l=parseFloat(l),isNaN(l)&&(a.error.message=o("translate")("Data.Import.Error.InvalidDecimal"),l=null);break;case"date":var d=_(l);l="Invalid Date"===d.toString()?l.split(" ")[0]:d;var p=a.getDateFormat(),u="DD/MM/YYYY",s=moment(l,p,!0);if(s.isValid())l=s.format();else if(s=moment(l,u,!0),!s.isValid()){var m=a.dateDelimiter,c=l.split(dateDelimiter),f=p.split(dateDelimiter);if(c.length>1){var v=c[f.indexOf("YYYY")],h=c[f.indexOf("MM")],x=c[f.indexOf("DD")],M=Date.parse(h+m+x+dateDelimiter+v);s=moment(new Date(M),p,!0),s.isValid()?l=s.format():(a.error.message=o("translate")("Data.Import.Error.InvalidDate"),l=null)}}break;case"date_time":var d=_(l);l="Invalid Date"===d.toString()?l.split(" ")[0]:d;var F=a.getDateTimeFormat(),y="DD/MM/YYYY "+a.timeFormat,b=moment(l,F,!0);b.isValid()?l=b.toDate():(b=moment(l,y,!0),b.isValid()?l=b.toDate():(a.error.message=o("translate")("Data.Import.Error.InvalidDateTime"),l=null));break;case"time":var V,D=k(r,i-1,n),w=[];D?(w=D.split(":"),V=moment(new Date(null,null,null,w[0],w[1]),a.timeFormat,!0)):(l=_(l),V=moment(l.toUTCString(),a.timeFormat,!0)),V.isValid()?l=V.toDate():(a.error.message=o("translate")("Data.Import.Error.InvalidTime"),l=null);break;case"email":g.test(l)||(a.error.message=o("translate")("Data.Import.Error.InvalidEmail"),l=null);break;case"picklist":var I=o("filter")(a.picklistsModule[r.picklist_id],{labelStr:l},!0)[0];I||(a.error.message=o("translate")("Data.Import.Error.PicklistItemNotFound"),l=null);break;case"multiselect":var S=l.split("|");l="{";for(var $=0;$<S.length;$++){var N=S[$],E=o("filter")(a.picklistsModule[r.picklist_id],{labelStr:N})[0];if(!E){a.error.message=o("translate")("Data.Import.Error.MultiselectItemNotFound",{item:N}),l=null;break}l+='"'+N+'",'}l&&(l=l.slice(0,-1)+"}");break;case"lookup":"relation"===r.lookup_type&&(r.lookup_type=a.fixedValue.related_module.value);for(var L=[],C=0;C<a.lookupIds[r.lookup_type].length;C++){var T=a.lookupIds[r.lookup_type][C],Y=l%1===0;Y&&T.id===parseInt(l)?L.push(T):T.value==l&&L.push(T)}var A=o("filter")(e.modules,{name:r.lookup_type})[0];if("users"===r.lookup_type&&(A={},A.label_tr_singular="Kullanıcı",A.label_en_singular="User"),L.length>1)return a.error.message=o("translate")("Data.Import.Error.LookupMoreThanOne",{module:A["label_"+e.language+"_singular"]}),void(l=null);var P=L[0];if(!P)return a.error.message=o("translate")("Data.Import.Error.LookupNotFound",{module:A["label_"+e.language+"_singular"]}),void(l=null);l=P.id;break;case"checkbox":"yes"===l.toLowerCase()||"evet"===l.toLowerCase()||"true"===l.toLowerCase()?l="true":"no"===l.toLowerCase()||"hayır"===l.toLowerCaseTurkish()||"false"===l.toLowerCase()?l="false":(a.error.message=o("translate")("Data.Import.Error.InvalidCheckbox"),l=null)}if(null!=l&&void 0!=l){if(r.validation||(r.validation={}),!r.validation.max_length)switch(r.data_type){case"text_single":r.validation.max_length=50;break;case"text_multi":r.validation.max_length=500;break;case"number":r.validation.max_length=15;break;case"number_decimal":r.validation.max_length=19;break;case"currency":r.validation.max_length=21;break;case"email":r.validation.max_length=100}if(r.validation.max||"number"!==r.data_type&&"number_decimal"!==r.data_type&&"currency"!==r.data_type||(r.validation.max=Number.MAX_VALUE),r.validation.max_length&&l.toString().length>r.validation.max_length&&(a.error.message=o("translate")("Data.Import.Validation.MaxLength",{maxLength:r.validation.max_length}),l=null),r.validation.min_length&&l.toString().length<r.validation.min_length&&(a.error.message=o("translate")("Data.Import.Validation.MinLength",{minLength:r.validation.min_length}),l=null),r.validation.max&&l>r.validation.max&&(a.error.message=o("translate")("Data.Import.Validation.Max",{max:r.validation.max}),l=null),r.validation.min&&l<r.validation.min&&(a.error.message=o("translate")("Data.Import.Validation.Min",{min:r.validation.min}),l=null),r.validation.pattern){var O=new RegExp(r.validation.pattern);O.test(l)||(a.error.message=o("translate")("Data.Import.Validation.Pattern"),l=null)}}return null!=l&&void 0!=l&&(a.error=null),l},i=a.prepareFixedValue(),n=0;n<a.rows.length;n++){var l={},d=a.rows[n];a.error=null;for(var p in a.fieldMap)if("fixed"!==p&&"fixedFormat"!==p&&a.fieldMap.hasOwnProperty(p)){var u=a.fieldMap[p];if("fixed"===u)l[p]=i[p];else{var s=angular.copy(o("filter")(a.module.fields,{name:p},!0)[0]),m=d[u];if(s.validation&&s.validation.required&&!m){a.error={},a.error.rowNo=n+2,a.error.cellName=u,a.error.cellValue=m,a.error.fieldLabel=s["label_"+e.language],a.error.message=o("translate")("Data.Import.Error.Required");break}if(!m)continue;var c=r(m,s,n+2,u);if(angular.isUndefined(c))break;l[p]=c}}if(a.error)break;t.push(l)}return a.preparing=!1,t},a.getLookupIds=function(){for(var t=n.defer(),r=[],i=0;i<a.rows.length;i++){var l=a.rows[i];for(var d in l)if(l.hasOwnProperty(d)){var p=l[d].toString().trim();for(var u in a.fieldMap)if(a.fieldMap.hasOwnProperty(u)){var s=a.fieldMap[u];if("fixed"!=u&&d===s){var m=angular.copy(o("filter")(a.module.fields,{name:u},!0)[0]);if("lookup"!=m.data_type)break;"relation"===m.lookup_type&&(m.lookup_type=a.fixedValue.related_module.value);var c=o("filter")(r,{type:m.lookup_type},!0)[0];if(!c){if(c={},c.type=m.lookup_type,c.values=[],"users"!=m.lookup_type){var f=o("filter")(e.modules,{name:m.lookup_type},!0)[0];c.field=o("filter")(f.fields,{primary:!0},!0)[0].name}else c.field=p.indexOf("@")>-1?"email":"full_name";r.push(c)}p&&c.values.indexOf(p)<0&&c.values.push(p)}}}}return r.length?h.getLookupIds(r).then(function(e){t.resolve(e.data)})["catch"](function(e){t.reject(e.data)}):t.resolve([]),t.promise},a.submit=function(e){if(a.error=null,a.errorUnique=null,"activities"!==a.module.name||!a.fieldMap.related_to||a.fixedValue&&a.fixedValue.related_module||a.importForm.related_module.$setValidity("required",!1),a.importForm.$valid){if(F)return void(F=!1);e||(a.wizardStep=2),a.preparing=!0,a.trying=!1,c(function(){a.getLookupIds().then(function(e){a.lookupIds=e,a.records=a.prepareRecords()})},200)}},a.tryAgain=function(){a.trying=!0,a.error=null,a.errorUnique=null;var e=b.queue[0]._file;y(e,!0)},a.saveLoaded=function(){a.getLookupIds().then(function(e){a.lookupIds=e,a.records=a.prepareRecords(),a.save()})},a.save=function(){return!a.records&&a.selectedMapping.id?void a.saveLoaded():(a.saving=!0,c(function(){a.saving&&(a.longProcessing=!0)},4e3),void h["import"](a.records,a.module.name).then(function(e){var t=a.module.name+"_"+a.module.name;f.remove(t),a.importResponse=e.data,b.uploadAll(),c(function(){a.saving=!1,a.longProcessing=!1,s.create({content:o("translate")("Data.Import.Success"),className:"success",timeout:6e3}),r.go("app.moduleList",{type:a.type})},500)})["catch"](function(e){409===e.status&&(a.errorUnique={},a.errorUnique.field=e.data.field,e.data.field2&&(a.errorUnique.field=e.data.field2)),a.saving=!1,a.longProcessing=!1}))},a.combinationFilter=function(e){return e.combination?!1:!0}}]);