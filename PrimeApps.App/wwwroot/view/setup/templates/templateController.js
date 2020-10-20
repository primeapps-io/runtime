"use strict";angular.module("primeapps").controller("TemplateController",["$rootScope","$scope","$filter","$state","helper","$localStorage","config","TemplateService","FileUploader","mdToast","$mdDialog","AppService","$window","$timeout",function(e,t,a,l,n,i,r,o,s,d,u,c,m,p){t.loading=!0,c.checkPermission().then(function(c){function f(){var e='<td class="hide-on-m2"><span>{{dataItem.name}}</span></td>';e+="<td class=\"hide-on-m2\"><span>{{ modulus[dataItem.module]['label_'+ language+ '_plural']}}</span></td>",e+='<td class="hide-on-m2"><span>{{'+E+" }}</span></td>",e+='<td class="show-on-m2">';var t="<div>"+a("translate")("Setup.Templates.TemplateName")+": <strong>{{dataItem.name}}</strong></div>";return t+="<div>"+a("translate")("Common.Module")+": <strong>{{ modulus[dataItem.module]['label_'+ language+ '_plural']}}</strong></div>",t+="<div>"+a("translate")("Setup.Templates.Status")+": <strong>{{"+E+" }}</strong></div>",e+=t+"</td>",e+='<td ng-click="$event.stopPropagation();"><span><md-button class="md-icon-button" aria-label=" " ng-click="delete($event,dataItem)"><i class="fas fa-trash"></i> </md-button></span></td>'}if(c&&c.data){var g=JSON.parse(c.data.profile),h=void 0;if(c.data.customProfilePermissions&&(h=JSON.parse(c.data.customProfilePermissions)),t.hasEmailPermission=t.hasSMSPermission=t.hasExcelPermission=t.hasDocumentPermission=!0,!g.HasAdminRights){var v=void 0;h&&(v=h.permissions.indexOf("profiles")>-1),v||(g.SendEmail||g.SendSMS||g.ExportData||g.WordPdfDownload?(t.hasEmailPermission=g.SendEmail,t.hasSMSPermission=g.SendSMS,t.hasExcelPermission=g.ExportData,t.hasDocumentPermission=g.WordPdfDownload):(d.error(a("translate")("Common.Forbidden")),l.go("app.dashboard")))}}e.breadcrumblist=[{title:a("translate")("Layout.Menu.Dashboard"),link:"#/app/dashboard"},{title:a("translate")("Layout.Menu.Templates"),link:"#/app/setup/templates"},{title:null}],t.templateActiveTab=g.HasAdminRights||t.hasEmailPermission?l.params.tab?l.params.tab:"email":t.hasDocumentPermission?l.params.tab?l.params.tab:"document":t.hasExcelPermission?l.params.tab?l.params.tab:"excel":l.params.tab?l.params.tab:"sms";var b={name:"docFilter",fn:function(e){var t=n.getFileExtension(e.name);return"txt"===t||"docx"===t||"pdf"===t||"doc"===t}},_={name:"excelFilter",fn:function(e){var t=n.getFileExtension(e.name);return"xls"===t||"xlsx"===t}};t.goUrl=function(n){e.breadcrumblist[2].title=a("translate")("Setup.Nav.Tabs."+n.charAt(0).toUpperCase()+n.substr(1).toLowerCase()+"Template"),t.gridOptions&&t.grid&&t.grid.dataSource.filter(t.filters[n]),l.go("app.setup.templates",{tab:n},{notify:!1})},t.changeTemplateActiveTab=function(e){t.templateActiveTab=e;var a=S.filters.indexOf(b),l=S.filters.indexOf(_);"document"===e?-1===a&&(S.filters[S.filters.length-1]=b,l=S.filters.indexOf(_),l>-1&&delete S.filters[l]):"excel"===e&&-1===l&&(S.filters[S.filters.length-1]=_,a>-1&&delete S.filters[a]),t.fileUpload=S},t.currentChangeModule=function(){t.module=a("filter")(e.modules,{name:t.current.module},!0)[0],t.moduleFields=o.getFields(t.module),t.moduleRequired()},t.searchTags=function(e){var a=[];if(t.moduleFields&&t.moduleFields.length>0){for(var l=0;l<t.moduleFields.length;l++){var n=t.moduleFields[l];if("seperator"===n.name)return;n.name&&n.name.match("seperator")&&(n.name=n.label),n.name&&n.name.indexOf(e)>=0&&a.push(n)}return t.tags=a,a}},t.getTagTextRaw=function(e){return p(function(){t.$broadcast("$tinymce:refreshContent")},50),e.name.indexOf("seperator")<0?'<i style="color:#0f1015;font-style:normal">{'+e.name+"}</i>":void 0};var y={Authorization:"Bearer "+window.localStorage.getItem("access_token"),Accept:"application/json","x-app-id":e.user.app_id};t.download=function(e){m.open("/storage/download_template?fileId="+e.id+"&tempType="+e.template_type,"_blank")};var S=t.fileUpload=new s({url:"storage/upload_template",chunk_size:"256kb",queueLimit:1});S.onAfterAddingFile=function(e){var t=new FileReader;t.readAsDataURL(e._file)},S.onWhenAddingFileFailed=function(e,t){switch(t.name){case"docFilter":d.warning(a("translate")("Setup.Settings.DocumentTypeError"));break;case"excelFilter":d.warning(a("translate")("Data.Import.FormatError"));break;case"sizeFilter":d.warning(a("translate")("Setup.Settings.SizeError"))}},S.filters.push({name:"sizeFilter",fn:function(e){return e.size<10485760}}),S.onAfterAddingFile=function(e){t.current.content=e._file.name,t.requiredColor=void 0},t.remove=function(){S.queue[0]&&S.queue[0].remove(),t.current.content=void 0,t.templateFileCleared=!0},t.showSideModal=function(){S.queue[0]&&S.queue[0].remove(),e.sideLoad=!1,e.buildToggler("sideModal","view/setup/templates/templateForm.html"),t.loadingModal=!1,t.templateFileCleared=!1,t.saving=!1},t.openFormModal=function(e){"new"===e&&(t.current={active:!0}),t.showSideModal()},t.moduleRequired=function(){var e=document.getElementById("module"),t=document.getElementsByClassName("k-dropdown")[1];t.className=null===e.value||void 0===e.value||"? undefined:undefined ?"===e.value||""===e.value?"k-widget k-dropdown form-control ng-pristine ng-empty ng-invalid ng-invalid-required k-valid ng-touched":"k-widget k-dropdown form-control ng-pristine ng-untouched ng-empty ng-invalid ng-invalid-required"},t.save=function(e){t.saving=!0;var l=!0;if(t.moduleRequired(),"document"===t.templateActiveTab||"excel"===t.templateActiveTab){var n=document.getElementById("fileUploadReq");n&&(l=!1,n.style="color:#ff000075")}if("email"===t.templateActiveTab&&""===e.tinymceModel.$viewValue&&d.error(a("translate")("Template.ContentRequired")),!e.$valid||!l)return t.saving=!1,!1;if("profile"===t.current.sharing_type){for(var i=[],r=0;r<t.current.profile.length;r++)i.push(t.current.profile[r].id);t.current.profiles=i}else t.current.profiles=null;if("custom"===t.current.sharing_type){for(var s=[],r=0;r<t.current.shares.length;r++)s.push(t.current.shares[r].id);t.current.shares=s}else t.current.shares=[];if(t.current.id)o.update(t.current).then(function(){t.grid.dataSource.read(),d.success(a("translate")("Template.SuccessMessage")),t.closeSide("sideModal")});else{switch(t.templateActiveTab){case"email":t.current.template_type="email";break;case"document":t.current.template_type="module",t.current.subject="Document";break;case"excel":t.current.template_type="excel",t.current.subject="Excel";break;case"sms":t.current.template_type="email",t.current.subject="SMS"}"document"===t.templateActiveTab||"excel"===t.templateActiveTab?(S.queue[0].uploader.headers=y,S.queue[0].headers=y,S.queue[0].upload(),S.onCompleteItem=function(e,l){t.current.content=l.unique_name,t.current.chunks=l.chunks,t.current.content=l.unique_name,t.current.subject="Word",o.create(t.current).then(function(){t.grid.dataSource.read(),d.success(a("translate")("Template.SuccessMessage")),t.closeSide("sideModal")})}):o.create(t.current).then(function(){d.success(a("translate")("Template.SuccessMessage")),t.grid.dataSource.read(),t.closeSide("sideModal")})}},t.current={},t.iframeElement={},t.changeModule=function(){},t.moduleOptions={dataSource:a("filter")(t.modules,function(e){return"roles"!==e.name&&"users"!==e.name&&"profiles"!==e.name}),dataTextField:"label_"+t.language+"_plural",dataValueField:"name"};for(var x=[],M=0;M<t.profiles.length;M++)x.push({profile_id:t.profiles[M].id,name:t.profiles[M]["name_"+t.language]});t.profileOptions={dataSource:x,dataTextField:"name",dataValueField:"profile_id"},t.clickRow=function(e){var a=window.getSelection();0===a.toString().length&&(t.current=angular.copy(e),"profile"===t.current.sharing_type&&(t.current.profile=t.getProfilisByIds(t.current.profile_list)),"custom"===t.current.sharing_type&&(t.current.shares=t.getUsersByIds(t.current.shares)),t.openFormModal())};var T,w,k=plupload.guid();t.imgUpload={settings:{multi_selection:!1,url:r.apiUrl+"document/upload_attachment",headers:{Authorization:"Bearer "+i.read("access_token"),Accept:"application/json","X-App-Id":applicationId,"X-Tenant-Id":tenantId},multipart_params:{container:k},filters:{mime_types:[{title:"Image files",extensions:"jpg,gif,png"}],max_file_size:"2mb"},resize:{quality:90}},events:{filesAdded:function(e){e.start(),tinymce.activeEditor.windowManager.open({title:a("translate")("Common.PleaseWait"),width:50,height:50,body:[{type:"container",name:"container",label:"",html:"<span>"+a("translate")("EMail.UploadingAttachment")+"</span>"}],buttons:[]})},uploadProgress:function(){},fileUploaded:function(e,t,a){tinymce.activeEditor.windowManager.close();var l=JSON.parse(a.response);T(blobUrl+"/"+l.public_url,{alt:t.name}),T=null},error:function(e,t){switch(t.code){case-600:tinymce.activeEditor.windowManager.alert(a("translate")("EMail.MaxImageSizeExceeded"))}w&&(w(),w=null)}}},t.fileUpload={settings:{multi_selection:!1,unique_names:!1,url:r.apiUrl+"document/upload_attachment",headers:{Authorization:"Bearer "+i.read("access_token"),Accept:"application/json"},multipart_params:{container:k},filters:{mime_types:[{title:"Email Attachments",extensions:"pdf,doc,docx,xls,xlsx,csv"}],max_file_size:"50mb"}},events:{filesAdded:function(e){e.start(),tinymce.activeEditor.windowManager.open({title:a("translate")("Common.PleaseWait"),width:50,height:50,body:[{type:"container",name:"container",label:"",html:"<span>"+a("translate")("EMail.UploadingAttachment")+"</span>"}],buttons:[]})},uploadProgress:function(){},fileUploaded:function(e,t,a){var l=JSON.parse(a.response);T(l.PublicURL,{alt:t.name}),T=null,tinymce.activeEditor.windowManager.close()},error:function(e,t){switch(t.code){case-600:tinymce.activeEditor.windowManager.alert(a("translate")("EMail.MaxFileSizeExceeded"))}w&&(w(),w=null)}}},t.tinymceOptions=function(l){t[l]={setup:function(e){e.addButton("addParameter",{type:"button",text:a("translate")("EMail.AddParameter"),onclick:function(){tinymce.activeEditor.execCommand("mceInsertContent",!1,"#")}}),e.on("init",function(){t.loadingModal=!1})},onChange:function(){},inline:!1,height:200,language:e.language,plugins:["advlist autolink lists link image charmap print preview anchor table","searchreplace visualblocks code fullscreen","insertdatetime table contextmenu paste imagetools wordcount textcolor colorpicker"],toolbar:"addParameter | addQuoteTemplate | styleselect | bold italic underline | forecolor backcolor | alignleft aligncenter alignright alignjustify | link image imagetools | table bullist numlist  blockquote code fullscreen",menubar:"false",templates:[{title:"Test template 1",content:"Test 1"},{title:"Test template 2",content:"Test 2"}],skin:"lightgray",theme:"modern",file_picker_callback:function(e,t,a){if(T=e,"file"===a.filetype){var l=document.getElementById("uploadFile");l.click()}if("image"===a.filetype){var l=document.getElementById("uploadImage");l.click()}},image_advtab:!0,file_browser_callback_types:"image file",paste_data_images:!0,paste_as_text:!0,spellchecker_language:e.language,images_upload_handler:function(e,a,l){var n=e.blob();T=a,w=l,t.imgUpload.uploader.addFile(n)},init_instance_callback:function(e){t.iframeElement[l]=e.iframeElement},resize:!1,width:"99,9%",toolbar_items_size:"small",statusbar:!1,convert_urls:!1,remove_script_host:!1}},t.tinymceOptions("tinymceTemplate"),t.tinymceOptions("tinymceTemplateEdit"),t.filters={email:[{field:"Subject",operator:"ne",value:"SMS"},{logic:"or",filters:[{field:"TemplateType",operator:"eq",value:"Sms"},{field:"TemplateType",operator:"eq",value:"System"}]}],document:[{field:"TemplateType",operator:"eq",value:"Module"}],excel:[{field:"TemplateType",operator:"eq",value:"Excel"}],sms:[{field:"TemplateType",operator:"eq",value:"Sms"},{field:"Subject",operator:"eq",value:"SMS"}]};var E="dataItem.active ? ('Setup.Modules.Active' | translate) : ('Setup.Modules.Passive' | translate)";t["delete"]=function(e,l){var n=u.confirm().title(a("translate")("Common.AreYouSure")).targetEvent(e).ok(a("translate")("Common.Yes")).cancel(a("translate")("Common.No"));u.show(n).then(function(){o["delete"](l.id).then(function(){t.grid.dataSource.read(),d.success(a("translate")("Template.SuccessDelete"))})},function(){t.status="You decided to keep your debt."})};var F=function(){var l=new kendo.data.DataSource({type:"odata-v4",page:1,pageSize:10,serverPaging:!0,serverFiltering:!0,serverSorting:!0,transport:{read:{url:"/api/template/get_all_template_list",type:"GET",dataType:"json",beforeSend:e.beforeSend()}},schema:{data:"items",total:"count",model:{id:"id",fields:{name:{type:"string"},module:{type:"string"},active:{type:"boolean"}}}},filter:t.filters[t.templateActiveTab]});t.gridOptions={dataSource:l,scrollable:!1,persistSelection:!0,sortable:!0,noRecords:!0,pageable:{refresh:!0,pageSize:10,pageSizes:[10,25,50,100],buttonCount:5,info:!0},filterable:!0,rowTemplate:function(e){return'<tr ng-click="clickRow(dataItem)">'+f(e)+"</tr>"},altRowTemplate:function(e){return'<tr ng-click="clickRow(dataItem)" class="k-alt">'+f(e)+"</tr>"},columns:[{field:"name",title:a("translate")("Setup.Templates.TemplateName"),media:"(min-width: 575px)"},{field:"module",title:a("translate")("Common.Module"),media:"(min-width: 575px)"},{field:"active",title:a("translate")("Setup.Templates.Status"),media:"(min-width: 575px)",values:[{value:"true",text:a("translate")("Setup.Modules.Active")},{value:"false",text:a("translate")("Setup.Modules.Passive")}]},{title:"Items",media:"(max-width: 575px)"},{field:"",title:"",filterable:!1,width:"40px"}]},l.fetch(function(){t.loading=!1,e.isMobile()||$(".k-pager-wrap").removeClass("k-pager-sm")})};angular.element(document).ready(function(){F()})}),t.sharesOptions={dataSource:t.users,filter:"contains",dataTextField:"full_name",dataValueField:"id",optionLabel:a("translate")("Common.Select")},t.profilesOptions={dataSource:e.profiles,filter:"contains",dataTextField:"name_"+e.language,dataValueField:"id",optionLabel:a("translate")("Common.Select")},t.getProfilisByIds=function(t){for(var l=[],n=0;n<t.length;n++){var i=a("filter")(e.profiles,{id:parseInt(t[n])},!0);i&&l.push(i[0])}return l},t.getUsersByIds=function(t){for(var l=[],n=0;n<t.length;n++){var i=a("filter")(e.users,{id:parseInt(t[n].user_id)},!0);i&&l.push(i[0])}return l},t.showTemplateGuideModal=function(){e.sideLoad=!1,t.selectedSubModule=null,t.selectedModule=null,t.tempalteFieldName="/"+a("translate")("Setup.Templates.TemplateFieldName"),e.buildToggler("sideModal","view/setup/templates/wordTemplateGuide.html")},t.moduleChanged=function(a){t.selectedModule=e.modulus[a],t.lookupModules=f(t.selectedModule),t.getModuleRelations(t.selectedModule),t.selectedSubModule=null},t.subModuleOptions={dataTextField:"label_"+t.language+"_plural",dataValueField:"name"},t.subModuleChanged=function(e){t.selectedSubModule=a("filter")(t.selectedModule.relatedModules,{name:e},!0)[0]};var f=function(t){if(t){for(var a=[],l=0;l<t.fields.length;l++)if("lookup"===t.fields[l].data_type)for(var n=0;n<e.modules.length;n++)if(t.fields[l].lookup_type===e.modules[n].name){var i=angular.copy(e.modules[n]);i.parent_field=t.fields[l],a.push(i);break}a.length&&(t.lookupModules=a)}};t.getModuleRelations=function(a){a&&(a.relatedModules=[],angular.forEach(a.relations,function(t){var l=e.modulus[t.related_module];!t.deleted&&l&&0!==l.order&&(l=angular.copy(l),"many_to_many"===t.relation_type?angular.forEach(l.fields,function(e){e.name=t.related_module+"_id."+e.name}):f(l),a.relatedModules.push(l))}),t.guideLoading=!1,g(a))};var g=function(e){var t={};t.type="custom",t.name="notes",t.label_tr_singular="Not",t.label_tr_plural="Notlar",t.label_en_singular="Note",t.label_en_plural="Notes",t.order=9999,t.fields=[],t.fields.push({id:1,name:"text",label_tr:"Not",label_en:"Note"}),t.fields.push({id:2,name:"first_name",label_tr:"Oluşturan - Adı",label_en:"First Name"}),t.fields.push({id:3,name:"last_name",label_tr:"Oluşturan - Soyadı",label_en:"Last Name"}),t.fields.push({id:4,name:"full_name",label_tr:"Oluşturan - Adı Soyadı",label_en:"Full Name"}),t.fields.push({id:5,name:"email",label_tr:"Oluşturan - Eposta",label_en:"Email"}),t.fields.push({id:6,name:"created_at",label_tr:"Oluşturulma Tarihi",label_en:"Created at"}),e.relatedModules.push(t)};t.copyToClipboard=function(e){var t=document.createElement("textarea");t.value=e,t.setAttribute("readonly",""),t.style.position="absolute",t.style.left="-9999px",document.body.appendChild(t),t.select(),document.execCommand("copy"),document.body.removeChild(t),d.success("Copied: "+e)},t.getRelatedFieldName=function(e,t){return t.parent_field.name+"."+(e.multiline_type_use_html?"html__":"")+e.name},t.getDownloadUrlExcel=function(){m.open("/attach/export_excel?module="+t.selectedModule.name+"&locale="+e.locale,"_blank"),d.success(a("translate")("Module.ExcelDesktop"))}}]);