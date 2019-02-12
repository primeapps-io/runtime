'use strict';

angular.module('primeapps')

    .controller('FunctionsDeploymentController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', '$timeout', 'helper', 'dragularService', 'FunctionsDeploymentService', '$sce', '$http', 'config',
        function ($rootScope, $scope, $filter, $state, $stateParams, $modal, $timeout, helper, dragularService, FunctionsDeploymentService, $sce, $http, config) {

            //$rootScope.modules = $http.get(config.apiUrl + 'module/get_all');

            //$scope.$parent.activeMenu = 'app';
            $scope.$parent.activeMenuItem = 'functionsDeployment';
            $rootScope.breadcrumblist[2].title = 'Functions Deployments';

            $scope.deployments = [];
            $scope.loading = true;

            $scope.generator = function (limit) {
                $scope.placeholderArray = [];
                for (var i = 0; i < limit; i++) {
                    $scope.placeholderArray[i] = i;
                }
            };

            $scope.generator(10);

            $scope.requestModel = {
                limit: "10",
                offset: 0
            };

            $scope.reload = function () {
                FunctionsDeploymentService.count()
                    .then(function (response) {
                        $scope.pageTotal = response.data;

                        if ($scope.requestModel.offset != 0 && ($scope.requestModel.offset * $scope.requestModel.limit) >= $scope.pageTotal) {
                            $scope.requestModel.offset = $scope.requestModel.offset - 1;
                        }

                        FunctionsDeploymentService.find($scope.requestModel)
                            .then(function (response) {
                                $scope.deployments = response.data;
                                $scope.loading = false;
                            });
                    });
            };

            $scope.reload();

            $scope.changePage = function (page) {
                $scope.loading = true;
                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;
                FunctionsDeploymentService.find(requestModel)
                    .then(function (response) {
                        $scope.deployments = response.data;
                        $scope.loading = false;
                    });

            };

            $scope.getTime = function (time) {
                return moment(time).format("DD-MM-YYYY HH:ss");
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

            $scope.changeOffset = function () {
                $scope.changePage(1)
            };

        }
    ]);