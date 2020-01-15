'use strict';

angular.module('primeapps')

    .factory('FunctionsService', ['$rootScope', '$http', 'config', '$filter', '$q', 'helper', '$cache', 'dataTypes', 'systemFields','environments',
        function ($rootScope, $http, config, $filter, $q, helper, $cache, dataTypes, systemFields, environments) {
            return {
                count: function () {
                    return $http.get(config.apiUrl + 'functions/count');
                },
                find: function (request) {
                    return $http.post(config.apiUrl + 'functions/find', request);
                },
                get: function (id) {
                    return $http.get(config.apiUrl + 'functions/get/' + id);
                },
                getByName: function (name) {
                    return $http.get(config.apiUrl + 'functions/get_by_name/' + name);
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
                getPods: function (name) {
                    return $http.get(config.apiUrl + 'functions/get_pods/' + name);
                },
                getLogs: function (name) {
                    return $http.get(config.apiUrl + 'functions/get_logs/' + name);
                },
                isFunctionNameUnique: function (name, canceller) {
                    return $http.get(config.apiUrl + 'functions/is_unique_name?name=' + name, {timeout: canceller.promise});
                },
                deploy: function (name) {
                    return $http.get(config.apiUrl + 'functions/deploy/' + name);
                },
                getEnvironments: function () {
                    return environments.data;
                }
            };
        }]);

