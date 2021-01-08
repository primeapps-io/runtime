'use strict';

angular.module('primeapps')

    .controller('ExportDataController', ['$rootScope', '$scope', '$filter', '$window', '$mdDialog', 'mdToast',
        function ($rootScope, $scope, $filter, $window, $mdDialog, mdToast) {

            $scope.getDownloadViewUrlExcel = function () {
                var module = $scope.module.name;
                var viewId = $scope.activeView.id;
                var profileId = $rootScope.user.profile.id;
                var isViewFields = $scope.export.moduleAllColumn;
                if (!$rootScope.preview) {
                    if (isViewFields)
                        $window.open("/attach/export_excel_view?module=" + module + "&viewId=" + viewId + "&profileId=" + profileId + '&listFindRequestJson=' + JSON.stringify($scope.findRequest) + '&isViewFields=' + false + '&locale=' + $rootScope.locale, "_blank");
                    else
                        $window.open("/attach/export_excel_view?module=" + module + "&viewId=" + viewId + "&profileId=" + profileId + '&listFindRequestJson=' + JSON.stringify($scope.findRequest) + '&isViewFields=' + true + '&locale=' + $rootScope.locale, "_blank");
                }
                else {
                    if (isViewFields)
                        $window.open("/attach/export_excel_view?module=" + module + "&viewId=" + viewId + "&profileId=" + profileId + '&listFindRequestJson=' + JSON.stringify($scope.findRequest) + '&isViewFields=' + false + '&locale=' + $rootScope.locale + '&appId=' + $rootScope.user.app_id, "_blank");
                    else
                        $window.open("/attach/export_excel_view?module=" + module + "&viewId=" + viewId + "&profileId=" + profileId + '&listFindRequestJson=' + JSON.stringify($scope.findRequest) + '&isViewFields=' + true + '&locale=' + $rootScope.locale + '&appId=' + $rootScope.user.app_id, "_blank");
                }

                mdToast.success($filter('translate')('Module.ExcelDesktop'));
            };

            $scope.excelNoData = function () {
                var module = $scope.module.name;
                var templateId = $scope.quoteTemplate.id;
                var templateName = $scope.quoteTemplate.name;
                var viewId = $rootScope.activeView.id;
                $window.open("/attach/export_excel_no_data?module=" + module + "&viewId=" + viewId + "&templateId=" + templateId + "&templateName=" + templateName + '&locale=' + $rootScope.locale + '&listFindRequestJson=' + JSON.stringify($scope.findRequest), "_blank");
                mdToast.success($filter('translate')('Module.ExcelDesktop'));
            };

            $scope.excelData = function () {
                var module = $scope.module.name;
                var templateId = $scope.quoteTemplate.id;
                var templateName = $scope.quoteTemplate.name;
                var viewId = $rootScope.activeView.id;
                $window.open("/attach/export_excel_data?module=" + module + "&viewId=" + viewId + "&templateId=" + templateId + "&templateName=" + templateName + '&locale=' + $rootScope.locale + '&listFindRequestJson=' + JSON.stringify($scope.findRequest), "_blank");
                mdToast.success($filter('translate')('Module.ExcelDesktop'));
            };
            
            $scope.cancel = function () {
                $mdDialog.cancel();
            };

            $scope.quoteTemplatesOptions = {
                dataSource: $filter('filter')($scope.quoteTemplates,{active: true, isShown : true}),
                dataTextField: "name",
                dataValueField: "id",
            };
            
        }
    ]);