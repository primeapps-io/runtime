'use strict';

angular.module('primeapps')

    .controller('UsersController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', '$timeout', 'UsersService',
        function ($rootScope, $scope, $filter, $state, $stateParams, $modal, $timeout, UsersService) {

            $scope.$parent.activeMenuItem = 'users';
            
            $scope.generator = function (limit) {
                $scope.placeholderArray = [];
                for (var i = 0; i < limit; i++) {
                    $scope.placeholderArray[i] = i;
                }
            };

            $scope.generator(10);
            $rootScope.breadcrumblist[2].title = 'Users';
            $scope.roles = [];
            $scope.profiles = [];
            $scope.users = [];
            $scope.loading = true;
            $scope.requestModel = {
                limit: "10",
                offset: 0
            };

            $scope.activePage = 1;

            UsersService.getAllRoles()
                .then(function (response) {
                    $scope.roles = response.data;
                });

            UsersService.getAllProfiles()
                .then(function (response) {
                    $scope.profiles = response.data;
                    $scope.profiles[0].name = 'Administrator';
                    $scope.profiles[1].name = 'Standart';
                });

            UsersService.count()
                .then(function (response) {
                    $scope.pageTotal = response.data;
                    $scope.changePage(1);
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

                $scope.activePage = page;
                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;

                UsersService.find(requestModel)
                    .then(function (response) {
                        $scope.users = response.data;
                        $scope.loading = false;
                    });

            };

            $scope.changeOffset = function () {
                $scope.changePage($scope.activePage, true)
            };

            $scope.delete = function (id, event) {
                var willDelete =
                    swal({
                        title: "Are you sure?",
                        text: " ",
                        icon: "warning",
                        buttons: ['Cancel', 'Yes'],
                        dangerMode: true
                    }).then(function (value) {
                        if (value) {
                            var elem = angular.element(event.srcElement);
                            angular.element(elem.closest('tr')).addClass('animated-background');
                            UsersService.delete(id)
                                .then(function () {
                                    $scope.pageTotal--;

                                    angular.element(document.getElementsByClassName('ng-scope animated-background')).remove();
                                    $scope.changePage($scope.activePage, true);
                                    toastr.success("User is deleted successfully.", "Deleted!");

                                })
                                .catch(function () {
                                    angular.element(document.getElementsByClassName('ng-scope animated-background')).removeClass('animated-background');
                                    toastr.error("User is not deleted successfully.", "Deleted!");
                                });

                        }
                    });
            };

            $scope.save = function (userForm) {
                if (!userForm.$valid) {
                    return;
                }
                $scope.saving = true;

                if (!$scope.editing) {
                    if ($scope.user.auto_pass) {
                        delete $scope.user.password;
                    }

                    //delete $scope.user.auto_pass;
                    $scope.user.created_at = new Date();
                    $scope.user.is_active = true;
                    
                    UsersService.create($scope.user)
                        .then(function (response) {
                            if (response.data) {
                                toastr.success('User is saved successfully');
                                if ($scope.user.auto_pass) {
                                    $scope.showPassword = true;
                                    $scope.autoPassword = response.data.password;
                                }
                                else {
                                    $scope.userFormModal.hide();
                                }
                                $scope.changePage(1);
                                $scope.saving = false;
                                $scope.user = null;
                            }
                        })
                        .catch(function (response) {
                            if (response.data && response.data.message) {
                                toastr.error(response.data.message);
                            }
                            else {
                                toastr.error($filter('translate')('Common.Error'));
                            }
                            $scope.saving = false;
                        });
                }
                else {
                    UsersService.update($scope.user.id, $scope.user)
                        .then(function (response) {
                            toastr.success('User is saved successfully');
                            $scope.userFormModal.hide();
                            $scope.changePage($scope.activePage);
                            $scope.saving = false;
                            $scope.user = null;
                        })
                        .catch(function () {
                            toastr.error($filter('translate')('Common.Error'));
                            $scope.saving = false;
                        });
                }

            };

            $scope.showFormModal = function (id) {
                if (id) {
                    $scope.user = $filter('filter')($scope.users, {id: id}, true)[0];
                    $scope.editing = true;
                }
                else {
                    $scope.user = null;
                    $scope.editing = false;
                }

                $scope.userFormModal = $scope.userFormModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/manage/users/userFormModal.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
                });

                $scope.userFormModal.$promise.then(function () {
                    $scope.userFormModal.show();
                });
            };

            $scope.closeModal = function () {
                $scope.userFormModal.hide();
                $scope.user = null;
                $scope.showPassword = false;
                $scope.autoPassword = null;
            };
        }
    ]);