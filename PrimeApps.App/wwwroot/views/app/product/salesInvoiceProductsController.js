'use strict';

angular.module('ofisim')
    .controller('SalesInvoiceProductsController', ['$rootScope', '$scope', '$state', 'config', 'ngToast', '$localStorage', '$filter', 'ngTableParams', '$stateParams', 'helper', 'QuoteProductsService', 'ModuleService', '$popover',
        function ($rootScope, $scope, $state, config, ngToast, $localStorage, $filter, ngTableParams, $stateParams, helper, QuoteProductsService, ModuleService, $popover) {
            if ($scope.$parent.$parent.type != 'sales_invoices')
                return;

            $scope.isMobile = false;

            if (typeof window.orientation !== 'undefined' || window.innerWidth <= 500) {
                $scope.isMobile = true;
            }

            $scope.salesInvoiceProductModule = $filter('filter')($rootScope.modules, { name: 'sales_invoices_products' }, true)[0];

            if (!$scope.salesInvoiceProductModule) {
                ngToast.create({ content: $filter('translate')('Common.NotFound'), className: 'warning' });
                $state.go('app.crm.dashboard');
                return;
            }
            $scope.fields = [];

            $scope.salesInvoiceProductModule.fields.forEach(function (field) {
                $scope.fields[field.name] = field;
            });

            $scope.productField = $filter('filter')($scope.salesInvoiceProductModule.fields, { name: 'product' }, true)[0];
            $scope.productModule = $filter('filter')($rootScope.modules, { name: 'products' }, true)[0];
            $scope.productField.lookupModulePrimaryField = $filter('filter')($scope.productModule.fields, { name: 'name' }, true)[0];

            $scope.productFields = [];
            angular.forEach($scope.productModule.fields, function (productField) {
                $scope.productFields[productField.name] = productField;
            });

            ModuleService.getPicklists($scope.salesInvoiceProductModule)
                .then(function (picklists) {
                    $scope.picklistsModule = picklists;
                    $scope.usageUnitList = $scope.picklistsModule[$scope.fields['usage_unit'].picklist_id];
                    $scope.currencyList = $scope.picklistsModule[$scope.fields['currency'].picklist_id];
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
            var isExtraField = true;
            var additionalFields = ['unit_price', 'usage_unit', 'vat_percent'];
            var extraadditionalFields = ['purchase_price'];
            $scope.lookup = function (searchTerm) {
                if (isExtraField) {
                    for (var i = 0; extraadditionalFields.length > i; i++) {
                        var field = $filter('filter')($scope.salesInvoiceProductModule.fields, { name: extraadditionalFields[i] }, true);
                        if (field.length > 0) {
                            additionalFields.push(extraadditionalFields[i])
                        }
                    }
                    if ($scope.$parent.$parent.currencyField)
                        additionalFields.push('currency');
                    isExtraField = false;
                }
                return ModuleService.lookup(searchTerm, $scope.productField, $scope.currentLookupProduct, additionalFields);
            };

            $scope.addSalesInvoiceProduct = function (separator) {

                var salesInvoiceProduct = {};
                salesInvoiceProduct.id = 0;
                salesInvoiceProduct.discount_type = 'percent';
                if (separator) {
                    salesInvoiceProduct.separator = "";
                    salesInvoiceProduct.product = {};
                }

                var sortOrders = [];

                angular.forEach($scope.$parent.$parent.salesInvoiceProducts, function (salesInvoiceProduct) {
                    sortOrders.push(salesInvoiceProduct.order);
                });

                salesInvoiceProduct.order = Math.max.apply(null, sortOrders) + 1;

                if (!salesInvoiceProduct.order || salesInvoiceProduct.order < 1)
                    salesInvoiceProduct.order = 1;

                $scope.$parent.$parent.salesInvoiceProducts.push(salesInvoiceProduct);
            };

            $scope.currencyConvert = function (salesInvoiceProduct, value) {

                var currencyConvertList = [];
                switch (salesInvoiceProduct.defaultCurrency) {
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

            $scope.setVat = function (salesInvoiceProduct) {
                if (salesInvoiceProduct.product.vat_percent) {
                    salesInvoiceProduct.vat_percent = salesInvoiceProduct.product.vat_percent;
                }
            };
            $scope.selectProduct = function (salesInvoiceProduct) {
                if (!salesInvoiceProduct.product)
                    return;

                if (!salesInvoiceProduct.defaultCurrency) {
                    salesInvoiceProduct.defaultCurrency = salesInvoiceProduct.product.currency.value;
                    salesInvoiceProduct.currencyConvertList = $scope.currencyConvert(salesInvoiceProduct, salesInvoiceProduct.product.unit_price);
                } else {
                    salesInvoiceProduct.product.unit_price = salesInvoiceProduct.currencyConvertList[salesInvoiceProduct.product.currency.value];
                }

                var unitPrice = parseFloat(salesInvoiceProduct.product.unit_price);

                if (!salesInvoiceProduct.quantity)
                    salesInvoiceProduct.quantity = 1;
                salesInvoiceProduct.unit_price = unitPrice;
                salesInvoiceProduct.amount = unitPrice;

                if (salesInvoiceProduct.product.usage_unit) {
                    salesInvoiceProduct.usage_unit = salesInvoiceProduct.product.usage_unit;
                }

                if (salesInvoiceProduct.product.currency) {
                    salesInvoiceProduct.currency = salesInvoiceProduct.product.currency;
                }

                $scope.calculate(salesInvoiceProduct);

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

            $scope.calculate = function (salesInvoiceProduct) {
                var quantity = 0;
                var unitPrice = 0;

                if (!isNaN(salesInvoiceProduct.quantity))
                    quantity = angular.copy(salesInvoiceProduct.quantity);

                if (!isNaN(salesInvoiceProduct.unit_price))
                    unitPrice = angular.copy(salesInvoiceProduct.unit_price);


                if ($scope.fields['no']) {
                    salesInvoiceProduct.no = 0;
                }

                salesInvoiceProduct.amount = quantity * unitPrice;
                switch (salesInvoiceProduct.discount_type) {
                    case 'percent':
                        if ($scope.$parent.$parent.record.contact && $scope.$parent.$parent.record.contact.discount && salesInvoiceProduct.discount_percent === undefined && salesInvoiceProduct.discount_percent !== null) {
                            if (angular.isObject($scope.$parent.$parent.record.contact.discount))
                                salesInvoiceProduct.discount_percent = $scope.$parent.$parent.record.contact.discount.value;
                            else
                                salesInvoiceProduct.discount_percent = $scope.$parent.$parent.record.contact.discount;
                        }

                        if ($scope.$parent.$parent.record.account && $scope.$parent.$parent.record.account.discount && salesInvoiceProduct.discount_percent === undefined && salesInvoiceProduct.discount_percent !== null) {
                            if (angular.isObject($scope.$parent.$parent.record.account.discount))
                                salesInvoiceProduct.discount_percent = $scope.$parent.$parent.record.account.discount.value;
                            else
                                salesInvoiceProduct.discount_percent = $scope.$parent.$parent.record.account.discount;
                        }

                        if (salesInvoiceProduct.discount_percent != undefined && salesInvoiceProduct.discount_percent != null && !isNaN(salesInvoiceProduct.discount_percent)) {
                            if (salesInvoiceProduct.discount_percent < 0) {
                                salesInvoiceProduct.discount_percent = 0;
                            }

                            if (salesInvoiceProduct.discount_percent > 100) {
                                salesInvoiceProduct.discount_percent = 100;
                            }

                            salesInvoiceProduct.amount -= (salesInvoiceProduct.amount * salesInvoiceProduct.discount_percent) / 100;


                        }
                        salesInvoiceProduct.discount_amount = ((unitPrice * quantity) * salesInvoiceProduct.discount_percent) / 100;
                        if ($scope.fields['unit_amount']) {
                            salesInvoiceProduct.unit_amount = salesInvoiceProduct.amount / quantity;
                        }
                        break;
                    case 'amount':
                        if (salesInvoiceProduct.discount_amount != undefined && salesInvoiceProduct.discount_amount != null && !isNaN(salesInvoiceProduct.discount_amount)) {
                            if (salesInvoiceProduct.discount_amount > salesInvoiceProduct.unit_price) {
                                //salesInvoiceProduct.discount_amount = salesInvoiceProduct.unit_price * salesInvoiceProduct.quantity;
                            }

                            salesInvoiceProduct.amount -= salesInvoiceProduct.discount_amount;

                            salesInvoiceProduct.discount_percent = (100 / (unitPrice * quantity)) * salesInvoiceProduct.discount_amount;
                        }
                        if ($scope.fields['unit_amount']) {
                            salesInvoiceProduct.unit_amount = salesInvoiceProduct.amount / quantity;
                        }
                        break;
                }
                if (salesInvoiceProduct.product.purchase_price && (salesInvoiceProduct.purchase_price === undefined || salesInvoiceProduct.purchase_price === null)) {
                    salesInvoiceProduct.purchase_price = $scope.currencyConvert(salesInvoiceProduct, salesInvoiceProduct.product.purchase_price)[salesInvoiceProduct.product.currency.value];
                } else {
                    salesInvoiceProduct.purchase_price = $scope.currencyConvert(salesInvoiceProduct, salesInvoiceProduct.product.purchase_price)[salesInvoiceProduct.product.currency.value];
                }
                if (salesInvoiceProduct.purchase_price != undefined || salesInvoiceProduct.purchase_price != null) {
                    salesInvoiceProduct.profit_amount = salesInvoiceProduct.amount - salesInvoiceProduct.purchase_price * quantity;
                    salesInvoiceProduct.profit_percent = ((salesInvoiceProduct.amount - (quantity * salesInvoiceProduct.purchase_price)) / (quantity * salesInvoiceProduct.purchase_price)) * 100;
                }

                var vat = parseFloat(salesInvoiceProduct.product.vat_percent || 0);
                salesInvoiceProduct.vat = (salesInvoiceProduct.amount * vat) / 100;
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
                angular.forEach($scope.$parent.$parent.salesInvoiceProducts, function (salesInvoiceProduct) {
                    if (!salesInvoiceProduct.amount || salesInvoiceProduct.deleted)
                        return;

                    counter++;
                    if (salesInvoiceProduct.quantity < 0) {
                        salesInvoiceProduct.quantity = 0;
                    }

                    if (salesInvoiceProduct.unit_price < 0) {
                        salesInvoiceProduct.unit_price = 0;
                    }

                    if (salesInvoiceProduct.unit_price > 1000000000000) {
                        salesInvoiceProduct.unit_price = 1000000000000;
                    }

                    var amount = angular.copy(salesInvoiceProduct.amount);
                    var vat = angular.copy(salesInvoiceProduct.vat) || 0;

                    if (salesInvoiceProduct.product.currency && $scope.$parent.$parent.record.currency && salesInvoiceProduct.product.currency.value && $scope.$parent.$parent.record.currency.value && salesInvoiceProduct.product.currency.value != $scope.$parent.$parent.record.currency.value) {
                        switch ($scope.$parent.$parent.record.currency.value) {
                            case '₺':
                                if (salesInvoiceProduct.product.currency.value === '$') {
                                    amount = amount * $scope.$parent.$parent.record.exchange_rate_try_usd;
                                    vat = vat * $scope.$parent.$parent.record.exchange_rate_try_usd;
                                }
                                else if (salesInvoiceProduct.product.currency.value === '€') {
                                    amount = amount * $scope.$parent.$parent.record.exchange_rate_try_eur;
                                    vat = vat * $scope.$parent.$parent.record.exchange_rate_try_eur;
                                }
                                break;
                            case '$':
                                if (salesInvoiceProduct.product.currency.value === '₺') {
                                    amount = amount * $scope.$parent.$parent.record.exchange_rate_usd_try;
                                    vat = vat * $scope.$parent.$parent.record.exchange_rate_usd_try;
                                }
                                else if (salesInvoiceProduct.product.currency.value === '€') {
                                    amount = amount * $scope.$parent.$parent.record.exchange_rate_usd_eur;
                                    vat = vat * $scope.$parent.$parent.record.exchange_rate_usd_eur;
                                }
                                break;
                            case '€':
                                if (salesInvoiceProduct.product.currency.value === '₺') {
                                    amount = amount * $scope.$parent.$parent.record.exchange_rate_eur_try;
                                    vat = vat * $scope.$parent.$parent.record.exchange_rate_eur_try;
                                }
                                else if (salesInvoiceProduct.product.currency.value === '$') {
                                    amount = amount * $scope.$parent.$parent.record.exchange_rate_eur_usd;
                                    vat = vat * $scope.$parent.$parent.record.exchange_rate_eur_usd;
                                }
                                break;
                        }
                    }

                    total += amount;
                    vatTotal += vat;

                    var vatItem = $filter('filter')(vatList, { percent: salesInvoiceProduct.product.vat_percent }, true)[0];

                    if (!vatItem) {
                        vatItem = {};
                        vatItem.percent = parseFloat(salesInvoiceProduct.product.vat_percent || 0);
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

            $scope.delete = function (salesInvoiceProduct) {
                salesInvoiceProduct.deleted = true;
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
                var salesInvoiceProducts = $filter('filter')($scope.$parent.$parent.salesInvoiceProducts, { deleted: false });
                salesInvoiceProducts = $filter('orderBy')($scope.$parent.$parent.salesInvoiceProducts, 'order');

                var prev = angular.copy(salesInvoiceProducts[index - 1]);
                salesInvoiceProducts[index].order = prev.order;
                salesInvoiceProducts[index - 1].order = angular.copy(order);
            };
            $scope.down = function (index, order) {
                var salesInvoiceProducts = $filter('filter')($scope.$parent.$parent.salesInvoiceProducts, { deleted: false });
                salesInvoiceProducts = $filter('orderBy')($scope.$parent.$parent.salesInvoiceProducts, 'order');

                var prev = angular.copy(salesInvoiceProducts[index + 1]);
                salesInvoiceProducts[index].order = prev.order;
                salesInvoiceProducts[index + 1].order = angular.copy(order);
            };
            $scope.productInfo = function (salesInvoiceProduct) {
                $scope.currentProduct = salesInvoiceProduct;
            }
        }
    ]);