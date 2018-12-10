'use strict';

angular.module('primeapps')
    .controller('BulkEMailController', ['$rootScope', '$scope', 'ngToast', '$filter', 'helper', '$location', '$state', '$stateParams', '$q', '$window', '$localStorage', '$cache', 'config', 'ModuleService', 'TemplateService',
        function ($rootScope, $scope, ngToast, $filter, helper, $location, $state, $stateParams, $q, $window, $localStorage, $cache, config, ModuleService, TemplateService) {
            $scope.loadingModal = true;
            $scope.module = $filter('filter')($rootScope.modules, { name: $stateParams.type }, true)[0];
            var uploadSuccessCallback,
                uploadFailedCallback;

            $scope.formType = 'email';

            if (!$rootScope.system.messaging.PersonalEMail && !$rootScope.system.messaging.SystemEMail) {
                ngToast.create({ content: $filter('translate')('EMail.MessageQueued'), className: 'danger' })
            }

            var systemEmailSettings = angular.copy($rootScope.system.messaging.SystemEMail || {}),
                personalEmailSettings = angular.copy($rootScope.system.messaging.PersonalEMail || {});

            var dialog_uid = plupload.guid();
            /// set default email field if available.
            $scope.emailField = $filter('filter')($scope.$parent.$parent.allFields, { data_type: 'email' })[0];
            $scope.senderAlias = null;
            $scope.senders = [];

            /// add system defined senders to the sender list, if exists.
            if (systemEmailSettings.senders) {
                if (systemEmailSettings.senders.length > 0) {
                    systemEmailSettings.senders.forEach(function (sender) {
                        sender.type = "System";
                        $scope.senders.push(sender);
                    });
                }
            }

            $scope.getTagTextRaw = function (item) {
                if (item.name.indexOf("seperator") >= 0) {

                } else {
                    return '<i style="color:#0f1015;font-style:normal">' + '{' + item.name + '}' + '</i>';
                }
            };
            $scope.moduleFields = TemplateService.getFields($scope.module);
            $scope.emailFields = [];

            angular.forEach($scope.moduleFields, function (item) {
                if (item.data_type === 'email' && !item.deleted && item.parent_type != 'users') {
                    $scope.emailFields.push(item);
                }
            });

            if ($scope.emailFields.length > 0)
                $scope.emailField = $scope.emailFields[0];

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

            /// add personal defined senders to the sender list, if exists.
            if (personalEmailSettings.senders) {
                if (personalEmailSettings.senders.length > 0) {
                    personalEmailSettings.senders.forEach(function (sender) {
                        sender.type = "Personal";
                        $scope.senders.push(sender);
                    });
                }
            }

            /// uploader configuration for image files.
            $scope.imgUpload = {
                settings: {
                    multi_selection: false,
                    url: config.apiUrl + 'Document/upload_attachment',
                    headers: {
                        'Authorization': 'Bearer ' + $localStorage.read('access_token'),
                        'Accept': 'application/json'
                    },
                    multipart_params: {
                        container: dialog_uid
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
                        tinymce.activeEditor.windowManager.close();
                        var resp = JSON.parse(response.response);
                        uploadSuccessCallback(resp.PublicURL, { alt: file.name });
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

            TemplateService.getAll('email', $scope.module.name)
                .then(function (response) {
                    $scope.templates = response.data;
                });

            $scope.addressType = function (type) {
                return $filter('translate')("EMail." + type);
            };

            /// uploader configuration for files.
            $scope.fileUpload = {
                settings: {
                    multi_selection: false,
                    unique_names: false,
                    url: config.apiUrl + 'Document/upload_attachment',
                    headers: {
                        'Authorization': 'Bearer ' + $localStorage.read('access_token'),
                        'Accept': 'application/json'
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
                        uploadSuccessCallback(resp.PublicURL, { alt: file.name });
                        uploadSuccessCallback = null;
                        tinymce.activeEditor.windowManager.close();
                    },
                    error: function (file, error) {
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
                        debugger;
                        // put logic here for keypress and cut/paste changes
                    },
                    inline: false,
                    height: 300,
                    language: $rootScope.language,
                    plugins: [
                        "advlist autolink lists link image charmap print preview anchor table",
                        "searchreplace visualblocks code fullscreen",
                        "insertdatetime table contextmenu paste imagetools wordcount textcolor colorpicker"
                    ],
                    imagetools_cors_hosts: ['crm.ofisim.com', 'test.ofisim.com', 'ofisimcomdev.blob.core.windows.net'],
                    toolbar: "addParameter | styleselect | bold italic underline strikethrough | forecolor backcolor | alignleft aligncenter alignright alignjustify | table bullist numlist | link image imagetools |  cut copy paste | undo redo searchreplace | outdent indent | blockquote hr insertdatetime charmap | visualblocks code preview fullscreen",
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

            $scope.newtemplate = {};
            $scope.newtemplate.system_type = 'custom';
            $scope.newtemplate.sharing_type = 'me';


            $scope.addCustomField = function ($event, customField) {
                /// adds custom fields to the html template.
                tinymce.activeEditor.execCommand('mceInsertContent', false, "{" + customField.name + "}");
            };

            $scope.showAddTemplate = function () {
                if (!$scope.quoteTemplate)
                    return;

                $scope.addTemplatePopover = $scope.addTemplatePopover || $popover(angular.element(document.getElementById('addTemplate')), {
                    templateUrl: 'view/app/email/addTemplate.html',
                    placement: 'bottom-right',
                    autoClose: true,
                    scope: $scope,
                    show: true
                });
            };

            $scope.addTemplate = function (type) {
                if (!$scope.quoteTemplate)
                    return;

                $scope.templateAdding = {};
                $scope.templateAdding[type] = true;

                var fileName = $scope.$parent.$parent.selectedRecords[0].displayName + '.pdf';

                if ($scope.quoteTemplate.link) {
                    if (type === 'link') {
                        tinymce.activeEditor.execCommand('mceInsertContent', false, '<a href="' + $scope.quoteTemplate.link.fileurl + '">' + fileName + '</a>');
                    }
                    else {
                        $scope.attachmentLink = $scope.quoteTemplate.link.fileurl;
                        $scope.attachmentName = fileName;
                    }

                    $scope.templateAdding[type] = false;
                    $scope.addTemplatePopover.hide();
                }
                else {
                    var url = config.apiUrl + 'Document/export?module=' + $scope.type + '&id=' + $scope.$parent.$parent.selectedRecords[0].id + "&templateId=" + $scope.quoteTemplate.id + '&access_token=' + $localStorage.read('access_token') + '&format=pdf&locale=' + $rootScope.locale + '&timezoneOffset=' + new Date().getTimezoneOffset() + '&save=' + true;

                    $http.get(url).then(function (response) {
                        $scope.quoteTemplate.link = response.data;

                        if (type === 'link') {
                            tinymce.activeEditor.execCommand('mceInsertContent', false, '<a href="' + response.data.fileurl + '">' + fileName + '</a>');
                        }
                        else {
                            $scope.attachmentLink = response.data.fileurl;
                            $scope.attachmentName = fileName;
                        }

                        $scope.templateAdding[type] = false;
                        $scope.addTemplatePopover.hide();
                    });
                }
            };

            $scope.submitEMail = function () {
                if (!$scope.emailModalForm.$valid)
                    return;

                var selectedIds = $scope.$parent.$parent.selectedRecords.map(function (row) {
                    return row.id;
                });

                $scope.queryRequest = {};

                if ($scope.$parent.$parent.isAllSelected)
                    $scope.queryRequest.query = angular.toJson($scope.$parent.$parent.findRequest);

                $scope.submittingModal = true;
                var emailProviderType = $scope.senderAlias.type == "System" ? 1 : 3; //1 = System, 3=Personal

                ModuleService.sendEMail($scope.$parent.$parent.module.id,
                    selectedIds,
                    $scope.queryRequest.query || null,
                    $scope.$parent.$parent.$parent.isAllSelected,
                    $scope.tinymceModel,
                    $scope.emailField.name,
                    $scope.Cc,
                    $scope.Bcc,
                    $scope.senderAlias.alias,
                    $scope.senderAlias.email,
                    emailProviderType,
                    dialog_uid,
                    $scope.Subject).then(function (response) {
                        $scope.submittingModal = false;
                        $scope.mailModal.hide();
                        $scope.$parent.$parent.$parent.isAllSelected = false;
                        $scope.templateSubject = $scope.Subject;
                        $scope.$parent.$parent.$parent.selectedRecords = [];
                        $scope.$parent.$parent.$parent.selectedRows = [];
                        if ($scope.$parent.$parent.emailSent) {
                            $scope.$parent.$parent.emailSent();
                        }
                        ngToast.create({ content: $filter('translate')('EMail.MessageQueued'), className: 'success' });
                    })
                    .catch(function () {
                        $scope.submittingModal = false;
                        $scope.mailModal.hide();
                        $scope.$parent.$parent.isAllSelected = false;
                        $scope.$parent.$parent.selectedRecords = [];
                        $scope.$parent.$parent.selectedRows = [];
                        ngToast.create({ content: $filter('translate')('Common.Error'), className: 'danger' });
                    });
            };

            $scope.setTemplate = function () {
                $scope.newtemplate.template_subject = $scope.Subject;
                $scope.newtemplate.tinymce_content = $scope.tinymceModel;
                if ($scope.currentTemplate) {
                    $scope.newtemplate.sharing_type = $scope.currentTemplate.sharing_type;
                    $scope.newtemplate.shares = $scope.currentTemplate.shares;
                }
                else {
                    $scope.newtemplate.sharing_type = 'me';
                }

            };


            $scope.backTemplate = function () {
                $scope.Subject = $scope.newtemplate.template_subject;
                $scope.tinymceModel = $scope.newtemplate.tinymce_content;
            };


            $scope.templateDelete = function () {
                var templates;
                templates = $scope.template;
                TemplateService.delete(templates)
                    .then(function () {
                        TemplateService.getAll('email', $scope.module.name)
                            .then(function (response) {
                                $scope.templates = response.data;
                            });
                        ngToast.create({ content: $filter('translate')('Template.SuccessDelete'), className: 'success' });

                    });
            };

            $scope.setContent = function (temp) {
                var template = $filter('filter')($scope.templates, { id: temp }, true)[0];

                if (temp) {
                    $scope.newtemplate.system_type = 'custom';
                    $scope.newtemplate.sharing_type = 'me';
                    $scope.tinymceModel = template.content;
                    $scope.Subject = template.subject;
                    $scope.currentTemplate = template;
                    $scope.newtemplate.template_name = template.name;

                }
                else {
                    $scope.tinymceModel = null;
                    $scope.Subject = null;
                    $scope.currentTemplate = null;
                    $scope.newtemplate.template_name = null;
                    $scope.newtemplate.template_subject = null;
                    $scope.newtemplate.tinymce_content = null;
                    $scope.newtemplate.sharing_type = null;
                    $scope.newtemplate.shares = null;
                }
            };

            $scope.templateSave = function () {
                var template = {};
                template.module = $scope.module.name;
                template.name = $scope.newtemplate.template_name;
                template.subject = $scope.newtemplate.template_subject;
                template.content = $scope.newtemplate.tinymce_content;
                template.sharing_type = $scope.newtemplate.sharing_type;
                template.template_type = 2;
                template.active = true;

                if ($scope.newtemplate.sharing_type === 'custom') {
                    template.shares = [];

                    angular.forEach($scope.newtemplate.shares, function (user) {
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
                    $scope.currentTemplate = saveResponse.data;
                    TemplateService.getAll('email', $scope.module.name)
                        .then(function (listResponse) {
                            $scope.templates = listResponse.data;
                            $scope.template = saveResponse.data.id;
                            ngToast.create({ content: $filter('translate')('Template.SuccessMessage'), className: 'success' });
                        });
                });
            }
        }
    ])
    ;