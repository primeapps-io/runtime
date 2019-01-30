'use strict';

angular.module('primeapps')

    .controller('AuditTrailController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', '$timeout', 'helper', 'dragularService', 'AuditTrailService', 'LayoutService', '$http', 'config',
        function ($rootScope, $scope, $filter, $state, $stateParams, $modal, $timeout, helper, dragularService, AuditTrailService, LayoutService, $http, config) {

            //$rootScope.modules = $http.get(config.apiUrl + 'module/get_all');

            $scope.$parent.menuTopTitle = "Security";
            //$scope.$parent.activeMenu = 'security';
            $scope.$parent.activeMenuItem = 'auditTrail';
            $rootScope.breadcrumblist[2].title = 'Audit Trail';

            console.log("AuditTrailController");

        }
    ]);