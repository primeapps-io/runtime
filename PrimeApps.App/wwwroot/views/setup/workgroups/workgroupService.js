'use strict';

angular.module('primeapps')

    .factory('WorkgroupService', ['$http', 'config', '$filter', 'entityTypes',
        function ($http, config, $filter, entityTypes) {
            return {
                getWorkgoups: function () {
                    return $http.post(config.apiUrl + 'Instance/GetWorkgroups', {});
                },

                getWorkgoup: function () {
                    return $http.post(config.apiUrl + 'Instance/GetWorkgroup', {});
                },

                create: function (title) {
                    return $http.post(config.apiUrl + 'Instance/Create', angular.toJson(title));
                },

                edit: function (title, instanceId) {
                    return $http.post(config.apiUrl + 'Instance/Edit', {
                        title: title,
                        instanceID: instanceId
                    });
                },

                remove: function (workgroupId) {
                    return $http.post(config.apiUrl + 'Instance/Remove', angular.toJson(workgroupId));
                },

                leave: function (workgroupId) {
                    return $http.post(config.apiUrl + 'Instance/Leave', angular.toJson(workgroupId));
                },

                join: function (instanceId) {
                    return $http.post(config.apiUrl + 'Instance/Join', angular.toJson(instanceId));
                },

                decline: function (instanceId) {
                    return $http.post(config.apiUrl + 'Instance/Decline', angular.toJson(instanceId));
                },

                isAvailableToInvite: function (email, instanceId) {
                    return $http.post(config.apiUrl + 'User/IsAvailableToInvite', {
                        EMail: email,
                        InstanceID: instanceId
                    });
                },

                saveType: function (entityType) {
                    return $http.post(config.apiUrl + 'Entity/SaveOrUpdateType', entityType);
                },

                removeType: function (entityType) {
                    return $http.post(config.apiUrl + 'Entity/RemoveType', entityType);
                },

                removeTypeHistory: function (entityTypeId, instanceId) {
                    return $http.post(config.apiUrl + 'User/RemoveTypeHistory', {
                        EntityType: entityTypeId,
                        InstanceID: instanceId
                    });
                },

                upgradeLicense: function (licenseId, frequency) {
                    return $http.post(config.apiUrl + 'License/Upgrade', {
                        LicenseID: licenseId,
                        Frequency: frequency,
                        IsFreeLicense: true
                    });
                },

                getValues: function (fields, users, clientData, countries, instanceId) {
                    var vals = [];

                    angular.forEach(fields, function (value) {
                        var newValue = {};
                        angular.copy(value, newValue);
                        newValue.ValueId = value.ID;

                        switch (value.Type) {
                            case 3:
                                newValue.ValueInt = value.DefaultValue && parseInt(value.DefaultValue);
                                break;
                            case 4:
                                newValue.ValueDate = value.DefaultValue && new Date(value.DefaultValue.split('.').reverse().join('-'));
                                break;
                            case 6:
                                newValue.ValueList = value.DefaultValue;
                                newValue.Values = value.Value.split(',');
                                break;
                            case 9:
                                newValue.ValueText = value.DefaultValue == 'true' ? $filter('translate')('Common.Yes') : $filter('translate')('Common.No');
                                break;
                            case 10:
                                var data = $filter('filter')(clientData, {EntityID: value.DefaultValue, InstanceID: instanceId}, true)[0];
                                newValue.ValueText = data && data.EntityName;
                                newValue.ValueObject = data;
                                newValue.Values = $filter('filter')(clientData, {EntityType: value.Value, InstanceID: instanceId}, true);
                                break;
                            case 13:
                                var user = $filter('filter')(users, {EntityID: value.DefaultValue}, true)[0];
                                newValue.ValueText = user ? user.EntityName : '';
                                newValue.Values = users;
                                break;
                            case 14:
                                var country = $filter('filter')(countries, {Code: value.DefaultValue}, true)[0];
                                newValue.ValueText = country && country.Name;
                                newValue.Values = countries;
                                break;
                        }

                        this.push(newValue);
                    }, vals);

                    vals = $filter('orderBy')(vals, 'Order');

                    return vals;
                },

                getResourceUsageForType: function (entityType, instanceId) {
                    return $http.post(config.apiUrl + 'Entity/GetResourceUsageForType', {
                        EntityType: entityType,
                        InstanceID: instanceId
                    });
                },

                getAllEntityTypes: function (workGroupEntityTypes) {
                    var allEntityTypes = [];
                    allEntityTypes.push({id: entityTypes.lead, name: $filter('translate')('Lead.Lead')});
                    allEntityTypes.push({id: entityTypes.account, name: $filter('translate')('Account.Account')});
                    allEntityTypes.push({id: entityTypes.contact, name: $filter('translate')('Contact.Contact')});
                    allEntityTypes.push({id: entityTypes.opportunity, name: $filter('translate')('Opportunity.Opportunity')});
                    allEntityTypes.push({id: entityTypes.activity, name: $filter('translate')('Activity.Activity')});

                    angular.forEach(workGroupEntityTypes, function (workGroupEntityType) {
                        allEntityTypes.push({id: workGroupEntityType.ID, name: workGroupEntityType.Title});
                    });

                    return allEntityTypes;
                }
            };
        }]);