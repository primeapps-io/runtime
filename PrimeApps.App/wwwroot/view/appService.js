'use strict';

angular.module('primeapps')

    .factory('AppService', ['$rootScope', '$http', '$localStorage', '$cache', '$q', '$filter', '$timeout', '$state', 'config', 'helper', 'dataTypes', 'operators', 'systemRequiredFields', 'systemReadonlyFields', '$sce', 'AuthService', '$cookies', 'blockUI',
        function ($rootScope, $http, $localStorage, $cache, $q, $filter, $timeout, $state, config, helper, dataTypes, operators, systemRequiredFields, systemReadonlyFields, $sce, AuthService, $cookies, blockUI) {
            return {

                getMyAccount: function (refresh) {
                    if (!refresh && $rootScope.config && $rootScope.user && $rootScope.workgroups && $rootScope.modules && $rootScope.users)
                        return null;
                    $rootScope.menu = [];
                    var that = this;
                    var deferred = $q.defer();

                    var clearAuthCache = function () {
                        $localStorage.remove('access_token');
                        $localStorage.remove('refresh_token');
                        $localStorage.remove('Workgroup');
                    };

                    $http.post(config.apiUrl + 'User/MyAccount', {})
                        .then(function (responseAccount) {
                            if (responseAccount.status !== 200 || !responseAccount.data) {
                                clearAuthCache();
                                if (responseAccount.data === null) {
                                    deferred.resolve(401);
                                } else {
                                    deferred.resolve(false);
                                }

                                AuthService.logout()
                                    .then(function (response) {
                                        AuthService.logoutComplete();

                                        if (preview) {
                                            $cookies.remove('preview_app_id');
                                            $cookies.remove('preview_tenant_id');
                                        } else
                                            $cookies.remove('tenant_id');

                                        window.location = response.data['redirect_url'];
                                        blockUI.stop();
                                    });


                                return deferred.promise;
                            }

                            var promises = [];

                            promises.push($http.get(config.apiUrl + 'messaging/get_config'));
                            promises.push($http.get(config.apiUrl + 'User/get_all'));
                            promises.push($http.get(config.apiUrl + 'Profile/GetAllBasic'));
                            promises.push($http.get(config.apiUrl + 'module/get_module_settings'));
                            promises.push($http.get(config.apiUrl + 'phone/get_config'));
                            promises.push($http.get(config.apiUrl + 'module_profile_settings/get_all'));
                            promises.push($http.get(config.apiUrl + 'help/get_all'));
                            promises.push($http.get(config.apiUrl + 'help/get_first_screen?helpType=' + 'modal' + '&firstscreen=' + true));
                            promises.push($http.get(config.apiUrl + 'menu/get/' + profileConfigs.menu));
                            promises.push($http.get(config.apiUrl + 'settings/get_all/custom?userId=' + responseAccount.data.user.id));
                            promises.push($http.get(config.apiUrl + 'settings/get_all/1'));
                            promises.push($http.get(config.apiUrl + 'settings/get_by_key/1/custom_profile_permissions'));
                            promises.push($http.get(config.apiUrl + 'module/get_all'));
                            promises.push($http.get(config.apiUrl + 'module/get_default_system_fields?label=' + $rootScope.globalization.Label));
                            promises.push($http.get(config.apiUrl + 'picklist/get_yes_no'));
                            promises.push($http.get(config.apiUrl + 'droplist/get_all_with_items'));

                            $q.all(promises)
                                .then(function (response) {
                                    if (response.length < 6
                                        || response[0].status !== 200 || response[1].status !== 200 || response[2].status !== 200 || response[3].status !== 200 || response[4].status !== 200 ||
                                        (response[12].status !== 200 || !response[12].data) || !response[0].data || !response[1].data || !response[2].data || !response[3].data) {
                                        clearAuthCache();
                                        deferred.resolve(false);
                                        return deferred.promise;
                                    }

                                    var isDemo = responseAccount.data.user.isDemo || false;
                                    var myAccount = responseAccount.data;


                                    if (response[12] && response[12].data)
                                        account.modules = response[12].data;

                                    var modules = !isDemo ? account.modules : $filter('filter')(account.modules, function (value) {
                                        return value.created_by_id === myAccount.user.id || value.system_type === 'system';
                                    }, true);

                                    var messaging = response[0].data;

                                    var users = !isDemo ? response[1].data : $filter('filter')(response[1].data, function (value) {
                                        return value.id === myAccount.user.id;
                                    }, true);

                                    var profiles = !isDemo ? response[2].data : $filter('filter')(response[2].data, function (value) {
                                        return value.created_by_id === myAccount.user.id || value.is_persistent === true;
                                    }, true);

                                    var moduleSettings = response[3].data;
                                    //TODO REMOVE
                                    var phoneSettings = response[4].data;

                                    var userSettings = response[9].data;
                                    $rootScope.defaultSystemFields = response[13].data;
                                    $rootScope.yesNo = response[14].data;

                                    if (myAccount.instances.length < 1) {
                                        $state.go('join');
                                        return;
                                    }

                                    $rootScope.modulus = [];

                                    //#697 Remove lookup fields ( When lookup module deleted ).
                                    var activeModuleNames = modules.map(function (a) {
                                        return a.name;
                                    });

                                    for (var moduleKey = modules.length - 1; moduleKey >= 0; moduleKey--) {

                                        $rootScope.modulus[modules[moduleKey].name] = modules[moduleKey];

                                        for (var fieldKey = modules[moduleKey].fields.length - 1; fieldKey >= 0; fieldKey--) {
                                            if (modules[moduleKey].fields[fieldKey].data_type === "lookup"
                                                && modules[moduleKey].fields[fieldKey].lookup_type !== "users"
                                                && modules[moduleKey].fields[fieldKey].lookup_type !== "profiles"
                                                && modules[moduleKey].fields[fieldKey].lookup_type !== "roles"
                                                && modules[moduleKey].fields[fieldKey].lookup_type !== "relation"
                                                && activeModuleNames.indexOf(modules[moduleKey].fields[fieldKey].lookup_type) === -1
                                            )
                                                modules[moduleKey].fields.splice(fieldKey, 1);

                                            if (modules[moduleKey].fields[fieldKey] && modules[moduleKey].fields[fieldKey].primary) {
                                                $rootScope.modulus[modules[moduleKey].name].primaryField = modules[moduleKey].fields[fieldKey];
                                            }
                                        }
                                    }

                                    $rootScope.modulus['users'] = that.getUserModule();
                                    $rootScope.modulus['profiles'] = that.getProfileModule();
                                    $rootScope.modulus['roles'] = that.getRoleModule();

                                    //697 End

                                    $rootScope.user = myAccount.user;
                                    $rootScope.userPicture = blobUrl + '/' + $rootScope.user.picture;
                                    $rootScope.workgroups = myAccount.instances;
                                    $rootScope.multiTenant = myAccount.apps;
                                    var workgroupId = $localStorage.read('Workgroup');
                                    $rootScope.workgroup = myAccount.instances[0];

                                    if (workgroupId) {
                                        var workgroup = $filter('filter')(myAccount.instances, {instanceID: workgroupId}, true)[0];

                                        if (workgroup)
                                            $rootScope.workgroup = workgroup;
                                    }
                                    //config['imageUrl'] = myAccount.imageUrl;
                                    config['imageUrl'] = blobUrl + '/';
                                    config['storage_host'] = blobUrl + '/';

                                    $rootScope.processLanguage = function (entity) {
                                        if (entity && entity.languages && !angular.isObject(entity.languages))
                                            entity.languages = JSON.parse(entity.languages);
                                        for (var key in entity) {
                                            if (entity.hasOwnProperty(key)) {
                                                if (angular.isArray(entity[key])) {
                                                    $rootScope.processLanguages(entity[key]);
                                                } else if (angular.isObject(entity[key]) && key !== 'languages') {
                                                    for (var key1 in entity[key]) {
                                                        if (entity[key].hasOwnProperty(key1) && entity[key][key1] && angular.isObject(entity[key][key1])) {
                                                            $rootScope.processLanguage(entity[key]);
                                                        } else if (entity[key][key1] && entity[key][key1] && !angular.isObject(entity[key][key1]) && key1 === 'languages')
                                                            entity[key][key1] = JSON.parse(entity[key][key1]);
                                                    }
                                                }
                                            }
                                        }
                                    };

                                    $rootScope.processLanguages = function (entities) {
                                        if (entities && entities.length > 0) {
                                            for (var o = 0; o < entities.length; o++) {
                                                $rootScope.processLanguage(entities[o]);
                                            }
                                        }
                                    };

                                    $rootScope.languageStringify = function (entity) {
                                        if (entity && entity.languages && angular.isObject(entity.languages)) {
                                            entity.languages = JSON.stringify(entity.languages);
                                            for (var key in entity) {
                                                if (entity.hasOwnProperty(key))
                                                    if (angular.isArray(entity[key])) {
                                                        for (var l = 0; l < entity[key].length; l++) {
                                                            var prop = entity[key][l];
                                                            $rootScope.languageStringify(prop);
                                                        }
                                                    } else if (angular.isObject(entity[key])) {
                                                        for (var key1 in entity[key]) {
                                                            if (entity[key].hasOwnProperty(key1) && entity[key][key1] && angular.isObject(entity[key][key1])) {
                                                                $rootScope.languageStringify(entity[key]);
                                                            }
                                                        }
                                                    }
                                            }
                                        }
                                    };

                                    $rootScope.processPicklistLanguages = function (entities) {
                                        for (var key in entities) {
                                            if (entities.hasOwnProperty(key))
                                                $rootScope.processLanguages(entities[key]);
                                        }
                                    };

                                    $rootScope.getLanguageValue = function (language, firstProperty, secondProperty) {
                                        if (language && language[$rootScope.globalization.Label]) {
                                            if (!secondProperty)
                                                return language[$rootScope.globalization.Label][firstProperty] || '';

                                            return language[$rootScope.globalization.Label][firstProperty][secondProperty] || '';
                                        }
                                        return '';
                                    };
                                    //Drop List
                                    if (response[15] && response[15].data){
                                        window.droplist = {};
                                        for (var p = 0; p < response[15].data.length; p++) {
                                            var dropItem = response[15].data[p];
                                            $rootScope.processLanguage(dropItem);
                                            dropItem.label = dropItem.languages[globalization.Label].label;
                                            dropItem.items = that.proccesDropListItems(dropItem.items);
                                            window.droplist[dropItem.id] = dropItem;

                                        }
                                    }

                                    $rootScope.config = config;
                                    $rootScope.users = users;
                                    $rootScope.language = $localStorage.read('NG_TRANSLATE_LANG_KEY');
                                    $rootScope.locale = $localStorage.read('locale_key');
                                    $rootScope.currencySymbol = that.getCurrencySymbol($rootScope.workgroup.currency);
                                    $rootScope.processLanguages(modules);
                                    $rootScope.modules = modules;
                                    $rootScope.modules.push(that.getUserModule());
                                    $rootScope.modules.push(that.getProfileModule());
                                    $rootScope.modules.push(that.getRoleModule());

                                    $rootScope.processLanguages(profiles);
                                    $rootScope.profiles = profiles;
                                    $rootScope.moduleSettings = moduleSettings;
                                    $rootScope.system = {};
                                    $rootScope.helpPageFirstScreen = response[7].data;
                                    var customSettings = response[10].data;

                                    $rootScope.user.settings = [];
                                    for (var i = 0; i < userSettings.length; i++) {
                                        $rootScope.user.settings[userSettings[i].key] = userSettings[i];
                                    }

                                    $rootScope.isMobile = function () {
                                        var check = false;
                                        (function (a) {
                                            if (/(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino/i.test(a) || /1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-/i.test(a.substr(0, 4))) check = true;
                                        })(navigator.userAgent || navigator.vendor || window.opera);
                                        return check;
                                    };

                                    $rootScope.openFirtScreenHelpModal = function () {
                                        if ($rootScope.isMobile()) {
                                            return false;
                                        }

                                    };

                                    $rootScope.closeDialog = function (route, moduleId, dontShow) {
                                        if (!dontShow) {
                                            $rootScope.show = false;
                                            return;
                                        }

                                        if (moduleId) {
                                            if ($localStorage.read("moduleShow")) {
                                                var modalModules = JSON.parse($localStorage.read("moduleShow"));
                                                var modulShowArray = {
                                                    name: moduleId,
                                                    value: false
                                                };
                                                var sameModal = $filter('filter')(modalModules, {name: modulShowArray.name})[0];
                                                if (!sameModal) {
                                                    modalModules.push(modulShowArray);
                                                    $localStorage.write("moduleShow", JSON.stringify(modalModules));
                                                }
                                            } else {
                                                modalModules = [];
                                                modulShowArray = {
                                                    name: moduleId,
                                                    value: false
                                                };
                                                modalModules.push(modulShowArray);
                                                $localStorage.write("moduleShow", JSON.stringify(modalModules));
                                            }
                                        } else {
                                            if ($localStorage.read("routeShow")) {
                                                var routes = JSON.parse($localStorage.read("routeShow"));
                                                var routeShowArray = {
                                                    name: route,
                                                    value: 3
                                                };
                                                routes.push(routeShowArray);
                                                $localStorage.write("routeShow", JSON.stringify(routes));

                                            } else {
                                                routes = [];
                                                routeShowArray = {
                                                    name: route,
                                                    value: 3
                                                };
                                                routes.push(routeShowArray);
                                                $localStorage.write("routeShow", JSON.stringify(routes));
                                            }
                                        }
                                        $rootScope.show = false;
                                    };

                                    $rootScope.firtScreenShow = true;
                                    if ($rootScope.helpPageFirstScreen) {
                                        if ($rootScope.helpPageFirstScreen.show_type === "publish") {
                                            $rootScope.helpTemplate = $sce.trustAsHtml($rootScope.helpPageFirstScreen.template);

                                            if ($localStorage.read('FirstScreen')) {
                                                $rootScope.firtScreenShow = JSON.parse($localStorage.read('FirstScreen'));
                                            }
                                            if ($rootScope.firtScreenShow) {
                                                $rootScope.openFirtScreenHelpModal();
                                            }

                                            if (!$localStorage.read("startPage")) {
                                                var routes = [];
                                                var routeShow = {
                                                    name: $rootScope.currentPath,
                                                    value: 1
                                                };
                                                routes.push(routeShow);
                                                $localStorage.write("startPage", JSON.stringify(routes));
                                            }

                                            $localStorage.write('FirstScreen', false);

                                            $rootScope.showModal = function () {

                                                if ($localStorage.read('ModalShow')) {
                                                    $localStorage.remove('ModalShow');
                                                } else {
                                                    $localStorage.write('ModalShow', false);
                                                }
                                            };
                                        }
                                    } else $rootScope.firtScreenShow = false;

                                    //custom menü
                                    $rootScope.customMenu = false;
                                    var menu = $filter('filter')(response[8].data, {deleted: false}, true);
                                    if (menu.length > 0) {
                                        $rootScope.processLanguages(menu);
                                        $rootScope.customMenu = true;
                                        $rootScope.menu = $filter('orderBy')(menu, 'order', false);
                                    }

                                    //custom profile permissions
                                    var profilePermissions = response[11].data;
                                    if (profilePermissions) {
                                        $rootScope.customProfilePermissions = JSON.parse(profilePermissions.value).profilePermissions;
                                    }

                                    //module profile settings
                                    var profileSettings = response[5].data;
                                    if (profileSettings.length > 0) {
                                        for (var j = 0; j < profileSettings.length; j++) {
                                            var profileSetting = profileSettings[j];
                                            for (var k = 0; k < profileSetting.profile_list.length; k++) {
                                                var profile = profileSetting.profile_list[k];
                                                if (parseInt(profile) === $rootScope.user.profile.id) {
                                                    var moduleSetting = $filter('filter')($rootScope.modules, {id: profileSetting.module_id}, true)[0];
                                                    if (moduleSetting) {
                                                        if ($rootScope.customMenu) {
                                                            var customMenuItem;
                                                            customMenuItem = $filter('filter')($rootScope.menu, {route: moduleSetting.name}, true)[0];
                                                            if (!customMenuItem) {
                                                                for (var z = 0; z < $rootScope.menu.length; z++) {
                                                                    if (!customMenuItem) {
                                                                        var menuItem = $rootScope.menu[z];
                                                                        customMenuItem = $filter('filter')(menuItem.menu_items, {route: moduleSetting.name}, true)[0];
                                                                    }
                                                                }
                                                            }

                                                            if (!customMenuItem)
                                                                customMenuItem = {};

                                                            customMenuItem.label_tr = profileSetting.label_tr_plural;
                                                            customMenuItem.label_en = profileSetting.label_en_plural;
                                                            customMenuItem.menu_icon = profileSetting.menu_icon;
                                                            moduleSetting.display = profileSetting.display;

                                                            if (!profileSetting.display)
                                                                customMenuItem.hide = true;
                                                        } else {
                                                            moduleSetting.label_en_plural = profileSetting.label_en_plural;
                                                            moduleSetting.label_en_singular = profileSetting.label_en_singular;
                                                            moduleSetting.label_tr_plural = profileSetting.label_tr_plural;
                                                            moduleSetting.label_tr_singular = profileSetting.label_tr_singular;
                                                            moduleSetting.menu_icon = profileSetting.menu_icon;
                                                            moduleSetting.display = profileSetting.display;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    if ($rootScope.customMenu) {
                                        for (var a = 0; a < $rootScope.menu.length; a++) {
                                            var mainMenuItem = $rootScope.menu[a];
                                            if ($filter('filter')(mainMenuItem.menu_items, {hide: true}, true).length === mainMenuItem.menu_items.length)
                                                mainMenuItem.hide = true;

                                            //display values are taken according to module IDs.
                                            if (mainMenuItem.module_id) {
                                                var result_module = $filter('filter')($rootScope.modules, {id: mainMenuItem.module_id}, true)[0];

                                                if (result_module)
                                                    mainMenuItem.display = result_module.display;
                                            }
                                            /**Parentlar ya da Ana Kırılımlar, Örn: Genel / module1, module2 vb.
                                             * Custom modüller, Dashboard ve Reports vb. durumların kontrolü yapılmaktadır
                                             */
                                            else {
                                                mainMenuItem.display = true;
                                                /*Custom menü ilk yüklendiğinde dashboard active yapılmalı*/
                                                if (mainMenuItem.route === 'dashboard')
                                                    mainMenuItem.active = true;
                                                for (var o = 0; o < mainMenuItem.menu_items.length; o++) {
                                                    //Components don't have module_id
                                                    if (mainMenuItem.menu_items[o].module_id) {
                                                        var result_mainMenuItem_module = $filter('filter')($rootScope.modules, {id: mainMenuItem.menu_items[o].module_id}, true)[0];
                                                        mainMenuItem.menu_items[o].display = result_mainMenuItem_module ? result_mainMenuItem_module.display : false;
                                                    } else
                                                        mainMenuItem.menu_items[o].display = true;
                                                }
                                            }
                                        }
                                    }

                                    if (messaging) {
                                        if (messaging.SystemEMail)
                                            messaging.SystemEMail.enable_ssl = messaging.SystemEMail.enable_ssl === 'True';

                                        if (messaging.SystemEMail && messaging.SystemEMail.send_bulk_email_result)
                                            messaging.SystemEMail.send_bulk_email_result = messaging.SystemEMail.send_bulk_email_result === 'True';

                                        if (messaging.PersonalEMail)
                                            messaging.PersonalEMail.enable_ssl = messaging.PersonalEMail.enable_ssl === 'True';

                                        if (messaging.PersonalEMail && messaging.PersonalEMail.send_bulk_email_result)
                                            messaging.PersonalEMail.send_bulk_email_result = messaging.PersonalEMail.send_bulk_email_result === 'True';

                                        $rootScope.system.messaging = messaging;
                                    }

                                    if (!$rootScope.locale)
                                        $rootScope.locale = $rootScope.language;
                                    /** Farklı kullanıcılarla login oldunduğunda locale değişmiyordu. Bu da data exportta sorun teşkil ediyordu.
                                     * Locale'in giriş yapan kullanıcının language'ne eşit olması gerekmektedir.
                                     */
                                    else if ($rootScope.locale !== $rootScope.language)
                                        $rootScope.locale = $rootScope.language;

                                    $localStorage.write('locale_key', $rootScope.language);

                                    for (var i = 0; i < $rootScope.modules.length; i++) {
                                        var module = $rootScope.modules[i];
                                        module = that.processModule(module);

                                    }

                                    $rootScope.helpIconHide = $filter('filter')($rootScope.moduleSettings, {key: 'help_icon_hide'}, true)[0];
                                    $rootScope.helpIconHide = $rootScope.helpIconHide && $rootScope.helpIconHide.value === 'true';


                                    $rootScope.taskReminderAuto = $filter('filter')($rootScope.moduleSettings, {key: 'task_reminder_auto'}, true)[0];
                                    $rootScope.taskReminderAuto = $rootScope.taskReminderAuto && $rootScope.taskReminderAuto.value === 'true';
                                    $rootScope.detailViewType = $filter('filter')($rootScope.moduleSettings, {key: 'detail_view_type'}, true)[0];
                                    $rootScope.detailViewType = $rootScope.detailViewType ? $rootScope.detailViewType.value : 'tab';

                                    // $rootScope.showNotes = $filter('filter')($rootScope.moduleSettings, {key: 'show_notes'}, true)[0];
                                    //  $rootScope.showNotes = $rootScope.showNotes ? $rootScope.showNotes.value : true;

                                    $rootScope.showSaveAndNew = $filter('filter')($rootScope.moduleSettings, {key: 'show_save_and_new'}, true)[0];
                                    $rootScope.showSaveAndNew = $rootScope.showSaveAndNew ? $rootScope.showSaveAndNew.value : true;

                                    $rootScope.viewPermissions = $filter('filter')($rootScope.user.profile.permissions, {type: 2}, true)[0];

                                    $rootScope.deleteAllHiddenModules = $filter('filter')($rootScope.moduleSettings, {key: 'delete_all_hidden_modules'}, true)[0];
                                    $rootScope.deleteAllHiddenModules = $rootScope.deleteAllHiddenModules ? $rootScope.deleteAllHiddenModules.value.split(',') : [];

                                    //  $rootScope.showAttachments = $filter('filter')($rootScope.moduleSettings, {key: 'show_attachments'}, true)[0];
                                    // $rootScope.showAttachments = $rootScope.showAttachments ? $rootScope.showAttachments.value : true;
                                    //TODO REMOVE
                                    // $rootScope.newEpostaFieldName = $filter('filter')($rootScope.moduleSettings, {key: 'e_posta'}, true)[0];
                                    // $rootScope.newEpostaFieldName = $rootScope.newEpostaFieldName ? $rootScope.newEpostaFieldName.value : undefined;
                                    // $rootScope.newAdFieldName = $filter('filter')($rootScope.moduleSettings, {key: 'ad'}, true)[0];
                                    // $rootScope.newAdFieldName = $rootScope.newAdFieldName ? $rootScope.newAdFieldName.value : undefined;
                                    // $rootScope.newSoyadFieldName = $filter('filter')($rootScope.moduleSettings, {key: 'soyad'}, true)[0];
                                    // $rootScope.newSoyadFieldName = $rootScope.newSoyadFieldName ? $rootScope.newSoyadFieldName.value : undefined;

                                    if (customSettings) {

                                        $rootScope.showAccountOwner = $filter('filter')(customSettings, {key: 'show_admin'}, true)[0];
                                        $rootScope.showSubscriber = $filter('filter')(customSettings, {key: 'show_subscriber'}, true)[0];

                                        //var employeeSettings = $filter('filter')(customSettings, {key: 'employee'}, true)[0];
                                       // $rootScope.isEmployee = employeeSettings ? employeeSettings.value : undefined;

                                        /*
										* Check branch mode is available.
										* */
                                        // var branchSettings = $filter('filter')(response[10].data, {key: 'branch'}, true)[0];
                                        // $rootScope.branchAvailable = branchSettings ? branchSettings.value === 't' : undefined;
                                        //
                                        // if ($rootScope.branchAvailable && $rootScope.isEmployee) {
                                        //     var calisanRequest = {
                                        //         filters: [
                                        //             {
                                        //                 field: $rootScope.newEpostaFieldName ? $rootScope.newEpostaFieldName : 'e_posta',
                                        //                 operator: 'is',
                                        //                 value: myAccount.user.email,
                                        //                 no: 1
                                        //             },
                                        //             {field: 'deleted', operator: 'equals', value: false, no: 2}
                                        //         ],
                                        //         limit: 1
                                        //     };
                                        //
                                        //     $http.post(config.apiUrl + 'record/find/' + $rootScope.isEmployee, calisanRequest)
                                        //         .then(function (response) {
                                        //             var calisan = response.data;
                                        //             if (calisan.length > 0) {
                                        //                 $rootScope.user.calisanId = calisan[0]['id'];
                                        //                 $rootScope.user.branchId = calisan[0]['branch'];
                                        //             } else if (myAccount.user.profile.has_admin_rights) {
                                        //                 $rootScope.user.branchId = 1;
                                        //             }
                                        //         });
                                        // }
                                        //
                                    }

                                    helper.hideLoader();
                                    deferred.resolve(true);
                                });
                        });

                    return deferred.promise;
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
                    profileModule.languages = {};
                    profileModule.languages[$rootScope.globalization.Label] = angular.copy($rootScope.defaultSystemFields['profiles']);
                    var section = {};
                    section.name = 'profile_information';
                    section.system_type = 'system';
                    section.order = 1;
                    section.column_count = 1;
                    section.label_en = 'Profile Information';
                    section.label_tr = 'Profil Bilgisi';
                    section.display_detail = true;
                    section.languages = {};
                    section.languages[$rootScope.globalization.Label] = angular.copy($rootScope.defaultSystemFields['profile_information']);

                    var fieldName = {};
                    fieldName.name = 'languages.' + $rootScope.globalization.Label + '.name';
                    fieldName.system_type = 'system';
                    fieldName.data_type = 'text_single';
                    fieldName.order = 2;
                    fieldName.section = 1;
                    fieldName.section_column = 1;
                    fieldName.primary = true;
                    fieldName.inline_edit = false;
                    fieldName.label_en = 'Name';
                    fieldName.label_tr = 'İsim';
                    fieldName.display_list = true;
                    fieldName.display_detail = true;
                    fieldName.languages = {};
                    fieldName.languages[$rootScope.globalization.Label] = angular.copy($rootScope.defaultSystemFields['name']);

                    profileModule.fields.push(fieldName);

                    return profileModule;
                },
                getRoleModule: function () {
                    var roleModule = {};
                    roleModule.id = 1001;
                    roleModule.name = 'roles';
                    roleModule.system_type = 'system';
                    roleModule.order = 999;
                    roleModule.display = false;
                    roleModule.label_en_singular = 'Role';
                    roleModule.label_en_plural = 'Roles';
                    roleModule.label_tr_singular = 'Rol';
                    roleModule.label_tr_plural = 'Roller';
                    roleModule.menu_icon = 'fa fa-users';
                    roleModule.sections = [];
                    roleModule.fields = [];
                    roleModule.languages = {};
                    roleModule.languages[$rootScope.globalization.Label] = angular.copy($rootScope.defaultSystemFields['roles']);

                    var section = {};
                    section.name = 'role_information';
                    section.system_type = 'system';
                    section.order = 1;
                    section.column_count = 1;
                    section.label_en = 'Role Information';
                    section.label_tr = 'Rol Bilgisi';
                    section.display_detail = true;
                    section.languages = {};
                    section.languages[$rootScope.globalization.Label] = angular.copy($rootScope.defaultSystemFields['role_information']);

                    var fieldLabelEn = {};
                    //fieldLabelEn.name = 'label_en';
                    fieldLabelEn.name = 'languages.' + $rootScope.globalization.Label + '.label';
                    fieldLabelEn.system_type = 'system';
                    fieldLabelEn.data_type = 'text_single';
                    fieldLabelEn.order = 2;
                    fieldLabelEn.section = 1;
                    fieldLabelEn.section_column = 1;
                    fieldLabelEn.primary = true;
                    fieldLabelEn.inline_edit = false;
                    fieldLabelEn.label_en = 'Name English';
                    fieldLabelEn.label_tr = 'İsim İngilizce';
                    fieldLabelEn.display_list = true;
                    fieldLabelEn.display_detail = true;
                    fieldLabelEn.languages = {};
                    fieldLabelEn.languages[$rootScope.globalization.Label] = angular.copy($rootScope.defaultSystemFields['name']);

                    roleModule.fields.push(fieldLabelEn);

                    return roleModule;
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
                    userModule.languages = {};
                    userModule.languages[$rootScope.globalization.Label] = angular.copy($rootScope.defaultSystemFields['users']);


                    var section = {};
                    section.name = 'user_information';
                    section.system_type = 'system';
                    section.order = 1;
                    section.column_count = 1;
                    section.label_en = 'User Information';
                    section.label_tr = 'Kullanıcı Bilgisi';
                    section.display_detail = true;
                    section.languages = {};
                    section.languages[$rootScope.globalization.Label] = angular.copy($rootScope.defaultSystemFields['user_information']);

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
                    fieldEmail.display_detail = true;
                    fieldEmail.languages = {};
                    fieldEmail.languages[$rootScope.globalization.Label] = angular.copy($rootScope.defaultSystemFields['email']);
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
                    fieldFirstName.display_detail = true;
                    fieldFirstName.languages = {};
                    fieldFirstName.languages[$rootScope.globalization.Label] = angular.copy($rootScope.defaultSystemFields['first_name']);
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
                    fieldLastName.display_detail = true;
                    fieldLastName.languages = {};
                    fieldLastName.languages[$rootScope.globalization.Label] = angular.copy($rootScope.defaultSystemFields['last_name']);

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
                    fieldFullName.display_detail = true;
                    fieldFullName.combination = {};
                    fieldFullName.combination.field_1 = 'first_name';
                    fieldFullName.combination.field_2 = 'last_name';
                    fieldFullName.languages = {};
                    fieldFullName.languages[$rootScope.globalization.Label] = angular.copy($rootScope.defaultSystemFields['full_name']);

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
                    fieldPhone.display_detail = true;
                    fieldPhone.languages = {};
                    fieldPhone.languages[$rootScope.globalization.Label] = angular.copy($rootScope.defaultSystemFields['phone']);

                    userModule.fields.push(fieldPhone);

                    var fieldProfileId = {};
                    fieldProfileId.name = 'profile_id';
                    fieldProfileId.system_type = 'system';
                    fieldProfileId.data_type = 'number';
                    fieldProfileId.order = 6;
                    fieldProfileId.section = 1;
                    fieldProfileId.section_column = 1;
                    fieldProfileId.primary = false;
                    fieldProfileId.inline_edit = true;
                    fieldProfileId.editable = true;
                    fieldProfileId.show_label = true;
                    fieldProfileId.label_en = 'Profile Id';
                    fieldProfileId.label_tr = 'Profile Id';
                    fieldProfileId.display_list = true;
                    fieldProfileId.display_detail = true;
                    fieldProfileId.languages = {};
                    fieldProfileId.languages[$rootScope.globalization.Label] = angular.copy($rootScope.defaultSystemFields['profile_id']);

                    userModule.fields.push(fieldProfileId);

                    var fieldRoleId = {};
                    fieldRoleId.name = 'role_id';
                    fieldRoleId.system_type = 'system';
                    fieldRoleId.data_type = 'number';
                    fieldRoleId.order = 7;
                    fieldRoleId.section = 1;
                    fieldRoleId.section_column = 1;
                    fieldRoleId.primary = false;
                    fieldRoleId.inline_edit = true;
                    fieldRoleId.editable = true;
                    fieldRoleId.show_label = true;
                    fieldRoleId.label_en = 'Role Id';
                    fieldRoleId.label_tr = 'Role Id';
                    fieldRoleId.display_list = true;
                    fieldRoleId.display_detail = true;
                    fieldRoleId.languages = {};
                    fieldRoleId.languages[$rootScope.globalization.Label] = angular.copy($rootScope.defaultSystemFields['role_id']);

                    userModule.fields.push(fieldRoleId);

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

                            var sectionPermission = $filter('filter')(sectionPermissions, {profile_id: profile.id}, true)[0];

                            if (!sectionPermission) {
                                section.permissions.push({
                                    profile_id: profile.id,
                                    profile_name: profile.name,
                                    profile_is_admin: profile.has_admin_rights,
                                    type: 'full'
                                });
                            } else {
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
                        $rootScope.processLanguage(field);
                        field.label = $rootScope.getLanguageValue(field.languages, 'label');
                        field.dataType = dataTypes[field.data_type];
                        field.operators = [];
                        field.sectionObj = $filter('filter')(module.sections, {name: field.section}, true)[0];

                        if (field.data_type === 'lookup' || field.data_type === 'multiselect_lookup') {
                            if (field.lookup_type !== 'users' && field.lookup_type !== 'profiles' && field.lookup_type !== 'roles' && field.lookup_type !== 'relation') {
                                var lookupModule = $filter('filter')($rootScope.modules, {name: field.lookup_type}, true)[0];

                                if (!lookupModule)
                                    continue;

                                field.lookupModulePrimaryField = $filter('filter')(lookupModule.fields, {primary_lookup: true}, true)[0];

                                if (!field.lookupModulePrimaryField)
                                    field.lookupModulePrimaryField = $filter('filter')(lookupModule.fields, {primary: true}, true)[0];

                                var lookupModulePrimaryFieldDataType = dataTypes[field.lookupModulePrimaryField.data_type];

                                for (var m = 0; m < lookupModulePrimaryFieldDataType.operators.length; m++) {
                                    var operatorIdLookup = lookupModulePrimaryFieldDataType.operators[m];
                                    var operatorLookup = $rootScope.operators[operatorIdLookup];
                                    field.operators.push(operatorLookup);
                                }
                            } else {
                                field.operators.push($rootScope.operators.equals);
                                field.operators.push($rootScope.operators.not_equal);
                                field.operators.push($rootScope.operators.empty);
                                field.operators.push($rootScope.operators.not_empty);

                                if (field.lookup_type === 'users') {
                                    lookupModule = $filter('filter')($rootScope.modules, {name: 'users'}, true)[0];
                                    field.lookupModulePrimaryField = $filter('filter')(lookupModule.fields, {primary: true}, true)[0];
                                } else if (field.lookup_type === 'profiles') {
                                    lookupModule = $filter('filter')($rootScope.modules, {name: 'profiles'}, true)[0];
                                    field.lookupModulePrimaryField = $filter('filter')(lookupModule.fields, {primary: true}, true)[0];
                                } else if (field.lookup_type === 'roles') {
                                    lookupModule = $filter('filter')($rootScope.modules, {name: 'roles'}, true)[0];
                                    field.lookupModulePrimaryField = $filter('filter')(lookupModule.fields, {primary: true}, true)[0];
                                }
                            }

                        } else {
                            for (var n = 0; n < field.dataType.operators.length; n++) {
                                var operatorId = field.dataType.operators[n];
                                var operator = $rootScope.operators[operatorId];
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

                            var childField = $filter('filter')(module.fields, {
                                name: dependency.child_field,
                                inline_edit: true
                            }, true)[0];
                            if (childField)
                                childField.inline_edit = false;
                            var parentField = $filter('filter')(module.fields, {
                                name: dependency.parent_field,
                                inline_edit: true
                            }, true)[0];
                            if (parentField)
                                parentField.inline_edit = false;

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
                                        displayDependency.values.push(value);
                                    }

                                    if(dependency.values_array && angular.isArray(dependency.values_array)){
                                        displayDependency.values  = dependency.values_array;
                                    }
                                }

                                module.display_dependencies.push(displayDependency);
                            } else {
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
                    this.moduleOptionsMap(module)
                    return module;
                },
                getCurrencySymbol: function (currency) {
                    if (!currency)
                        return;

                    switch (currency) {
                        case 'TRY':
                            return '\u20ba';
                        case 'USD':
                            return '$';
                        case 'EURO':
                            return '€';
                    }
                },
                removeSampleData: function () {
                    return $http.delete(config.apiUrl + 'data/remove_sample_data');
                },
                addApp: function (appId) {
                    return $http.get(config.apiUrl + 'platform/office_app_create?appId=' + appId);
                },
                getHelp: function () {
                    return $http.get(config.apiUrl + 'help/get_all');
                },
                checkPermission: function () {
                    return $http.get(config.apiUrl + 'Profile/check_permission');
                },
                getOperators: function (label) {
                    return $http.get(config.apiUrl + 'localization/get_operators?label=' + label);
                },
                moduleOptionsMap: function (module) {
                    var defaultOptions = {
                        "show_notes": false,
                        "show_attachments": false,
                        "advanced_sharing_show": false
                    }

                    if (module.options) {
                        var options = JSON.parse(module.options);
                        for (var optionKey in options) {
                            if (angular.isDefined(defaultOptions[optionKey])) {
                                defaultOptions[optionKey] = options[optionKey];
                            }
                        }
                    }

                    module.options = defaultOptions;
                },

                proccesDropListItems:function (items) {
                    var lists= [];
                    for(var i = 0; i < items.length; i++) {
                        var item = items[i];
                        var label = "";
                        if(item.languages[globalization.Label] && item.languages[globalization.Label]["label"])
                            label = item.languages[globalization.Label]["label"];

                        lists.push(
                            {
                                "id":item.id,
                                "name":item.name,
                                "order":item.order,
                                "inactive":item.inactive,
                                "label":label
                            }
                        );
                    }
                   return lists;
                },

            };
        }]);

