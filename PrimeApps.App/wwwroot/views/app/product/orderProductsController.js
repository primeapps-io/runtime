'use strict';

angular.module('ofisim')
    .controller('OrderProductsController', ['$rootScope', '$scope', '$state', 'config', 'ngToast', '$localStorage', '$filter', 'ngTableParams', '$stateParams', 'OrderProductsService', 'ModuleService',
        function ($rootScope, $scope, $state, config, ngToast, $localStorage, $filter, ngTableParams, $stateParams, OrderProductsService, ModuleService) {
            if ($scope.$parent.$parent.type != 'sales_orders')
                return;

            $scope.isMobile = false;

            if (typeof window.orientation !== 'undefined' || window.innerWidth <= 500) {
                $scope.isMobile = true;
            }

            $scope.orderProductModule = $filter('filter')($rootScope.modules, { name: 'order_products' }, true)[0];
            
            if (!$scope.orderProductModule) {
                ngToast.create({ content: $filter('translate')('Common.NotFound'), className: 'warning' });
                $state.go('app.crm.dashboard');
                return;
            }

            $scope.productField = $filter('filter')($scope.orderProductModule.fields, { name: 'product' }, true)[0];
            $scope.productModule = $filter('filter')($rootScope.modules, { name: 'products' }, true)[0];
            $scope.productField.lookupModulePrimaryField = $filter('filter')($scope.productModule.fields, { name: 'name' }, true)[0];

            ModuleService.getPicklists($scope.orderProductModule)
                .then(function (picklists) {
                    $scope.picklistsModule = picklists;
                });

            $scope.setCurrentLookupProduct = function (product, field) {
                $scope.currentLookupProduct = product;
                $scope.productSelected = true;

                field.special_type = "order_products";
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

            $scope.addOrderProduct = function () {
                var orderProduct = {};
                orderProduct.id = 0;
                orderProduct.discount_type = 'percent';
                var sortOrders = [];

                angular.forEach($scope.$parent.$parent.orderProducts, function (orderProduct) {
                    sortOrders.push(orderProduct.order);
                });

                orderProduct.order = Math.max.apply(null, sortOrders) + 1;

                if (!orderProduct.order || orderProduct.order < 1)
                    orderProduct.order = 1;

                $scope.$parent.$parent.orderProducts.push(orderProduct);
            };

            $scope.selectProduct = function (orderProduct) {
                if (!orderProduct.product)
                    return;

                var unitPrice = parseFloat(orderProduct.product.unit_price);

                if (!orderProduct.quantity)
                    orderProduct.quantity = 1;

                if ($scope.productSelected) {
                    orderProduct.unit_price = unitPrice;
                    orderProduct.amount = unitPrice;

                    if (orderProduct.product.usage_unit) {
                        orderProduct.usage_unit = orderProduct.product.usage_unit.label[$rootScope.language];
                    }
                }

                $scope.calculate(orderProduct);

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

            $scope.calculate = function (orderProduct) {
                var quantity = 0;
                var unitPrice = 0;

                if (!isNaN(orderProduct.quantity))
                    quantity = angular.copy(orderProduct.quantity);

                if (!isNaN(orderProduct.unit_price))
                    unitPrice = angular.copy(orderProduct.unit_price);

                orderProduct.amount = quantity * unitPrice;

                switch (orderProduct.discount_type) {
                    case 'percent':
                        if ($scope.$parent.$parent.record.contact && $scope.$parent.$parent.record.contact.discount && orderProduct.discount_percent === undefined && orderProduct.discount_percent !== null) {
                            if (angular.isObject($scope.$parent.$parent.record.contact.discount))
                                orderProduct.discount_percent = $scope.$parent.$parent.record.contact.discount.value;
                            else
                                orderProduct.discount_percent = $scope.$parent.$parent.record.contact.discount;
                        }

                        if ($scope.$parent.$parent.record.account && $scope.$parent.$parent.record.account.discount && orderProduct.discount_percent === undefined && orderProduct.discount_percent !== null) {
                            if (angular.isObject($scope.$parent.$parent.record.account.discount))
                                orderProduct.discount_percent = $scope.$parent.$parent.record.account.discount.value;
                            else
                                orderProduct.discount_percent = $scope.$parent.$parent.record.account.discount;
                        }

                        if (orderProduct.discount_percent != undefined && orderProduct.discount_percent != null && !isNaN(orderProduct.discount_percent)) {
                            if (orderProduct.discount_percent < 0) {
                                orderProduct.discount_percent = 0;
                            }

                            if (orderProduct.discount_percent > 100) {
                                orderProduct.discount_percent = 100;
                            }

                            orderProduct.amount -= (orderProduct.amount * orderProduct.discount_percent) / 100;
                        }
                        orderProduct.discount_amount = null;
                        break;
                    case 'amount':
                        if (orderProduct.discount_amount != undefined && orderProduct.discount_amount != null && !isNaN(orderProduct.discount_amount)) {
                            if (orderProduct.discount_amount > orderProduct.unit_price) {
                                orderProduct.discount_amount = orderProduct.unit_price;
                            }

                            orderProduct.amount -= orderProduct.discount_amount;
                        }

                        orderProduct.discount_percent = null;
                        break;
                }

                var vat = parseFloat(orderProduct.product.vat_percent || 0);
                orderProduct.vat = (orderProduct.amount * vat) / 100;
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
                angular.forEach($scope.$parent.$parent.orderProducts, function (orderProduct) {
                    if (!orderProduct.amount || orderProduct.deleted)
                        return;

                    if (orderProduct.quantity < 0) {
                        orderProduct.quantity = 0;
                    }

                    if (orderProduct.unit_price < 0) {
                        orderProduct.unit_price = 0;
                    }

                    if (orderProduct.unit_price > 1000000000000) {
                        orderProduct.unit_price = 1000000000000;
                    }

                    var amount = angular.copy(orderProduct.amount);
                    var vat = angular.copy(orderProduct.vat) || 0;
                    if (orderProduct.product.currency && $scope.$parent.$parent.record.currency && orderProduct.product.currency.value && $scope.$parent.$parent.record.currency.value && orderProduct.product.currency.value != $scope.$parent.$parent.record.currency.value) {
                        switch ($scope.$parent.$parent.record.currency.value) {
                            case '₺':
                                if (orderProduct.product.currency.value === '$') {
                                    amount = amount * $scope.$parent.$parent.record.exchange_rate_try_usd;
                                    vat = vat * $scope.$parent.$parent.record.exchange_rate_try_usd;
                                }
                                else if (orderProduct.product.currency.value === '€') {
                                    amount = amount * $scope.$parent.$parent.record.exchange_rate_try_eur;
                                    vat = vat * $scope.$parent.$parent.record.exchange_rate_try_eur;
                                }
                                break;
                            case '$':
                                if (orderProduct.product.currency.value === '₺') {
                                    amount = amount * $scope.$parent.$parent.record.exchange_rate_usd_try;
                                    vat = vat * $scope.$parent.$parent.record.exchange_rate_usd_try;
                                }
                                else if (orderProduct.product.currency.value === '€') {
                                    amount = amount * $scope.$parent.$parent.record.exchange_rate_usd_eur;
                                    vat = vat * $scope.$parent.$parent.record.exchange_rate_usd_eur;
                                }
                                break;
                            case '€':
                                if (orderProduct.product.currency.value === '₺') {
                                    amount = amount * $scope.$parent.$parent.record.exchange_rate_eur_try;
                                    vat = vat * $scope.$parent.$parent.record.exchange_rate_eur_try;
                                }
                                else if (orderProduct.product.currency.value === '$') {
                                    amount = amount * $scope.$parent.$parent.record.exchange_rate_eur_usd;
                                    vat = vat * $scope.$parent.$parent.record.exchange_rate_eur_usd;
                                }
                                break;
                        }
                    }

                    total += amount;
                    vatTotal += vat;

                    var vatItem = $filter('filter')(vatList, {percent: orderProduct.product.vat_percent}, true)[0];

                    if (!vatItem) {
                        vatItem = {};
                        vatItem.percent = parseFloat(orderProduct.product.vat_percent || 0);
                        vatItem.total = vat;

                        if (vat)
                            vatList.push(vatItem);
                    }
                    else {
                        vatItem.total += vat || 0;
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

            $scope.delete = function (orderProduct) {
                orderProduct.deleted = true;
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