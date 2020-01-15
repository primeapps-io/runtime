'use strict';

angular.module('primeapps')

    .controller('ReleaseDetailController', ['$rootScope', '$scope', '$state', 'PackageService', '$location', '$filter', '$sce',
        function ($rootScope, $scope, $state, PackageService, $location, $filter, $sce) {

            $scope.loading = true;
            $scope.releaseId = $location.search().id;

            $scope.log = "";

            if (!$scope.releaseId) {
                toastr.warning("Release not found.");

                $state.go('studio.app.packages', {
                    orgId: $rootScope.currentOrgId,
                    appId: $rootScope.currentAppId
                });
            }

            PackageService.get($scope.releaseId)
                .then(function (response) {
                    $scope.release = response.data;
                    if (response.data && response.data.status === 'running') {
                        $scope.releaseLog();
                    }
                    else {
                        PackageService.log($scope.releaseId)
                            .then(function (response) {
                                $scope.log = $sce.trustAsHtml(response.data);
                                $scope.loading = false;
                            })
                            .catch(function () {
                                toastr.error($filter('translate')('Common.Error'));
                                $scope.loading = false;
                            })
                    }
                })
                .catch(function () {
                    toastr.error($filter('translate')('Common.Error'));
                    $scope.loading = false;
                });

            

            $scope.downloadPackage = function () {

            };
        }
    ]);