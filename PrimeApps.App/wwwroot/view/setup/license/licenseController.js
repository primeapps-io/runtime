'use strict';

angular.module('primeapps')

    .controller('LicenseController', ['$rootScope', '$scope', '$filter', '$popover', '$state', 'ngToast', 'config', 'LicenseService', 'WorkgroupService', 'AuthService', 'PaymentService',
        function ($rootScope, $scope, $filter, $popover, $state, ngToast, config, LicenseService, WorkgroupService, AuthService, PaymentService) {
            $scope.loading = true;

            LicenseService.getLicense()
                .then(function onSuccess(response) {
                    $scope.licenseInfo = response.data;
                    $scope.loading = false;

                    setPrices();
                });

            PaymentService.getPayment()
                .then(function onSuccess(response) {
                    $scope.payment = response.data;
                });

            function setPrices() {
                $scope.licensePrice = 45;
                $scope.addonPrice = 45;
                $scope.addonStoragePrice = 10;

                if ($rootScope.user.currency === 'USD') {
                    $scope.licensePrice = 10;
                    $scope.addonPrice = 10;
                    $scope.addonStoragePrice = 4;
                }

                if ($scope.licenseInfo.CurrentPaymentFrequency === 12) {
                    $scope.licensePrice = 37.5;
                    $scope.addonPrice = 37.5;
                    $scope.addonStoragePrice = 10;

                    if ($rootScope.user.currency === 'USD') {
                        $scope.licensePrice = 8.5;
                        $scope.addonPrice = 8.5;
                        $scope.addonStoragePrice = 4;
                    }
                }
            }

            function setParams() {
                var nextInvoiceDate = $filter('msDate')($rootScope.licenseStatus.NextInvoiceDate);
                nextInvoiceDate = $filter('date')(nextInvoiceDate, 'dd MMMM yyyy');
                $scope.notAllowedParams = { date: nextInvoiceDate };

                var nextPaymentDate = $filter('msDate')($scope.licenseInfo.AvailableLicenses[0].NextPaymentDate);
                nextPaymentDate = $filter('date')(nextPaymentDate, 'dd MMMM yyyy');

                var priceMounth = 29.5;
                var priceYear = 294;

                if ($rootScope.user.currency === 'USD') {
                    priceMounth = 11.5;
                    priceYear = 114;
                }

                var price = $scope.licenseInfo.CurrentPaymentFrequency === 1 ? priceYear : priceMounth;
                var period = $scope.licenseInfo.CurrentPaymentFrequency === 1 ? $filter('translate')('Setup.License.LicenseTypeAnnual') : $filter('translate')('Setup.License.LicenseTypeMountly');
                $scope.changeParams = { date: nextPaymentDate, price: price, currency: $rootScope.user.currency.toLowerCase(), period: period.toLowerCase() };
            }

            $scope.showChangePopover = function () {
                setParams();

                $scope.changePopover = $scope.changePopover || $popover(angular.element(document.getElementById('changeButton')), {
                        templateUrl: 'view/setup/license/licenseChange.html',
                    placement: 'right',
                    scope: $scope,
                    show: true
                });
            };

            $scope.change = function () {
                $scope.licenseChanging = true;
                var licenseId = $rootScope.licenseStatus.License.id;
                var frequency = $scope.licenseInfo.CurrentPaymentFrequency === 1 ? 12 : 1;

                LicenseService.change(licenseId, frequency)
                    .then(function onSuccess() {
                        LicenseService.getLicenseStatus()
                            .then(function onSuccess(licenseStatusData) {
                                $rootScope.licenseStatus = licenseStatusData.data;

                                LicenseService.getLicense()
                                    .then(function onSuccess(licenseInfoData) {
                                        $scope.licenseInfo = licenseInfoData.data;
                                        setParams();
                                        setPrices();

                                        $scope.changePopover.hide();
                                        $scope.licenseChanging = false;
                                        ngToast.create({ content: $filter('translate')('Setup.License.ChangeSuccess'), className: 'success' });
                                    })
                                    .catch(function onError() {
                                        $scope.changePopover.hide();
                                        $scope.licenseChanging = false;
                                    });
                            })
                            .catch(function onError() {
                                $scope.changePopover.hide();
                                $scope.licenseChanging = false;
                            });
                    })
                    .catch(function onError() {
                        $scope.changePopover.hide();
                        $scope.licenseChanging = false;
                    });
            };

            $scope.showAddonPopover = function () {
                setParams();

                $scope.addonPopover = $scope.addonPopover || $popover(angular.element(document.getElementById('addAddonButton')), {
                        templateUrl: 'view/setup/license/licenseAddon.html',
                    placement: 'right',
                    scope: $scope,
                    show: true
                });
            };

            $scope.gotoPaymentInfo = function () {
                var menuItem = $filter('filter')($scope.$parent.menuItems, { link: '#/app/setup/payment' })[0];
                $scope.$parent.selectMenuItem(menuItem);

                $state.go('app.setup.payment');
            };

            $scope.addonModel = {};
            $scope.addonModel.user = 0;
            $scope.addonModel.disk = 0;

            $scope.addAddon = function () {
                $scope.addonAdding = true;
                var addonModel = $scope.addonModel;

                LicenseService.addAddonLicense(addonModel.user, addonModel.disk)
                    .then(function onSuccess() {
                        LicenseService.getLicenseStatus()
                            .then(function onSuccess(licenseStatusData) {
                                $rootScope.licenseStatus = licenseStatusData.data;

                                LicenseService.getLicense()
                                    .then(function onSuccess(licenseInfoData) {
                                        $scope.licenseInfo = licenseInfoData.data;
                                        setParams();

                                        $scope.addonModel.user = 0;
                                        $scope.addonModel.disk = 0;

                                        $scope.addonAdding = false;
                                        ngToast.create({ content: $filter('translate')('Setup.License.AddonSuccess'), className: 'success' });
                                        $scope.addonPopover.hide();
                                    })
                                    .catch(function onError() {
                                        $scope.addonPopover.hide();
                                        $scope.addonAdding = false;
                                    });
                            })
                            .catch(function onError() {
                                $scope.addonPopover.hide();
                                $scope.addonAdding = false;
                            });
                    })
                    .catch(function onError() {
                        $scope.addonPopover.hide();
                        $scope.addonAdding = false;
                    });
            };

            $scope.showRemovePopover = function (type) {
                $scope.removeType = type;

                if (type === 'user') {
                    $scope.removePopoverUser = $scope.removePopoverUser || $popover(angular.element(document.getElementById('removeButtonUser')), {
                            templateUrl: 'view/setup/license/licenseRemove.html',
                        placement: 'left',
                        scope: $scope,
                        show: true
                    });
                    $scope.removePopoverStorage && $scope.removePopoverStorage.hide();
                }
                else {
                    $scope.removePopoverStorage = $scope.removePopoverStorage || $popover(angular.element(document.getElementById('removeButtonStorage')), {
                            templateUrl: 'view/setup/license/licenseRemove.html',
                        placement: 'left',
                        scope: $scope,
                        show: true
                    });
                    $scope.removePopoverUser && $scope.removePopoverUser.hide();
                }
            };

            $scope.removeAddon = function () {
                $scope.addonRemoving = true;

                LicenseService.removeAddonLicense($scope.removeType)
                    .then(function onSuccess() {
                        LicenseService.getLicenseStatus()
                            .then(function onSuccess(licenseStatusData) {
                                $rootScope.licenseStatus = licenseStatusData.data;

                                LicenseService.getLicense()
                                    .then(function onSuccess(licenseInfoData) {
                                        $scope.licenseInfo = licenseInfoData.data;
                                        setParams();

                                        $scope.addonRemoving = false;
                                        ngToast.create({ content: $filter('translate')('Setup.License.RemoveSuccess'), className: 'success' });
                                        $scope.removePopoverUser && $scope.removePopoverUser.hide();
                                        $scope.removePopoverStorage && $scope.removePopoverStorage.hide();
                                    })
                                    .catch(function onError() {
                                        $scope.removePopoverUser.hide();
                                        $scope.removePopoverStorage.hide();
                                        $scope.addonRemoving = false;
                                    });
                            })
                            .catch(function onError() {
                                $scope.removePopoverUser.hide();
                                $scope.removePopoverStorage.hide();
                                $scope.addonRemoving = false;
                            });
                    })
                    .catch(function onError() {
                        $scope.removePopoverUser.hide();
                        $scope.removePopoverStorage.hide();
                        $scope.addonRemoving = false;
                    });
            };

            $scope.upgrade = function () {
                $scope.upgrading = true;

                WorkgroupService.upgradeLicense(config.planIdMembers, 1)
                    .then(function onSuccess() {
                        AuthService.logout()
                            .then(function onSuccess() {
                                AuthService.logoutComplete();
                                ngToast.create({ content: $filter('translate')('Join.JoinSuccess'), className: 'success', timeout: 8000 });
                            })
                            .catch(function onError() {
                                $scope.upgrading = false;
                            });
                    })
                    .catch(function onError() {
                        $scope.upgrading = false;
                    });
            };
        }
    ]);