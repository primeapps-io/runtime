'use strict';

angular.module('primeapps')

    .controller('AppController', ['$rootScope', '$scope', '$filter', 'ngToast', '$state', '$cookies', '$http', 'config', '$localStorage',
        function ($rootScope, $scope, $filter, ngToast, $state, $cookies, $http, config, $localStorage) {

            $scope.appId = $state.params.appId;

            if (!$scope.appId) {
                ngToast.create({content: $filter('translate')('Common.NotFound'), className: 'warning'});
                $state.go('app.allApps');
                return;
            }

            $cookies.put('app_id', $scope.appId);

            if ($scope.appId != ($localStorage.get("currentApp") != null ? $localStorage.get("currentApp").id : false)) {
                $http.get(config.apiUrl + "app/get/" + $scope.appId).then(function (result) {
                    if (result.data) {
                        $scope.menuTopTitle = result.data.label;
                        $localStorage.set("currentApp", result.data)
                    }
                });
            } else {
                $scope.setTopTitle = function () {
                    $scope.menuTopTitle = $localStorage.get("currentApp").label;
                }
            }


            $scope.activeMenu = 'app';
            $scope.activeMenuItem = 'overview';
            $scope.tabTitle = 'Overview';
            $rootScope.breadcrumbListe = [
                {
                    title: 'First Organization',
                    link: "asdasd"
                },
                {
                    title: 'XBrand CRM'
                }
            ];

        }
    ]);