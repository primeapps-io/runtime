'use strict';

angular.module('primeapps')

    .controller('ReleaseController', ['$rootScope', '$scope', '$state', 'PackageService', '$timeout', '$sce',
        function ($rootScope, $scope, $state, PackageService, $timeout, $sce) {
            $scope.loading = true;
            $scope.activePage = 1;

            $scope.$parent.activeMenu = 'app';
            $scope.$parent.activeMenuItem = 'packages';
            $rootScope.breadcrumblist[2].title = 'Packages';

            $scope.generator = function (limit) {
                $scope.placeholderArray = [];
                for (var i = 0; i < limit; i++) {
                    $scope.placeholderArray[i] = i;
                }
            };

            $scope.$on('package-created', function (event, args) {
                $scope.reload();
            });

            $scope.generator(10);

            $scope.requestModel = {
                limit: "10",
                offset: 0,
                order_column: 'version',
                order_type: 'desc'
            };

            $scope.reload = function () {
                PackageService.count()
                    .then(function (response) {
                        $scope.pageTotal = response.data;

                        if ($scope.requestModel.offset != 0 && ($scope.requestModel.offset * $scope.requestModel.limit) >= $scope.pageTotal) {
                            $scope.requestModel.offset = $scope.requestModel.offset - 1;
                        }

                        PackageService.find($scope.requestModel)
                            .then(function (response) {
                                $scope.packages = response.data;
                                $scope.loading = false;
                            });
                    });
            };

            $scope.reload();

            $scope.changePage = function (page) {
                $scope.loading = true;
                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;

                $scope.page = requestModel.offset + 1;
                PackageService.find(requestModel)
                    .then(function (response) {
                        $scope.functions = response.data;
                        $scope.loading = false;
                    });
            };

            $scope.changeOffset = function () {
                $scope.changePage($scope.activePage);
            };

            $scope.getTime = function (time) {
                return moment(time).format("DD-MM-YYYY HH:ss");
            };

            $scope.asHtml = function () {
                return $sce.trustAsHtml($rootScope.goLive ? $rootScope.goLive.logs : '');
            };

            $scope.getIcon = function (status) {
                switch (status) {
                    case 'running':
                        return $sce.trustAsHtml('<i style="color:#0d6faa;" class="fas fa-clock"></i>');
                    case 'failed':
                        return $sce.trustAsHtml('<i style="color:rgba(218,10,0,1);" class="fas fa-times"></i>');
                    case 'succeed':
                        return $sce.trustAsHtml('<i style="color:rgba(16,124,16,1);" class="fas fa-check"></i>');
                }
            };
        }
    ]);