'use strict';

angular.module('primeapps')

    .factory('$localStorage', ['$window', function ($window) {
        return {
            set: function (key, value) {
                $window.localStorage[key] = angular.toJson(value);
            },
            get: function (key) {
                var value = this.read(key);

                if (!value)
                    return null;

                return angular.fromJson(value);
            },
            write: function (key, value) {
                $window.localStorage[key] = value;
            },
            read: function (key) {
                return $window.localStorage[key];
            },
            remove: function (key) {
                $window.localStorage.removeItem(key);
            }
        }
    }])

    .factory('$sessionStorage', ['$window', function ($window) {
        return {
            set: function (key, value) {
                $window.sessionStorage[key] = angular.toJson(value);
            },
            get: function (key) {
                var value = this.read(key);

                if (!value)
                    return null;

                return angular.fromJson(value);
            },
            write: function (key, value) {
                $window.sessionStorage[key] = value;
            },
            read: function (key) {
                return $window.sessionStorage[key];
            },
            remove: function (key) {
                $window.sessionStorage.removeItem(key);
            },
            clear: function () {
                $window.sessionStorage.clear();
            }
        }
    }])

    .factory('$cache', ['$cacheFactory', function ($cacheFactory) {
        return $cacheFactory('primeapps');
    }])

    .factory('helper', ['$rootScope', '$timeout', '$filter', '$localStorage', '$sessionStorage', '$q', '$http', 'config', '$cache',
        function ($rootScope, $timeout, $filter, $localStorage, $sessionStorage, $q, $http, config, $cache) {
            return {
                SnakeToCamel: function (data, depth) {

                    function _processKeys(obj, processer, depth) {
                        if (depth === 0 || !angular.isObject(obj)) {
                            return obj;
                        }

                        var result = {};
                        var keys = Object.keys(obj);

                        for (var i = 0; i < keys.length; i++) {
                            result[processer(keys[i])] = _processKeys(obj[keys[i]], processer, depth - 1);
                        }

                        return result;
                    }

                    function _snakelize(key) {
                        var separator = '_';
                        var split = /(?=[A-Z])/;

                        return key.split(split).join(separator).toLowerCase();
                    }

                    if (angular.isObject(data)) {
                        if (typeof depth === 'undefined') {
                            depth = 1;
                        }
                        return _processKeys(data, _snakelize, depth);
                    } else {
                        return _snakelize(data);
                    }
                },
                getTime: function (str) {
                    if (!str)
                        return '';

                    var date = new Date(str);

                    return date.getTime();
                },
                dateDiff: function (dt) {
                    var today = new Date();
                    today.setHours(0, 0, 0, 0);
                    var diff = Math.floor((dt > today ? dt - today : today - dt) / 86400000);

                    return diff;
                },
                hideLoader: function () {
                    if (document.body.className.indexOf('loaded') === -1)
                        document.body.className += ' loaded';

                    $timeout(function () {
                        var loaderElement = document.getElementById('loader');

                        if (loaderElement)
                            loaderElement.parentNode.removeChild(loaderElement);
                    }, 300);
                },
                getFileExtension: function (fileName) {
                    var extension = fileName.split('.');

                    if (extension.length < 2)
                        return '';

                    extension = extension[extension.length - 1].toLowerCase() || '';

                    return extension;
                },
                arrayObjectIndexOf: function (arr, obj) {
                    for (var i = 0; i < arr.length; i++) {
                        if (angular.equals(arr[i], obj)) {
                            return i;
                        }
                    }

                    return -1;
                },
                hasPermission: function (moduleName, operation, record) {
                    if (moduleName === 'related_to')
                        moduleName = 'activities';

                    if (moduleName === 'stage_history')
                        moduleName = 'opportunities';

                    var module = $filter('filter')($rootScope.modules, {name: moduleName}, true)[0];

                    if (!module) return false;

                    var permission = $filter('filter')($rootScope.user.profile.permissions, {module_id: module.id}, true)[0];

                    if (permission && permission[operation]) {
                        if ((operation === 'Modify' || operation === 'Remove') && record && (!record.shared_users_edit || record.shared_users_edit.indexOf($rootScope.user.ID) === -1) && (record.shared_users && record.shared_users.indexOf($rootScope.user.ID) > -1)) {
                            return false;
                        }
                        return true;
                    } else if (record) {
                        /*
                        * Checking record advanced sharing.
                        * */
                        switch (operation) {
                            case 'Read': {
                                if (record.shared_users && record.shared_users.indexOf($rootScope.user.id) > -1) {
                                    return true;
                                }

                                if (record.shared_user_groups && $rootScope.user.groups.length) {
                                    for (var userGroupId in record.shared_user_groups) {
                                        if ($rootScope.user.groups.indexOf(record.shared_user_groups[userGroupId]) > -1)
                                            return true;
                                    }
                                }
                                return false;
                            }
                            case 'Remove':
                            case 'Modify': {
                                if ($rootScope.user.profile.has_admin_rights) {
                                    return true;
                                }

                                if (record.shared_users_edit && record.shared_users_edit.indexOf($rootScope.user.id) > -1) {
                                    return true;
                                }

                                if (record.shared_user_groups_edit) {
                                    for (var userGroupId in record.shared_user_groups_edit) {
                                        if ($rootScope.user.groups.indexOf(record.shared_user_groups_edit[userGroupId]) > -1)
                                            return true;
                                    }
                                }
                                return false;
                            }
                        }
                    }

                    return false;
                },
                hasDocumentsPermission: function (operation) {
                    var permission = $filter('filter')($rootScope.user.profile.permissions, {type: 1})[0];

                    if (!permission)
                        return false;

                    return permission[operation];
                },
                hasAdminRights: function () {
                    return true;
                },
                getCulture: function () {
                    var language = $localStorage.read('NG_TRANSLATE_LANG_KEY') || 'tr';

                    switch (language) {
                        case 'tr':
                            return 'tr-TR';
                            break;
                        case 'en':
                            return 'en-US';
                            break
                    }
                },
                getCurrency: function () {
                    var language = $localStorage.read('NG_TRANSLATE_LANG_KEY') || 'tr';

                    switch (language) {
                        case 'tr':
                            return 'TRY';
                            break;
                        case 'en':
                            return 'USD';
                            break
                    }
                },
                getCurrentDateMin: function () {
                    var minDate = new Date();
                    minDate.setHours(0);
                    minDate.setMinutes(0);
                    minDate.setSeconds(0);
                    minDate.setMilliseconds(0);

                    return minDate;
                },
                getCurrentDateMax: function () {
                    var maxDate = new Date();
                    maxDate.setHours(23);
                    maxDate.setMinutes(59);
                    maxDate.setSeconds(59);
                    maxDate.setMilliseconds(0);

                    return maxDate;
                },
                floorMinutes: function (time) {
                    var coeff = 1e3 * 60 * 5;
                    return new Date(Math.floor(time.getTime() / coeff) * coeff);
                },
                lookupProfile: function (searchTerm, firstItem, includeInactiveUsers) {
                    var deferred = $q.defer();

                    if (!searchTerm && !firstItem) {
                        deferred.resolve([]);
                        return deferred.promise;
                    }

                    var findRequest = {
                        fields: ['id', 'name'],
                        filters: [
                            {
                                field: 'name',
                                operator: 'starts_with',
                                value: searchTerm,
                                no: 1
                            },
                            {
                                field: 'deleted',
                                operator: 'equals',
                                value: false,
                                no: 2
                            }
                        ],
                        limit: 20,
                        sort_field: 'name',
                        sort_direction: 'asc'
                    };

                    if (!searchTerm) {
                        findRequest.filters.shift();
                        findRequest.filters[0].no = 1;
                    }

                    $http.post(config.apiUrl + 'record/find/profiles', findRequest)
                        .then(function (response) {
                            response = response.data;
                            if (!response) {
                                deferred.resolve([]);
                                return deferred.promise;
                            }

                            var profiles = [];

                            for (var i = 0; i < response.length; i++) {
                                var userRecord = response[i];

                                var profile = {};
                                profile.id = userRecord.id;
                                profile.name = userRecord.name;

                                profiles.push(profile);
                            }

                            deferred.resolve(profiles);
                        })
                        .catch(function (reason) {
                            deferred.reject(reason.data);
                        });

                    return deferred.promise;
                },
                lookupRole: function (searchTerm, firstItem, includeInactiveUsers) {
                    var deferred = $q.defer();

                    if (!searchTerm && !firstItem) {
                        deferred.resolve([]);
                        return deferred.promise;
                    }

                    var findRequest = {
                        fields: ['id', 'label_en', 'label_tr'],
                        filters: [
                            {
                                field: 'label_' + $rootScope.user.tenant_language,
                                operator: 'starts_with',
                                value: searchTerm,
                                no: 1
                            },
                            {
                                field: 'deleted',
                                operator: 'equals',
                                value: false,
                                no: 2
                            }
                        ],
                        limit: 20,
                        sort_field: 'label_' + $rootScope.user.tenant_language,
                        sort_direction: 'asc'
                    };

                    if (!searchTerm) {
                        findRequest.filters.shift();
                        findRequest.filters[0].no = 1;
                    }

                    $http.post(config.apiUrl + 'record/find/roles', findRequest)
                        .then(function (response) {
                            response = response.data;
                            if (!response) {
                                deferred.resolve([]);
                                return deferred.promise;
                            }

                            var roles = [];

                            for (var i = 0; i < response.length; i++) {
                                var userRecord = response[i];

                                var role = {};
                                role.id = userRecord.id;
                                role.name = userRecord['label_' + $rootScope.user.tenant_language];

                                roles.push(role);
                            }

                            deferred.resolve(roles);
                        })
                        .catch(function (reason) {
                            deferred.reject(reason.data);
                        });

                    return deferred.promise;
                },
                lookupUser: function (searchTerm, firstItem, includeInactiveUsers) {
                    // var deferred = $q.defer();
                    //
                    // if (!searchTerm && !firstItem) {
                    //     deferred.resolve([]);
                    //     return deferred.promise;
                    // }
                    //
                    // var findRequest = {
                    //     fields: ['id', 'full_name', 'email', 'is_active'],
                    //     filters: [
                    //         {
                    //             field: 'full_name',
                    //             operator: 'starts_with',
                    //             value: searchTerm,
                    //             no: 1
                    //         },
                    //         {
                    //             field: 'is_active',
                    //             operator: 'equals',
                    //             value: true,
                    //             no: 2
                    //         }
                    //     ],
                    //     limit: 20,
                    //     sort_field: 'full_name',
                    //     sort_direction: 'asc'
                    // };
                    //
                    // //includes users whose are not active but records owners
                    // if (includeInactiveUsers && includeInactiveUsers == true) {
                    //     for (var i = 0; i < findRequest.filters.length; i++) {
                    //         var obj = findRequest.filters[i];
                    //
                    //         if (obj.field == 'is_active') {
                    //             var itemIndex = findRequest.filters.indexOf(obj);
                    //             findRequest.filters.splice(itemIndex, 1);
                    //         }
                    //     }
                    // }
                    //
                    // if (!searchTerm) {
                    //     findRequest.filters.shift();
                    //     findRequest.filters[0].no = 1;
                    // }
                    //
                    // $http.post(config.apiUrl + 'record/find/users', findRequest)
                    //     .then(function (response) {
                    //         response = response.data;
                    //         if (!response) {
                    //             deferred.resolve([]);
                    //             return deferred.promise;
                    //         }
                    //
                    //         var users = [];
                    //
                    //         if (firstItem && !searchTerm) {
                    //             var userFirstItem = {};
                    //             userFirstItem.id = 0;
                    //
                    //             switch (firstItem) {
                    //                 case 'record_owner':
                    //                     userFirstItem.email = '[owner]';
                    //                     userFirstItem.full_name = $filter('translate')('Common.RecordOwner');
                    //                     break;
                    //                 case 'logged_in_user':
                    //                     userFirstItem.email = '[me]';
                    //                     userFirstItem.full_name = $filter('translate')('Common.LoggedInUser');
                    //                     break;
                    //             }
                    //
                    //             users.push(userFirstItem)
                    //         }
                    //
                    //         for (var i = 0; i < response.length; i++) {
                    //             var userRecord = response[i];
                    //
                    //             var user = {};
                    //             user.id = userRecord.id;
                    //             user.email = userRecord.email;
                    //             user.full_name = userRecord.full_name;
                    //
                    //             users.push(user)
                    //         }
                    //
                    //         deferred.resolve(users);
                    //     })
                    //     .catch(function (reason) {
                    //         deferred.reject(reason.data);
                    //     });
                    //
                    // return deferred.promise;
                    var users = [];
                    if (firstItem && !searchTerm) {
                        var userFirstItem = {};
                        userFirstItem.id = 0;

                        switch (firstItem) {
                            case 'record_owner':
                                userFirstItem.email = '[owner]';
                                userFirstItem.full_name = $filter('translate')('Common.RecordOwner');
                                break;
                            case 'logged_in_user':
                                userFirstItem.email = '[me]';
                                userFirstItem.full_name = $filter('translate')('Common.LoggedInUser');
                                break;
                        }

                        users.push(userFirstItem)
                    }
                    return users;
                },
                lookupUserAndGroup: function (moduleId, isReadonly, searchTerm) {
                    var deferred = $q.defer();

                    if (!searchTerm) {
                        deferred.resolve([]);
                        return deferred.promise;
                    }

                    var request = {
                        module_id: moduleId,
                        is_readonly: isReadonly,
                        search_term: searchTerm
                    };

                    $http.post(config.apiUrl + 'record/lookup_user', request)
                        .then(function (records) {
                            if (!records.data) {
                                deferred.resolve([]);
                                return deferred.promise;
                            }

                            deferred.resolve(records.data);
                        })
                        .catch(function (reason) {
                            deferred.reject(reason.data);
                        });

                    return deferred.promise;
                },
                getPicklists: function (picklistTypes, refresh, modules) {

                    if (!modules) {
                        modules = $rootScope.appModules;
                    }

                    var deferred = $q.defer();
                    var picklists = {};
                    var picklistIds = [];
                    var that = this;

                    for (var i = 0; i < picklistTypes.length; i++) {
                        var picklistType = picklistTypes[i];

                        var picklistCache = $cache.get('picklist_' + picklistType);

                        if (picklistType === 0) {
                            if (picklistCache && !refresh) {
                                picklists[picklistType] = picklistCache;
                                break;
                            }

                            var modulePicklist = [];

                            for (var j = 0; j < modules.length; j++) {
                                var moduleItem = modules[j];

                                if (!moduleItem.order)
                                    continue;
                                if (moduleItem.order == 0 || moduleItem.name === 'users')
                                    continue;

                                var modulePicklistItem = {};
                                modulePicklistItem.id = parseInt(moduleItem.id) + 900000;
                                modulePicklistItem.type = 900000;
                                modulePicklistItem.order = moduleItem.order;
                                modulePicklistItem.label = {};
                                modulePicklistItem.label.en = moduleItem.label_en_singular;
                                modulePicklistItem.label.tr = moduleItem.label_tr_singular;
                                modulePicklistItem.labelStr = moduleItem['label_' + $rootScope.language + '_singular'];
                                modulePicklistItem.value = moduleItem.name;

                                modulePicklist.push(modulePicklistItem);
                            }

                            modulePicklist = $filter('orderBy')(modulePicklist, 'order');
                            picklists['900000'] = modulePicklist;
                            $cache.put('picklist_' + 900000, modulePicklist);

                            continue;
                        }

                        if (!picklistCache || refresh)
                            picklistIds.push(picklistType);
                        else
                            picklists[picklistType] = picklistCache;
                    }

                    if (picklistIds.length <= 0) {
                        deferred.resolve(picklists);
                        return deferred.promise;
                    }

                    $http.post(config.apiUrl + 'picklist/find', picklistIds)
                        .then(function (response) {
                            if (!response.data) {
                                deferred.resolve(picklists);
                                return deferred.promise;
                            }

                            for (var i = 0; i < picklistTypes.length; i++) {
                                var picklistType = picklistTypes[i];

                                if (picklistIds.indexOf(picklistType) < 0)
                                    continue;

                                var picklistItems = that.mergePicklists(response.data);
                                picklistItems = $filter('orderBy')(picklistItems, 'label_' + $rootScope.language);
                                picklists[picklistType] = picklistItems;

                                $cache.put('picklist_' + picklistType, picklists[picklistType]);
                            }

                            deferred.resolve(picklists);
                        })
                        .catch(function (reason) {
                            deferred.reject(reason.data);
                        });

                    return deferred.promise;
                },
                mergePicklists: function (picklistsResponse) {
                    var picklistItems = [];

                    if (picklistsResponse) {
                        for (var i = 0; i < picklistsResponse.length; i++) {
                            var picklistResponse = picklistsResponse[i];

                            for (var j = 0; j < picklistResponse.items.length; j++) {
                                var picklistResponseItem = picklistResponse.items[j];
                                var picklistItem = {};
                                picklistItem.type = picklistResponse.id;
                                picklistItem.id = picklistResponseItem.id;
                                picklistItem.label = {};
                                picklistItem.label.en = picklistResponseItem.label_en;
                                picklistItem.label.tr = picklistResponseItem.label_tr;
                                picklistItem.label_en = picklistResponseItem.label_en;
                                picklistItem.label_tr = picklistResponseItem.label_tr;
                                picklistItem.value = picklistResponseItem.value;
                                picklistItem.value2 = picklistResponseItem.value2;
                                picklistItem.value3 = picklistResponseItem.value3;
                                picklistItem.system_code = picklistResponseItem.system_code;
                                picklistItem.order = picklistResponseItem.order;
                                picklistItem.inactive = picklistResponseItem.inactive;
                                picklistItem.labelStr = picklistItem.label[$rootScope.language];

                                picklistItems.push(picklistItem);
                            }
                        }
                    }

                    return picklistItems;
                },
                getSlug: function (str, seperator) {
                    if (!str)
                        return '';

                    if (!seperator)
                        seperator = '_';

                    // Unicode (non-control) characters in the Latin-1 Supplement and Latin. Extended-A blocks, transliterated into ASCII characters.
                    var charmap = {
                        ' ': " ",
                        '¡': "!",
                        '¢': "c",
                        '£': "lb",
                        '¥': "yen",
                        '¦': "|",
                        '§': "SS",
                        '¨': "\"",
                        '©': "(c)",
                        'ª': "a",
                        '«': "<<",
                        '¬': "not",
                        '­': "-",
                        '®': "(R)",
                        '°': "^0",
                        '±': "+/-",
                        '²': "^2",
                        '³': "^3",
                        '´': "'",
                        'µ': "u",
                        '¶': "P",
                        '·': ".",
                        '¸': ",",
                        '¹': "^1",
                        'º': "o",
                        '»': ">>",
                        '¼': " 1/4 ",
                        '½': " 1/2 ",
                        '¾': " 3/4 ",
                        '¿': "?",
                        'À': "`A",
                        'Á': "'A",
                        'Â': "^A",
                        'Ã': "~A",
                        'Ä': '"A',
                        'Å': "A",
                        'Æ': "AE",
                        'Ç': "C",
                        'È': "`E",
                        'É': "'E",
                        'Ê': "^E",
                        'Ë': '"E',
                        'Ì': "`I",
                        'Í': "'I",
                        'Î': "^I",
                        'Ï': '"I',
                        'Ð': "D",
                        'Ñ': "~N",
                        'Ò': "`O",
                        'Ó': "'O",
                        'Ô': "^O",
                        'Õ': "~O",
                        'Ö': '"O',
                        '×': "x",
                        'Ø': "O",
                        'Ù': "`U",
                        'Ú': "'U",
                        'Û': "^U",
                        'Ü': '"U',
                        'Ý': "'Y",
                        'Þ': "Th",
                        'ß': "ss",
                        'à': "`a",
                        'á': "'a",
                        'â': "^a",
                        'ã': "~a",
                        'ä': '"a',
                        'å': "a",
                        'æ': "ae",
                        'ç': "c",
                        'è': "`e",
                        'é': "'e",
                        'ê': "^e",
                        'ë': '"e',
                        'ì': "`i",
                        'í': "'i",
                        'î': "^i",
                        'ï': '"i',
                        'ð': "d",
                        'ñ': "~n",
                        'ò': "`o",
                        'ó': "'o",
                        'ô': "^o",
                        'õ': "~o",
                        'ö': '"o',
                        '÷': ":",
                        'ø': "o",
                        'ù': "`u",
                        'ú': "'u",
                        'û': "^u",
                        'ü': '"u',
                        'ý': "'y",
                        'þ': "th",
                        'ÿ': '"y',
                        'Ā': "A",
                        'ā': "a",
                        'Ă': "A",
                        'ă': "a",
                        'Ą': "A",
                        'ą': "a",
                        'Ć': "'C",
                        'ć': "'c",
                        'Ĉ': "^C",
                        'ĉ': "^c",
                        'Ċ': "C",
                        'ċ': "c",
                        'Č': "C",
                        'č': "c",
                        'Ď': "D",
                        'ď': "d",
                        'Đ': "D",
                        'đ': "d",
                        'Ē': "E",
                        'ē': "e",
                        'Ĕ': "E",
                        'ĕ': "e",
                        'Ė': "E",
                        'ė': "e",
                        'Ę': "E",
                        'ę': "e",
                        'Ě': "E",
                        'ě': "e",
                        'Ĝ': "^G",
                        'ĝ': "^g",
                        'Ğ': "G",
                        'ğ': "g",
                        'Ġ': "G",
                        'ġ': "g",
                        'Ģ': "G",
                        'ģ': "g",
                        'Ĥ': "^H",
                        'ĥ': "^h",
                        'Ħ': "H",
                        'ħ': "h",
                        'Ĩ': "~I",
                        'ĩ': "~i",
                        'Ī': "I",
                        'ī': "i",
                        'Ĭ': "I",
                        'ĭ': "i",
                        'Į': "I",
                        'į': "i",
                        'İ': "I",
                        'ı': "i",
                        'Ĳ': "IJ",
                        'ĳ': "ij",
                        'Ĵ': "^J",
                        'ĵ': "^j",
                        'Ķ': "K",
                        'ķ': "k",
                        'Ĺ': "L",
                        'ĺ': "l",
                        'Ļ': "L",
                        'ļ': "l",
                        'Ľ': "L",
                        'ľ': "l",
                        'Ŀ': "L",
                        'ŀ': "l",
                        'Ł': "L",
                        'ł': "l",
                        'Ń': "'N",
                        'ń': "'n",
                        'Ņ': "N",
                        'ņ': "n",
                        'Ň': "N",
                        'ň': "n",
                        'ŉ': "'n",
                        'Ō': "O",
                        'ō': "o",
                        'Ŏ': "O",
                        'ŏ': "o",
                        'Ő': '"O',
                        'ő': '"o',
                        'Œ': "OE",
                        'œ': "oe",
                        'Ŕ': "'R",
                        'ŕ': "'r",
                        'Ŗ': "R",
                        'ŗ': "r",
                        'Ř': "R",
                        'ř': "r",
                        'Ś': "'S",
                        'ś': "'s",
                        'Ŝ': "^S",
                        'ŝ': "^s",
                        'Ş': "S",
                        'ş': "s",
                        'Š': "S",
                        'š': "s",
                        'Ţ': "T",
                        'ţ': "t",
                        'Ť': "T",
                        'ť': "t",
                        'Ŧ': "T",
                        'ŧ': "t",
                        'Ũ': "~U",
                        'ũ': "~u",
                        'Ū': "U",
                        'ū': "u",
                        'Ŭ': "U",
                        'ŭ': "u",
                        'Ů': "U",
                        'ů': "u",
                        'Ű': '"U',
                        'ű': '"u',
                        'Ų': "U",
                        'ų': "u",
                        'Ŵ': "^W",
                        'ŵ': "^w",
                        'Ŷ': "^Y",
                        'ŷ': "^y",
                        'Ÿ': '"Y',
                        'Ź': "'Z",
                        'ź': "'z",
                        'Ż': "Z",
                        'ż': "z",
                        'Ž': "Z",
                        'ž': "z",
                        'ſ': "s"
                    };
                    var ascii = [];
                    var ch, cp;

                    for (var i = 0; i < str.length; i++) {
                        if ((cp = str.charCodeAt(i)) < 0x180) {
                            ch = String.fromCharCode(cp);
                            ascii.push(charmap[ch] || ch);
                        }
                    }

                    str = ascii.join('');
                    str = str.replace(/[^\w\s-]/g, '').trim().toLowerCase();
                    return str.replace(/[-\s]+/g, seperator);
                },
                roundBy: function (func, number, prec) {
                    var nbr = number * Math.pow(10, prec);
                    nbr = func(nbr);

                    return nbr / Math.pow(10, prec);
                },
                parseQueryString: function (queryString) {
                    var data = {}, pairs, pair, separatorIndex, escapedKey, escapedValue, key, value;

                    if (queryString === null) {
                        return data;
                    }

                    pairs = queryString.split("&");

                    for (var i = 0; i < pairs.length; i++) {
                        pair = pairs[i];
                        separatorIndex = pair.indexOf("=");

                        if (separatorIndex === -1) {
                            escapedKey = pair;
                            escapedValue = null;
                        } else {
                            escapedKey = pair.substr(0, separatorIndex);
                            escapedValue = pair.substr(separatorIndex + 1);
                        }

                        key = decodeURIComponent(escapedKey);
                        value = decodeURIComponent(escapedValue);

                        data[key] = value;
                    }

                    return data;
                }
            }
        }])

    .factory('convert', ['helper', function (helper) {
        return {
            fromMsDate: function (str) {
                str = helper.getTime(str);

                return new Date(str);
            },
            toMsDate: function (dt) {
                return '/Date(' + dt.getTime() + ')/';
            }
        }
    }])

    .factory('exportFile', function () {
        return {
            excel: function (jsonData, name) {
                var columnLength = jsonData[0].length - 1;
                var rowLength = 0;

                var ctx = {
                    columns: '',
                    rows: ''
                };

                var template = {
                    excel: '<html xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:x="urn:schemas-microsoft-com:office:excel" xmlns="http://www.w3.org/TR/REC-html40"><head><meta http-equiv="Content-Type" content="text/html; charset=UTF-8"><!--[if gte mso 9]><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet><x:Name>{worksheet}</x:Name><x:WorksheetOptions><x:DisplayGridlines/></x:WorksheetOptions></x:ExcelWorksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml><![endif]--></head><body><table>{table}</table></body></html>',
                    excelML: '<?xml version="1.0"?>'
                    + '<?mso-application progid="Excel.Sheet"?>'
                    + '<ss:Workbook xmlns:="urn:schemas-microsoft-com:office:spreadsheet" '
                    + 'xmlns:o="urn:schemas-microsoft-com:office:office" '
                    + 'xmlns:x="urn:schemas-microsoft-com:office:excel" '
                    + 'xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet" '
                    + 'xmlns:html="http://www.w3.org/TR/REC-html40">'
                    + '<ss:Styles>'
                    + '<ss:Style ss:ID="1">'
                    + '<ss:Font ss:Bold="1"/>'
                    + '</ss:Style>'
                    + '</ss:Styles>'
                    + '<ss:Worksheet ss:Name="Sheet1">'
                    + '<ss:Table>'
                    + '{columns}{rows}'
                    + '</ss:Table>'
                    + '</ss:Worksheet>'
                    + '</ss:Workbook>',
                    rowOpen: "<ss:Row>",
                    rowClose: "</ss:Row>",
                    dataOpenString: '<ss:Data ss:Type="String">',
                    dataOpenNumber: '<ss:Data ss:Type="Number">',
                    dataClose: '</ss:Data>',
                    column: '<ss:Column ss:Width="80"/>',
                    cellOpen: '<ss:Cell>',
                    cellClose: '</ss:Cell>'
                };

                var fixCSVField = function (value) {
                    if (value === null || value === undefined)
                        return '';

                    var fixedValue = "<![CDATA[" + value + "]]>";
                    var valueStr = value.toString();
                    var addQuotes = (valueStr.indexOf(',') > -1) || (valueStr.indexOf('\r') > -1) || (valueStr.indexOf('\n') > -1);
                    var replaceDoubleQuotes = (valueStr.indexOf('"') > -1);

                    if (replaceDoubleQuotes) {
                        fixedValue = fixedValue.replace(/"/g, '""');
                    }
                    if (addQuotes || replaceDoubleQuotes) {
                        fixedValue = '"' + fixedValue + '"';
                    }
                    return fixedValue;
                };

                var base64 = function (s) {
                    return window.btoa(unescape(encodeURIComponent(s)));
                };

                var format = function (s, c) {
                    return s.replace(/{(\w+)}/g, function (m, p) {
                        return c[p];
                    });
                };

                ///Generate columns
                for (var c = 0; c < columnLength; c++) {
                    ctx.columns += template.column;
                }

                /// Generate rows and cells.
                for (var i = 0, row; row = jsonData[i]; i++) {
                    ctx.rows += template.rowOpen;
                    rowLength = jsonData[i].length;
                    for (var j = 0; j < rowLength; j++) {
                        var col = row[j];
                        ctx.rows += template.cellOpen;
                        if (typeof col === 'number')
                            ctx.rows += template.dataOpenNumber;
                        else
                            ctx.rows += template.dataOpenString;
                        ctx.rows += fixCSVField(col);
                        ctx.rows += template.dataClose;
                        ctx.rows += template.cellClose
                    }
                    ctx.rows += template.rowClose;
                }

                var data = base64(format(template.excelML, ctx));

                var byteCharacters = atob(data);

                var byteNumbers = new Array(byteCharacters.length);
                for (var k = 0; k < byteCharacters.length; k++) {
                    byteNumbers[k] = byteCharacters.charCodeAt(k);
                }

                var byteArray = new Uint8Array(byteNumbers);
                var blob = new Blob([byteArray], {type: 'application/octet-stream'});

                /// save it by file save dialog.
                saveAs(blob, name);
            }
        }
    })

    .factory('officeHelper', ['$http', 'config', function ($http, config) {
        return {
            officeTenantInfo: function () {
                return $http.get(config.apiUrl + 'User/ActiveDirectoryInfo');
            }
        }
    }]);


//Extension methods
String.prototype.toUpperCaseTurkish = function () {
    return this.replace(/i/g, 'İ').toLocaleUpperCase();
};

String.prototype.toLowerCaseTurkish = function () {
    return this.replace(/I/g, 'ı').toLocaleLowerCase();
};

String.prototype.replaceAll = function (find, replace) {
    function escapeRegExp(str) {
        return str.replace(/([.*+?^=!:${}()|\[\]\/\\])/g, '\\$1');
    }

    return this.replace(new RegExp(escapeRegExp(find), 'g'), replace);
};

Array.prototype.getUnique = function () {
    var u = {}, a = [];

    for (var i = 0, l = this.length; i < l; ++i) {
        if (u.hasOwnProperty(this[i]))
            continue;

        a.push(this[i]);
        u[this[i]] = 1;
    }

    return a;
};