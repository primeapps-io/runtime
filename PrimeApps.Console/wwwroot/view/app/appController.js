'use strict';

angular.module('primeapps')

    .controller('AppController', ['$rootScope', '$scope', '$filter', '$location', 'helper', 'ngToast', '$state', '$cookies',
        function ($rootScope, $scope, $filter, $location, helper, ngToast, $state, $cookies) {

            var appId = $state.params.appId;

            if (!appId) {
                ngToast.create({ content: $filter('translate')('Common.NotFound'), className: 'warning' });
                $state.go('app.allApps');
                return;
            }

            $cookies.put('app_id', appId);

            $scope.menuTopTitle = "XBrand CRM";
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