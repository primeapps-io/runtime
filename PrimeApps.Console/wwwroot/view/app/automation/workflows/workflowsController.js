'use strict';

angular.module('primeapps')

    .controller('WorkflowsController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', 'ngToast', '$modal', '$timeout', 'helper', 'dragularService', 'WorkflowsService', 'LayoutService', '$http', 'config',
        function ($rootScope, $scope, $filter, $state, $stateParams, ngToast, $modal, $timeout, helper, dragularService, WorkflowsService, LayoutService, $http, config) {
            $scope.loading = true;
            $scope.$parent.loadingFilter = false;
            //$rootScope.modules = $http.get(config.apiUrl + 'module/get_all');
            $scope.workflows = [];
            $scope.$parent.workflows = [];
            $scope.$parent.menuTopTitle = "Automation";
            $scope.$parent.activeMenu = 'automation';
            $scope.$parent.activeMenuItem = 'workflows'; 
            //Pagening Start
            $scope.requestModel = { //default page value
                limit: "10",
                offset: 0,
                order_column: "name"
            };

            WorkflowsService.count().then(function (response) {
                $scope.pageTotal = response.data;
            });

            WorkflowsService.find($scope.requestModel).then(function (response) {
                if (response.data) {
                    var data = fillModule(response.data);

                    $scope.workflows = data;
                    $scope.$parent.workflows = data;
                    $scope.loading = false;
                }
            });

            $scope.changePage = function (page) {
                $scope.loading = true;
                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;


                WorkflowsService.find(requestModel, organitzationId).then(function (response) {
                    var data = fillModule(response.data);

                    $scope.workflows = data;
                    $scope.$parent.workflows = data;
                    $scope.loading = false;
                });

            };

            $scope.changeOffset = function () {
                $scope.changePage(1);
            };

            var fillModule = function (data) {
                for (var i = 0; i < data.length; i++) {
                    var moduleId = data[i].module_id;
                    var module = $filter('filter')($scope.$parent.modules, { id: moduleId }, true)[0];
                    data[i].module = angular.copy(module);
                }

                return data;
            };
            //Pagening End


            //Modal Start
            $scope.showFormModal = function (id) {
                if (id) {
                    $scope.id = id;

                    //selectRule();

                }

                $scope.formModal = $scope.formModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/automation/workflows/workflowModal.html',
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

            //Modal End
        }
    ]);