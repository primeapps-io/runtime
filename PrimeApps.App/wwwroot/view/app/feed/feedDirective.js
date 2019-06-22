"use strict";angular.module("primeapps").directive("feedList",["convert","entityTypes","guidEmpty","$localStorage","FeedService",function(e,t,n,i,o){return{restrict:"EA",scope:{feed:"=",entityId:"=",entityType:"="},templateUrl:cdnUrl+"view/app/feed/feedList.html",controller:["$rootScope","$scope",function(r,c){c.pageIndex=2,c.pagingIcon="fa-chevron-right",c.entityTypes=t,c.guidEmpty=n,c.config=r.config,c.loadMore=function(){c.pagingIcon="fa-spinner fa-spin",o.getActivityFeedDelta(r.workgroup.tenant_id,c.pageIndex,c.entityId,c.entityType).then(function(e){c.feed=c.feed.concat(e.data),c.count=e.data.length,c.pageIndex+=1,c.pagingIcon="fa-chevron-right"})},c.comment=function(t){t.comment&&(t.commentSending=!0,o.comment(r.workgroup.tenant_id,t.ID,t.comment,"").then(function(){var n={};n.userID=c.$root.user.id,n.userName=c.$root.user.firstName+" "+c.$root.user.lastName,n.timeStamp=e.toMsDate(new Date),n.entityName=t.comment,t.activities.push(n),t.commentSending=!1,t.comment=null,t.formOpened=!1}))},c.cancelComment=function(e){e.comment=null,e.formOpened=!1},c.enterToSend=!0,c.changeEnterToSend=function(e){i.write("EnterToSend",e)}}]}}]).directive("feedForm",["$filter","convert","entityTypes","FeedService",function(e,t,n,i){return{restrict:"EA",scope:{entityId:"=",entityType:"=",show:"="},templateUrl:cdnUrl+"view/app/feed/feedForm.html",controller:["$scope",function(e){e.feedCreating=!1,e.create=function(o){if(o&&o.text.trim()){e.feedCreating=!0;var r=e.$root.workgroup.tenant_id,c=o.text;i.create(r,e.entityId,e.entityType,c).then(function(i){var r={};r.ID=i.data,r.userID=e.$root.user.id,r.userName=e.$root.user.firstName+" "+e.$root.user.lastName,r.timeStamp=t.toMsDate(new Date),r.entityName=c,r.entityType=n.note,r.activities=[],e.$parent.feed.unshift(r),e.feedCreating=!1,e.show=!1,o.text=null})["catch"](function(){e.feedCreating=!1})}},e.cancelCreate=function(t){t&&(t.text=null),e.show=!1}}]}}]).directive("customSubmit",function(){return{restrict:"A",scope:{action:"&"},link:function(e,t,n){t.bind("keydown keypress",function(t){n.customSubmit&&angular.fromJson(n.customSubmit)&&13===t.which&&(e.$apply(function(){e.action()}),t.preventDefault())})}}}).directive("onEnter",function(){return function(e,t,n){t.bind("keydown keypress",function(t){13===t.which&&(e.$apply(function(){e.$eval(n.onEnter,{event:t})}),t.preventDefault())})}});