'use strict';

angular.module('primeapps')

    .controller('WordTemplatesController', ['$rootScope', '$scope', '$state', '$stateParams', '$location', 'ngToast', '$filter', '$cache', '$q', 'helper', 'dragularService', 'operators', 'WordTemplatesService',  '$http', 'config','$modal',
        function ($rootScope, $scope, $state, $stateParams, $location, ngToast, $filter, $cache, $q, helper, dragularService, operators, WordTemplatesService, $http, config,$modal) {

            $scope.$parent.menuTopTitle = "Templates";
            $scope.$parent.activeMenu = 'templates';
            $scope.$parent.activeMenuItem = 'templatesWord';

            $scope.showFormModal = function () {

                $scope.addNewWordTemplateFormModal = $scope.addNewWordTemplateFormModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/templates/wordtemplates/wordTemplatesForm.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
                });

                $scope.addNewWordTemplateFormModal.$promise.then(function () {
                    // if (!relation.isNew)
                    //     $scope.bindDragDrop();

                    $scope.addNewWordTemplateFormModal.show();
                });
            };
        }
    ]);