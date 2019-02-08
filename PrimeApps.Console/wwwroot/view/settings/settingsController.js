'use strict';

angular.module('primeapps')

    .controller('SettingsController', ['$rootScope', '$scope', 'SettingService',
        function ($rootScope, $scope, SettingService) {

            //$scope.$parent.menuTopTitle = "Settings";
            $scope.$parent.activeMenuItem = 'profile';

        }
    ]);