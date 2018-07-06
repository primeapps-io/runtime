'use strict';

angular.module('primeapps')

    .controller('DirectoryDetailController', ['$rootScope', '$scope', '$location',
        function ($rootScope, $scope, $location) {
            $scope.id = $location.search().id;
            $scope.showBack = false;
        }
    ]);