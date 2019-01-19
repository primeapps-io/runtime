'use strict';

angular.module('primeapps')

    .controller('CollaboratorsController', ['$rootScope', '$scope', 'guidEmpty', 'entityTypes', 'helper', 'config', '$http', '$localStorage', 'operations', '$filter', 'ngToast', '$cache', 'activityTypes', 'CollaboratorsService', '$window', '$state', '$modal', 'dragularService', '$timeout', '$interval', '$aside',
        function ($rootScope, $scope, guidEmpty, entityTypes, helper, config, $http, $localStorage, operations, $filter, ngToast, $cache, activityTypes, CollaboratorsService, $window, $state, $modal, dragularService, $timeout, $interval, $aside) {
            $scope.collaboratorArray = [];

            $scope.$parent.menuTopTitle = "Organization";
            $scope.$parent.activeMenu = 'organization';
            $scope.$parent.activeMenuItem = 'collaborators';
            $scope.collaboratorModel = {};
            var organitzationId = $rootScope.currentOrganization ? $rootScope.currentOrganization.id : 1;

            $scope.requestModel = {
                limit: "10",
                offset: 0
            };

            CollaboratorsService.count(organitzationId).then(function (response) {
                $scope.pageTotal = response.data;
            });

            CollaboratorsService.find($scope.requestModel, organitzationId).then(function (response) {
                $scope.collaboratorArray = response.data;
                $scope.$parent.collaboratorArray = response.data;
                $scope.loading = false;
            });

            $scope.changePage = function (page) {
                $scope.loading = true;
                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;


                CollaboratorsService.find(requestModel, organitzationId).then(function (response) {
                    $scope.collaboratorArray = response.data;
                    $scope.$parent.collaboratorArray = response.data;
                    $scope.loading = false;
                });

            };

            $scope.changeOffset = function () {
                $scope.changePage(1)
            };

            $scope.getCollaborators = function () {
                var filter = {};
                filter.organization_id = organitzationId;
                filter.page = 1;
                filter.order_by = null;
                filter.order_field = null;

                CollaboratorsService.get(filter)
                    .then(function (response) {
                        if (response.data) {
                            $scope.collaboratorArray = response.data;
                            $scope.$parent.collaboratorArray = response.data;
                        }
                    })
                    .catch(function (error) {
                        getToastMsg('Common.Error', 'danger');
                    });
            }

            //$scope.getCollaborators();

            $scope.selectCollaborators = function (id) {
                if (!id)
                    return false;

                var result = $filter('filter')($scope.collaboratorArray, { id: id }, true)[0];
                $scope.selectedCollaborator = angular.copy(result);

                $scope.$parent.activeMenu = "collaborator";
                $scope.$parent.activeMenuItem = 'collaborator';
            }

            $scope.$parent.selectCollaborators = function (id) {
                if (!id)
                    return false;

                var result = $filter('filter')($scope.collaboratorArray, { id: id }, true)[0];
                $scope.collaboratorId = id;
                $scope.$parent.collaboratorId = id;
                $scope.selectedCollaborator = angular.copy(result);
                $scope.$parent.selectedCollaborator = angular.copy(result);
            }

            $scope.roles = [
                { 'name': 'Admin', 'value': 'administrator' },
                { 'name': 'Collaborator', 'value': 'collaborator' }
            ];

            $scope.addNewCollaborator = function () {
                $scope.addNewCollaboratorModal = $scope.addNewCollaboratorModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/organization/collaborators/addNewCollaborator.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
                });
                $scope.addNewCollaboratorModal.$promise.then(function () {
                    $scope.addNewCollaboratorModal.show();

                });

            };

            $scope.save = function (newCollaboratorForm) {
                if (!newCollaboratorForm.$valid)
                    return false;

                var result = $filter('filter')($scope.collaboratorArray, { email: $scope.collaboratorModel.email }, true)[0];

                if (result)
                    return false;

                $scope.submitting = true;

                var newCol = {};
                newCol.organization_id = organitzationId;
                newCol.role = $scope.collaboratorModel.role.value;
                newCol.email = $scope.collaboratorModel.email;
                newCol.first_name = $scope.collaboratorModel.first_name;
                newCol.last_name = $scope.collaboratorModel.last_name;
                newCol.created_at = new Date();

                CollaboratorsService.save(newCol)
                    .then(function (response) {
                        if (response.data) {
                            getToastMsg('Common.Success', 'success');
                            $scope.collaboratorModel.email = "";
                            $scope.getCollaborators();
                            $state.reload();
                            $scope.submitting = false;
                            $scope.addNewCollaboratorModal.hide();
                        }
                    })
                    .catch(function () {
                        getToastMsg('Common.Error', 'danger');
                        $scope.submitting = false;
                    });

            }

            $scope.update = function () {
                if (!$scope.selectedCollaborator)
                    return false;

                CollaboratorsService.update($scope.selectedCollaborator)
                    .then(function (response) {
                        if (response.data) {
                            getToastMsg('Common.Success', 'success');
                            $scope.getCollaborators();
                            $state.reload();
                        }
                    })
                    .catch(function (error) {
                        getToastMsg('Common.Error', 'danger');
                    });
            }
             
            $scope.delete = function (id) {
                if (!id)
                    return false;

                var result = $filter('filter')($scope.collaboratorArray, { id: id }, true)[0];

                if (!result)
                    return false;

                var data = {};
                data.user_id = id;
                data.organization_id = organitzationId;
                data.role = result.role;

                CollaboratorsService.delete(data)
                    .then(function (response) {
                        if (response.data) {
                            getToastMsg('Common.Success', 'success');

                            $scope.selectedCollaborator = {};
                            $scope.$parent.selectedCollaborator = {};
                            $scope.collaboratorId = null;
                            $scope.$parent.collaboratorId = null;

                            $scope.getCollaborators();
                            $state.reload();
                        }
                    })
                    .catch(function (error) {
                        getToastMsg('Common.Error', 'danger');
                    });
            }



            var getToastMsg = function (msg, type = 'success') {
                ngToast.create({
                    content: $filter('translate')(msg),
                    className: type
                });
            }
        }
    ]);