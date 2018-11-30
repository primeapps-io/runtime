'use strict';

angular.module('primeapps')

    .controller('ApprovelProcessController', ['$rootScope', '$scope', '$window', '$filter', 'ngToast', 'guidEmpty', '$modal', 'ApprovelProcessService',
        function ($rootScope, $scope,$window, $filter, ngToast, guidEmpty, $modal, ApprovelProcessService) {
            $scope.loading = true;

            var getProcesses = function () {
                ApprovelProcessService.getAll()
                    .then(function (response) {
                        $scope.processes = ApprovelProcessService.process(response.data);
                        $scope.loading = false;
                        $rootScope.approvalProcesses = response.data;
                    });
            };

            getProcesses();

            $scope.delete = function (id) {
                ApprovelProcessService.getAllProcessRequests(id)
                    .then(function (response) {

                        if($filter('filter')(response.data, { status: '!approved' }, true).length > 0){
                            ngToast.create({ content: $filter('translate')('Setup.Workflow.ProcessCanNotDelete'), className: 'danger' });
                            $scope.loading = false;
                        }else{
                            ApprovelProcessService.delete(id)
                                .then(function () {
                                    getProcesses();
                                });
                        }
                    });
            };

            $scope.edit = function (id) {
                ApprovelProcessService.getAllProcessRequests(id)
                    .then(function (response) {

                        $window.location.href = "#/app/setup/approvel?id=" + id;
                        //if($filter('filter')(response.data, { status: '!approved' }, true).length > 0){
                        //    ngToast.create({ content: $filter('translate')('Setup.Workflow.ProcessCanNotUpdate'), className: 'danger' });
                        //    $scope.loading = false;
                        //}else{
                        //    $window.location.href="#/app/setup/approvel?id="+id;
                        //}
                    });
            }
        }
    ]);