'use strict';

angular.module('primeapps')

    .controller('TeamsController', ['$rootScope', '$scope', '$filter', 'TeamsService', '$state', '$modal', 'ModuleService',
        function ($rootScope, $scope, $filter, TeamsService, $state, $modal, ModuleService) {
            $scope.loading = true;
            $scope.activePage = 1;
            $scope.stepNo = 0;
            $scope.teamArray = [];
            $scope.orgranizationUserArray = [];
            $scope.teamModel = {};
            $scope.selectedUser = {};
            $scope.teamId;
            $scope.icons = ModuleService.getIcons(2);
            $scope.editForm = false;

            if ($rootScope.currentOrganization.role != 'administrator') {
                toastr.warning($filter('translate')('Common.Forbidden'));
                $state.go('studio.allApps');
                return;
            }


            //$scope.$parent.menuTopTitle = "Organization";
            $scope.$parent.activeMenu = 'organization';
            $scope.$parent.activeMenuItem = 'teams';
            $rootScope.breadcrumblist[2].title = "Teams";
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

            TeamsService.count().then(function (response) {
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

                if (page !== 1) {
                    var difference = Math.ceil($scope.pageTotal / $scope.requestModel.limit);

                    if (page > difference) {
                        if (Math.abs(page - difference) < 1)
                            --page;
                        else
                            page = page - Math.abs(page - Math.ceil($scope.pageTotal / $scope.requestModel.limit))
                    }
                }

                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;
                TeamsService.count().then(function (response) {
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
                $scope.changePage($scope.activePage);
            };

            $scope.setStep = function (value) {
                $scope.stepNo = value;
            };

            $scope.getOrganizationUserList = function () {
                $scope.loadingMembers = true;
                $scope.generator(10);

                TeamsService.getOrganizationUsers($rootScope.currentOrgId)
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
                        toastr.error($filter('translate')('Common.Error'));
                    });
            }

            //$scope.getTeamsList();

            $scope.selectTeam = function (id) {
                if (id)
                    $scope.teamId = id;

                $scope.loadingMembers = true;
                $scope.generator(10);
                TeamsService.get(id)
                    .then(function (response) {
                        if (response.data) {
                            $scope.selectedTeam = response.data;
                            $scope.teamOrganizationName = $filter('filter')($rootScope.organizations, { id: $scope.selectedTeam.organization_id }, true)[0].label;
                        }

                        $scope.getOrganizationUserList();
                    });
            }

            $scope.selectUserForTeam = function (id) {
                if (!id)
                    return;

                TeamsService.userAddForTeam(id, $scope.selectedTeam)
                    .then(function (response) {
                        if (response.data) {
                            toastr.success('Collaborator is added successfully');
                            $scope.selectTeam($scope.teamId);
                            $scope.selectedUser = {};
                        }
                    })
                    .catch(function (error) {
                        toastr.error($filter('translate')('Common.Error'));
                    });

            }

            $scope.checkNameBlur = function () {
                $scope.nameBlur = true;
                $scope.checkName($scope.teamModel);
            };

            $scope.checkName = function (team) {
                if (!$scope.nameBlur)
                    return;

                $scope.nameChecking = true;
                $scope.nameValid = false;

                TeamsService.isUniqueName(team)
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

                var request = {
                    id: $scope.teamId ? $scope.teamId : 0,
                    name: $scope.teamModel.name,
                };

                if (angular.isObject($scope.teamModel.icon))
                    request.icon = $scope.teamModel.icon.value;
                else
                    request.icon = $scope.teamModel.icon;

                TeamsService.isUniqueName(request)
                    .then(function (response) {
                        $scope.nameChecking = false;
                        if (response.data) {
                            $scope.nameValid = true;
                            if (!$scope.teamId) {
                                TeamsService.create(request)
                                    .then(function (response) {
                                        if (response.data) {
                                            $scope.submitting = false;
                                            toastr.success('Team created successfully');
                                            $scope.clearModels();
                                            $scope.changeOffset();
                                        }
                                    })
                                    .catch(function (error) {
                                        toastr.error($filter('translate')('Common.Error'));
                                        $scope.submitting = false;
                                        return false;
                                    });
                            }
                            else { //Edit team
                                TeamsService.update($scope.teamId, request)
                                    .then(function (response) {
                                        if (response.data >= 0) {
                                            toastr.success($filter('translate')('Common.Success'));
                                            $scope.clearModels();
                                            $scope.changeOffset();
                                            $scope.submitting = false;
                                        }
                                    })
                                    .catch(function (error) {
                                        $scope.submitting = false;
                                        toastr.error($filter('translate')('Common.Error'));
                                    });
                            }
                        }
                        else {
                            addNewTeamForm.name.$invalid = true;
                            $scope.submitting = false;
                            toastr.warning('A team with the same name is available.');
                            return false;
                        }
                    })
                    .catch(function () {
                        $scope.nameValid = false;
                        $scope.nameChecking = false;
                        $scope.submitting = false;
                    });
            };

            $scope.delete = function (team) {
                team.deleting = true;

                swal({
                    title: "Are you sure?",
                    text: " ",
                    icon: "warning",
                    buttons: ['Cancel', 'Yes'],
                    dangerMode: true
                }).then(function (value) {
                    if (value) {
                        if (!team.id) {
                            team.deleting = false;
                            return false;
                        }
                        TeamsService.delete(team.id)
                            .then(function (response) {
                                if (response.data) {
                                    $scope.changeOffset();
                                    $scope.teamId = null;
                                    $scope.teamModel = {};
                                    toastr.success("Team is deleted successfully.", "Deleted!");
                                }
                                team.deleting = false;
                            })
                            .catch(function (result) {
                                toastr.error($filter('translate')('Common.Error'));
                                team.deleting = false;
                            });
                    }
                    else
                        team.deleting = false;
                });
            };

            $scope.deleteUser = function (user) {
                user.deleting = true;
                swal({
                    title: "Are you sure?",
                    text: " ",
                    icon: "warning",
                    buttons: ['Cancel', 'Yes'],
                    dangerMode: true
                }).then(function (value) {
                    if (value) {
                        if (!user.user_id) {
                            user.deleting = false;
                            return false;
                        }

                        TeamsService.deleteUser(user.user_id, $scope.selectedTeam)
                            .then(function (response) {
                                if (response) {
                                    $scope.selectTeam($scope.teamId);
                                    $scope.selectedUser = {};
                                    toastr.success("Member is deleted successfully.", "Deleted!");
                                }
                                user.deleting = false;
                            })
                            .catch(function (error) {
                                toastr.error($filter('translate')('Common.Error'));
                                user.deleting = false;
                            });
                    }
                    else
                        user.deleting = false;
                })
            }

            $scope.addNewTeam = function (id) {
                $scope.teamModel = {};
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
                $scope.teamId = null;
            };

            $scope.cancel = function () {
                if ($scope.addNewTeamFormModal) {
                    $scope.addNewTeamFormModal.hide();
                    $scope.teamModel = {}; 
                    $scope.teamId = null;
                    $scope.stepNo = 0;
                }
            };

        }
    ]);