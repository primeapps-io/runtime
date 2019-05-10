'use strict';

angular.module('primeapps')

    .controller('AdvancedWorkflowsController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', '$timeout', 'helper', 'dragularService', 'AdvancedWorkflowsService', 'LayoutService', '$http', 'config',
        function ($rootScope, $scope, $filter, $state, $stateParams, $modal, $timeout, helper, dragularService, AdvancedWorkflowsService, LayoutService, $http, config) {
            $scope.loading = true;
            $scope.$parent.loadingFilter = false;
            $scope.workflows = [];
            $scope.$parent.workflows = [];
            //$scope.$parent.menuTopTitle = "Automation";
            //$scope.$parent.activeMenu = 'automation';
            $scope.$parent.activeMenuItem = 'advancedWorkflows';
            $rootScope.subtoggleClass = '';
            $rootScope.toggleClass = ''
            $rootScope.breadcrumblist[2].title = 'Advanced Workflows';

            $scope.generator = function (limit) {
                $scope.placeholderArray = [];
                for (var i = 0; i < limit; i++) {
                    $scope.placeholderArray[i] = i;
                }

            };

            $scope.generator(10);


            //Pagening Start
            $scope.requestModel = { //default page value
                limit: "10",
                offset: 0,
                order_column: "name"
            };

            var count = function () {
                AdvancedWorkflowsService.count().then(function (response) {
                    $scope.pageTotal = response.data;
                });
            };

            count();
            AdvancedWorkflowsService.find($scope.requestModel).then(function (response) {
                if (response.data) {
                    var data = fillModule(response.data);

                    $scope.workflows = data;
                    //$scope.$parent.workflows = data;
                    $scope.loading = false;
                }
            });

            $scope.changePage = function (page) {
                $scope.loading = true;
                count();
                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;


                AdvancedWorkflowsService.find(requestModel, $rootScope.currentOrgId)
                    .then(function (response) {
                        var data = fillModule(response.data);

                        $scope.workflows = data;
                        //$scope.$parent.workflows = data;
                        $scope.loading = false;
                    });

            };

            $scope.changeOffset = function (value) {
                $scope.changePage(value);
            };

            var fillModule = function (data) {
                for (var i = 0; i < data.length; i++) {
                    var moduleId = data[i].module_id;

                    if (moduleId) {
                        var module = $filter('filter')($rootScope.appModules, { id: moduleId }, true)[0];
                        data[i].module = angular.copy(module);
                    }
                }

                return data;
            };


            //Modal Start
            $scope.showFormModal = function (id) {
                if (id) {
                    $scope.id = id;

                    //selectRule();

                }

                $scope.formModal = $scope.formModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/processautomation/advancedworkflows/advancedworkflowModal.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
                });

                $scope.formModal.$promise.then(function () {
                    $scope.formModal.show();
                });

            };

            $scope.cancel = function () {
                angular.forEach($scope.currentRelation, function (value, key) {
                    $scope.currentRelation[key] = $scope.currentRelationState[key];
                });

                $scope.workflowModel = [];
                $scope.id = null;
                $scope.formModal.hide();
            };

            $scope.delete = function (workflow) {
                swal({
                    title: "Are you sure?",
                    text: " ",
                    icon: "warning",
                    buttons: ['Cancel', 'Yes'],
                    dangerMode: true
                }).then(function (value) {
                    if (value) {
                        workflow.delete = true;
                        AdvancedWorkflowsService.delete(workflow.id)
                            .then(function (response) {
                                if (response.data) {
                                    $scope.workflowModel = [];
                                    $scope.id = null;
                                    //$state.reload();
                                    $scope.changePage(1);
                                    toastr.success("Workflow is deleted successfully.", "Deleted!");
                                    workflow.delete = false;
                                }
                            });
                    }
                });
            };

            //Modal End

        }
    ]);