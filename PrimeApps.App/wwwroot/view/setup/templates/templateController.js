'use strict';

angular.module('primeapps')

    .controller('TemplateController', ['$rootScope', '$scope', '$filter', '$state', 'helper', '$localStorage', 'config', 'TemplateService', 'FileUploader', 'mdToast', '$mdDialog', 'AppService', '$window', '$timeout',
        function ($rootScope, $scope, $filter, $state, helper, $localStorage, config, TemplateService, FileUploader, mdToast, $mdDialog, AppService, $window, $timeout) {

            $scope.loading = true;
            $scope.searchOldValues = {};
            $scope.showAddParameter = false;

            AppService.checkPermission().then(function (res) {

                if (res && res.data) {
                    var profile = JSON.parse(res.data["profile"]);
                    var customProfilePermissions = undefined;
                    if (res.data["customProfilePermissions"])
                        customProfilePermissions = JSON.parse(res.data["customProfilePermissions"]);

                    $scope.hasEmailPermission = $scope.hasSMSPermission = $scope.hasExcelPermission = $scope.hasDocumentPermission = true;

                    if (!profile.HasAdminRights) {
                        var profileIsExist = undefined;
                        if (customProfilePermissions)
                            profileIsExist = customProfilePermissions.permissions.indexOf('profiles') > -1;

                        if (!profileIsExist) {
                            if (!profile.SendEmail && !profile.SendSMS && !profile.ExportData && !profile.WordPdfDownload) {
                                mdToast.error($filter('translate')('Common.Forbidden'));
                                $state.go('app.dashboard');
                            } else {
                                $scope.hasEmailPermission = profile.SendEmail;
                                $scope.hasSMSPermission = profile.SendSMS;
                                $scope.hasExcelPermission = profile.ExportData;
                                $scope.hasDocumentPermission = profile.WordPdfDownload;
                            }
                        }
                    }
                }

                $rootScope.breadcrumblist = [
                    {
                        title: $filter('translate')('Layout.Menu.Dashboard'),
                        link: "#/app/dashboard"
                    },
                    {
                        title: $filter('translate')('Layout.Menu.Templates'),
                        link: '#/app/setup/templates'
                    },
                    {
                        title: null
                    }
                ];

                if (profile.HasAdminRights || $scope.hasEmailPermission)
                    $scope.templateActiveTab = $state.params.tab ? $state.params.tab : 'email';
                else if ($scope.hasDocumentPermission)
                    $scope.templateActiveTab = $state.params.tab ? $state.params.tab : 'document';
                else if ($scope.hasExcelPermission)
                    $scope.templateActiveTab = $state.params.tab ? $state.params.tab : 'excel';
                else
                    $scope.templateActiveTab = $state.params.tab ? $state.params.tab : 'sms';

                var docFilter = {
                    name: 'docFilter',
                    fn: function (item, options) {
                        var extension = helper.getFileExtension(item.name);
                        return extension === 'txt' || extension === 'docx' || extension === 'pdf' || extension === 'doc';
                    }
                };

                var excelFilter = {
                    name: 'excelFilter',
                    fn: function (item, options) {
                        var extension = helper.getFileExtension(item.name);
                        return extension === 'xls' || extension === 'xlsx';
                    }
                };

                $scope.goUrl = function (tabKey) {

                    $rootScope.breadcrumblist[2].title = $filter('translate')('Setup.Nav.Tabs.' + tabKey.charAt(0).toUpperCase() + tabKey.substr(1).toLowerCase() + 'Template');

                    if ($scope.gridOptions && $scope.grid) {
                        $scope.grid.dataSource.filter($scope.filters[tabKey]);
                        //I do not know why more than once triggered.
                    }
                    $state.go('app.setup.templates', { tab: tabKey }, { notify: false });
                };

                var sizeFilter = {
                    name: 'sizeFilter',
                    fn: function (item) {
                        return item.size < 10485760;//10 mb
                    }
                };

                $scope.changeTemplateActiveTab = function (activeTab) {
                    $scope.templateActiveTab = activeTab;
                    //We have to change fileUpload filters when we were changing activetab
                    var docIndex = fileUpload.filters.indexOf(docFilter);
                    var excelIndex = fileUpload.filters.indexOf(excelFilter);
                    if (activeTab === 'document') {
                        if (docIndex === -1) {
                            fileUpload.filters[fileUpload.filters.length - 1] = docFilter;
                            excelIndex = fileUpload.filters.indexOf(excelFilter);
                            if (excelIndex > -1)
                                delete fileUpload.filters[excelIndex];
                        }
                    } else if (activeTab === 'excel') {
                        if (excelIndex === -1) {
                            fileUpload.filters[fileUpload.filters.length - 1] = excelFilter;
                            if (docIndex > -1)
                                delete fileUpload.filters[docIndex];
                        }
                    }

                    if (activeTab === 'document' || activeTab === 'excel') {
                        var sizeFilterIndex = fileUpload.filters.indexOf(sizeFilter);
                        if (sizeFilterIndex === -1)
                            fileUpload.filters.push(sizeFilter);
                        fileUpload.filters = fileUpload.filters.filter(function (item) {
                            return item;
                        })
                    }

                    $scope.fileUpload = fileUpload;
                };

                $scope.currentChangeModule = function () {
                    $scope.module = $filter('filter')($rootScope.modules, { name: $scope.current.module }, true)[0];
                    $scope.moduleFields = TemplateService.getFields($scope.module);
                    $scope.moduleRequired();
                };

                $scope.searchTags = function (term) {
                    var tagsList = [];
                    $timeout(function () {
                        if ($scope.moduleFields && $scope.moduleFields.length > 0) {
                            for (var j = 0; j < $scope.moduleFields.length; j++) {
                                var item = $scope.moduleFields[j];
                                if (item.name === "seperator")
                                    return;

                                if (item.name && item.name.match('seperator'))
                                    item.name = item.label;

                                if (item.name && item.name.indexOf(term) >= 0) {
                                    tagsList.push(item);
                                }
                            }

                            $scope.tags = tagsList;
                            return tagsList;
                        }
                    }, 150);
                };

                $scope.getTagTextRaw = function (item) {
                    $timeout(function () {
                        $scope.$broadcast("$tinymce:refreshContent");
                    }, 50);

                    if (item.name.indexOf("seperator") < 0) {
                        return '<i style="color:#0f1015;font-style:normal">' + '{' + item.name + '}' + '</i>';
                    }
                };

                var header = {
                    'Authorization': 'Bearer ' + window.localStorage.getItem('access_token'),
                    'Accept': 'application/json',
                    'x-app-id': $rootScope.user.app_id,

                };

                $scope.download = function (temp) {
                    $window.open('/storage/download_template?fileId=' + temp.id + "&tempType=" + temp.template_type, "_blank");
                };

                var fileUpload = $scope.fileUpload = new FileUploader({
                    url: 'storage/upload_template',
                    chunk_size: '256kb',
                    queueLimit: 1
                });

                fileUpload.onAfterAddingFile = function (item) {

                    var reader = new FileReader();

                    // reader.onload = function (event) {
                    //     $scope.$apply(function () {
                    //         item.template = event.target.result;
                    //     });
                    // };
                    reader.readAsDataURL(item._file);
                };

                fileUpload.onWhenAddingFileFailed = function (item, filter, options) {
                    switch (filter.name) {
                        case 'docFilter':
                            mdToast.warning($filter('translate')('Setup.Settings.DocumentTypeError'));
                            break;
                        case 'excelFilter':
                            mdToast.warning($filter('translate')('Data.Import.FormatError'));
                            break;
                        case 'sizeFilter':
                            mdToast.warning($filter('translate')('Documents.SizeError'));
                            break;
                    }
                };

                fileUpload.filters.push({
                    name: 'sizeFilter',
                    fn: function (item) {
                        return item.size < 10485760;//10 mb
                    }
                });

                fileUpload.onAfterAddingFile = function (fileItem) {
                    $scope.current.content = fileItem._file.name;
                    $scope.requiredColor = undefined;
                };

                $scope.remove = function () {
                    if (fileUpload.queue[0]) {
                        fileUpload.queue[0].remove();
                    }

                    $scope.current.content = undefined;
                    $scope.templateFileCleared = true;
                };

                $scope.showSideModal = function () {
                    //If user set any template before that, we have to delete that ago the open new template modal
                    if (fileUpload.queue[0]) {
                        fileUpload.queue[0].remove();
                    }
                    $rootScope.sideLoad = false;
                    $rootScope.buildToggler('sideModal', 'view/setup/templates/templateForm.html');
                    $scope.loadingModal = false;
                    $scope.templateFileCleared = false;
                    $scope.saving = false;
                    $scope.showAddParameter = false;
                    if ($scope.current.module) {
                        $scope.module = $filter('filter')($rootScope.modules, { name: $scope.current.module }, true)[0];
                        $scope.moduleFields = TemplateService.getFields($scope.module);
                    }
                };

                $scope.openFormModal = function (type) {
                    if (type === 'new') {
                        $scope.current = { active: true };
                        $scope.current.profile = [];
                        $scope.current.sharesData = [];
                        $scope.current.sharing_type = 'me';
                    }

                    $scope.showSideModal();
                };

                $scope.moduleRequired = function () {
                    var modulSelect = document.getElementById("module");
                    var modulSelectSpan = document.getElementsByClassName("k-dropdown")[1];
                    if (modulSelect.value === null || modulSelect.value === undefined || modulSelect.value === "? undefined:undefined ?" || modulSelect.value === "") {
                        modulSelectSpan.className = "k-widget k-dropdown form-control ng-pristine ng-empty ng-invalid ng-invalid-required k-valid ng-touched";
                    } else {
                        modulSelectSpan.className = "k-widget k-dropdown form-control ng-pristine ng-untouched ng-empty ng-invalid ng-invalid-required";
                    }
                };
                $scope.save = function (templateForm) {

                    $scope.saving = true;
                    var isFileUploadValid = true;

                    $scope.moduleRequired();
                    if ($scope.templateActiveTab === 'document' || $scope.templateActiveTab === 'excel') {
                        var fileRequired = document.getElementById("fileUploadReq");
                        if (fileRequired) {
                            isFileUploadValid = false;
                            fileRequired.style = "color:#ff000075";
                        }
                    }

                    var stripedHtml = "";
                    if ($scope.current.content)
                        stripedHtml = $scope.current.content.replace(/<[^>]+>/g, '').replace(/&[^;]+;/g, "").replace(/\n/g, "");

                    if ($scope.templateActiveTab === 'email' && (templateForm.tinymceModel.$viewValue === "" || stripedHtml === "")) {
                        mdToast.error($filter('translate')('Template.ContentRequired'));
                        $scope.saving = false;
                        return false;
                    }

                    if ($scope.current.sharing_type === 'profile' && (!$scope.current.profile || $scope.current.profile.length === 0)
                        || $scope.current.sharing_type === 'custom' && (!$scope.current.sharesData || $scope.current.sharesData.length === 0)) {
                        $scope.saving = false;
                        return false
                    }

                    if (!templateForm.$valid || !isFileUploadValid) {
                        $scope.saving = false;
                        return false;
                    }

                    if ($scope.current.sharing_type === 'profile' && $scope.current.profile && $scope.current.profile.length > 0) {
                        var profiles = [];
                        for (var i = 0; i < $scope.current.profile.length; i++) {
                            profiles.push($scope.current.profile[i].id);
                        }
                        $scope.current.profiles = profiles;
                    } else {
                        $scope.current.profiles = null;
                    }

                    if ($scope.current.sharing_type === 'custom' && $scope.current.sharesData && $scope.current.sharesData.length > 0) {
                        var shares = [];
                        for (var i = 0; i < $scope.current.sharesData.length; i++) {
                            shares.push($scope.current.sharesData[i].id);
                        }
                        $scope.current.shares = shares;
                    } else {
                        $scope.current.shares = [];
                    }

                    switch ($scope.templateActiveTab) {
                        case "email":
                            $scope.current.template_type = 'email';
                            break;
                        case "document":
                            $scope.current.template_type = 'module';
                            $scope.current.subject = 'Document';
                            break;
                        case "excel":
                            $scope.current.template_type = 'excel';
                            $scope.current.subject = 'Excel';
                            break;
                        case "sms":
                            $scope.current.template_type = 'email';
                            $scope.current.subject = 'SMS';
                            break;
                        default:
                            break;
                    }

                    if ($scope.templateActiveTab === 'document' || $scope.templateActiveTab === 'excel') {
                        fileUpload.queue[0].uploader.headers = header;
                        fileUpload.queue[0].headers = header;
                        fileUpload.queue[0].upload();
                        fileUpload.onCompleteItem = function (fileItem, tempInfo, status) {
                            if (status === 200) {
                                $scope.current.content = tempInfo.unique_name;
                                $scope.current.chunks = tempInfo.chunks;
                                $scope.current.content = tempInfo.unique_name;
                                $scope.current.subject = "Word";

                                if ($scope.current.id) {
                                    TemplateService.update($scope.current)
                                        .then(function (response) {
                                            $scope.grid.dataSource.read();
                                            mdToast.success($filter('translate')('Template.SuccessMessage'));
                                            $scope.closeSide('sideModal');
                                        });
                                } else {
                                    TemplateService.create($scope.current).then(function (response) {
                                        $scope.grid.dataSource.read();
                                        mdToast.success($filter('translate')('Template.SuccessMessage'));
                                        $scope.closeSide('sideModal');
                                    });
                                }
                            }
                            else {
                                mdToast.error($filter('translate')('Common.Error'));
                            }
                        };
                    } else {
                        TemplateService.create($scope.current)
                            .then(function (response) {
                                mdToast.success($filter('translate')('Template.SuccessMessage'));

                                $scope.grid.dataSource.read();
                                $scope.closeSide('sideModal');

                            });
                    }
                    //}
                };

                $scope.current = {};
                $scope.iframeElement = {};
                $scope.changeModule = function () {
                    //  $scope.moduleFields = TemplateService.getFields($scope.current.module);
                };

                $scope.moduleOptions = {
                    dataSource: $filter('filter')($scope.modules, function (item) {
                        return item.name !== 'roles' && item.name !== 'users' && item.name !== 'profiles';
                    }),
                    dataTextField: 'languages.' + $rootScope.globalization.Label + '.label.plural',
                    dataValueField: "name"
                };

                var profiles = [];

                for (var i = 0; i < $scope.profiles.length; i++) {

                    profiles.push({
                        profile_id: $scope.profiles[i].id,
                        name: $rootScope.getLanguageValue($scope.profiles[i].languages, 'name')
                    });
                }

                $scope.profileOptions = {
                    dataSource: profiles,
                    dataTextField: "name",
                    dataValueField: "profile_id",
                };

                $scope.clickRow = function (item) {
                    var selection = window.getSelection();
                    if (selection.toString().length === 0) {
                        $scope.current = angular.copy(item);

                        if ($scope.current.sharing_type === 'profile')
                            $scope.current.profile = $scope.getProfilisByIds($scope.current.profile_list);

                        if ($scope.current.sharing_type === 'custom')
                            $scope.current.sharesData = $scope.getUsersByIds($scope.current.shares);

                        $scope.openFormModal();
                    }
                };

                var uploadSuccessCallback,
                    uploadFailedCallback;
                var dialog_uid = plupload.guid();
                /// uploader configuration for image files.
                $scope.imgUpload = {
                    settings: {
                        multi_selection: false,
                        url: config.apiUrl + 'document/upload_attachment',
                        headers: {
                            'Authorization': 'Bearer ' + $localStorage.read('access_token'),
                            'Accept': 'application/json',
                            'X-App-Id': applicationId,
                            "X-Tenant-Id": tenantId
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
                            uploadSuccessCallback(blobUrl + "/" + resp.public_url, { alt: file.name });
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
                        url: config.apiUrl + 'document/upload_attachment',
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

                $scope.tinymceOptions = function (scope) {
                    $scope[scope] = {
                        setup: function (editor) {
                            editor.addButton('addParameter', {
                                type: 'button',
                                text: $filter('translate')('EMail.AddParameter'),
                                onclick: function () {
                                    $scope.showAddParameter = true;
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

                $scope.filters = {
                    email: [
                        { field: "Subject", operator: "ne", value: "SMS" },
                        {
                            logic: "or",
                            filters: [
                                /** Sms and Email are same value
                                 * When i try to write value:"Email", we get this error
                                 * The string 'Email' is not a valid enumeration type constant
                                 * We have to set value="Sms"*/
                                { field: "TemplateType", operator: "eq", value: "Sms" },
                                { field: "TemplateType", operator: "eq", value: "System" },
                            ]
                        }
                    ],
                    document: [
                        { field: "TemplateType", operator: "eq", value: "Module" }
                    ],
                    excel: [
                        { field: "TemplateType", operator: "eq", value: "Excel" }
                    ],
                    sms: [
                        { field: "TemplateType", operator: "eq", value: "Sms" },
                        { field: "Subject", operator: "eq", value: "SMS" }
                    ]
                };


                var txt = "dataItem.active ? ('Setup.Modules.Active' | translate) : ('Setup.Modules.Passive' | translate)";

                function rowTemplate(e) {
                    return '<td class="hide-on-m2"><span>' + e.name + '</span></td>'
                        + '<td class="hide-on-m2"><span>' + getLabel(e.module) + '</span></td>'
                        + '<td class="hide-on-m2"><span>{{' + txt + ' }}</span></td>'
                        + '<td class="show-on-m2">'
                        + '<div>' + $filter('translate')('Setup.Templates.TemplateName') + ': <strong>' + e.name + '</strong></div>'
                        + '<div>' + $filter('translate')('Common.Module') + ': <strong>' + getLabel(e.module) + '</strong></div>'
                        + '<div>' + $filter('translate')('Setup.Templates.Status') + ': <strong>{{' + txt + ' }}</strong></div></td>'
                        + '<td ng-click="$event.stopPropagation();"><span><md-button class="md-icon-button" aria-label=" " ng-click="delete($event,dataItem)"><i class="fas fa-trash"></i> </md-button></span></td>';

                }

                $scope.delete = function (ev, item) {

                    var confirm = $mdDialog.confirm()
                        .title($filter('translate')('Common.AreYouSure'))
                        .targetEvent(ev)
                        .ok($filter('translate')('Common.Yes'))
                        .cancel($filter('translate')('Common.No'));

                    $mdDialog.show(confirm).then(function () {
                        TemplateService.delete(item.id)
                            .then(function () {
                                $scope.grid.dataSource.read();
                                mdToast.success($filter('translate')('Template.SuccessDelete'));
                            });
                    }, function () {
                        $scope.status = 'You decided to keep your debt.';
                    });
                };

                var createGrid = function () {

                    var dataSource = new kendo.data.DataSource({
                        type: "odata-v4",
                        page: 1,
                        pageSize: 10,
                        serverPaging: true,
                        serverFiltering: true,
                        serverSorting: true,
                        transport: {
                            read: {
                                url: "/api/template/get_all_template_list",
                                type: 'GET',
                                dataType: "json",
                                beforeSend: $rootScope.beforeSend(),
                            }
                        },
                        schema: {
                            data: "items",
                            total: "count",
                            model: {
                                id: "id",
                                fields: {
                                    name: { type: "string" },
                                    module: { type: "string" },
                                    active: { type: "boolean" }
                                }
                            }
                        },
                        filter: $scope.filters[$scope.templateActiveTab]
                    });

                    $scope.gridOptions = {
                        dataSource: dataSource,
                        scrollable: false,
                        persistSelection: true,
                        sortable: true,
                        noRecords: true,
                        pageable: {
                            refresh: true,
                            pageSize: 10,
                            pageSizes: [10, 25, 50, 100],
                            buttonCount: 5,
                            info: true,
                        },
                        filterable: true,
                        filter: function (e) {
                            if (e.filter && e.field !== 'active') {
                                for (var i = 0; i < e.filter.filters.length; i++) {
                                    e.filter.filters[i].ignoreCase = true;
                                    if (e.field === 'module') {
                                        //module adı "test_name" şeklinde kayıt edildiği için boşluklu isimlerde sorun oluşuyordu.
                                        var newValue = helper.getSlug(e.filter.filters[i].value, "_");
                                        $scope.searchOldValues[newValue] = e.filter.filters[i].value
                                        e.filter.filters[i].value = newValue;
                                    }
                                }
                            }
                        },
                        filterMenuOpen: function (e) {
                            if (e.field === 'module' && e.container[0]) {
                                e.container[0][1].value = $scope.searchOldValues[e.container[0][1].value] ? $scope.searchOldValues[e.container[0][1].value] : e.container[0][1].value;
                                e.container[0][4].value = $scope.searchOldValues[e.container[0][4].value] ? $scope.searchOldValues[e.container[0][4].value] : e.container[0][4].value;
                            }
                        },
                        rowTemplate: function (e) {
                            return '<tr ng-click="clickRow(dataItem)">' + rowTemplate(e) + '</tr>';
                        },
                        altRowTemplate: function (e) {
                            return '<tr ng-click="clickRow(dataItem)" class="k-alt">' + rowTemplate(e) + '</tr>';
                        },
                        columns: [
                            {
                                field: "name",
                                title: $filter('translate')('Setup.Templates.TemplateName'),
                                media: "(min-width: 575px)"
                            },
                            {
                                field: "module",
                                title: $filter('translate')('Common.Module'),
                                media: "(min-width: 575px)"
                            },
                            {
                                field: "active",
                                title: $filter('translate')('Setup.Templates.Status'),
                                media: "(min-width: 575px)",
                                values: [
                                    { value: "true", text: $filter('translate')('Setup.Modules.Active') },
                                    { value: "false", text: $filter('translate')('Setup.Modules.Passive') }
                                ]
                            },
                            {
                                title: "Items",
                                media: "(max-width: 575px)"
                            },
                            {
                                field: "",
                                title: "",
                                filterable: false,
                                width: "40px",
                            }
                        ]
                    };
                    //After from service success
                    dataSource.fetch(function () {
                        $scope.loading = false;
                        if (!$rootScope.isMobile())
                            $(".k-pager-wrap").removeClass("k-pager-sm");
                    });
                };

                angular.element(document).ready(function () {
                    createGrid();
                    // $scope.loading = false;
                });
            });

            $scope.sharesOptions = {
                dataSource: $scope.users,
                filter: "contains",
                dataTextField: "full_name",
                dataValueField: "id",
                optionLabel: $filter('translate')('Common.Select')
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

            $scope.showTemplateGuideModal = function () {
                $rootScope.sideLoad = false;
                $scope.selectedSubModule = null;
                $scope.selectedModule = null;
                $scope.tempalteFieldName = "/" + $filter('translate')('Setup.Templates.TemplateFieldName');
                $rootScope.buildToggler('sideModal', 'view/setup/templates/wordTemplateGuide.html');
            };

            $scope.moduleChanged = function (templateModule) {
                // $scope.guideLoading = true;
                $scope.selectedModule = $rootScope.modulus[templateModule];
                $scope.lookupModules = getLookupModules($scope.selectedModule);
                $scope.getModuleRelations($scope.selectedModule);

                $scope.selectedSubModule = null;

            };
            $scope.subModuleOptions = {
                dataTextField: 'languages.' + $rootScope.globalization.Label + '.label.plural',
                dataValueField: "name",
            };
            $scope.subModuleChanged = function (moduleName) {
                $scope.selectedSubModule = $filter('filter')($scope.selectedModule.relatedModules, { name: moduleName }, true)[0];
            };

            //for GuideTemplate
            var getLookupModules = function (module) {

                if (!module) return;

                var lookupModules = [];
                for (var i = 0; i < module.fields.length; i++) {
                    if (module.fields[i].data_type === 'lookup') {
                        for (var j = 0; j < $rootScope.modules.length; j++) {
                            if (module.fields[i].lookup_type === $rootScope.modules[j].name) {
                                var lookupModule = angular.copy($rootScope.modules[j]);
                                lookupModule.parent_field = module.fields[i];
                                lookupModules.push(lookupModule);
                                break;
                            }
                        }
                    }
                }

                if (lookupModules.length)
                    module.lookupModules = lookupModules;
            };

            $scope.getModuleRelations = function (module) {
                if (!module)
                    return;

                module.relatedModules = [];

                angular.forEach(module.relations, function (relation) {
                    var relatedModule = $rootScope.modulus[relation.related_module];
                    if (relation.deleted || !relatedModule || relatedModule.order === 0)
                        return;

                    relatedModule = angular.copy(relatedModule);

                    if (relation.relation_type === 'many_to_many') {
                        angular.forEach(relatedModule.fields, function (field) {
                            field.name = relation.related_module + '_id.' + field.name;
                        });
                    } else {
                        getLookupModules(relatedModule);
                    }

                    module.relatedModules.push(relatedModule);

                });

                $scope.guideLoading = false;
                addNoteModuleRelation(module);
            };

            var addNoteModuleRelation = function (module) {
                var noteModule = {};
                noteModule.type = 'custom';
                noteModule.name = 'notes';
                noteModule.languages = {};
                noteModule.languages[$rootScope.globalization.Label] = {
                    label: {
                        singular: 'Note',
                        plural: 'Notes'
                    }
                };
                noteModule.order = 9999;
                noteModule.fields = [];

                var note = { id: 1, name: 'text', label_tr: 'Not', label_en: 'Note', languages: {} };
                note.languages[$rootScope.globalization.Label] = {
                    label: 'Note',
                };
                noteModule.fields.push(note);

                var firstName = {
                    id: 2,
                    name: 'first_name',
                    label_tr: 'Oluşturan - Adı',
                    label_en: 'First Name',
                    languages: {}
                };
                firstName.languages[$rootScope.globalization.Label] = $scope.defaultSystemFields['first_name'];

                noteModule.fields.push(firstName);

                var lastName = {
                    id: 3,
                    name: 'last_name',
                    label_tr: 'Oluşturan - Soyadı',
                    label_en: 'Last Name',
                    languages: {}
                };
                lastName.languages[$rootScope.globalization.Label] = $scope.defaultSystemFields['last_name'];

                noteModule.fields.push(lastName);

                var fullName = {
                    id: 4,
                    name: 'full_name',
                    label_tr: 'Oluşturan - Adı Soyadı',
                    label_en: 'Full Name',
                    languages: {}
                };

                fullName.languages[$rootScope.globalization.Label] = $scope.defaultSystemFields['full_name'];

                noteModule.fields.push(fullName);

                var email = {
                    id: 5,
                    name: 'email',
                    label_tr: 'Oluşturan - Eposta',
                    label_en: 'Email',
                    languages: {}
                };

                email.languages[$rootScope.globalization.Label] = $scope.defaultSystemFields['email'];

                noteModule.fields.push(email);

                var createdAt = {
                    id: 6,
                    name: 'created_at',
                    label_tr: 'Oluşturulma Tarihi',
                    label_en: 'Created at',
                    languages: {}
                };
                createdAt.languages[$rootScope.globalization.Label] = $scope.defaultSystemFields['created_at'];

                noteModule.fields.push(createdAt);

                module.relatedModules.push(noteModule);
            };

            $scope.copyToClipboard = function (str) {
                var el = document.createElement('textarea');
                el.value = str;
                el.setAttribute('readonly', '');
                el.style.position = 'absolute';
                el.style.left = '-9999px';
                document.body.appendChild(el);
                el.select();
                document.execCommand('copy');
                document.body.removeChild(el);
                mdToast.success('Copied: ' + str);
            };

            $scope.getRelatedFieldName = function (field, module) {
                return module.parent_field.name + '.' + (field.multiline_type_use_html ? 'html__' : '') + field.name;
            };

            $scope.getDownloadUrlExcel = function () {
                $window.open("/attach/export_excel?module=" + $scope.selectedModule.name + '&locale=' + $rootScope.locale, "_blank");
                mdToast.success($filter('translate')('Module.ExcelDesktop'));
            };

            $scope.languageOptions = {
                dataSource: $rootScope.globalizations,
                dataTextField: "Language",
                dataValueField: "Label",
                filter: $rootScope.globalizations.length > 10 ? 'startswith' : null,
                optionLabel: $filter('translate')('Common.Select')
            };

            function getLabel(module) {
                if (module && $scope.modulus[module]) {
                    return $rootScope.getLanguageValue($scope.modulus[module].languages, 'label', 'plural');
                }
                return '';
            }
        }
    ]);