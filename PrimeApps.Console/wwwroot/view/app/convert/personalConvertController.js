'use strict';

angular.module('primeapps')

    .controller('PersonalConvertController', ['$rootScope', '$scope', '$location', '$state', '$filter', '$q', 'ngToast', 'helper', 'PersonalConvertService', 'ModuleService', '$cache',
        function ($rootScope, $scope, $location, $state, $filter, $q, ngToast, helper, PersonalConvertService, ModuleService, $cache) {
            $scope.id = $location.search().id;
            $scope.module = $filter('filter')($rootScope.modules, { name: 'adaylar' }, true)[0];
            $scope.currentDayMin = helper.getCurrentDateMin().toISOString();
            $scope.currentDayMax = helper.getCurrentDateMax().toISOString();
            $scope.deleted = false;

            if (!$scope.module) {
                ngToast.create({ content: $filter('translate')('Common.NotFound'), className: 'warning' });
                $state.go('app.dashboard');
                return;
            }

            if (!$scope.id) {
                ngToast.create({ content: $filter('translate')('Common.NotFound'), className: 'warning' });
                $state.go('app.dashboard');
                return;
            }

            $scope.accountModule = $filter('filter')($rootScope.modules, { name: 'calisanlar' }, true)[0];
            $scope.leadModulePrimaryField = $filter('filter')($scope.module.fields, { primary: true }, true)[0];
            $scope.loading = true;

            ModuleService.getPicklists($scope.module)
                .then(function (picklists) {
                    $scope.picklistsModule = picklists;

                    ModuleService.getRecord($scope.module.name, $scope.id)
                        .then(function (recordData) {
                            $scope.lead = ModuleService.processRecordSingle(recordData.data, $scope.module, $scope.picklistsModule);
                            ModuleService.formatRecordFieldValues(angular.copy(recordData.data), $scope.module, $scope.picklistsModule);
                        })
                        .finally(function () {
                            $scope.loading = false;
                        });
                });

            $scope.convert = function () {

                $scope.loading = true;

                var convertRequest = {};
                convertRequest.lead_id = $scope.lead.id;
                convertRequest.deleted = $scope.deleted;

                PersonalConvertService.convert(convertRequest)
                    .then(function (converted) {
                        $scope.converted = converted.data;
                        $scope.convertDisable = true;
                        $cache.remove('adaylar_adaylar');
                        $cache.remove('calisanlar_calisanlar');
                        $cache.remove('activities_activities');

                        ngToast.create({ content: $filter('translate')('Convert.Success', { type: $scope.module['label_' + language + '_singular'] }), className: 'success' });
                    })
                    .catch(function (data) {
                        if (data.status === 409) {
                            $scope.moduleForm[data.data.field].$setValidity('unique', false);
                        }
                    })
                    .finally(function () {
                        $scope.loading = false;
                    });

            };
        }
    ]);