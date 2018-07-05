angular.module('primeapps')
    .factory('HelpService', ['$http', 'config', '$filter', '$rootScope',
        function ($http, config, $filter, $rootScope) {
            return {
                create: function (help) {
                    return $http.post(config.apiUrl + 'help/create', help);
                },

                update: function (help) {
                    return $http.put(config.apiUrl + 'help/update/' + help.id, help);
                },

                delete: function (id) {
                    return $http.delete(config.apiUrl + 'help/delete/' + id);
                },

                getAll: function (type) {
                    return $http.get(config.apiUrl + 'help/get_all?modalType=' + type);
                },

                getById: function (id) {
                    return $http.get(config.apiUrl + 'help/get/' + id);
                },

                getByType: function (type, moduleId, route) {
                    moduleId = moduleId ? moduleId : null;
                    route = route ? route : null;
                    return $http.get(config.apiUrl + 'help/get_by_type?templateType=' + type + '&moduleId=' + moduleId + '&route=' + route);
                },
                getModuleType: function (templateType, moduleType, moduleId) {
                    moduleId = moduleId ? moduleId : null;
                    return $http.get(config.apiUrl + 'help/get_module_type?templateType=' + templateType + '&moduleType=' + moduleType + '&moduleId=' + moduleId);
                },

                getCustomHelp: function (templateType, customhelp) {

                    return $http.get(config.apiUrl + 'help/get_custom_help?templateType=' + templateType + '&customhelp=' + customhelp);
                },

                process: function (helpsidesData, modules, routes, helpEnums) {
                    var helpsides = [];

                    for (var i = 0; i < helpsidesData.length; i++) {
                        var helpside = helpsidesData[i];
                        helpside.binding = '';

                        if (helpside.module_id) {
                            var module = $filter('filter')(modules, { id: helpside.module_id }, true)[0];
                            var helpEnum = $filter('filter')(helpEnums, { Name: helpside.module_type }, true)[0];

                            helpside.binding = (module ? module['label_' + $rootScope.language + '_singular'] + ' ' + '(' : '') + (helpEnum ? helpEnum.Label + ')' : '');
                        }
                        else if (helpside.route_url) {
                            var route = $filter('filter')(routes, { value: helpside.route_url }, true)[0];

                            if (route)
                                helpside.binding = route.name;
                        }
                        else {
                            helpside.binding = $filter('translate')('Setup.HelpGuide.Independent');
                        }

                        helpsides.push(helpside);
                    }

                    return helpsides;
                }
            };
        }
    ]);