'use strict';

angular.module('primeapps')
    .controller('PurchaseProductsController', ['$rootScope', '$scope', '$state', 'config', 'ngToast', '$localStorage', '$filter', 'ngTableParams', '$stateParams', 'helper', 'PurchaseProductsService', 'ModuleService', '$popover',
        function ($rootScope, $scope, $state, config, ngToast, $localStorage, $filter, ngTableParams, $stateParams, helper, PurchaseProductsService, ModuleService, $popover) {
            if ($scope.$parent.$parent.type != 'purchase_orders')
                return;

            $scope.isMobile = false;

            if (typeof window.orientation !== 'undefined' || window.innerWidth <= 500) {
                $scope.isMobile = true;
            }

            $scope.purchaseProductModule = $filter('filter')($rootScope.modules, { name: 'purchase_order_products' }, true)[0];

            if (!$scope.purchaseProductModule) {
                ngToast.create({ content: $filter('translate')('Common.NotFound'), className: 'warning' });
                $state.go('app.dashboard');
                return;
            }

            $scope.purchaseFields = [];
            $scope.purchaseModule = $filter('filter')($rootScope.modules, { name: 'purchase_orders' }, true)[0];
            $scope.purchaseModule.fields.forEach(function (field) {
                $scope.purchaseFields[field.name] = field;
            });

            $scope.fields = [];
            $scope.purchaseProductModule.fields.forEach(function (field) {
                $scope.fields[field.name] = field;
            });

            $scope.productField = $filter('filter')($scope.purchaseProductModule.fields, { name: 'product' }, true)[0];
            $scope.productModule = $filter('filter')($rootScope.modules, { name: 'products' }, true)[0];
            $scope.productField.lookupModulePrimaryField = $filter('filter')($scope.productModule.fields, { name: 'name' }, true)[0];

            $scope.productFields = [];
            angular.forEach($scope.productModule.fields, function (productField) {
                $scope.productFields[productField.name] = productField;
            });

            ModuleService.getPicklists($scope.purchaseProductModule)
                .then(function (picklists) {
                    $scope.picklistsModule = picklists
                    $scope.usageUnitList = $scope.picklistsModule[$scope.fields['usage_unit'].picklist_id];
                    $scope.currencyList = $scope.picklistsModule[$scope.fields['currency'].picklist_id];
                    $scope.defaultCurrency = $filter('filter')($scope.currencyList, {value: $rootScope.currencySymbol})[0];
                });
            ModuleService.getPicklists($scope.productModule)
                .then(function (picklists) {
                    $scope.productModulePicklists = picklists;
                });

            $scope.setCurrentLookupProduct = function (product, field) {
                $scope.currentLookupProduct = product;
                $scope.productSelected = true;
                field.special_type = "quate_products";
                field.currentproduct = product;
                field.selectProduct = $scope.selectProduct;
                if (field.currentproduct.defaultCurrency || field.currentproduct.currencyConvertList) {
                    delete field.currentproduct.defaultCurrency;
                    delete field.currentproduct.currencyConvertList;
                }
                $scope.$parent.setCurrentLookupField(field);
            };

            var additionalFields = ['unit_price', 'usage_unit', 'vat_percent'];

            if ($scope.fields['purchase_price'])
                additionalFields.push("purchase_price");

            if ($scope.fields['currency'])
                additionalFields.push("currency");

            $scope.lookup = function (searchTerm) {
                return ModuleService.lookup(searchTerm, $scope.productField, $scope.currentLookupProduct, additionalFields);
            };

            $scope.addPurchaseProduct = function (separator) {

                var purchaseProduct = {};
                purchaseProduct.id = 0;
                purchaseProduct.discount_type = 'percent';
                if (separator) {
                    purchaseProduct.separator = "";
                    purchaseProduct.product = {};
                }

                var sortOrders = [];

                angular.forEach($scope.$parent.$parent.purchaseProducts, function (purchaseProduct) {
                    sortOrders.push(purchaseProduct.order);
                });

                purchaseProduct.order = Math.max.apply(null, sortOrders) + 1;

                if (!purchaseProduct.order || purchaseProduct.order < 1)
                    purchaseProduct.order = 1;

                $scope.$parent.$parent.purchaseProducts.push(purchaseProduct);
            };

            $scope.currencyConvert = function (purchaseProduct, value) {

                var currencyConvertList = [];
                switch (purchaseProduct.defaultCurrency) {
                    case '₺':
                        currencyConvertList['$'] = value * $scope.$parent.$parent.record.exchange_rate_usd_try;
                        currencyConvertList['€'] = value * $scope.$parent.$parent.record.exchange_rate_eur_try;
                        currencyConvertList['₺'] = value;
                        break;
                    case '$':

                        currencyConvertList['₺'] = value * $scope.$parent.$parent.record.exchange_rate_try_usd;
                        currencyConvertList['€'] = value * $scope.$parent.$parent.record.exchange_rate_eur_usd;
                        currencyConvertList['$'] = value;
                        break;
                    case '€':
                        currencyConvertList['₺'] = value * $scope.$parent.$parent.record.exchange_rate_try_eur;
                        currencyConvertList['$'] = value * $scope.$parent.$parent.record.exchange_rate_usd_eur;
                        currencyConvertList['€'] = value;
                        break;
                }
                return currencyConvertList;

            };

            $scope.setVat = function (purchaseProduct) {
                if (purchaseProduct.product.vat_percent != undefined) {
                    purchaseProduct.vat_percent = purchaseProduct.product.vat_percent;
                }
            };
            $scope.selectProduct = function (purchaseProduct) {
                if (!purchaseProduct.product)
                    return;

                if (!purchaseProduct.defaultCurrency) {

                    purchaseProduct.defaultCurrency = purchaseProduct.product.currency ? purchaseProduct.product.currency.value : $rootScope.currencySymbol;
                    //purchaseProduct.currencyConvertList = $scope.currencyConvert(purchaseProduct, purchaseProduct.product.unit_price);
                    purchaseProduct.currencyConvertList = $scope.currencyConvert(purchaseProduct, purchaseProduct.product.purchase_price);

                } else {

                    purchaseProduct.product.purchase_price = purchaseProduct.currencyConvertList[purchaseProduct.product.currency.value];
                    purchaseProduct.product.purchase_price = purchaseProduct.currencyConvertList[purchaseProduct.product.currency ? purchaseProduct.product.currency.value : $rootScope.currencySymbol];

                }

                var purchasePrice = parseFloat(purchaseProduct.product.purchase_price);

                if (!purchaseProduct.quantity)
                    purchaseProduct.quantity = 1;
                purchaseProduct.purchase_price = purchasePrice;
                purchaseProduct.amount = purchasePrice;

                if (purchaseProduct.product.usage_unit) {
                    if (!angular.isObject(purchaseProduct.product.usage_unit)) {
                        $scope.usageUnitList.forEach(function (unit) {
                            if (purchaseProduct.product.usage_unit === unit.labelStr) {
                                purchaseProduct.product.usage_unit = unit;
                            }
                        });
                    }
                    else {
                        purchaseProduct.usage_unit = purchaseProduct.product.usage_unit;
                    }
                }

                if (purchaseProduct.product.currency) {
                    purchaseProduct.currency = purchaseProduct.product.currency;
                }
                else {
                    purchaseProduct.currency = $scope.defaultCurrency;
                    purchaseProduct.product.currency = $scope.defaultCurrency;
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
                var purchasePrice = 0;

                if (!isNaN(purchaseProduct.quantity))
                    quantity = angular.copy(purchaseProduct.quantity);

                if (!isNaN(purchaseProduct.purchase_price))
                    purchasePrice = angular.copy(purchaseProduct.purchase_price);


                if ($scope.fields['no']) {
                    purchaseProduct.no = 0;
                }

                purchaseProduct.amount = quantity * purchasePrice;
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
                        purchaseProduct.discount_amount = ((purchasePrice * quantity) * purchaseProduct.discount_percent) / 100;
                        if ($scope.fields['unit_amount']) {
                            purchaseProduct.unit_amount = purchaseProduct.amount / quantity;
                        }
                        break;
                    case 'amount':
                        if (purchaseProduct.discount_amount != undefined && purchaseProduct.discount_amount != null && !isNaN(purchaseProduct.discount_amount)) {
                            // if (purchaseProduct.discount_amount > purchaseProduct.unit_price) {
                            //purchaseProduct.discount_amount = purchaseProduct.unit_price * purchaseProduct.quantity;
                            // }

                            purchaseProduct.amount -= purchaseProduct.discount_amount;

                            purchaseProduct.discount_percent = (100 / (purchasePrice * quantity)) * purchaseProduct.discount_amount;
                        }
                        if ($scope.fields['unit_amount']) {
                            purchaseProduct.unit_amount = purchaseProduct.amount / quantity;
                        }
                        break;
                }
                if (purchaseProduct.product.purchase_price && (purchaseProduct.purchase_price === undefined || purchaseProduct.purchase_price === null)) {
                    // purchaseProduct.purchase_price = $scope.currencyConvert(purchaseProduct, purchaseProduct.product.purchase_price)[purchaseProduct.product.currency.value];
                } else {
                    // purchaseProduct.purchase_price = $scope.currencyConvert(purchaseProduct, purchaseProduct.product.purchase_price)[purchaseProduct.product.currency.value];
                }
                if (purchaseProduct.purchase_price != undefined || purchaseProduct.purchase_price != null) {
                    purchaseProduct.profit_amount = purchaseProduct.amount - purchaseProduct.purchase_price * quantity;
                    purchaseProduct.profit_percent = ((purchaseProduct.amount - (quantity * purchaseProduct.purchase_price)) / (quantity * purchaseProduct.purchase_price)) * 100;
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
                    if (!$scope.$parent.$parent.currencyField.validation)
                        $scope.$parent.$parent.currencyField.validation = {};

                    $scope.$parent.$parent.currencyField.validation.readonly = true;
                }
                var counter = 0;
                angular.forEach($scope.$parent.$parent.purchaseProducts, function (purchaseProduct) {
                    if (!purchaseProduct.amount || purchaseProduct.deleted)
                        return;

                    counter++;
                    if (purchaseProduct.quantity < 0) {
                        purchaseProduct.quantity = 0;
                    }

                    if (purchaseProduct.purchase_price < 0) {
                        purchaseProduct.purchase_price = 0;
                    }

                    if (purchaseProduct.purchase_price > 1000000000000) {
                        purchaseProduct.purchase_price = 1000000000000;
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
                        vatItem.total = vat;

                        if (vat)
                            vatList.push(vatItem);
                    }
                    else {
                        vatItem.total += vat || 0;
                    }
                });
                if ($scope.$parent.$parent.currencyField && $scope.$parent.$parent.record['currency'] && counter < 1) {
                    if (!$scope.$parent.$parent.currencyField.validation)
                        $scope.$parent.$parent.currencyField.validation = {};

                    $scope.$parent.$parent.currencyField.validation.readonly = false;
                }
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

            $scope.up = function (index, order) {
                var purchaseProducts = $filter('filter')($scope.$parent.$parent.purchaseProducts, { deleted: false });
                purchaseProducts = $filter('orderBy')($scope.$parent.$parent.purchaseProducts, 'order');

                var prev = angular.copy(purchaseProducts[index - 1]);
                purchaseProducts[index].order = prev.order;
                purchaseProducts[index - 1].order = angular.copy(order);
            };
            $scope.down = function (index, order) {
                var purchaseProducts = $filter('filter')($scope.$parent.$parent.purchaseProducts, { deleted: false });
                purchaseProducts = $filter('orderBy')($scope.$parent.$parent.purchaseProducts, 'order');

                var prev = angular.copy(purchaseProducts[index + 1]);
                purchaseProducts[index].order = prev.order;
                purchaseProducts[index + 1].order = angular.copy(order);
            };
            $scope.productInfo = function (purchaseProduct) {
                $scope.currentProduct = purchaseProduct;
            }
        }
    ]);