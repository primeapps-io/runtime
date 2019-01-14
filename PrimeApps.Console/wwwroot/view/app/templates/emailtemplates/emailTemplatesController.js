'use strict';

angular.module('primeapps')

    .controller('EmailTemplatesController', ['$rootScope', '$scope', '$state', '$stateParams', '$location', 'ngToast', '$filter', '$cache', '$q', 'helper', 'dragularService', 'operators', 'EmailTemplatesService', '$http', 'config', '$modal',
        function ($rootScope, $scope, $state, $stateParams, $location, ngToast, $filter, $cache, $q, helper, dragularService, operators, EmailTemplatesService, $http, config, $modal) {

            $scope.$parent.menuTopTitle = "Templates";
            $scope.$parent.activeMenu = 'templates';
            $scope.$parent.activeMenuItem = 'templatesEmail';
            $scope.loading = true;

            EmailTemplatesService.getAll(2).then(function (response) {
                $scope.templates = response.data;
            }).finally(function () {
                $scope.loading = false;
            });

            $scope.showFormModal = function (template) {

                $scope.template = template;

                $scope.addNewEmailTemplateFormModal = $scope.addNewEmailTemplateFormModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/templates/emailtemplates/emailTemplatesForm.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
                });

                $scope.addNewEmailTemplateFormModal.$promise.then(function () {
                    $scope.addNewEmailTemplateFormModal.show();
                });
            };
        }
    ]);