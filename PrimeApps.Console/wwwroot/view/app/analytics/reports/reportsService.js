'use strict';

angular.module('primeapps')

    .factory('ReportsService', ['$rootScope', '$http', 'config', '$filter', '$q', 'helper',  '$cache', 'dataTypes', 'systemFields',
        function ($rootScope, $http, config, $filter, $q, helper, defaultLabels, $cache, systemFields) {
            return {
                getReports: function () {
                    console.log("Merhaba");
                }
            };
        }]);
