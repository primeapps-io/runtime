'use strict';

angular.module('ofisim')
    .controller('PurchaseProductsController', ['$rootScope', '$scope', '$state', 'config', 'ngToast', '$localStorage', '$filter', 'ngTableParams', '$stateParams', 'PurchaseProductsService', 'ModuleService',
        function ($rootScope, $scope, $state, config, ngToast, $localStorage, $filter, ngTableParams, $stateParams, PurchaseProductsService, ModuleService) {

            if ($scope.$parent.$parent.type != 'purchase_orders')
                return;

            $scope.isMobile = false;

            if (typeof window.orientation !== 'undefined' || window.innerWidth <= 500) {
                $scope.isMobile = true;
            }

            $scope.purchaseProductModule = $filter('filter')($rootScope.modules, { name: 'purchase_order_products' }, true)[0];

            if (!$scope.purchaseProductModule) {
                ngToast.create({ content: $filter('translate')('Common.NotFound'), className: 'warning' });
                $state.go('app.crm.dashboard');
                return;
            }

            $scope.productField = $filter('filter')($scope.purchaseProductModule.fields, { name: 'product' }, true)[0];
            $scope.productModule = $filter('filter')($rootScope.modules, { name: 'products' }, true)[0];
            $scope.productField.lookupModulePrimaryField = $filter('filter')($scope.productModule.fields, { name: 'name' }, true)[0];

            ModuleService.getPicklists($scope.purchaseProductModule)
                .then(function (picklists) {
                    $scope.picklistsModule = picklists;
                });

            $scope.setCurrentLookupProduct = function (product, field) {
                $scope.currentLookupProduct = product;
                $scope.productSelected = true;

                field.special_type = "purchase_products";
                field.currentproduct = product;
                field.selectProduct = $scope.selectProduct;
                $scope.$parent.setCurrentLookupField(field);
            };

            $scope.lookup = function (searchTerm) {
                var additionalFields = ['unit_price', 'usage_unit', 'vat_percent'];

                if ($scope.$parent.$parent.currencyField)
                    additionalFields.push('currency');

                return ModuleService.lookup(searchTerm, $scope.productField, $scope.currentLookupProduct, additionalFields);
            };

            $scope.addPurchaseProduct = function () {
                var purchaseProduct = {};
                purchaseProduct.id = 0;
                purchaseProduct.discount_type = 'percent';
                var sortOrders = [];

                angular.forEach($scope.$parent.$parent.purchaseProducts, function (purchaseProduct) {
                    sortOrders.push(purchaseProduct.order);
                });

                purchaseProduct.order = Math.max.apply(null, sortOrders) + 1;

                if (!purchaseProduct.order || purchaseProduct.order < 1)
                    purchaseProduct.order = 1;

                $scope.$parent.$parent.purchaseProducts.push(purchaseProduct);
            };

            $scope.selectProduct = function (purchaseProduct) {
                if (!purchaseProduct.product)
                    return;

                var unitPrice = 0;

                if (!purchaseProduct.quantity)
                    purchaseProduct.quantity = 1;

                if ($scope.productSelected) {
                    purchaseProduct.unit_price = unitPrice;
                    purchaseProduct.amount = unitPrice;

                    if (purchaseProduct.product.usage_unit) {
                        purchaseProduct.usage_unit = purchaseProduct.product.usage_unit.label[$rootScope.language];
                    }
                }

                $scope.calculate(purchaseProduct);

            };

            if (!$scope.$parent.$parent.id) {
                $scope.$parent.$parent.record.total = 0;
                $scope.$parent.$parent.record.vat_total = 0;
                $scope.$parent.$parent.record.grand_total = 0;
                $scope.$parent.$parent.record.discount_type = 'percent';
            }

            if ($scope.$parent.$parent.isDetail) {
                $scope.readonly = true;
            }

            $scope.calculate = function (purchaseProduct) {
                var quantity = 0;
                var unitPrice = 0;

                if (!isNaN(purchaseProduct.quantity))
                    quantity = angular.copy(purchaseProduct.quantity);

                if (!isNaN(purchaseProduct.unit_price))
                    unitPrice = angular.copy(purchaseProduct.unit_price);

                purchaseProduct.amount = quantity * unitPrice;

                switch (purchaseProduct.discount_type) {
                    case 'percent':
                        if ($scope.$parent.$parent.record.contact && $scope.$parent.$parent.record.contact.discount && purchaseProduct.discount_percent === undefined && purchaseProduct.discount_percent !== null) {
                            if (angular.isObject($scope.$parent.$parent.record.contact.discount))
                                purchaseProduct.discount_percent = $scope.$parent.$parent.record.contact.discount.value;
                            else
                                purchaseProduct.discount_percent = $scope.$parent.$parent.record.contact.discount;
                        }

                        if ($scope.$parent.$parent.record.account && $scope.$parent.$parent.record.account.discount && purchaseProduct.discount_percent === undefined && purchaseProduct.discount_percent !== null) {
                            if (angular.isObject($scope.$parent.$parent.record.account.discount))
                                purchaseProduct.discount_percent = $scope.$parent.$parent.record.account.discount.value;
                            else
                                purchaseProduct.discount_percent = $scope.$parent.$parent.record.account.discount;
                        }

                        if (purchaseProduct.discount_percent != undefined && purchaseProduct.discount_percent != null && !isNaN(purchaseProduct.discount_percent)) {
                            if (purchaseProduct.discount_percent < 0) {
                                purchaseProduct.discount_percent = 0;
                            }

                            if (purchaseProduct.discount_percent > 100) {
                                purchaseProduct.discount_percent = 100;
                            }

                            purchaseProduct.amount -= (purchaseProduct.amount * purchaseProduct.discount_percent) / 100;
                        }
                        purchaseProduct.discount_amount = null;
                        break;
                    case 'amount':
                        if (purchaseProduct.discount_amount != undefined && purchaseProduct.discount_amount != null && !isNaN(purchaseProduct.discount_amount)) {
                            if (purchaseProduct.discount_amount > purchaseProduct.unit_price) {
                                purchaseProduct.discount_amount = purchaseProduct.unit_price;
                            }

                            purchaseProduct.amount -= purchaseProduct.discount_amount;
                        }

                        purchaseProduct.discount_percent = null;
                        break;
                }

                var vat = parseFloat(purchaseProduct.product.vat_percent || 0);
                purchaseProduct.vat = (purchaseProduct.amount * vat) / 100;
                $scope.calculateAll();
            };

            $scope.calculateAll = function () {
                var total = 0;
                var vatTotal = 0;
                var discount = 0;
                var vatList = [];
                if ($scope.$parent.$parent.currencyField && $scope.$parent.$parent.record['currency']) {
                    $scope.$parent.$parent.currencyField.validation.readonly = true;
                }
                angular.forEach($scope.$parent.$parent.purchaseProducts, function (purchaseProduct) {
                    if (!purchaseProduct.amount || purchaseProduct.deleted)
                        return;

                    if (purchaseProduct.quantity < 0) {
                        purchaseProduct.quantity = 0;
                    }

                    if (purchaseProduct.unit_price < 0) {
                        purchaseProduct.unit_price = 0;
                    }

                    if (purchaseProduct.unit_price > 1000000000000) {
                        purchaseProduct.unit_price = 1000000000000;
                    }

                    var amount = angular.copy(purchaseProduct.amount);
                    var vat = angular.copy(purchaseProduct.vat) || 0;
                    if (purchaseProduct.product.currency && $scope.$parent.$parent.record.currency && purchaseProduct.product.currency.value && $scope.$parent.$parent.record.currency.value && purchaseProduct.product.currency.value != $scope.$parent.$parent.record.currency.value) {
                        switch ($scope.$parent.$parent.record.currency.value) {
                            case '₺':
                                if (purchaseProduct.product.currency.value === '$') {
                                    amount = amount * $scope.$parent.$parent.record.exchange_rate_try_usd;
                                    vat = vat * $scope.$parent.$parent.record.exchange_rate_try_usd;
                                }
                                else if (purchaseProduct.product.currency.value === '€') {
                                    amount = amount * $scope.$parent.$parent.record.exchange_rate_try_eur;
                                    vat = vat * $scope.$parent.$parent.record.exchange_rate_try_eur;
                                }
                                break;
                            case '$':
                                if (purchaseProduct.product.currency.value === '₺') {
                                    amount = amount * $scope.$parent.$parent.record.exchange_rate_usd_try;
                                    vat = vat * $scope.$parent.$parent.record.exchange_rate_usd_try;
                                }
                                else if (purchaseProduct.product.currency.value === '€') {
                                    amount = amount * $scope.$parent.$parent.record.exchange_rate_usd_eur;
                                    vat = vat * $scope.$parent.$parent.record.exchange_rate_usd_eur;
                                }
                                break;
                            case '€':
                                if (purchaseProduct.product.currency.value === '₺') {
                                    amount = amount * $scope.$parent.$parent.record.exchange_rate_eur_try;
                                    vat = vat * $scope.$parent.$parent.record.exchange_rate_eur_try;
                                }
                                else if (purchaseProduct.product.currency.value === '$') {
                                    amount = amount * $scope.$parent.$parent.record.exchange_rate_eur_usd;
                                    vat = vat * $scope.$parent.$parent.record.exchange_rate_eur_usd;
                                }
                                break;
                        }
                    }

                    total += amount;
                    vatTotal += vat;

                    var vatItem = $filter('filter')(vatList, { percent: purchaseProduct.product.vat_percent }, true)[0];

                    if (!vatItem) {
                        vatItem = {};
                        vatItem.percent = parseFloat(purchaseProduct.product.vat_percent || 0);
                        vatItem.total = purchaseProduct.vat;

                        if (vat)
                            vatList.push(vatItem);
                    }
                    else {
                        vatItem.total += purchaseProduct.vat || 0;
                    }
                });

                if ($scope.$parent.$parent.record.discount_percent || $scope.$parent.$parent.record.discount_amount) {
                    switch ($scope.$parent.$parent.record.discount_type) {
                        case 'percent':
                            if ($scope.$parent.$parent.record.discount_percent != undefined && $scope.$parent.$parent.record.discount_percent != null && !isNaN($scope.$parent.$parent.record.discount_percent)) {
                                if ($scope.$parent.$parent.record.discount_percent > 100) {
                                    $scope.$parent.$parent.record.discount_percent = 100;
                                }

                                discount = (total * $scope.$parent.$parent.record.discount_percent) / 100;
                                vatTotal -= (vatTotal * $scope.$parent.$parent.record.discount_percent) / 100;

                                angular.forEach(vatList, function (vat) {
                                    vat.total -= (vat.total * $scope.$parent.$parent.record.discount_percent) / 100;
                                });
                            }
                            $scope.$parent.$parent.record.discount_amount = null;
                            break;
                        case 'amount':
                            if ($scope.$parent.$parent.record.discount_amount != undefined && $scope.$parent.$parent.record.discount_amount != null && !isNaN($scope.$parent.$parent.record.discount_amount)) {
                                discount = $scope.$parent.$parent.record.discount_amount;
                                var percent = ($scope.$parent.$parent.record.discount_amount * 100) / total;
                                vatTotal -= (vatTotal * percent) / 100;

                                angular.forEach(vatList, function (vat) {
                                    vat.total -= (vat.total * percent) / 100;
                                });
                            }
                            $scope.$parent.$parent.record.discount_percent = null;
                            break;
                    }
                }

                $scope.$parent.$parent.record.total = total;
                $scope.$parent.$parent.record.discounted_total = discount ? total - discount : undefined;
                $scope.$parent.$parent.record.vat_total = vatTotal;
                $scope.$parent.$parent.record.grand_total = total - discount + vatTotal;
                $scope.$parent.$parent.record.vat_list = $scope.prepareVatList(vatList);
                $scope.$parent.$parent.vatList = vatList;
            };

            $scope.delete = function (purchaseProduct) {
                purchaseProduct.deleted = true;
                $scope.calculateAll();
            };

            $scope.prepareVatList = function (vatList) {
                if (!vatList)
                    return;

                var vatListStr = '';

                angular.forEach(vatList, function (vat) {
                    vatListStr += vat.percent + ';' + vat.total + '|';
                });

                return vatListStr.slice(0, -1);
            };
        }
    ]);