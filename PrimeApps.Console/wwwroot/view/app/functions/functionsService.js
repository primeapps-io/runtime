'use strict';

angular.module('primeapps')

    .factory('FunctionsService', ['$rootScope', '$http', 'config', '$filter', '$q', 'helper',   '$cache', 'dataTypes', 'systemFields',
        function ($rootScope, $http, config, $filter, $q, helper,  $cache, dataTypes, systemFields) {
            return {
                get: function (name) {
                    return $http.get(config.apiUrl + 'functions/get/' + name);
                },
                run: function (name, type, request) {
                    if (type === 'post') {
                        return $http.post(config.apiUrl + 'functions/run/' + name, request);
                    }
                    else if (type === 'get') {
                        return $http.get(config.apiUrl + 'functions/run/' + name);
                    }
                },
                create: function (request) {
                    return $http.post(config.apiUrl + 'functions/create', request);
                },
                update: function (name, request) {
                    return $http.put(config.apiUrl + 'functions/update/' + name, request);
                },
                delete: function (name) {
                    return $http.delete(config.apiUrl + 'functions/delete/' + name);
                },
                getAll: function () {
                    return $http.get(config.apiUrl + 'functions/get_all');
                },
                getPods: function (name){
                    return $http.get(config.apiUrl + 'functions/get_pods/' + name);
                },
                getLogs: function (name){
                    return $http.get(config.apiUrl + 'functions/get_logs/' + name);
                }
            };
        }]);

