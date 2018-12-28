'use strict';

angular.module('primeapps')

    .controller('ExcelTemplatesController', ['$rootScope', '$scope', '$state', '$stateParams', '$location', 'ngToast', '$filter', '$cache', '$q', 'helper', 'dragularService', 'operators', 'ExcelTemplatesService', '$http', 'config', '$modal',
        function ($rootScope, $scope, $state, $stateParams, $location, ngToast, $filter, $cache, $q, helper, dragularService, operators, ExcelTemplatesService, $http, config, $modal) {

            $scope.$parent.menuTopTitle = "Templates";
            $scope.$parent.activeMenu = 'templates';
            $scope.$parent.activeMenuItem = 'templatesExcel';


            $scope.showFormModal = function () {

                $scope.addNewExcelTemplateFormModal = $scope.addNewExcelTemplateFormModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/templates/exceltemplates/excelTemplatesForm.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
                });

                $scope.addNewExcelTemplateFormModal.$promise.then(function () {
                    // if (!relation.isNew)
                    //     $scope.bindDragDrop();

                    $scope.addNewExcelTemplateFormModal.show();
                });
            };
        }
    ]);