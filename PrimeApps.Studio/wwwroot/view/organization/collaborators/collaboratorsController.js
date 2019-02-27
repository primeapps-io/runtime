'use strict';

angular.module('primeapps')

    .controller('CollaboratorsController', ['$rootScope', '$scope', 'guidEmpty', 'entityTypes', 'helper', 'config', '$http', '$localStorage', 'operations', '$filter', '$cache', 'activityTypes', 'CollaboratorsService', '$window', '$state', '$modal', 'dragularService', '$timeout', '$interval', '$aside',
        function ($rootScope, $scope, guidEmpty, entityTypes, helper, config, $http, $localStorage, operations, $filter, $cache, activityTypes, CollaboratorsService, $window, $state, $modal, dragularService, $timeout, $interval, $aside) {
            $scope.collaboratorArray = [];

            // $scope.$parent.menuTopTitle = "Organization";
            $scope.$parent.activeMenu = 'organization';
            $scope.$parent.activeMenuItem = 'collaborators';
            $scope.updatingRole = false;
            $scope.collaboratorModel = {};
            $scope.loading = true;
            $scope.showNewCollaboratorInfo = false;

            $scope.requestModel = {
                limit: "10",
                offset: 0
            };

            $scope.generator = function (limit) {
                $scope.placeholderArray = [];
                for (var i = 0; i < limit; i++) {
                    $scope.placeholderArray[i] = i;
                }

            };
            $scope.generator(10);

            CollaboratorsService.count($rootScope.currentOrgId).then(function (response) {
                $scope.$parent.collaboratorCount = response.data;
                $scope.pageTotal = response.data;
            });

            CollaboratorsService.find($scope.requestModel, $rootScope.currentOrgId).then(function (response) {
                $scope.collaboratorArray = response.data;
                $scope.$parent.collaboratorArray = response.data;
                $scope.loading = false;
            });

            $scope.changePage = function (page) {
                $scope.loading = true;
                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;

                CollaboratorsService.count($rootScope.currentOrgId).then(function (response) {
                    $scope.$parent.collaboratorCount = response.data;
                    $scope.pageTotal = response.data;
                });
                CollaboratorsService.find(requestModel, $rootScope.currentOrgId).then(function (response) {
                    $scope.collaboratorArray = response.data;
                    $scope.$parent.collaboratorArray = response.data;
                    $scope.loading = false;
                });

            };

            $scope.changeOffset = function () {
                $scope.changePage(1);
            };

            $scope.getCollaborators = function () {
                var filter = {};
                filter.organization_id = $rootScope.currentOrgId;
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

                var result = $filter('filter')($scope.collaboratorArray, {id: id}, true)[0];
                $scope.selectedCollaborator = angular.copy(result);
                $scope.collaboratorModel.role = $filter('filter')($scope.roles, {value: $scope.selectedCollaborator.role}, true)[0];
                $scope.$parent.activeMenu = "collaborator";
                $scope.$parent.activeMenuItem = 'collaborator';
            }

            $scope.$parent.selectCollaborators = function (id) {
                if (!id)
                    return false;

                var result = $filter('filter')($scope.collaboratorArray, {id: id}, true)[0];
                $scope.collaboratorId = id;
                $scope.$parent.collaboratorId = id;
                $scope.selectedCollaborator = angular.copy(result);
                $scope.$parent.selectedCollaborator = angular.copy(result);
                $scope.collaboratorModel.role = $filter('filter')($scope.roles, {value: $scope.$parent.selectedCollaborator.role}, true)[0];
            }

            $scope.roles = [
                {'name': 'Admin', 'value': 'administrator'},
                {'name': 'Collaborator', 'value': 'collaborator'}
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

                var result = $filter('filter')($scope.collaboratorArray, {email: $scope.collaboratorModel.email}, true)[0];

                if (result)
                    return false;

                $scope.submitting = true;

                var newCol = {};
                newCol.organization_id = $rootScope.currentOrgId;
                newCol.role = $scope.collaboratorModel.role.value;
                newCol.email = $scope.collaboratorModel.email;
                newCol.first_name = $scope.collaboratorModel.first_name;
                newCol.last_name = $scope.collaboratorModel.last_name;
                newCol.created_at = new Date();

                CollaboratorsService.save(newCol)
                    .then(function (response) {
                        if (response.data) {
                            toastr.success('Collaborator is saved successfully');
                            $scope.collaboratorModel.email = "";

                            $scope.submitting = false;
                            $scope.userPassword = response.data.password;
                            $scope.showNewCollaboratorInfo = true;
                            $scope.changePage(1);
                        }
                    })
                    .catch(function () {
                        toastr.error($filter('translate')('Common.Error'));
                        $scope.submitting = false;
                    });

            }

            $scope.close = function () {
                $scope.getCollaborators();
                $state.reload();
                $scope.addNewCollaboratorModal.hide();
                $scope.showNewCollaboratorInfo = false;
            }

            $scope.update = function (collaboratorModel) {
                if (!$scope.selectedCollaborator)
                    return false;

                var updCollaborator = {};
                updCollaborator.id = $scope.selectedCollaborator.id;
                updCollaborator.organization_id = $scope.selectedCollaborator.organization_id;
                updCollaborator.email = $scope.selectedCollaborator.email;
                updCollaborator.role = collaboratorModel.role.value;
                CollaboratorsService.update(updCollaborator)
                    .then(function (response) {
                        if (response.data) {
                            toastr.success('Role is changed successfully');
                            $scope.getCollaborators();
                            $scope.updatingRole = false;
                        }
                    })
                    .catch(function (error) {
                        toastr.error($filter('translate')('Common.Error'));
                        $scope.updatingRole = false;
                        $scope.collaboratorModel.role = $filter('filter')($scope.roles, {value: $scope.selectedCollaborator.role}, true)[0];
                    });
            }

            $scope.delete = function (id) {
                if (!id)
                    return false;

                var result = $filter('filter')($scope.collaboratorArray, {id: id}, true)[0];

                if (!result)
                    return false;

                $scope.removing = true;
                var data = {};
                data.user_id = id;
                data.organization_id = $rootScope.currentOrgId;
                data.role = result.role;

                CollaboratorsService.delete(data)
                    .then(function (response) {
                        if (response.data) {
                            toastr.success('Collaborator is deleted successfully');

                            $scope.selectedCollaborator = {};
                            $scope.$parent.selectedCollaborator = {};
                            $scope.collaboratorId = null;
                            $scope.$parent.collaboratorId = null;
                            $scope.removing = false;
                            $scope.getCollaborators();
                            $state.reload();
                        }
                    })
                    .catch(function (error) {
                        toastr.error($filter('translate')('Common.Error'));
                        $scope.removing = false;
                    });
            };

            $scope.addMasterUser = function () {
                var newCol = {};
                newCol.organization_id = $rootScope.currentOrgId;
                newCol.role = $scope.collaboratorModel.role.value;
                newCol.email = $scope.collaboratorModel.email;
                newCol.first_name = $scope.collaboratorModel.first_name;
                newCol.last_name = $scope.collaboratorModel.last_name;
                newCol.created_at = new Date();

                CollaboratorsService.save(newCol)
                    .then(function (response) {
                        if (response.data) {
                            toastr.success('Collaborator is saved successfully');
                            $scope.collaboratorModel.email = "";

                            $scope.submitting = false;
                            $scope.userPassword = response.data.password;
                            $scope.showNewCollaboratorInfo = true;
                            $scope.changePage(1);
                        }
                    })
                    .catch(function () {
                        toastr.error($filter('translate')('Common.Error'));
                        $scope.submitting = false;
                    });
            };
        }
    ]);