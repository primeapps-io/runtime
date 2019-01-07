'use strict';

angular.module('primeapps')

    .controller('CollaboratorsController', ['$rootScope', '$scope', 'guidEmpty', 'entityTypes', 'helper', 'config', '$http', '$localStorage', 'operations', '$filter', 'ngToast', '$cache', 'activityTypes', 'CollaboratorsService', '$window', '$state', '$modal', 'dragularService', '$timeout', '$interval', '$aside',
        function ($rootScope, $scope, guidEmpty, entityTypes, helper, config, $http, $localStorage, operations, $filter, ngToast, $cache, activityTypes, CollaboratorsService, $window, $state, $modal, dragularService, $timeout, $interval, $aside) {
            $scope.collaboratorArray = [];

            $scope.$parent.menuTopTitle = "Organization";
            $scope.$parent.activeMenu = 'organization';
            $scope.$parent.activeMenuItem = 'collaborators';
            var organitzationId = $rootScope.currentOrganization || 1;

            $scope.tabelPagination = {
                currentPage: 1,
                total: 100,
                pageSize: 10
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

            $scope.getCollaborators();

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

            $scope.save = function () {
                if (!$scope.newUserEmail)
                    return false;

                var result = $filter('filter')($scope.collaboratorArray, { email: $scope.newUserEmail }, true)[0];

                if (result)
                    return false;

                var newCol = {};
                newCol.organization_id = organitzationId;
                newCol.role = 'collaborator';
                newCol.email = $scope.newUserEmail;
                newCol.first_name = "";
                newCol.last_name = "";
                newCol.created_at = new Date();

                CollaboratorsService.save(newCol)
                    .then(function (response) {
                        if (response.data) {
                            getToastMsg('Common.Success', 'success');
                            $scope.newUserEmail = "";
                            $scope.getCollaborators();
                            $state.reload();
                        }
                    })
                    .catch(function () {
                        getToastMsg('Common.Error', 'danger');
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
                data.organization_id = organitzationIds;
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