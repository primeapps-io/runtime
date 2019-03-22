'use strict';

angular.module('primeapps')

    .controller('AppDetailsController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', '$timeout', 'helper', 'dragularService', 'AppDetailsService', 'LayoutService', '$http', 'config', '$location', 'FileUploader', '$cookies', '$localStorage', '$q', 'resizeService',
        function ($rootScope, $scope, $filter, $state, $stateParams, $modal, $timeout, helper, dragularService, AppDetailsService, LayoutService, $http, config, $location, FileUploader, $cookies, $localStorage, $q, resizeService) {

            $scope.appModel = {};
            $scope.authTheme = {};
            $scope.appTheme = {};
            $scope.image = {};
            $scope.imageTotal = {};
            $scope.imageRun = {};
            $scope.$parent.activeMenuItem = 'appDetails';
            $rootScope.breadcrumblist[2].title = 'App Details';

            $scope.uploaderImage = function (field) {
                $scope.image[field] = {};
                var uploader = $scope.uploaderImage[field] = new FileUploader({
                    url: 'storage/upload_logo',
                    headers: {
                        'Authorization': 'Bearer ' + window.localStorage.getItem('access_token'),//$localStorage.get('access_token'),
                        'Accept': 'application/json',
                        'X-Organization-Id': $rootScope.currentOrgId,
                        'X-App-Id': $scope.appId
                    },
                    queueLimit: 1
                });

                uploader.onAfterAddingFile = function (item) {
                    readFile(item._file)
                        .then(function (image) {
                            item.image = image;
                            var img = new Image();
                            resizeService.resizeImage(item.image, { width: 1024 }, function (err, resizedImage) {
                                if (err)
                                    return;

                                item.file.size = item._file.size;
                                $scope.fileLoadingCounter++;
                                var selectedFile = item.uploader.queue[0].file.name;
                                $scope.imageTotal[field] = selectedFile;
                                $scope.image[field]['Name'] = item.uploader.queue[0].file.name;
                                $scope.image[field]['Size'] = item.uploader.queue[0].file.size;
                                $scope.image[field]['Type'] = item.uploader.queue[0].file.type;

                                $scope.imageRun[field] = item.uploader.queue[0];
                                var images = $scope.imageRun;

                                if (field === 'appLogo') {
                                    $scope.appLogo = item.uploader.queue[0];
                                    var appLogo = images['appLogo'];
                                    appLogo.upload();
                                }

                                if (appLogo) {
                                    appLogo.uploader.onCompleteItem = function (fileItem, logoUrl, status) {
                                        if (status === 200) {
                                            $scope.appModel.logo = logoUrl;
                                        }
                                    };
                                }
                            });
                        });
                };
                uploader.onWhenAddingFileFailed = function (item, filter, options) {
                    switch (filter.name) {
                        case 'imgFilter':
                            ngToast.create({
                                content: $filter('translate')('Setup.Settings.ImageError'),
                                className: 'warning'
                            });
                            break;
                        case 'sizeFilter':
                            ngToast.create({
                                content: $filter('translate')('Setup.Settings.SizeError'),
                                className: 'warning'
                            });
                            break;
                    }
                };

                uploader.filters.push({
                    name: 'imgFilter',
                    fn: function (item) {
                        var extension = helper.getFileExtension(item.name);
                        return true ? (extension === 'jpg' || extension == 'jpeg' || extension == 'png' || extension == 'doc' || extension == 'gif' || extension == 'ico') : false;
                    }
                });

                uploader.filters.push({
                    name: 'sizeFilter',
                    fn: function (item) {
                        return item.size < 5242880;// 5mb
                    }
                });

                // uploader_image.onSuccessItem = function (item, response) {
                //     $scope.image[field.name]['UniqueName'] = response.UniqueName;
                //     $scope.fileLoadingCounter--;
                // };

                function readFile(file) {
                    var deferred = $q.defer();
                    var reader = new FileReader();

                    reader.onload = function (e) {
                        deferred.resolve(e.target.result);
                    };

                    reader.readAsDataURL(file);

                    return deferred.promise;
                }

                return uploader;

            };

            // $scope.logoRemove = function () {
            //     if (uploader.queue[0]) {
            //         //uploader.queue[0].image = null;
            //         uploader.queue[0].remove();
            //     }
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
                AppDetailsService.update($scope.appId, $scope.appModel)
                    .then(function (response) {
                        toastr.success($filter('translate')('App Details is updated successfully.'));
                        $scope.saving = false;
                    });
            };

            // $scope.addMasterUser = function () {
            //     var newCol = {};
            //     newCol.role_id = 1;
            //     newCol.profile_id = 1;
            //     newCol.first_name = "master";
            //     newCol.last_name = "test";
            //     newCol.email = "master.test@usertest3.com";
            //     newCol.password = "1234567";
            //     newCol.created_at = new Date();
            //
            //     AppDetailsService.addAppUser(newCol)
            //         .then(function (response) {
            //             if (response.data) {
            //                 toastr.success('Collaborator is saved successfully');
            //
            //             }
            //         })
            //         .catch(function () {
            //             toastr.error($filter('translate')('Common.Error'));
            //
            //         });
            // };
        }
    ]);