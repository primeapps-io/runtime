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

            $cookies.put('app_id', $scope.appId);

            if (!$rootScope.currentOrganization) {
                $rootScope.currentOrganization = $filter('filter')($rootScope.organizations, { id: parseInt($scope.orgId) }, true)[0];
            }

            if ($scope.appId != ($localStorage.get("current_app") != null ? $localStorage.get("current_app").id : false)) {
                $http.get(config.apiUrl + "app/get/" + $scope.appId).then(function (result) {
                    if (result.data) {
                        $scope.menuTopTitle = result.data.label;
                        $localStorage.set("current_app", result.data);

                    }
                });
            } else {
                $scope.setTopTitle = function (link) {
                    $scope.menuTopTitle = $localStorage.get("current_app").label;

                }
            }

            $rootScope.language = 'en';
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