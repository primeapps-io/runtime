'use strict';

angular.module('ofisim')
    .controller('QuoteProductsController', ['$rootScope', '$scope', '$state', 'config', 'ngToast', '$localStorage', '$filter', 'ngTableParams', '$stateParams', 'helper', 'QuoteProductsService', 'ModuleService', '$popover',
        function ($rootScope, $scope, $state, config, ngToast, $localStorage, $filter, ngTableParams, $stateParams, helper, QuoteProductsService, ModuleService, $popover) {
            if ($scope.$parent.$parent.type != 'quotes')
                return;

            $scope.isMobile = false;

            if (typeof window.orientation !== 'undefined' || window.innerWidth <= 500) {
                $scope.isMobile = true;
            }

            $scope.quoteProductModule = $filter('filter')($rootScope.modules, { name: 'quote_products' }, true)[0];



            if (!$scope.quoteProductModule) {
                ngToast.create({ content: $filter('translate')('Common.NotFound'), className: 'warning' });
                $state.go('app.crm.dashboard');
                return;
            }
            $scope.fields = [];

            $scope.quoteProductModule.fields.forEach(function (field) {
                $scope.fields[field.name] = field;
            });

            $scope.quoteFields = [];
            $scope.quoteModule = $filter('filter')($rootScope.modules, { name: 'quotes' }, true)[0];
            $scope.quoteModule.fields.forEach(function (field) {
                $scope.quoteFields[field.name] = field;
            });

            $scope.productField = $filter('filter')($scope.quoteProductModule.fields, { name: 'product' }, true)[0];
            $scope.productModule = $filter('filter')($rootScope.modules, { name: 'products' }, true)[0];
            $scope.productField.lookupModulePrimaryField = $filter('filter')($scope.productModule.fields, { name: 'name' }, true)[0];

            $scope.productFields = [];
            angular.forEach($scope.productModule.fields, function (productField) {
                $scope.productFields[productField.name] = productField;
            });

            ModuleService.getPicklists($scope.quoteProductModule)
                .then(function (picklists) {
                    $scope.picklistsModule = picklists;
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

            $scope.addQuoteProduct = function (separator) {

                var quoteProduct = {};
                quoteProduct.id = 0;
                quoteProduct.discount_type = 'percent';
                if (separator) {
                    quoteProduct.separator = "";
                    quoteProduct.product = {};
                }

                var sortOrders = [];

                angular.forEach($scope.$parent.$parent.quoteProducts, function (quoteProduct) {
                    sortOrders.push(quoteProduct.order);
                });

                quoteProduct.order = Math.max.apply(null, sortOrders) + 1;

                if (!quoteProduct.order || quoteProduct.order < 1)
                    quoteProduct.order = 1;

                $scope.$parent.$parent.quoteProducts.push(quoteProduct);
            };

            $scope.currencyConvert = function (quoteProduct, value) {
                var currencyConvertList = [];

                switch (quoteProduct.defaultCurrency) {
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

            $scope.setVat = function (quoteProduct) {
                if (quoteProduct.product.vat_percent) {
                    quoteProduct.vat_percent = quoteProduct.product.vat_percent;
                }
            };

            $scope.selectProduct = function (quoteProduct) {
                if (!quoteProduct.product)
                    return;

                if (!quoteProduct.defaultCurrency) {
                    quoteProduct.defaultCurrency = quoteProduct.product.currency ? quoteProduct.product.currency.value : $rootScope.currencySymbol;
                    quoteProduct.currencyConvertList = $scope.currencyConvert(quoteProduct, quoteProduct.product.unit_price);
                } else {
                    quoteProduct.product.unit_price = quoteProduct.currencyConvertList[quoteProduct.product.currency ? quoteProduct.product.currency.value : $rootScope.currencySymbol];
                }

                var unitPrice = parseFloat(quoteProduct.product.unit_price);

                if (!quoteProduct.quantity)
                    quoteProduct.quantity = 1;

                quoteProduct.unit_price = unitPrice;
                quoteProduct.amount = unitPrice;

                if (quoteProduct.product.usage_unit) {
                    if (!angular.isObject(quoteProduct.product.usage_unit)) {

                        $scope.usageUnitList.forEach(function (unit) {

                            if (quoteProduct.product.usage_unit === unit.labelStr) {
                                quoteProduct.product.usage_unit = unit;
                            }

                        });

                    } else {
                        quoteProduct.usage_unit = quoteProduct.product.usage_unit;
                    }

                }

                if (quoteProduct.product.currency) {
                    quoteProduct.currency = quoteProduct.product.currency;
                }
                else {

                    quoteProduct.currency = $scope.defaultCurrency;
                    quoteProduct.product.currency = $scope.defaultCurrency;
                }

                $scope.calculate(quoteProduct);
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

            $scope.calculate = function (quoteProduct) {
                var quantity = 0;
                var unitPrice = 0;

                if (!isNaN(quoteProduct.quantity))
                    quantity = angular.copy(quoteProduct.quantity);

                if (!isNaN(quoteProduct.unit_price))
                    unitPrice = angular.copy(quoteProduct.unit_price);


                if ($scope.fields['no']) {
                    quoteProduct.no = 0;
                }

                quoteProduct.amount = quantity * unitPrice;
                switch (quoteProduct.discount_type) {
                    case 'percent':
                        if ($scope.$parent.$parent.record.contact && $scope.$parent.$parent.record.contact.discount && quoteProduct.discount_percent === undefined && quoteProduct.discount_percent !== null) {
                            if (angular.isObject($scope.$parent.$parent.record.contact.discount))
                                quoteProduct.discount_percent = $scope.$parent.$parent.record.contact.discount.value;
                            else
                                quoteProduct.discount_percent = $scope.$parent.$parent.record.contact.discount;
                        }

                        if ($scope.$parent.$parent.record.account && $scope.$parent.$parent.record.account.discount && quoteProduct.discount_percent === undefined && quoteProduct.discount_percent !== null) {
                            if (angular.isObject($scope.$parent.$parent.record.account.discount))
                                quoteProduct.discount_percent = $scope.$parent.$parent.record.account.discount.value;
                            else
                                quoteProduct.discount_percent = $scope.$parent.$parent.record.account.discount;
                        }

                        if (quoteProduct.discount_percent != undefined && quoteProduct.discount_percent != null && !isNaN(quoteProduct.discount_percent)) {
                            if (quoteProduct.discount_percent < 0) {
                                quoteProduct.discount_percent = 0;
                            }

                            if (quoteProduct.discount_percent > 100) {
                                quoteProduct.discount_percent = 100;
                            }

                            quoteProduct.amount -= (quoteProduct.amount * quoteProduct.discount_percent) / 100;


                        }
                        quoteProduct.discount_amount = ((unitPrice * quantity) * quoteProduct.discount_percent) / 100;
                        if ($scope.fields['unit_amount']) {
                            quoteProduct.unit_amount = quoteProduct.amount / quantity;
                        }
                        break;
                    case 'amount':
                        if (quoteProduct.discount_amount != undefined && quoteProduct.discount_amount != null && !isNaN(quoteProduct.discount_amount)) {
                            if (quoteProduct.discount_amount > quoteProduct.unit_price) {
                                //quoteProduct.discount_amount = quoteProduct.unit_price * quoteProduct.quantity;
                            }

                            quoteProduct.amount -= quoteProduct.discount_amount;

                            quoteProduct.discount_percent = (100 / (unitPrice * quantity)) * quoteProduct.discount_amount;
                        }
                        if ($scope.fields['unit_amount']) {
                            quoteProduct.unit_amount = quoteProduct.amount / quantity;
                        }
                        break;
                }
                if ($scope.fields['purchase_price']) {
                    if (quoteProduct.product.purchase_price && (quoteProduct.purchase_price === undefined || quoteProduct.purchase_price === null)) {
                        quoteProduct.purchase_price = $scope.currencyConvert(quoteProduct, quoteProduct.product.purchase_price)[quoteProduct.product.currency.value];
                    } else {
                        quoteProduct.purchase_price = $scope.currencyConvert(quoteProduct, quoteProduct.product.purchase_price)[quoteProduct.product.currency ? quoteProduct.product.currency.value : $rootScope.currencySymbol];
                    }
                }
                if (quoteProduct.purchase_price != undefined || quoteProduct.purchase_price != null) {
                    quoteProduct.profit_amount = quoteProduct.amount - quoteProduct.purchase_price * quantity;
                    quoteProduct.profit_percent = ((quoteProduct.amount - (quantity * quoteProduct.purchase_price)) / (quantity * quoteProduct.purchase_price)) * 100;
                }

                var vat = parseFloat(quoteProduct.product.vat_percent || 0);
                quoteProduct.vat = (quoteProduct.amount * vat) / 100;
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
                angular.forEach($scope.$parent.$parent.quoteProducts, function (quoteProduct) {
                    if (!quoteProduct.amount || quoteProduct.deleted)
                        return;

                    counter++;
                    if (quoteProduct.quantity < 0) {
                        quoteProduct.quantity = 0;
                    }

                    if (quoteProduct.unit_price < 0) {
                        quoteProduct.unit_price = 0;
                    }

                    if (quoteProduct.unit_price > 1000000000000) {
                        quoteProduct.unit_price = 1000000000000;
                    }

                    var amount = angular.copy(quoteProduct.amount);
                    var vat = angular.copy(quoteProduct.vat) || 0;

                    if (quoteProduct.product.currency && $scope.$parent.$parent.record.currency && quoteProduct.product.currency.value && $scope.$parent.$parent.record.currency.value && quoteProduct.product.currency.value != $scope.$parent.$parent.record.currency.value) {
                        switch ($scope.$parent.$parent.record.currency.value) {
                            case '₺':
                                if (quoteProduct.product.currency.value === '$') {
                                    amount = amount * $scope.$parent.$parent.record.exchange_rate_try_usd;
                                    vat = vat * $scope.$parent.$parent.record.exchange_rate_try_usd;
                                }
                                else if (quoteProduct.product.currency.value === '€') {
                                    amount = amount * $scope.$parent.$parent.record.exchange_rate_try_eur;
                                    vat = vat * $scope.$parent.$parent.record.exchange_rate_try_eur;
                                }
                                break;
                            case '$':
                                if (quoteProduct.product.currency.value === '₺') {
                                    amount = amount * $scope.$parent.$parent.record.exchange_rate_usd_try;
                                    vat = vat * $scope.$parent.$parent.record.exchange_rate_usd_try;
                                }
                                else if (quoteProduct.product.currency.value === '€') {
                                    amount = amount * $scope.$parent.$parent.record.exchange_rate_usd_eur;
                                    vat = vat * $scope.$parent.$parent.record.exchange_rate_usd_eur;
                                }
                                break;
                            case '€':
                                if (quoteProduct.product.currency.value === '₺') {
                                    amount = amount * $scope.$parent.$parent.record.exchange_rate_eur_try;
                                    vat = vat * $scope.$parent.$parent.record.exchange_rate_eur_try;
                                }
                                else if (quoteProduct.product.currency.value === '$') {
                                    amount = amount * $scope.$parent.$parent.record.exchange_rate_eur_usd;
                                    vat = vat * $scope.$parent.$parent.record.exchange_rate_eur_usd;
                                }
                                break;
                        }
                    }

                    total += amount;
                    vatTotal += vat;

                    var vatItem = $filter('filter')(vatList, { percent: quoteProduct.product.vat_percent }, true)[0];

                    if (!vatItem) {
                        vatItem = {};
                        vatItem.percent = parseFloat(quoteProduct.product.vat_percent || 0);
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

            $scope.delete = function (quoteProduct) {
                quoteProduct.deleted = true;
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
                var quoteProducts = $filter('filter')($scope.$parent.$parent.quoteProducts, { deleted: false });
                quoteProducts = $filter('orderBy')($scope.$parent.$parent.quoteProducts, 'order');

                var prev = angular.copy(quoteProducts[index - 1]);
                quoteProducts[index].order = prev.order;
                quoteProducts[index - 1].order = angular.copy(order);
            };
            $scope.down = function (index, order) {
                var quoteProducts = $filter('filter')($scope.$parent.$parent.quoteProducts, { deleted: false });
                quoteProducts = $filter('orderBy')($scope.$parent.$parent.quoteProducts, 'order');

                var prev = angular.copy(quoteProducts[index + 1]);
                quoteProducts[index].order = prev.order;
                quoteProducts[index + 1].order = angular.copy(order);
            };
            $scope.productInfo = function (quoteProduct) {
                $scope.currentProduct = quoteProduct;
            }
        }
    ]);