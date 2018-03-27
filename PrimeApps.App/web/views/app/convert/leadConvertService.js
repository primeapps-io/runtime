'use strict';

angular.module('ofisim')

    .factory('LeadConvertService', ['$rootScope', '$http', 'config', '$filter', 'helper',
        function ($rootScope, $http, config, $filter, helper) {
            return {
                convert: function (query) {
                    return $http.post(config.apiUrl + 'convert/convert_lead', query);
                }
            };
        }]);