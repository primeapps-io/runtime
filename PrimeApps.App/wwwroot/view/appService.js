'use strict';

angular.module('primeapps')

    .factory('AppService', ['$rootScope', '$http', '$localStorage', '$cache', '$q', '$filter', '$timeout', '$state', 'config', 'helper', 'sipHelper', 'entityTypes', 'taskDate', 'dataTypes', 'activityTypes', 'operators', 'systemRequiredFields', 'systemReadonlyFields', '$window', '$modal', '$sce', 'AuthService', '$cookies', 'blockUI',
        function ($rootScope, $http, $localStorage, $cache, $q, $filter, $timeout, $state, config, helper, sipHelper, entityTypes, taskDate, dataTypes, activityTypes, operators, systemRequiredFields, systemReadonlyFields, $window, $modal, $sce, AuthService, $cookies, blockUI) {
            return {

                getMyAccount: function (refresh) {
                    if (!refresh && $rootScope.config && $rootScope.user && $rootScope.workgroups && $rootScope.modules && $rootScope.users)
                        return null;

                    var that = this;
                    var deferred = $q.defer();

                    var clearAuthCache = function () {
                        $localStorage.remove('access_token');
                        $localStorage.remove('refresh_token');
                        $localStorage.remove('Workgroup');
                    };

                    $http.post(config.apiUrl + 'User/MyAccount', {})
                        .then(function (responseAccount) {
                            if (responseAccount.status != 200 || !responseAccount.data) {
                                clearAuthCache();
                                if (responseAccount.data === null) {
                                    deferred.resolve(401);
                                }
                                else {
                                    deferred.resolve(false);
                                }

                                AuthService.logout()
                                    .then(function (response) {
                                        $rootScope.app = 'crm';
                                        AuthService.logoutComplete();
                                        $cookies.remove('tenant_id')
                                        //$state.go('auth.login');
                                        window.location = response.data['redirect_url'];
                                        blockUI.stop();
                                    });


                                return deferred.promise;
                            }

                            //Check app domain and user app
                            var host = window.location.hostname;
                            var userAppId = responseAccount.data.user.appId;
                            var validApp = true;

                            if (host.indexOf('localhost') < 0) {
                                if (host.indexOf('crm.ofisim.com') > -1 && userAppId != 1) {
                                    validApp = false;
                                }
                                else if (host === 'test.ofisim.com' && userAppId != 1) {
                                    validApp = false;
                                }
                                else if (host === 'dev.ofisim.com' && userAppId != 1) {
                                    validApp = false;
                                }
                                else if (host.indexOf('kobi.ofisim.com') > -1 && userAppId != 2) {
                                    validApp = false;
                                }
                                else if (host.indexOf('kobi-test.ofisim.com') > -1 && userAppId != 2) {
                                    validApp = false;
                                }
                                else if (host.indexOf('asistan.ofisim.com') > -1 && userAppId != 3) {
                                    validApp = false;
                                }
                                else if (host.indexOf('asistan-test.ofisim.com') > -1 && userAppId != 3) {
                                    validApp = false;
                                }
                                else if (host.indexOf('ik.ofisim.com') > -1 && userAppId != 4) {
                                    validApp = false;
                                }
                                else if (host.indexOf('ik-test.ofisim.com') > -1 && userAppId != 4) {
                                    validApp = false;
                                }
                                else if (host.indexOf('ik-dev.ofisim.com') > -1 && userAppId != 4) {
                                    validApp = false;
                                }
                                else if (host.indexOf('hr.ofisim.com') > -1 && userAppId != 8) {
                                    validApp = false;
                                }
                                else if (host.indexOf('hr-test.ofisim.com') > -1 && userAppId != 8) {
                                    validApp = false;
                                }
                                else if (host.indexOf('hr-dev.ofisim.com') > -1 && userAppId != 8) {
                                    validApp = false;
                                }
                                else if (host.indexOf('cagri.ofisim.com') > -1 && userAppId != 5) {
                                    validApp = false;
                                }
                                else if (host.indexOf('cagri-test.ofisim.com') > -1 && userAppId != 5) {
                                    validApp = false;
                                }
                            }

                            if (!validApp) {
                                $window.location.href = '/auth/login?Error=isNotValidApp';
                                return;
                            }

                            if (responseAccount.data.user.email.indexOf('app_') > -1)
                                $rootScope.preview = true;

                            var promises = [];
                            promises.push($http.get(config.apiUrl + 'module/get_all'));
                            promises.push($http.get(config.apiUrl + 'messaging/get_config'));
                            promises.push($http.get(config.apiUrl + 'User/get_all'));
                            promises.push($http.get(config.apiUrl + 'Profile/GetAllBasic'));
                            promises.push($http.get(config.apiUrl + 'module/get_module_settings'));
                            promises.push($http.get(config.apiUrl + 'phone/get_config'));
                            promises.push($http.get(config.apiUrl + 'process/get_all'));
                            promises.push($http.get(config.apiUrl + 'module_profile_settings/get_all'));
                            promises.push($http.get(config.apiUrl + 'help/get_all'));
                            promises.push($http.get(config.apiUrl + 'help/get_first_screen?templateType=' + 'modal' + '&firstscreen=' + true));
                            promises.push($http.get(config.apiUrl + 'menu/get/' + responseAccount.data.user.profile.id));
                            promises.push($http.get(config.apiUrl + 'settings/get_all/custom?userId=' + responseAccount.data.user.id));
                            promises.push($http.get(config.apiUrl + 'settings/get_by_key/1/branch'));

                            $q.all(promises)
                                .then(function (response) {
                                    if (response.length < 6
                                        || response[0].status != 200 || response[1].status != 200 || response[2].status != 200 || response[3].status != 200 || response[4].status != 200 || response[5].status != 200
                                        || !response[0].data || !response[1].data || !response[2].data || !response[3].data || !response[4].data) {
                                        clearAuthCache();
                                        deferred.resolve(false);
                                        return deferred.promise;
                                    }

                                    var isDemo = responseAccount.data.user.isDemo || false;
                                    var account = responseAccount.data;

                                    /*
                                   * Check branch mode is available.
                                   * */
                                    if (response[12].status === 200 && response[12].data.value) {
                                        $rootScope.branchAvailable = response[12].data.value === 't';

                                        var calisanRequest = {
                                            filters: [
                                                { field: 'e_posta', operator: 'is', value: account.user.email, no: 1 },
                                                { field: 'deleted', operator: 'equals', value: false, no: 2 }
                                            ],
                                            limit: 1
                                        };

                                        $http.post(config.apiUrl + 'record/find/calisanlar', calisanRequest)
                                            .then(function (response) {
                                                var calisan = response.data;
                                                if (calisan.length > 0) {
                                                    $rootScope.user.calisanId = calisan[0]['id'];
                                                    $rootScope.user.branchId = calisan[0]['branch'];
                                                } else if (account.user.profile.has_admin_rights) {
                                                    $rootScope.user.branchId = 1;
                                                }
                                            });
                                    }
                                    var modules = !isDemo ? response[0].data : $filter('filter')(response[0].data, function (value) {
                                        return value.created_by_id == account.user.id || value.system_type == 'system';
                                    }, true);

                                    var messaging = response[1].data;

                                    var users = !isDemo ? response[2].data : $filter('filter')(response[2].data, function (value) {
                                        return value.id == account.user.id;
                                    }, true);

                                    var profiles = !isDemo ? response[3].data : $filter('filter')(response[3].data, function (value) {
                                        return value.created_by_id == account.user.id || value.is_persistent === true;
                                    }, true);

                                    var moduleSettings = response[4].data;
                                    var phoneSettings = response[5].data;
                                    var userSettings = response[11].data;

                                    if (account.instances.length < 1) {
                                        $state.go('join');
                                        return;
                                    }

                                    //#697 Remove lookup fields ( When lookup module deleted ).
                                    var activeModuleNames = modules.map(function (a) {
                                        return a.name;
                                    });

                                    for (var moduleKey = modules.length - 1; moduleKey >= 0; moduleKey--) {
                                        for (var fieldKey = modules[moduleKey].fields.length - 1; fieldKey >= 0; fieldKey--) {
                                            if (modules[moduleKey].fields[fieldKey].data_type == "lookup"
                                                && modules[moduleKey].fields[fieldKey].lookup_type != "users"
                                                && modules[moduleKey].fields[fieldKey].lookup_type != "profiles"
                                                && modules[moduleKey].fields[fieldKey].lookup_type != "roles"
                                                && modules[moduleKey].fields[fieldKey].lookup_type != "relation"
                                                && activeModuleNames.indexOf(modules[moduleKey].fields[fieldKey].lookup_type) === -1)
                                                modules[moduleKey].fields.splice(fieldKey, 1);
                                        }
                                    }
                                    //697 End

                                    $rootScope.user = account.user;
                                    $rootScope.workgroups = account.instances;
                                    $rootScope.multiTenant = account.apps;
                                    var workgroupId = $localStorage.read('Workgroup');
                                    $rootScope.workgroup = account.instances[0];

                                    if (workgroupId) {
                                        var workgroup = $filter('filter')(account.instances, { instanceID: workgroupId }, true)[0];

                                        if (workgroup)
                                            $rootScope.workgroup = workgroup;
                                    }
                                    config['imageUrl'] = account.imageUrl;
                                    $rootScope.config = config;
                                    $rootScope.taskDate = taskDate;
                                    $rootScope.users = users;
                                    $rootScope.language = $localStorage.read('NG_TRANSLATE_LANG_KEY');
                                    $rootScope.locale = $localStorage.read('locale_key');
                                    $rootScope.currencySymbol = that.getCurrencySymbol($rootScope.workgroup.currency);
                                    $rootScope.modules = modules;
                                    $rootScope.modules.push(that.getUserModule());
                                    $rootScope.modules.push(that.getProfileModule());
                                    $rootScope.modules.push(that.getRoleModule());
                                    $rootScope.profiles = profiles;
                                    $rootScope.moduleSettings = moduleSettings;
                                    $rootScope.system = {};
                                    $rootScope.approvalProcesses = response[6].data;
                                    $rootScope.helpPageFirstScreen = response[9].data;

                                    $rootScope.user.settings = [];
                                    for (var i = 0; i < userSettings.length; i++) {
                                        $rootScope.user.settings[userSettings[i].key] = userSettings[i];
                                    }

                                    if ($rootScope.user.settings['has_analytics'])
                                        $rootScope.user.settings['has_analytics'].value === 'True' ? $rootScope.user.has_analytics = true : $rootScope.user.has_analytics = false;
                                    $rootScope.openFirtScreenHelpModal = function () {
                                        $rootScope.isMobile = function () {
                                            var check = false;
                                            (function (a) {
                                                if (/(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino/i.test(a) || /1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-/i.test(a.substr(0, 4))) check = true;
                                            })(navigator.userAgent || navigator.vendor || window.opera);
                                            return check;
                                        };
                                        if ($rootScope.isMobile()) {
                                            return false;
                                        }
                                        $rootScope.helpModal = $rootScope.helpModal || $modal({
                                            scope: $rootScope,
                                            templateUrl: 'view/setup/help/helpPageModal.html',
                                            animation: '',
                                            backdrop: 'static',
                                            show: false,
                                            tag: 'createModal'
                                        });

                                        $rootScope.helpModal.$promise.then($rootScope.helpModal.show);

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
                                                $localStorage.write('ModalShow', false);
                                            };
                                        }
                                    }

                                    //custom menü
                                    $rootScope.customMenu = false;
                                    var menu = response[10].data;
                                    if (menu) {
                                        $rootScope.customMenu = true;
                                        $rootScope.menu = $filter('orderBy')(menu, 'order', false);
                                    }

                                    //module profile settings
                                    var profileSettings = response[7].data;
                                    if (profileSettings.length > 0) {
                                        for (var j = 0; j < profileSettings.length; j++) {
                                            var profileSetting = profileSettings[j];
                                            for (var k = 0; k < profileSetting.profile_list.length; k++) {
                                                var profile = profileSetting.profile_list[k];
                                                if (parseInt(profile) === $rootScope.user.profile.id) {
                                                    var moduleSetting = $filter('filter')($rootScope.modules, { id: profileSetting.module_id }, true)[0];
                                                    if (moduleSetting) {
                                                        if ($rootScope.customMenu) {
                                                            var customMenuItem;
                                                            customMenuItem = $filter('filter')($rootScope.menu, { route: moduleSetting.name }, true)[0];
                                                            if (!customMenuItem) {
                                                                for (var z = 0; z < $rootScope.menu.length; z++) {
                                                                    if (!customMenuItem) {
                                                                        var menuItem = $rootScope.menu[z];
                                                                        customMenuItem = $filter('filter')(menuItem.menu_items, { route: moduleSetting.name }, true)[0];
                                                                    }
                                                                }
                                                            }

                                                            if (!customMenuItem)
                                                                var customMenuItem = {};

                                                            customMenuItem.label_tr = profileSetting.label_tr_plural;
                                                            customMenuItem.label_en = profileSetting.label_en_plural;
                                                            customMenuItem.menu_icon = profileSetting.menu_icon;
                                                            moduleSetting.display = profileSetting.display;

                                                            if (!profileSetting.display)
                                                                customMenuItem.hide = true;
                                                        }
                                                        else {
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
                                            if ($filter('filter')(mainMenuItem.menu_items, { hide: true }, true).length === mainMenuItem.menu_items.length)
                                                mainMenuItem.hide = true;

                                            //display values are taken according to module IDs.
                                            if (mainMenuItem.module_id) {
                                                var result_module = $filter('filter')($rootScope.modules, { id: mainMenuItem.module_id }, true)[0];
                                                mainMenuItem.display = result_module.display;
                                            }
                                            else
                                                mainMenuItem.display = true;
                                        }
                                    }

                                    //if activities module not exist, calendar and task modules hided
                                    $rootScope.isActivityModuleExist = false;
                                    if ($filter('filter')($rootScope.modules, { name: 'activities' }, true).length > 0) {
                                        $rootScope.isActivityModuleExist = true;
                                    }

                                    //holidays
                                    var holidaysModule = $filter('filter')($rootScope.modules, { name: 'holidays' }, true)[0];

                                    if (holidaysModule) {
                                        var countryField = $filter('filter')(holidaysModule.fields, { name: 'country' }, true)[0];
                                        helper.getPicklists([countryField.picklist_id])
                                            .then(function (picklists) {
                                                var countryPicklist = picklists[countryField.picklist_id];
                                                var countryPicklistItemTr = $filter('filter')(countryPicklist, { value: 'tr' }, true)[0];
                                                var countryPicklistItemEn = $filter('filter')(countryPicklist, { value: 'en' }, true)[0];
                                                var request = {};
                                                request.limit = 1000;

                                                if ($rootScope.language === 'tr' && countryPicklistItemTr)
                                                    request.filters = [{ field: 'country', operator: 'equals', value: countryPicklistItemTr.labelStr, no: 1 }];
                                                else if (countryPicklistItemEn)
                                                    request.filters = [{ field: 'country', operator: 'is', value: countryPicklistItemEn.labelStr }];

                                                $http.post(config.apiUrl + 'record/find/holidays', request).then(function (response) {
                                                    var data = response.data;
                                                    var holidays = [];
                                                    for (var i = 0; i < data.length; i++) {
                                                        var date = moment(data[i].date).format('DD-MM-YYYY');
                                                        holidays.push(date);
                                                    }
                                                    $rootScope.holidaysData = data;
                                                    $rootScope.holidays = holidays;
                                                });
                                            });
                                    }

                                    if (messaging) {
                                        if (messaging.SystemEMail)
                                            messaging.SystemEMail.enable_ssl = messaging.SystemEMail.enable_ssl === 'True';

                                        if (messaging.PersonalEMail)
                                            messaging.PersonalEMail.enable_ssl = messaging.PersonalEMail.enable_ssl === 'True';

                                        $rootScope.system.messaging = messaging;
                                    }


                                    if ($rootScope.workgroup.licenses.sip_license_count > 0) {
                                        if (angular.isObject(phoneSettings))
                                            $rootScope.phoneSettings = phoneSettings;
                                        else
                                            $rootScope.phoneSettings = {};

                                        $rootScope.phoneSettings.sipLicenseCount = $rootScope.workgroup.licenses.sip_license_count;
                                        //getUserSpecific sipAccount Info
                                        if (phoneSettings.sipUsers) {
                                            var sipData = $filter('filter')(phoneSettings.sipUsers, { userId: account.user.id.toString(), isActive: 'true' }, true)[0];
                                            if (sipData) {
                                                var sipPromises = [];
                                                sipPromises.push($http.get(config.apiUrl + 'phone/get_sip_password'));

                                                $q.all(sipPromises)
                                                    .then(function (response) {
                                                        if (response[0].status === 200) {
                                                            var sipPassword = response[0].data;

                                                            $rootScope.sipUser = {
                                                                Extension: sipData.extension,
                                                                Server: sipData.server,
                                                                SipUri: sipData.sipuri,
                                                                Password: sipPassword,
                                                                UserId: sipData.userId,
                                                                IsAutoRegister: sipData.isAutoRegister,
                                                                IsAutoRecordDetail: sipData.isAutoRecordDetail,
                                                                RecordDetailModuleName: sipData.recordDetailModuleName,
                                                                RecordDetailPhoneFieldName: sipData.recordDetailPhoneFieldName,
                                                                userAgent: null,
                                                                events: {},
                                                                session: null,
                                                                line: null,
                                                                lineInfo: {
                                                                    State: null,
                                                                    TalkingTimer: 0,
                                                                    TalkingNumber: null,
                                                                    ActiveNumber: null,
                                                                    IncomingCallNumber: null,
                                                                    TalkingRecordInfo: null,
                                                                    IsMuted: false,
                                                                    IsHold: false,
                                                                    ModuleName: null,
                                                                    RecordId: null
                                                                },
                                                                activePhoneScreen: 'readyScreen',
                                                                numberToDial: ''

                                                            };

                                                            $rootScope.sipAccessRights = true;

                                                            if (sipData.isAutoRegister === 'true') {
                                                                sipHelper.register();
                                                            }
                                                        }
                                                    });
                                            }
                                        }
                                    }

                                    if (!$rootScope.locale) {
                                        $rootScope.locale = $rootScope.language;
                                        $localStorage.write('locale_key', $rootScope.language);
                                    }

                                    for (var i = 0; i < $rootScope.modules.length; i++) {
                                        var module = $rootScope.modules[i];
                                        module = that.processModule(module);

                                        if (!module.menu_icon) {
                                            that.setModuleMenuIcon(module);
                                        }
                                    }

                                    $rootScope.tasksNamePlural = $filter('filter')($rootScope.moduleSettings, { key: 'tasks_name_plural' }, true)[0];
                                    $rootScope.tasksNameSingular = $filter('filter')($rootScope.moduleSettings, { key: 'tasks_name_singular' }, true)[0];
                                    $rootScope.activityTypeCustom = $filter('filter')($rootScope.moduleSettings, { key: 'activity_type_custom' }, true)[0];
                                    $rootScope.helpIconHide = $filter('filter')($rootScope.moduleSettings, { key: 'help_icon_hide' }, true)[0];
                                    $rootScope.helpIconHide = $rootScope.helpIconHide && $rootScope.helpIconHide.value === 'true';
                                    $rootScope.showTimesheetMenu = $filter('filter')($rootScope.moduleSettings, { key: 'show_timesheet_menu' }, true)[0];
                                    $rootScope.showTimesheetMenu = $rootScope.showTimesheetMenu && $rootScope.showTimesheetMenu.value === 'true';
                                    $rootScope.taskReminderAuto = $filter('filter')($rootScope.moduleSettings, { key: 'task_reminder_auto' }, true)[0];
                                    $rootScope.taskReminderAuto = $rootScope.taskReminderAuto && $rootScope.taskReminderAuto.value === 'true';
                                    $rootScope.detailViewType = $filter('filter')($rootScope.moduleSettings, { key: 'detail_view_type' }, true)[0];
                                    $rootScope.detailViewType = $rootScope.detailViewType ? $rootScope.detailViewType.value : 'tab';
                                    $rootScope.advancedDocumentSearch = $filter('filter')($rootScope.moduleSettings, { key: 'advanced_document_search' }, true)[0];
                                    $rootScope.advancedDocumentSearch = $rootScope.advancedDocumentSearch ? ($rootScope.advancedDocumentSearch.value && $rootScope.user.profile.document_search) : false;
                                    $rootScope.showNotes = $filter('filter')($rootScope.moduleSettings, { key: 'show_notes' }, true)[0];
                                    $rootScope.showNotes = $rootScope.showNotes ? $rootScope.showNotes.value : true;
                                    $rootScope.calendarFields = $filter('filter')($rootScope.moduleSettings, { key: 'calendar_fields' }, true)[0];
                                    $rootScope.showSaveAndNew = $filter('filter')($rootScope.moduleSettings, { key: 'show_save_and_new' }, true)[0];
                                    $rootScope.showSaveAndNew = $rootScope.showSaveAndNew ? $rootScope.showSaveAndNew.value : true;
                                    $rootScope.personalConvertShow = $filter('filter')($rootScope.moduleSettings, { key: 'personal_convert_show' }, true)[0];
                                    $rootScope.personalConvertShow = $rootScope.personalConvertShow ? $rootScope.personalConvertShow.value : false;
                                    $rootScope.permissionsReport = $filter('filter')($rootScope.user.profile.permissions, { type: 2 }, true)[0];
                                    $rootScope.permissionsNewsfeed = $filter('filter')($rootScope.user.profile.permissions, { 'Type': 3 }, true)[0];

                                    that.setCustomActivityTypes(activityTypes);

                                    helper.hideLoader();
                                    deferred.resolve(true);
                                });
                        });

                    if (!$rootScope.pageTitle)
                        $rootScope.pageTitle = 'Ofisim.com';

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
                    fieldProfileId.display_form = true;
                    fieldProfileId.display_detail = true;
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
                    fieldRoleId.display_form = true;
                    fieldRoleId.display_detail = true;
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

                            var childField = $filter('filter')(module.fields, { name: dependency.child_field, inline_edit: true }, true)[0];
                            if (childField)
                                childField.inline_edit = false;
                            var parentField = $filter('filter')(module.fields, { name: dependency.parent_field, inline_edit: true }, true)[0];
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
