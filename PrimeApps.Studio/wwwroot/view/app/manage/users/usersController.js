'use strict';

angular.module('primeapps')

    .controller('UsersController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', '$timeout', 'UsersService',
        function ($rootScope, $scope, $filter, $state, $stateParams, $modal, $timeout, UsersService) {
            $scope.$parent.activeMenuItem = 'users';
            $rootScope.breadcrumblist[2].title = 'Users';
            $scope.loading = true;
            $scope.userModel = {};

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

            UsersService.count()
                .then(function (response) {
                    $scope.pageTotal = response.data;
                });

            UsersService.find($scope.requestModel).then(function (response) {
                $scope.users = response.data;
                $scope.loading = false;
            });


            $scope.changePage = function (page) {
                $scope.loading = true;
                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;

                CollaboratorsService.count().then(function (response) {
                    $scope.pageTotal = response.data;
                });

                CollaboratorsService.find(requestModel).then(function (response) {
                    if (response.data) {
                        $scope.users = response.data;
                    }
                    $scope.loading = false;
                });

                $scope.changeOffset = function (value) {
                    $scope.changePage(value);
                };
            };
        }
    ]);