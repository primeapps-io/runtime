'use strict';

angular.module('primeapps')

    .controller('ScriptsController', ['$rootScope', '$scope', '$state', 'ScriptsService', '$modal',
        function ($rootScope, $scope, $state, ScriptsService, $modal) {
            $scope.$parent.activeMenuItem = 'scripts';
            $rootScope.breadcrumblist[2].title = 'Scripts';
            $scope.scripts = [];
            $scope.scriptModel = {};
            $scope.loading = false;

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

            var count = function () {
                ScriptsService.count().then(function (response) {
                    $scope.pageTotal = response.data;
                });
            };
            count();

            $scope.changePage = function (page) {
                $scope.loading = true;
                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;
                ScriptsService.find(requestModel)
                    .then(function (response) {
                        if (response.data) {
                            $scope.scripts = response.data;
                        }
                        $scope.loading = false;
                    })
                    .catch(function (reason) {
                        $scope.loading = false;
                    });

            };

            $scope.changePage(1);

            $scope.changeOffset = function () {
                $scope.changePage(1)
            };

            var showFormModal = function (script) {
                $scope.createFormModal = $scope.createFormModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/customcode/scripts/sciprtFormModal.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
                });
                $scope.createFormModal.$promise.then(function () {
                    $scope.createFormModal.show();
                });
            };

        }
    ]);