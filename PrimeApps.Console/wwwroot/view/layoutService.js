'use strict';

angular.module('primeapps')

    .factory('LayoutService', ['$rootScope', '$http', '$localStorage', '$cache', '$q', '$filter', '$timeout', '$state', 'config', 'helper', 'entityTypes', 'taskDate', 'dataTypes', 'activityTypes', 'operators', 'systemRequiredFields', 'systemReadonlyFields', '$window', '$modal', '$sce',
        function ($rootScope, $http, $localStorage, $cache, $q, $filter, $timeout, $state, config, helper, entityTypes, taskDate, dataTypes, activityTypes, operators, systemRequiredFields, systemReadonlyFields, $window, $modal, $sce) {
            return {
                getAll: function () {
                    var deferred = $q.defer();
                    var promises = [];
                    promises.push($http.get(config.apiUrl + 'user/me'));
                    promises.push($http.get(config.apiUrl + 'user/organizations'));
                    $q.all(promises).then(function (response) {
                        $rootScope.me = response[0].data;
                        $rootScope.organizations = response[1].data;

                        if (!$rootScope.breadcrumblist) {
                            $rootScope.breadcrumblist = [{}, {}, {}];
                        }
                        helper.hideLoader();
                        deferred.resolve(true);
                        return deferred.promise;
                    });

                    return deferred.promise;
                },
                me: function () {
                    return $http.get(config.apiUrl + 'user/me');
                },
                myOrganizations: function () {
                    return $http.get(config.apiUrl + 'user/organizations');
                },
                createOrganization: function (data) {
                    return $http.post(config.apiUrl + 'organization/create', data);
                },
                isOrganizationShortnameUnique: function (name) {
                    return $http.get(config.apiUrl + 'organization/is_unique_name?name=' + name);
                },
                getAppInfo: function (appId) {
                    $http.get(config.apiUrl + "app/get/" + $scope.appId);
                },
                getBasicModules: function () {
                    return $http.get(config.apiUrl + 'module/get_all_basic');
                },
                getPreviewToken: function () {
                    return $http.get(config.apiUrl + 'preview/key');
                },
                processModule: function (module) {
                    for (var i = 0; i < module.sections.length; i++) {
                        var section = module.sections[i];
                        section.columns = [];
                        var sectionPermissions = [];

                        if (section.permissions)
                            sectionPermissions = angular.copy(section.permissions);

                        section.permissions = [];

                        for (var j = 0; j < $rootScope.profiles.length; j++) {
                            var profile = $rootScope.profiles[j];

                            if (profile.is_persistent && profile.has_admin_rights)
                                profile.name = $filter('translate')('Setup.Profiles.Administrator');

                            if (profile.is_persistent && !profile.has_admin_rights)
                                profile.name = $filter('translate')('Setup.Profiles.Standard');

                            var sectionPermission = $filter('filter')(sectionPermissions, {profile_id: profile.id}, true)[0];

                            if (!sectionPermission) {
                                section.permissions.push({
                                    profile_id: profile.id,
                                    profile_name: profile.name,
                                    profile_is_admin: profile.has_admin_rights,
                                    type: 'full'
                                });
                            }
                            else {
                                section.permissions.push({
                                    id: sectionPermission.id,
                                    profile_id: profile.id,
                                    profile_name: profile.name,
                                    profile_is_admin: profile.has_admin_rights,
                                    type: sectionPermission.type
                                });
                            }
                        }

                        for (var k = 1; k <= section.column_count; k++) {
                            var column = {};
                            column.no = k;

                            section.columns.push(column);
                        }
                    }

                    for (var l = 0; l < module.fields.length; l++) {
                        var field = module.fields[l];
                        field.label = field['label_' + $rootScope.language];
                        field.dataType = dataTypes[field.data_type];
                        field.operators = [];
                        field.sectionObj = $filter('filter')(module.sections, {name: field.section}, true)[0];

                        if (field.data_type === 'lookup') {
                            if (field.lookup_type != 'users' && field.lookup_type != 'profiles' && field.lookup_type != 'roles' && field.lookup_type != 'relation') {
                                var lookupModule = $filter('filter')($rootScope.modules, {name: field.lookup_type}, true)[0];

                                if (!lookupModule)
                                    continue;

                                field.lookupModulePrimaryField = $filter('filter')(lookupModule.fields, {primary_lookup: true}, true)[0];

                                if (!field.lookupModulePrimaryField)
                                    field.lookupModulePrimaryField = $filter('filter')(lookupModule.fields, {primary: true}, true)[0];

                                var lookupModulePrimaryFieldDataType = dataTypes[field.lookupModulePrimaryField.data_type];

                                for (var m = 0; m < lookupModulePrimaryFieldDataType.operators.length; m++) {
                                    var operatorIdLookup = lookupModulePrimaryFieldDataType.operators[m];
                                    var operatorLookup = operators[operatorIdLookup];
                                    field.operators.push(operatorLookup);
                                }
                            }
                            else {
                                field.operators.push(operators.equals);
                                field.operators.push(operators.not_equal);
                                field.operators.push(operators.empty);
                                field.operators.push(operators.not_empty);

                                if (field.lookup_type === 'users') {
                                    var lookupModule = $filter('filter')($rootScope.modules, {name: 'users'}, true)[0];
                                    field.lookupModulePrimaryField = $filter('filter')(lookupModule.fields, {primary: true}, true)[0];
                                }
                                else if (field.lookup_type === 'profiles') {
                                    var lookupModule = $filter('filter')($rootScope.modules, {name: 'profiles'}, true)[0];
                                    field.lookupModulePrimaryField = $filter('filter')(lookupModule.fields, {primary: true}, true)[0];
                                }
                                else if (field.lookup_type === 'roles') {
                                    var lookupModule = $filter('filter')($rootScope.modules, {name: 'roles'}, true)[0];
                                    field.lookupModulePrimaryField = $filter('filter')(lookupModule.fields, {primary: true}, true)[0];
                                }
                            }

                        }
                        else {
                            for (var n = 0; n < field.dataType.operators.length; n++) {
                                var operatorId = field.dataType.operators[n];
                                var operator = operators[operatorId];
                                field.operators.push(operator);
                            }
                        }

                        if (field.name === 'related_module') {
                            field.picklist_original_id = angular.copy(field.picklist_id);
                            field.picklist_id = 900000;
                        }

                        if (systemRequiredFields.all.indexOf(field.name) > -1 || (systemRequiredFields[module.name] && systemRequiredFields[module.name].indexOf(field.name) > -1))
                            field.systemRequired = true;

                        if (systemReadonlyFields.all.indexOf(field.name) > -1 || (systemReadonlyFields[module.name] && systemReadonlyFields[module.name].indexOf(field.name) > -1))
                            field.systemReadonly = true;

                        var fieldPermissions = [];

                        if (field.permissions)
                            fieldPermissions = angular.copy(field.permissions);

                        field.permissions = [];

                        for (var o = 0; o < $rootScope.profiles.length; o++) {
                            var profileItem = $rootScope.profiles[o];

                            if (profileItem.is_persistent && profileItem.has_admin_rights)
                                profileItem.name = $filter('translate')('Setup.Profiles.Administrator');

                            if (profileItem.is_persistent && !profileItem.has_admin_rights)
                                profileItem.name = $filter('translate')('Setup.Profiles.Standard');

                            var fieldPermission = $filter('filter')(fieldPermissions, {profile_id: profileItem.id}, true)[0];

                            if (!fieldPermission)
                                field.permissions.push({
                                    profile_id: profileItem.id,
                                    profile_name: profileItem.name,
                                    profile_is_admin: profileItem.has_admin_rights,
                                    type: 'full'
                                });
                            else
                                field.permissions.push({
                                    id: fieldPermission.id,
                                    profile_id: profileItem.id,
                                    profile_name: profileItem.name,
                                    profile_is_admin: profileItem.has_admin_rights,
                                    type: fieldPermission.type
                                });
                        }
                    }

                    if (module.dependencies) {
                        for (var p = 0; p < module.dependencies.length; p++) {
                            var dependency = module.dependencies[p];

                            if (dependency.dependency_type === 'display') {
                                if (!module.display_dependencies)
                                    module.display_dependencies = [];

                                var displayDependency = {};
                                displayDependency.field = dependency.parent_field;
                                displayDependency.dependent_field = dependency.child_field;
                                displayDependency.dependent_section = dependency.child_section;
                                displayDependency.otherwise = dependency.otherwise;
                                displayDependency.deleted = dependency.deleted;
                                displayDependency.values = [];

                                if (dependency.values && !angular.isArray(dependency.values)) {
                                    var values = dependency.values.split(',');

                                    for (var ji = 0; ji < values.length; ji++) {
                                        var value = values[ji];
                                        displayDependency.values.push(parseInt(value));
                                    }
                                }

                                module.display_dependencies.push(displayDependency);
                            }
                            else {
                                if (dependency.value_map && !angular.isArray(dependency.value_map)) {
                                    dependency.value_maps = {};

                                    var valueMaps = dependency.value_map.split('|');

                                    for (var jk = 0; jk < valueMaps.length; jk++) {
                                        var valueMap = valueMaps[jk];
                                        var map = valueMap.split(';');
                                        var valuesStrArray = map[1].split(',');
                                        var valuesMap = [];

                                        for (var jl = 0; jl < valuesStrArray.length; jl++) {
                                            var valueStr = valuesStrArray[jl];
                                            valuesMap.push(parseInt(valueStr));
                                        }

                                        dependency.value_maps[map[0]] = valuesMap;
                                    }
                                }
                            }
                        }
                    }

                    if (module.relations) {
                        for (var r = 0; r < module.relations.length; r++) {
                            var relation = module.relations[r];
                            relation.display_fields = relation.display_fields_array;
                        }
                    }

                    if (module.name === 'activities')
                        module.display_calendar = true;

                    return module;
                },

                getAppData: function () {
                    var promises = [];
                    var deferred = $q.defer();
                    promises.push($http.get(config.apiUrl + 'module/get_all_basic'));
                    promises.push($http.get(config.apiUrl + 'profile/get_all_basic'));
                    promises.push($http.get(config.apiUrl + "app/get/" + $rootScope.currentAppId));

                    return $q.all(promises).then(function (response) {
                        $rootScope.appModules = response[0].data;
                        $rootScope.appProfiles = response[1].data;
                        var result = response[2];
                        $rootScope.currentApp = result.data;
                        $rootScope.currentOrganization = $filter('filter')($rootScope.organizations, { id: parseInt($rootScope.currentApp.organization_id) } ,true)[0];

                        if (!angular.isArray($rootScope.breadcrumblist))
                            $rootScope.breadcrumblist = [{}, {}, {}];

                        $rootScope.breadcrumblist[0].title = $rootScope.currentOrganization.name;
                        $rootScope.breadcrumblist[0].link = '#/apps?organizationId=' + $rootScope.currentApp.organization_id;

                        $rootScope.breadcrumblist[1] = {
                            title: result.data.name,
                            link: '#/org/' + $rootScope.currentApp.organization_id + '/app/' + $rootScope.currentApp.id + '/overview'
                        };

                        $rootScope.menuTopTitle = $rootScope.currentApp.name;
                        deferred.resolve(true);
                        return deferred.promise;
                    });

                    return deferred.promise;
                }
            };
        }]);

