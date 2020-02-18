"use strict";angular.module("primeapps").controller("RoleController",["$rootScope","$scope","$filter","ngToast","guidEmpty","$modal","RoleService","AppService","$state",function(e,t,r,o,l,n,a,s,i){return t.loading=!0,e.branchAvailable&&!e.user.profile.has_admin_rights?(o.create({content:r("translate")("Common.Forbidden"),className:"warning"}),void i.go("app.dashboard")):(a.getAll().then(function(e){t.roles=e.data,t.tree=t.rolesToTree(e.data),t.loading=!1}),t.toggle=function(e){e.toggle()},t.rolesToTree=function(e){var o=r("filter")(e,{master:!0})[0],l=t.getItem(o),n=t.getChildren(e,l),a=[],s=t.traverseTree(e,n,l);return a.push(s),a},t.traverseTree=function(e,r,o){return angular.forEach(r,function(r){var l=t.getItem(r),n=t.getChildren(e,l);if(0===n.length)t.addChild(o,l);else{var a=t.traverseTree(e,n,l);t.addChild(o,a)}}),o},t.getChildren=function(e,t){var o=r("filter")(e,{reports_to:t.id},!0);return o},t.addChild=function(e,t){return e.nodes.push(t)},t.getItem=function(e){var r={id:e.id,title:e["label_"+t.language],system_type:e.system_type,nodes:[]};return r},t.showDeleteForm=function(e){t.selectedRoleId=e,t.transferRoles=r("filter")(t.roles,{id:"!"+e}),t.transferRoles=r("filter")(t.transferRoles,{reports_to:"!"+e}),t.deleteModal=t.deleteModal||n({scope:t,templateUrl:"view/setup/roles/roleDelete.html",animation:"",backdrop:"static",show:!1}),t.deleteModal.$promise.then(function(){t.deleteModal.show()})},void(t["delete"]=function(e){e||(e=t.transferRoles[0].id),t.roleDeleting=!0,a["delete"](t.selectedRoleId,e).then(function(){a.getAll().then(function(e){t.roles=e.data,t.tree=t.rolesToTree(e.data),t.roleDeleting=!1,o.create({content:r("translate")("Setup.Roles.DeleteSuccess"),className:"success"}),s.getMyAccount(!0),t.deleteModal.hide()})})["catch"](function(){t.roleDeleting=!1})}))}]);