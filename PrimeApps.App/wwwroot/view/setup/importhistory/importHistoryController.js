'use strict';

angular.module('primeapps')
    .controller('ImportHistoryController', ['$rootScope', '$scope', '$cache', 'helper', 'ImportHistoryService',
        function ($rootScope, $scope, $cache, helper, ImportHistoryService) {
            $scope.currentPage = 1;
            $scope.importHistoryFilter = {};

            $scope.find = function () {
                $scope.allImportsLoaded = false;

                if (!helper.hasAdminRights())
                    $scope.importHistoryFilter.userId = $rootScope.user.id;

                var request = {
                    limit: 30,
                    offset: ($scope.currentPage - 1) * 30,
                    module_id: $scope.importHistoryFilter.moduleId,
                    user_id: $scope.importHistoryFilter.userId
                };

                ImportHistoryService.find(request)
                    .then(function (imports) {
                        imports = imports.data;
                        $scope.pagingIcon = 'fa-chevron-right';

                        angular.forEach(imports, function (impt) {
                            var excelUrl = decodeURIComponent(impt.excel_url);
                            impt.file_name = excelUrl.slice(excelUrl.indexOf('--') + 2);
                        });

                        if ($scope.currentPage === 1)
                            $scope.imports = imports;
                        else
                            $scope.imports = $scope.imports.concat(imports);

                        if (imports.length < 1 || imports.length < request.limit)
                            $scope.allImportsLoaded = true;

                        $scope.searching = false;
                    });
            };

            $scope.loadMore = function () {
                if ($scope.allImportsLoaded)
                    return;

                $scope.pagingIcon = 'fa-spinner fa-spin';
                $scope.currentPage = $scope.currentPage + 1;
                $scope.find();
            };

            $scope.cancel = function () {
                $scope.showFilter = false;
                $scope.showFilterButton = true;

                if ($scope.importHistoryFilter) {
                    $scope.currentPage = 1;
                    $scope.importHistoryFilter = {};
                    $scope.find();
                }
            };

            $scope.find();

            $scope.revert = function (imprt) {
                ImportHistoryService.revert(imprt.id)
                    .then(function () {
                        $scope.currentPage = 1;
                        $scope.find();

                        var cacheKey = imprt.module.name + '_' + imprt.module.name;
                        $cache.remove(cacheKey);
                    });
            }
        }
    ]);