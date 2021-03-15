"use strict";angular.module("primeapps").controller("UserController",["$rootScope","$cookies","AuthService","$scope","$filter","$state","guidEmpty","helper","UserService","AppService","ProfileService","$q","officeHelper","$localStorage","mdToast","$mdDialog",function(e,t,a,s,r,o,i,l,n,d,c,u,f,p,m,g){s.loading=!0,e.globalLoading=!0,d.checkPermission().then(function(o){function i(){var t=[];t.push(c.getAll()),u.all(t).then(function(t){var a=t[0].data;s.profiles=c.getProfiles(a,e.workgroup.tenant_id,!0),e.processLanguages(s.profiles),e.user.profile.has_admin_rights||(s.profiles=r("filter")(s.profiles,{has_admin_rights:!1},!0)),h()})}function f(t){return'<td ng-click="$event.stopPropagation();" class="position-relative"><input ng-click="selectRow($event,dataItem);$event.stopPropagation();" ng-checked="isRowSelected(dataItem.id) || dataItem.selected"  type="checkbox" id="{{dataItem.id}}" class="k-checkbox row-checkbox"><label class="k-checkbox-label" for="{{dataItem.id}}"></label></td><td class="hide-on-m2"><span ng-if="'+t.has_account+'">'+t.first_name+" "+t.last_name+'</span></td><td class="hide-on-m2"><span>'+t.email+'</span></td><td class="hide-on-m2"><span>'+(e.getLanguageValue(t.profile.languages,"name")||"-")+'</span></td><td class="hide-on-m2"><span>'+(e.getLanguageValue(null!=t.role?t.role.languages:null,"label")||"-")+'</span></td><td class="hide-on-m2"><span>'+s.getStatus(t.is_active)+'</span></td><td class="show-on-m2"><div><strong ng-if="'+t.has_account+'">'+t.user_name+'</strong></div><strong class="k-info-colored paddingl5 paddingr5">'+(e.getLanguageValue(t.profile.languages,"name")||"-")+"</strong><div><span>"+t.email+"</span></div></td>"}if(o&&o.data){var p=(JSON.parse(o.data.profile),void 0);o.data.customProfilePermissions&&(p=JSON.parse(o.data.customProfilePermissions))}e.breadcrumblist=[{title:r("translate")("Setup.Nav.Users"),link:"#/app/setup/users"},{title:r("translate")("Setup.Nav.Tabs.Users")}],s.isOfficeConnected=!1,s.officeUserReady=!1,s.officeUsers=null,s.selectedOfficeUser={},s.addUserModel={},s.addUserForm=!0,s.submitting=!1,s.hideSendEmailToUser=!1,s.selectedRows=[],s.selectedUsers=[],s.isAllSelected=!1,s.field=null,s.transferValue=null,s.officeUserChanged=function(e){s.addUserModel.email=e.email,s.addUserModel.phone=e.phone,s.addUserModel.full_name=e.fullName,s.addUserModel.first_bame=e.name,s.addUserModel.last_name=e.surname},s.changeUserIsActive=function(){s.addUserModel.is_active=!s.addUserModel.is_active},s.sendResetPassword=function(){var e=t.get("app_id"),o=t.get("tenant_id");a.forgotPassword(s.addUserModel.email,e,o).then(function(){m.success({content:r("translate")("Setup.Office.SendEmailSuccess"),timeout:5e3})})["catch"](function(){m.error({content:r("translate")("Setup.Office.SendEmailError"),timeout:5e3})})},s.sendOfficeUserPassword=function(){var t="crm";switch(e.user.appId){case 2:t="kobi";break;case 3:t="asistan";break;case 4:t="ik";break;case 5:t="cagri";break;default:t="crm"}s.submitting=!0;var a={};a.full_name=s.addedUser.first_name+" "+s.addedUser.last_name,a.password=s.userPassword,a.email=s.addedUser.email,n.sendPasswordToOfficeUser(a).then(function(){s.closeUserInfoPopover(),m.success({content:r("translate")("Setup.Office.SendEmailSuccess"),timeout:5e3})})["catch"](function(){s.closeUserInfoPopover(),m.error({content:r("translate")("Setup.Office.SendEmailError"),timeout:5e3})})},s.closeUserInfoPopover=function(){s.submitting=!1,s.addUserForm=!0,s.userPassword=null,s.hideSendEmailToUser=!1,e.closeSide("sideModal"),s.addedUser={},s.grid.dataSource.read()},i(),s.showCreateForm=function(){s.addedUser={},s.addUserModel={},s.addUserForm=!0,s.addNewUser=!0,s.addUserModel.is_active=!0,s.showSideModal()},s.showOfficeUserCreateForm=function(){s.addedUser={},s.addUserForm=!0,s.createOfficePopover=s.createOfficePopover||$popover(angular.element(document.getElementById("officeCreateButton")),{templateUrl:"view/setup/users/officeUserCreate.html",placement:"bottom-right",scope:s,autoClose:!0,show:!0})},s.addUser=function(t){if(!(t&&t.email&&t.profile&&t.role&&t.first_name&&t.last_name))return void m.error(r("translate")("Module.RequiredError"));s.loadingModal=!0;var a={};return t.id?(s.edit(t),void(t.phone&&s.editModelState.phone&&t.phone.toString()!==s.editModelState.phone.toString()&&n.updateUserPhone(t).then(function(){n.getAllUser().then(function(t){e.users=t.data})}))):(s.userInviting=!0,a.firstName=t.first_name,a.LastName=t.last_name,a.email=t.email,a.profileId=t.profile.id,a.phone=t.phone,s.addedUser=angular.copy(t),a=l.SnakeToCamel(a),void n.addUser(a).then(function(t){t.data&&(i(),s.userInviting=!1,m.success(r("translate")("Setup.Users.NewUserSuccess")),s.grid.dataSource.read(),s.loadingModal=!1,s.userPassword=t.data.password,s.hideSendEmailToUser=t.data.password.contains("***"),s.addUserForm=!1,s.addUserModel={}),n.getAllUser().then(function(t){e.users=t.data})})["catch"](function(e){s.userInviting=!1,s.loadingModal=!1,409===e.status&&m.warning(r("translate")("Setup.Users.NewUserError"))}))},s.numberOptions={format:"#",decimals:0,spinners:!1},s.showEditForm=function(e){s.addNewUser=!1,s.addUserForm=!0,s.loadingModal=!0,s.selectedUser=angular.copy(e),s.addUserModel=e,s.addUserModel.pictureData=e.picture?blobUrl+"/"+angular.copy(e.picture):null,s.editModel={},s.editModel.profile=e.profile.id,s.editModel.phone=e.phone,s.editModel.activeDirectoryEmail=e.activeDirectoryEmail,s.userHaveActiveDirectoryEmail=null!==e.activeDirectoryEmail&&"null"!==e.activeDirectoryEmail&&""!==e.activeDirectoryEmail,s.editModelState=angular.copy(s.editModel),s.showSideModal()},s.edit=function(t){if(s.loadingModal=!0,s.editModel=t,s.editModel.profile===s.editModelState.profile&&s.editModel.role===s.editModelState.role&&s.editModel.activeDirectoryEmail===s.editModelState.activeDirectoryEmail)return void(s.loadingModal=!1);n.updateUserStatus({email:s.editModel.email,is_active:s.addUserModel.is_active}),c.changeUserProfile(s.selectedUser.id,e.workgroup.tenant_id,s.editModel.profile.id).then(function(){})["catch"](function(){s.loadingModal=!1,s.userEditing=!1})},s.dismiss=function(t){if(t){var a=r("filter")(s.users,{id:t},!0)[0];n.dismiss(a,e.workgroup.tenant_id).then(function(){s.closeUserInfoPopover(),m.success(r("translate")("Setup.Users.DismissSuccess")),d.getMyAccount(!0)})["catch"](function(){})}},s.showConfirm=function(e,t){var a=g.confirm(r("translate")("Setup.Users.UserDeleteMessage")).title(r("translate")("Common.AreYouSure")).targetEvent(t).ok(r("translate")("Common.Yes")).cancel(r("translate")("Common.No"));g.show(a).then(function(){s.dismiss(e)},function(){})},s.showSideModal=function(){e.sideLoad=!1,e.buildToggler("sideModal","view/setup/users/userSideModal.html"),s.loadingModal=!1},s.copySuccess=function(){m.success(r("translate")("Setup.Users.PasswordCopySuccess"))},s.showBulkUpdate=function(e){var t=angular.element(document.body);g.show({parent:t,templateUrl:"view/setup/users/userBulkUpdateModal.html",clickOutsideToClose:!1,targetEvent:e,scope:s,preserveScope:!0})},s.updateSelected=function(t){return t.$submitted=!0,!t.$invalid&&s.field&&s.transferValue?void(!s.field||s.selectedRows.length<1||!s.transferValue||("profile"===s.field.value&&c.changeUsersProfile(s.selectedRows,e.workgroup.tenant_id,s.transferValue.id).then(function(){s.closeLightBox(),i(),s.grid.dataSource.read(),m.success(r("translate")("Setup.Users.EditsSuccess"))}),"role"===s.field.value&&RoleService.roleChangeUsers(s.selectedRows,s.transferValue.id).then(function(){s.closeLightBox(),i(),s.grid.dataSource.read(),m.success(r("translate")("Setup.Users.EditsSuccess"))}))):void m.error(r("translate")("Module.RequiredError"))},s.deleteSelecteds=function(e){if(!s.selectedRows||!s.selectedRows.length)return void m.warning(r("translate")("Module.NoRecordSelected"));var t=g.confirm().title(r("translate")("Common.AreYouSure")).textContent(r("translate")("Setup.Users.SelectedUsersCount")+s.selectedRows.length).targetEvent(e).ok(r("translate")("Common.Yes")).cancel(r("translate")("Common.No"));g.show(t).then(function(){n.dismissUsers(s.selectedRows).then(function(){i(),s.grid.dataSource.read(),s.closeLightBox(),m.success(r("translate")("Setup.Users.DismissesSuccess"))})},function(){s.status="You decided to keep your debt."})},s.closeLightBox=function(){g.hide(),s.field=null,s.isAllSelected=!1,s.transferValue=null,s.selectedRows=[]},s.selectAll=function(e,t){if(s.selectedRows=[],s.isAllSelected){s.isAllSelected=!1;for(var a=0;a<t.length;a++)t[a].selected&&(t[a].selected=!1)}else{s.isAllSelected=!0;for(var a=0;a<t.length;a++)t[a].selected=!0,s.selectedRows.push(t[a].id)}},s.selectRow=function(e,t){return e.target.checked?(t.selected=!0,void s.selectedRows.push(t.id)):(t.selected=!1,s.selectedRows=s.selectedRows.filter(function(e){return e!==t.id}),void(s.isAllSelected=!1))},s.isRowSelected=function(e){return s.selectedRows.filter(function(t){return t===e}).length>0},s.goUrl2=function(e){var t=window.getSelection();0===t.toString().length&&s.showEditForm(angular.copy(e))};var h=function(){s.usersGridOptions={dataSource:new kendo.data.DataSource({type:"odata-v4",page:1,pageSize:10,serverPaging:!0,serverFiltering:!0,serverSorting:!0,transport:{read:{url:"/api/user/find_users",type:"GET",dataType:"json",beforeSend:e.beforeSend()}},requestEnd:function(t){s.loading=!1,e.globalLoading=!1,e.isMobile()||$(".k-pager-wrap").removeClass("k-pager-sm");var a=t.response;if(a&&a.count>0)for(var r=0;r<a.count;r++)e.processLanguage(a.items[r])},schema:{data:"items",total:"count",model:{id:"id",fields:{email:{type:"string"},user_name:{type:"string"},first_name:{type:"string"},last_name:{type:"string"}}}}}),scrollable:!1,persistSelection:!0,sortable:!0,noRecords:!0,pageable:{refresh:!0,pageSize:10,pageSizes:[10,25,50,100],buttonCount:5,info:!0},filterable:!0,filter:function(e){if(e.filter&&e.field!=="Role.Label"+s.language&&"Profile.Name"!==e.field)for(var t=0;t<e.filter.filters.length;t++)e.filter.filters[t].ignoreCase=!0},rowTemplate:function(e){return'<tr ng-click="goUrl2(dataItem)">'+f(e)+"</tr>"},altRowTemplate:function(e){return'<tr class="k-alt" ng-click="goUrl2(dataItem)">'+f(e)+"</tr>"},columns:[{width:"40px",headerTemplate:"<input type='checkbox' ng-if='grid.dataSource.data().length>0' ng-checked='isAllSelected' ng-click='selectAll($event, grid.dataSource.data())' id='header-chb' class='k-checkbox header-checkbox'><label class='k-checkbox-label' for='header-chb'></label>"},{field:"UserName",title:r("translate")("Setup.Users.UserFullName"),media:"(min-width: 575px)"},{field:"Email",title:r("translate")("Setup.Users.UserEmail"),media:"(min-width: 575px)"},{field:"Profile.Name",title:r("translate")("Setup.Users.Profile"),media:"(min-width: 575px)"},{field:"Role.Label"+s.language,title:r("translate")("Setup.Users.Role"),media:"(min-width: 575px)"},{field:"",title:r("translate")("Setup.Users.UserStatus"),filterable:!1,media:"(min-width: 575px)"},{title:r("translate")("Setup.Nav.Tabs.Users"),media:"(max-width: 575px)"}]}};s.fieldOptions={dataSource:[{label:r("translate")("Setup.Users.Profile"),value:"profile"},{label:r("translate")("Setup.Users.Role"),value:"role"}],dataTextField:"label",dataValueField:"value"},s.profileOptions={dataSource:{transport:{read:function(t){t.success(r("orderBy")(s.profiles,"languages."+e.globalization.Label+".name"))}}},filter:"contains",dataTextField:"languages."+e.globalization.Label+".name",dataValueField:"id"},s.getStatus=function(e){return r("translate")(e?"Common.Active":"Common.Deactivated")}})}]);