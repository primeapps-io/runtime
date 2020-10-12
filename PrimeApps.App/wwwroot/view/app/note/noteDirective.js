"use strict";angular.module("primeapps").directive("noteList",["convert","$localStorage","NoteService","FileUploader","config","$filter","helper","$cookies","$mdDialog","mdToast",function(e,t,a,n,i,o,l,r,s,p){return{restrict:"EA",scope:{moduleId:"=",recordId:"="},templateUrl:"view/app/note/noteList.html",controller:["$rootScope","$scope",function(e,n){n.user=e.user,n.pagingIcon="fa-chevron-right",n.config=e.config,n.$parent.loadingNotes=!0,n.$parent.allNotesLoaded=!1,n.$parent.currentPage=1,n.$parent.limit=10,n.addActivity=n.$parent.addActivity,n.blobUrl=blobUrl;var l,d,c=plupload.guid();n.modules=e.modules,n.newNoteForm=!1,n.newsfeed=!0,n.$parent.$parent.module&&(n.newsfeed=!1);var u={module_id:n.$parent.module.id,record_id:n.recordId,limit:n.$parent.limit,offset:0};a.count(u).then(function(e){a.find(u).then(function(t){n.$parent.notes=t.data,n.$parent.loadingNotes=!1,n.$parent.$parent.notesCount=e.data,e.data<=n.$parent.limit&&(n.$parent.hidePaging=!0)})}),n.loadMore=function(){n.$parent.allNotesLoaded||(u.offset=n.$parent.currentPage*n.$parent.limit,n.pagingIcon="fa-spinner fa-spin",a.find(u).then(function(e){if(e=e.data,n.$parent.notes=n.$parent.notes.concat(e),n.pagingIcon="fa-chevron-right",n.$parent.currentPage=n.$parent.currentPage+1,0===e.length)if(n.user.profile.has_admin_rights)n.$parent.allNotesLoaded=!0;else{if(n.$parent.allNotesLoaded)return;u.offset=n.$parent.currentPage*n.$parent.limit,n.pagingIcon="fa-spinner fa-spin",a.find(u).then(function(e){e=e.data,n.$parent.notes=n.$parent.notes.concat(e),n.pagingIcon="fa-chevron-right",n.$parent.currentPage=n.$parent.currentPage+1,n.$parent.allNotesLoaded=!0})}}))},n.addNote=function(e){if(e&&e.text.trim()){n.noteCreating=!0;var t={};t.text=e.text.trim(),t.text.length>3800?(p.warning(o("translate")("Note.LimitWarn")),n.noteCreating=!1):a.create(t).then(function(e){e=e.data;for(var t=new Date,a=t.getDay(),i=(t.getDate(),t.getMonth()+1,t.getFullYear()),l=/(<([^>]+)>)/gi,r=e.text.replace(l,"").split(" "),s=0;s<r.length;s++){if(moment(r[s],"DD/MM/YYYY",!0).isValid()||moment(r[s],"DD.MM.YYYY",!0).isValid()){var p=moment(1e3*moment(r[s],"DD/MM/YYYY HH:mm").unix());e.text=e.text.replace(r[s],'<a href="" ng-click="addActivity('+p+', \'date\')" data-placement="right" bs-tooltip data-title="Etkinlik eklemek için tıklayınız"/>'+r[s]+"</a>")}var d=r[s].toLowerCase();if(o("filter")(m,{code:d},!0).length>0){var c,u=o("filter")(m,{code:d},!0)[0];if("day"===u.type){var g;if("haftaya"===r[s-1]||"hafta"===r[s-1]||"Hafta"===r[s-1]||"Haftaya"===r[s-1]){u.value>a?c=u.value-a+7:(g=a-u.value,c=7-g);var _=r[s-1]+" "+r[s];e.text=e.text.replace(_,'<a href="" ng-click="addActivity('+c+', \'null\')" data-placement="right" bs-tooltip data-title="Etkinlik eklemek için tıklayınız"/>'+r[s-1]+" "+r[s]+"</a>")}else u.value>a?c=u.value-a:(g=a-u.value,c=7-g),e.text=e.text.replace(r[s],'<a href="" ng-click="addActivity('+c+', \'null\')" data-placement="right" bs-tooltip data-title="Etkinlik eklemek için tıklayınız"/>'+r[s]+"</a>")}else if("dayTime"===u.type)c=u.value,e.text=e.text.replace(r[s],'<a href="" ng-click="addActivity('+c+', \'null\')" data-placement="right" bs-tooltip data-title="Etkinlik eklemek için tıklayınız"/>'+r[s]+"</a>");else if("month"===u.type&&(r[s-1]=parseInt(r[s-1]),r[s-1]===parseInt(r[s-1],10))){var f=r[s-1]+"."+u.value+"."+i;c=moment(1e3*moment(f,"DD/MM/YYYY HH:mm").unix());var h=r[s-1]+" "+r[s];e.text=e.text.replace(h,'<a href="" ng-click="addActivity('+c+', \'month\')" data-placement="top" bs-tooltip data-title="Etkinlik eklemek için tıklayınız"/>'+r[s-1]+" "+r[s]+"</a>")}}}var v=new Date;e.created_at=v.setSeconds(v.getSeconds()-1),n.$parent.notes.unshift(e),n.$parent.$parent.notesCount=n.$parent.notesCount+1,n.noteCreating=!1,n.newNoteForm=!1,n.note=null})["catch"](function(){n.noteCreating=!1})}},n.addComment=function(e){if(e.subNote.text&&e.subNote.text.trim()){e.noteCreating=!0;var t={};t.text=e.subNote.text.trim(),t.record_id=n.recordId,t.note_id=e.id,t.module_id=n.$parent.module.id,t.text.length>3800?(p.warning(o("translate")("Note.LimitWarn")),e.noteCreating=!1):a.create(t).then(function(t){e.notes||(e.notes=[]),e.notes.unshift(t.data),e.noteCreating=!1,e.showForm=!1,n.$parent.allNotesLoaded=!1})}},n.updateNote=function(e){if(e.text&&e.text.trim()){e.noteUpdating=!0;var t={};t.id=e.id,t.text=e.text.trim(),a.update(t).then(function(){e.noteUpdating=!1,e.showFormEdit=!1})}},n.deleteNote=function(e,t,i){var l=s.confirm().title(o("translate")("Common.AreYouSure")).targetEvent(e).ok(o("translate")("Common.Yes")).cancel(o("translate")("Common.No"));s.show(l).then(function(){a["delete"](t.id).then(function(){i?i.notes.splice(i.notes.indexOf(t),1):n.$parent.notes.splice(n.$parent.notes.indexOf(t),1),n.$parent.$parent.notesCount=n.$parent.notesCount-1})},function(){})},n.like=function(e,t){var i={note_id:e.id,user_id:n.user.id};a.like(i).then(function(){n.likeButton=!0;var i=e.id;a.get(i).then(function(e){var a=e.data;if(n.likeButton=!1,"sub"===t)for(var l=o("filter")(n.$parent.notes,{id:a.note_id},!0)[0],r=0;r<l.notes.length;r++){var s=l.notes[r];s.id===i&&(s.likes=a.likes)}else o("filter")(n.$parent.notes,{id:a.id},!0)[0].likes=a.likes})})},n.noteLikesList=function(e,t){n.likes=t;var a=angular.element(document.body);s.show({parent:a,templateUrl:"view/app/note/noteLikes.html",clickOutsideToClose:!1,scope:n,preserveScope:!0})},n.close=function(){s.hide()},n.imgUpload={settings:{multi_selection:!1,url:"storage/upload",headers:{Authorization:"Bearer "+t.read("access_token"),Accept:"application/json","X-Tenant-Id":r.get(preview?"preview_tenant_id":"tenant_id"),"X-App-Id":r.get(preview?"preview_app_id":"app_id")},multipart_params:{container:c,type:"note",upload_id:0,response_list:""},filters:{mime_types:[{title:"Image files",extensions:"jpg,gif,png"}],max_file_size:"2mb"},resize:{quality:90}},events:{filesAdded:function(e){e.start(),tinymce.activeEditor.windowManager.open({title:o("translate")("Common.PleaseWait"),width:50,height:50,body:[{type:"container",name:"container",label:"",html:"<span>"+o("translate")("EMail.UploadingAttachment")+"</span>"}],buttons:[]})},uploadProgress:function(){},fileUploaded:function(e,t,a){e.settings.multipart_params.response_list="",e.settings.multipart_params.upload_id=0,tinymce.activeEditor.windowManager.close();var n=JSON.parse(a.response);l(i.storage_host+n.public_url,{alt:t.name}),l=null},chunkUploaded:function(e,t,a){var n=JSON.parse(a.response);n.upload_id&&(e.settings.multipart_params.upload_id=n.upload_id),e.settings.multipart_params.response_list+=""==e.settings.multipart_params.response_list?n.e_tag:"|"+n.e_tag},error:function(e,t){switch(this.settings.multipart_params.response_list="",this.settings.multipart_params.upload_id=0,t.code){case-600:tinymce.activeEditor.windowManager.alert(o("translate")("EMail.MaxImageSizeExceeded"))}d&&(d(),d=null)}}},n.fileUpload={settings:{multi_selection:!1,unique_names:!1,url:"storage/upload",headers:{Authorization:"Bearer "+t.read("access_token"),Accept:"application/json","X-Tenant-Id":r.get(preview?"preview_tenant_id":"tenant_id"),"X-App-Id":r.get(preview?"preview_app_id":"app_id")},multipart_params:{container:c,type:"note",upload_id:0,response_list:""},filters:{mime_types:[{title:"Email Attachments",extensions:"pdf,doc,docx,xls,xlsx,csv"}],max_file_size:"50mb"}},events:{filesAdded:function(e){e.start(),tinymce.activeEditor.windowManager.open({title:o("translate")("Common.PleaseWait"),width:50,height:50,body:[{type:"container",name:"container",label:"",html:"<span>"+o("translate")("EMail.UploadingAttachment")+"</span>"}],buttons:[]})},uploadProgress:function(){},fileUploaded:function(e,t,a){this.settings.multipart_params.response_list="",this.settings.multipart_params.upload_id=0;var n=JSON.parse(a.response);l(i.storage_host+n.public_url,{alt:t.name}),l=null,tinymce.activeEditor.windowManager.close()},chunkUploaded:function(e,t,a){var n=JSON.parse(a.response);n.upload_id&&(e.settings.multipart_params.upload_id=n.upload_id),e.settings.multipart_params.response_list+=""==e.settings.multipart_params.response_list?n.e_tag:"|"+n.e_tag},error:function(e,t){switch(this.settings.multipart_params.response_list="",this.settings.multipart_params.upload_id=0,t.code){case-600:tinymce.activeEditor.windowManager.alert(o("translate")("EMail.MaxFileSizeExceeded"))}d&&(d(),d=null)}}},n.tinymceTemplate={onChange:function(){},valid_elements:"@[id|class|title|style],a[name|href|target|title|alt|ng-click],#p,blockquote,-ol,-ul,-li,br,img[src|height|width],-sub,-sup,-b,-i,-u,-span,hr",inline:!1,height:125,language:e.language,plugins:["advlist autolink lists link image charmap print preview anchor placeholder","searchreplace visualblocks code fullscreen","insertdatetime table contextmenu paste imagetools wordcount textcolor colorpicker"],toolbar:" link image imagetools ",menubar:"false",placeholder_attrs:{style:{position:"absolute",top:"5px",left:0,color:"lightgrey",padding:"1%",width:"98%",overflow:"hidden","white-space":"pre-wrap"}},skin:"custom",theme:"modern",file_picker_callback:function(e,t,a){if(l=e,"file"==a.filetype){var n=document.getElementById("uploadFile");n.click()}if("image"==a.filetype){var n=document.getElementById("uploadImage");n.click()}},image_advtab:!0,file_browser_callback_types:"image file",paste_data_images:!0,paste_as_text:!0,spellchecker_language:e.language,images_upload_handler:function(e,t,a){var i=e.blob();l=t,d=a,n.imgUpload.uploader.addFile(i)},resize:!1,width:"99,9%",toolbar_items_size:"small",statusbar:!1};var m=[{code:"pazartesi",type:"day",value:1},{code:"salı",type:"day",value:2},{code:"&ccedil;arşamba",type:"day",value:3},{code:"perşembe",type:"day",value:4},{code:"cuma",type:"day",value:5},{code:"cumartesi",type:"day",value:6},{code:"pazar",type:"day",value:7},{code:"bug&uuml;n",type:"dayTime",value:0},{code:"yarın",type:"dayTime",value:1},{code:"ocak",type:"month",value:1},{code:"şubat",type:"month",value:2},{code:"mart",type:"month",value:3},{code:"nisan",type:"month",value:4},{code:"mayıs",type:"month",value:5},{code:"haziran",type:"month",value:6},{code:"temmuz",type:"month",value:7},{code:"ağustos",type:"month",value:8},{code:"eyl&uuml;l",type:"month",value:9},{code:"ekim",type:"month",value:10},{code:"kasım",type:"month",value:11},{code:"aralık",type:"month",value:12}]}]}}]).directive("noteForm",["$filter","$localStorage","FileUploader","config","convert","entityTypes","NoteService","$cookies",function(e,t,a,n,i,o,l,r){return{restrict:"EA",scope:{recordId:"=",show:"="},templateUrl:"view/app/note/noteForm.html",controller:["$scope","$rootScope",function(a,i){a.noteCreating=!1;var o,s,p=plupload.guid();a.create=function(t){if(t&&t.text.trim()){a.noteCreating=!0;var n={};n.text=t.text.trim(),n.record_id=a.recordId,n.module_id=a.$parent.module.id,n.text.length>3800?(mdToast.warning(e("translate")("Note.LimitWarn")),a.noteCreating=!1):l.create(n).then(function(e){e=e.data;var t=new Date;e.created_at=t.setSeconds(t.getSeconds()-1),a.$parent.notes.unshift(e),a.$parent.$parent.notesCount=a.$parent.notesCount+1,a.noteCreating=!1,a.show=!1,a.note=null})["catch"](function(){a.noteCreating=!1})}},a.imgUploadForm={settings:{multi_selection:!1,url:"storage/upload",multipart_params:{container:p,type:"note",upload_id:0,response_list:""},filters:{mime_types:[{title:"Image files",extensions:"jpg,gif,png"}],max_file_size:"2mb"},resize:{quality:90}},events:{filesAdded:function(t){t.start(),tinymce.activeEditor.windowManager.open({title:e("translate")("Common.PleaseWait"),width:50,height:50,body:[{type:"container",name:"container",label:"",html:"<span>"+e("translate")("EMail.UploadingAttachment")+"</span>"}],buttons:[]})},uploadProgress:function(){},fileUploaded:function(e,t,a){e.settings.multipart_params.response_list="",e.settings.multipart_params.upload_id=0,tinymce.activeEditor.windowManager.close();var i=JSON.parse(a.response);o(n.storage_host+i.public_url,{alt:t.name}),o=null},chunkUploaded:function(e,t,a){var n=JSON.parse(a.response);n.upload_id&&(e.settings.multipart_params.upload_id=n.upload_id),e.settings.multipart_params.response_list+=""==e.settings.multipart_params.response_list?n.e_tag:"|"+n.e_tag},error:function(t,a){switch(this.settings.multipart_params.response_list="",this.settings.multipart_params.upload_id=0,a.code){case-600:tinymce.activeEditor.windowManager.alert(e("translate")("EMail.MaxImageSizeExceeded"))}s&&(s(),s=null)}}},a.fileUploadForm={settings:{multi_selection:!1,unique_names:!1,url:"storage/upload",headers:{Authorization:"Bearer "+t.read("access_token"),Accept:"application/json","X-Tenant-Id":r.get(preview?"preview_tenant_id":"tenant_id"),"X-App-Id":r.get(preview?"preview_app_id":"app_id")},multipart_params:{container:p,type:"note",upload_id:0,response_list:""},filters:{mime_types:[{title:"Email Attachments",extensions:"pdf,doc,docx,xls,xlsx,csv"}],max_file_size:"50mb"}},events:{filesAdded:function(t){t.start(),tinymce.activeEditor.windowManager.open({title:e("translate")("Common.PleaseWait"),width:50,height:50,body:[{type:"container",name:"container",label:"",html:"<span>"+e("translate")("EMail.UploadingAttachment")+"</span>"}],buttons:[]})},uploadProgress:function(){},fileUploaded:function(e,t,a){this.settings.multipart_params.response_list="",this.settings.multipart_params.upload_id=0;var n=JSON.parse(a.response);o(n.public_url,{alt:t.name}),o=null,tinymce.activeEditor.windowManager.close()},chunkUploaded:function(e,t,a){var n=JSON.parse(a.response);n.upload_id&&(e.settings.multipart_params.upload_id=n.upload_id),e.settings.multipart_params.response_list+=""==e.settings.multipart_params.response_list?n.e_tag:"|"+n.e_tag},error:function(t,a){switch(this.settings.multipart_params.response_list="",this.settings.multipart_params.upload_id=0,a.code){case-600:tinymce.activeEditor.windowManager.alert(e("translate")("EMail.MaxFileSizeExceeded"))}s&&(s(),s=null)}}},a.tinymceTemplate={onChange:function(){},inline:!1,height:80,language:i.language,plugins:["advlist autolink lists link image charmap print preview anchor placeholder","searchreplace visualblocks code fullscreen","insertdatetime table contextmenu paste imagetools wordcount textcolor colorpicker"],toolbar:" link image imagetools ",menubar:"false",placeholder_attrs:{style:{position:"absolute",top:"5px",left:0,color:"lightgrey",padding:"1%",width:"98%",overflow:"hidden","white-space":"pre-wrap"}},skin:"custom",theme:"modern",file_picker_callback:function(e,t,a){if(o=e,"file"==a.filetype){var n=document.getElementById("uploadFile");n.click()}if("image"==a.filetype){var n=document.getElementById("uploadImage");n.click()}},image_advtab:!0,file_browser_callback_types:"image file",paste_data_images:!0,paste_as_text:!0,spellchecker_language:i.language,images_upload_handler:function(e,t,n){var i=e.blob();o=t,s=n,a.imgUploadForm.uploader.addFile(i)},resize:!1,width:"99,9%",toolbar_items_size:"small",statusbar:!1}}]}}]).directive("compile",["$compile",function(e){return function(t,a,n){t.$watch(function(e){return e.$eval(n.compile)},function(n){a.html(n),e(a.contents())(t)})}}]).directive("onEnter",function(){return function(e,t,a){t.bind("keydown keypress",function(t){13===t.which&&(e.$apply(function(){e.$eval(a.onEnter,{event:t})}),t.preventDefault())})}});