'use strict';

angular.module('ofisim')

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
                        if (profile.IsPersistent && profile.HasAdminRights) {
                            profile.Name = $filter('translate')('Setup.Profiles.Administrator');
                            profile.Description = $filter('translate')('Setup.Profiles.AdministratorDescription');
                        }

                        if (profile.IsPersistent && !profile.HasAdminRights) {
                            profile.Name = $filter('translate')('Setup.Profiles.Standard');
                            profile.Description = $filter('translate')('Setup.Profiles.StandardDescription');
                        }

                        if (clearPermissions) {
                            delete profile.Permissions;
                        }
                        else {
                            var permissions = [];

                            angular.forEach(profile.Permissions, function (permission) {
                                var addPermission = true;

                                switch (permission.Type) {
                                    case 1:/// Document
                                        permission.EntityTypeName = $filter('translate')('Layout.Menu.Documents');
                                        permission.Order = 999;
                                        break;
                                    case 2:
                                        permission.EntityTypeName = $filter('translate')('Layout.Menu.Reports');
                                        permission.Order = 1000;
                                        break;

                                    case 0 :/// Module
                                        var module = $filter('filter')($rootScope.modules, { id: permission.ModuleId }, true)[0];

                                        if (module && module.order > 0) {
                                            permission.EntityTypeName = module["label_" + $rootScope.language + "_plural"];
                                            permission.Order = module.order;
                                        }
                                        else {
                                            addPermission = false;
                                        }
                                        break;
                                }

                                if (addPermission)
                                    permissions.push(permission);
                            });

                            profile.Permissions = $filter('orderBy')(permissions, 'Order');
                        }
                    });

                    profiles = $filter('orderBy')(profiles, ['-IsPersistent', '-HasAdminRights', '+Name']);

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
                            RemovedProfile: { ID: removedProfileId, InstanceID: instanceId },
                            TransferProfile: { ID: transferProfileId, InstanceID: instanceId }
                        });
                }
            };
        }]);