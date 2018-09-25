'use strict';

angular.module('ofisim')
    .controller('PurchaseInvoiceProductsController', ['$rootScope', '$scope', '$state', 'config', 'ngToast', '$localStorage', '$filter', 'ngTableParams', '$stateParams', 'helper', 'QuoteProductsService', 'ModuleService', '$popover',
        function ($rootScope, $scope, $state, config, ngToast, $localStorage, $filter, ngTableParams, $stateParams, helper, QuoteProductsService, ModuleService, $popover) {
            if ($scope.$parent.$parent.type != 'purchase_invoices')
                return;

            $scope.isMobile = false;

            if (typeof window.orientation !== 'undefined' || window.innerWidth <= 500) {
                $scope.isMobile = true;
            }

            $scope.purchaseInvoiceProductModule = $filter('filter')($rootScope.modules, {name: 'purchase_invoices_products'}, true)[0];

            if (!$scope.purchaseInvoiceProductModule) {
                ngToast.create({content: $filter('translate')('Common.NotFound'), className: 'warning'});
                $state.go('app.crm.dashboard');
                return;
            }
            $scope.fields = [];

            $scope.purchaseInvoiceProductModule.fields.forEach(function (field) {
                $scope.fields[field.name] = field;
            });

            $scope.purchaseInvoiceFields = [];
            $scope.purchaseInvoiceModule = $filter('filter')($rootScope.modules, {name: 'purchase_invoices'}, true)[0];
            $scope.purchaseInvoiceModule.fields.forEach(function (field) {
                $scope.purchaseInvoiceFields[field.name] = field;
            });

            $scope.productField = $filter('filter')($scope.purchaseInvoiceProductModule.fields, {name: 'product'}, true)[0];
            $scope.productModule = $filter('filter')($rootScope.modules, {name: 'products'}, true)[0];
            $scope.productField.lookupModulePrimaryField = $filter('filter')($scope.productModule.fields, {name: 'name'}, true)[0];

            $scope.productFields = [];
            angular.forEach($scope.productModule.fields, function (productField) {
                $scope.productFields[productField.name] = productField;
            });

            ModuleService.getPicklists($scope.purchaseInvoiceProductModule)
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

            $scope.addPurchaseInvoiceProduct = function (separator) {

                var purchaseInvoiceProduct = {};
                purchaseInvoiceProduct.id = 0;
                purchaseInvoiceProduct.discount_type = 'percent';
                if (separator) {
                    purchaseInvoiceProduct.separator = "";
                    purchaseInvoiceProduct.product = {};
                }

                var sortOrders = [];

                angular.forEach($scope.$parent.$parent.purchaseInvoiceProducts, function (purchaseInvoiceProduct) {
                    sortOrders.push(purchaseInvoiceProduct.order);
                });

                purchaseInvoiceProduct.order = Math.max.apply(null, sortOrders) + 1;

                if (!purchaseInvoiceProduct.order || purchaseInvoiceProduct.order < 1)
                    purchaseInvoiceProduct.order = 1;

                $scope.$parent.$parent.purchaseInvoiceProducts.push(purchaseInvoiceProduct);
            };

            $scope.currencyConvert = function (purchaseInvoiceProduct, value) {

                var currencyConvertList = [];
                switch (purchaseInvoiceProduct.defaultCurrency) {
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

            $scope.setVat = function (purchaseInvoiceProduct) {
                if (purchaseInvoiceProduct.product.vat_percent) {
                    purchaseInvoiceProduct.vat_percent = purchaseInvoiceProduct.product.vat_percent;
                }
            };
            $scope.selectProduct = function (purchaseInvoiceProduct) {
                if (!purchaseInvoiceProduct.product)
                    return;

                if (!purchaseInvoiceProduct.defaultCurrency) {

                    purchaseInvoiceProduct.defaultCurrency = purchaseInvoiceProduct.product.currency ? purchaseInvoiceProduct.product.currency.value : $rootScope.currencySymbol;
                    //purchaseProduct.currencyConvertList = $scope.currencyConvert(purchaseProduct, purchaseProduct.product.unit_price);
                    purchaseInvoiceProduct.currencyConvertList = $scope.currencyConvert(purchaseInvoiceProduct, purchaseInvoiceProduct.product.purchase_price);

                } else {

                    purchaseInvoiceProduct.product.purchase_price = purchaseInvoiceProduct.currencyConvertList[purchaseInvoiceProduct.product.currency.value];
                    purchaseInvoiceProduct.product.purchase_price = purchaseInvoiceProduct.currencyConvertList[purchaseInvoiceProduct.product.currency ? purchaseInvoiceProduct.product.currency.value : $rootScope.currencySymbol];

                }

                var purchasePrice = parseFloat(purchaseInvoiceProduct.product.purchase_price);

                if (!purchaseInvoiceProduct.quantity)
                    purchaseInvoiceProduct.quantity = 1;
                purchaseInvoiceProduct.purchase_price = purchasePrice;
                purchaseInvoiceProduct.amount = purchasePrice;


                if (purchaseInvoiceProduct.product.usage_unit) {
                    if (!angular.isObject(purchaseInvoiceProduct.product.usage_unit)) {
                        $scope.usageUnitList.forEach(function (unit) {
                            if (purchaseInvoiceProduct.product.usage_unit === unit.labelStr) {
                                purchaseInvoiceProduct.product.usage_unit = unit;
                            }
                        });
                    }
                    else {
                        purchaseInvoiceProduct.usage_unit = purchaseInvoiceProduct.product.usage_unit;
                    }
                }

                if (purchaseInvoiceProduct.product.currency) {
                    purchaseInvoiceProduct.currency = purchaseInvoiceProduct.product.currency;
                }
                else {
                    purchaseInvoiceProduct.currency = $scope.defaultCurrency;
                    purchaseInvoiceProduct.product.currency = $scope.defaultCurrency;
                }


                $scope.calculate(purchaseInvoiceProduct);

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

            $scope.calculate = function (purchaseInvoiceProduct) {
                var quantity = 0;
                var purchasePrice = 0;

                if (!isNaN(purchaseInvoiceProduct.quantity))
                    quantity = angular.copy(purchaseInvoiceProduct.quantity);

                if (!isNaN(purchaseInvoiceProduct.purchase_price))
                    purchasePrice = angular.copy(purchaseInvoiceProduct.purchase_price);


                if ($scope.fields['no']) {
                    purchaseInvoiceProduct.no = 0;
                }

                purchaseInvoiceProduct.amount = quantity * purchasePrice;
                switch (purchaseInvoiceProduct.discount_type) {
                    case 'percent':
                        if ($scope.$parent.$parent.record.contact && $scope.$parent.$parent.record.contact.discount && purchaseInvoiceProduct.discount_percent === undefined && purchaseInvoiceProduct.discount_percent !== null) {
                            if (angular.isObject($scope.$parent.$parent.record.contact.discount))
                                purchaseInvoiceProduct.discount_percent = $scope.$parent.$parent.record.contact.discount.value;
                            else
                                purchaseInvoiceProduct.discount_percent = $scope.$parent.$parent.record.contact.discount;
                        }

                        if ($scope.$parent.$parent.record.account && $scope.$parent.$parent.record.account.discount && purchaseInvoiceProduct.discount_percent === undefined && purchaseInvoiceProduct.discount_percent !== null) {
                            if (angular.isObject($scope.$parent.$parent.record.account.discount))
                                purchaseInvoiceProduct.discount_percent = $scope.$parent.$parent.record.account.discount.value;
                            else
                                purchaseInvoiceProduct.discount_percent = $scope.$parent.$parent.record.account.discount;
                        }

                        if (purchaseInvoiceProduct.discount_percent != undefined && purchaseInvoiceProduct.discount_percent != null && !isNaN(purchaseInvoiceProduct.discount_percent)) {
                            if (purchaseInvoiceProduct.discount_percent < 0) {
                                purchaseInvoiceProduct.discount_percent = 0;
                            }

                            if (purchaseInvoiceProduct.discount_percent > 100) {
                                purchaseInvoiceProduct.discount_percent = 100;
                            }

                            purchaseInvoiceProduct.amount -= (purchaseInvoiceProduct.amount * purchaseInvoiceProduct.discount_percent) / 100;


                        }
                        purchaseInvoiceProduct.discount_amount = ((purchasePrice * quantity) * purchaseInvoiceProduct.discount_percent) / 100;
                        if ($scope.fields['unit_amount']) {
                            purchaseInvoiceProduct.unit_amount = purchaseInvoiceProduct.amount / quantity;
                        }
                        break;
                    case 'amount':
                        if (purchaseInvoiceProduct.discount_amount != undefined && purchaseInvoiceProduct.discount_amount != null && !isNaN(purchaseInvoiceProduct.discount_amount)) {
                            if (purchaseInvoiceProduct.discount_amount > purchaseInvoiceProduct.unit_price) {
                                //purchaseInvoiceProduct.discount_amount = purchaseInvoiceProduct.unit_price * purchaseInvoiceProduct.quantity;
                            }

                            purchaseInvoiceProduct.amount -= purchaseInvoiceProduct.discount_amount;

                            purchaseInvoiceProduct.discount_percent = (100 / (purchasePrice * quantity)) * purchaseInvoiceProduct.discount_amount;
                        }
                        if ($scope.fields['unit_amount']) {
                            purchaseInvoiceProduct.unit_amount = purchaseInvoiceProduct.amount / quantity;
                        }
                        break;
                }

                if ($scope.fields['purchase_price']) {
                    if (purchaseInvoiceProduct.product.purchase_price && (purchaseInvoiceProduct.purchase_price === undefined || purchaseInvoiceProduct.purchase_price === null)) {
                       // purchaseInvoiceProduct.purchase_price = $scope.currencyConvert(purchaseInvoiceProduct, purchaseInvoiceProduct.product.purchase_price)[purchaseInvoiceProduct.product.currency.value];
                    } else {
                       // purchaseInvoiceProduct.purchase_price = $scope.currencyConvert(purchaseInvoiceProduct, purchaseInvoiceProduct.product.purchase_price)[purchaseInvoiceProduct.product.currency.value];
                    }
                }

                if (purchaseInvoiceProduct.purchase_price != undefined || purchaseInvoiceProduct.purchase_price != null) {
                    purchaseInvoiceProduct.profit_amount = purchaseInvoiceProduct.amount - purchaseInvoiceProduct.purchase_price * quantity;
                    purchaseInvoiceProduct.profit_percent = ((purchaseInvoiceProduct.amount - (quantity * purchaseInvoiceProduct.purchase_price)) / (quantity * purchaseInvoiceProduct.purchase_price)) * 100;
                }

                var vat = parseFloat(purchaseInvoiceProduct.product.vat_percent || 0);
                purchaseInvoiceProduct.vat = (purchaseInvoiceProduct.amount * vat) / 100;
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
                var counter = 0;
                angular.forEach($scope.$parent.$parent.purchaseInvoiceProducts, function (purchaseInvoiceProduct) {
                    if (!purchaseInvoiceProduct.amount || purchaseInvoiceProduct.deleted)
                        return;

                    counter++;
                    if (purchaseInvoiceProduct.quantity < 0) {
                        purchaseInvoiceProduct.quantity = 0;
                    }

                    if (purchaseInvoiceProduct.purchase_price < 0) {
                        purchaseInvoiceProduct.purchase_price = 0;
                    }

                    if (purchaseInvoiceProduct.purchase_price > 1000000000000) {
                        purchaseInvoiceProduct.purchase_price = 1000000000000;
                    }

                    var amount = angular.copy(purchaseInvoiceProduct.amount);
                    var vat = angular.copy(purchaseInvoiceProduct.vat) || 0;

                    if (purchaseInvoiceProduct.product.currency && $scope.$parent.$parent.record.currency && purchaseInvoiceProduct.product.currency.value && $scope.$parent.$parent.record.currency.value && purchaseInvoiceProduct.product.currency.value != $scope.$parent.$parent.record.currency.value) {
                        switch ($scope.$parent.$parent.record.currency.value) {
                            case '₺':
                                if (purchaseInvoiceProduct.product.currency.value === '$') {
                                    amount = amount * $scope.$parent.$parent.record.exchange_rate_try_usd;
                                    vat = vat * $scope.$parent.$parent.record.exchange_rate_try_usd;
                                }
                                else if (purchaseInvoiceProduct.product.currency.value === '€') {
                                    amount = amount * $scope.$parent.$parent.record.exchange_rate_try_eur;
                                    vat = vat * $scope.$parent.$parent.record.exchange_rate_try_eur;
                                }
                                break;
                            case '$':
                                if (purchaseInvoiceProduct.product.currency.value === '₺') {
                                    amount = amount * $scope.$parent.$parent.record.exchange_rate_usd_try;
                                    vat = vat * $scope.$parent.$parent.record.exchange_rate_usd_try;
                                }
                                else if (purchaseInvoiceProduct.product.currency.value === '€') {
                                    amount = amount * $scope.$parent.$parent.record.exchange_rate_usd_eur;
                                    vat = vat * $scope.$parent.$parent.record.exchange_rate_usd_eur;
                                }
                                break;
                            case '€':
                                if (purchaseInvoiceProduct.product.currency.value === '₺') {
                                    amount = amount * $scope.$parent.$parent.record.exchange_rate_eur_try;
                                    vat = vat * $scope.$parent.$parent.record.exchange_rate_eur_try;
                                }
                                else if (purchaseInvoiceProduct.product.currency.value === '$') {
                                    amount = amount * $scope.$parent.$parent.record.exchange_rate_eur_usd;
                                    vat = vat * $scope.$parent.$parent.record.exchange_rate_eur_usd;
                                }
                                break;
                        }
                    }

                    total += amount;
                    vatTotal += vat;

                    var vatItem = $filter('filter')(vatList, {percent: purchaseInvoiceProduct.product.vat_percent}, true)[0];

                    if (!vatItem) {
                        vatItem = {};
                        vatItem.percent = parseFloat(purchaseInvoiceProduct.product.vat_percent || 0);
                        vatItem.total = vat;

                        if (vat)
                            vatList.push(vatItem);
                    }
                    else {
                        vatItem.total += vat || 0;
                    }
                });
                if ($scope.$parent.$parent.currencyField && $scope.$parent.$parent.record['currency'] && counter < 1) {
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

            $scope.delete = function (purchaseInvoiceProduct) {
                purchaseInvoiceProduct.deleted = true;
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
                var purchaseInvoiceProducts = $filter('filter')($scope.$parent.$parent.purchaseInvoiceProducts, {deleted: false});
                purchaseInvoiceProducts = $filter('orderBy')($scope.$parent.$parent.purchaseInvoiceProducts, 'order');

                var prev = angular.copy(purchaseInvoiceProducts[index - 1]);
                purchaseInvoiceProducts[index].order = prev.order;
                purchaseInvoiceProducts[index - 1].order = angular.copy(order);
            };
            $scope.down = function (index, order) {
                var purchaseInvoiceProducts = $filter('filter')($scope.$parent.$parent.purchaseInvoiceProducts, {deleted: false});
                purchaseInvoiceProducts = $filter('orderBy')($scope.$parent.$parent.purchaseInvoiceProducts, 'order');

                var prev = angular.copy(purchaseInvoiceProducts[index + 1]);
                purchaseInvoiceProducts[index].order = prev.order;
                purchaseInvoiceProducts[index + 1].order = angular.copy(order);
            };
            $scope.productInfo = function (purchaseInvoiceProduct) {
                $scope.currentProduct = purchaseInvoiceProduct;
            }
        }
    ]);