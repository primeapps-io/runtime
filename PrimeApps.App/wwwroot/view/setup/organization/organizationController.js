'use strict';

angular.module('primeapps')

    .controller('OrganizationController', ['$rootScope', '$scope', '$filter', 'blockUI', 'FileUploader', 'AppService', 'OrganizationService', '$cookies', '$state', 'helper', 'mdToast', '$mdDialog', '$window',
        function ($rootScope, $scope, $filter, blockUI, FileUploader, AppService, OrganizationService, $cookies, $state, helper, mdToast, $mdDialog, $window) {

            AppService.checkPermission().then(function (res) {

                if (res && res.data) {
                    var profile = JSON.parse(res.data["profile"]);
                    var customProfilePermissions = undefined;
                    if (res.data["customProfilePermissions"])
                        customProfilePermissions = JSON.parse(res.data["customProfilePermissions"]);

                    if (!profile.HasAdminRights) {
                        var organizationIsExist = undefined;
                        if (customProfilePermissions)
                            organizationIsExist = customProfilePermissions.permissions.indexOf('organization') > -1;

                        if (!organizationIsExist) {
                            mdToast.error($filter('translate')('Common.Forbidden'));
                            $state.go('app.dashboard');
                        }
                    }
                }

                $rootScope.breadcrumblist = [
                    {
                        title: $filter('translate')('Layout.Menu.Dashboard'),
                        link: "#/app/dashboard"
                    },
                    {
                        title: $filter('translate')('Setup.Nav.OrganizationSettings')
                    }
                ];

                $scope.cultureArray = [{ value: 'tr-TR', label: $filter('translate')('Setup.Settings.Turkish') },
                { value: 'en-US', label: $filter('translate')('Setup.Settings.English') }];
                $scope.currencyArray = [{ value: 'TRY', label: $filter('translate')('Setup.Settings.TurkishLira') },
                { value: 'USD', label: $filter('translate')('Setup.Settings.Dollar') }];

                $scope.company = {};
                $scope.company.instanceID = $rootScope.workgroup.tenant_id;
                $scope.company.title = $rootScope.workgroup.title;
                $scope.company.currency = $filter('filter')($scope.currencyArray, { value: $rootScope.workgroup.currency }, true)[0];
                $scope.company.culture = $filter('filter')($scope.cultureArray, { value: $rootScope.workgroup.setting.culture }, true)[0];
                $scope.company.logo = $rootScope.workgroup.setting.logo;
                $scope.logo = $rootScope.workgroup.logo_url ? blobUrl + '/' + $rootScope.workgroup.logo_url : null;
                $scope.company.tenant_id = $rootScope.user.tenant_id;
                $scope.company.language = $rootScope.workgroup.setting.language;
                $scope.bounds = {};
                $scope.bounds.left = 0;
                $scope.bounds.right = 0;
                $scope.bounds.top = 0;
                $scope.bounds.bottom = 0;
                $scope.user = $rootScope.user;
                $scope.loading = false;


                $scope.editCompany = function () {
                    if ($scope.companyForm.validate()) {
                        $scope.loading = true;
                        editCompany(false);
                    } else {
                        mdToast.error($filter('translate')('Module.RequiredError'));

                    }
                };

                function editCompany(logoUpload) {
                    $scope.loading = true;

                    $scope.company.currency = $scope.company.currency.value;
                    $scope.company.culture = $scope.company.culture.value;

                    OrganizationService.editCompany($scope.company)
                        .then(function () {
                            mdToast.success($filter('translate')('Setup.Organization.UpdateSuccess'));

                            uploader.clearQueue();
                            //AppService.getMyAccount(true);
                            $window.location.reload();
                        });
                }

                var appId = $cookies.get(preview ? 'preview_app_id' : 'app_id');
                var tenantId = $cookies.get(preview ? 'preview_tenant_id' : 'tenant_id');

                var uploader = $scope.uploader = new FileUploader({
                    url: 'storage/upload_logo',
                    headers: {
                        'Authorization': 'Bearer ' + window.localStorage.getItem('access_token'),//$localStorage.get('access_token'),
                        'Accept': 'application/json',
                        'Content-Type': 'application/x-www-form-urlencoded;charset=utf-8',
                        'X-Tenant-Id': tenantId,
                        'X-App-Id': appId
                    },
                    queueLimit: 1
                });

                uploader.onCompleteItem = function (fileItem, response, status, headers) {
                    if (status === 200) {
                        $scope.company.logo = response;
                        mdToast.success($filter('translate')('Setup.Settings.ImageUpload'));
                        //editCompany(true);
                        uploader.clearQueue();
                    }
                };

                uploader.onWhenAddingFileFailed = function (item, filter, options) {
                    switch (filter.name) {
                        case 'imageFilter':
                            mdToast.warning($filter('translate')('Setup.Settings.ImageError'));
                            break;
                        case 'sizeFilter':
                            mdToast.warning($filter('translate')('Setup.Organization.SizeError'));
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
                //uploader.onBeforeUploadItem = function (item) {
                //    //item._file = dataURItoBlob($scope.croppedImage);
                //};

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
                        return item.size < 5242880;//5mb
                    }
                });


                $scope.removeLogo = function (ev) {
                    var confirm = $mdDialog.confirm()
                        .title($filter('translate')('Common.AreYouSure'))
                        .targetEvent(ev)
                        .ok($filter('translate')('Common.Yes'))
                        .cancel($filter('translate')('Common.No'));

                    $mdDialog.show(confirm)
                        .then(function () {
                            $scope.company.logo = null;
                            $scope.imagePreview = null;
                            $scope.logo = null;
                            $scope.uploader.clearQueue();
                            //editCompany(true);
                        });
                };

                //For Kendo UI
                $scope.cultureOptions = {
                    dataSource: $scope.cultureArray,
                    dataTextField: "label",
                    dataValueField: "value"
                };

                $scope.currencyOptions = {
                    dataSource: $scope.currencyArray,
                    dataTextField: "label",
                    dataValueField: "value"
                }
            });
        }
    ]);