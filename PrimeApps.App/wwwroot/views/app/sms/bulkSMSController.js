'use strict';

angular.module('ofisim')
    .controller('BulkSMSController', ['$rootScope', '$scope', 'ngToast', '$filter', 'helper', '$location', '$state', '$stateParams', '$q', '$window', '$localStorage', '$cache', 'config', 'ModuleService', 'TemplateService',
        function ($rootScope, $scope, ngToast, $filter, helper, $location, $state, $stateParams, $q, $window, $localStorage, $cache, config, ModuleService, TemplateService) {

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
            $scope.formType = 'sms';

            $scope.getTagTextRaw = function (item) {
                if (item.name.indexOf("seperator") < 0) {
                    return '{' + item.name + '}';
                }
            };
            $scope.moduleFields = TemplateService.getFields($scope.$parent.$parent.$parent.module, $scope.$parent.$parent.$parent.view);

            $scope.searchTags = function (term) {
                var tagsList = [];
                angular.forEach($scope.moduleFields, function (item) {
                    if (item.name == "seperator")
                        return;
                    if (item.label.indexOf(term) >= 0) {
                        tagsList.push(item);
                    }
                });

                $scope.tags = tagsList;
                return tagsList;
            };


            $scope.setTemplate = function () {
                $scope.smstemplate.template_subject = $scope.Subject;
                $scope.smstemplate.tinymce_content = $scope.tinymceModel;

            };


            TemplateService.getAll('email')
                .then(function (response) {
                    $scope.templates = response.data;
                });

            function htmltext(html) {
                var tag = document.createElement('div');
                tag.innerHTML = html;

                return tag.innerText;
            }

            $scope.setContent = function (temp) {
                var template = $filter('filter')($scope.templates, { id: temp }, true)[0];

                if (temp) {

                    $scope.tinymceModel = htmltext(template.content);
                    $scope.smstemplate.template_name = template.name;
                    $scope.currentTemplate = template;


                }
                else {
                    $scope.tinymceModel = null;
                    $scope.smstemplate.template_name = null;
                }
            };




            $scope.moduleFields = TemplateService.getFields($scope.module);
            $scope.phoneFields = [];

            angular.forEach($scope.moduleFields, function (item) {
                if (item.name === 'mobile' && !item.deleted && item.parent_type != 'users') {
                    $scope.phoneFields.push(item);
                }
            });

            if ($scope.phoneFields.length > 0)
                $scope.phoneField = $scope.phoneFields[0];
            else {
                $scope.phoneField = $scope.moduleFields[0];
            }




            $scope.templateSave = function () {
                var template = {};
                template.module_id = $scope.$parent.$parent.$parent.module.id;
                template.name = $scope.smstemplate.template_name;
                template.subject = "SMS";
                template.content = $scope.smstemplate.tinymce_content;
                template.sharing_type = $scope.smstemplate.sharing_type;
                template.template_type = 2;
                template.active = true;

                if ($scope.smstemplate.sharing_type === 'custom') {
                    template.shares = [];

                    angular.forEach($scope.smstemplate.shares, function (user) {
                        template.shares.push(user.id);
                    });
                }

                var result;

                if ($scope.currentTemplate) {
                    template.id = $scope.currentTemplate.id;
                    result = TemplateService.update(template);
                }
                else {
                    result = TemplateService.create(template);
                }

                result.then(function (saveResponse) {
                    TemplateService.getAll('email')
                        .then(function (listResponse) {
                            $scope.templates = listResponse.data;
                            $scope.template = saveResponse.data.id;
                            ngToast.create({ content: $filter('translate')('Template.SuccessMessage'), className: 'success' });
                        });
                });
            };

            $scope.smstemplate = {};
            $scope.smstemplate.system_type = 'custom';
            $scope.smstemplate.sharing_type = 'me';

            $scope.backTemplate = function () {
                $scope.tinymceModel = $scope.smstemplate.tinymce_content;

            };

            $scope.templateDelete = function () {
                var templates;
                templates=$scope.template;
                TemplateService.delete(templates)
                    .then(function () {
                        TemplateService.getAll('email')
                            .then(function (response) {
                                $scope.templates = response.data;
                            });
                        ngToast.create({ content: $filter('translate')('Template.SuccessDelete'), className: 'success' });

                    });
            };


            $scope.tinymceOptions = function (scope) {
                $scope[scope] = {
                    init_instance_callback: function (editor) {
                        $scope.iframeElement[scope] = editor.iframeElement;
                    },
                    resize: false,
                    width: '136%',
                    height: 160,
                    language: $rootScope.language,
                    menubar: false,
                    statusbar: false,
                    toolbar: false
                };
            };

            $scope.iframeElement = {};

            $scope.tinymceOptions('tinymceTemplate');
            $scope.tinymceOptions('tinymceTemplateEdit');

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
                if (!$scope.tinymceModel) return;
                $scope.tinymceModel = $scope.tinymceModel.replace(/(\r\n|\n|\r)/gm, " ");

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
            $scope.tinymceModel = "";
            $scope.totalSMS = 0;

            $scope.addCustomField = function ($event, customField) {
                $scope.tinymceModel += "{" + customField.name + "}";
            };

            $scope.calculateSMS = function (text) {
                $scope.totalSMS = estimateMessages($scope.tinymceModel);
            };

            $scope.filterKey = function (e) {
                var regex = new RegExp("^[^\n\r]*$");
                var character = String.fromCharCode(e.which);
                var text = angular.element(e.srcElement).val();
                if (!regex.test(character) || !regex.test(text)) {
                    event.preventDefault();
                }
            };


            $scope.submitSMS = function () {
                if (!$scope.smsModalForm.$valid) return;

                var selectedIds = $scope.$parent.$parent.selectedRecords.map(function (row) {
                    return row.id;
                });

                $scope.queryRequest = {};

                if ($scope.$parent.$parent.isAllSelected)
                    $scope.queryRequest.query = angular.toJson($scope.$parent.$parent.findRequest);

                $scope.submittingModal = true;

                ModuleService.sendSMS($scope.$parent.$parent.module.id,
                    selectedIds,
                    $scope.queryRequest.query || null,
                    $scope.$parent.$parent.$parent.isAllSelected,
                    $scope.tinymceModel.replace(/(\r\n|\n|\r)/gm, " "),
                    $scope.phoneField.name).then(function (response) {
                    $scope.submittingModal = false;
                    $scope.smsModal.hide();
                    $scope.phoneField.name, $scope.$parent.$parent.$parent.isAllSelected = false;
                    $scope.$parent.$parent.$parent.selectedRecords = [];
                    $scope.$parent.$parent.$parent.selectedRows = [];
                    ngToast.create({ content: $filter('translate')('SMS.MessageQueued'), className: 'success' });

                })
                    .catch(function () {
                        $scope.submittingModal = false;
                        $scope.smsModal.hide();
                        $scope.$parent.$parent.isAllSelected = false;
                        $scope.$parent.$parent.selectedRecords = [];
                        $scope.$parent.$parent.selectedRows = [];
                        ngToast.create({ content: $filter('translate')('Common.Error'), className: 'danger' });
                    });
            };
        }
    ]);