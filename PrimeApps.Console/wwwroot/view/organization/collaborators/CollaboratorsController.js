'use strict';

angular.module('primeapps')

    .controller('CollaboratorsController', ['$rootScope', '$scope', 'guidEmpty', 'entityTypes', 'helper', 'config', '$http', '$localStorage', 'operations', '$filter', '$cache', 'activityTypes', 'CollaboratorsService', '$window', '$state', '$modal', 'dragularService', '$timeout', '$interval', '$aside',
        function ($rootScope, $scope, guidEmpty, entityTypes, helper, config, $http, $localStorage, operations, $filter, $cache, activityTypes, CollaboratorsService, $window, $state, $modal, dragularService, $timeout, $interval, $aside) {
            console.log("CollaboratorsController");
        }
    ]);