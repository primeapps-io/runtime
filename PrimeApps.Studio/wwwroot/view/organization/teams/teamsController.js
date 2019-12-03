'use strict';

angular.module('primeapps')

    .controller('TeamsController', ['$rootScope', '$scope', '$filter', 'TeamsService', '$state', '$modal', 'ModuleService', '$localStorage',
        function ($rootScope, $scope, $filter, TeamsService, $state, $modal, ModuleService, $localStorage) {
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
                var defaultOrg = $filter('filter')($rootScope.organizations, { default: true }, true)[0];
                window.location.href = '/#/apps?orgId=' + defaultOrg.id;
                return;
            }


            //$scope.$parent.menuTopTitle = "Organization";
            $scope.$parent.activeMenu = 'organization';
            $scope.$parent.activeMenuItem = 'teams';
            $rootScope.breadcrumblist[2].title = "Teams";
            //$scope.requestModel = {
            //    limit: "10",
            //    offset: 0
            //};

            //$scope.generator = function (limit) {
            //    $scope.placeholderArray = [];
            //    for (var i = 0; i < limit; i++) {
            //        $scope.placeholderArray[i] = i;
            //    }

            //};
            //$scope.generator(10);

            //TeamsService.count().then(function (response) {
            //    $scope.$parent.teamCount = response.data;
            //    $scope.pageTotal = response.data;
            //});

            //TeamsService.find($scope.requestModel, $rootScope.currentOrgId).then(function (response) {
            //    $scope.teamArray = response.data;

            //    for (var i = 0; i < $scope.teamArray.length; i++) {
            //        var team = $scope.teamArray[i];
            //        team.organizationName = $filter('filter')($rootScope.organizations, { id: team.organization_id }, true)[0].label;
            //    }
            //    $scope.$parent.teamArray = response.data;
            //    $scope.loading = false;
            //});

            //$scope.changePage = function (page) {
            //    $scope.loading = true;

            //    if (page !== 1) {
            //        var difference = Math.ceil($scope.pageTotal / $scope.requestModel.limit);

            //        if (page > difference) {
            //            if (Math.abs(page - difference) < 1)
            //                --page;
            //            else
            //                page = page - Math.abs(page - Math.ceil($scope.pageTotal / $scope.requestModel.limit))
            //        }
            //    }

            //    var requestModel = angular.copy($scope.requestModel);
            //    requestModel.offset = page - 1;
            //    TeamsService.count().then(function (response) {
            //        if (response.data > 0) {
            //            $scope.$parent.teamCount = response.data;
            //            $scope.pageTotal = response.data;
            //        }
            //    });

            //    TeamsService.find(requestModel, $rootScope.currentOrgId).then(function (response) {
            //        $scope.teamArray = response.data;
            //        for (var i = 0; i < $scope.teamArray.length; i++) {
            //            var team = $scope.teamArray[i];
            //            team.organizationName = $filter('filter')($rootScope.organizations, { id: team.organization_id }, true)[0].label;
            //        }
            //        if ($scope.$parent)
            //            $scope.$parent.teamArray = response.data;

            //        $scope.loading = false;
            //    });
            //};

            //$scope.changeOffset = function () {
            //    $scope.changePage($scope.activePage);
            //};

            $scope.setStep = function (value) {
                $scope.stepNo = value;
            };

            $scope.getOrganizationUserList = function () {
                $scope.loadingMembers = true;
                //$scope.generator(10);

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
                // $scope.generator(10);
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
                if (!addNewTeamForm.$valid) {
                    toastr.error($filter('translate')('Module.RequiredError'));
                    return false;
                }

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
                                            $scope.grid.dataSource.read();
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
                                            toastr.success($filter('translate')('Team.SaveSuccess'));
                                            $scope.clearModels();
                                            $scope.grid.dataSource.read();
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
                                    $scope.grid.dataSource.read();
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
                    $scope.grid.dataSource.read();
                }
            };

            //For Kendo UI
            $scope.goUrl = function (item) {
                var selection = window.getSelection();
                if (selection.toString().length === 0) {
                    $scope.selectTeam(item.id);
                    $scope.addNewTeam(item.id); //click event.
                }
            };

            var accessToken = $localStorage.read('access_token');

            $scope.mainGridOptions = {
                dataSource: {
                    type: "odata-v4",
                    page: 1,
                    pageSize: 10,
                    serverPaging: true,
                    serverFiltering: true,
                    serverSorting: true,
                    transport: {
                        read: {
                            url: "/api/team/find/" + $rootScope.currentOrgId,
                            type: 'GET',
                            dataType: "json",
                            beforeSend: function (req) {
                                req.setRequestHeader('Authorization', 'Bearer ' + accessToken);
                                req.setRequestHeader('X-App-Id', $rootScope.currentAppId);
                                req.setRequestHeader('X-Organization-Id', $rootScope.currentOrgId);
                            }
                        }
                    },
                    schema: {
                        data: "items",
                        total: "count",
                        model: {
                            id: "id",
                            fields: {
                                TeamUsers: { type: "number" }
                            }
                        },
                        parse: function (data) {
                            $scope.teamArray = data.items;
                            return data;
                        }
                    }
                },
                scrollable: false,
                persistSelection: true,
                sortable: true,
                filterable: {
                    extra: false
                },
                rowTemplate: function (e) {
                    var trTemp = '<tr ng-click="goUrl(dataItem)">';
                    trTemp += '<td><div class="user-info"><div class="user-image"><i ng-class="dataItem.icon"></i></div><div class="user-text"><h2 class="ng-binding">' + e.name + '</h2></div></div></td > ';
                    trTemp += e.team_users ? '<td> <span>' + e.team_users.length + '</span></td > ' : '<td> <span>0</span></td > ';
                    trTemp += '<td><span>' + e.organization.label + '</span></td>';
                    trTemp += '<td ng-click="$event.stopPropagation();"> <button ng-click="$event.stopPropagation(); delete(dataItem, $event);" type="button" class="action-button2-delete"><i class="fas fa-trash"></i></button></td></tr>';
                    return trTemp;
                },
                pageable: {
                    refresh: true,
                    pageSize: 10,
                    pageSizes: [10, 25, 50, 100],
                    buttonCount: 5,
                    info: true,
                },
                columns: [
                    {
                        field: 'Name',
                        title: $filter('translate')('Team.Name'),
                    },
                    {
                        field: '',
                        title: $filter('translate')('Team.TeamMember'),
                    },
                    {
                        field: 'Organization.Label',
                        title: $filter('translate')('Team.Organization'),
                    },
                    {
                        field: '',
                        title: '',
                        width: "90px"
                    }]
            };
            //For Kendo UI 
        }
    ]);