'use strict';

angular.module('primeapps')

    .controller('AppDetailsController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', 'ngToast', '$modal', '$timeout', 'helper', 'dragularService', 'AppDetailsService', 'LayoutService', '$http', 'config', '$location',
        function ($rootScope, $scope, $filter, $state, $stateParams, ngToast, $modal, $timeout, helper, dragularService, AppDetailsService, LayoutService, $http, config, $location) {

            //$rootScope.modules = $http.get(config.apiUrl + 'module/get_all');
            $scope.appModel = {};
            $scope.$parent.menuTopTitle = "Settings";
            $scope.$parent.activeMenu = 'settings';
            $scope.$parent.activeMenuItem = 'appDetails';
            $rootScope.breadcrumblist[2].title = 'App Details';

            // $scope.checkNameBlur = function () {
            //     $scope.nameBlur = true;
            //     $scope.checkName($scope.appModel.name);
            // };
            //
            // $scope.checkName = function (name) {
            //     if (!name)
            //         return;
            //
            //     $scope.appModel.name = name.replace(/\s/g, '');
            //     $scope.appModel.name = name.replace(/[^a-zA-Z0-9\_\-]/g, '');
            //
            //     $scope.appModel.name = name.replace(/\s/g, '');
            //     $scope.appModel.name = name.replace(/[^a-zA-Z0-9\_\-]/g, '');
            //
            //     if (!$scope.nameBlur)
            //         return;
            //
            //     $scope.nameChecking = true;
            //     $scope.nameValid = null;
            //
            //     if (!name || name === '') {
            //         $scope.nameChecking = false;
            //         $scope.nameValid = false;
            //         return;
            //     }
            //
            //     AppDetailsService.isUniqueName(name)
            //         .then(function (response) {
            //             $scope.nameChecking = false;
            //             if (response.data) {
            //                 $scope.nameValid = true;
            //             }
            //             else {
            //                 $scope.nameValid = false;
            //             }
            //         })
            //         .catch(function () {
            //             $scope.nameValid = false;
            //             $scope.nameChecking = false;
            //         });
            // };

            AppDetailsService.get($scope.appId).then(function (response) {
                var app = response.data;
                $scope.appModel.name = app.name;
                $scope.appModel.label = app.label;
                $scope.appModel.description = app.description;
                //Logo gelecek
            });

            $scope.save = function () {
                AppDetailsService.update($scope.appId, $scope.appModel)
                    .then(function (response) {
                        ngToast.create({ content: $filter('translate')('Güncelleme Başarılı'), className: 'success' });
                    });
            };
        }
    ]);