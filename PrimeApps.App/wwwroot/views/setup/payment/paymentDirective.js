'use strict';

angular.module('primeapps')

    .directive('paymentForm', ['$filter', 'ngToast', 'PaymentService', 'AppService',
        function ($filter, ngToast, PaymentService, AppService) {
            return {
                restrict: 'EA',
                scope: {
                    payment: '='
                },
                templateUrl: 'views/setup/payment/paymentForm.html',
                controller: ['$rootScope', '$scope',
                    function ($rootScope, $scope) {
                        $scope.expirationMonths = [];
                        $scope.expirationYears = [];
                        $scope.user = $rootScope.user;

                        for (var i = 1; i < 13; i++) {
                            $scope.expirationMonths.push(i);
                        }

                        var currentYear = new Date().getFullYear();

                        for (var j = currentYear - 1; j < 2050; j++) {
                            $scope.expirationYears.push(j + 1);
                        }

                        cardsOff();

                        function cardsOff() {
                            $scope.visaCard = 'off';
                            $scope.masterCard = 'off';
                            $scope.maestroCard = 'off';
                            $scope.amexCard = 'off';
                            $scope.discoverCard = 'off';
                            $scope.jcbCard = 'off';
                        }

                        var cardsRegex;
                        cardsRegex = {
                            visa: /^4[0-9]{12}(?:[0-9]{3})?$/,
                            mastercard: /^5[1-5][0-9]{14}$/,
                            maestro: /^6[7-9][0-9]{0,17}$/,
                            amex: /^3[47][0-9]{13}$/,
                            diners: /^3(?:0[0-5]|[68][0-9])[0-9]{11}$/,
                            discover: /^6(?:011|5[0-9]{2})[0-9]{12}$/,
                            jcb: /^(?:2131|1800|35\d{3})\d{11}$/
                        };

                        $scope.changeCardType = function (cardNumber) {
                            cardsOff();

                            if (cardsRegex.visa.test(cardNumber)) {
                                $scope.visaCard = 'on';
                            } else if (cardsRegex.mastercard.test(cardNumber)) {
                                $scope.masterCard = 'on';
                            } else if (cardsRegex.maestro.test(cardNumber)) {
                                $scope.maestroCard = 'on';
                            } else if (cardsRegex.amex.test(cardNumber)) {
                                $scope.amexCard = 'on';
                            } else if (cardsRegex.discover.test(cardNumber)) {
                                $scope.discoverCard = 'on';
                            } else if (cardsRegex.jcb.test(cardNumber)) {
                                $scope.jcbCard = 'on';
                            }
                        };

                        function validateCard(cardNumber) {
                            if (!cardNumber)
                                return;

                            $scope.paymentInfoForm.cardNumber.$setValidity('cardNumber', false);

                            for (var rgx in cardsRegex) {
                                if (cardsRegex[rgx].test(cardNumber)) {
                                    $scope.paymentInfoForm.cardNumber.$setValidity('cardNumber', true);
                                    return;
                                }
                            }
                        }

                        $scope.update = function (payment) {
                            validateCard(payment.CardNumber);
                            $scope.$broadcast('show-errors-check-validity');

                            if ($scope.paymentInfoForm.$valid) {
                                $scope.paymentUpdating = true;

                                PaymentService.update(payment)
                                    .then(function () {
                                        $scope.paymentUpdating = false;

                                        if ($scope.$root.licenseStatus)
                                            $scope.$root.licenseStatus.LicenseUsage.IsInNotificationPeriod = false;

                                        AppService.getMyAccount(true);

                                        ngToast.create({content: $filter('translate')('Setup.Settings.UpdateSuccess'), className: 'success'});
                                    })
                                    .catch(function () {
                                        $scope.paymentUpdating = false;
                                    })
                            }
                        };

                        $scope.updateCampaign = function (payment) {
                            $scope.campaingForm.campaignCode.$setValidity('campaignCode', true);

                            if ($scope.campaingForm.$valid) {
                                $scope.campaignUpdating = true;

                                PaymentService.checkCampaignCode(payment.CampaignCode)
                                    .then(function (response) {
                                        if (!response.data) {
                                            $scope.campaingForm.campaignCode.$setValidity('campaignCode', false);
                                            $scope.campaignUpdating = false;
                                            return;
                                        }

                                        PaymentService.updateCampaign(payment.ID, payment.CampaignCode)
                                            .then(function () {
                                                $scope.campaignUpdating = false;

                                                ngToast.create({content: $filter('translate')('Setup.Payment.CampaignUpdateSuccess'), className: 'success'});
                                            })
                                            .catch(function () {
                                                $scope.campaignUpdating = false;
                                            })
                                    });
                            }
                        };

                        $scope.changeCurrency = function (code) {
                            PaymentService.changeCurrency(code)
                                .then(function () {
                                    $rootScope.user.currency = code;
                                    ngToast.create({content: $filter('translate')('Setup.Settings.CurrencySuccess'), className: 'success'});
                                });
                        };
                    }]
            };
        }]);