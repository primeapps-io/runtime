"use strict";angular.module("primeapps").directive("taskList",["$filter","entityTypes","helper","operations","TaskService","ModuleService","activityTypes","$cache",function(t,e,s,i,a,r,l,n){return{restrict:"EA",scope:{hideUser:"@",entityId:"=",entityType:"=",isAll:"=",showAll:"=",isLimitless:"="},templateUrl:cdnUrl+"view/app/tasks/taskList.html",controller:["$rootScope","$scope","$filter","$location","$state",function(t,o,c){o.editedTask=null,o.taskState={},o.taskUpdating=!1,o.hasPermission=s.hasPermission,o.entityTypes=e,o.operations=i,o.module=c("filter")(o.$root.modules,{name:"activities"},!0)[0],o.views=a.getViews(),o.view=o.views[0],o.filter={},o.loading=!0,o.pagingIcon="fa-chevron-right",o.currentPage=1;var u=angular.copy(t.users);o.users=c("filter")(u,{is_active:"true"}),o.users=c("orderBy")(o.users,"full_name");var d={id:0,full_name:c("translate")("Tasks.AllUsers")};o.users.unshift(d),o.filter.assignedTo=d;var f={};f.limit=20,f.offset=0;var k=n.get("assignTo");k&&(o.filter.assignedTo=k),o.getTasks=function(){r.getPicklists(o.module).then(function(e){o.picklistsModule=e,o.taskSubjectField=c("filter")(o.module.fields,{name:"subject"},!0)[0],o.taskStatusField=c("filter")(o.module.fields,{name:"task_status"},!0)[0],o.taskStatusCompletedPicklistItem=c("filter")(o.picklistsModule[o.taskStatusField.picklist_id],{system_code:"completed"},!0)[0];var s=c("filter")(l,{system_code:"task"},!0)[0];f.fields=["subject","task_due_date","task_status","task_priority","updated_at","owner.users.full_name","created_by.users.full_name"],f.filters=[{field:"activity_type",operator:"is",value:s.label[t.user.tenant_language],no:1}],f.sort_field="task_due_date",f.sort_direction="asc",f.filter_logic="((1 and 2) &)";var i="is_not";o.view&&"completed"===o.view.type&&(i="is"),f.filters.push({field:"task_status",operator:i,value:o.taskStatusCompletedPicklistItem.label[t.user.tenant_language],no:2}),o.view&&"completed"!=o.view.type&&(f.filters.push({field:"task_status",operator:"empty",value:"-",no:3}),f.filter_logic=f.filter_logic.replace("2","(2 or 3)")),o.filter&&o.filter.assignedTo&&0!=o.filter.assignedTo.id&&(f.filters.push({field:"owner",operator:"equals",value:o.filter.assignedTo.id,no:f.filters.length+1}),f.filter_logic=f.filter_logic.replace("&","and "+f.filters.length+" &")),o.filter&&o.filter.subject&&(f.filters.push({field:"subject",operator:"contains",value:o.filter.subject,no:f.filters.length+1}),f.filter_logic=f.filter_logic.replace("&","and "+f.filters.length+" &"),o.searching=!0),1===o.currentPage&&(f.offset=0),f.filter_logic=f.filter_logic.replace(" &",""),r.findRecords("activities",f).then(function(t){t=t.data,o.pagingIcon="fa-chevron-right",t=a.processTasks(t,o.taskStatusCompletedPicklistItem),o.tasks=1===o.currentPage?t:o.tasks.concat(t),(t.length<1||t.length<f.limit)&&(o.allTasksLoaded=!0)})["finally"](function(){o.loading=!1,o.searching=!1})})},o.getTasks(),o.edit=function(t){o.editedTask=t,o.taskState=angular.copy(t),t.editing=!0},o.cancelEdit=function(){this.task=o.taskState,o.editedTask=null},o.update=function(t){if(t&&t.subject&&t.subject.trim()){o.taskUpdating=!0;var e=a.prepareTask(t);r.updateRecord("activities",e).then(function(){t["owner.users.id"]=t.assignedTo.id,t["owner.users.full_name"]=t.assignedTo.full_name,a.processTask(t,o.taskStatusCompletedPicklistItem),t.editing=!1})["finally"](function(){o.taskUpdating=!1})}},o.mark=function(t){t.marking=!0;var e={};e.id=t.id,e.task_status=o.taskStatusCompletedPicklistItem.id,r.updateRecord("activities",e).then(function(){o.tasks.splice(o.tasks.indexOf(t),1)})["catch"](function(t){409===t.status&&ngToast.create({content:c("translate")("Module.UniqueError"),className:"danger"})})["finally"](function(){o.marking=!1})},o.remove=function(t,e){r.deleteRecord("activities",t).then(function(){o.tasks.splice(e,1)})},o.loadMore=function(){o.allTasksLoaded||(o.pagingIcon="fa-spinner fa-spin",f.offset=o.currentPage*f.limit,r.findRecords("activities",f).then(function(t){t=t.data,o.pagingIcon="fa-chevron-right",o.currentPage=o.currentPage+1,t=a.processTasks(t,o.taskStatusCompletedPicklistItem),o.tasks=1===o.currentPage?t:o.tasks.concat(t),(t.length<1||t.length<f.limit)&&(o.allTasksLoaded=!0)}))},o.filterChanged=function(){o.currentPage=1,f.offset=0,o.filter&&o.filter.subject&&(o.filter.subject=null),o.allTasksLoaded=!1,o.getTasks(),n.put("assignTo",o.filter.assignedTo)}}]}}]).directive("taskForm",["$filter","ngToast","TaskService","ModuleService",function(t,e,s,i){return{restrict:"EA",scope:{isAll:"=",show:"=",taskDate:"=",taskCreated:"@"},templateUrl:cdnUrl+"view/app/tasks/taskForm.html",controller:["$scope",function(a){a.now=(new Date).getTime(),a.taskCreating=!1,a.module=t("filter")(a.$root.modules,{name:"activities"},!0)[0];var r=function(){a.task={},a.task.task_due_date=(new Date).setHours(0,0,0,0),a.task.assignedTo=t("filter")(a.$root.users,{id:a.$root.user.id},!0)[0]};i.getPicklists(a.module).then(function(e){a.picklistsModule=e,a.taskSubjectField=t("filter")(a.module.fields,{name:"subject"},!0)[0],a.taskStatusField=t("filter")(a.module.fields,{name:"task_status"},!0)[0],a.taskStatusNotStartedPicklistItem=t("filter")(a.picklistsModule[a.taskStatusField.picklist_id],{system_code:"not_started"},!0)[0],r()}),a.create=function(l){if(l&&l.subject&&l.subject.trim()){a.taskCreating=!0;var n=s.prepareTask(l,a.taskStatusNotStartedPicklistItem);i.insertRecord("activities",n).then(function(){a.$parent.views&&(a.$parent.view=a.$parent.views[0]),a.$parent.currentPage=1,a.$parent.getTasks(),a.$parent.allTasksLoaded=!1,a.$parent.showTaskForm=!1,r()})["catch"](function(s){409===s.status&&e.create({content:t("translate")("Module.UniqueError"),className:"danger"})})["finally"](function(){a.taskCreating=!1})}},a.cancelCreate=function(e){e.dueDate=41322528e5,e.taskText=null,a.selectedUser=t("filter")(a.$root.users,{EntityID:a.$root.user.id},!0)[0],a.show=!1,a.taskCreated&&a.taskCreated()}}]}}]);