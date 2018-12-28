'use strict';

angular.module('primeapps')

    .controller('AccountController', ['$rootScope', '$scope', 'guidEmpty', 'entityTypes', 'helper', 'config', '$http', '$localStorage', 'operations', '$filter', '$cache', 'activityTypes', 'AccountService', '$window', '$state', '$modal', 'dragularService', '$timeout', '$interval', '$aside',
        function ($rootScope, $scope, guidEmpty, entityTypes, helper, config, $http, $localStorage, operations, $filter, $cache, activityTypes, AccountService, $window, $state, $modal, dragularService, $timeout, $interval, $aside) {
            console.log("AccountController");
        }
    ]);