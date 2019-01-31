'use strict';

angular.module('primeapps')

    .controller('ExportDataController', ['$rootScope', '$scope', '$filter', 'ngToast', '$window',
        function ($rootScope, $scope, $filter, ngToast, $window) {

            $scope.getDownloadViewUrlExcel = function () {
                var module = $scope.module.name;
                var viewId = $scope.view.id;
                var profileId = $rootScope.user.profile.ID;
                var isViewFields = $scope.export.moduleAllColumn;
                if (isViewFields)
                    $window.open("/attach/export_excel_view?module=" + module + "&viewId=" + viewId + "&profileId=" + profileId + '&listFindRequestJson=' + JSON.stringify($scope.findRequest) + '&isViewFields=' + false + '&locale=' + $rootScope.locale, "_blank");
                else
                    $window.open("/attach/export_excel_view?module=" + module + "&viewId=" + viewId + "&profileId=" + profileId + '&listFindRequestJson=' + JSON.stringify($scope.findRequest) + '&isViewFields=' + true + '&locale=' + $rootScope.locale, "_blank");
                ngToast.create({ content: $filter('translate')('Module.ExcelDesktop'), className: 'success' });
            };

            $scope.excelNoData = function () {
                var module = $scope.module.name;
                var templateId = $scope.quoteTemplate.id;
                var templateName = $scope.quoteTemplate.name;
                var viewId = $scope.view.id;
                $window.open("/attach/export_excel_no_data?module=" + module + "&viewId=" + viewId + "&templateId=" + templateId + "&templateName=" + templateName + '&locale=' + $rootScope.locale + '&listFindRequestJson=' + JSON.stringify($scope.findRequest), "_blank");
                ngToast.create({ content: $filter('translate')('Module.ExcelDesktop'), className: 'success' });
            };

            $scope.excelData = function () {
                var module = $scope.module.name;
                var templateId = $scope.quoteTemplate.id;
                var templateName = $scope.quoteTemplate.name;
                var viewId = $scope.view.id;
                $window.open("/attach/export_excel_data?module=" + module + "&viewId=" + viewId + "&templateId=" + templateId + "&templateName=" + templateName + '&locale=' + $rootScope.locale + '&listFindRequestJson=' + JSON.stringify($scope.findRequest), "_blank");
                ngToast.create({ content: $filter('translate')('Module.ExcelDesktop'), className: 'success' });
            };
        }
    ]);