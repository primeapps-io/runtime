'use strict';

angular.module('primeapps')

	.controller('TenantsController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', '$timeout', 'helper', 'dragularService', 'TenantsService', 'LayoutService', '$http', 'config',
		function ($rootScope, $scope, $filter, $state, $stateParams, $modal, $timeout, helper, dragularService, TenantsService, LayoutService, $http, config) {
         
            $scope.$parent.activeMenuItem = 'tenants';
            $rootScope.breadcrumblist[2].title = 'Tenants';

			console.log("TenantsController");

        }
    ]);