'use strict';

angular.module('primeapps')

    .controller('PasswordPoliciesController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', 'ngToast', '$modal', '$timeout', 'helper', 'dragularService', 'PasswordPoliciesService', 'LayoutService', '$http', 'config',
        function ($rootScope, $scope, $filter, $state, $stateParams, ngToast, $modal, $timeout, helper, dragularService,PasswordPoliciesService, LayoutService, $http, config) {

            //$rootScope.modules = $http.get(config.apiUrl + 'module/get_all');
            $scope.$parent.menuTopTitle = "Security";
            $scope.$parent.activeMenu = 'security';
            $scope.$parent.activeMenuItem = 'passwordPolicies';
            $rootScope.breadcrumblist[2].title = 'Password Policies';

            console.log("PasswordPoliciesController");

        }
    ]);