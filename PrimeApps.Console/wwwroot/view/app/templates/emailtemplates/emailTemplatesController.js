'use strict';

angular.module('primeapps')

    .controller('EmailTemplatesController', ['$rootScope', '$scope', '$state', '$stateParams', '$location', 'ngToast', '$filter', '$cache', '$q', 'helper', 'dragularService', 'operators', 'EmailTemplatesService', '$http', 'config', '$modal', '$localStorage', '$cookies',
        function ($rootScope, $scope, $state, $stateParams, $location, ngToast, $filter, $cache, $q, helper, dragularService, operators, EmailTemplatesService, $http, config, $modal, $localStorage, $cookies) {

            $scope.templateModules = $filter('filter')($scope.$parent.modules, { deleted: false });
            $scope.$parent.menuTopTitle = "Templates";
            $scope.$parent.activeMenu = 'templates';
            $scope.$parent.activeMenuItem = 'templatesEmail';

            $rootScope.breadcrumblist[0].link = '#/apps?organizationId=' + $rootScope.currentOrganization.id;
            $rootScope.breadcrumblist[1].link = '#/org/' + $rootScope.currentOrganization.id + '/app/' + $rootScope.appId + '/overview';
            $rootScope.breadcrumblist[2].title = 'Email Templates';

            $scope.loading = true;
            $scope.newtemplate = {};
            $scope.newtemplate.system_type = 'custom';
            $scope.newtemplate.sharing_type = 'me';
            var uploadSuccessCallback,
                uploadFailedCallback;

            var dialog_uid = plupload.guid();
            /// set default email field if available.
            $scope.senderAlias = null;
            $scope.senders = [];

            /// uploader configuration for image files.
            $scope.imgUpload = {
                settings: {
                    multi_selection: false,
                    url: config.apiUrl + 'Document/upload_attachment',
                    headers: {
                        'Authorization': 'Bearer ' + $localStorage.read('access_token'),
                        'Accept': 'application/json',
                        'X-Tenant-Id': $cookies.get('tenant_id')
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
                        'X-Tenant-Id': $cookies.get('tenant_id')
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

            $scope.addressType = function (type) {
                return $filter('translate')("EMail." + type);
            };

            $scope.setTemplate = function (template) {
                if (template) {
                    $scope.newtemplate.template_subject = template.subject;
                    $scope.newtemplate.tinymce_content = template.content;
                    $scope.newtemplate.sharing_type = template.sharing_type;
                    $scope.newtemplate.template_name = template.name;
                    var module = $filter('filter')($scope.$parent.modules, { name: template.module.name }, true)[0];
                    $scope.newtemplate.moduleName = module;

                    if (template.shares)
                        $scope.newtemplate.shares = template.shares;
                }
                else {
                    $scope.newtemplate.template_subject = $scope.Subject;
                    $scope.newtemplate.tinymce_content = $scope.tinymceModel;
                    if ($scope.currentTemplate) {
                        $scope.newtemplate.sharing_type = $scope.currentTemplate.sharing_type;
                        $scope.newtemplate.shares = $scope.currentTemplate.shares;
                    }
                    else {
                        $scope.newtemplate.sharing_type = 'me';
                    }
                }


            };

            // EmailTemplatesService.getAll(2).then(function (response) {
            //     $scope.templates = response.data;
            // }).finally(function () {
            //     $scope.loading = false;
            // });

            $scope.requestModel = {
                limit: '10',
                offset: 0
            };

            EmailTemplatesService.count().then(function (response) {
                $scope.pageTotal = response.data;
            });

            //2 templateType Module
            EmailTemplatesService.find($scope.requestModel, 2).then(function (response) {
                var templates = response.data;
                angular.forEach(templates, function (template) {
                    template.module = $filter('filter')($scope.$parent.modules, { name: template.module }, true)[0];
                });
                $scope.templates = templates;

            }).finally(function () {
                $scope.loading = false;
            });

            $scope.changePage = function (page) {
                $scope.loading = true;
                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;

                EmailTemplatesService.find(requestModel, 2).then(function (response) {
                    var templates = response.data;
                    angular.forEach(templates, function (template) {
                        template.module = $filter('filter')($scope.$parent.modules, { name: template.module }, true)[0];
                    });
                    $scope.templates = templates;

                }).finally(function () {
                    $scope.loading = false;
                });
            };

            $scope.changeOffset = function () {
                $scope.changePage(1);
            };

            $scope.templateSave = function () {
                $scope.saving = true;
                var template = {};
                template.module = $scope.newtemplate.moduleName.name;
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
                    result = EmailTemplatesService.update(template);
                    $scope.saving = false;
                    $scope.addNewEmailTemplateFormModal.hide();
                    $scope.changePage(1);
                    ngToast.create({ content: $filter('translate')('Template.SuccessMessage'), className: 'success' });
                }
                else {
                    result = EmailTemplatesService.create(template);
                    $scope.saving = false;
                    $scope.addNewEmailTemplateFormModal.hide();
                    $scope.changePage(1);
                    ngToast.create({ content: $filter('translate')('Template.SuccessMessage'), className: 'success' });
                }

                // result.then(function (saveResponse) {
                //     $scope.currentTemplate = saveResponse.data;
                //     EmailTemplatesService.getAll('email', $scope.module.name)
                //         .then(function (listResponse) {
                //             $scope.templates = listResponse.data;
                //             $scope.template = saveResponse.data.id;
                //             ngToast.create({ content: $filter('translate')('Template.SuccessMessage'), className: 'success' });
                //         });
                // });
            };

            $scope.showFormModal = function (template) {
                if (template) {
                    $scope.setTemplate(template);
                    $scope.currentTemplate = template;
                }
                else {
                    $scope.newtemplate = {};
                    $scope.newtemplate.system_type = 'custom';
                    $scope.newtemplate.sharing_type = 'me';
                    $scope.currentTemplate = null;
                }

                $scope.addNewEmailTemplateFormModal = $scope.addNewEmailTemplateFormModal || $modal({
                        scope: $scope,
                        templateUrl: 'view/app/templates/emailtemplates/emailTemplatesForm.html',
                        animation: 'am-fade-and-slide-right',
                        backdrop: 'static',
                        show: false
                    });

                $scope.addNewEmailTemplateFormModal.$promise.then(function () {
                    $scope.addNewEmailTemplateFormModal.show();
                });
            };

            $scope.delete = function (template) {
                const willDelete =
                    swal({
                        title: "Are you sure?",
                        text: "Are you sure that you want to delete this email template ?",
                        icon: "warning",
                        buttons: ['Cancel', 'Okey'],
                        dangerMode: true
                    }).then(function (value) {
                        if (value) {
                            EmailTemplatesService.delete(template.id)
                                .then(function () {
                                    var templateToDeleteIndex = helper.arrayObjectIndexOf($scope.templates, template);
                                    $scope.templates.splice(templateToDeleteIndex, 1);
                                    swal("Deleted!", "Your  email template has been deleted!", "success");

                                })
                                .catch(function () {

                                    if ($scope.addNewHelpFormModal) {
                                        $scope.addNewHelpFormModal.hide();
                                        $scope.saving = false;
                                    }
                                });
                        }
                    });
            };
        }
    ]);