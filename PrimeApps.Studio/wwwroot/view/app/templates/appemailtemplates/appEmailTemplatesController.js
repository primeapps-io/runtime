'use strict';

angular.module('primeapps')

    .controller('AppEmailTemplatesController', ['$rootScope', '$scope', '$state', '$stateParams', '$location', '$filter', '$cache', '$q', 'helper', 'dragularService', 'operators', 'AppEmailTemplatesService', '$http', 'config', '$modal', '$localStorage', '$cookies',
        function ($rootScope, $scope, $state, $stateParams, $location, $filter, $cache, $q, helper, dragularService, operators, AppEmailTemplatesService, $http, config, $modal, $localStorage, $cookies) {

            $scope.templateModules = $filter('filter')($rootScope.appModules, {deleted: false});
            //$scope.$parent.menuTopTitle = "Settings";
            //$scope.$parent.activeMenu = 'templates';
            $scope.$parent.activeMenuItem = 'appEmailTemplates';

            $rootScope.breadcrumblist[2].title = 'Email Templates';

            $scope.loading = true;
            $scope.template = {};

            var dialog_uid = plupload.guid();
            var uploadSuccessCallback,
                uploadFailedCallback;

            // uploader configuration for image files.
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
                            {title: "Image files", extensions: "jpg,gif,png"},
                        ],
                        max_file_size: "2mb"
                    },
                    resize: {quality: 90}
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
                        uploadSuccessCallback(resp.public_url, {alt: file.name});
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

            // uploader configuration for files.
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
                            {title: "Email Attachments", extensions: "pdf,doc,docx,xls,xlsx,csv"},
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
                        uploadSuccessCallback(resp.public_url, {alt: file.name});
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
                        {title: 'Test template 1', content: 'Test 1'},
                        {title: 'Test template 2', content: 'Test 2'}
                    ],
                    skin: 'lightgray',
                    theme: 'modern',

                    file_picker_callback: function (callback, value, meta) {
                        // Provide file and text for the link dialog
                        uploadSuccessCallback = callback;

                        if (meta.filetype === 'file') {
                            var uploadButton = document.getElementById('uploadFile');
                            uploadButton.click();
                        }

                        // Provide image and alt text for the image dialog
                        if (meta.filetype === 'image') {
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
                    $scope.template.template_subject = template.subject;
                    $scope.template.tinymce_content = template.content;
                    $scope.template.template_name = template.name;
                    $scope.template.active = template.active;
                    $scope.template.language = template.language;
                    var mailInfo = JSON.parse(template.settings);
                    $scope.template.mail_sender_name = mailInfo["MailSenderName"];
                    $scope.template.mail_sender_email = mailInfo["MailSenderEmail"];
                    $scope.template.isNew = false;
                }
            };

            $scope.requestModel = {
                limit: '10',
                offset: 0
            };

            $scope.generator = function (limit) {
                $scope.placeholderArray = [];
                for (var i = 0; i < limit; i++) {
                    $scope.placeholderArray[i] = i;
                }
            };

            $scope.generator(10);
            $scope.activePage = 1;
            AppEmailTemplatesService.count($scope.currentApp.name).then(function (response) {
                $scope.pageTotal = response.data;
                $scope.changePage(1);
            });


            $scope.changePage = function (page) {
                $scope.loading = true;

                if (page !== 1) {
                    var difference = Math.ceil($scope.pageTotal / $scope.requestModel.limit);

                    if (page > difference) {
                        if (Math.abs(page - difference) < 1)
                            --page;
                        else
                            page = page - Math.abs(page - Math.ceil($scope.pageTotal / $scope.requestModel.limit))
                    }
                }

                $scope.activePage = page;
                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;

                AppEmailTemplatesService.find(requestModel, $scope.currentApp.name).then(function (response) {
                    $scope.templates = response.data;
                }).finally(function () {
                    $scope.loading = false;
                });
            };

            $scope.changeOffset = function () {
                $scope.changePage($scope.activePage);
            };

            $scope.templateSave = function (uploadForm) {

                if (!uploadForm.$valid)
                    return;

                $scope.saving = true;
                var template = {};
                template.name = $scope.template.template_name;
                template.subject = $scope.template.template_subject;
                template.content = $scope.template.tinymce_content;
                template.active = $scope.template.active;
                template.language = $scope.template.language;
                var mailInfo = '{ "MailSenderName":"' + $scope.template.mail_sender_name + '", "MailSenderEmail":"' + $scope.template.mail_sender_email + '"}';
                template.settings = mailInfo;

                if ($scope.currentTemplate) {
                    template.id = $scope.currentTemplate.id;
                    AppEmailTemplatesService.update(template).then(function () {
                        $scope.addNewEmailTemplateFormModal.hide();
                        $scope.changePage($scope.activePage);
                        toastr.success($filter('translate')('Template.SuccessMessage'));
                    }).finally(function () {
                        $scope.saving = false;
                    });

                } else {
                    AppEmailTemplatesService.create(template, $scope.currentApp.name).then(function () {
                        $scope.saving = false;
                        $scope.addNewEmailTemplateFormModal.hide();
                        $scope.pageTotal++;
                        $scope.changePage($scope.activePage);
                        toastr.success($filter('translate')('Template.SuccessMessage'));
                    });
                }

            };

            $scope.showFormModal = function (template) {
                if (template) {
                    $scope.setTemplate(template);
                    $scope.currentTemplate = template;
                } else {
                    $scope.template = {};
                    $scope.template.isNew = true;
                    $scope.currentTemplate = null;
                }

                $scope.addNewEmailTemplateFormModal = $scope.addNewEmailTemplateFormModal || $modal({
                    scope: $scope,
					templateUrl: 'view/app/templates/appemailtemplates/appEmailTemplatesForm.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
                });

                $scope.addNewEmailTemplateFormModal.$promise.then(function () {
                    $scope.addNewEmailTemplateFormModal.show();
                });
            };

            // $scope.delete = function (template) {
            //     var willDelete =
            //         swal({
            //             title: "Are you sure?",
            //             text: " ",
            //             icon: "warning",
            //             buttons: ['Cancel', 'Yes'],
            //             dangerMode: true
            //         }).then(function (value) {
            //             if (value) {
            //                 EmailTemplatesService.delete(template.id)
            //                     .then(function () {
            //                         var templateToDeleteIndex = helper.arrayObjectIndexOf($scope.templates, template);
            //                         $scope.templates.splice(templateToDeleteIndex, 1);
            //                         toastr.success("Email template is deleted successfully.", "Deleted!");
            //
            //                     })
            //                     .catch(function () {
            //
            //                         if ($scope.addNewHelpFormModal) {
            //                             $scope.addNewHelpFormModal.hide();
            //                             $scope.saving = false;
            //                         }
            //                     });
            //             }
            //         });
            // };
        }
    ]);