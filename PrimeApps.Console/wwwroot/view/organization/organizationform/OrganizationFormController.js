'use strict';

angular.module('primeapps')

    .controller('OrganizationFormController', ['$rootScope', '$scope', 'guidEmpty', 'entityTypes', 'helper', 'config', '$http', '$localStorage', 'operations', '$filter', '$cache', 'activityTypes', 'OrganizationFormService', '$window', '$state', '$modal', 'dragularService', '$timeout', '$interval', '$aside',
        function ($rootScope, $scope, guidEmpty, entityTypes, helper, config, $http, $localStorage, operations, $filter, $cache, activityTypes, OrganizationFormService, $window, $state, $modal, dragularService, $timeout, $interval, $aside) {
            console.log("OrganizationFormController");
        }
    ]);