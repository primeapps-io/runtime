angular.module("primeapps").controller("ActionButtonFrameController",["$rootScope","$scope","$location","$sce",function(r,o,e,t){o.url=t.trustAsResourceUrl(o.$parent.$parent.frameUrl)}]);