'use strict';

angular.module('primeapps')

    .controller('AppController', ['$rootScope', '$scope', '$filter', '$state', '$cookies', '$http', 'config', '$localStorage', 'LayoutService', '$q', '$window', 'helper',
        function ($rootScope, $scope, $filter, $state, $cookies, $http, config, $localStorage, LayoutService, $q, $window, helper) {

            $scope.appId = $state.params.appId;
            $scope.orgId = $state.params.orgId;
            $rootScope.menuOpen = [];
            $rootScope.menuOpen[$scope.orgId] = true;
            $rootScope.subMenuOpen = "";

            if (!$rootScope.currentAppId) {
                toastr.warning($filter('translate')('Common.NotFound'));
                var defaultOrg = $filter('filter')($rootScope.organizations, { default: true }, true)[0];
                window.location.href = '/#/apps?orgId=' + defaultOrg.id;
                return;
            }

            $rootScope.language = 'en';
            $scope.activeMenu = 'app';
            $scope.activeMenuItem = 'overview';
            $scope.tabTitle = 'Overview';


            $rootScope.openSubMenu = function (name) {
                if ($rootScope.subMenuOpen == name)
                    $rootScope.subMenuOpen = "";
                else
                    $rootScope.subMenuOpen = name;

                if (name != "")
                    $scope.activeMenuItem = "";
            }

        }
    ]);