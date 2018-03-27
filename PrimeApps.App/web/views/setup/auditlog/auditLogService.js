'use strict';

angular.module('ofisim')
    .factory('AuditLogService', ['$rootScope', '$http', '$filter', 'config',
        function ($rootScope, $http, $filter, config) {
            return {

                find: function (request) {
                    return $http.post(config.apiUrl + 'data/find_audit_logs', request);
                },

                getDeletedModules: function () {
                    return $http.get(config.apiUrl + 'module/get_all_deleted');
                },

                process: function (auditLogs, modules) {
                    angular.forEach(auditLogs, function (auditLog) {
                        if (auditLog.audit_type === 'setup')
                            auditLog.module = $filter('filter')(modules, { id: auditLog.record_id }, true)[0];
                    });

                    return auditLogs;
                },

                getActionsTypes: function () {
                    var actionTypes = [];
                    var actionType1 = { id: 'inserted', label: $filter('translate')('Setup.AuditLog.ActionTypes.inserted'), type: 'module' };
                    var actionType2 = { id: 'updated', label: $filter('translate')('Setup.AuditLog.ActionTypes.updated'), type: 'module' };
                    var actionType3 = { id: 'deleted', label: $filter('translate')('Setup.AuditLog.ActionTypes.deleted'), type: 'module' };
                    var actionType4 = { id: 'module_created', label: $filter('translate')('Setup.AuditLog.ActionTypes.module_created'), type: 'setup' };
                    var actionType5 = { id: 'module_updated', label: $filter('translate')('Setup.AuditLog.ActionTypes.module_updated'), type: 'setup' };
                    var actionType6 = { id: 'module_deleted', label: $filter('translate')('Setup.AuditLog.ActionTypes.module_deleted'), type: 'setup' };
                    actionTypes.push(actionType1);
                    actionTypes.push(actionType2);
                    actionTypes.push(actionType3);
                    actionTypes.push(actionType4);
                    actionTypes.push(actionType5);
                    actionTypes.push(actionType6);

                    return actionTypes;
                }
            };
        }]);

