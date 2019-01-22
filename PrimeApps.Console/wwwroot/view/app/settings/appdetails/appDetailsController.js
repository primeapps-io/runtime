'use strict';

angular.module('primeapps')

    .controller('AppDetailsController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', 'ngToast', '$modal', '$timeout', 'helper', 'dragularService', 'AppDetailsService', 'LayoutService', '$http', 'config', '$location', 'FileUploader', '$cookies','$localStorage',
        function ($rootScope, $scope, $filter, $state, $stateParams, ngToast, $modal, $timeout, helper, dragularService, AppDetailsService, LayoutService, $http, config, $location, FileUploader, $cookies,$localStorage) {

            //$rootScope.modules = $http.get(config.apiUrl + 'module/get_all');
            $scope.appModel = {};
            $scope.$parent.menuTopTitle = "Settings";
            $scope.$parent.activeMenu = 'settings';
            $scope.$parent.activeMenuItem = 'appDetails';
            $rootScope.breadcrumblist[2].title = 'App Details';

            $scope.checkNameBlur = function () {
                $scope.nameBlur = true;
                $scope.checkName($scope.appModel.name);
            };

            $scope.checkName = function (name) {
                if (!name)
                    return;

                $scope.appModel.name = name.replace(/\s/g, '');
                $scope.appModel.name = name.replace(/[^a-zA-Z0-9\_\-]/g, '');

                $scope.appModel.name = name.replace(/\s/g, '');
                $scope.appModel.name = name.replace(/[^a-zA-Z0-9\_\-]/g, '');

                if (!$scope.nameBlur)
                    return;

                $scope.nameChecking = true;
                $scope.nameValid = null;

                if (!name || name === '') {
                    $scope.nameChecking = false;
                    $scope.nameValid = false;
                    return;
                }

                AppDetailsService.isUniqueName(name)
                    .then(function (response) {
                        $scope.nameChecking = false;
                        if (response.data) {
                            $scope.nameValid = true;
                        }
                        else {
                            $scope.nameValid = false;
                        }
                    })
                    .catch(function () {
                        $scope.nameValid = false;
                        $scope.nameChecking = false;
                    });
            };

            var uploader = $scope.uploader = new FileUploader({
                url: 'storage/upload_logo',
                headers: {
                    'Authorization': 'Bearer ' + $localStorage.read('access_token'),
                    'Accept': 'application/json',
                    'X-Tenant-Id': $cookies.get('tenant_id')
                },
                queueLimit: 1
            });

            uploader.onCompleteItem = function (fileItem, response, status, headers) {
                if (status === 200) {
                    $scope.company.logo = response;

                    editCompany(true);
                }
            };

            uploader.onWhenAddingFileFailed = function (item, filter, options) {
                switch (filter.name) {
                    case 'imageFilter':
                        ngToast.create({ content: $filter('translate')('Setup.Settings.ImageError'), className: 'warning' });
                        break;
                    case 'sizeFilter':
                        ngToast.create({ content: $filter('translate')('Setup.Organization.SizeError'), className: 'warning' });
                        break;
                }
            };

            uploader.onAfterAddingFile = function (item) {
                var reader = new FileReader();

                reader.onload = function (event) {
                    $scope.$apply(function () {
                        $scope.imagePreview = event.target.result;
                    });
                };

                reader.readAsDataURL(item._file);
            };

            uploader.filters.push({
                name: 'imageFilter',
                fn: function (item, options) {
                    var type = '|' + item.type.slice(item.type.lastIndexOf('/') + 1) + '|';
                    return '|jpg|png|jpeg|bmp|'.indexOf(type) > -1;
                }
            });

            uploader.filters.push({
                name: 'sizeFilter',
                fn: function (item) {
                    return item.size < 50000;//50 kb
                }
            });

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