angular.module('primeapps')
    .constant('helps',
        {
            maps: [
                {
                    "route": "default",
                    "help": "https://help.ofisim.com/",
                    "language": "en",
                    "appId": 1
                },
                {
                    "route": "default",
                    "help": "https://help.ofisim.com/",
                    "language": "en",
                    "appId": 2
                },
                {
                    "route": "default",
                    "help": "https://help.ofisim.com/",
                    "language": "en",
                    "appId": 3
                },
                {
                    "route": "default",
                    "help": "https://help.ofisim.com/",
                    "language": "en",
                    "appId": 4
                },
                {
                    "route": "default",
                    "help": "https://help.ofisim.com/",
                    "language": "en",
                    "appId": 5
                },
                {
                    "route": "default",
                    "help": "https://yardim.ofisim.com/category/genel/",
                    "language": "tr",
                    "appId": 1
                },
                {
                    "route": "default",
                    "help": "https://yardim.ofisim.com/category/genel/",
                    "language": "tr",
                    "appId": 2
                },
                {
                    "route": "default",
                    "help": "https://yardim.ofisim.com/category/asistan/",
                    "language": "tr",
                    "appId": 3
                },
                {
                    "route": "default",
                    "help": "https://yardim.ofisim.com/category/ik/",
                    "language": "tr",
                    "appId": 4
                },
                {
                    "route": "default",
                    "help": "https://yardim.ofisim.com/category/cagri/",
                    "language": "tr",
                    "appId": 5
                },

                {
                    "route": "default-setup",
                    "help": "https://yardim.ofisim.com/category/ayarlar/",
                    "language": "tr",
                    "appId": 1
                },
                {
                    "route": "default-setup",
                    "help": "https://yardim.ofisim.com/category/ayarlar/",
                    "language": "tr",
                    "appId": 2
                },
                {
                    "route": "default-setup",
                    "help": "https://yardim.ofisim.com/category/asistan-10/",
                    "language": "tr",
                    "appId": 3
                },
                {
                    "route": "default-setup",
                    "help": "https://yardim.ofisim.com/category/ik-13/",
                    "language": "tr",
                    "appId": 4
                },
                {
                    "route": "default-setup",
                    "help": "https://yardim.ofisim.com/category/cagri-9",
                    "language": "tr",
                    "appId": 5
                },
                {
                    "route": "#/app/dashboard",
                    "help": "https://yardim.ofisim.com/category/genel/",
                    "language": "tr",
                    "appId": 1
                },
                {
                    "route": "#/app/dashboard",
                    "help": "https://yardim.ofisim.com/category/genel/",
                    "language": "tr",
                    "appId": 2
                },
                {
                    "route": "#/app/dashboard",
                    "help": "https://yardim.ofisim.com/category/asistan/",
                    "language": "tr",
                    "appId": 3
                },
                {
                    "route": "#/app/dashboard",
                    "help": "https://yardim.ofisim.com/category/ik/",
                    "language": "tr",
                    "appId": 4
                },
                {
                    "route": "#/app/dashboard",
                    "help": "https://yardim.ofisim.com/category/cagri/",
                    "language": "tr",
                    "appId": 5
                },
                {
                    "route": "#/app/import/",
                    "help": "https://yardim.ofisim.com/genel-kullanim-iceri-veri-aktarimi/",
                    "language": "tr",
                    "appId": 1
                },
                {
                    "route": "#/app/import/",
                    "help": "https://yardim.ofisim.com/genel-kullanim-iceri-veri-aktarimi/",
                    "language": "tr",
                    "appId": 2
                },
                {
                    "route": "#/app/import/",
                    "help": "https://yardim.ofisim.com/genel-kullanim-iceri-veri-aktarimi/",
                    "language": "tr",
                    "appId": 3
                },
                {
                    "route": "#/app/import/",
                    "help": "https://yardim.ofisim.com/genel-kullanim-iceri-veri-aktarimi/",
                    "language": "tr",
                    "appId": 4
                },
                {
                    "route": "#/app/import/",
                    "help": "https://yardim.ofisim.com/genel-kullanim-iceri-veri-aktarimi/",
                    "language": "tr",
                    "appId": 5
                },
                {
                    "route": "#/app/modules/leads",
                    "help": "https://yardim.ofisim.com/musteri-adaylari/",
                    "language": "tr",
                    "appId": 1
                },
                {
                    "route": "#/app/modules/leads",
                    "help": "https://yardim.ofisim.com/musteri-adaylari/",
                    "language": "tr",
                    "appId": 2
                },
                {
                    "route": "#/app/modules/leads",
                    "help": "https://yardim.ofisim.com/musteri-adaylari/",
                    "language": "tr",
                    "appId": 3
                },
                {
                    "route": "#/app/modules/leads",
                    "help": "https://yardim.ofisim.com/musteri-adaylari/",
                    "language": "tr",
                    "appId": 4
                },
                {
                    "route": "#/app/modules/leads",
                    "help": "https://yardim.ofisim.com/musteri-adaylari/",
                    "language": "tr",
                    "appId": 5
                },
                {
                    "route": "#/app/modules/accounts",
                    "help": "https://yardim.ofisim.com/firma-liste-gorunumu/",
                    "language": "tr",
                    "appId": 1
                },
                {
                    "route": "#/app/modules/accounts",
                    "help": "https://yardim.ofisim.com/firma-liste-gorunumu/",
                    "language": "tr",
                    "appId": 2
                },
                {
                    "route": "#/app/modules/accounts",
                    "help": "https://yardim.ofisim.com/firma-liste-gorunumu/",
                    "language": "tr",
                    "appId": 3
                },
                {
                    "route": "#/app/modules/accounts",
                    "help": "https://yardim.ofisim.com/firma-liste-gorunumu/",
                    "language": "tr",
                    "appId": 4
                },
                {
                    "route": "#/app/modules/accounts",
                    "help": "https://yardim.ofisim.com/firma-liste-gorunumu/",
                    "language": "tr",
                    "appId": 5
                },
                {
                    "route": "#/app/module/accounts",
                    "help": "https://yardim.ofisim.com/firma-detaylari/",
                    "language": "tr",
                    "appId": 1
                },
                {
                    "route": "#/app/module/accounts",
                    "help": "https://yardim.ofisim.com/firma-detaylari/",
                    "language": "tr",
                    "appId": 2
                },
                {
                    "route": "#/app/module/accounts",
                    "help": "https://yardim.ofisim.com/firma-detaylari/",
                    "language": "tr",
                    "appId": 3
                },
                {
                    "route": "#/app/module/accounts",
                    "help": "https://yardim.ofisim.com/firma-detaylari/",
                    "language": "tr",
                    "appId": 4
                },
                {
                    "route": "#/app/module/accounts",
                    "help": "https://yardim.ofisim.com/firma-detaylari/",
                    "language": "tr",
                    "appId": 5
                },

                {
                    "route": "#/app/modules/contacts",
                    "help": "https://yardim.ofisim.com/kontak-kisi-yonetimi/",
                    "language": "tr",
                    "appId": 1
                },
                {
                    "route": "#/app/modules/contacts",
                    "help": "https://yardim.ofisim.com/kontak-kisi-yonetimi/",
                    "language": "tr",
                    "appId": 2
                },
                {
                    "route": "#/app/modules/contacts",
                    "help": "https://yardim.ofisim.com/kontak-kisi-yonetimi/",
                    "language": "tr",
                    "appId": 3
                },
                {
                    "route": "#/app/modules/contacts",
                    "help": "https://yardim.ofisim.com/kontak-kisi-yonetimi/",
                    "language": "tr",
                    "appId": 4
                },
                {
                    "route": "#/app/modules/contacts",
                    "help": "https://yardim.ofisim.com/kontak-kisi-yonetimi/",
                    "language": "tr",
                    "appId": 5
                },
                {

                    "route": "#/app/modules/opportunities",
                    "help": "https://yardim.ofisim.com/firsat-yonetimi/",
                    "language": "tr",
                    "appId": 1
                },
                {

                    "route": "#/app/modules/opportunities",
                    "help": "https://yardim.ofisim.com/firsat-yonetimi/",
                    "language": "tr",
                    "appId": 2
                },
                {

                    "route": "#/app/modules/opportunities",
                    "help": "https://yardim.ofisim.com/firsat-yonetimi/",
                    "language": "tr",
                    "appId": 3
                },
                {

                    "route": "#/app/modules/opportunities",
                    "help": "https://yardim.ofisim.com/firsat-yonetimi/",
                    "language": "tr",
                    "appId": 4
                },
                {

                    "route": "#/app/modules/opportunities",
                    "help": "https://yardim.ofisim.com/firsat-yonetimi/",
                    "language": "tr",
                    "appId": 5
                },
                {
                    "route": "#/app/modules/activities",
                    "help": "https://yardim.ofisim.com/aktivite-liste-gorunumu/",
                    "language": "tr",
                    "appId": 1
                },
                {
                    "route": "#/app/modules/activities",
                    "help": "https://yardim.ofisim.com/aktivite-liste-gorunumu/",
                    "language": "tr",
                    "appId": 2
                },
                {
                    "route": "#/app/modules/activities",
                    "help": "https://yardim.ofisim.com/aktivite-liste-gorunumu/",
                    "language": "tr",
                    "appId": 3
                },
                {
                    "route": "#/app/modules/activities",
                    "help": "https://yardim.ofisim.com/aktivite-liste-gorunumu/",
                    "language": "tr",
                    "appId": 4
                },
                {
                    "route": "#/app/modules/activities",
                    "help": "https://yardim.ofisim.com/aktivite-liste-gorunumu/",
                    "language": "tr",
                    "appId": 5
                },
                {
                    "route": "#/app/modules/products",
                    "help": "https://yardim.ofisim.com/urunler-modulu/",
                    "language": "tr",
                    "appId": 1
                },
                {
                    "route": "#/app/modules/products",
                    "help": "https://yardim.ofisim.com/urunler-modulu/",
                    "language": "tr",
                    "appId": 2
                },
                {
                    "route": "#/app/modules/products",
                    "help": "https://yardim.ofisim.com/urunler-modulu/",
                    "language": "tr",
                    "appId": 3
                },
                {
                    "route": "#/app/modules/products",
                    "help": "https://yardim.ofisim.com/urunler-modulu/",
                    "language": "tr",
                    "appId": 4
                },
                {
                    "route": "#/app/modules/products",
                    "help": "https://yardim.ofisim.com/urunler-modulu/",
                    "language": "tr",
                    "appId": 5
                },
                {
                    "route": "#/app/modules/quotes",
                    "help": "https://yardim.ofisim.com/teklif-yonetimi/",
                    "language": "tr",
                    "appId": 1
                },
                {
                    "route": "#/app/modules/quotes",
                    "help": "https://yardim.ofisim.com/teklif-yonetimi/",
                    "language": "tr",
                    "appId": 2
                },
                {
                    "route": "#/app/modules/quotes",
                    "help": "https://yardim.ofisim.com/teklif-yonetimi/",
                    "language": "tr",
                    "appId": 3
                },
                {
                    "route": "#/app/modules/quotes",
                    "help": "https://yardim.ofisim.com/teklif-yonetimi/",
                    "language": "tr",
                    "appId": 4
                },
                {
                    "route": "#/app/modules/quotes",
                    "help": "http://yardim.ofisim.com/teklif-hazirlama-ve-yonetimi/",
                    "language": "tr",
                    "appId": 5
                },
                {
                    "route": "#/app/modules/sales_orders",
                    "help": "https://yardim.ofisim.com/siparis-yonetimi/",
                    "language": "tr",
                    "appId": 1
                },
                {
                    "route": "#/app/modules/sales_orders",
                    "help": "https://yardim.ofisim.com/siparis-yonetimi/",
                    "language": "tr",
                    "appId": 2
                },
                {
                    "route": "#/app/modules/sales_orders",
                    "help": "https://yardim.ofisim.com/siparis-yonetimi/",
                    "language": "tr",
                    "appId": 3
                },
                {
                    "route": "#/app/modules/sales_orders",
                    "help": "https://yardim.ofisim.com/siparis-yonetimi/",
                    "language": "tr",
                    "appId": 4
                },
                {
                    "route": "#/app/modules/sales_orders",
                    "help": "http://yardim.ofisim.com/satis-detaylari-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 5
                },
                {
                    "route": "#/app/calendar",
                    "help": "https://yardim.ofisim.com/takvim-gorunumu/",
                    "language": "tr",
                    "appId": 1
                },
                {
                    "route": "#/app/calendar",
                    "help": "https://yardim.ofisim.com/takvim-gorunumu/",
                    "language": "tr",
                    "appId": 2
                },
                {
                    "route": "#/app/calendar",
                    "help": "http://yardim.ofisim.com/takvim-gorunumu-3/",
                    "language": "tr",
                    "appId": 3
                },
                {
                    "route": "#/app/calendar",
                    "help": "http://yardim.ofisim.com/takvim-gorunumu-2/",
                    "language": "tr",
                    "appId": 4
                },
                {
                    "route": "#/app/calendar",
                    "help": "http://yardim.ofisim.com/takvim-gorunumu-4/",
                    "language": "tr",
                    "appId": 5
                },
                {
                    "route": "#/app/setup/messaging",
                    "help": "https://yardim.ofisim.com/kisisel-ayarlar-2/",
                    "language": "tr",
                    "appId": 1
                },
                {
                    "route": "#/app/setup/messaging",
                    "help": "https://yardim.ofisim.com/kisisel-ayarlar-2/",
                    "language": "tr",
                    "appId": 2
                },
                {
                    "route": "#/app/setup/messaging",
                    "help": "https://yardim.ofisim.com/kisisel-ayarlar-4/",
                    "language": "tr",
                    "appId": 3
                },
                {
                    "route": "#/app/setup/messaging",
                    "help": "http://yardim.ofisim.com/kisisel-ayarlar/",
                    "language": "tr",
                    "appId": 4
                },
                {
                    "route": "#/app/setup/messaging",
                    "help": "https://yardim.ofisim.com/kisisel-ayarlar-3/",
                    "language": "tr",
                    "appId": 5
                },
                {
                    "route": "#/app/setup/users",
                    "help": "https://yardim.ofisim.com/kullanici-yonetimi-kullanicilar/",
                    "language": "tr",
                    "appId": 1
                },
                {
                    "route": "#/app/setup/users",
                    "help": "https://yardim.ofisim.com/kullanici-yonetimi-kullanicilar/",
                    "language": "tr",
                    "appId": 2
                },
                {
                    "route": "#/app/setup/users",
                    "help": "https://yardim.ofisim.com/kullanici-yonetimi-kullanicilar/",
                    "language": "tr",
                    "appId": 3
                },

                {
                    "route": "#/app/setup/users",
                    "help": "http://yardim.ofisim.com/kullanici-yonetimi-kullanicilar-2/",
                    "language": "tr",
                    "appId": 4
                },
                {
                    "route": "#/app/setup/users",
                    "help": "http://yardim.ofisim.com/kullanici-yonetimi-kullanicilar-3/",
                    "language": "tr",
                    "appId": 5
                },
                {
                    "route": "#/app/setup/profiles",
                    "help": "https://yardim.ofisim.com/kullanici-yonetimi-profiller/",
                    "language": "tr",
                    "appId": 1
                },
                {
                    "route": "#/app/setup/profiles",
                    "help": "https://yardim.ofisim.com/kullanici-yonetimi-profiller/",
                    "language": "tr",
                    "appId": 2
                },
                {
                    "route": "#/app/setup/profiles",
                    "help": "https://yardim.ofisim.com/kullanici-yonetimi-profiller/",
                    "language": "tr",
                    "appId": 3
                },
                {
                    "route": "#/app/setup/profiles",
                    "help": "http://yardim.ofisim.com/kullanici-yonetimi-profiller-2/",
                    "language": "tr",
                    "appId": 4
                },
                {
                    "route": "#/app/setup/profiles",
                    "help": "http://yardim.ofisim.com/kullanici-yonetimi-profiller-3/",
                    "language": "tr",
                    "appId": 5
                },
                {
                    "route": "#/app/setup/organization",
                    "help": "https://yardim.ofisim.com/firma-ayarlari/",
                    "language": "tr",
                    "appId": 1
                },
                {
                    "route": "#/app/setup/organization",
                    "help": "https://yardim.ofisim.com/firma-ayarlari/",
                    "language": "tr",
                    "appId": 2
                },
                {
                    "route": "#/app/setup/organization",
                    "help": "https://yardim.ofisim.com/firma-ayarlari-4/",
                    "language": "tr",
                    "appId": 3
                },
                {
                    "route": "#/app/setup/organization",
                    "help": "http://yardim.ofisim.com/kullanici-yonetimi-kullanicilar-2/",
                    "language": "tr",
                    "appId": 4
                },
                {
                    "route": "#/app/setup/organization",
                    "help": "https://yardim.ofisim.com/firma-ayarlari-3/",
                    "language": "tr",
                    "appId": 5
                },
                {
                    "route": "#/app/setup/license",
                    "help": "https://yardim.ofisim.com/lisans-ayarlari/",
                    "language": "tr",
                    "appId": 1
                },
                {
                    "route": "#/app/setup/license",
                    "help": "https://yardim.ofisim.com/lisans-ayarlari/",
                    "language": "tr",
                    "appId": 2
                },
                {
                    "route": "#/app/setup/license",
                    "help": "https://yardim.ofisim.com/lisans-ayarlari/",
                    "language": "tr",
                    "appId": 3
                },
                {
                    "route": "#/app/setup/license",
                    "help": "https://yardim.ofisim.com/lisans-ayarlari/",
                    "language": "tr",
                    "appId": 4
                },
                {
                    "route": "#/app/setup/license",
                    "help": "https://yardim.ofisim.com/lisans-ayarlari/",
                    "language": "tr",
                    "appId": 5
                },
                {
                    "route": "#/app/modules/",
                    "help": "https://yardim.ofisim.com/category/genel/",
                    "language": "tr",
                    "appId": 1
                },
                {
                    "route": "#/app/modules/",
                    "help": "https://yardim.ofisim.com/category/genel/",
                    "language": "tr",
                    "appId": 2
                },
                {
                    "route": "#/app/modules/",
                    "help": "https://yardim.ofisim.com/category/asistan/",
                    "language": "tr",
                    "appId": 3
                },
                {
                    "route": "#/app/modules/",
                    "help": "https://yardim.ofisim.com/category/ik/",
                    "language": "tr",
                    "appId": 4
                },
                {
                    "route": "#/app/modules/",
                    "help": "https://yardim.ofisim.com/category/cagri/",
                    "language": "tr",
                    "appId": 5
                },
                {
                    "route": "#/app/module/",
                    "help": "https://yardim.ofisim.com/category/genel/",
                    "language": "tr",
                    "appId": 1
                },
                {
                    "route": "#/app/module/",
                    "help": "https://yardim.ofisim.com/category/genel/",
                    "language": "tr",
                    "appId": 2
                },
                {
                    "route": "#/app/module/",
                    "help": "https://yardim.ofisim.com/category/asistan/",
                    "language": "tr",
                    "appId": 3
                },
                {
                    "route": "#/app/module/",
                    "help": "https://yardim.ofisim.com/category/ik/",
                    "language": "tr",
                    "appId": 4
                },
                {
                    "route": "#/app/module/",
                    "help": "https://yardim.ofisim.com/category/cagri/",
                    "language": "tr",
                    "appId": 5
                },
                {
                    "route": "#/app/moduleForm/",
                    "help": "https://yardim.ofisim.com/category/genel/",
                    "language": "tr",
                    "appId": 1
                },
                {
                    "route": "#/app/moduleForm/",
                    "help": "https://yardim.ofisim.com/category/genel/",
                    "language": "tr",
                    "appId": 2
                },
                {
                    "route": "#/app/moduleForm/",
                    "help": "https://yardim.ofisim.com/category/asistan/",
                    "language": "tr",
                    "appId": 3
                },
                {
                    "route": "#/app/moduleForm/",
                    "help": "https://yardim.ofisim.com/category/ik/",
                    "language": "tr",
                    "appId": 4
                },
                {
                    "route": "#/app/moduleForm/",
                    "help": "https://yardim.ofisim.com/category/cagri/",
                    "language": "tr",
                    "appId": 5
                },
                {
                    "route": "#/app/modules/leads",
                    "help": "http://yardim.ofisim.com/musteri-adaylari/",
                    "language": "tr",
                    "appId": 1
                },
                {
                    "route": "#/app/modules/leads",
                    "help": "http://yardim.ofisim.com/musteri-adaylari/",
                    "language": "tr",
                    "appId": 2
                },
                {
                    "route": "#/app/modules/leads",
                    "help": "http://yardim.ofisim.com/musteri-adaylari/",
                    "language": "tr",
                    "appId": 3
                },
                {
                    "route": "#/app/modules/leads",
                    "help": "http://yardim.ofisim.com/musteri-adaylari/",
                    "language": "tr",
                    "appId": 4
                },
                {
                    "route": "#/app/modules/leads",
                    "help": "http://yardim.ofisim.com/musteri-adaylari/",
                    "language": "tr",
                    "appId": 5
                },
                {
                    "route": "#/app/modules/accounts",
                    "help": "http://yardim.ofisim.com/firma-liste-gorunumu/",
                    "language": "tr",
                    "appId": 1
                },
                {
                    "route": "#/app/modules/accounts",
                    "help": "http://yardim.ofisim.com/firma-liste-gorunumu/",
                    "language": "tr",
                    "appId": 2
                },
                {
                    "route": "#/app/modules/accounts",
                    "help": "http://yardim.ofisim.com/firma-detaylari-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 3
                },
                {
                    "route": "#/app/modules/accounts",
                    "help": "http://yardim.ofisim.com/firma-liste-gorunumu/",
                    "language": "tr",
                    "appId": 4
                },
                {
                    "route": "#/app/modules/accounts",
                    "help": "http://yardim.ofisim.com/firma-liste-gorunumu/",
                    "language": "tr",
                    "appId": 5
                },
                {
                    "route": "#/app/modules/izinler",
                    "help": "http://yardim.ofisim.com/izin-detaylari-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 1
                },
                {
                    "route": "#/app/modules/izinler",
                    "help": "http://yardim.ofisim.com/izin-detaylari-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 2
                },
                {
                    "route": "#/app/modules/izinler",
                    "help": "http://yardim.ofisim.com/izin-detaylari-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 3
                },
                {
                    "route": "#/app/modules/izinler",
                    "help": "http://yardim.ofisim.com/izin-detaylari-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 4
                },
                {
                    "route": "#/app/modules/izinler",
                    "help": "http://yardim.ofisim.com/izin-detaylari-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 5
                },
                {
                    "route": "#/app/modules/harcamalar",
                    "help": "https://yardim.ofisim.com/harcama-detaylari-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 1
                },
                {
                    "route": "#/app/modules/harcamalar",
                    "help": "https://yardim.ofisim.com/harcama-detaylari-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 2
                },
                {
                    "route": "#/app/modules/harcamalar",
                    "help": "https://yardim.ofisim.com/harcama-detaylari-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 3
                },
                {
                    "route": "#/app/modules/harcamalar",
                    "help": "https://yardim.ofisim.com/harcama-detaylari-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 4
                },
                {
                    "route": "#/app/modules/harcamalar",
                    "help": "https://yardim.ofisim.com/harcama-detaylari-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 5
                },
                {
                    "route": "#/app/modules/zimmetler",
                    "help": "http://yardim.ofisim.com/zimmet-detaylari-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 1
                },
                {
                    "route": "#/app/modules/zimmetler",
                    "help": "http://yardim.ofisim.com/zimmet-detaylari-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 2
                },
                {
                    "route": "#/app/modules/zimmetler",
                    "help": "http://yardim.ofisim.com/zimmet-detaylari-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 3
                },
                {
                    "route": "#/app/modules/zimmetler",
                    "help": "http://yardim.ofisim.com/zimmet-detaylari-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 4
                },
                {
                    "route": "#/app/modules/zimmetler",
                    "help": "http://yardim.ofisim.com/zimmet-detaylari-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 5
                },
                {
                    "route": "#/app/modules/talepler",
                    "help": "http://yardim.ofisim.com/talep-detaylari-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 1
                },
                {
                    "route": "#/app/modules/talepler",
                    "help": "http://yardim.ofisim.com/talep-detaylari-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 2
                },
                {
                    "route": "#/app/modules/talepler",
                    "help": "http://yardim.ofisim.com/talep-detaylari-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 3
                },
                {
                    "route": "#/app/modules/talepler",
                    "help": "http://yardim.ofisim.com/talep-detaylari-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 4
                },
                {
                    "route": "#/app/modules/talepler",
                    "help": "http://yardim.ofisim.com/talep-detaylari-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 5
                },
                {
                    "route": "#/app/modules/odemeler",
                    "help": "http://yardim.ofisim.com/odemeler-detay-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 1
                },
                {
                    "route": "#/app/modules/odemeler",
                    "help": "http://yardim.ofisim.com/odemeler-detay-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 2
                },
                {
                    "route": "#/app/modules/odemeler",
                    "help": "http://yardim.ofisim.com/odemeler-detay-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 3
                },
                {
                    "route": "#/app/modules/odemeler",
                    "help": "http://yardim.ofisim.com/odemeler-detay-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 4
                },
                {
                    "route": "#/app/modules/odemeler",
                    "help": "http://yardim.ofisim.com/odemeler-detay-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 5
                },
                {
                    "route": "#/app/modules/egitimler",
                    "help": "http://yardim.ofisim.com/egitim-detay-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 1
                },
                {
                    "route": "#/app/modules/egitimler",
                    "help": "http://yardim.ofisim.com/egitim-detay-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 2
                },
                {
                    "route": "#/app/modules/egitimler",
                    "help": "http://yardim.ofisim.com/egitim-detay-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 3
                },
                {
                    "route": "#/app/modules/egitimler",
                    "help": "http://yardim.ofisim.com/egitim-detay-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 4
                },
                {
                    "route": "#/app/modules/egitimler",
                    "help": "http://yardim.ofisim.com/egitim-detay-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 5
                },
                {
                    "route": "#/app/modules/vize_islemleri",
                    "help": "http://yardim.ofisim.com/vize-detay-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 1
                },
                {
                    "route": "#/app/modules/vize_islemleri",
                    "help": "http://yardim.ofisim.com/vize-detay-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 2
                },
                {
                    "route": "#/app/modules/vize_islemleri",
                    "help": "http://yardim.ofisim.com/vize-detay-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 3
                },
                {
                    "route": "#/app/modules/vize_islemleri",
                    "help": "http://yardim.ofisim.com/vize-detay-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 4
                },
                {
                    "route": "#/app/modules/vize_islemleri",
                    "help": "http://yardim.ofisim.com/vize-detay-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 5
                },
                {
                    "route": "#/app/module/activities",
                    "help": "http://yardim.ofisim.com/etkinlikler-ve-gorev-olusturma/",
                    "language": "tr",
                    "appId": 1
                },
                {
                    "route": "#/app/module/activities",
                    "help": "http://yardim.ofisim.com/etkinlikler-ve-gorev-olusturma/",
                    "language": "tr",
                    "appId": 2
                },
                {
                    "route": "#/app/module/activities",
                    "help": "http://yardim.ofisim.com/etkinlikler-ve-gorev-olusturma/",
                    "language": "tr",
                    "appId": 3
                },
                {
                    "route": "#/app/module/activities",
                    "help": "http://yardim.ofisim.com/etkinlikler-ve-gorev-olusturma/",
                    "language": "tr",
                    "appId": 4
                },
                {
                    "route": "#/app/module/activities",
                    "help": "http://yardim.ofisim.com/etkinlikler-ve-gorev-olusturma/",
                    "language": "tr",
                    "appId": 5
                },
                {
                    "route": "#/app/module/seyahatler",
                    "help": "http://yardim.ofisim.com/seyahat-detaylari/",
                    "language": "tr",
                    "appId": 1
                },
                {
                    "route": "#/app/module/seyahatler",
                    "help": "http://yardim.ofisim.com/seyahat-detaylari/",
                    "language": "tr",
                    "appId": 2
                },
                {
                    "route": "#/app/module/seyahatler",
                    "help": "http://yardim.ofisim.com/seyahat-detaylari/",
                    "language": "tr",
                    "appId": 3
                },
                {
                    "route": "#/app/module/seyahatler",
                    "help": "http://yardim.ofisim.com/seyahat-detaylari/",
                    "language": "tr",
                    "appId": 4
                },
                {
                    "route": "#/app/module/seyahatler",
                    "help": "http://yardim.ofisim.com/seyahat-detaylari/",
                    "language": "tr",
                    "appId": 5
                },
                {
                    "route": "#/app/modules/personel",
                    "help": "http://yardim.ofisim.com/personel-detaylari-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 1
                },
                {
                    "route": "#/app/modules/personel",
                    "help": "http://yardim.ofisim.com/personel-detaylari-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 2
                },
                {
                    "route": "#/app/modules/personel",
                    "help": "http://yardim.ofisim.com/personel-detaylari-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 3
                },
                {
                    "route": "#/app/modules/personel",
                    "help": "http://yardim.ofisim.com/personel-detaylari-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 4
                },
                {
                    "route": "#/app/modules/personel",
                    "help": "http://yardim.ofisim.com/personel-detaylari-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 5
                },
                {
                    "route": "#/app/modules/aramalar",
                    "help": "http://yardim.ofisim.com/arama-detaylari-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 1
                },
                {
                    "route": "#/app/modules/aramalar",
                    "help": "http://yardim.ofisim.com/arama-detaylari-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 2
                },
                {
                    "route": "#/app/modules/aramalar",
                    "help": "http://yardim.ofisim.com/arama-detaylari-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 3
                },
                {
                    "route": "#/app/modules/aramalar",
                    "help": "http://yardim.ofisim.com/arama-detaylari-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 4
                },
                {
                    "route": "#/app/modules/aramalar",
                    "help": "http://yardim.ofisim.com/arama-detaylari-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 5
                },
                {
                    "route": "#/app/modules/arama_detaylari",
                    "help": "http://yardim.ofisim.com/arama-detaylari-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 1
                },
                {
                    "route": "#/app/modules/arama_detaylari",
                    "help": "http://yardim.ofisim.com/arama-detaylari-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 2
                },
                {
                    "route": "#/app/modules/arama_detaylari",
                    "help": "http://yardim.ofisim.com/arama-detaylari-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 3
                },
                {
                    "route": "#/app/modules/arama_detaylari",
                    "help": "http://yardim.ofisim.com/arama-detaylari-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 4
                },
                {
                    "route": "#/app/modules/arama_detaylari",
                    "help": "http://yardim.ofisim.com/arama-detaylari-ve-liste-gorunumu/",
                    "language": "tr",
                    "appId": 5
                },

                {
                    "route": "#/app/setup/templates",
                    "help": "http://yardim.ofisim.com/yeni-bir-teklif-olusturma/",
                    "language": "tr",
                    "appId": 1
                },
                {
                    "route": "#/app/setup/templates",
                    "help": "http://yardim.ofisim.com/yeni-bir-teklif-olusturma/",
                    "language": "tr",
                    "appId": 2
                },
                {
                    "route": "#/app/setup/templates",
                    "help": "http://yardim.ofisim.com/yeni-bir-teklif-olusturma/",
                    "language": "tr",
                    "appId": 3
                },
                {
                    "route": "#/app/setup/templates",
                    "help": "http://yardim.ofisim.com/yeni-bir-teklif-olusturma/",
                    "language": "tr",
                    "appId": 4
                },
                {
                    "route": "#/app/setup/templates",
                    "help": "http://yardim.ofisim.com/yeni-bir-teklif-olusturma/",
                    "language": "tr",
                    "appId": 5
                },
                {
                    "route": "#/app/setup/templateguide",
                    "help": "http://yardim.ofisim.com/teklif-sablonu-hazirlama/",
                    "language": "tr",
                    "appId": 1
                },
                {
                    "route": "#/app/setup/templateguide",
                    "help": "http://yardim.ofisim.com/teklif-sablonu-hazirlama/",
                    "language": "tr",
                    "appId": 2
                },
                {
                    "route": "#/app/setup/templateguide",
                    "help": "http://yardim.ofisim.com/teklif-sablonu-hazirlama/",
                    "language": "tr",
                    "appId": 3
                },
                {
                    "route": "#/app/setup/templateguide",
                    "help": "http://yardim.ofisim.com/teklif-sablonu-hazirlama/",
                    "language": "tr",
                    "appId": 4
                },
                {
                    "route": "#/app/setup/templateguide",
                    "help": "http://yardim.ofisim.com/teklif-sablonu-hazirlama/",
                    "language": "tr",
                    "appId": 5
                },

                {
                    "route": "#/app/modules/activities",
                    "help": "http://yardim.ofisim.com/aktivite-liste-gorunumu/",
                    "language": "tr",
                    "appId": 1
                },
                {
                    "route": "#/app/modules/activities",
                    "help": "http://yardim.ofisim.com/aktivite-liste-gorunumu/",
                    "language": "tr",
                    "appId": 2
                },
                {
                    "route": "#/app/modules/activities",
                    "help": "http://yardim.ofisim.com/aktivite-liste-gorunumu/",
                    "language": "tr",
                    "appId": 3
                },
                {
                    "route": "#/app/modules/activities",
                    "help": "http://yardim.ofisim.com/aktivite-liste-gorunumu/",
                    "language": "tr",
                    "appId": 4
                },
                {
                    "route": "#/app/modules/activities",
                    "help": "http://yardim.ofisim.com/aktivite-liste-gorunumu/",
                    "language": "tr",
                    "appId": 5
                },
            ]
        });
