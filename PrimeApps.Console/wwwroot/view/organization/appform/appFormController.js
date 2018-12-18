'use strict';

angular.module('primeapps')

    .controller('AppFormController', ['$rootScope', '$scope', 'guidEmpty', 'entityTypes', 'helper', 'config', '$http', '$localStorage', 'operations', '$filter', '$cache', 'activityTypes', 'AppFormService', '$window', '$state', '$modal', 'dragularService', '$timeout', '$interval', '$aside',
        function ($rootScope, $scope, guidEmpty, entityTypes, helper, config, $http, $localStorage, operations, $filter, $cache, activityTypes, AppFormService, $window, $state, $modal, dragularService, $timeout, $interval, $aside) {

            $scope.openModal = function () {
                $scope.formModal = $scope.formModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/organization/appform/dependencyForm.html',
                    animation: 'side',
                    backdrop: 'static',
                    show: false
                });
                $scope.formModal.$promise.then(function () {
                    $scope.formModal.show();
                });
            }

        }
    ]);