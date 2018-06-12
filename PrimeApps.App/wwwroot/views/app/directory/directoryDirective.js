'use strict';

angular.module('ofisim')
    .directive('directory', ['$rootScope', '$state', '$filter', 'ModuleService',
        function ($rootScope, $state, $filter, ModuleService) {
            return {
                restrict: 'EA',
                scope: {
                    id: '=',
                    showBack: '='
                },
                templateUrl: cdnUrl + 'views/app/directory/directory.html',
                controller: ['$scope',
                    function ($scope) {
                        $scope.loading = true;
                        $scope.showInfo = true;
                        $scope.module = $filter('filter')($rootScope.modules, { name: 'rehber' }, true)[0];

                        if (!$scope.module) {
                            ngToast.create({ content: $filter('translate')('Common.NotFound'), className: 'warning' });
                            $state.go('app.crm.dashboard');
                            return;
                        }

                        var findRequest = {
                            fields: ['ad_soyad', 'cep_telefonu', 'is_telefonu', 'e_posta', 'lokasyon', 'sube', 'fotograf', 'departman', 'unvan', 'calisan_id', 'yoneticisi.rehber.ad_soyad', 'yoneticisi.rehber.unvan', 'yoneticisi.rehber.fotograf', 'yoneticisi.rehber.yoneticisi'],
                            filters: [{ field: 'e_posta', operator: 'is', value: $rootScope.user.email, no: 1 }],
                            sort_field: 'ad_soyad',
                            sort_direction: 'asc',
                            limit: 1,
                            offset: 0
                        };

                        var ozelCepTelefonuField = $filter('filter')($scope.module.fields, { name: 'ozel_cep_telefonu' }, true)[0];

                        if (ozelCepTelefonuField)
                            findRequest.fields.push('ozel_cep_telefonu');

                        if ($scope.id) {
                            findRequest.filters = [{ field: 'id', operator: 'equals', value: $scope.id, no: 1 }];
                            $scope.showInfo = false;
                        }

                        ModuleService.findRecords('rehber', findRequest)
                            .then(function (response) {
                                $scope.record = response.data[0];

                                if (!$scope.record)
                                    return;

                                if ($scope.record['yoneticisi.rehber.id']) {
                                    var findRequestPeer = {
                                        filters: [
                                            { field: 'yoneticisi', operator: 'equals', value: $scope.record['yoneticisi.rehber.id'], no: 1 },
                                            { field: 'id', operator: 'not_equal', value: $scope.record['id'], no: 2 }
                                        ],
                                        sort_field: 'ad_soyad',
                                        sort_direction: 'asc',
                                        limit: 50,
                                        offset: 0
                                    };

                                    if (!$scope.id)
                                        findRequestPeer.filters.push({ field: 'departman', operator: 'is', value: $scope.record['departman'], no: 3 });

                                    ModuleService.findRecords('rehber', findRequestPeer)
                                        .then(function (responsePeer) {
                                            $scope.peers = responsePeer.data;

                                            if (!$scope.record['yoneticisi.rehber.id'])
                                                return;

                                            var setManager = function (record) {
                                                var manager = {
                                                    id: record['yoneticisi.rehber.id'],
                                                    ad_soyad: record['yoneticisi.rehber.ad_soyad'],
                                                    unvan: record['yoneticisi.rehber.unvan'],
                                                    fotograf: record['yoneticisi.rehber.fotograf'],
                                                    yoneticisi: record['yoneticisi.rehber.yoneticisi']
                                                };

                                                return manager;
                                            };

                                            $scope.managerFirst = setManager($scope.record);
                                        });
                                }

                                var findRequestDirectReport = {
                                    filters: [
                                        { field: 'yoneticisi', operator: 'equals', value: $scope.record['id'], no: 1 }
                                    ],
                                    sort_field: 'ad_soyad',
                                    sort_direction: 'asc',
                                    limit: 50,
                                    offset: 0
                                };

                                ModuleService.findRecords('rehber', findRequestDirectReport)
                                    .then(function (responseDirectReport) {
                                        $scope.directReports = responseDirectReport.data;
                                    });
                            })
                            .finally(function () {
                                $scope.loading = false;
                                $scope.$parent.showBack = true;
                            });

                        $scope.goto = function (id) {
                            if (id)
                                $state.go('app.crm.directory', { id: id });
                            else if (!$scope.id)
                                $state.go('app.crm.moduleDetail', { type: 'calisanlar', id: $scope.record['calisan_id'] });
                        };

                        $scope.getField = function (fieldName) {
                            var field = $filter('filter')($scope.module.fields, { name: fieldName, deleted: '!true' })[0];

                            return field;
                        };

                        $scope.getFieldLabel = function (fieldName) {
                            var field = $filter('filter')($scope.module.fields, { name: fieldName, deleted: '!true' })[0];

                            if (!field) {
                                ngToast.create({ content: $filter('translate')('Common.NotFound'), className: 'warning' });
                                $state.go('app.crm.dashboard');
                                return;
                            }

                            return field['label_' + $rootScope.language];
                        }
                    }]
            };
        }]);