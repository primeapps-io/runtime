'use strict';

angular.module('primeapps')

    .controller('HelpController', ['$rootScope', '$scope', 'HelpService', 'ngToast', '$filter', '$window', '$modal', 'config', '$localStorage', '$location', '$cache', '$state', '$cookies',
        function ($rootScope, $scope, HelpService, ngToast, $filter, $window, $modal, config, $localStorage, $location, $cache, $state, $cookies) {
            $scope.moduleFilter = $filter('filter')($scope.$parent.modules, { deleted: false });
            $scope.selectHelp = 'modules';
            $scope.selectHelpRelation = 'any';
            $scope.isTimetrackerExist = false;
            $scope.$parent.collapsed = true;
            $scope.id = $location.search().id;
            $scope.$parent.activeMenu = 'app';
            $scope.$parent.activeMenuItem = 'help';
            var location;

            $scope.loading = true;
            $scope.requestModel = {
                limit: "10",
                offset: 0
            };

            HelpService.count().then(function (response) {
                $scope.pageTotal = response.data;
            });

            HelpService.find($scope.requestModel).then(function (response) {
                $scope.helpsides = HelpService.process(response.data, $scope.moduleFilter, $scope.routeModuleSide, $scope.helpEnums);
                $scope.loading = false;
            });

            $scope.changePage = function (page) {
                $scope.loading = true;
                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;

                HelpService.find(requestModel).then(function (response) {
                    $scope.helpsides = HelpService.process(response.data, $scope.moduleFilter, $scope.routeModuleSide, $scope.helpEnums);
                    $scope.loading = false;
                });

            };

            $scope.changeOffset = function () {
                $scope.changePage(1)
            };


            if ($scope.id) {
                location = window.location.hash.split('/')[3].split('?')[0];
            }
            else {
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

            $scope.routeModule = [
                {
                    "name": $filter('translate')('Setup.HelpGuide.FirstScreen'),
                    "value": "firstscreen"
                },
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
                }
            ];

            $scope.routeModuleSide = [
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

            var dialog_uid = plupload.guid();
            var uploadSuccessCallback,
                uploadFailedCallback;

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
            }
            else {
                $scope.modalType = "modal";
            }

            if ($scope.id) {

                $scope.editDisable = true;

                HelpService.getById($scope.id)
                    .then(function (response) {
                        $scope.helpTemplatesSide = response.data;

                        if ($scope.helpTemplatesSide && !$scope.helpTemplatesSide.module_id && !$scope.helpTemplatesSide.route_url) {
                            $scope.helpName = $scope.helpTemplatesSide.name;
                            $scope.tinymceModel = $scope.helpTemplatesSide.template;
                        }

                        else if ($scope.helpTemplatesSide && $scope.helpTemplatesSide.module_id) {
                            $scope.selectHelpRelation = 'module';
                            if ($scope.helpTemplatesSide.module_type == "module_list") {
                                $scope.selectHelp = 'list';
                                $scope.helpName = $scope.helpTemplatesSide.name;
                                $scope.tinymceModel = $scope.helpTemplatesSide.template;
                                var selectListModule = $filter('filter')($scope.moduleFilter, { id: $scope.helpTemplatesSide.module_id })[0];
                                $scope.modulePicklist = selectListModule;
                            }
                            else if ($scope.helpTemplatesSide.module_type == "module_detail") {
                                $scope.selectHelp = 'detail';
                                $scope.helpName = $scope.helpTemplatesSide.name;
                                $scope.tinymceModel = $scope.helpTemplatesSide.template;
                                var selectDetailModule = $filter('filter')($scope.moduleFilter, { id: $scope.helpTemplatesSide.module_id })[0];
                                $scope.moduleDetail = selectDetailModule;
                            }
                            else {
                                $scope.selectHelp = 'form';
                                $scope.helpName = $scope.helpTemplatesSide.name;
                                $scope.tinymceModel = $scope.helpTemplatesSide.template;
                                var selectFormModule = $filter('filter')($scope.moduleFilter, { id: $scope.helpTemplatesSide.module_id })[0];
                                $scope.moduleForm = selectFormModule;
                            }
                        }

                        else {
                            $scope.selectHelpRelation = 'other';
                            $scope.helpName = $scope.helpTemplatesSide.name;
                            $scope.tinymceModel = $scope.helpTemplatesSide.template;
                            var selectOtherPicklist = $filter('filter')($scope.routeModuleSide, { value: $scope.helpTemplatesSide.route_url })[0];
                            $scope.routeModules = selectOtherPicklist;

                        }

                    });

            }

            $scope.setContent = function () {
                if ($scope.modulePicklist) {
                    HelpService.getModuleType($scope.modalType, 'modulelist', $scope.modulePicklist.id)
                        .then(function (response) {
                            $scope.helpTemplates = response.data;

                            if ($scope.helpTemplates) {
                                $scope.helpName = $scope.helpTemplates.name;
                                $scope.tinymceModel = $scope.helpTemplates.template;
                                $scope.currentTemplate = $scope.helpTemplates;
                            }
                            else {
                                $scope.tinymceModel = null;
                                $scope.currentTemplate = null;
                            }
                        });
                }

                else {
                    $scope.helpName = null;
                    $scope.tinymceModel = null;
                    $scope.currentTemplate = null;
                }
            };

            $scope.setContentSettingsModul = function () {
                if ($scope.routeModules) {
                    HelpService.getByType($scope.modalType, null, $scope.routeModules.value)
                        .then(function (response) {
                            $scope.helpTemplates = response.data;

                            if ($scope.helpTemplates) {
                                $scope.tinymceModel = $scope.helpTemplates.template;
                                $scope.currentTemplate = $scope.helpTemplates;
                            }
                            else {
                                $scope.tinymceModel = null;
                                $scope.currentTemplate = null;
                            }
                        });
                }
                else {
                    $scope.tinymceModel = null;
                    $scope.currentTemplate = null;
                }
            };


            $scope.radioButtonTemplateClear = function () {
                if ($scope.selectHelp === 'modules')
                    $scope.setContent();
                else
                    $scope.setContentSettingsModul();
            };

            $scope.helpModalSave = function () {
                var help = {};

                if ($scope.selectHelp === 'modules') {
                    var help = {};
                    help.module_id = $scope.modulePicklist.id;
                    help.route_url = null;
                    help.first_screen = false;
                    help.template = $scope.tinymceModel;
                    if (location === "helpside") {
                        help.modal_type = 2;
                    }
                    else {
                        help.modal_type = 1;
                    }
                    help.show_type = 1;
                    help.module_type = 1;
                    help.name = "modal";
                }

                else {
                    var help = {};

                    help.module_id = null;
                    help.route_url = $scope.routeModules.value;
                    if ($scope.routeModules.value === "firstscreen") {
                        help.first_screen = true;
                    }
                    help.template = $scope.tinymceModel;
                    if (location === "helpside") {
                        help.modal_type = 2;
                    }
                    else {
                        help.modal_type = 1;
                    }
                    help.show_type = 1;
                    help.module_type = 1;
                    help.name = "modal";
                }

                if ($scope.currentTemplate || $scope.id) {

                    help.id = $scope.helpTemplates.id;
                    HelpService.update(help);
                    $cache.removeAll();
                    ngToast.create({ content: $filter('translate')('Setup.HelpGuide.HelPTemplateUpdate'), className: 'success' });
                }
                else {
                    HelpService.create(help).then(function () {
                        HelpService.getByType($scope.modalType)
                            .then(function (response) {
                                $scope.helpTemplates = response.data;
                                ngToast.create({ content: $filter('translate')('Setup.HelpGuide.HelPTemplatePublish'), className: 'success' });
                            });

                    });

                }
            };

            $scope.helpModalDraftSave = function () {
                var help = {};


                help.module_id = $scope.modulePicklist.id;
                help.route_url = null;
                help.template = $scope.tinymceModel;
                if (location === "helpside") {
                    help.modal_type = 2;
                }
                else {
                    help.modal_type = 1;
                }
                help.show_type = 2;
                help.module_type = 1;
                help.name = $scope.modulePicklist.label_tr_singular;

                if ($scope.currentTemplate || $scope.id) {

                    if ($scope.id) {
                        help.id = $scope.id;
                    }
                    else {
                        help.id = $scope.helpTemplates.id;
                    }
                    HelpService.update(help);
                    $cache.removeAll();
                    ngToast.create({ content: $filter('translate')('Setup.HelpGuide.HelPTemplateDraftUpdate'), className: 'success' });
                }
                else {
                    HelpService.create(help).then(function () {
                        HelpService.getByType($scope.modalType)
                            .then(function (response) {
                                $scope.helpTemplates = response.data;
                                $cache.removeAll();
                                ngToast.create({ content: $filter('translate')('Setup.HelpGuide.HelPTemplateDraftSave'), className: 'success' });

                            });

                    });
                }
            };

            $scope.helpSideSave = function () {
                var help = {};

                if ($scope.selectHelpRelation === 'any') {
                    help.module_id = null;
                    help.route_url = null;
                    help.template = $scope.tinymceModel;
                    if (location === "helpside") {
                        help.modal_type = 2;
                    }
                    else {
                        help.modal_type = 1;
                    }
                    help.show_type = 1;
                    help.module_type = 1;
                    help.name = $scope.helpName;

                }
                else if ($scope.selectHelp === 'list') {
                    help.module_id = $scope.modulePicklist.id;
                    help.route_url = null;
                    help.first_screen = false;
                    help.template = $scope.tinymceModel;
                    if (location === "helpside") {
                        help.modal_type = 2;
                    }
                    else {
                        help.modal_type = 1;
                    }
                    help.show_type = 1;
                    help.module_type = 1;
                    help.name = $scope.helpName;
                }

                else if ($scope.selectHelp === 'detail') {
                    help.module_id = $scope.moduleDetail.id;
                    help.route_url = null;
                    help.first_screen = false;
                    help.template = $scope.tinymceModel;
                    if (location === "helpside") {
                        help.modal_type = 2;
                    }
                    else {
                        help.modal_type = 1;
                    }
                    help.show_type = 1;
                    help.module_type = 2;
                    help.name = $scope.helpName;
                }

                else if ($scope.selectHelp === 'form') {
                    help.module_id = $scope.moduleForm.id;
                    help.route_url = null;
                    help.first_screen = false;
                    help.template = $scope.tinymceModel;
                    if (location === "helpside") {
                        help.modal_type = 2;
                    }
                    else {
                        help.modal_type = 1;
                    }
                    help.show_type = 1;
                    help.module_type = 3;
                    help.name = $scope.helpName;
                }

                else if ($scope.selectHelpRelation === 'other') {
                    help.module_id = null;
                    help.route_url = $scope.routeModules.value;
                    help.first_screen = false;
                    help.template = $scope.tinymceModel;
                    if (location === "helpside") {
                        help.modal_type = 2;
                    }
                    else {
                        help.modal_type = 1;
                    }
                    help.show_type = 1;
                    help.module_type = 1;
                    help.name = $scope.helpName;
                }


                if ($scope.currentTemplate || $scope.id) {

                    if ($scope.id) {
                        help.id = $scope.id;
                    }
                    else {
                        help.id = $scope.helpTemplates.id;
                    }
                    HelpService.update(help);
                    $cache.removeAll();
                    $state.go('app.setup.helpsides');
                    ngToast.create({ content: $filter('translate')('Setup.HelpGuide.HelPTemplateUpdate'), className: 'success' });
                }
                else {
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
                                    $state.go('app.setup.helpsides');
                                    ngToast.create({ content: $filter('translate')('Setup.HelpGuide.HelPTemplatePublish'), className: 'success' });

                                });
                        });
                    }
                    else {
                        ngToast.create({ content: $filter('translate')('Setup.HelpGuide.SomeModuleNotAvailable'), className: 'warning' });

                    }


                }
            };

            $scope.helpSideDraftSave = function () {
                var help = {};

                if ($scope.selectHelpRelation === 'any') {
                    help.module_id = null;
                    help.route_url = null;
                    help.template = $scope.tinymceModel;
                    if (location === "helpside") {
                        help.modal_type = 2;
                    }
                    else {
                        help.modal_type = 1;
                    }
                    help.show_type = 2;
                    help.module_type = 1;
                    help.name = $scope.helpName;

                }

                else if ($scope.selectHelp === 'list') {
                    help.module_id = $scope.modulePicklist.id;
                    help.route_url = null;
                    help.template = $scope.tinymceModel;
                    if (location === "helpside") {
                        help.modal_type = 2;
                    }
                    else {
                        help.modal_type = 1;
                    }
                    help.show_type = 2;
                    help.module_type = 1;
                    help.name = $scope.helpName;

                }

                else if ($scope.selectHelp === 'detail') {
                    help.module_id = $scope.moduleDetail.id;
                    help.route_url = null;
                    help.template = $scope.tinymceModel;
                    if (location === "helpside") {
                        help.modal_type = 2;
                    }
                    else {
                        help.modal_type = 1;
                    }
                    help.show_type = 2;
                    help.module_type = 2;
                    help.name = $scope.helpName;
                }

                else if ($scope.selectHelp === 'form') {
                    help.module_id = $scope.moduleForm.id;
                    help.route_url = null;
                    help.template = $scope.tinymceModel;
                    if (location === "helpside") {
                        help.modal_type = 2;
                    }
                    else {
                        help.modal_type = 1;
                    }
                    help.show_type = 2;
                    help.module_type = 3;
                    help.name = $scope.helpName;
                }

                else if ($scope.selectHelpRelation === 'other') {
                    help.module_id = null;
                    help.route_url = $scope.routeModules.value;
                    if ($scope.routeModules.value === "firstscreen") {
                        help.first_screen = true;
                    }
                    help.template = $scope.tinymceModel;
                    if (location === "helpside") {
                        help.modal_type = 2;
                    }
                    else {
                        help.modal_type = 1;
                    }
                    help.show_type = 2;
                    help.module_type = 1;
                    help.name = $scope.helpName;
                }


                if ($scope.currentTemplate || $scope.id) {

                    if ($scope.id) {
                        help.id = $scope.id;
                    }
                    else {
                        help.id = $scope.helpTemplates.id;
                    }
                    HelpService.update(help);
                    $cache.removeAll();
                    $state.go('app.setup.helpsides');
                    ngToast.create({ content: $filter('translate')('Setup.HelpGuide.HelPTemplateDraftUpdate'), className: 'success' });
                }
                else {
                    HelpService.create(help).then(function () {
                        HelpService.getByType($scope.modalType)
                            .then(function (response) {
                                $scope.helpTemplates = response.data;
                                // createHelpList();
                                // $state.reload();
                                $state.go('app.setup.helpsides');
                                ngToast.create({ content: $filter('translate')('Setup.HelpGuide.HelPTemplateDraftSave'), className: 'success' });
                            });

                    });
                }
            };

            $scope.templateDelete = function () {
                var templates;
                templates = $scope.helpTemplates;
                HelpService.delete(templates.id)
                    .then(function () {
                        HelpService.getModuleType($scope.modalType, 'modulelist', $scope.modulePicklist.id)
                            .then(function (response) {
                                $scope.helpTemplates = response.data;
                                $scope.tinymceModel = null;
                            });
                        ngToast.create({ content: $filter('translate')('Template.SuccessDelete'), className: 'success' });

                    });
            };


            $scope.delete = function (id) {
                HelpService.delete(id)
                    .then(function () {

                    });
            };

        }
    ]);