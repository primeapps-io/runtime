'use strict';

angular.module('primeapps')
    .controller('BulkSMSController', ['$rootScope', '$scope', '$filter', 'ModuleService', 'TemplateService', '$mdDialog', 'mdToast',
        function ($rootScope, $scope, $filter, ModuleService, TemplateService, $mdDialog, mdToast) {

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
            $scope.loadingModal = true;

            $scope.getTagTextRaw = function (item) {
                if (item.name.indexOf("seperator") < 0) {
                    return '{' + item.name + '}';
                }
            };
            $scope.moduleFields = TemplateService.getFields($scope.$parent.module, $scope.$parent.view);

            $scope.searchTags = function (term) {
                var tagsList = [];
                angular.forEach($scope.moduleFields, function (item) {
                    if (item.name === "seperator")
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

                $scope.formType = 'template';
                if ($scope.currentTemplate) {
                    if ($scope.currentTemplate.sharing_type === 'profile')
                        $scope.smstemplate.profile = $scope.getProfilisByIds($scope.currentTemplate.profile_list);
                    else
                        $scope.smstemplate.profiles = null;

                    if ($scope.currentTemplate.sharing_type === 'custom')
                        $scope.smstemplate.shares = $scope.getUsersByIds($scope.currentTemplate.shares);
                    else
                        $scope.smstemplate.shares = [];

                    $scope.smstemplate.sharing_type = $scope.currentTemplate.sharing_type;
                    $scope.smstemplate.language = $scope.currentTemplate.language;
                } else {
                    $scope.smstemplate.sharing_type = 'me';
                    $scope.smstemplate.language = $rootScope.globalization.Label;
                    $scope.smstemplate.profile = null;
                    $scope.smstemplate.shares = [];
                }
            };

            $scope.profilesOptions = {
                dataSource: $rootScope.profiles,
                filter: "contains",
                dataTextField: 'languages.' + $rootScope.globalization.Label + '.name',
                dataValueField: "id",
                optionLabel: $filter('translate')('Common.Select')
            };

            $scope.getProfilisByIds = function (ids) {
                var profileList = [];
                for (var i = 0; i < ids.length; i++) {
                    var profile = $filter('filter')($rootScope.profiles, { id: parseInt(ids[i]) }, true);
                    if (profile) {
                        profileList.push(profile[0]);
                    }

                }
                return profileList;
            };

            $scope.getUsersByIds = function (ids) {
                var usersList = [];
                for (var i = 0; i < ids.length; i++) {
                    var user = $filter('filter')($rootScope.users, { id: parseInt(ids[i].user_id) }, true);
                    if (user) {
                        usersList.push(user[0]);
                    }

                }
                return usersList;
            };


            TemplateService.getAll('email', '', 'SMS')
                .then(function (response) {
                    $scope.templates = [];
                    for (var i = 0; i < response.data.length; i++) {
                        //if(response.data[i].subject === "SMS"){
                        $scope.templates.push(response.data[i])
                        //}
                    }
                    $scope.loadingModal = false;
                }).catch(function () {
                    $scope.loadingModal = false;
                });

            function htmltext(html) {
                var tag = document.createElement('div');
                tag.innerHTML = html;

                return tag.innerText;
            }

            $scope.setContent = function (temp) {

                $scope.currentTemplate = null;

                if (!temp)
                    return;

                var template = $filter('filter')($scope.templates, { id: temp.id }, true)[0];

                if (template) {
                    $scope.tinymceModel = htmltext(template.content);
                    $scope.calculateSMS($scope.tinymceModel);
                    $scope.smstemplate.template_name = template.name;
                    $scope.currentTemplate = template;
                }
                else {
                    $scope.tinymceModel = null;
                    $scope.calculateSMS($scope.tinymceModel);
                    $scope.smstemplate.template_name = null;
                }
            };

            $scope.moduleFields = TemplateService.getFields($scope.module);
            $scope.phoneFields = [];

            angular.forEach($scope.moduleFields, function (item) {
                if (item.name === 'mobile' && !item.deleted && item.parent_type !== 'users') {
                    $scope.phoneFields.push(item);
                }
            });

            if ($scope.phoneFields.length > 0)
                $scope.phoneField = $scope.phoneFields[0];
            else {
                $scope.phoneField = $scope.moduleFields[0];
            }

            $scope.templateSave = function () {

                $scope.saving = true;
                $scope.clicked = true;

                function validate() {

                    if ((!$scope.smstemplate.template_name || !$scope.smstemplate.language)) {
                        mdToast.error($filter('translate')('Module.RequiredError'));
                        return false;
                    }

                    if (!$scope.smstemplate.tinymce_content) {
                        mdToast.error($filter('translate')('Template.ContentRequired'));
                        return false;
                    }

                    return true;
                }

                if (!validate()) {
                    $scope.saving = false;
                    return;
                }

                var template = {};
                template.module_id = $scope.$parent.module.id;
                template.module = $scope.module.name;
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

                if ($scope.smstemplate.sharing_type === 'profile') {
                    var profiles = [];
                    for (var i = 0; i < $scope.smstemplate.profile.length; i++) {
                        profiles.push($scope.smstemplate.profile[i].id);
                    }
                    template.profiles = profiles;
                } else {
                    template.profiles = null;
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
                    $scope.$parent.template = saveResponse.data;
                    TemplateService.getAll('email')
                        .then(function (listResponse) {
                            $scope.saving = false;
                            $scope.clicked = false;
                            $scope.templates = [];
                            for (var i = 0; i < listResponse.data.length; i++) {
                                if (listResponse.data[i].subject === "SMS") {
                                    $scope.templates.push(listResponse.data[i])
                                }
                            }

                            $scope.setContent($scope.$parent.template);
                            $scope.template = $scope.$parent.template;
                            mdToast.success($filter('translate')('Template.SuccessMessage'));

                        }).catch(function () {
                            $scope.saving = false;
                            $scope.clicked = false;
                        });
                }).catch(function () {
                    $scope.saving = false;
                    $scope.clicked = false;
                });
            };

            $scope.smstemplate = {};
            $scope.smstemplate.system_type = 'custom';
            $scope.smstemplate.sharing_type = 'me';

            $scope.backTemplate = function () {

                if ($scope.currentTemplate)
                    $scope.tinymceModel = $scope.smstemplate.tinymce_content;

                if ($scope.smstemplate.template_name)
                    $scope.template = $filter('filter')($scope.templates, { name: $scope.smstemplate.template_name }, true)[0];

            };

            $scope.templateDelete = function () {
                var templates = $scope.template;
                TemplateService.delete(templates)
                    .then(function () {
                        TemplateService.getAll('email')
                            .then(function (response) {
                                $scope.templates = [];
                                for (var i = 0; i < response.data.length; i++) {
                                    if (response.data[i].subject === "SMS") {
                                        $scope.templates.push(response.data[i])
                                    }
                                }
                            });
                        mdToast.success($filter('translate')('Template.SuccessDelete'));
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
                if (!text) return 0;

                text = text.replace(/(\r\n|\n|\r)/gm, " ");

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

            //$scope.loadingModal = false;
            $scope.tinymceModel = "";
            $scope.totalSMS = 0;

            $scope.addCustomField = function ($event, customField) {
                $scope.tinymceModel += "{" + customField.name + "}";
            };

            $scope.calculateSMS = function (text) {
                $scope.totalSMS = estimateMessages(text);
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
                $scope.submittingModal = true;
                $scope.clicked = true;
                if (!$scope.smsModalForm.validate()) {
                    mdToast.error($filter('translate')('Module.RequiredError'));
                    $scope.submittingModal = false;
                    return;
                }



                var selectedIds = $scope.$parent.selectedRecords.map(function (row) {
                    return row.id;
                });

                $scope.queryRequest = {};

                if ($scope.$parent.isAllSelected)
                    $scope.queryRequest.query = angular.toJson($scope.$parent.findRequest);

                $scope.submittingModal = true;

                ModuleService.sendSMS($scope.$parent.module.id,
                    selectedIds,
                    $scope.queryRequest.query || null,
                    $scope.$parent.isAllSelected,
                    null,
                    $scope.phoneField.name,
                    $scope.template).then(function (response) {
                        $scope.submittingModal = false;
                        $scope.clicked = false;
                        $scope.close();
                        $scope.phoneField.name;
                        $scope.$parent.isAllSelected = false;
                        $scope.$parent.selectedRecords = [];
                        $scope.$parent.selectedRows = [];
                        $scope.refreshGrid();
                        mdToast.success($filter('translate')('SMS.MessageQueued'));

                    })
                    .catch(function () {
                        $scope.submittingModal = false;
                        $scope.clicked = false;
                        $scope.close();
                        $scope.$parent.isAllSelected = false;
                        $scope.$parent.selectedRecords = [];
                        $scope.$parent.selectedRows = [];
                        mdToast.error($filter('translate')('Common.Error'));
                    });
            };

            //FOR Kendo UI
            $scope.close = function () {
                $scope.$parent.template = {}
                $mdDialog.hide();
            };

            $scope.selectedRecordOpstions = {
                dataSource: $scope.selectedRecords,
                dataTextField: 'displayName',
                dataValueField: 'id'
            };

            $scope.templateOptions = {
                dataSource: {
                    transport: {
                        read: function (o) {
                            o.success($scope.templates)
                        }
                    }
                },
                dataBound: $scope.templates,
                change: function (e) {
                    var value = this.value();
                    $scope.setContent(value);
                },
                dataTextField: "name",
                dataValueField: "id",
            };

            $scope.phoneFieldOptions = {
                dataSource: $filter('filter')($scope.moduleFields, function (field) {
                    return (field.data_type == 'number' || field.data_type == 'text_single') && !field.deleted && field.parent_type != 'users';
                }, true),
                valueTemplate: '<span class="k-state-default">{{dataItem.label}}  {{dataItem.labelExt}}  </span>',
                template: '<span class="k-state-default">{{dataItem.label}}  {{dataItem.labelExt}}  </span>',
                dataTextField: "label",
                dataValueField: "name"
            };

            $scope.sharesOptions = {
                dataSource: $scope.users,
                filter: "contains",
                dataTextField: "full_name",
                dataValueField: "id",
            };
            //FOR Kendo UI

        }
    ]);