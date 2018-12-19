'use strict';

angular.module('primeapps')

    .controller('AppFormController', ['$rootScope', '$scope', 'guidEmpty', 'entityTypes', 'helper', 'config', '$http', '$localStorage', 'operations', '$filter', '$cache', 'activityTypes', 'AppFormService', '$window', '$state', '$modal', 'dragularService', '$timeout', '$interval', 'FileUploader',
        function ($rootScope, $scope, guidEmpty, entityTypes, helper, config, $http, $localStorage, operations, $filter, $cache, activityTypes, AppFormService, $window, $state, $modal, dragularService, $timeout, $interval, FileUploader) {

            var uploader = $scope.uploader = new FileUploader({
                url: 'upload.php'
            });

            uploader.filters.push({
                name: 'syncFilter',
                fn: function(item /*{File|FileLikeObject}*/, options) {
                    console.log('syncFilter');
                    return this.queue.length < 10;
                }
            });

            // an async filter
            uploader.filters.push({
                name: 'asyncFilter',
                fn: function(item /*{File|FileLikeObject}*/, options) {

                }
            });

            // CALLBACKS

            uploader.onWhenAddingFileFailed = function(item /*{File|FileLikeObject}*/, filter, options) {
                console.info('onWhenAddingFileFailed', item, filter, options);
            };
            uploader.onAfterAddingFile = function(fileItem) {
                console.info('onAfterAddingFile', fileItem);
            };
            uploader.onAfterAddingAll = function(addedFileItems) {
                console.info('onAfterAddingAll', addedFileItems);
            };
            uploader.onBeforeUploadItem = function(item) {
                console.info('onBeforeUploadItem', item);
            };
            uploader.onProgressItem = function(fileItem, progress) {
                console.info('onProgressItem', fileItem, progress);
            };
            uploader.onProgressAll = function(progress) {
                console.info('onProgressAll', progress);
            };
            uploader.onSuccessItem = function(fileItem, response, status, headers) {
                console.info('onSuccessItem', fileItem, response, status, headers);
            };
            uploader.onErrorItem = function(fileItem, response, status, headers) {
                console.info('onErrorItem', fileItem, response, status, headers);
            };
            uploader.onCancelItem = function(fileItem, response, status, headers) {
                console.info('onCancelItem', fileItem, response, status, headers);
            };
            uploader.onCompleteItem = function(fileItem, response, status, headers) {
                console.info('onCompleteItem', fileItem, response, status, headers);
            };
            uploader.onCompleteAll = function() {
                console.info('onCompleteAll');
            };

            console.info('uploader', uploader);
            $scope.openModal = function () {
                $scope.newFormModal = $scope.newFormModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/organization/appform/newAppForm.html',
                    animation: 'side',
                    backdrop: 'static',
                    show: false
                });
                $scope.newFormModal.$promise.then(function () {
                    $scope.newFormModal.show();
                });
            }

        }
    ]);