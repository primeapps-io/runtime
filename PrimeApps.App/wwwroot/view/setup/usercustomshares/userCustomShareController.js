"use strict";angular.module("primeapps").controller("UserCustomShareController",["$rootScope","$scope","$filter","ngToast","$popover","helper","UserCustomShareService",function(e,t,r,n,o,s,a){function l(){a.getAll().then(function(n){t.userowners=n.data;for(var o=0;o<n.data.length;o++){var s=n.data[o];t.userowners[o].userName=r("filter")(e.users,{id:s.user_id},!0)[0].full_name}t.loading=!1})["catch"](function(){t.loading=!1})}t.loading=!0,l(),t["delete"]=function(e){a["delete"](e).then(function(){n.create({content:r("translate")("Setup.UserGroups.DeleteSuccess"),className:"success"}),l()})}}]);