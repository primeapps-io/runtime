'use strict';

angular.module('primeapps')

    .controller('SettingsController', ['$rootScope', '$scope', 'SettingService',
        function ($rootScope, $scope, SettingService) {

            //$rootScope.modules = $http.get(config.apiUrl + 'module/get_all');

            $scope.$parent.menuTopTitle = "Settings";
            $scope.$parent.activeMenuItem = 'profile';

            console.log("SettingsController");

        }
    ]);