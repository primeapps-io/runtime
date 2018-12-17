'use strict';

angular.module('primeapps')

    .controller('AppsFormController', ['$rootScope', '$scope', 'guidEmpty', 'entityTypes', 'helper', 'config', '$http', '$localStorage', 'operations', '$filter', '$cache', 'activityTypes', 'AppsFormService', '$window', '$state', '$modal', 'dragularService', '$timeout', '$interval', '$aside',
        function ($rootScope, $scope, guidEmpty, entityTypes, helper, config, $http, $localStorage, operations, $filter, $cache, activityTypes, AppsFormService, $window, $state, $modal, dragularService, $timeout, $interval, $aside) {
            console.log("AppsFormController");
        }
    ]);