'use strict';

angular.module('primeapps')

    .factory('ProfileService', ['$rootScope', '$http', 'config', '$filter', 'entityTypes',
        function ($rootScope, $http, config, $filter, entityTypes) {
            return {
                getAll: function () {
                    return $http.post(config.apiUrl + 'Profile/GetAll', {});
                },

                getAllBasic: function () {
                    return $http.get(config.apiUrl + 'Profile/GetAllBasic');
                },

                getProfiles: function (allProfiles, instanceId, clearPermissions) {
                    var profiles = allProfiles;

                    angular.forEach(profiles, function (profile) {
                        if (profile.is_persistent && profile.has_admin_rights) {
                            profile.name = $filter('translate')('Setup.Profiles.Administrator');
                            profile.description = $filter('translate')('Setup.Profiles.AdministratorDescription');
                        }

                        if (profile.is_persistent && !profile.has_admin_rights) {
                            profile.name = $filter('translate')('Setup.Profiles.Standard');
                            profile.description = $filter('translate')('Setup.Profiles.StandardDescription');
                        }

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
                                    case 1:/// Document
                                        permission.EntityTypeName = $filter('translate')('Layout.Menu.Documents');
                                        permission.order = 999;
                                        break;
                                    case 2:
                                        permission.EntityTypeName = $filter('translate')('Layout.Menu.Reports');
                                        permission.order = 1000;
                                        break;
                                    case 0:/// Module
                                        var module = $filter('filter')($rootScope.modules, { id: permission.module_id }, true)[0];

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

                changeUserProfile: function (userId, instanceId, transferedProfileId) {
                    return $http.post(config.apiUrl + 'Profile/ChangeUserProfile', {
                        UserID: userId,
                        InstanceID: instanceId,
                        TransferedProfileID: transferedProfileId
                    });
                },

                create: function (profile) {
                    return $http.post(config.apiUrl + 'Profile/Create', profile);
                },

                update: function (profile) {
                    return $http.post(config.apiUrl + 'Profile/Update', profile);
                },

                remove: function (removedProfileId, transferProfileId, instanceId) {
                    return $http.post(config.apiUrl + 'Profile/Remove',
                        {
                            RemovedProfile: { id: removedProfileId, InstanceID: instanceId },
                            TransferProfile: { id: transferProfileId, InstanceID: instanceId }
                        });
                }
            };
        }]);