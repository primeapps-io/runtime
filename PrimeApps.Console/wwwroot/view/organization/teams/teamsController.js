'use strict';

angular.module('primeapps')

    .controller('TeamsController', ['$rootScope', '$scope', 'guidEmpty', 'entityTypes', 'helper', 'config', '$http', '$localStorage', 'operations', '$filter', 'ngToast', '$cache', 'activityTypes', 'TeamsService', '$window', '$state', '$modal', 'dragularService', '$timeout', '$interval', '$aside',
        function ($rootScope, $scope, guidEmpty, entityTypes, helper, config, $http, $localStorage, operations, $filter, ngToast, $cache, activityTypes, TeamsService, $window, $state, $modal, dragularService, $timeout, $interval, $aside) {
            $scope.teamArray = [];
            $scope.teamModel = {};
            $scope.teamId;

            $scope.$parent.menuTopTitle ="Organization";
            $scope.$parent.activeMenu= 'organization';
            $scope.$parent.activeMenuItem = 'teams';

            $scope.getTeamsList = function () {
                TeamsService.getAll()
                    .then(function (response) {
                        if (response.data)
                            $scope.teamArray = response.data;
                    })
                    .catch(function (error) {
                        getToastMsg('Common.Error', 'error');
                    });
            }

            $scope.getTeamsList();


            $scope.save = function (addNewTeamForm) {
                if (!addNewTeamForm.$valid)
                    return false;

                //New add team
                if (!$scope.teamId) {

                    $scope.teamModel.organization_id = 1; //TODO get Organization ID

                    $scope.getTeamsList();

                    var searchTeamName = $filter('filter')($scope.teamArray, { name: $scope.teamModel.name }, true)[0];

                    if (searchTeamName) {
                        getToastMsg('A team with the same name is available.', 'warning');
                        return false;
                    }

                    TeamsService.create($scope.teamModel)
                        .then(function (response) {
                            if (response.data) {
                                getToastMsg('Common.Success', 'success');
                                return true;
                            }
                        })
                        .catch(function (error) {
                            getToastMsg('Common.Error', 'error');
                            return false;
                        });;
                }
                else { //Edit team
                    TeamsService.update($scope.teamModel.id, $scope.teamModel)
                        .then(function (response) {
                            if (response.data) {
                                getToastMsg('Common.Success', 'success');
                                $scope.getTeamsList();
                                return true;
                            }
                        })
                        .catch(function (error) {
                            getToastMsg('Common.Error', 'error');
                        });
                }
            }

            $scope.addNewTeam = function (id) {
                if (id) {
                    $scope.teamId = id;
                    var findTeam = $filter('filter')($scope.teamArray, { name: $scope.teamModel.name }, true)[0];
                    $scope.teamModel.name = findTeam.name;
                    $scope.teamModel.icon = findTeam.icon;
                    $scope.teamModel.organization_id = findTeam.organization_id;
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

            var getToastMsg = function (msg, type = 'success') {
                ngToast.create({
                    content: $filter('translate')(msg),
                    className: type
                });
            }
        }
    ]);