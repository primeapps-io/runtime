'use strict';

angular.module('primeapps')

    .controller('OrganizationController', ['$rootScope', '$scope', '$translate', 'tmhDynamicLocale', '$localStorage', 'ngToast', 'config', '$window', '$timeout', '$filter', 'blockUI', 'FileUploader', 'LayoutService', 'OrganizationService','$cookies',
        function ($rootScope, $scope, $translate, tmhDynamicLocale, $localStorage, ngToast, config, $window, $timeout, $filter, blockUI, FileUploader, LayoutService, OrganizationService,$cookies) {
            $scope.company = {};
            $scope.company.instanceID = $rootScope.workgroup.tenant_id;
            $scope.company.title = $rootScope.workgroup.title;
            $scope.company.currency = $rootScope.workgroup.currency;
            $scope.company.culture = $rootScope.workgroup.setting.culture;
            $scope.company.logo = $rootScope.workgroup.setting.logo;
            $scope.company.tenant_id = $rootScope.user.tenant_id;
            $scope.company.language = $rootScope.workgroup.setting.language;
            $scope.bounds = {};
            $scope.bounds.left = 0;
            $scope.bounds.right = 0;
            $scope.bounds.top = 0;
            $scope.bounds.bottom = 0;
            $scope.user = $rootScope.user;

            $scope.editCompany = function () {
                if ($scope.companyForm.$valid) {
                    editCompany(false);
                }
            };

            function editCompany(logoUpload) {
                if (!logoUpload)
                    $scope.companyUpdating = true;

                OrganizationService.editCompany($scope.company)
                    .then(function () {
                        if (!logoUpload) {
                            ngToast.create({content: $filter('translate')('Setup.Organization.UpdateSuccess'), className: 'success'});
                            $scope.companyUpdating = false;
                        }
                        else {
                            uploader.clearQueue();
                        }

                        LayoutService.getMyAccount(true);
                    });
            }

            var uploader = $scope.uploader = new FileUploader({
                url: 'attach/upload_logo',
                headers: {'Authorization': 'Bearer ' + $localStorage.read('access_token')},
                'X-Tenant-Id': $cookies.get('tenant_id'),
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
                        ngToast.create({content: $filter('translate')('Setup.Settings.ImageError'), className: 'warning'});
                        break;
                    case 'sizeFilter':
                        ngToast.create({content: $filter('translate')('Setup.Organization.SizeError'), className: 'warning'});
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

            $scope.removeLogo = function () {
                $scope.company.logo = null;
                $scope.imagePreview = null;
                editCompany(true);
            }
        }
    ]);