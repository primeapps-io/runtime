'use strict';

angular.module('primeapps')

    .controller('AppController', ['$rootScope', '$scope', '$filter', 'ngToast', '$state', '$cookies', '$http', 'config', '$localStorage', 'LayoutService', '$q', '$window',
        function ($rootScope, $scope, $filter, ngToast, $state, $cookies, $http, config, $localStorage, LayoutService, $q, $window) {

            $scope.appId = $state.params.appId;
            $scope.orgId = $state.params.orgId;

            if (!$scope.appId) {
                ngToast.create({ content: $filter('translate')('Common.NotFound'), className: 'warning' });
                $state.go('app.allApps');
                return;
            }

            $rootScope.language = 'en';
            $scope.menuTopTitle = $rootScope.currentApp.label;
            $scope.activeMenu = 'app';
            $scope.activeMenuItem = 'overview';
            $scope.tabTitle = 'Overview';

            $scope.getBasicModules = function () {
                LayoutService.getBasicModules().then(function (result) {
                    $scope.modules = result.data;
                });
            };

            $scope.getBasicModules();

            $scope.preview = function () {
                $scope.previewActivating = true;
                LayoutService.getPreviewToken()
                    .then(function (response) {
                        $scope.previewActivating = false;
                        $window.open('http://localhost:5001?preview=' + encodeURIComponent(response.data), '_blank');
                    })
                    .catch(function (response) {
                        $scope.previewActivating = false;
                    });
            };
        }
    ]);