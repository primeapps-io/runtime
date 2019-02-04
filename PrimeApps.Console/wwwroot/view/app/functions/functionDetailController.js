'use strict';

angular.module('primeapps')

    .controller('ComponentDetailController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', '$timeout', 'helper', 'dragularService', 'FunctionsService', '$localStorage',
        function ($rootScope, $scope, $filter, $state, $stateParams, $modal, $timeout, helper, dragularService, FunctionsService, $localStorage) {

            $scope.id = $state.params.id;
            $scope.orgId = $state.params.orgId;

            $scope.$parent.menuTopTitle = $scope.currentApp.label;
            $scope.$parent.activeMenu = 'app';
            $scope.$parent.activeMenuItem = 'functions';

            $scope.currentApp = $localStorage.get("current_app");

            /*if (!$scope.orgId || !$scope.appId) {
             $state.go('studio.apps', { organizationId: $scope.orgId });
             }*/

            $scope.functionForm = {};
            $scope.loading = true;
            //var currentOrganization = $localStorage.get("currentApp");
            $scope.organization = $filter('filter')($rootScope.organizations, {id: $scope.orgId})[0];
            $scope.giteaUrl = giteaUrl;

            /*$scope.aceOption = {
             mode: 'javascript',
             theme: 'tomorrow_night',
             onLoad: function (_ace) {
             // HACK to have the ace instance in the scope...
             $scope.modeChanged = function () {
             _ace.getSession().setMode("ace/mode/javascript");
             };
             }
             };*/

            if (!$scope.id) {
                $state.go('app.functions');
            }

            FunctionsService.get($scope.id)
                .then(function (response) {
                    if (!response.data) {
                        toastr.error('Function Not Found !');
                        $state.go('app.functions');
                    }

                    $scope.function = response.data;
                    $scope.loading = false;
                });

            $scope.edit = function () {
                //$scope.modalLoading = true;
                $scope.editing = true;

                $scope.createFormModal = $scope.createFormModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/functions/functionFormModal.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
                });
                $scope.createFormModal.$promise.then(function () {
                    $scope.createFormModal.show();
                });
            };

            $scope.save = function (functionFormValidation) {
                if (!functionFormValidation.$valid)
                    return;

                $scope.saving = true;

                if ($scope.componentForm.type === 2) {
                    $scope.componentForm.place = 0;
                    $scope.componentForm.order = 0;
                }

                FunctionsService.update($scope.functionForm)
                    .then(function (response) {
                        $scope.saving = false;
                        $scope.createFormModal.hide();
                        $scope.editing = false;
                    });
            }
        }
    ]);