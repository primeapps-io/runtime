'use strict';

angular.module('ofisim')
    .controller('SingleSMSController', ['$rootScope', '$scope', 'ngToast', '$filter', 'helper', '$location', '$state', '$stateParams', '$q', '$window', '$localStorage', '$cache', 'config', 'ModuleService',
        function ($rootScope, $scope, ngToast, $filter, helper, $location, $state, $stateParams, $q, $window, $localStorage, $cache, config, ModuleService) {

            var gsm7bitChars = "@£$¥èéùìòÇ\\nØø\\rÅåΔ_ΦΓΛΩΠΨΣΘΞÆæßÉ !\\\"#¤%&'()*+,-./0123456789:;<=>?¡ABCDEFGHIJKLMNOPQRSTUVWXYZÄÖÑÜ§¿abcdefghijklmnopqrstuvwxyzäöñüà",
                gsm7bitExChar = "\\^{}\\\\\\[~\\]|€",

                gsm7bitRegExp = RegExp("^[" + gsm7bitChars + "]*$"),
                gsm7bitExRegExp = RegExp("^[" + gsm7bitChars + gsm7bitExChar + "]*$"),
                gsm7bitExOnlyRegExp = RegExp("^[\\" + gsm7bitExChar + "]*$"),
                GSM_7BIT = 'GSM_7BIT',
                GSM_7BIT_EX = 'GSM_7BIT_EX',
                UTF16 = 'UTF16',
                messageLength = {
                    GSM_7BIT: 160,
                    GSM_7BIT_EX: 160,
                    UTF16: 70
                },
                multiMessageLength = {
                    GSM_7BIT: 153,
                    GSM_7BIT_EX: 153,
                    UTF16: 67
                };

            function detectSMSEncoding(text) {
                switch (false) {
                    case text.match(gsm7bitRegExp) == null:
                        return GSM_7BIT;
                    case text.match(gsm7bitExRegExp) == null:
                        return GSM_7BIT_EX;
                    default:
                        return UTF16;
                }
            };

            function countGsm7bitEx(text) {
                var char2, chars;
                chars = (function () {
                    var _i, _len, _results;
                    _results = [];
                    for (_i = 0, _len = text.length; _i < _len; _i++) {
                        char2 = text[_i];
                        if (char2.match(gsm7bitExOnlyRegExp) != null) {
                            _results.push(char2);
                        }
                    }
                    return _results;
                }).call(this);
                return chars.length;
            };

            function estimateMessages(text) {
                if (!$scope.txtSMS) return;
                $scope.txtSMS = $scope.txtSMS.replace(/(\r\n|\n|\r)/gm, " ");

                var count, encoding, length, messages, per_message;
                encoding = detectSMSEncoding(text);
                length = text.length;
                if (encoding === GSM_7BIT_EX) {
                    length += countGsm7bitEx(text);
                }
                per_message = messageLength[encoding];
                if (length > per_message) {
                    per_message = multiMessageLength[encoding];
                }
                messages = Math.ceil(length / per_message);
                return messages;
            };

            $scope.loadingModal = false;
            $scope.txtSMS = "";
            $scope.totalSMS = 0;

            $scope.addCustomField = function ($event, customField) {
                $scope.txtSMS += "{" + customField.name + "}";
            };

            $scope.calculateSMS = function (text) {
                $scope.totalSMS = estimateMessages($scope.txtSMS);
            };

            $scope.filterKey = function (e) {
                var regex = new RegExp("^[^\n\r]*$");
                var character = String.fromCharCode(e.which);
                var text = angular.element(e.srcElement).val();
                if (!regex.test(character) || !regex.test(text)) {
                    event.preventDefault();
                }
            };

            $scope.phoneField = $filter('filter')($scope.$parent.$parent.allFields, { name: 'mobile' })[0];

            $scope.submitSMS = function () {
                if (!$scope.smsModalForm.$valid) return;

                $scope.selectedIds = [];
                $scope.selectedIds.push($scope.$parent.$parent.record.id);

                $scope.submittingModal = true;

                ModuleService.sendSMS($scope.$parent.$parent.module.id,
                    $scope.selectedIds,
                    angular.toJson($scope.$parent.$parent.findRequest),
                    $scope.$parent.$parent.isAllSelected,
                    $scope.txtSMS.replace(/(\r\n|\n|\r)/gm, " "),
                    $scope.phoneField.name).then(function (response) {
                        $scope.submittingModal = false;
                        $scope.smsModal.hide();
                        $scope.$parent.$parent.isAllSelected = false;
                        $scope.$parent.$parent.selectedRows = [];
                        ngToast.create({ content: $filter('translate')('SMS.MessageQueued'), className: 'success' });

                    })
                    .catch(function () {
                        $scope.submittingModal = false;
                        $scope.smsModal.hide();
                        $scope.$parent.$parent.isAllSelected = false;
                        $scope.$parent.$parent.selectedRows = [];
                        ngToast.create({ content: $filter('translate')('Common.Error'), className: 'danger' });
                    });
            };
        }
    ]);