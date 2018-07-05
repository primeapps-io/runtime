'use strict';

angular.module('primeapps')

    .controller('LeadConvertController', ['$rootScope', '$scope', '$location', '$state', '$filter', '$q', 'ngToast', 'helper', 'LeadConvertService', 'ModuleService', '$cache',
        function ($rootScope, $scope, $location, $state, $filter, $q, ngToast, helper, LeadConvertService, ModuleService, $cache) {
            $scope.id = $location.search().id;
            $scope.module = $filter('filter')($rootScope.modules, { name: 'leads' }, true)[0];
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

            $scope.accountModule = $filter('filter')($rootScope.modules, { name: 'accounts' }, true)[0];
            $scope.contactModule = $filter('filter')($rootScope.modules, { name: 'contacts' }, true)[0];
            $scope.opportunityModule = $filter('filter')($rootScope.modules, { name: 'opportunities' }, true)[0];
            $scope.nameField = $filter('filter')($scope.opportunityModule.fields, { name: 'name' }, true)[0];
            $scope.amountField = $filter('filter')($scope.opportunityModule.fields, { name: 'amount' }, true)[0];
            $scope.closingDateField = $filter('filter')($scope.opportunityModule.fields, { name: 'closing_date' }, true)[0];
            $scope.stageField = $filter('filter')($scope.opportunityModule.fields, { name: 'stage' }, true)[0];
            $scope.leadModulePrimaryField = $filter('filter')($scope.module.fields, { primary: true }, true)[0];
            $scope.loading = true;

            ModuleService.getPicklists($scope.opportunityModule)
                .then(function (picklists) {
                    $scope.stageList = picklists[$scope.stageField.picklist_id];
                });

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
                // Gecici olarak commentlendi. Kalıcı cozum aranacak
                // //Company,name and surname control for convert action. : #610
                // var required = {"first_name": "NameError", "last_name": "SurnameError", "company": "CompanyError"};
                // var message = "";
                //
                // angular.forEach(required, function (key, val) {
                //     if (!$scope.lead[val])
                //         message += $filter('translate')('Convert.Lead.' + key) + "<br>";
                // });

                // if (message) {
                //     ngToast.create({
                //         content: message,
                //         className: 'warning'
                //     });
                //     return;
                // }

                if ($scope.createOpportunity && !$scope.opportunityForm.$valid)
                    return;

                $scope.loading = true;

                var convertRequest = {};
                convertRequest.lead_id = $scope.lead.id;
                convertRequest.deleted = $scope.deleted;

                if ($scope.opportunity) {
                    convertRequest.opportunity = $scope.opportunity;
                }

                LeadConvertService.convert(convertRequest)
                    .then(function (converted) {
                        $scope.converted = converted.data;
                        $scope.convertDisable = true;
                        $cache.remove('leads_leads');
                        $cache.remove('accounts_accounts');
                        $cache.remove('contacts_contacts');
                        $cache.remove('opportunities_opportunities');
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