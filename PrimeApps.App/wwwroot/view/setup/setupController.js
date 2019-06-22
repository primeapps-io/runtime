"use strict";angular.module("primeapps").controller("SetupController",["$rootScope","$scope","$filter","$location","helper",function(e,p,t,a,r){p.helper=r,p.selectMenuItem=function(p){e.selectedSetupMenuLink=p.link},p.setMenuItems=function(){if(p.menuItems=[{link:"#/app/setup/settings",label:"Setup.Nav.PersonalSettings",order:1,app:"crm"},{link:"#/app/setup/importhistory",label:"Setup.Nav.Data",order:7,app:"crm"}],r.hasAdminRights()&&!e.preview){var s=[{link:"#/app/setup/users",label:"Setup.Nav.Users",order:2,app:"crm"},{link:"#/app/setup/organization",label:"Setup.Nav.OrganizationSettings",order:3,app:"crm"},{link:"#/app/setup/general",label:"Setup.Nav.System",order:8,app:"crm"}],n=p.menuItems.concat(s);p.menuItems=t("orderBy")(n,"order")}var u=a.path();switch(u){case"/app/setup/paymenthistory":u="/app/setup/payment";break;case"/app/setup/profiles":case"/app/setup/profile":u="/app/setup/users";break;case"app/setup/reportForm":u="/app/setup/reports"}var i=t("filter")(p.menuItems,{link:"#"+u})[0];p.selectMenuItem(i?i:p.menuItems[0]),angular.forEach(p.menuItems,function(p){p.active="common"===p.app||p.app===e.app?!0:!1})},p.setMenuItems(),p.menuItemClass=function(p){return e.selectedSetupMenuLink===p.link?"active":""}}]);