'use strict';

angular.module('primeapps')
    .controller('AuditLogController', ['$rootScope', '$scope', 'config', 'ngToast', '$localStorage', '$filter', 'ngTableParams', '$stateParams', 'AuditLogService',
        function ($rootScope, $scope, config, ngToast, $localStorage, $filter, ngTableParams, $stateParams, AuditLogService) {
            $scope.actionTypes = AuditLogService.getActionsTypes();
            $scope.currentPage = 1;

            var find = function (limit, page, recordActionType, setupActionType, userId, startDate, endDate) {
                $scope.allLogsLoaded = false;

                var request = {
                    limit: limit,
                    offset: (page - 1) * limit,
                    record_action_type: recordActionType,
                    setup_action_type: setupActionType,
                    user_id: userId,
                    start_date: startDate,
                    end_date: endDate
                };

                AuditLogService.find(request)
                    .then(function (auditLogs) {
                        auditLogs = auditLogs.data;
                        $scope.pagingIcon = 'fa-chevron-right';

                        if ($scope.currentPage === 1)
                            $scope.auditLogs = AuditLogService.process(auditLogs, $scope.allModules);
                        else
                            $scope.auditLogs = $scope.auditLogs.concat(AuditLogService.process(auditLogs, $scope.allModules));

                        if (auditLogs.length < 1 || auditLogs.length < request.limit)
                            $scope.allLogsLoaded = true;

                        $scope.searching = false;
                    });
            };
            $scope.init = 1;
            $scope.find = function () {
                if ($scope.init === 1) {
                    $scope.searching = true;
                    $scope.init++;
                    find(30, 1);
                    return;
                }

                if (!$scope.auditLogFilter) {
                    find(30, $scope.currentPage);
                    return;
                }

                var recordActionType = null;
                var setupActionType = null;
                var userId = null;
                var startDate = null;
                var endDate = null;

                if ($scope.auditLogFilter.action_type) {
                    if ($scope.auditLogFilter.action_type.indexOf("module") > -1) {
                        setupActionType = $scope.auditLogFilter.action_type;
                    } else {
                        recordActionType = $scope.auditLogFilter.action_type;
                    }
                }

                if ($scope.auditLogFilter.user)
                    userId = $scope.auditLogFilter.user;
                if ($scope.auditLogFilter.startDate)
                    startDate = $scope.auditLogFilter.startDate;
                if ($scope.auditLogFilter.endDate)
                    endDate = $scope.auditLogFilter.endDate;

                find(30, $scope.currentPage, recordActionType, setupActionType, userId, startDate, endDate);
            };


            $scope.loadMore = function () {
                if ($scope.allLogsLoaded)
                    return;

                $scope.pagingIcon = 'fa-spinner fa-spin';
                $scope.currentPage = $scope.currentPage + 1;
                $scope.find();
            };

            $scope.cancel = function () {
                $scope.showFilter = false;
                $scope.showFilterButton = true;

                if ($scope.auditLogFilter) {
                    $scope.auditLogFilter = null;
                    $scope.find();
                }
            };

            if (!$scope.allModules) {
                AuditLogService.getDeletedModules()
                    .then(function (deletedModules) {
                        $scope.allModules = $rootScope.modules.concat(deletedModules.data);
                        $scope.find();
                    });
            }
            else {
                $scope.find();
            }
        }
    ]);