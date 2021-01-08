'use strict';

angular.module('primeapps')
    .controller('SingleEMailController', ['$rootScope', '$scope', '$filter', 'helper', '$location', '$state', '$stateParams', '$q', '$window', '$localStorage', '$cache', 'config', '$http', 'ModuleService', 'TemplateService', '$cookies', '$mdDialog', 'mdToast', 'components', '$timeout',
        function ($rootScope, $scope, $filter, helper, $location, $state, $stateParams, $q, $window, $localStorage, $cache, config, $http, ModuleService, TemplateService, $cookies, $mdDialog, mdToast, components, $timeout) {
            $scope.loadingModal = true;
            $scope.module = $filter('filter')($rootScope.modules, { name: $stateParams.type }, true)[0];
            $scope.loadingModal = false;
            $scope.nonEmails = {};
            $scope.isBcc = true;
            $scope.isCc = true;

            var loadTemplates = function () {
                $scope.loadingModal = true;
                TemplateService.getAll('email', $scope.module.name)
                    .then(function (response) {
                        $scope.templates = [];
                        for (var i = 0; i < response.data.length; i++) {
                            if (response.data[i].subject !== "SMS") {
                                $scope.templates.push(response.data[i])
                            }
                        }
                        $scope.loadingModal = false;
                    })
                    .catch(function () {
                        $scope.loadingModal = false;
                    });
            };
            loadTemplates();

            $scope.profilesOptions = {
                dataSource: $rootScope.profiles,
                filter: "contains",
                dataTextField: 'languages.' + $rootScope.globalization.Label + '.name',
                dataValueField: "id",
                optionLabel: $filter('translate')('Common.Select')
            };

            $scope.iframeElement = {};
            /// tinymce editor configuration.
            $scope.tinymceOptions = function (scope) {
                $scope[scope] = {
                    setup: function (editor) {
                        editor.addButton('addParameter', {
                            type: 'button',
                            text: $filter('translate')('EMail.AddParameter'),
                            onclick: function () {
                                tinymce.activeEditor.execCommand('mceInsertContent', false, '#');
                            }
                        });
                        editor.on("init", function () {
                            $scope.loadingModal = false;
                        });
                    },
                    onChange: function (e) {
                        
                        // put logic here for keypress and cut/paste changes
                    },
                    inline: false,
                    height: 200,
                    language: $rootScope.language,
                    plugins: [
                        "advlist autolink lists link image charmap print preview anchor table",
                        "searchreplace visualblocks code fullscreen",
                        "insertdatetime table contextmenu paste imagetools wordcount textcolor colorpicker"
                    ],
                    toolbar: "addParameter | addQuoteTemplate | styleselect | bold italic underline | forecolor backcolor | alignleft aligncenter alignright alignjustify | link image imagetools | table bullist numlist  blockquote code fullscreen",
                    menubar: 'false',
                    templates: [
                        { title: 'Test template 1', content: 'Test 1' },
                        { title: 'Test template 2', content: 'Test 2' }
                    ],
                    skin: 'lightgray',
                    theme: 'modern',

                    file_picker_callback: function (callback, value, meta) {
                        // Provide file and text for the link dialog
                        uploadSuccessCallback = callback;

                        if (meta.filetype == 'file') {
                            var uploadButton = document.getElementById('uploadFile');
                            uploadButton.click();
                        }

                        // Provide image and alt text for the image dialog
                        if (meta.filetype == 'image') {
                            var uploadButton = document.getElementById('uploadImage');
                            uploadButton.click();
                        }
                    },
                    image_advtab: true,
                    file_browser_callback_types: 'image file',
                    paste_data_images: true,
                    paste_as_text: true,
                    spellchecker_language: $rootScope.language,
                    images_upload_handler: function (blobInfo, success, failure) {
                        var blob = blobInfo.blob();
                        uploadSuccessCallback = success;
                        uploadFailedCallback = failure;
                        $scope.imgUpload.uploader.addFile(blob);
                        ///TODO: in future will be implemented to upload pasted data images into server.
                    },
                    init_instance_callback: function (editor) {
                        $scope.iframeElement[scope] = editor.iframeElement;
                    },
                    resize: false,
                    width: '99,9%',
                    toolbar_items_size: 'small',
                    statusbar: false,
                    convert_urls: false,
                    remove_script_host: false
                };
            };

            $scope.tinymceOptions('tinymceTemplate');
            $scope.tinymceOptions('tinymceTemplateEdit');

            $scope.formType = 'email';

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

            $scope.templateSave = function () {

                $scope.saving = true;
                $scope.clicked = true;
                
                function validate() {
                    
                    if (!$scope.template_name || !$scope.template_subject || !$scope.templateLanguage) {
                        mdToast.error($filter('translate')('Module.RequiredError'));
                        return false;
                    }
                    
                    if (!$scope.tinymce_content) {
                        mdToast.error($filter('translate')('Template.ContentRequired'));
                        return false;
                    }
                    
                    return true;
                }

                if (!validate()){
                    $scope.saving = false;
                    return;
                } 

                var template = {};
                template.module = $scope.module.name;
                template.name = $scope.template_name;
                template.subject = $scope.template_subject;
                template.content = $scope.tinymce_content;
                template.sharing_type = $scope.newtemplate.sharing_type;
                template.template_type = 2;
                template.active = true;
                template.language = $scope.templateLanguage;

                if ($scope.newtemplate.system_type === 'custom') {
                    template.shares = [];

                    angular.forEach($scope.newtemplate.shares, function (user) {
                        template.shares.push(user.id);
                    });
                }

                if ($scope.newtemplate.sharing_type === 'profile') {
                    var profiles = [];
                    for (var i = 0; i < $scope.newtemplate.profile.length; i++) {
                        profiles.push($scope.newtemplate.profile[i].id);
                    }
                    template.profiles = profiles;
                } else {
                    template.profiles = null;
                }

                var result;

                if ($scope.currentTemplate) {
                    template.id = $scope.currentTemplate.id;
                    result = TemplateService.update(template);
                } else {
                    result = TemplateService.create(template);
                }

                result.then(function (saveResponse) {
                    $scope.currentTemplate = saveResponse.data;
                    TemplateService.getAll('email', $scope.module.name)
                        .then(function (listResponse) {
                            $scope.saving = false;
                            $scope.clicked = false;
                            $scope.templates = [];
                            for (var i = 0; i < listResponse.data.length; i++) {
                                if (listResponse.data[i].subject !== "SMS") {
                                    $scope.templates.push(listResponse.data[i])
                                }
                            }
                            $scope.template = saveResponse.data;
                            mdToast.success($filter('translate')('Template.SuccessMessage'));
                            $scope.templateOptions.dataSource.read();
                            $scope.setContent($scope.currentTemplate);
                            $scope.formType = 'email';
                        }).catch(function () {
                        $scope.saving = false;
                        $scope.clicked = false;
                    });
                }).catch(function () {
                    $scope.saving = false;
                    $scope.clicked = false;
                });
            };

            $scope.templateDelete = function () {
                var templates;
                templates = $scope.template;
                TemplateService.delete(templates.id)
                    .then(function () {
                        TemplateService.getAll('email', $scope.module.name)
                            .then(function (response) {
                                $scope.templates = [];
                                for (var i = 0; i < response.data.length; i++) {
                                    if (response.data[i].subject !== "SMS") {
                                        $scope.templates.push(response.data[i])
                                    }
                                }
                                $scope.templateOptions.dataSource.read();
                                $scope.currentTemplate = null;
                                $scope.template = null;
                                $scope.tinymceModel = null;
                                $scope.subject = null;
                                $scope.template_subject = null;
                                $scope.tinymce_content = null;
                                $scope.templateLanguage = null;
                                $scope.template_name = null;
                                $scope.template_subject = null;
                            });
                        mdToast.success($filter('translate')('Template.SuccessDelete'));
                    });
            };

            $scope.setContent = function (temp) {

                var template = $filter('filter')($scope.templates, { id: temp.id }, true)[0];

                if (template) {
                    $scope.newtemplate.system_type = 'custom';
                    $scope.newtemplate.sharing_type = 'me';
                    $scope.tinymceModel = template.content;
                    $scope.subject = template.subject;
                    $scope.currentTemplate = template;
                    $scope.template_name = template.name;
                    $scope.editPermission = template.system_type === 'custom';
                }
                else {
                    $scope.tinymceModel = null;
                    $scope.subject = null;
                    $scope.currentTemplate = null;
                    $scope.template_name = null;
                    $scope.template_subject = null;
                    $scope.tinymce_content = null;
                    $scope.newtemplate.system_type = null;
                    $scope.shares = [];
                    $scope.templateLanguage = null;
                }
            };

            $scope.setTemplate = function () {

                $scope.template_subject = $scope.subject;
                $scope.tinymce_content = $scope.tinymceModel;

                if ($scope.currentTemplate) {
                    if ($scope.currentTemplate.sharing_type === 'profile')
                        $scope.newtemplate.profile = $scope.getProfilisByIds($scope.currentTemplate.profile_list);
                    else
                        $scope.newtemplate.profiles = null;

                    if ($scope.currentTemplate.sharing_type === 'custom')
                        $scope.newtemplate.shares = $scope.getUsersByIds($scope.currentTemplate.shares);
                    else
                        $scope.newtemplate.shares = [];

                    $scope.newtemplate.sharing_type = $scope.currentTemplate.sharing_type;
                    $scope.templateLanguage = $scope.currentTemplate.language;
                } else {
                    $scope.newtemplate.sharing_type = 'me';
                    $scope.templateLanguage = $rootScope.globalization.Label;
                    $scope.newtemplate.profile = null;
                    $scope.newtemplate.shares = [];
                }
            };

            $scope.backTemplate = function () {
                $scope.subject = $scope.template_subject;
                $scope.tinymceModel = $scope.tinymce_content;
                if ($scope.template_name)
                    $scope.template = $filter('filter')($scope.templates, { name: $scope.template_name }, true)[0];
            };

            $scope.newtemplate = {};
            $scope.newtemplate.system_type = 'custom';
            $scope.newtemplate.sharing_type = 'me';

            var uploadSuccessCallback,
                uploadFailedCallback;

            if (!$rootScope.system.messaging.PersonalEMail && !$rootScope.system.messaging.SystemEMail) {
                mdToast.error($filter('translate')('EMail.MessageQueued'));
            }

            var systemEmailSettings = angular.copy($rootScope.system.messaging.SystemEMail || {}),
                personalEmailSettings = angular.copy($rootScope.system.messaging.PersonalEMail || {});

            var dialog_uid = plupload.guid();
            $scope.moduleFields = TemplateService.getFields($scope.module);
            $scope.emailFields = [];

            angular.forEach($scope.moduleFields, function (item) {
                if (item.data_type === 'email' && !item.deleted && item.parent_type !== 'users') {
                    $scope.emailFields.push(item);
                }
            });

            if ($scope.emailFields.length > 0)
                $scope.emailField = $scope.emailFields[0];

            $scope.senderAlias = null;

            $scope.senders = [];

            /// add system defined senders to the sender list, if exists.
            if (systemEmailSettings.senders) {
                if (systemEmailSettings.senders.length > 0) {
                    systemEmailSettings.senders.forEach(function (sender) {
                        sender.type = $filter('translate')("EMail.System");
                        $scope.senders.push(sender);
                    });
                }
            }

            /// add personal defined senders to the sender list, if exists.
            if (personalEmailSettings.senders) {
                if (personalEmailSettings.senders.length > 0) {
                    personalEmailSettings.senders.forEach(function (sender) {
                        sender.type = $filter('translate')("EMail.Personal");
                        $scope.senders.push(sender);
                    });
                }
            }

            $scope.getTagTextRaw = function (item) {
                $timeout(function () {
                    $scope.$broadcast("$tinymce:refreshContent");
                }, 50);

                if (item.name.indexOf("seperator") < 0) {
                    return '<i style="color:#0f1015;font-style:normal">' + '{' + item.name + '}' + '</i>';
                }
            };

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

            $scope.primaryField = $filter('filter')($scope.module.fields, { primary: true })[0];
            $scope.recordId = $scope.$parent.id;


            $scope.addressType = function (type) {
                return $filter('translate')("EMail." + type);
            };

            /// uploader configuration for image files.
            $scope.imgUpload = {
                settings: {
                    multi_selection: false,
                    url: 'storage/upload',
                    multipart_params: {
                        container: dialog_uid,
                        type: "mail",
                        upload_id: 0,
                        response_list: ""
                    },
                    filters: {
                        mime_types: [
                            { title: "Image files", extensions: "jpg,gif,png" },
                        ],
                        max_file_size: "2mb"
                    },
                    resize: { quality: 90 }
                },
                events: {
                    filesAdded: function (uploader, files) {
                        uploader.start();
                        tinymce.activeEditor.windowManager.open({
                            title: $filter('translate')('Common.PleaseWait'),
                            width: 50,
                            height: 50,
                            body: [
                                {
                                    type: 'container',
                                    name: 'container',
                                    label: '',
                                    html: '<span>' + $filter('translate')('EMail.UploadingAttachment') + '</span>'
                                },
                            ],
                            buttons: []
                        });
                    },
                    uploadProgress: function (uploader, file) {
                    },
                    fileUploaded: function (uploader, file, response) {
                        uploader.settings.multipart_params.response_list = "";
                        uploader.settings.multipart_params.upload_id = 0;

                        tinymce.activeEditor.windowManager.close();
                        var resp = JSON.parse(response.response);
                        uploadSuccessCallback(resp.public_url, { alt: file.name });
                        uploadSuccessCallback = null;
                    },
                    error: function (file, error) {
                        switch (error.code) {
                            case -600:
                                tinymce.activeEditor.windowManager.alert($filter('translate')('EMail.MaxImageSizeExceeded'));
                                break;
                            default:
                                break;
                        }
                        if (uploadFailedCallback) {
                            uploadFailedCallback();
                            uploadFailedCallback = null;
                        }
                    }
                }
            };

            /// uploader configuration for files.
            $scope.fileUpload = {
                settings: {
                    multi_selection: false,
                    unique_names: false,
                    url: config.apiUrl + 'Document/upload_attachment',
                    headers: {
                        'Authorization': 'Bearer ' + $localStorage.read('access_token'),
                        'Accept': 'application/json',
                        'X-Tenant-Id': $cookies.get(preview ? 'preview_tenant_id' : 'tenant_id'),
                        'X-App-Id': $cookies.get(preview ? 'preview_app_id' : 'app_id')
                    },
                    multipart_params: {
                        container: dialog_uid
                    },
                    filters: {
                        mime_types: [
                            { title: "Email Attachments", extensions: "pdf,doc,docx,xls,xlsx,csv" },
                        ],
                        max_file_size: "50mb"
                    }
                },
                events: {
                    filesAdded: function (uploader, files) {
                        uploader.start();
                        tinymce.activeEditor.windowManager.open({
                            title: $filter('translate')('Common.PleaseWait'),
                            width: 50,
                            height: 50,
                            body: [
                                {
                                    type: 'container',
                                    name: 'container',
                                    label: '',
                                    html: '<span>' + $filter('translate')('EMail.UploadingAttachment') + '</span>'
                                },
                            ],
                            buttons: []
                        });
                    },
                    uploadProgress: function (uploader, file) {
                    },
                    fileUploaded: function (uploader, file, response) {
                        var resp = JSON.parse(response.response);
                        uploadSuccessCallback(resp.public_url, { alt: file.name });
                        uploadSuccessCallback = null;
                        tinymce.activeEditor.windowManager.close();
                    },
                    chunkUploaded: function (up, file, response) {
                        var resp = JSON.parse(response.response);
                        if (resp.upload_id)
                            up.settings.multipart_params.upload_id = resp.upload_id;

                        if (up.settings.multipart_params.response_list == "") {
                            up.settings.multipart_params.response_list += resp.e_tag;
                        } else {
                            up.settings.multipart_params.response_list += "|" + resp.e_tag;
                        }
                    },
                    error: function (file, error) {
                        this.settings.multipart_params.response_list = "";
                        this.settings.multipart_params.upload_id = 0;

                        switch (error.code) {
                            case -600:
                                tinymce.activeEditor.windowManager.alert($filter('translate')('EMail.MaxFileSizeExceeded'));
                                break;
                            default:
                                break;
                        }
                        if (uploadFailedCallback) {
                            uploadFailedCallback();
                            uploadFailedCallback = null;
                        }
                    }
                }
            };

            /// tinymce editor configuration.
            function createTempMenu(editor) {
                var menuItems = [];
                ModuleService.getTemplates($scope.type, 'module')
                    .then(function (templateResponse) {
                        if (templateResponse.data.length !== 0) {
                            $scope.quoteTemplates = templateResponse.data;
                            for (var i = 0; i < $scope.quoteTemplates.length; i++) {
                                menuItems[i] = {
                                    text: $scope.quoteTemplates[i].name,
                                    id: i,
                                    onclick: function () {
                                        editor.setProgressState(true);
                                        $scope.addTemplate('attachment', $scope.quoteTemplates[this.settings.id], editor);

                                    }
                                };
                            }
                        }
                    });

                return menuItems;
            }

            $scope.tinymceOptions = {
                setup: function (editor) {
                    editor.addButton('addQuoteTemplate', {
                        type: 'menubutton',
                        text: $filter('translate')('EMail.AddPdfTemplate', { module: $rootScope.getLanguageValue($scope.module.languages, 'label', 'singular') }),
                        icon: false,
                        menu: createTempMenu(editor)
                    });
                    editor.addButton('addParameter', {
                        type: 'button',
                        text: $filter('translate')('EMail.AddParameter'),
                        onclick: function () {
                            tinymce.activeEditor.execCommand('mceInsertContent', false, '#');
                        }
                    });
                    editor.on("init", function () {
                        $scope.loadingModal = false;
                        $scope.editor = editor;
                    });
                },
                onChange: function (e) {
                    
                    // put logic here for keypress and cut/paste changes
                },
                inline: false,
                height: 200,
                language: $rootScope.language,
                plugins: [
                    "advlist autolink lists link image charmap print preview anchor",
                    "searchreplace visualblocks code fullscreen",
                    "insertdatetime table contextmenu paste imagetools wordcount textcolor colorpicker"
                ],
                toolbar: "addParameter | addQuoteTemplate | styleselect | bold italic underline | forecolor backcolor | alignleft aligncenter alignright alignjustify | link image imagetools | table bullist numlist  blockquote code fullscreen",
                menubar: 'false',
                templates: [
                    { title: 'Test template 1', content: 'Test 1' },
                    { title: 'Test template 2', content: 'Test 2' }
                ],
                skin: 'lightgray',
                theme: 'modern',

                file_picker_callback: function (callback, value, meta) {
                    // Provide file and text for the link dialog
                    uploadSuccessCallback = callback;

                    if (meta.filetype == 'file') {
                        var uploadButton = document.getElementById('uploadFile');
                        uploadButton.click();
                    }

                    // Provide image and alt text for the image dialog
                    if (meta.filetype == 'image') {
                        var uploadButton = document.getElementById('uploadImage');
                        uploadButton.click();
                    }
                },
                image_advtab: true,
                file_browser_callback_types: 'image file',
                paste_data_images: true,
                paste_as_text: true,
                spellchecker_language: $rootScope.language,
                images_upload_handler: function (blobInfo, success, failure) {
                    var blob = blobInfo.blob();
                    uploadSuccessCallback = success;
                    uploadFailedCallback = failure;
                    $scope.imgUpload.uploader.addFile(blob);
                    ///TODO: in future will be implemented to upload pasted data images into server.
                },
                init_instance_callback: function (editor) {
                    $scope.iframeElement = editor.iframeElement;
                },
                resize: false,
                width: '99,9%',
                toolbar_items_size: 'small',
                statusbar: false,
                convert_urls: false,
                remove_script_host: false

            };

            $scope.addCustomField = function ($event, customField) {
                /// adds custom fields to the html template.
                tinymce.activeEditor.execCommand('mceInsertContent', false, "{" + customField.name + "}");
            };

            $scope.addTemplate = function (type, quoteTemplate, editor) {
                if (!quoteTemplate)
                    return;

                $scope.templateAdding = {};
                $scope.templateAdding[type] = true;

                var primaryKey = $filter('filter')($scope.module.fields, { primary: true }, true)[0];
                var fileName = $scope.$parent.record[primaryKey.name] + '.pdf';

                if (quoteTemplate.link) {
                    if (type === 'link') {
                        tinymce.activeEditor.execCommand('mceInsertContent', false, '<a href="' + quoteTemplate.link.fileurl + '">' + fileName + '</a>');
                    } else {
                        $scope.attachmentLink = quoteTemplate.link.fileurl;
                        $scope.attachmentName = fileName.substring(0, 50);
                        $scope.quoteTemplateName = " ( " + quoteTemplate.name + " ) ";
                    }

                    $scope.templateAdding[type] = false;
                    editor.setProgressState(false);
                    $scope.$apply();
                } else {
                    var url = config.apiUrl + 'Document/export?module=' + $scope.type + '&id=' + $scope.$parent.record.id + "&templateId=" + quoteTemplate.id + '&access_token=' + $localStorage.read('access_token') + '&format=pdf&locale=' + $rootScope.locale + '&timezoneOffset=' + new Date().getTimezoneOffset() + '&save=' + true;

                    $http.get(url).then(function (response) {
                        quoteTemplate.link = response.data;

                        if (type === 'link') {
                            tinymce.activeEditor.execCommand('mceInsertContent', false, '<a href="' + response.data.fileurl + '">' + fileName + '</a>');
                        } else {
                            $scope.attachmentLink = response.data.fileurl;
                            $scope.attachmentName = fileName.substring(0, 50);
                            $scope.quoteTemplateName = " ( " + quoteTemplate.name + " ) ";
                        }

                        $scope.templateAdding[type] = false;
                        editor.setProgressState(false);
                    });
                }
            };

            $scope.submitEMail = function () {
                if (!$scope.emailModalForm.validate()) {
                    mdToast.error($filter('translate')('Module.RequiredError'));
                    return;
                }

                if (!$scope.isCc || !$scope.isBcc) {
                    mdToast.error($scope.nonEmails['cc'] || $scope.nonEmails['bcc']);
                    return;
                }

                $scope.selectedIds = [];
                $scope.selectedIds.push($scope.$parent.record.id);

                $scope.queryRequest = {};
                $scope.queryRequest.query = '*:*';
                var emailProviderType = $scope.senderAlias.type === $filter('translate')("EMail.System") ? 1 : 3; //1 = System, 3=Personal

                $scope.submittingModal = true;

                var sendEmail = function () {
                    ModuleService.sendEMail($scope.module.id,
                        $scope.selectedIds,
                        $scope.queryRequest.query,
                        false, //is all selected value
                        $scope.template,
                        $scope.emailField.name,
                        $scope.Cc,
                        $scope.Bcc,
                        $scope.senderAlias.alias,
                        $scope.senderAlias.email,
                        emailProviderType,
                        dialog_uid,
                        $scope.subject,
                        $scope.attachmentLink,
                        $scope.attachmentName).then(function (response) {
                            components.run('AfterSingleEmail', 'Script', $scope);
                            $scope.submittingModal = false;
                            $scope.close();
                            $scope.$parent.isAllSelected = false;
                            $scope.$parent.selectedRows = [];
                            if ($scope.$parent.emailSent) {
                                $scope.$parent.emailSent();
                            }
                            mdToast.success($filter('translate')('EMail.MessageQueued'));


                        })
                        .catch(function () {
                            $scope.submittingModal = false;
                            $scope.close();
                            $scope.isAllSelected = false;
                            $scope.selectedRows = [];
                            mdToast.error($filter('translate')('Common.Error'));
                        });
                };

                sendEmail();

            };

            //For Kendo IU or MD
            $scope.close = function () {
                $scope.template = {};
                $mdDialog.hide();
            };

            $scope.senderOptions = {
                dataSource: $scope.senders,
                valueTemplate: '<span class="k-state-default">{{dataItem.alias}} <{{dataItem.email}}> - {{dataItem.type}}  </span>',
                template: '<span class="k-state-default">{{dataItem.alias}} <{{dataItem.email}}> - {{dataItem.type}}  </span>',
                dataTextField: "alias",
                dataValueField: "email"
            };

            $scope.emailFieldOptions = {
                dataSource: $filter('filter')($scope.emailFields, { data_type: 'email' }, true),
                valueTemplate: '<span class="k-state-default">{{dataItem.label}}  {{dataItem.labelExt}}  </span>',
                template: '<span class="k-state-default">{{dataItem.label}}  {{dataItem.labelExt}}  </span>',
                dataTextField: "label",
                dataValueField: "name"
            };

            $scope.templateOptions = {
                dataSource: new kendo.data.DataSource({
                    transport: {
                        read: function (o) {
                            o.success($scope.templates)
                        }
                    }
                }),
                dataBound: $scope.templates,
                change: $scope.setContent,
                dataTextField: "name",
                dataValueField: "id",
            };

            $scope.users = $rootScope.users;

            $scope.sharesOptions = {
                dataSource: $scope.users,
                filter: "contains",
                dataTextField: "full_name",
                dataValueField: "id",
            };

            $scope.deleteTemplate = function () {
                kendo.confirm($filter('translate')('Common.AreYouSure'))
                    .then(function () {
                        $scope.templateDelete();
                        $scope.formType = 'email';
                    }, function () {

                    });
            };
            //For Kendo IU or MD

            $scope.checkEmails = function (emails, isCc) {

                if (emails) {
                    const emailArray = emails.split(',');
                    var nonEmails = $filter('filter')(emailArray, function (email) {
                        return email.indexOf('@') <= 0 || email.contains('*') || email.indexOf('@') >= email.length;
                    }, true);

                    if (nonEmails && nonEmails.length > 0) {
                        const message = $filter('translate')('Setup.Settings.ErrorEmail') + ' ' + nonEmails.toString();
                        mdToast.error(message);

                        condition(isCc, false);
                        $scope.nonEmails[isCc ? 'cc' : 'bcc'] = message;
                    }
                    else {
                        condition(isCc, true);
                        $scope.nonEmails[isCc ? 'cc' : 'bcc'] = '';
                    }
                } else {
                    condition(isCc, true);
                    $scope.nonEmails[isCc ? 'cc' : 'bcc'] = '';
                }
            };

            function condition(isCc, value) {
                if (isCc) {
                    $scope.isCc = value;
                } else {
                    $scope.isBcc = value;
                }
            }
        }
    ]);