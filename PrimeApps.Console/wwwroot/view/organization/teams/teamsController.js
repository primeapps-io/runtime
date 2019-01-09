'use strict';

angular.module('primeapps')

    .controller('TeamsController', ['$rootScope', '$scope', 'guidEmpty', 'entityTypes', 'helper', 'config', '$http', '$localStorage', 'operations', '$filter', 'ngToast', '$cache', 'activityTypes', 'TeamsService', '$window', '$state', '$modal', 'dragularService', '$timeout', '$interval', '$stateParams',
        function ($rootScope, $scope, guidEmpty, entityTypes, helper, config, $http, $localStorage, operations, $filter, ngToast, $cache, activityTypes, TeamsService, $window, $state, $modal, dragularService, $timeout, $interval, $stateParams) {

            $scope.loading = true;

            $scope.teamArray = [];
            $scope.orgranizationUserArray = [];
            $scope.teamModel = {};
            $scope.teamId;
            var organitzationId = $rootScope.currentOrganization ? $rootScope.currentOrganization.id : 1;


            $scope.$parent.menuTopTitle = "Organization";
            $scope.$parent.activeMenu = 'organization';
            $scope.$parent.activeMenuItem = 'teams';

            $scope.requestModel = {
                limit: "10",
                offset: 0
            };

            TeamsService.count(organitzationId).then(function (response) {
                $scope.pageTotal = response.data;
            });

            TeamsService.find($scope.requestModel, organitzationId).then(function (response) {
                $scope.teamArray = response.data;
                $scope.$parent.teamArray = response.data;
                $scope.loading = false;
            });

            $scope.changePage = function (page) {
                $scope.loading = true;
                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;


                TeamsService.find(requestModel, organitzationId).then(function (response) {
                    $scope.teamArray = response.data;
                    $scope.$parent.teamArray = response.data;
                    $scope.loading = false;
                });

            };

            $scope.changeOffset = function () {
                $scope.changePage(1)
            };

            $scope.getTeamsList = function () {
                TeamsService.getAll()
                    .then(function (response) {
                        if (response.data) {
                            $scope.teamArray = response.data;
                            $scope.$parent.teamArray = response.data;
                        }
                        $scope.loading = false;
                    })
                    .catch(function (error) {
                        getToastMsg('Common.Error', 'danger');
                    });
            }

            $scope.getOrganizationUserList = function () {
                TeamsService.getOrganizationUsers(organitzationId) //TODO Organization ID 
                    .then(function (response) {
                        if (response.data) {
                            var userList = response.data;

                            for (var i = 0; i < $scope.selectedTeam.team_users.length; i++) {
                                userList.users = $filter('filter')(userList.users, function (value) { return value.user_id != $scope.selectedTeam.team_users[i].user_id });
                            }
                            $scope.orgranizationUserArray = angular.copy(userList.users);
                        }
                    })
                    .catch(function (error) {
                        getToastMsg('Common.Error', 'danger');
                    });
            }

            //$scope.getTeamsList();

            $scope.selectTeam = function (id) {
                if (id)
                    $scope.teamId = id;
                $scope.$parent.teamId = id;

                TeamsService.get(id)
                    .then(function (response) {
                        if (response.data)
                            $scope.selectedTeam = response.data;

                        //$scope.$parent.menuTopTitle = $scope.selectedTeam.name;
                        $scope.$parent.activeMenu = "teams";
                        $scope.$parent.activeMenuItem = 'team';
                        $scope.getOrganizationUserList();
                    });
            }

            $scope.selectUserForTeam = function (id) {
                if (!id)
                    return;

                TeamsService.userAddForTeam(id, $scope.selectedTeam)
                    .then(function (response) {
                        if (response.data) {
                            getToastMsg('Common.Success', 'success');
                            $scope.selectTeam($scope.teamId);
                            $scope.getOrganizationUserList();
                            $scope.selectedUser = {};
                        }
                    })
                    .catch(function (error) {
                        getToastMsg('Common.Error', 'danger');
                    });

            }

            $scope.$parent.selectTeam = function (id) {
                if (id)
                    $scope.teamId = id;
                $scope.$parent.teamId = id;

                TeamsService.get(id)
                    .then(function (response) {
                        if (response.data)
                            $scope.selectedTeam = response.data;

                        //$scope.$parent.menuTopTitle = $scope.selectedTeam.name;
                        $scope.$parent.activeMenu = "teams";
                        $scope.$parent.activeMenuItem = 'team';
                        $scope.getOrganizationUserList();
                    });
            }

            $scope.checkNameBlur = function () {
                $scope.nameBlur = true;
                $scope.checkName($scope.teamModel.name);
            };

            $scope.checkName = function (name) {
                if (!$scope.nameBlur)
                    return;

                $scope.nameChecking = true;
                $scope.nameValid = false;

                TeamsService.isUniqueName(name)
                    .then(function (response) {
                        $scope.nameChecking = false;
                        if (response.data) {
                            $scope.nameValid = true;
                        }
                        else {
                            $scope.nameValid = false;
                        }
                    })
                    .catch(function () {
                        $scope.nameValid = false;
                        $scope.nameChecking = false;
                    });
            };

            $scope.save = function (addNewTeamForm) {
                if (!addNewTeamForm.$valid)
                    return false;

                //New add team
                if (!$scope.teamId) {

                    $scope.getTeamsList();

                    var searchTeamName = $filter('filter')($scope.teamArray, { name: $scope.teamModel.name }, true)[0];

                    if (searchTeamName) {
                        addNewTeamForm.name.$invalid = true;
                        getToastMsg('A team with the same name is available.', 'warning');
                        return false;
                    }

                    TeamsService.create($scope.teamModel)
                        .then(function (response) {
                            if (response.data) {
                                getToastMsg('Team created successfully', 'success');
                                $scope.clearModels();
                                $scope.getTeamsList();
                            }
                        })
                        .catch(function (error) {
                            getToastMsg('Common.Error', 'danger');
                            return false;
                        });
                    ;
                }
                else { //Edit team
                    TeamsService.update($scope.teamId, $scope.teamModel)
                        .then(function (response) {
                            if (response.data) {
                                getToastMsg('Common.Success', 'success');
                                $scope.clearModels();
                                $scope.getTeamsList();
                            }
                        })
                        .catch(function (error) {
                            getToastMsg('Common.Error', 'danger');
                        });
                }
            }

            $scope.delete = function (id) {
                if (!id)
                    return false;

                TeamsService.delete(id)
                    .then(function (response) {
                        if (response.data) {
                            getToastMsg('Team deleted successfully', 'success');
                            $scope.getTeamsList();
                            $scope.teamId = 0;
                            $scope.$parent.teamId = 0;
                            $scope.addNewTeamFormModal.hide();
                            $scope.teamModel = {};
                            $state.reload();
                        }
                    })
                    .catch(function (result) {
                        getToastMsg('Common.Error', 'danger');
                    });
            }

            $scope.deleteUser = function (id) {
                if (!id)
                    return false;

                TeamsService.deleteUser(id, $scope.selectedTeam)
                    .then(function (response) {
                        if (response) {
                            getToastMsg('User of team deleted successfully', 'success');
                            $scope.selectTeam($scope.teamId);
                            $scope.getOrganizationUserList();
                            $scope.selectedUser = {};
                        }
                    })
                    .catch(function (error) {
                        getToastMsg('Common.Error', 'danger');
                    });
            }

            $scope.addNewTeam = function (id) {
                if (id) {
                    $scope.teamId = id;
                    var findTeam = $filter('filter')($scope.teamArray, { id: id }, true)[0];
                    $scope.teamModel.name = findTeam.name;
                    $scope.teamModel.icon = findTeam.icon;
                }

                $scope.addNewTeamFormModal = $scope.addNewTeamFormModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/organization/teams/addNewTeamForm.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
                });

                $scope.addNewTeamFormModal.$promise.then(function () {
                    $scope.addNewTeamFormModal.show();
                });

            };

            $scope.menuItemClass = function (menuItem) {
                if ($rootScope.selectedSetupMenuLink === menuItem.link) {
                    return 'active';
                } else {
                    return '';
                }

            };

            $scope.clearModels = function () {
                $scope.addNewTeamFormModal.hide();
                $scope.teamModel = {};
            };

            var getToastMsg = function (msg, type = 'success') {
                ngToast.create({
                    content: $filter('translate')(msg),
                    className: type
                });
            }
        }
    ]);