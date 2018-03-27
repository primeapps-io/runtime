'use strict';

angular.module('ofisim')

    .controller('QuoteConvertController', ['$rootScope', '$scope', '$location', '$state', '$filter', '$q', '$window', 'ngToast', 'QuoteConvertService', 'ModuleService', '$cache',
        function ($rootScope, $scope, $location, $state, $filter, $q, $window, ngToast, QuoteConvertService, ModuleService, $cache) {
            $scope.id = $location.search().id;

            $scope.module = $filter('filter')($rootScope.modules, { name: 'quotes' }, true)[0];

            if (!$scope.module) {
                ngToast.create({ content: $filter('translate')('Common.NotFound'), className: 'warning' });
                $state.go('app.crm.dashboard');
                return;
            }

            if (!$scope.id) {
                ngToast.create({ content: $filter('translate')('Common.NotFound'), className: 'warning' });
                $state.go('app.crm.dashboard');
                return;
            }

            var orderModule = $filter('filter')($rootScope.modules, { name: 'sales_orders' }, true)[0];
            $scope.loading = true;

            ModuleService.getPicklists(orderModule)
                .then(function (picklists) {
                    var stageField = $filter('filter')(orderModule.fields, { name: 'order_stage' }, true)[0];
                    $scope.stageList = picklists[stageField.picklist_id];
                });

            ModuleService.getPicklists($scope.module)
                .then(function (picklists) {
                    $scope.picklistsModule = picklists;

                    ModuleService.getRecord($scope.module.name, $scope.id)
                        .then(function (recordData) {
                            $scope.quote = ModuleService.processRecordSingle(recordData.data, $scope.module, $scope.picklistsModule);
                            ModuleService.formatRecordFieldValues(angular.copy(recordData.data), $scope.module, $scope.picklistsModule);
                        })
                        .finally(function () {
                            $scope.loading = false;
                        });
                });


            //Quote Convert Text
            $scope.salesOrderModule = $filter('filter')($rootScope.modules, { name: 'sales_orders' }, true)[0];
            $scope.stageField = $filter('filter')($scope.salesOrderModule.fields, { name: 'order_stage' }, true)[0];


            $scope.convert = function () {
                $scope.converting = true;
                var convertRequest = {};
                convertRequest.quote_id = $scope.quote.id;

                if ($scope.order_stage) {
                    convertRequest.order_stage = $scope.order_stage;
                }

                QuoteConvertService.convert(convertRequest)
                    .then(function (converted) {
                        $scope.convertDisable = true;
                        $cache.remove('quotes_quotes');
                        $cache.remove('sales_orders_sales_orders');

                        ngToast.create({ content: $filter('translate')('Convert.Quote.Success'), className: 'success' });
                        $window.location.href = '#/app/crm/module/sales_orders?id=' + converted.data.sales_order_id + '&back=quotes';
                    })
                    .catch(function () {
                        $scope.converting = false;
                    });
            };
        }
    ]);