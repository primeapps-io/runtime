'use strict';

angular.module('primeapps')

    .controller('EmailTemplatesController', ['$rootScope', '$scope', '$state', '$stateParams', '$location', 'ngToast', '$filter', '$cache', '$q', 'helper', 'dragularService', 'operators', 'EmailTemplatesService',  '$http', 'config','$modal',
        function ($rootScope, $scope, $state, $stateParams, $location, ngToast, $filter, $cache, $q, helper, dragularService, operators, EmailTemplatesService, $http, config,$modal) {

            $scope.$parent.menuTopTitle = "Templates";
            $scope.$parent.activeMenu = 'templates';
            $scope.$parent.activeMenuItem = 'templatesEmail';

            $scope.showFormModal = function () {

                $scope.addNewEmailTemplateFormModal = $scope.addNewEmailTemplateFormModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/templates/emailtemplates/emailTemplatesForm.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
                });

                $scope.addNewEmailTemplateFormModal.$promise.then(function () {
                    // if (!relation.isNew)
                    //     $scope.bindDragDrop();

                    $scope.addNewEmailTemplateFormModal.show();
                });
            };
        }
    ]);