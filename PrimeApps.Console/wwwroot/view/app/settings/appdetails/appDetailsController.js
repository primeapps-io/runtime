'use strict';

angular.module('primeapps')

    .controller('AppDetailsController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', '$timeout', 'helper', 'dragularService', 'AppDetailsService', 'LayoutService', '$http', 'config', '$location', 'FileUploader', '$cookies', '$localStorage',
        function ($rootScope, $scope, $filter, $state, $stateParams, $modal, $timeout, helper, dragularService, AppDetailsService, LayoutService, $http, config, $location, FileUploader, $cookies, $localStorage) {

            //$rootScope.modules = $http.get(config.apiUrl + 'module/get_all');
            $scope.appModel = {};
            //$scope.$parent.menuTopTitle = "Settings";
            //$scope.$parent.activeMenu = 'settings';
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
                $scope.appModel.template_id = 0;
                $scope.appModel.status = 1;
                $scope.appModel.logo = app.logo;
            });

            $scope.save = function () {
                AppDetailsService.update($scope.appId, $scope.appModel)
                    .then(function (response) {
                        toastr.success($filter('translate')('Güncelleme Başarılı'));
                    });
            };
        }
    ]);