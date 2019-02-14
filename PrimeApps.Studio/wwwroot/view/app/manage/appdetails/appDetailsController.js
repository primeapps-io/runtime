'use strict';

angular.module('primeapps')

    .controller('AppDetailsController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', '$timeout', 'helper', 'dragularService', 'AppDetailsService', 'LayoutService', '$http', 'config', '$location', 'FileUploader', '$cookies', '$localStorage',
        function ($rootScope, $scope, $filter, $state, $stateParams, $modal, $timeout, helper, dragularService, AppDetailsService, LayoutService, $http, config, $location, FileUploader, $cookies, $localStorage) {

            $scope.appModel = {};
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
                })
            ;

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