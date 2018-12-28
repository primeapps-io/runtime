'use strict';

angular.module('primeapps')

    .factory('ComponentsService', ['$rootScope', '$http', 'config', '$filter', '$q', 'helper',   '$cache', 'dataTypes', 'systemFields',
        function ($rootScope, $http, config, $filter, $q, helper,  $cache, dataTypes, systemFields) {
            return {
                getReports: function () {
                        console.log("Merhaba");
                }
            };
        }]);

