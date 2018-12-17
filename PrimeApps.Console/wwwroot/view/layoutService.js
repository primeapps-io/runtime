'use strict';

angular.module('primeapps')

    .factory('LayoutService', ['$rootScope', '$http', '$localStorage', '$cache', '$q', '$filter', '$timeout', '$state', 'config', 'helper', 'sipHelper', 'entityTypes', 'taskDate', 'dataTypes', 'activityTypes', 'operators', 'systemRequiredFields', 'systemReadonlyFields', '$window', '$modal', '$sce',
        function ($rootScope, $http, $localStorage, $cache, $q, $filter, $timeout, $state, config, helper, sipHelper, entityTypes, taskDate, dataTypes, activityTypes, operators, systemRequiredFields, systemReadonlyFields, $window, $modal, $sce) {
            return {

                getOrg: function (refresh) {
                    helper.hideLoader();
                },
                getProfileModule: function () {
                    var profileModule = {};
                    profileModule.id = 1000;
                    profileModule.name = 'profiles';
                    profileModule.system_type = 'system';
                    profileModule.order = 999;
                    profileModule.display = false;
                    profileModule.label_en_singular = 'Profile';
                    profileModule.label_en_plural = 'Profiles';
                    profileModule.label_tr_singular = 'Profil';
                    profileModule.label_tr_plural = 'Profiller';
                    profileModule.menu_icon = 'fa fa-users';
                    profileModule.sections = [];
                    profileModule.fields = [];

                    var section = {};
                    section.name = 'profile_information';
                    section.system_type = 'system';
                    section.order = 1;
                    section.column_count = 1;
                    section.label_en = 'Profile Information';
                    section.label_tr = 'Profil Bilgisi';
                    section.display_form = true;
                    section.display_detail = true;

                    var fieldEmail = {};
                    fieldEmail.name = 'name';
                    fieldEmail.system_type = 'system';
                    fieldEmail.data_type = 'text_single';
                    fieldEmail.order = 2;
                    fieldEmail.section = 1;
                    fieldEmail.section_column = 1;
                    fieldEmail.primary = true;
                    fieldEmail.inline_edit = false;
                    fieldEmail.label_en = 'Name';
                    fieldEmail.label_tr = 'İsim';
                    fieldEmail.display_list = true;
                    fieldEmail.display_form = true;
                    fieldEmail.display_detail = true;
                    profileModule.fields.push(fieldEmail);

                    return profileModule;
                },
                getRoleModule: function () {
                    var profileModule = {};
                    profileModule.id = 1001;
                    profileModule.name = 'roles';
                    profileModule.system_type = 'system';
                    profileModule.order = 999;
                    profileModule.display = false;
                    profileModule.label_en_singular = 'Role';
                    profileModule.label_en_plural = 'Roles';
                    profileModule.label_tr_singular = 'Rol';
                    profileModule.label_tr_plural = 'Roller';
                    profileModule.menu_icon = 'fa fa-users';
                    profileModule.sections = [];
                    profileModule.fields = [];

                    var section = {};
                    section.name = 'role_information';
                    section.system_type = 'system';
                    section.order = 1;
                    section.column_count = 1;
                    section.label_en = 'Role Information';
                    section.label_tr = 'Rol Bilgisi';
                    section.display_form = true;
                    section.display_detail = true;

                    var fieldLabelEn = {};
                    fieldLabelEn.name = 'label_en';
                    fieldLabelEn.system_type = 'system';
                    fieldLabelEn.data_type = 'text_single';
                    fieldLabelEn.order = 2;
                    fieldLabelEn.section = 1;
                    fieldLabelEn.section_column = 1;
                    fieldLabelEn.primary = $rootScope.user.tenant_language == "en";
                    fieldLabelEn.inline_edit = false;
                    fieldLabelEn.label_en = 'Name English';
                    fieldLabelEn.label_tr = 'İsim İngilizce';
                    fieldLabelEn.display_list = true;
                    fieldLabelEn.display_form = true;
                    fieldLabelEn.display_detail = true;
                    profileModule.fields.push(fieldLabelEn);

                    var fieldLabelTr = {};
                    fieldLabelTr.name = 'label_tr';
                    fieldLabelTr.system_type = 'system';
                    fieldLabelTr.data_type = 'text_single';
                    fieldLabelTr.order = 2;
                    fieldLabelTr.section = 1;
                    fieldLabelTr.section_column = 1;
                    fieldLabelTr.primary = $rootScope.user.tenant_language == "tr";
                    fieldLabelTr.inline_edit = false;
                    fieldLabelTr.label_en = 'Name Turkish';
                    fieldLabelTr.label_tr = 'İsim Türkçe';
                    fieldLabelTr.display_list = true;
                    fieldLabelTr.display_form = true;
                    fieldLabelTr.display_detail = true;
                    profileModule.fields.push(fieldLabelTr);

                    return profileModule;
                },
                getUserModule: function () {
                    var userModule = {};
                    userModule.id = 999;
                    userModule.name = 'users';
                    userModule.system_type = 'system';
                    userModule.order = 999;
                    userModule.display = false;
                    userModule.label_en_singular = 'User';
                    userModule.label_en_plural = 'Users';
                    userModule.label_tr_singular = 'Kullanıcı';
                    userModule.label_tr_plural = 'Kullanıcılar';
                    userModule.menu_icon = 'fa fa-users';
                    userModule.sections = [];
                    userModule.fields = [];

                    var section = {};
                    section.name = 'user_information';
                    section.system_type = 'system';
                    section.order = 1;
                    section.column_count = 1;
                    section.label_en = 'User Information';
                    section.label_tr = 'Kullanıcı Bilgisi';
                    section.display_form = true;
                    section.display_detail = true;

                    var fieldEmail = {};
                    fieldEmail.name = 'email';
                    fieldEmail.system_type = 'system';
                    fieldEmail.data_type = 'email';
                    fieldEmail.order = 2;
                    fieldEmail.section = 1;
                    fieldEmail.section_column = 1;
                    fieldEmail.primary = false;
                    fieldEmail.inline_edit = true;
                    fieldEmail.label_en = 'Email';
                    fieldEmail.label_tr = 'Eposta';
                    fieldEmail.display_list = true;
                    fieldEmail.display_form = true;
                    fieldEmail.display_detail = true;
                    userModule.fields.push(fieldEmail);

                    var fieldFirstName = {};
                    fieldFirstName.name = 'first_name';
                    fieldFirstName.system_type = 'system';
                    fieldFirstName.data_type = 'text_single';
                    fieldFirstName.order = 3;
                    fieldFirstName.section = 1;
                    fieldFirstName.section_column = 1;
                    fieldFirstName.primary = false;
                    fieldFirstName.inline_edit = true;
                    fieldFirstName.editable = true;
                    fieldFirstName.show_label = true;
                    fieldFirstName.label_en = 'First Name';
                    fieldFirstName.label_tr = 'Adı';
                    fieldFirstName.display_list = true;
                    fieldFirstName.display_form = true;
                    fieldFirstName.display_detail = true;
                    userModule.fields.push(fieldFirstName);

                    var fieldLastName = {};
                    fieldLastName.name = 'last_name';
                    fieldLastName.system_type = 'system';
                    fieldLastName.data_type = 'text_single';
                    fieldLastName.order = 4;
                    fieldLastName.section = 1;
                    fieldLastName.section_column = 1;
                    fieldLastName.primary = false;
                    fieldLastName.inline_edit = true;
                    fieldLastName.editable = true;
                    fieldLastName.show_label = true;
                    fieldLastName.label_en = 'Last Name';
                    fieldLastName.label_tr = 'Soyadı';
                    fieldLastName.display_list = true;
                    fieldLastName.display_form = true;
                    fieldLastName.display_detail = true;
                    userModule.fields.push(fieldLastName);

                    var fieldFullName = {};
                    fieldFullName.name = 'full_name';
                    fieldFullName.system_type = 'system';
                    fieldFullName.data_type = 'text_single';
                    fieldFullName.order = 5;
                    fieldFullName.section = 1;
                    fieldFullName.section_column = 1;
                    fieldFullName.primary = true;
                    fieldFullName.inline_edit = true;
                    fieldFullName.editable = true;
                    fieldFullName.show_label = true;
                    fieldFullName.label_en = 'Name';
                    fieldFullName.label_tr = 'Adı Soyadı';
                    fieldFullName.display_list = true;
                    fieldFullName.display_form = true;
                    fieldFullName.display_detail = true;
                    fieldFullName.combination = {};
                    fieldFullName.combination.field_1 = 'first_name';
                    fieldFullName.combination.field_2 = 'last_name';
                    userModule.fields.push(fieldFullName);

                    var fieldPhone = {};
                    fieldPhone.name = 'phone';
                    fieldPhone.system_type = 'system';
                    fieldPhone.data_type = 'text_single';
                    fieldPhone.order = 6;
                    fieldPhone.section = 1;
                    fieldPhone.section_column = 1;
                    fieldPhone.primary = false;
                    fieldPhone.inline_edit = true;
                    fieldPhone.label_en = 'Phone';
                    fieldPhone.label_tr = 'Telefon';
                    fieldPhone.display_list = true;
                    fieldPhone.display_form = true;
                    fieldPhone.display_detail = true;
                    userModule.fields.push(fieldPhone);

                    return userModule;
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

                            var sectionPermission = $filter('filter')(sectionPermissions, { profile_id: profile.id }, true)[0];

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
                        field.sectionObj = $filter('filter')(module.sections, { name: field.section }, true)[0];

                        if (field.data_type === 'lookup') {
                            if (field.lookup_type != 'users' && field.lookup_type != 'profiles' && field.lookup_type != 'roles' && field.lookup_type != 'relation') {
                                var lookupModule = $filter('filter')($rootScope.modules, { name: field.lookup_type }, true)[0];

                                if (!lookupModule)
                                    continue;

                                field.lookupModulePrimaryField = $filter('filter')(lookupModule.fields, { primary_lookup: true }, true)[0];

                                if (!field.lookupModulePrimaryField)
                                    field.lookupModulePrimaryField = $filter('filter')(lookupModule.fields, { primary: true }, true)[0];

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
                                    var lookupModule = $filter('filter')($rootScope.modules, { name: 'users' }, true)[0];
                                    field.lookupModulePrimaryField = $filter('filter')(lookupModule.fields, { primary: true }, true)[0];
                                }
                                else if (field.lookup_type === 'profiles') {
                                    var lookupModule = $filter('filter')($rootScope.modules, { name: 'profiles' }, true)[0];
                                    field.lookupModulePrimaryField = $filter('filter')(lookupModule.fields, { primary: true }, true)[0];
                                }
                                else if (field.lookup_type === 'roles') {
                                    var lookupModule = $filter('filter')($rootScope.modules, { name: 'roles' }, true)[0];
                                    field.lookupModulePrimaryField = $filter('filter')(lookupModule.fields, { primary: true }, true)[0];
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

                            var fieldPermission = $filter('filter')(fieldPermissions, { profile_id: profileItem.id }, true)[0];

                            if (!fieldPermission)
                                field.permissions.push({ profile_id: profileItem.id, profile_name: profileItem.name, profile_is_admin: profileItem.has_admin_rights, type: 'full' });
                            else
                                field.permissions.push({ id: fieldPermission.id, profile_id: profileItem.id, profile_name: profileItem.name, profile_is_admin: profileItem.has_admin_rights, type: fieldPermission.type });
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

                getCurrencySymbol: function (currency) {
                    if (!currency)
                        return;

                    switch (currency) {
                        case 'TRY':
                            return '\u20ba';
                            break;
                        case 'USD':
                            return '$';
                            break
                    }
                },

                removeSampleData: function () {
                    return $http.delete(config.apiUrl + 'data/remove_sample_data');
                },

                setModuleMenuIcon: function (module) {
                    switch (module.name) {
                        case 'products':
                            module.menu_icon = 'fa fa-shopping-cart';
                            break;
                        case 'leads':
                            module.menu_icon = 'fa fa-coffee';
                            break;
                        case 'sales_orders':
                            module.menu_icon = 'fa fa-credit-card';
                            break;
                        case 'accounts':
                            module.menu_icon = 'fa fa-cubes';
                            break;
                        case 'contacts':
                            module.menu_icon = 'fa fa-users';
                            break;
                        case 'quotes':
                            module.menu_icon = 'fa fa-file-pdf-o';
                            break;
                        case 'opportunities':
                            module.menu_icon = 'fa fa-star-half-empty';
                            break;
                        case 'activities':
                            module.menu_icon = 'fa fa-paper-plane';
                            break;
                        case 'current_accounts':
                            module.menu_icon = 'fa fa-calculator';
                            break;
                        case 'suppliers':
                            module.menu_icon = 'fa fa-truck';
                            break;
                        default:
                            module.menu_icon = 'fa fa-square';
                            break;
                    }
                },

                setCustomActivityTypes: function (activityTypes) {
                    var activityTypesCustom = $filter('filter')($rootScope.moduleSettings, { key: 'custom_activity_types' }, true)[0];

                    if (activityTypesCustom) {
                        for (var j = 0; j < activityTypes.length; j++) {
                            var activityTypeItem = activityTypes[j];
                            activityTypeItem.hidden = true;
                        }

                        var activityTypesParts = activityTypesCustom.value.split('|');

                        for (var k = 0; k < activityTypesParts.length; k++) {
                            var activityType = activityTypesParts[k];
                            var activityTypeParts = activityType.split(':');
                            var activityTypeCode = activityTypeParts[0];
                            var activityTypeLabel = activityTypeParts[1];

                            var activityTypeCurrent = $filter('filter')(activityTypes, { system_code: activityTypeCode }, true)[0];

                            if (activityTypeCurrent) {
                                activityTypeCurrent.hidden = false;
                                activityTypeCurrent.label[$rootScope.user.tenant_language] = activityTypeLabel;
                            }
                        }
                    }
                },

                changeTenant: function (userId, tenantId, appId, email) {
                    return $http.get(config.apiUrl + 'account/change_tenant?userId=' + userId + '&tenantId=' + parseInt(tenantId) + '&appId=' + parseInt(appId) + '&email=' + email);
                },

                addApp: function (appId) {
                    return $http.get(config.apiUrl + 'platform/office_app_create?appId=' + appId);
                },

                getHelp: function () {
                    return $http.get(config.apiUrl + 'help/get_all');
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
