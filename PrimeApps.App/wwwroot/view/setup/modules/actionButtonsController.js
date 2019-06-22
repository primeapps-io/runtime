"use strict";angular.module("primeapps").controller("ActionButtonsController",["$rootScope","$scope","$filter","$state","$stateParams","ngToast","$modal","helper","$cache","systemRequiredFields","systemReadonlyFields","ModuleSetupService","ModuleService","AppService",function(e,t,o,a,n,r,s,l,i,u,c,d,m){var p=o("filter")(e.modules,{name:n.module},!0)[0];if(!p)return r.create({content:o("translate")("Common.NotFound"),className:"warning"}),void a.go("app.dashboard");t.module=angular.copy(p);var f=function(e){m.getActionButtons(t.module.id,e).then(function(e){t.actionButtons=e,t.actionButtonState=angular.copy(t.actionButtons)})};f(),t.showFormModal=function(a){if(a||(a={},a.type=3,a.triggerType=1,a.isNew=!0),t.actionButtonTypes=[{type:"Modal",value:3},{type:"Script",value:1},{type:"Webhook",value:2}],t.displayPages=[{name:o("translate")("Setup.Modules.Detail"),value:1},{name:"Form",value:2},{name:o("translate")("Setup.Modules.List"),value:4},{name:o("translate")("Setup.Modules.All"),value:3},{name:o("translate")("Setup.Modules.Relation"),value:5}],"Scripting"===a.action_type&&(a.type=1),"Webhook"===a.action_type&&(a.type=2),"ModalFrame"===a.action_type&&(a.type=3),"Detail"===a.trigger&&(a.triggerType=1),"Form"===a.trigger&&(a.triggerType=2),"All"===a.trigger&&(a.triggerType=3),"List"===a.trigger&&(a.triggerType=4),3===a.type){t.hookParameters=[],t.hookModules=[],angular.forEach(t.updatableModules,function(e){t.hookModules.push(e)});var n={};n.parameterName=null,n.selectedModules=t.hookModules,n.selectedField=null,t.hookParameters.push(n)}if(a.method_type&&a.parameters&&2==a.type){t.hookParameters=[];var r=a.parameters.split(",");angular.forEach(r,function(e){var a=e.split("|",3),n={};n.parameterName=a[0],n.selectedModules=angular.copy(t.updatableModules);var r;if(t.module.name===a[1])r=o("filter")(n.selectedModules,{name:a[1]},!0)[0];else{var s=o("filter")(t.module.fields,{name:a[1]},!0)[0].lookup_type;r=o("filter")(n.selectedModules,{name:s},!0)[0]}r&&(n.selectedModule=r,n.selectedField=o("filter")(n.selectedModule.fields,{name:a[2]},!0)[0],t.hookParameters.push(n))})}var l=function(a){t.actionButtonPermission=[],a.isNew&&(a.permissions=[]),angular.forEach(e.profiles,function(e){e.is_persistent&&e.is_persistent&&(e.name=o("translate")("Setup.Profiles.Administrator")),e.is_persistent&&!e.has_admin_rights&&(e.name=o("translate")("Setup.Profiles.Standard")),t.actionButtonPermission.push({profile_id:e.id,profile_name:e.name,type:"full",profile_is_admin:e.has_admin_rights})})};if(!a.isNew&&(l(a),t.actionButtonPermission.length!=a.permissions.length))for(var i=a.permissions.length;i<t.actionButtonPermission.length;i++)a.permissions.push(t.actionButtonPermission[i]);a.isNew?(l(a),a.permissions=t.actionButtonPermission):a.permissions&&a.permissions.length>0?angular.forEach(a.permissions,function(t){var a=o("filter")(e.profiles,{id:t.profile_id},!0)[0];t.profile_name=a.name,t.profile_is_admin=a.has_admin_rights}):l(a),t.currentActionButton=a,t.currentActionButtonState=angular.copy(t.currentActionButton),t.formModal=t.formModal||s({scope:t,templateUrl:"view/setup/modules/actionButtonForm.html",animation:"",backdrop:"static",show:!1}),t.formModal.$promise.then(function(){t.formModal.show()})},t.save=function(e){if(e.$valid){t.saving=!0;var a=angular.copy(t.currentActionButton);if(a.isNew&&delete a.isNew,a.module_id=t.module.id,1!==a.type&&(a.template="template"),a.trigger=a.triggerType,delete a.triggerType,2===a.type){var n=[];angular.forEach(t.hookParameters,function(e){var a;a=t.module.name!=e.selectedModule.name?o("filter")(t.module.fields,{lookup_type:e.selectedModule.name},!0)[0].name:e.selectedModule.name;var r=e.parameterName+"|"+a+"|"+e.selectedField.name;n.push(r)}),n.length>0&&(a.parameters=n.toString())}else a.parameters=null,a.method_type=null,a.url=1===a.type?"#":t.currentActionButton.url;a.css_class||(a.css_class="btn-sm btn-custom");var s=function(){f(!0),r.create({content:o("translate")("Setup.Modules.ActionButtonSaveSuccess"),className:"success"}),t.saving=!1,t.formModal.hide()};a.id?d.updateActionButton(a).then(function(){s()})["catch"](function(){t.actionButtons=t.actionButtonState,t.formModal&&(t.formModal.hide(),t.saving=!1)}):d.createActionButton(a).then(function(e){t.actionButtons||(t.actionButtons=[]),a.action_type=e.data.type,a.trigger=e.data.trigger,a.id=e.data.id,s()})["catch"](function(){t.actionButtons=t.actionButtonState,t.formModal&&(t.formModal.hide(),t.saving=!1)})}},t["delete"]=function(e){delete e.$$hashKey,d.deleteActionButton(e.id).then(function(){f(!0),t.saving=!1,r.create({content:o("translate")("Setup.Modules.ActionButtonDeleteSuccess"),className:"success"})})["catch"](function(){t.actionButtons=t.actionButtonState,t.formModal&&(t.formModal.hide(),t.saving=!1)})},t.cancel=function(){angular.forEach(t.currentActionButton,function(e,o){t.currentActionButton[o]=t.currentActionButtonState[o]}),t.formModal.hide()};var h=function(){t.updatableModules=[],t.updatableModules.push(t.module),angular.forEach(t.module.fields,function(a){if(a.lookup_type&&a.lookup_type!=t.module.name&&"users"!=a.lookup_type&&!a.deleted){var n=o("filter")(e.modules,{name:a.lookup_type},!0)[0];t.updatableModules.push(n)}}),t.hookParameters=[],t.hookModules=[],angular.forEach(t.updatableModules,function(e){t.hookModules.push(e)});var a={};a.parameterName=null,a.selectedModules=t.hookModules,a.selectedField=null,t.hookParameters.push(a)};h(),t.webhookParameterAdd=function(e){var a={};a.parameterName=e.parameterName,a.selectedModules=e.selectedModules,a.selectedField=e.selectedField,a.parameterName&&a.selectedModules&&a.selectedField&&(t.hookParameters.length<=10?t.hookParameters.push(a):r.create({content:o("translate")("Setup.Workflow.MaximumHookWarning"),className:"warning"}));var n=t.hookParameters[t.hookParameters.length-1];n.parameterName=null,n.selectedField=null},t.webhookParameterRemove=function(e){var o=t.hookParameters.indexOf(e);t.hookParameters.splice(o,1)}}]);