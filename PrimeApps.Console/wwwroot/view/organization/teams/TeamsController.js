'use strict';

angular.module('primeapps')

    .controller('TeamsController', ['$rootScope', '$scope', 'guidEmpty', 'entityTypes', 'helper', 'config', '$http', '$localStorage', 'operations', '$filter', '$cache', 'activityTypes', 'TeamsService', '$window', '$state', '$modal', 'dragularService', '$timeout', '$interval', '$aside',
        function ($rootScope, $scope, guidEmpty, entityTypes, helper, config, $http, $localStorage, operations, $filter, $cache, activityTypes, TeamsService, $window, $state, $modal, dragularService, $timeout, $interval, $aside) {
            console.log("TeamsController");
        }
    ]);