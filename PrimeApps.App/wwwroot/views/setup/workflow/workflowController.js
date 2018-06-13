'use strict';

angular.module('primeapps')

    .controller('WorkflowController', ['$rootScope', '$scope', '$filter', 'ngToast', 'guidEmpty', '$modal', 'WorkflowService',
        function ($rootScope, $scope, $filter, ngToast, guidEmpty, $modal, WorkflowService) {
            $scope.loading = true;

            var getWorkflows = function () {
                WorkflowService.getAll()
                    .then(function (response) {
                        $scope.workflows = WorkflowService.process(response.data);
                        $scope.loading = false;
                    });
            };



            getWorkflows();

            $scope.delete = function (id) {
                WorkflowService.delete(id)
                    .then(function () {
                        getWorkflows();
                    });
            }
        }
    ]);