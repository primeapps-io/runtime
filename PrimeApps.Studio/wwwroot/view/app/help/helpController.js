'use strict';

angular.module('primeapps')

    .controller('HelpController', ['$rootScope', '$scope', 'HelpService', '$filter', '$window', '$modal', 'config', '$localStorage', '$location', '$cache', '$state', '$cookies', 'helper',
        function ($rootScope, $scope, HelpService, $filter, $window, $modal, config, $localStorage, $location, $cache, $state, $cookies, helper) {
            $scope.moduleFilter = $filter('filter')($rootScope.appModules, { deleted: false });
            $scope.selectHelpRelation = 'any';
            $scope.isTimetrackerExist = false;
            $scope.$parent.collapsed = true;
            $scope.id = $location.search().id;
            $scope.activePage = 1;

            $scope.$parent.activeMenuItem = 'help';
            $rootScope.breadcrumblist[2].title = 'Help';
            $scope.helpModalObj = {};
            $scope.helpModalObj.selectHelp = 'modules';
            $scope.helpModalObj.selectHelpRelation = 'any';
            var location;
            $scope.loading = true;
            $scope.tab = 1;
            $scope.helpModalObj.relationType = false;
            $scope.editDisable = false;

            $scope.relationTypeShow = function () {
                if ($scope.helpModalObj.modulePicklist && $scope.helpModalObj.modulePicklist.id) {
                    $scope.helpModalObj.relationType = true;
                    $scope.helpModalObj.selectHelp = 'list';
                } else {
                    $scope.helpModalObj.relationType = false;
                    $scope.helpModalObj.selectHelp = null;
                }
            };

            if (!$scope.moduleFilter) {
                HelpService.getBasicModules().then(function (result) {
                    $scope.moduleFilter = $filter('filter')(result.data, { deleted: false });
                });
            }

            $scope.requestModel = {
                limit: "10",
                offset: 0
            };

            $scope.generator = function (limit) {
                $scope.placeholderArray = [];
                for (var i = 0; i < limit; i++) {
                    $scope.placeholderArray[i] = i;
                }
            };
            $scope.generator(10);

            HelpService.count().then(function (response) {
                $scope.pageTotal = response.data;
            });


            if ($scope.id) {
                location = window.location.hash.split('/')[3].split('?')[0];
            } else {
                location = window.location.hash.split('/')[3];
            }

            $scope.helpEnums = [
                {
                    Id: 1,
                    Name: 'module_list',
                    Label: $filter('translate')('Setup.HelpGuide.List'),
                }, {
                    Id: 2,
                    Name: 'module_detail',
                    Label: $filter('translate')('Setup.HelpGuide.Detail'),
                }, {
                    Id: 3,
                    Name: 'module_form',
                    Label: $filter('translate')('Setup.HelpGuide.Form'),
                }];

            // $scope.routeModule = [
            //     {
            //         "name": $filter('translate')('Setup.HelpGuide.FirstScreen'),
            //         "value": "firstscreen",
            //         "type": 2
            //     },
            //     {
            //         "name": $filter('translate')('Setup.HelpGuide.Dashboard'),
            //         "value": "/app/dashboard",
            //         "type": 2
            //     },
            //     {
            //         "name": $filter('translate')('Setup.HelpGuide.Newsfeed'),
            //         "value": "/app/newsfeed",
            //         "type": 2
            //     },
            //     {
            //         "name": $filter('translate')('Setup.HelpGuide.Report'),
            //         "value": "/app/reports",
            //         "type": 2
            //     },
            //     {
            //         "name": $filter('translate')('Setup.HelpGuide.Tasks'),
            //         "value": "/app/tasks",
            //         "type": 2
            //     },
            //     {
            //         "name": $filter('translate')('Setup.HelpGuide.Calendar'),
            //         "value": "/app/calendar",
            //         "type": 2
            //     }
            // ];

            $scope.helpModalObj.routeModuleSide = [
                {
                    "name": $filter('translate')('Setup.HelpGuide.Dashboard'),
                    "value": "/app/dashboard"
                },
                {
                    "name": $filter('translate')('Setup.HelpGuide.Newsfeed'),
                    "value": "/app/newsfeed"
                },
                {
                    "name": $filter('translate')('Setup.HelpGuide.Report'),
                    "value": "/app/reports"
                },
                {
                    "name": $filter('translate')('Setup.HelpGuide.Tasks'),
                    "value": "/app/tasks"
                },
                {
                    "name": $filter('translate')('Setup.HelpGuide.Calendar'),
                    "value": "/app/calendar"
                },
                {
                    "name": $filter('translate')('Setup.Nav.PersonalSettings'),
                    "value": "/app/setup/settings"
                },
                {
                    "name": $filter('translate')('Setup.Nav.Users'),
                    "value": "/app/setup/users"
                },
                {
                    "name": $filter('translate')('Setup.Nav.OrganizationSettings'),
                    "value": "/app/setup/organization"
                },
                {
                    "name": $filter('translate')('Setup.Nav.Customization'),
                    "value": "/app/setup/modules"
                },
                {
                    "name": $filter('translate')('Setup.Nav.Data'),
                    "value": "/app/setup/importhistory"
                },
                {
                    "name": $filter('translate')('Setup.Nav.System'),
                    "value": "/app/setup/general"
                },
                {
                    "name": $filter('translate')('Setup.Nav.Workflow'),
                    "value": "/app/setup/workflows"
                },
                {
                    "name": $filter('translate')('Setup.Nav.ApprovelProcess'),
                    "value": "/app/setup/approvel_process"
                },
            ];

            $scope.moduleFilter.push(
                {
                    "label_en_singular": $filter('translate')('Setup.HelpGuide.FirstScreen'),
                    "value": "firstscreen",
                    "type": "other"
                },
                {
                    "label_en_singular": $filter('translate')('Setup.HelpGuide.Dashboard'),
                    "value": "/app/dashboard",
                    "type": "other"
                },
                // {
                //     "label_en_singular": $filter('translate')('Setup.HelpGuide.Newsfeed'),
                //     "value": "/app/newsfeed",
                //     "type": "other"
                // },
                {
                    "label_en_singular": $filter('translate')('Setup.HelpGuide.Report'),
                    "value": "/app/reports",
                    "type": "other"
                }
                // {
                //     "label_en_singular": $filter('translate')('Setup.HelpGuide.Tasks'),
                //     "value": "/app/tasks",
                //     "type": "other"
                // },
                // {
                //     "label_en_singular": $filter('translate')('Setup.HelpGuide.Calendar'),
                //     "value": "/app/calendar",
                //     "type": "other"
                // }
            );

            var dialog_uid = plupload.guid();
            var uploadSuccessCallback,
                uploadFailedCallback;

            $scope.imgUpload = {
                settings: {
                    multi_selection: false,
                    url: 'storage/upload_attachment',
                    headers: {
                        'Authorization': 'Bearer ' + window.localStorage.getItem('access_token'),
                        'Accept': 'application/json',
                        'X-Organization-Id': $rootScope.currentOrgId,
                        'X-App-Id': $scope.appId
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
                        uploadSuccessCallback(config.storage_host + resp, { alt: file.name });
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

            var createHelpList = function () {
                var menuItems = [];

                HelpService.getCustomHelp('sidemodal', false)
                    .then(function (response) {
                        if (response.data && response.data.length != 0) {
                            $scope.helpTemplatesToolbar = response.data;


                            for (var i = 0; i < response.data.length; i++) {
                                menuItems[i] = {
                                    text: $scope.helpTemplatesToolbar[i].name,
                                    id: $scope.helpTemplatesToolbar[i].id,
                                    onclick: function () {
                                        tinymce.activeEditor.execCommand('mceInsertContent', false, '<a href="#" style="cursor: pointer" onclick="angular.element(document).scope().openHelp(' + this.settings.id + ');return false">' + this.settings.text + '</a>');
                                    }
                                };
                            }

                            menuItems.unshift({
                                text: 'Tümü',
                                id: 0,
                                onclick: function () {
                                    for (var i = 0; i < response.data.length; i++) {
                                        tinymce.activeEditor.execCommand('mceInsertContent', false, '<a href="#" style="cursor: pointer;display: block" onclick="angular.element(document).scope().openHelp(' + $scope.helpTemplatesToolbar[i].id + ');return false">' + $scope.helpTemplatesToolbar[i].name + '</a>');


                                    }
                                }
                            });
                        }
                    });

                return menuItems;
            };

            $scope.iframeElement = {};
            $scope.tinymceOptions = {

                setup: function (editor) {
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
                    "advlist autolink lists link image charmap print preview anchor",
                    "searchreplace visualblocks code fullscreen",
                    "insertdatetime media table contextmenu paste imagetools textcolor colorpicker"
                ],
                imagetools_cors_hosts: ['crm.ofisim.com', 'test.ofisim.com', 'ofisimcomdev.blob.core.windows.net'],
                toolbar: "insertfile undo redo | styleselect | bold italic | forecolor backcolor | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link image media imagetools | code preview fullscreen",
                menubar: 'false',
                templates: [
                    { title: 'Test template 1', content: 'Test 1' },
                    { title: 'Test template 2', content: 'Test 2' }
                ],
                skin: 'lightgray',
                resize: false,
                theme: 'modern',
                branding: false,
                valid_elements: '*[*]',

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
                spellchecker_language: $rootScope.language,
                images_upload_handler: function (blobInfo, success, failure) {
                    var blob = blobInfo.blob();
                    uploadSuccessCallback = success;
                    uploadFailedCallback = failure;
                    $scope.imgUpload.uploader.addFile(blob);
                    ///TODO: in future will be implemented to upload pasted data images into server.
                },
                convert_urls: false,
                remove_script_host: false
            };

            $scope.tinymceOptionsSide = {
                setup: function (editor) {
                    editor.addButton('helpTemplate', {
                        type: 'menubutton',
                        text: $filter('translate')('Yardımlar'),
                        icon: false,
                        menu: createHelpList()
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
                height: 800,
                language: $rootScope.language,
                plugins: [
                    "advlist autolink lists link image charmap print preview anchor",
                    "searchreplace visualblocks code fullscreen",
                    "insertdatetime media table contextmenu paste imagetools textcolor colorpicker"
                ],
                imagetools_cors_hosts: ['crm.ofisim.com', 'test.ofisim.com', 'ofisimcomdev.blob.core.windows.net'],
                toolbar: "insertfile undo redo | helpTemplate | styleselect | bold italic | forecolor backcolor | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link image media imagetools | code preview fullscreen",
                menubar: 'false',
                templates: [
                    { title: 'Test template 1', content: 'Test 1' },
                    { title: 'Test template 2', content: 'Test 2' }
                ],
                skin: 'lightgray',
                resize: false,
                theme: 'modern',
                branding: false,
                valid_elements: '*[*]',

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
                spellchecker_language: $rootScope.language,
                images_upload_handler: function (blobInfo, success, failure) {
                    var blob = blobInfo.blob();
                    uploadSuccessCallback = success;
                    uploadFailedCallback = failure;
                    $scope.imgUpload.uploader.addFile(blob);
                    ///TODO: in future will be implemented to upload pasted data images into server.
                },
                convert_urls: false,
                remove_script_host: false
            };

            // if ($filter('filter')($rootScope.modules, { name: 'timetrackers' }, true).length > 0)
            //     $scope.isTimetrackerExist = true;
            //
            // if ($scope.isTimetrackerExist) {
            //     $scope.routeModule.push({ 'name': $filter('translate')('Timetracker.Timetracker'), 'value': "/app/timetracker" });
            //     $scope.routeModuleSide.push({ 'name': $filter('translate')('Timetracker.Timetracker'), 'value': "/app/timetracker" });
            // }

            if (location === "helpside") {
                $scope.modalType = "sidemodal";
            } else {
                $scope.modalType = "modal";
            }

            if ($scope.id) {

                $scope.editDisable = true;

                HelpService.getById($scope.id)
                    .then(function (response) {
                        $scope.helpTemplatesSide = response.data;

                        if ($scope.helpTemplatesSide && !$scope.helpTemplatesSide.module_id && !$scope.helpTemplatesSide.route_url) {
                            $scope.helpModalObj.helpName = $scope.helpTemplatesSide.name;
                            $scope.helpModalObj.tinymceModel = $scope.helpTemplatesSide.template;
                        } else if ($scope.helpTemplatesSide && $scope.helpTemplatesSide.module_id) {
                            $scope.helpModalObj.selectHelpRelation = 'module';
                            if ($scope.helpTemplatesSide.module_type == "module_list") {
                                $scope.helpModalObj.selectHelp = 'list';
                                $scope.helpModalObj.helpName = $scope.helpTemplatesSide.name;
                                $scope.helpModalObj.tinymceModel = $scope.helpTemplatesSide.template;
                                var selectListModule = $filter('filter')($scope.moduleFilter, { id: $scope.helpTemplatesSide.module_id })[0];
                                $scope.helpModalObj.modulePicklist = selectListModule;
                            } else if ($scope.helpTemplatesSide.module_type == "module_detail") {
                                $scope.helpModalObj.selectHelp = 'detail';
                                $scope.helpModalObj.helpName = $scope.helpTemplatesSide.name;
                                $scope.helpModalObj.tinymceModel = $scope.helpTemplatesSide.template;
                                var selectDetailModule = $filter('filter')($scope.moduleFilter, { id: $scope.helpTemplatesSide.module_id })[0];
                                $scope.helpModalObj.moduleDetail = selectDetailModule;
                            } else {
                                $scope.helpModalObj.selectHelp = 'form';
                                $scope.helpModalObj.helpName = $scope.helpTemplatesSide.name;
                                $scope.helpModalObj.tinymceModel = $scope.helpTemplatesSide.template;
                                var selectFormModule = $filter('filter')($scope.moduleFilter, { id: $scope.helpTemplatesSide.module_id })[0];
                                $scope.helpModalObj.moduleForm = selectFormModule;
                            }
                        } else {
                            $scope.helpModalObj.selectHelpRelation = 'other';
                            $scope.helpModalObj.helpName = $scope.helpTemplatesSide.name;
                            $scope.helpModalObj.tinymceModel = $scope.helpTemplatesSide.template;
                            var selectOtherPicklist = $filter('filter')($scope.helpModalObj.routeModuleSide, { value: $scope.helpTemplatesSide.route_url })[0];
                            $scope.helpModalObj.routeModules = selectOtherPicklist;

                        }

                    });

            }

            $scope.setContent = function (helpTemplate) {
                if (helpTemplate) {
                    $scope.currentTemplate = helpTemplate;
                    var currentHelpModule;
                    if (helpTemplate.modal_type === "modal") {
                        if (helpTemplate.module_id)
                            currentHelpModule = $filter('filter')($scope.moduleFilter, { id: helpTemplate.module_id })[0];
                        else
                            currentHelpModule = $filter('filter')($scope.moduleFilter, { value: helpTemplate.route_url })[0];

                        $scope.helpModalObj.modulePicklist = currentHelpModule;
                        $scope.helpModalObj.helpName = helpTemplate.name;
                        $scope.helpModalObj.tinymceModel = helpTemplate.template;
                    } else {
                        if (helpTemplate.module_id) {
                            currentHelpModule = $filter('filter')($scope.moduleFilter, { id: helpTemplate.module_id })[0];
                            $scope.helpModalObj.relationType = true;
                            if (helpTemplate.module_type === "module_form")
                                $scope.helpModalObj.selectHelp = 'form';
                            else if (helpTemplate.module_type === "module_list")
                                $scope.helpModalObj.selectHelp = 'list';
                            else
                                $scope.helpModalObj.selectHelp = 'detail';
                        } else {
                            if (helpTemplate.route_url) {
                                currentHelpModule = $filter('filter')($scope.moduleFilter, { value: helpTemplate.route_url })[0];
                            }
                        }
                    }
                    $scope.editDisable = true;
                    $scope.helpModalObj.modulePicklist = currentHelpModule;
                    $scope.helpModalObj.helpName = helpTemplate.name;
                    $scope.helpModalObj.tinymceModel = helpTemplate.template;
                }
            };

            $scope.radioButtonTemplateClear = function () {
                if ($scope.helpModalObj.selectHelp === 'modules')
                    $scope.setContent();
                else
                    $scope.setContentSettingsModul();
            };

            $scope.helpModalSave = function (HelpPage) {

                if (!HelpPage.$valid) {
                    if (HelpPage.$error.required)
                        toastr.error($filter('translate')('Module.RequiredError'));

                    return;
                }


                var help = {};
                $scope.saving = true;
                if ($scope.helpModalObj.selectHelp === 'modules' && $scope.helpModalObj.modulePicklist.type != "other") {
                    var help = {};
                    help.module_id = $scope.helpModalObj.modulePicklist.id;
                    help.route_url = null;
                    help.first_screen = false;
                    help.template = $scope.helpModalObj.tinymceModel;
                    if (location === "helpside") {
                        help.modal_type = 2;
                    } else {
                        help.modal_type = 1;
                    }
                    help.show_type = 1;
                    help.module_type = 1;
                    help.name = $scope.helpModalObj.helpName;
                } else {
                    var help = {};

                    help.module_id = null;
                    help.route_url = $scope.helpModalObj.modulePicklist.value;
                    if ($scope.helpModalObj.modulePicklist.value === "firstscreen") {
                        help.first_screen = true;
                    }
                    help.template = $scope.helpModalObj.tinymceModel;
                    if (location === "helpside") {
                        help.modal_type = 2;
                    } else {
                        help.modal_type = 1;
                    }
                    help.show_type = 1;
                    help.module_type = 1;
                    help.name = $scope.helpModalObj.helpName;
                }

                if ($scope.currentTemplate) {
                    help.id = $scope.currentTemplate.id;
                    HelpService.update(help);
                    $cache.removeAll();
                    $scope.saving = false;
                    $scope.addNewHelpFormModal.hide();
                    toastr.success($filter('translate')('Setup.HelpGuide.HelPTemplateUpdate'));
                } else {
                    HelpService.create(help).then(function () {
                        HelpService.getByType($scope.modalType)
                            .then(function (response) {
                                $scope.helpTemplates = response.data;
                                $scope.saving = false;
                                $scope.addNewHelpFormModal.hide();
                                toastr.success($filter('translate')('Setup.HelpGuide.HelPTemplatePublish'));
                                $scope.grid.dataSource.read();
                            });

                    });

                }
            };

            $scope.helpSideSave = function (HelpPage) {

                if (!HelpPage.$valid) {
                    if (HelpPage.$error.required)
                        toastr.error($filter('translate')('Module.RequiredError'));

                    return;
                }


                var help = {};
                $scope.saving = true;

                if ($scope.helpModalObj.selectHelpRelation === 'any' && !$scope.helpModalObj.modulePicklist) {
                    help.module_id = null;
                    help.route_url = null;
                    help.template = $scope.helpModalObj.tinymceModel;
                    help.modal_type = 2;
                    help.show_type = 1;
                    help.module_type = 1;
                    help.name = $scope.helpModalObj.helpName;
                } else if ($scope.helpModalObj.modulePicklist.type === 'other') {
                    help.module_id = null;
                    help.route_url = $scope.helpModalObj.modulePicklist.value;
                    help.first_screen = false;
                    help.template = $scope.helpModalObj.tinymceModel;
                    help.modal_type = 2;
                    help.show_type = 1;
                    help.module_type = 1;
                    help.name = $scope.helpModalObj.helpName;
                } else {
                    if ($scope.helpModalObj.selectHelp === 'list') {
                        help.module_id = $scope.helpModalObj.modulePicklist.id;
                        help.route_url = null;
                        help.first_screen = false;
                        help.template = $scope.helpModalObj.tinymceModel;
                        help.modal_type = 2;
                        help.show_type = 1;
                        help.module_type = 1;
                        help.name = $scope.helpModalObj.helpName;
                    } else if ($scope.helpModalObj.selectHelp === 'detail') {
                        help.module_id = $scope.helpModalObj.modulePicklist.id;
                        help.route_url = null;
                        help.first_screen = false;
                        help.template = $scope.helpModalObj.tinymceModel;
                        help.modal_type = 2;
                        help.show_type = 1;
                        help.module_type = 2;
                        help.name = $scope.helpModalObj.helpName;
                    } else if ($scope.helpModalObj.selectHelp === 'form') {
                        help.module_id = $scope.helpModalObj.modulePicklist.id;
                        help.route_url = null;
                        help.first_screen = false;
                        help.template = $scope.helpModalObj.tinymceModel;
                        help.modal_type = 2;
                        help.show_type = 1;
                        help.module_type = 3;
                        help.name = $scope.helpModalObj.helpName;
                    }
                }

                if ($scope.currentTemplate) {
                    help.id = $scope.currentTemplate.id;
                    HelpService.update(help);
                    $scope.changeOffset();
                    $scope.saving = false;
                    $scope.addNewHelpFormSideModal.hide();
                    toastr.success($filter('translate')('Setup.HelpGuide.HelPTemplateUpdate'));
                } else {
                    $scope.moduleControl = false;
                    angular.forEach($scope.helpsides, function (item) {
                        var moduleTypeControl = $filter('filter')($scope.helpEnums, { Name: item.module_type })[0];

                        if (item.module_id === help.module_id && moduleTypeControl.id === help.module_type) {
                            $scope.moduleControl = true;
                        }
                    });

                    if (!$scope.moduleControl) {
                        HelpService.create(help).then(function () {
                            HelpService.getByType($scope.modalType)
                                .then(function (response) {
                                    $scope.helpTemplates = response.data;
                                    createHelpList();
                                    // $state.reload();
                                    $scope.saving = false;
                                    $scope.addNewHelpFormSideModal.hide();
                                    toastr.success($filter('translate')('Setup.HelpGuide.HelPTemplatePublish'));
                                    $scope.grid.dataSource.read();

                                });
                        });
                    } else {
                        toastr.warning($filter('translate')('Setup.HelpGuide.SomeModuleNotAvailable'));
                    }
                }
            };

            $scope.templateDelete = function () {
                var templates;
                templates = $scope.helpTemplates;
                HelpService.delete(templates.id)
                    .then(function () {
                        HelpService.getModuleType($scope.modalType, 'modulelist', $scope.helpModalObj.modulePicklist.id)
                            .then(function (response) {
                                $scope.helpTemplates = response.data;
                                $scope.helpModalObj.tinymceModel = null;
                            });
                        toastr.success($filter('translate')('Template.SuccessDelete'));
                    });
            };

            $scope.showFormModal = function (template) {
                $scope.currentTemplate = null;
                $scope.editDisable = false;
                $scope.helpModalObj = {};
                $scope.helpModalObj.selectHelp = 'modules';
                if (template)
                    $scope.setContent(template);

                $scope.addNewHelpFormModal = $scope.addNewHelpFormModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/help/helpPage.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
                });

                $scope.addNewHelpFormModal.$promise.then(function () {
                    $scope.addNewHelpFormModal.show();
                });
            };

            $scope.showFormSideModal = function (template) {
                $scope.currentTemplate = null;
                $scope.editDisable = false;
                $scope.helpModalObj = {};
                $scope.helpModalObj.selectHelpRelation = 'any';
                if (template)
                    $scope.setContent(template);

                $scope.addNewHelpFormSideModal = $scope.addNewHelpFormSideModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/help/helpPageSide.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
                });

                $scope.addNewHelpFormSideModal.$promise.then(function () {
                    $scope.addNewHelpFormSideModal.show();
                });
            };

            $scope.helpEdit = function (template) {
                if (template.modal_type == "modal")
                    $scope.showFormModal(template);
                else
                    $scope.showFormSideModal(template);
            };

            $scope.delete = function (helpside, event) {
                var willDelete =
                    swal({
                        title: "Are you sure?",
                        text: " ",
                        icon: "warning",
                        buttons: ['Cancel', 'Yes'],
                        dangerMode: true
                    }).then(function (value) {
                        if (value) {
                            var elem = angular.element(event.srcElement);
                            angular.element(elem.closest('tr')).addClass('animated-background');
                            HelpService.delete(helpside.id)
                                .then(function () {
                                    $scope.grid.dataSource.read();
                                    //var index = $rootScope.appModules.indexOf(module);
                                    // $rootScope.appModules.splice(index, 1);

                                    angular.element(document.getElementsByClassName('ng-scope animated-background')).remove();
                                    toastr.success("Help is deleted successfully.", "Deleted!");

                                })
                                .catch(function () {
                                    angular.element(document.getElementsByClassName('ng-scope animated-background')).removeClass('animated-background');
                                    if ($scope.addNewHelpFormModal) {
                                        $scope.addNewHelpFormModal.hide();
                                        $scope.saving = false;
                                    }
                                });
                        }
                    });
            };

            $scope.helpType = function (helpData) {
                if (helpData.modal_type == "modal")
                    return "Introduction";
                else
                    return "Help";
            };

            $scope.goUrl = function (emailTemp) {
                var selection = window.getSelection();
                if (selection.toString().length === 0) {
                    $scope.showFormModal(emailTemp);
                }
            };

            //For Kendo UI
            var accessToken = $localStorage.read('access_token');

            $scope.mainGridOptions = {
                dataSource: {
                    type: "odata-v4",
                    page: 1,
                    pageSize: 10,
                    serverPaging: true,
                    serverFiltering: true,
                    serverSorting: true,
                    transport: {
                        read: {
                            url: "/api/help/find",
                            type: 'GET',
                            dataType: "json",
                            beforeSend: function (req) {
                                req.setRequestHeader('Authorization', 'Bearer ' + accessToken);
                                req.setRequestHeader('X-App-Id', $rootScope.currentAppId);
                                req.setRequestHeader('X-Organization-Id', $rootScope.currentOrgId);
                            }
                        }
                    },
                    schema: {
                        data: "items",
                        total: "count",
                        model: {
                            id: "id",
                            fields: {
                                LabelEn: { type: "string" },
                                ModalType: { type: "enums" }
                            }
                        }
                    }
                },
                scrollable: false,
                persistSelection: true,
                sortable: true,
                filterable: {
                    extra: false
                },
                rowTemplate: function (help) {
                    var trTemp = '<tr ng-click="goUrl(dataItem)">';
                    var relationModule = $filter('filter')($scope.appModules, { id: help.module_id })[0];
                    var helpType = $scope.helpType(help, $scope.moduleFilter, $scope.helpEnums);
                    trTemp += '<td>' + help.name + '</td>';
                    trTemp += '<td>' + helpType + '</td>';
                    trTemp += '<td>' + relationModule.name + '</td>';
                    trTemp += '<td ng-click="$event.stopPropagation();"> <button ng-click="$event.stopPropagation(); delete(dataItem, $event);" type="button" class="action-button2-delete"><i class="fas fa-trash"></i></button></td></tr>';
                    return trTemp;
                },
                altRowTemplate: function (help) {
                    var trTemp = '<tr class="k-alt" ng-click="goUrl(dataItem)">';
                    var relationModule = $filter('filter')($scope.appModules, { id: help.module_id })[0];
                    var helpType = $scope.helpType(help, $scope.moduleFilter, $scope.helpEnums);
                    trTemp += '<td>' + help.name + '</td>';
                    trTemp += '<td>' + helpType + '</td>';
                    trTemp += '<td>' + relationModule.name + '</td>';
                    trTemp += '<td ng-click="$event.stopPropagation();"> <button ng-click="$event.stopPropagation(); delete(dataItem, $event);" type="button" class="action-button2-delete"><i class="fas fa-trash"></i></button></td></tr>';
                    return trTemp;
                },
                pageable: {
                    refresh: true,
                    pageSize: 10,
                    pageSizes: [10, 25, 50, 100],
                    buttonCount: 5,
                    info: true,
                },
                columns: [

                    {
                        field: 'Name',
                        title: $filter('translate')('Setup.HelpGuide.HelpScreenName'),
                    },
                    {
                        field: 'ModalType',
                        title: 'Type',
                        values: [
                            { text: 'Introduction', value: 'Modal' },
                            { text: 'Help', value: 'SideModal' }]
                    },

                    {
                        field: 'Module.Name',
                        title: $filter('translate')('Setup.HelpGuide.HelpScreenRelation'),
                    },
                    {
                        field: '',
                        title: '',
                        width: "90px"
                    }]
            };
        }
    ])
    ;