'use strict';

angular.module('primeapps')

    .controller('AppFormController', ['$rootScope', '$scope', 'guidEmpty', 'entityTypes', 'helper', 'config', '$http', '$localStorage', 'operations', '$filter', '$cache', 'activityTypes', 'AppFormService', '$window', '$state', '$modal', 'dragularService', '$timeout', '$interval', 'FileUploader', '$stateParams',
        function ($rootScope, $scope, guidEmpty, entityTypes, helper, config, $http, $localStorage, operations, $filter, $cache, activityTypes, AppFormService, $window, $state, $modal, dragularService, $timeout, $interval, FileUploader, $stateParams) {
            $scope.appModel = {};
            $scope.nameBlur = false;

            $rootScope.currentOrgId = parseInt($stateParams.orgId);

            if (!$rootScope.currentOrgId && $rootScope.organizations) {
                var defaultOrg = $filter('filter')($rootScope.organizations, { default: true }, true)[0];
                window.location.href = '/#/apps?orgId=' + defaultOrg.id;
            }

            if ($rootScope.organizations)
                $rootScope.currentOrganization = $filter('filter')($rootScope.organizations, { id: parseInt($rootScope.currentOrgId) }, true)[0];

            if ($rootScope.currentOrganization.role !== 'administrator') {
                toastr.warning($filter('translate')('Common.Forbidden'));
                var defaultOrg = $filter('filter')($rootScope.organizations, { default: true }, true)[0];
                window.location.href = '/#/apps?orgId=' + defaultOrg.id;
                return;
            }

            $rootScope.breadcrumblist[0] = {
                title: $rootScope.currentOrganization.label,
                link: '#/apps?orgId=' + $rootScope.currentOrgId
            };
            $rootScope.breadcrumblist[1] = { title: "Create App" };
            $rootScope.breadcrumblist[2] = {};

            if (!$rootScope.currentOrgId) {
                toastr.warning($filter('translate')('Common.NotFound'));
                var defaultOrg = $filter('filter')($rootScope.organizations, { default: true }, true)[0];
                window.location.href = '/#/apps?orgId=' + defaultOrg.id;
                return;
            }

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
                $scope.croppedImage = '';
                $scope.uploadLogo = true;
                var reader = new FileReader();

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

            // CALLBACKS

            // uploader.onWhenAddingFileFailed = function (item /*{File|FileLikeObject}*/, filter, options) {
            //     console.info('onWhenAddingFileFailed', item, filter, options);
            // };
            // uploader.onAfterAddingFile = function (fileItem) {
            //     console.info('onAfterAddingFile', fileItem);
            // };
            // uploader.onAfterAddingAll = function (addedFileItems) {
            //     console.info('onAfterAddingAll', addedFileItems);
            // };
            // uploader.onBeforeUploadItem = function (item) {
            //     console.info('onBeforeUploadItem', item);
            // };
            // uploader.onProgressItem = function (fileItem, progress) {
            //     console.info('onProgressItem', fileItem, progress);
            // };
            // uploader.onProgressAll = function (progress) {
            //     console.info('onProgressAll', progress);
            // };
            // uploader.onSuccessItem = function (fileItem, response, status, headers) {
            //     console.info('onSuccessItem', fileItem, response, status, headers);
            // };
            // uploader.onErrorItem = function (fileItem, response, status, headers) {
            //     console.info('onErrorItem', fileItem, response, status, headers);
            // };
            // uploader.onCancelItem = function (fileItem, response, status, headers) {
            //     console.info('onCancelItem', fileItem, response, status, headers);
            // };
            // uploader.onCompleteItem = function (fileItem, response, status, headers) {
            //     console.info('onCompleteItem', fileItem, response, status, headers);
            // };
            // uploader.onCompleteAll = function () {
            //     console.info('onCompleteAll');
            // };

            $scope.openModal = function () {
                $scope.appModel = {};
                $scope.nameValid = null;
                $scope.requiredColor = "";
                $scope.appFormModal = $scope.appFormModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/organization/appform/newAppForm.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
                });
                $scope.appFormModal.$promise.then(function () {
                    $scope.appFormModal.show();
                });
            };

            $scope.closeModal = function () {
                $scope.appFormModal.hide();
                //  $scope.appModel = {};
                $scope.logoRemove();
                $scope.nameValid = null;
                $scope.nameBlur = false;
            };

            $scope.checkNameBlur = function () {
                $scope.nameBlur = true;
                $scope.checkNameUnique($scope.appModel.name);
            };

            $scope.checkNameValid = function (name) {
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
            };

            $scope.checkNameUnique = function (name) {
                if (!name)
                    return;

                $scope.checkNameValid(name);

                AppFormService.isUniqueName(name)
                    .then(function (response) {
                        $scope.nameChecking = false;
                        if (response.data) {
                            $scope.nameValid = true;
                        } else {
                            $scope.nameValid = false;
                        }
                    })
                    .catch(function () {
                        $scope.nameValid = false;
                        $scope.nameChecking = false;
                    });
            };

            $scope.logoRemove = function () {
                if (uploader.queue[0]) {
                    //uploader.queue[0].image = null;
                    uploader.queue[0].remove();
                    $scope.uploadLogo = false;
                }
            };

            $scope.generateAppName = function () {
                if (!$scope.appModel || !$scope.appModel.label) {
                    $scope.appModel.name = null;
                    return;
                }

                $scope.appModel.name = helper.getSlug($scope.appModel.label, '-');
            };

            $scope.save = function (newAppForm) {
                if (!newAppForm.$valid) {
                    toastr.error($filter('translate')('Module.RequiredError'));
                    return false;
                }

                $scope.appSaving = true;
                $scope.checkNameValid($scope.appModel.name);

                AppFormService.isUniqueName($scope.appModel.name)
                    .then(function (response) {
                        $scope.nameChecking = false;
                        if (response.data) {
                            //$scope.appModel.logo = uploader;
                            $scope.appModel.template_id = 0;
                            $scope.appModel.status = 1;

                            AppFormService.create($scope.appModel)
                                .then(function (response) {
                                    $rootScope.currentAppId = response.data.id;
                                    var header = {
                                        'Authorization': 'Bearer ' + window.localStorage.getItem('access_token'),
                                        'Accept': 'application/json',
                                        'X-Organization-Id': $rootScope.currentOrgId,
                                        'X-App-Id': response.data.id
                                    };

                                    if ($scope.uploadLogo) {
                                        uploader.queue[0].uploader.headers = header;
                                        uploader.queue[0].headers = header;
                                        uploader.queue[0].upload();
                                    }
                                    else {
                                        var url = 'images/default-app-logo.png';
                                        var dummy;
                                        $http.get(url, { responseType: "blob" })
                                            .then(function (response, status, headers, config) {
                                                var mimetype = response.data.type;
                                                var file = new File([response.data], "default.jpg", { type: mimetype });

                                                dummy = new FileUploader.FileItem(uploader, {});
                                                dummy._file = file;
                                                dummy.progress = 100;
                                                dummy.isUploaded = true;
                                                dummy.isSuccess = true;
                                                uploader.queue.push(dummy);
                                                uploader.queue[0].uploader.headers = header;
                                                uploader.queue[0].headers = header;
                                                uploader.uploadItem(dummy);
                                            });
                                    }
                                    uploader.onCompleteItem = function (fileItem, logoUrl, status) {
                                        if (status === 200) {
                                            $scope.updateApp = {};
                                            $scope.updateApp.description = response.data.description;
                                            $scope.updateApp.label = response.data.label;
                                            $scope.updateApp.name = response.data.name;
                                            $scope.updateApp.status = response.data.status;
                                            $scope.updateApp.template_id = response.data.templet_id;
                                            $scope.updateApp.logo = logoUrl;
                                            AppFormService.update($rootScope.currentAppId, $scope.updateApp)
                                                .then(function (response) {
                                                    $scope.appSaving = false;
                                                    $scope.appFormModal.hide();
                                                    // $scope.appModel = {};
                                                    $scope.logoRemove();
                                                    toastr.success($filter('translate')('App successfully created.'));
                                                    $scope.nameValid = null;
                                                    $scope.nameBlur = false;
                                                    $state.go('studio.app.overview', {
                                                        orgId: $rootScope.currentOrgId,
                                                        appId: $rootScope.currentAppId
                                                    });
                                                });
                                        }
                                    };
                                })
                                .catch(function () {
                                    toastr.error('App ' + $scope.appModel.label + ' not created.');
                                    $scope.appSaving = false;
                                });
                        } else {
                            $scope.appSaving = false;
                            $scope.nameValid = false;
                        }
                    })
                    .catch(function () {
                        $scope.nameValid = false;
                        $scope.nameChecking = false;
                    });
            };
        }
    ]);