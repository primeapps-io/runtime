'use strict';

angular.module('primeapps')

    .factory('ProfilesService', ['$rootScope', '$http', 'config', '$filter', '$q', 'helper', '$cache', 'dataTypes', 'systemFields',
        function ($rootScope, $http, config, $filter, $q, helper, $cache, dataTypes, systemFields) {
            return {
                getAll: function () {
                    return $http.post(config.apiUrl + 'profile/get_all', {});
                },
                count: function () {
                    return $http.get(config.apiUrl + 'profile/count');
                },
                find: function (data) {
                    return $http.post(config.apiUrl + 'profile/find', data);
                },
                getAllBasic: function () {
                    return $http.get(config.apiUrl + 'profile/get_all_basic');
                },
                delete: function (id) {
                    return $http.delete(config.apiUrl + 'profile/delete/' + id);
                },
                getProfiles: function (allProfiles, modules, clearPermissions) {
                    var profiles = allProfiles;

                    angular.forEach(profiles, function (profile) {
                        if (profile.is_persistent && profile.has_admin_rights) {
                            profile.name = $filter('translate')('Setup.Profiles.Administrator');
                            profile.description = $filter('translate')('Setup.Profiles.AdministratorDescription');
                        }

                        //if (profile.is_persistent && !profile.has_admin_rights) {
                        //    profile.name = $filter('translate')('Setup.Profiles.Standard');
                        //    profile.description = $filter('translate')('Setup.Profiles.StandardDescription');
                        //}

                        if (clearPermissions) {
                            if (profile.permissions) {
                                profile.permissions = [];
                            }
                        }
                        else {
                            var permissions = [];

                            angular.forEach(profile.permissions, function (permission) {
                                var addPermission = true;

                                switch (permission.type) {
                                    case 'Document':/// Document
                                    case 1:
                                        permission.EntityTypeName = $filter('translate')('Layout.Menu.Documents');
                                        permission.order = 999;
                                        break;
                                    case 'Report':
                                    case 2:
                                        permission.EntityTypeName = $filter('translate')('Layout.Menu.Reports');
                                        permission.order = 1000;
                                        break;
                                    case 'Newsfeed':
                                    case 3:
                                        permission.EntityTypeName = $filter('translate')('Feed.Feed');
                                        permission.Order = 1001;
                                        break;

                                    case 'Module':/// Module
                                    case 0:
                                        var module = $filter('filter')(modules, { id: permission.module_id }, true)[0];

                                        if (module && module.order > 0) {
                                            permission.EntityTypeName = module["label_" + $rootScope.language + "_plural"];
                                            permission.order = module.order;
                                        }
                                        else {
                                            addPermission = false;
                                        }
                                        break;
                                }

                                if (addPermission)
                                    permissions.push(permission);
                            });

                            profile.permissions = $filter('orderBy')(permissions, 'Order');
                        }
                    });

                    profiles = $filter('orderBy')(profiles, ['-is_persistent', '-has_admin_rights', '+name']);

                    return profiles;
                },

                changeUserProfile: function (userId, tenantId, transferedProfileId) {
                    return $http.post(config.apiUrl + 'profile/change_user_profile', {
                        User_ID: userId,
                        tenant_id: tenantId,
                        Transfered_Profile_ID: transferedProfileId
                    });
                },

                create: function (profile) {
                    return $http.post(config.apiUrl + 'profile/create', profile);
                },

                update: function (profile) {
                    return $http.post(config.apiUrl + 'profile/update', profile);
                }
            };
        }]);

