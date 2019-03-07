'use strict';

angular.module('primeapps')

    .controller('AppDetailsController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', '$timeout', 'helper', 'dragularService', 'AppDetailsService', 'LayoutService', '$http', 'config', '$location', 'FileUploader', '$cookies', '$localStorage',
        function ($rootScope, $scope, $filter, $state, $stateParams, $modal, $timeout, helper, dragularService, AppDetailsService, LayoutService, $http, config, $location, FileUploader, $cookies, $localStorage) {

            $scope.appModel = {};
            $scope.authTheme = {};
            $scope.appTheme = {};
            $scope.$parent.activeMenuItem = 'appDetails';
            $rootScope.breadcrumblist[2].title = 'App Details';

            var uploader = $scope.uploader = new FileUploader({
                url: 'storage/upload_logo',
                headers: {
                    'Authorization': 'Bearer ' + window.localStorage.getItem('access_token'),//$localStorage.get('access_token'),
                    'Accept': 'application/json',
                    'X-Organization-Id': $rootScope.currentOrgId
                },
                queueLimit: 1
            });

            uploader.onWhenAddingFileFailed = function (item, filter, options) {
                switch (filter.name) {
                    case 'imageFilter':
                        toastr.warning($filter('translate')('Setup.Settings.ImageError'));
                        break;
                    case 'sizeFilter':
                        toastr.warning($filter('translate')('Setup.Settings.SizeError'));
                        break;
                }
            };

            uploader.onAfterAddingFile = function (item) {
                $scope.uploadImage = true;
                $scope.croppedImage = '';
                var reader = new FileReader();
                $scope.appModel.logo = item.file.name;

                reader.onload = function (event) {
                    $scope.$apply(function () {
                        item.image = event.target.result;
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
                    return item.size < 5242880;//5 mb
                }
            });

            $scope.logoRemove = function () {
                if (uploader.queue[0]) {
                    //uploader.queue[0].image = null;
                    uploader.queue[0].remove();
                }
            };

            AppDetailsService.get($scope.appId).then(function (response) {
                var app = response.data;
                $scope.appModel.name = app.name;
                $scope.appModel.label = app.label;
                $scope.appModel.description = app.description;
                $scope.appModel.template_id = 0;
                $scope.appModel.status = 1;
                $scope.appModel.logo = app.logo;
            });

            AppDetailsService.getAppTheme($scope.appId).then(function (response) {
                var appTheme = response.data;
                $scope.appTheme.color = appTheme.color;
                $scope.appTheme.title = appTheme.title;
                $scope.appTheme.favicon = appTheme.favicon;
                $scope.appTheme.logo = appTheme.logo;
            });

            $scope.saveAppTheme = function () {
                $scope.savingApp = true;
                var appThemes = {};
                appThemes.color = $scope.appTheme.color;
                appThemes.title = $scope.appTheme.title;
                appThemes.favicon = $scope.appTheme.favicon;
                appThemes.logo = $scope.appTheme.logo;


                AppDetailsService.updateAppTheme($scope.appId, appThemes)
                    .then(function (response) {
                        toastr.success($filter('translate')('Güncelleme Başarılı'));
                        $scope.savingApp = false;
                    });
            };

            AppDetailsService.getAuthTheme($scope.appId).then(function (response) {
                var authTheme = response.data;
                $scope.authTheme.banner = authTheme.banner[0].image;
                $scope.authTheme.color = authTheme.color;
                $scope.authTheme.title = authTheme.title;
                $scope.authTheme.descriptionTr = authTheme.banner[0].descriptions.tr;
                $scope.authTheme.descriptionEn = authTheme.banner[0].descriptions.en;
                $scope.authTheme.favicon = authTheme.favicon;
                $scope.authTheme.logo = authTheme.logo;
            });

            $scope.saveAuthTheme = function () {
                $scope.savingAuth = true;
                var authThemes = {};
                var description = {};
                description.en = $scope.authTheme.descriptionEn;
                description.tr = $scope.authTheme.descriptionTr;
                var banner = [
                    { description: description, image: $scope.appModel.logo }
                ];
                authThemes.logo = $scope.appModel.logo;
                authThemes.color = $scope.authTheme.color;
                authThemes.title = $scope.authTheme.title;
                authThemes.banner = banner;
                authThemes.favicon = $scope.appModel.logo;

                AppDetailsService.updateAuthTheme($scope.appId, authThemes)
                    .then(function (response) {
                        toastr.success($filter('translate')('Güncelleme Başarılı'));
                        $scope.savingAuth = false;
                    });
            };

            $scope.save = function () {
                $scope.saving = true;
                if ($scope.uploadImage) {
                    var header = {
                        'Authorization': 'Bearer ' + window.localStorage.getItem('access_token'),
                        'Accept': 'application/json',
                        'X-Organization-Id': $rootScope.currentOrgId,
                        'X-App-Id': $scope.appId
                    };
                    uploader.queue[0].uploader.headers = header;
                    uploader.queue[0].headers = header;
                    uploader.queue[0].upload();

                    uploader.onCompleteItem = function (fileItem, logoUrl, status) {
                        if (status === 200) {
                            $scope.appModel.logo = logoUrl;
                            AppDetailsService.update($scope.appId, $scope.appModel)
                                .then(function (response) {
                                    toastr.success($filter('translate')('Güncelleme Başarılı'));
                                    $scope.saving = false;
                                });
                        }
                    };
                }
                else {
                    AppDetailsService.update($scope.appId, $scope.appModel)
                        .then(function (response) {
                            toastr.success($filter('translate')('Güncelleme Başarılı'));
                            $scope.saving = false;
                        });
                }
            };
            
        }
    ]);