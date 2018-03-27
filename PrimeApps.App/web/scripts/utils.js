'use strict';

angular.module('ofisim')

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
        return $cacheFactory('ofisim');
    }])

    .factory('sipHelper', ['$rootScope', '$timeout', '$filter', '$localStorage', '$sessionStorage', '$q', '$http', 'config', '$cache', '$state', '$window', '$popover',
        function ($rootScope, $timeout, $filter, $localStorage, $sessionStorage, $q, $http, config, $cache, $state, $window, $popover) {

            function destroySessionEvents(session) {
                //CLEAN EVENT HANDLERS
                if (session) {
                    session.off('bye');
                    session.off('progress');
                    session.off('accepted');
                    session.off('rejected');
                    session.off('cancel');
                    session.off('terminated');
                    session.off('muted');
                    session.off('unmuted');
                    session.off('hold');
                    session.off('unhold');
                    session.off('refer');
                    session.off('failed');
                    session = null;

                }
                //Get sure about nulling - destroying this session
                if ($rootScope.sipUser.session) {
                    $timeout(function () {
                        $rootScope.sipUser.session = null;
                    });
                }
            }

            function soundPlay(soundId, status) {
                var sound = document.getElementById(soundId);
                if (status == 'play') {
                    sound.play();
                    sound.loop = true;
                } else {
                    sound.pause();
                    sound.currentTime = 0;
                }
            }

            //Call type -> INBOUND OR OUTBOUND
            var that = this;

            function createSessionEvents(session, callType) {

                //HANGUP - SESSION BYE
                session.on('bye', function (request) {
                    //console.log('BYE');
                });
                session.on('failed', function (request) {
                    //console.log(request);
                });
                //PROGRESSING OUTBOUND
                session.on('progress', function (response) {
                    $timeout(function () {
                        if (callType == 'OUTBOUND') {
                            $rootScope.sipUser.lineInfo.PhoneStatus = $filter('translate')('Setup.Phone.Ringing');
                            $rootScope.sipUser.lineInfo.State = 'Ringing';
                            soundPlay('DialSound', 'play');
                        }
                        else {
                            $rootScope.sipUser.lineInfo.PhoneStatus = $filter('translate')('Setup.Phone.IncomingCall');
                            that.showSipPhone('call');
                            $rootScope.sipUser.lineInfo.State = $filter("translate")("Setup.Phone.IncomingCall");
                            $rootScope.sipUser.activePhoneScreen = 'connectingScreen';

                        }
                    });
                });

                // //WHILE RINGING - BYE EVENT
                session.on('cancel', function () {

                    $timeout(function () {
                        soundPlay('IncomingSound', 'pause');
                        soundPlay('DialSound', 'pause');
                        $rootScope.sipUser.lineInfo.PhoneStatus = $filter('translate')('Setup.Phone.CanceledByUser');
                        $rootScope.sipUser.lineInfo.State = 'Cancel';

                    });

                });

                //ANSWERED EVENT
                session.on('accepted', function (data) {
                    $timeout(function () {
                        if ($rootScope.sipUser.lineInfo.State == 'IncomingCall')
                            $rootScope.transferShow = true;
                        else
                            $rootScope.transferShow = false;

                        $rootScope.sipUser.lineInfo.PhoneStatus = $filter("translate")("Setup.Phone.CallActive");
                        $rootScope.sipUser.lineInfo.State = 'Active';

                        soundPlay('DialSound', 'pause');
                        soundPlay('IncomingSound', 'pause');

                        var element = document.getElementsByTagName('video')[0];
                        var stream = session.mediaHandler.getRemoteStreams()[0];
                        if (typeof element.srcObject !== 'undefined') {
                            element.srcObject = stream;
                        } else if (typeof element.mozSrcObject !== 'undefined') {
                            element.mozSrcObject = stream;
                        } else if (typeof element.src !== 'undefined') {
                            element.src = URL.createObjectURL(stream);
                        } else {
                            console.log('Error attaching stream to element.');
                        }

                        //START TIMER CALL
                        startCallTimer();
                    });
                });

                //REJECTED (Hangup before answering like busy)
                session.on('rejected', function (response, cause) {
                    if ($rootScope.sipUser.session === session) {

                        $timeout(function () {

                                if (callType === 'OUTBOUND') { //IF OUR CALLING NUMBER REJECTS US !
                                    soundPlay('DialSound', 'pause');
                                    soundPlay('BusySound', 'play');

                                    if (callType == 'OUTBOUND') { //IF OUR CALLING NUMBER REJECTS US !
                                        $rootScope.sipUser.lineInfo.PhoneStatus = response.reason_phrase;
                                        $rootScope.sipUser.lineInfo.State = 'Reject';
                                    }
                                }
                            }
                        );
                    }
                });

                //MUTED
                session.on('muted', function (data) {
                    $timeout(function () {
                        $rootScope.sipUser.lineInfo.PhoneStatus = $filter('translate')('Setup.Phone.CallMuted');
                        $rootScope.sipUser.lineInfo.State = 'Active';
                        $rootScope.sipUser.lineInfo.IsMuted = true;
                    });
                });
                //UNMUTED
                session.on('unmuted', function (data) {

                    $timeout(function () {
                        soundPlay('DialSound', 'pause');
                        soundPlay('IncomingSound', 'pause');
                        $rootScope.sipUser.lineInfo.PhoneStatus = $filter('translate')('Setup.Phone.CallActive');
                        $rootScope.sipUser.lineInfo.State = 'Active';
                        $rootScope.sipUser.lineInfo.IsMuted = false;
                    });
                });

                //HOLD
                session.on('hold', function (data) {

                    $timeout(function () {
                        $rootScope.sipUser.lineInfo.PhoneStatus = $filter('translate')('Setup.Phone.Hold');
                        $rootScope.sipUser.lineInfo.State = 'Active';
                        $rootScope.sipUser.lineInfo.IsHold = true;
                    });
                });
                //UNHOLD
                session.on('unhold', function (data) {

                    $timeout(function () {
                        soundPlay('DialSound', 'pause');
                        soundPlay('IncomingSound', 'pause');
                        $rootScope.sipUser.lineInfo.PhoneStatus = $filter('translate')('Setup.Phone.CallActive');
                        $rootScope.sipUser.lineInfo.State = 'Active';
                        $rootScope.sipUser.lineInfo.IsHold = false;
                    });
                });

                //TRANSFER
                session.on('refer', session.followRefer(function (data) {

                }));
                //CALL ENDED EVENT (ALWAYS LAST POINT OF EVENTS LIFECYCLE, CAN BE SURE TO DISMISS GARBAGES) - Give some time to agent and softphone for breathing !
                session.on('terminated', function (message, cause) {

                    $timeout(function () {

                        //$rootScope.sipUser.lineInfo.PhoneStatus = 'Terminated..';

                        $rootScope.sipUser.session = null;
                        $rootScope.sipUser.lineInfo.PhoneStatus = $filter('translate')('Setup.Phone.Hangup');
                        $rootScope.sipUser.lineInfo.State = 'Hangup';
                        $timeout(function () {
                            soundPlay('BusySound', 'pause');
                            if ($rootScope.sipUser.userAgent.isRegistered()) {
                                $rootScope.sipUser.lineInfo.PhoneStatus = $filter('translate')('Setup.Phone.Ready');
                                $rootScope.sipUser.lineInfo.State = 'Ready';
                                $rootScope.sipUser.lineInfo.TalkingTimer = 0;
                                $rootScope.sipUser.lineInfo.ActiveNumber = null;
                                $rootScope.sipUser.activePhoneScreen = 'readyScreen';
                                $rootScope.sipUser.lineInfo.ModuleName = null;
                                $rootScope.sipUser.lineInfo.RecordId = null;
                                $rootScope.sipUser.lineInfo.TalkingRecordInfo = null;
                                $rootScope.transferShow = false;
                                $rootScope.sipUser.lineInfo.IsMuted = false;
                                $rootScope.sipUser.lineInfo.IsHold = false;
                            }
                        }, 1000);

                        destroySessionEvents(session);

                        //CLEAN TIMER OBJECT
                        endCallTimer();

                    }, 2000);
                });
            }

            var timeoutTimer = null;

            function startCallTimer() {
                $rootScope.sipUser.lineInfo.TalkingTimer++;
                timeoutTimer = $timeout(startCallTimer, 1000);
            }

            function endCallTimer() {
                $timeout.cancel(timeoutTimer);
                $rootScope.sipUser.lineInfo.TalkingTimer = 0;
            }

            function findRecordGoDetail(moduleName, fieldName, searchValue) {

                var module = $filter('filter')($rootScope.modules, { name: moduleName }, true)[0];
                var primaryField = $filter('filter')(module.fields, { primary: true })[0];

                var selectedFields = [];
                selectedFields.push('id');
                selectedFields.push(primaryField.name);

                var findRequest = {
                    fields: selectedFields,
                    filters: [],
                    sort_field: 'id',
                    sort_direction: 'asc',
                    limit: 1,
                    offset: 0
                };

                findRequest.filters.push({ field: fieldName, operator: 'equals', value: searchValue, no: 1 });

                $http.post(config.apiUrl + 'record/find/' + moduleName, findRequest)
                    .then(function (response) {
                        var callerName, recordId;

                        if (response.data && response.data.length) {
                            var record = response.data[0];

                            callerName = record[primaryField.name];
                            recordId = record['id'];
                        }
                        else {
                            callerName = $filter('translate')('Common.NewRecord');
                            recordId = 0;
                        }

                        $timeout(function () {
                            $rootScope.sipUser.lineInfo.ModuleName = moduleName;
                            $rootScope.sipUser.lineInfo.RecordId = recordId;
                            $rootScope.sipUser.lineInfo.TalkingRecordInfo = callerName;
                            $rootScope.sipUser.lineInfo.Field = fieldName;
                            $rootScope.sipUser.lineInfo.Value = searchValue;
                        });
                    });
            }

            return {
                register: function () {
                    var that = this;

                    if (!$rootScope.sipUser.userAgent) {
                        $rootScope.sipUser.userAgent = new SIP.UA({
                            uri: $rootScope.sipUser.Extension + '@' + $rootScope.sipUser.SipUri,
                            wsServers: [$rootScope.sipUser.Server],
                            authorizationUser: $rootScope.sipUser.Extension,
                            password: $rootScope.sipUser.Password,
                            log: {
                                builtinEnabled: false,
                                level: 0
                            },
                            rtcpMuxPolicy: 'negotiate',
                        });
                        $rootScope.sipUser.userAgent.on('connecting', function (args) {
                            $rootScope.sipUser.events.IsConnecting = true;
                            $rootScope.sipUser.lineInfo.PhoneStatus = $filter('translate')('Setup.Phone.Connecting');
                            $rootScope.sipUser.lineInfo.State = 'Connecting';
                        });

                        $rootScope.sipUser.userAgent.on('unregistered', function (response, cause) {
                            $timeout(function () {
                                $rootScope.sipUser.events.IsRegistered = false;
                                $rootScope.sipUser.events.IsConnecting = false;
                                $rootScope.sipUser.lineInfo.PhoneStatus = $filter('translate')('Setup.Phone.Unregistered');
                                $rootScope.sipUser.lineInfo.State = 'Unregistered';
                            });
                        });

                        $rootScope.sipUser.userAgent.on('registered', function () {
                            $timeout(function () {
                                $rootScope.sipUser.events.IsRegistered = true;
                                $rootScope.sipUser.events.IsConnecting = false;

                                $rootScope.sipUser.lineInfo.PhoneStatus = $filter('translate')('Setup.Phone.Ready');
                                $rootScope.sipUser.lineInfo.State = 'Ready';
                            });
                        });

                        $rootScope.sipUser.userAgent.on('disconnected', function () {

                            $timeout(function () {
                                $rootScope.sipUser.events.IsRegistered = false;
                                $rootScope.sipUser.events.IsConnecting = false;
                                $rootScope.sipUser.lineInfo.PhoneStatus = $filter('translate')('Setup.Phone.Disconnected');
                                $rootScope.sipUser.lineInfo.State = 'Disconnected';
                            });

                        });

                        $rootScope.sipUser.userAgent.on('registrationFailed', function (cause, response) {
                            $timeout(function () {
                                $rootScope.sipUser.events.IsRegistered = false;
                                $rootScope.sipUser.events.IsConnecting = false;
                                $rootScope.sipUser.events.RegistrationError = true;
                                $rootScope.sipUser.lineInfo.PhoneStatus = $filter('translate')('Setup.Phone.RegistrationError');
                                $rootScope.sipUser.lineInfo.State = 'RegistrationError';
                            });
                        });

                        $rootScope.sipUser.userAgent.on('invite', function (session) {
                            //CHECK THE LINE FOR ACTIVE CALLS - IF ANY ACTIVE CALL NO NEW SESSIONS ARE PERMITTED !
                            if ($rootScope.sipUser.session === null) {
                                //RINGING EVENT
                                $timeout(function () {
                                    $rootScope.sipUser.lineInfo.PhoneStatus = $filter('translate')('Setup.Phone.IncomingCall');
                                    that.showSipPhone('call');
                                    $rootScope.sipUser.lineInfo.State = 'IncomingCall';
                                    $rootScope.sipUser.activePhoneScreen = 'connectingScreen';
                                    $rootScope.sipUser.lineInfo.TalkingNumber = session.request.from.friendlyName;
                                });

                                createSessionEvents(session, 'INBOUND');

                                //SET SESSION GLOBALLY FOR GLOBAL ACCESS ! IMPORTANT
                                $timeout(function () {
                                    soundPlay('IncomingSound', 'play');
                                    //Set CALL SESSION TO GLOBAL OBJECT FOR INSTANCE TIME
                                    $rootScope.sipUser.session = session;

                                    //CALLING - ACTIVE - INCOMING NUMBER
                                    $rootScope.sipUser.lineInfo.ActiveNumber = session.request.from.friendlyName;

                                    //CHECK IF ANY EXISTING DATA ON MODULE WITH FIELD NAME MOBILE
                                    //ONLY PHONE FIELD ALLOWED FOR SEARCHDATA FOR NOW!
                                    findRecordGoDetail($rootScope.sipUser.RecordDetailModuleName, $rootScope.sipUser.RecordDetailPhoneFieldName, $rootScope.sipUser.lineInfo.TalkingNumber);
                                });
                            }
                            else {
                                //IF NEED NEW CALL ALERT FROM SECOND LINE THIRD LINE ETC WHILE TALKING - DO IT HERE FOR NOW WE DONT ACCEPT WAITING CALLERS OR SECOND LINE FEATURE, SENDING BUSY SIGNAL
                                //SEND BUSY SIGNAL TO NEW CALLER
                                var options = {
                                    statusCode: 600
                                };
                                session.reject(options);
                            }
                        });


                        //INVITE EVENT REMOVE FROM EVENT HANDLER MINIMIZE RESOURCE USAGE
                        $rootScope.sipUser.userAgent.off('invite', function (session) {
                            $timeout(function () {
                                $rootScope.sipUser.session = null;
                                if ($rootScope.sipUser.userAgent.isRegistered()) {
                                    $rootScope.sipUser.lineInfo.PhoneStatus = $filter('translate')('Setup.Phone.Ready');
                                    $rootScope.sipUser.lineInfo.State = 'Ready';
                                }
                                else {
                                    $rootScope.sipUser.lineInfo.PhoneStatus = $filter('translate')('Setup.Phone.Hangup');
                                    $rootScope.sipUser.lineInfo.State = 'Hangup';
                                }

                            });
                        })
                    }
                },
                unRegister: function () {
                    var options = {
                        'all': true
                    };
                    $rootScope.sipUser.userAgent.unregister(options);
                    $rootScope.sipUser.userAgent = null;
                    destroySessionEvents($rootScope.sipUser.session);
                },
                isRegistered: function () {
                    if ($rootScope.sipUser.userAgent.isRegistered()) {
                        $rootScope.sipUser.IsRegistered = true;
                    }
                    else {
                        $rootScope.sipUser.IsRegistered = false;
                    }
                },
                getUA: function () {
                    return $rootScope.sipUser.userAgent;
                },
                dial: function (number, videoCall) {
                    var options = {
                        media: {
                            constraints: {
                                audio: true,
                                video: false
                            }
                        }
                    };

                    var session = $rootScope.sipUser.userAgent.invite(number, options);

                    createSessionEvents(session, 'OUTBOUND');

                    $timeout(function () {
                        //Set CALL SESSION TO GLOBAL OBJECT FOR INSTANCE TIME
                        $rootScope.sipUser.session = session;

                    });
                },
                transfer: function (transferNumber) {
                    $rootScope.sipUser.session.refer(transferNumber);
                },
                mute: function () {
                    $rootScope.sipUser.session.mute();
                },
                hold: function () {
                    $rootScope.sipUser.session.hold();
                },
                unmute: function () {
                    $rootScope.sipUser.session.unmute();
                },
                unhold: function () {
                    $rootScope.sipUser.session.unhold();
                },
                isOutGoingRequest: function (session) {
                    if (session.request) {
                        if (!session.request.server_transaction) {
                            return true;
                        }
                        else {
                            return false;
                        }
                    }
                    return null;
                },
                goRecordDetail: function (moduleName, recordId) {
                    if ($rootScope.sipUser.IsAutoRecordDetail === 'true') {
                        //$state.go('app.crm.module',{type:moduleName,id: recordId});
                        $window.location.href = '#/app/crm/module/' + moduleName + '?id=' + recordId;
                    }
                },
                findRecordGoDetail: function (moduleName, fieldName, searchValue) {
                    findRecordGoDetail(moduleName, fieldName, searchValue);
                },
                showSipPhone: function (call) {
                    if (call == undefined) {
                        if (!$rootScope.sipPhone) {
                            $rootScope.sipPhone = {};
                            $rootScope.sipPhone[$rootScope.app] = $rootScope.sipPhone[$rootScope.app] || $popover(angular.element(document.getElementById('sipPhoneBtn')), {
                                templateUrl: cdnUrl + 'web/views/app/phone/sipPhone.html',
                                placement: 'bottom',
                                animation: "am-flip-x",
                                scope: $rootScope,
                                container: 'body',
                                autoClose: false,
                                show: true,
                                delay: { show: 500, hide: 100 }
                            });
                        }
                    } else {
                        if ($rootScope.sipPhone) {
                            $rootScope.sipPhone[$rootScope.app].show();
                        } else {
                            this.showSipPhone();
                        }
                    }

                }
            }
        }])

    .factory('helper', ['$rootScope', '$timeout', '$filter', '$localStorage', '$sessionStorage', '$q', '$http', 'config', '$cache',
        function ($rootScope, $timeout, $filter, $localStorage, $sessionStorage, $q, $http, config, $cache) {
            return {
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
                hasPermission: function (moduleName, operation) {
                    if (moduleName === 'related_to')
                        moduleName = 'activities';

                    if (moduleName === 'stage_history')
                        moduleName = 'opportunities';

                    var module = $filter('filter')($rootScope.modules, { name: moduleName }, true)[0];

                    if (!module) return false;

                    var permission = $filter('filter')($rootScope.user.profile.Permissions, { ModuleId: module.id }, true)[0];

                    if (!permission)
                        return false;

                    return permission[operation];
                },
                hasRecordEditPermission: function (record) {
                    if ($rootScope.user.profile.HasAdminRights)
                        return true;

                    if (record.shared_users && record.shared_users.indexOf($rootScope.user.ID) > -1) {
                        if (record.shared_users_edit && record.shared_users_edit.indexOf($rootScope.user.ID) > -1)
                            return true;

                        if (record.shared_user_groups_edit && $rootScope.user.groups.length) {
                            for (var userGroupId in record.shared_user_groups_edit) {
                                if ($rootScope.user.groups.indexOf(userGroupId) > -1)
                                    return true;
                            }
                        }

                        return false;
                    }

                    if (record.shared_user_groups && $rootScope.user.groups.length) {
                        if (record.shared_users_edit && record.shared_users_edit.indexOf($rootScope.user.ID) > -1)
                            return true;

                        if (record.shared_user_groups_edit) {
                            for (var userGroupId in record.shared_user_groups_edit) {
                                if ($rootScope.user.groups.indexOf(userGroupId) > -1)
                                    return true;
                            }
                        }

                        for (var userGroupId in record.shared_user_groups) {
                            if ($rootScope.user.groups.indexOf(userGroupId) > -1)
                                return false;
                        }
                    }

                    return true;
                },
                hasDocumentsPermission: function (operation) {
                    var permission = $filter('filter')($rootScope.user.profile.Permissions, { Type: 1 })[0];

                    if (!permission)
                        return false;

                    return permission[operation];
                },
                hasAdminRights: function () {
                    return $rootScope.user.profile.HasAdminRights;
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
                lookupUser: function (searchTerm, firstItem, includeInactiveUsers) {
                    var deferred = $q.defer();

                    if (!searchTerm && !firstItem) {
                        deferred.resolve([]);
                        return deferred.promise;
                    }

                    var findRequest = {
                        fields: ['id', 'full_name', 'email', 'is_active'],
                        filters: [
                            {
                                field: 'full_name',
                                operator: 'starts_with',
                                value: searchTerm,
                                no: 1
                            },
                            {
                                field: 'is_active',
                                operator: 'equals',
                                value: true,
                                no: 2
                            }
                        ],
                        limit: 20,
                        sort_field: 'full_name',
                        sort_direction: 'asc'
                    };

                    //includes users whose are not active but records owners
                    if (includeInactiveUsers && includeInactiveUsers == true) {
                        for (var i = 0; i < findRequest.filters.length; i++) {
                            var obj = findRequest.filters[i];

                            if (obj.field == 'is_active') {
                                var itemIndex = findRequest.filters.indexOf(obj);
                                findRequest.filters.splice(itemIndex, 1);
                            }
                        }
                    }

                    if (!searchTerm) {
                        findRequest.filters.shift();
                        findRequest.filters[0].no = 1;
                    }

                    $http.post(config.apiUrl + 'record/find/users', findRequest)
                        .then(function (response) {
                            response = response.data;
                            if (!response) {
                                deferred.resolve([]);
                                return deferred.promise;
                            }

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

                            for (var i = 0; i < response.length; i++) {
                                var userRecord = response[i];

                                var user = {};
                                user.id = userRecord.id;
                                user.email = userRecord.email;
                                user.full_name = userRecord.full_name;

                                users.push(user)
                            }

                            deferred.resolve(users);
                        })
                        .catch(function (reason) {
                            deferred.reject(reason.data);
                        });

                    return deferred.promise;
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
                getPicklists: function (picklistTypes, refresh) {
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

                            for (var j = 0; j < $rootScope.modules.length; j++) {
                                var moduleItem = $rootScope.modules[j];

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
                                picklistItem.value2 = picklistResponseItem.value_2;
                                picklistItem.value3 = picklistResponseItem.value_3;
                                picklistItem.system_code = picklistResponseItem.system_code;
                                picklistItem.order = picklistResponseItem.order;
                                picklistItem.inactive = picklistResponseItem.inactive;
                                picklistItem.labelStr = picklistItem.label[$rootScope.user.tenantLanguage];

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
                var blob = new Blob([byteArray], { type: 'application/octet-stream' });

                /// save it by file save dialog.
                saveAs(blob, name);
            }
        }
    })

    .factory('officeHelper', ['$http','config', function ($http, config) {
        return {
            officeTenantInfo: function () {
                return $http.get(config.apiUrl + 'User/ActiveDirectoryInfo');
            }
        }
    }])

    .factory('components', ['$rootScope', '$timeout', '$filter', '$localStorage', '$sessionStorage', '$q', '$http', 'config', '$cache', 'ngToast', 'ModuleService',
        function($rootScope, $timeout, $filter, $localStorage, $sessionStorage, $q, $http, config, $cache, ngToast, ModuleService){
            return {
                run: function(place, type, scope, record) {
                    var components = $filter('orderBy')($filter('filter')(scope.module.components, function(component){
                        return component.place === place && component.type === type && (component.module_id === scope.module.id || component.module_id === 0)
                    }, true), 'order');
                    for (var i = 0; i < components.length; i++) {
                        var component = components[i];
                        eval(component.content);
                    }
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