angular.module('primeapps')
    .controller('SipPhoneController', ['$rootScope', '$scope', 'ngToast', '$filter', '$timeout', 'helper', 'sipHelper', '$popover', '$location', '$state', '$stateParams', '$q', '$window', '$interval', '$localStorage', '$cache', 'config',
        function ($rootScope, $scope, ngToast, $filter, $timeout, helper, sipHelper, $popover, $location, $state, $stateParams, $q, $window, $interval, $localStorage, $cache, config) {
            $scope.isInbound = false;
            $scope.transferNumber = "";
            if ($rootScope.phoneSettings.sipUsers) {
                angular.forEach($rootScope.phoneSettings.sipUsers, function (sipUser) {
                    if (!sipUser.name) {
                        var user = $filter('filter')($rootScope.users, {id: parseInt(sipUser.userId)}, true)[0];
                        sipUser.name = user.FullName;
                    }
                });
            }

            $scope.registerUnregister = function () {
                if ($rootScope.sipUser.userAgent && $rootScope.sipUser.userAgent.isRegistered()) {
                    sipHelper.unRegister();
                }
                else {
                    sipHelper.register();
                }
            };
            $scope.setToInCall = function () {
                $scope.callAccepting = true;
                var session = $rootScope.sipUser.session;

                if (session) {
                    $scope.isInbound = sipHelper.isOutGoingRequest(session);

                    if (sipHelper.isOutGoingRequest(session) === false && !session.hasAnswer) {
                        var options = {
                            media: {
                                constraints: {
                                    audio: true,
                                    video: false
                                }
                            }
                        };

                        session.accept(options);

                        //Open Record Detail IF any on answer
                        var moduleName = $rootScope.sipUser.lineInfo.ModuleName;
                        var recordId = $rootScope.sipUser.lineInfo.RecordId;

                        if (moduleName && recordId) {
                            sipHelper.goRecordDetail(moduleName, recordId);
                        }

                        $scope.callAccepting = false;
                    }
                }
                else {
                    if ($rootScope.sipUser.numberToDial.length > 2) {
                        $rootScope.sipUser.lineInfo.PhoneStatus = $filter('translate')('Setup.Phone.Dialing');
                        $rootScope.sipUser.lineInfo.State = 'Dialing';
                        $rootScope.sipUser.lineInfo.TalkingNumber = $rootScope.sipUser.numberToDial;
                        sipHelper.dial($rootScope.sipUser.lineInfo.TalkingNumber, false);

                        $scope.changePhoneScreen('connectingScreen');
                    }
                    else {
                        ngToast.create({content: $filter('translate')('Setup.Phone.NoNumber'), className: 'warning'});
                    }

                    $scope.callAccepting = false;
                }

            };

            $scope.dialOrAnswer = function () {
                $timeout($scope.setToInCall, 500);
            };

            $scope.hangup = function () {
                var incomingSound = document.getElementById('IncomingSound');
                incomingSound.pause();
                var session = $rootScope.sipUser.session;

                if (session) {
                    if (sipHelper.isOutGoingRequest(session) === true) {
                        if (session.hasAnswer) {
                            $rootScope.sipUser.lineInfo.PhoneStatus = $filter('translate')('Setup.Phone.EndedCall');
                            session.bye();
                        }
                        else {
                            //Bug on cancel event - so manually set to ready state
                            if ($rootScope.sipUser.lineInfo.State === 'Ringing') {
                                session.cancel();
                            }

                            session.close();
                            session = null;
                            $rootScope.sipUser.session = null;

                            $rootScope.sipUser.lineInfo.PhoneStatus = $filter('translate')('Setup.Phone.Canceling');

                            $timeout(function () {
                                $rootScope.sipUser.lineInfo.PhoneStatus = $filter('translate')('Setup.Phone.Ready');
                                $rootScope.sipUser.lineInfo.State = 'Ready';
                                $scope.changePhoneScreen('readyScreen');
                            }, 2000)
                        }
                    }
                    else if (sipHelper.isOutGoingRequest(session) === false) {
                        if (session.hasAnswer) {
                            $rootScope.sipUser.lineInfo.PhoneStatus = $filter('translate')('Setup.Phone.EndedCall');
                            session.bye();
                        }
                        else {
                            $rootScope.sipUser.lineInfo.PhoneStatus = $filter('translate')('Setup.Phone.RejectedCall');
                            session.reject();
                        }
                    }

                    $rootScope.sipUser.session = null;
                }
                else {
                    //Before creating any session - cancel by user
                    $rootScope.sipUser.lineInfo.PhoneStatus = $filter('translate')('Setup.Phone.Canceling');

                    $timeout(function () {
                        $rootScope.sipUser.lineInfo.PhoneStatus = $filter('translate')('Setup.Phone.Ready');
                        $rootScope.sipUser.lineInfo.State = 'Ready';
                        $scope.changePhoneScreen('readyScreen');
                    }, 2000)
                }
            };

            $scope.changePhoneScreen = function (screen) {
                $rootScope.sipUser.activePhoneScreen = screen;
            };

            $scope.clearPhoneInput = function (event) {
                $rootScope.sipUser.numberToDial = $rootScope.sipUser.numberToDial.substring(0, $rootScope.sipUser.numberToDial.length - 1);
                $timeout($scope.addAnimationToButton(event), 200);
                $rootScope.sipUser.lineInfo.TalkingRecordInfo = null;

                if ($scope.sipUser.numberToDial.length > 10) {
                    sipHelper.findRecordGoDetail($rootScope.sipUser.RecordDetailModuleName, $rootScope.sipUser.RecordDetailPhoneFieldName, $scope.sipUser.numberToDial);
                }
            };

            var contextClass = ($window.AudioContext ||
            $window.webkitAudioContext ||
            $window.mozAudioContext ||
            $window.oAudioContext ||
            $window.msAudioContext);

            if (contextClass && !$rootScope.context) {
                // Web Audio API is available.
                $rootScope.context = new contextClass();
            }

            $scope.addAnimationToButton = function (element) {
                angular.element(element).removeClass('clicked');
                $timeout(function () {
                    angular.element(element).addClass('clicked');
                }, 100);
            };
            $scope.dialTone = function (freq1, freq2) {
                $scope.stopDialNumber();
                $scope.oscillator1 = $rootScope.context.createOscillator();
                $scope.oscillator1.type = 0;
                $scope.oscillator1.frequency.value = freq1;
                var gainNode = $rootScope.context.createGain ? $rootScope.context.createGain() : $rootScope.context.createGainNode();
                $scope.oscillator1.connect(gainNode, 0, 0);
                gainNode.connect($rootScope.context.destination);
                gainNode.gain.value = .1;
                $scope.oscillator1.start ? $scope.oscillator1.start(0) : $scope.oscillator1.noteOn(0);
                $scope.oscillator2 = $rootScope.context.createOscillator();
                $scope.oscillator2.type = 0;
                $scope.oscillator2.frequency.value = freq2;
                gainNode = $rootScope.context.createGain ? $rootScope.context.createGain() : $rootScope.context.createGainNode();
                $scope.oscillator2.connect(gainNode);
                gainNode.connect($rootScope.context.destination);
                gainNode.gain.value = .1;
                $scope.oscillator2.start ? $scope.oscillator2.start(0) : $scope.oscillator2.noteOn(0);
                $timeout(function () {
                    $scope.stopDialNumber();
                }, 200);


            };
            $scope.putDialNumber = function (number, freq1, freq2, $event) {
                $rootScope.sipUser.numberToDial += number.toString();
                $scope.addAnimationToButton($event.currentTarget);
                $scope.dialTone(freq1, freq2);
                $scope.findRecord();
            };
            $scope.findRecord = function (val, event) {

                $rootScope.sipUser.lineInfo.TalkingRecordInfo = null;
                if (val) $scope.sipUser.numberToDial = val;
                if ($scope.sipUser.numberToDial.length > 10) {
                    sipHelper.findRecordGoDetail($rootScope.sipUser.RecordDetailModuleName, $rootScope.sipUser.RecordDetailPhoneFieldName, $scope.sipUser.numberToDial);
                }
                if (event) {

                    switch (event.keyCode) {
                        case 97:
                            var element = document.getElementsByClassName("number-dig")[0];
                            $scope.addAnimationToButton(element);
                            $scope.dialTone(697, 1209);
                            break;
                        case 98:
                            var element = document.getElementsByClassName("number-dig")[1];
                            $scope.addAnimationToButton(element);
                            $scope.dialTone(697, 1336);
                            break;
                        case 99:
                            var element = document.getElementsByClassName("number-dig")[2];
                            $scope.addAnimationToButton(element);
                            $scope.dialTone(697, 1477);
                            break;
                        case 100:
                            var element = document.getElementsByClassName("number-dig")[3];
                            $scope.addAnimationToButton(element);
                            $scope.dialTone(770, 1209);
                            break;
                        case 101:
                            var element = document.getElementsByClassName("number-dig")[4];
                            $scope.addAnimationToButton(element);
                            $scope.dialTone(770, 1336);
                            break;
                        case 102:
                            var element = document.getElementsByClassName("number-dig")[5];
                            $scope.addAnimationToButton(element);
                            $scope.dialTone(770, 1477);
                            break;
                        case 103:
                            var element = document.getElementsByClassName("number-dig")[6];
                            $scope.addAnimationToButton(element);
                            $scope.dialTone(852, 1209);
                            break;
                        case 104:
                            var element = document.getElementsByClassName("number-dig")[7];
                            $scope.addAnimationToButton(element);
                            $scope.dialTone(770, 1477);
                            break;
                        case 105:
                            var element = document.getElementsByClassName("number-dig")[8];
                            $scope.addAnimationToButton(element);
                            $scope.dialTone(770, 1477);
                            break;
                        case 106:
                            var element = document.getElementsByClassName("number-dig")[9];
                            $scope.addAnimationToButton(element);
                            break;
                        case 96:
                            var element = document.getElementsByClassName("number-dig")[10];
                            $scope.addAnimationToButton(element);
                            $scope.dialTone(770, 1477);
                            break;
                        case 96:
                            var element = document.getElementsByClassName("number-dig")[10];
                            $scope.addAnimationToButton(element);
                            $scope.dialTone(770, 1477);
                            break;
                        case 18:
                            var element = document.getElementsByClassName("number-dig")[11];
                            $scope.addAnimationToButton(element);
                            $scope.dialTone(770, 1477);
                            break;
                        case 13:
                            var element = document.getElementsByClassName("action-dig");
                            $scope.dialOrAnswer(element);
                            $scope.dialTone(770, 1477);
                            break;
                    }
                    ;

                }

            }
            $scope.stopDialNumber = function () {
                if ($scope.oscillator1 && $scope.oscillator2) {
                    $scope.oscillator1.disconnect();
                    $scope.oscillator2.disconnect();
                }
                return;
            };

            $scope.mute = function () {
                sipHelper.mute();
            };

            $scope.unmute = function () {
                sipHelper.unmute();
            };
            $scope.hold = function () {
                sipHelper.hold();

            };
            $scope.unhold = function () {
                sipHelper.unhold();
            };

            $scope.transfer = function (transferNumber) {
                sipHelper.transfer(transferNumber);
            }
            $scope.isTransferChange = function (status) {
                $scope.isTransfer = status;
            }
        }

    ]);