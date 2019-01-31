'use strict';

angular.module('primeapps')

    .controller('TeamsController', ['$rootScope', '$scope', 'guidEmpty', 'entityTypes', 'helper', 'config', '$http', '$localStorage', 'operations', '$filter', '$cache', 'activityTypes', 'TeamsService', '$window', '$state', '$modal', 'dragularService', '$timeout', '$interval', '$stateParams',
        function ($rootScope, $scope, guidEmpty, entityTypes, helper, config, $http, $localStorage, operations, $filter, $cache, activityTypes, TeamsService, $window, $state, $modal, dragularService, $timeout, $interval, $stateParams) {

            $scope.loading = true;

            $scope.teamArray = [];
            $scope.orgranizationUserArray = [];
            $scope.teamModel = {};
            $scope.teamId;

            $scope.$parent.menuTopTitle = "Organization";
            $scope.$parent.activeMenu = 'organization';
            $scope.$parent.activeMenuItem = 'teams';

            $scope.requestModel = {
                limit: "10",
                offset: 0
            };

            toastr.options = {
                "closeButton": true,
                "debug": false,
                "newestOnTop": false,
                "progressBar": true,
                "positionClass": "toast-top-right",
                "preventDuplicates": false,
                "showDuration": "300",
                "hideDuration": "1000",
                "timeOut": "5000",
                "extendedTimeOut": "1000",
                "showEasing": "swing",
                "hideEasing": "linear",
                "showMethod": "fadeIn",
                "hideMethod": "fadeOut"
            }
            toastr.success('We do have the Kapua suite available.', 'Turtle Bay Resort', { timeOut: 5000 })

            $scope.generator = function (limit) {
                $scope.placeholderArray = [];
                for (var i = 0; i < limit; i++) {
                    $scope.placeholderArray[i] = i;
                }

            };
            $scope.generator(10);

            TeamsService.count($rootScope.currentOrgId).then(function (response) {
                $scope.$parent.teamCount = response.data;
                $scope.pageTotal = response.data;
            });

            TeamsService.find($scope.requestModel, $rootScope.currentOrgId).then(function (response) {
                $scope.teamArray = response.data;

                for (var i = 0; i < $scope.teamArray.length; i++) {
                    var team = $scope.teamArray[i];
                    team.organizationName = $filter('filter')($rootScope.organizations, { id: team.organization_id }, true)[0].label;
                }
                $scope.$parent.teamArray = response.data;
                $scope.loading = false;
            });

            $scope.changePage = function (page) {
                $scope.loading = true;
                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;
                TeamsService.count($rootScope.currentOrgId).then(function (response) {
                    $scope.$parent.teamCount = response.data;
                    $scope.pageTotal = response.data;
                });

                TeamsService.find(requestModel, $rootScope.currentOrgId).then(function (response) {
                    $scope.teamArray = response.data;
                    for (var i = 0; i < $scope.teamArray.length; i++) {
                        var team = $scope.teamArray[i];
                        team.organizationName = $filter('filter')($rootScope.organizations, { id: team.organization_id }, true)[0].label;
                    }
                    $scope.$parent.teamArray = response.data;
                    $scope.loading = false;
                });

            };

            $scope.changeOffset = function () {
                $scope.changePage(1);
            };

            $scope.getOrganizationUserList = function () {
                $scope.loadingMembers = true;
                $scope.generator(10);

                TeamsService.getOrganizationUsers($rootScope.currentOrgId) //TODO Organization ID
                    .then(function (response) {
                        if (response.data) {
                            var userList = response.data;

                            for (var i = 0; i < $scope.selectedTeam.team_users.length; i++) {
                                userList.users = $filter('filter')(userList.users, function (value) {
                                    return value.user_id != $scope.selectedTeam.team_users[i].user_id
                                });
                            }
                            $scope.orgranizationUserArray = angular.copy(userList.users);
                            $scope.loadingMembers = false;
                        }
                    })
                    .catch(function (error) {
                        swal($filter('translate')('Common.Error'), "error");
                    });
            }

            //$scope.getTeamsList();

            $scope.selectTeam = function (id) {
                if (id)
                    $scope.teamId = id;

                $scope.$parent.teamId = id;
                $scope.loadingMembers = true;
                $scope.generator(10);
                TeamsService.get(id)
                    .then(function (response) {
                        if (response.data) {
                            $scope.selectedTeam = response.data;
                            $scope.teamOrganizationName = $filter('filter')($rootScope.organizations, { id: $scope.selectedTeam.organization_id }, true)[0].label;
                        }

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
                            swal('Collaborator is added successfully', "success");
                            $scope.selectTeam($scope.teamId);
                            $scope.getOrganizationUserList();
                            $scope.selectedUser = {};
                        }
                    })
                    .catch(function (error) {
                        swal($filter('translate')('Common.Error'), "error");
                    });

            }

            $scope.$parent.selectTeam = function (id) {
                if (id)
                    $scope.teamId = id;

                $scope.$parent.teamId = id;
                $scope.loadingTeamMembers = true;

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

                $scope.submitting = true;

                TeamsService.isUniqueName($scope.teamModel.name)
                    .then(function (response) {
                        $scope.nameChecking = false;
                        if (response.data) {
                            $scope.nameValid = true;
                            if (!$scope.teamId) {
                                TeamsService.create($scope.teamModel)
                                    .then(function (response) {
                                        if (response.data) {
                                            $scope.submitting = false;
                                            swal('Team created successfully', "success");
                                            $scope.clearModels();
                                            $scope.changePage(1);
                                        }
                                    })
                                    .catch(function (error) {
                                        swal($filter('translate')('Common.Error'), "error");
                                        return false;
                                    });
                            }
                            else { //Edit team
                                TeamsService.update($scope.teamId, $scope.teamModel)
                                    .then(function (response) {
                                        if (response.data) {
                                            swal($filter('translate')('Common.Success'), "success");
                                            $scope.clearModels();
                                            $scope.changePage(1);
                                        }
                                    })
                                    .catch(function (error) {
                                        swal($filter('translate')('Common.Error'), "error");
                                    });
                            }
                        }
                        else {
                            addNewTeamForm.name.$invalid = true;
                            $scope.submitting = false;
                            swal('A team with the same name is available.', "warning");
                            return false;
                        }
                    })
                    .catch(function () {
                        $scope.nameValid = false;
                        $scope.nameChecking = false;
                        $scope.submitting = false;
                    });
            };

            $scope.delete = function (id) {
                swal({
                    title: "Are you sure?",
                    text: "Are you sure that you want to delete this team?",
                    icon: "warning",
                    buttons: ['Cancel', 'Yes'],
                    dangerMode: true
                }).then(function (value) {
                    if (value) {
                        if (!id)
                            return false;

                        TeamsService.delete(id)
                            .then(function (response) {
                                if (response.data) {
                                    $scope.changePage(1);
                                    $scope.teamId = 0;
                                    $scope.$parent.teamId = 0;
                                    $scope.addNewTeamFormModal.hide();
                                    $scope.teamModel = {};
                                    $state.reload();
                                    swal("Deleted!", "Team is deleted successfully.", "success");
                                }
                            })
                            .catch(function (result) {
                                swal($filter('translate')('Common.Error'), "error");
                            });
                    }
                });
            };

            $scope.deleteUser = function (id) {
                swal({
                    title: "Are you sure?",
                    text: "Are you sure that you want to delete this member?",
                    icon: "warning",
                    buttons: ['Cancel', 'Yes'],
                    dangerMode: true
                }).then(function (value) {
                    if (value) {
                        if (!id)
                            return false;

                        TeamsService.deleteUser(id, $scope.selectedTeam)
                            .then(function (response) {
                                if (response) {
                                    $scope.selectTeam($scope.teamId);
                                    $scope.getOrganizationUserList();
                                    $scope.selectedUser = {};
                                    swal("Deleted!", "Member is deleted successfully.", "success");
                                }
                            })
                            .catch(function (error) {
                                swal($filter('translate')('Common.Error'), "error");
                            });
                    }
                })
            }

            $scope.addNewTeam = function (id) {
                if (id) {
                    $scope.editForm = true;
                    $scope.teamId = id;
                    var findTeam = $filter('filter')($scope.teamArray, { id: id }, true)[0];
                    $scope.teamModel.name = findTeam.name;
                    $scope.teamModel.icon = findTeam.icon;
                } else {
                    $scope.editForm = false;
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
        }
    ]);