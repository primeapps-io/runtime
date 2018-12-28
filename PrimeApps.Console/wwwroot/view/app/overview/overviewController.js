'use strict';

angular.module('primeapps')

	.controller('OverviewController', ['$rootScope', '$scope',
		function ($rootScope,$scope) {

			 //console.log("asdfasdf")
            $scope.$parent.menuTopTitle ="XBrand CRM";
            $scope.$parent.activeMenu= 'app';
            $scope.$parent.activeMenuItem = 'overview';
            $scope.$parent.tabTitle='Overview';

        }
	]);