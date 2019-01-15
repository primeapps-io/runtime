'use strict';

angular.module('primeapps')

	.controller('ProcessesController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', 'ngToast', '$modal', '$timeout', 'helper', 'dragularService', 'ProcessesService', 'LayoutService', '$http', 'config',
		function ($rootScope, $scope, $filter, $state, $stateParams, ngToast, $modal, $timeout, helper, dragularService, ProcessesService, LayoutService, $http, config) {
            $scope.loading = true;
			//$rootScope.modules = $http.get(config.apiUrl + 'module/get_all');
            $scope.$parent.wizardStep = 0;
            $scope.$parent.tab = 0;
            $scope.processes = [];
            $scope.$parent.processes = [];
			$scope.$parent.menuTopTitle = "Automation";
			$scope.$parent.activeMenu = 'automation';
			$scope.$parent.activeMenuItem = 'processes';
            var organitzationId = $rootScope.currentOrganization ? $rootScope.currentOrganization.id : 1; //TODO Organization ID

            //Pagening Start
            $scope.requestModel = { //default page value
                limit: "10",
                offset: 0,
                order_column: "name"
            };

            ProcessesService.count(organitzationId).then(function (response) {
                $scope.pageTotal = response.data;
            });

            ProcessesService.find($scope.requestModel, organitzationId).then(function (response) {
                if (response.data) {
                    var data = fillModule(response.data);

                    $scope.rules = data;
                    $scope.loading = false;
                }
            });

            $scope.changePage = function (page) {
                $scope.loading = true;
                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;


                ProcessesService.find(requestModel, organitzationId).then(function (response) {
                    var data = fillModule(response.data);

                    $scope.rules = data;
                    $scope.loading = false;
                });

            };

            $scope.changeOffset = function () {
                $scope.changePage(1)
            };
            //Pagening End


            $scope.delete = function (id) {
                ApprovelProcessService.getAllProcessRequests(id)
                    .then(function (response) {

                        if ($filter('filter')(response.data, { status: '!approved' }, true).length > 0) {
                            ngToast.create({ content: $filter('translate')('Setup.Workflow.ProcessCanNotDelete'), className: 'danger' });
                            $scope.loading = false;
                        } else {
                            ApprovelProcessService.delete(id)
                                .then(function () {
                                    $socpe.changeOffset(1);
                                });
                        }
                    });
            };
		
            //Modal Start
            $scope.showFormModal = function (id) {
                if (id) {
                    $scope.id = id;
                    //selectRule();
                }

                $scope.prosessFormModal = $scope.prosessFormModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/automation/prosesses/prosessModal.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
                });

                $scope.ruleFormModal.$promise.then(function () {
                    $scope.ruleFormModal.show();
                });

            };

            $scope.cancel = function () {
                angular.forEach($scope.currentRelation, function (value, key) {
                    $scope.currentRelation[key] = $scope.currentRelationState[key];
                });

                $scope.processes = [];
                $scope.id = null;
                $scope.prosessFormModal.hide();
            }
            //Modal End

		}
	]);