'use strict';

angular.module('primeapps')

    .controller('AppFormController', ['$rootScope', '$scope', 'guidEmpty', 'entityTypes', 'helper', 'config', '$http', '$localStorage', 'operations', '$filter', '$cache', 'activityTypes', 'AppFormService', '$window', '$state', '$modal', 'dragularService', '$timeout', '$interval', 'FileUploader', 'ngToast','$stateParams',
        function ($rootScope, $scope, guidEmpty, entityTypes, helper, config, $http, $localStorage, operations, $filter, $cache, activityTypes, AppFormService, $window, $state, $modal, dragularService, $timeout, $interval, FileUploader, ngToast,$stateParams) {
            $scope.appModel = {};
            $scope.nameValid = null;
            $scope.nameBlur = false;

            $rootScope.currentOrgId = parseInt($stateParams.organizationId);
            if ($rootScope.organizations)
                $rootScope.currentOrganization = $filter('filter')($rootScope.organizations, {id: parseInt($rootScope.currentOrgId)},true)[0];


            $rootScope.breadcrumblist[0] = {title: $rootScope.currentOrganization.name};
            $rootScope.breadcrumblist[1] = {};
            $rootScope.breadcrumblist[2] = {};

            var uploader = $scope.uploader = new FileUploader({
                url: 'upload.php'
            });

            uploader.filters.push({
                name: 'imageFilter',
                fn: function (item /*{File|FileLikeObject}*/, options) {
                    var type = '|' + item.type.slice(item.type.lastIndexOf('/') + 1) + '|';
                    return '|jpg|png|jpeg|bmp|gif|'.indexOf(type) !== -1;
                }
            });


            // an async filter
            uploader.filters.push({
                name: 'asyncFilter',
                fn: function (item /*{File|FileLikeObject}*/, options) {

                }
            });

            // CALLBACKS

            uploader.onWhenAddingFileFailed = function (item /*{File|FileLikeObject}*/, filter, options) {
                console.info('onWhenAddingFileFailed', item, filter, options);
            };
            uploader.onAfterAddingFile = function (fileItem) {
                console.info('onAfterAddingFile', fileItem);
            };
            uploader.onAfterAddingAll = function (addedFileItems) {
                console.info('onAfterAddingAll', addedFileItems);
            };
            uploader.onBeforeUploadItem = function (item) {
                console.info('onBeforeUploadItem', item);
            };
            uploader.onProgressItem = function (fileItem, progress) {
                console.info('onProgressItem', fileItem, progress);
            };
            uploader.onProgressAll = function (progress) {
                console.info('onProgressAll', progress);
            };
            uploader.onSuccessItem = function (fileItem, response, status, headers) {
                console.info('onSuccessItem', fileItem, response, status, headers);
            };
            uploader.onErrorItem = function (fileItem, response, status, headers) {
                console.info('onErrorItem', fileItem, response, status, headers);
            };
            uploader.onCancelItem = function (fileItem, response, status, headers) {
                console.info('onCancelItem', fileItem, response, status, headers);
            };
            uploader.onCompleteItem = function (fileItem, response, status, headers) {
                console.info('onCompleteItem', fileItem, response, status, headers);
            };
            uploader.onCompleteAll = function () {
                console.info('onCompleteAll');
            };


            $scope.openModal = function () {
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

                AppFormService.isUniqueName(name)
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

            $scope.save = function (newAppForm) {
                if (!newAppForm.$valid)
                    return false;
                $scope.appSaving = true;
                //$scope.appModel.logo = uploader;
                $scope.appModel.template_id = 0;
                $scope.appModel.status = 1;

                AppFormService.create($scope.appModel)
                    .then(function (response) {
                        ngToast.create({ content: 'App ' + $scope.appModel.label + ' successfully created.', className: 'success' });
                        $scope.appModel = {};
                        $scope.appSaving = false;
                        $scope.appFormModal.hide();
                        $state.go('studio.app.overview', { orgId: $rootScope.currentOrgId, appId: response.data });
                    })
                    .catch(function () {
                        ngToast.create({ content: 'App ' + $scope.appModel.label + ' not created.', className: 'danger' });
                        $scope.appSaving = false;
                    });
            };
        }
    ]);