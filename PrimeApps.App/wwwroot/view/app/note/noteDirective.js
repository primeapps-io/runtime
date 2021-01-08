'use strict';

angular.module('primeapps')

    .directive('noteList', ['convert', '$localStorage', 'NoteService', 'FileUploader', 'config', '$filter', 'helper', '$cookies', '$mdDialog', 'mdToast',
        function (convert, $localStorage, NoteService, FileUploader, config, $filter, helper, $cookies, $mdDialog, mdToast) {
            return {
                restrict: 'EA',
                scope: {
                    moduleId: '=',
                    recordId: '='
                },
                templateUrl: 'view/app/note/noteList.html',
                controller: ['$rootScope', '$scope', function ($rootScope, $scope) {
                    $scope.user = $rootScope.user;
                    $scope.pagingIcon = 'fa-chevron-right';
                    $scope.config = $rootScope.config;
                    $scope.$parent.loadingNotes = true;
                    $scope.$parent.allNotesLoaded = false;
                    $scope.$parent.currentPage = 1;
                    $scope.$parent.limit = 10;
                    $scope.addActivity = $scope.$parent.addActivity;
                    $scope.blobUrl = blobUrl;
                    var uploadSuccessCallback,
                        uploadFailedCallback;
                    var dialog_uid = plupload.guid();

                    $scope.modules = $rootScope.modules;
                    $scope.globalization = $rootScope.globalization;
                    $scope.newNoteForm = false;
                    $scope.newsfeed = true;

                    if ($scope.$parent.$parent.module)
                        $scope.newsfeed = false;

                    var request = {
                        module_id: $scope.$parent.module.id,
                        record_id: $scope.recordId,
                        limit: $scope.$parent.limit,
                        offset: 0
                    };

                    NoteService.count(request).then(function (totalCount) {
                        NoteService.find(request)
                            .then(function (notes) {
                                $rootScope.processLanguages(notes.data);
                                $scope.$parent.notes = notes.data;
                                $scope.$parent.loadingNotes = false;
                                $scope.$parent.$parent.notesCount = totalCount.data;

                                if (totalCount.data <= $scope.$parent.limit)
                                    $scope.$parent.hidePaging = true;
                            });
                    });

                    $scope.loadMore = function () {
                        if ($scope.$parent.allNotesLoaded)
                            return;

                        request.offset = $scope.$parent.currentPage * $scope.$parent.limit;
                        $scope.pagingIcon = 'fa-spinner fa-spin';

                        NoteService.find(request)
                            .then(function (notes) {
                                $rootScope.processLanguages(notes.data);
                                notes = notes.data;
                                $scope.$parent.notes = $scope.$parent.notes.concat(notes);
                                $scope.pagingIcon = 'fa-chevron-right';
                                $scope.$parent.currentPage = $scope.$parent.currentPage + 1;

                                if (notes.length === 0) {
                                    if ($scope.user.profile.has_admin_rights)
                                        $scope.$parent.allNotesLoaded = true;
                                    else {
                                        if ($scope.$parent.allNotesLoaded)
                                            return;

                                        request.offset = $scope.$parent.currentPage * $scope.$parent.limit;
                                        $scope.pagingIcon = 'fa-spinner fa-spin';

                                        NoteService.find(request)
                                            .then(function (notes) {
                                                $rootScope.processLanguages(notes.data);
                                                notes = notes.data;
                                                $scope.$parent.notes = $scope.$parent.notes.concat(notes);
                                                $scope.pagingIcon = 'fa-chevron-right';
                                                $scope.$parent.currentPage = $scope.$parent.currentPage + 1;
                                                $scope.$parent.allNotesLoaded = true;
                                            });
                                    }
                                }
                            });
                    };

                    $scope.addNote = function (noteModel) {

                        if (!noteModel || !noteModel.languages[$rootScope.globalization.Label]['text'].trim())
                            return;

                        $scope.noteCreating = true;
                        var text = noteModel.languages[$rootScope.globalization.Label]['text'];

                        var note = {
                            languages: noteModel.languages
                        };

                        //note.text = noteModel.text.trim();
                        $rootScope.languageStringify(note);

                        if (text.length > 3800) {
                            mdToast.warning($filter('translate')('Note.LimitWarn'));
                            $scope.noteCreating = false;
                        } else {
                            NoteService.create(note)
                                .then(function (newNote) {
                                    newNote = newNote.data;
                                    $rootScope.processLanguage(newNote);
                                    //semantik tarih
                                    var today = new Date();
                                    var currentDay = today.getDay();
                                    var dd = today.getDate();
                                    var mm = today.getMonth() + 1;
                                    var yyyy = today.getFullYear();
                                    var calendarDate;
                                    var regex = /(<([^>]+)>)/ig;

                                    var content = newNote.text.replace(regex, "").split(" ");
                                    for (var i = 0; i < content.length; i++) {
                                        if (moment(content[i], "DD/MM/YYYY", true).isValid() || moment(content[i], "DD.MM.YYYY", true).isValid()) {
                                            var dateTime = moment(moment(content[i], "DD/MM/YYYY HH:mm").unix() * 1000);
                                            newNote.text = newNote.text.replace(content[i], '<a href="" ng-click="addActivity(' + dateTime + ', \'date\')" data-placement="right" bs-tooltip data-title="Etkinlik eklemek için tıklayınız"/>' + content[i] + '</a>');
                                        }
                                        var lowerCaseContent = content[i].toLowerCase();
                                        if ($filter('filter')(arr, {
                                            code: lowerCaseContent
                                        }, true).length > 0) {
                                            var difference;
                                            var arrEl = $filter('filter')(arr, {
                                                code: lowerCaseContent
                                            }, true)[0];

                                            if (arrEl.type === 'day') {
                                                var dayDiff;
                                                if (content[i - 1] === 'haftaya' || content[i - 1] === 'hafta' || content[i - 1] === 'Hafta' || content[i - 1] === 'Haftaya') {
                                                    if (arrEl.value > currentDay)
                                                        difference = arrEl.value - currentDay + 7;
                                                    else {
                                                        dayDiff = currentDay - arrEl.value;
                                                        difference = 7 - dayDiff;
                                                    }

                                                    var strWeek = content[i - 1] + ' ' + content[i];
                                                    newNote.text = newNote.text.replace(strWeek, '<a href="" ng-click="addActivity(' + difference + ', \'null\')" data-placement="right" bs-tooltip data-title="Etkinlik eklemek için tıklayınız"/>' + content[i - 1] + ' ' + content[i] + '</a>');
                                                } else {
                                                    if (arrEl.value > currentDay)
                                                        difference = arrEl.value - currentDay;
                                                    else {
                                                        dayDiff = currentDay - arrEl.value;
                                                        difference = 7 - dayDiff;
                                                    }

                                                    newNote.text = newNote.text.replace(content[i], '<a href="" ng-click="addActivity(' + difference + ', \'null\')" data-placement="right" bs-tooltip data-title="Etkinlik eklemek için tıklayınız"/>' + content[i] + '</a>');
                                                }
                                            } else if (arrEl.type === 'dayTime') {
                                                difference = arrEl.value;
                                                newNote.text = newNote.text.replace(content[i], '<a href="" ng-click="addActivity(' + difference + ', \'null\')" data-placement="right" bs-tooltip data-title="Etkinlik eklemek için tıklayınız"/>' + content[i] + '</a>');
                                            } else if (arrEl.type === 'month') {
                                                content[i - 1] = parseInt(content[i - 1]);
                                                if (content[i - 1] === parseInt(content[i - 1], 10)) {
                                                    var fullDate = content[i - 1] + '.' + arrEl.value + '.' + yyyy;
                                                    difference = moment(moment(fullDate, "DD/MM/YYYY HH:mm").unix() * 1000);
                                                    var strMonth = content[i - 1] + ' ' + content[i];
                                                    newNote.text = newNote.text.replace(strMonth, '<a href="" ng-click="addActivity(' + difference + ', \'month\')" data-placement="top" bs-tooltip data-title="Etkinlik eklemek için tıklayınız"/>' + content[i - 1] + ' ' + content[i] + '</a>');
                                                }
                                            }
                                        }
                                    }

                                    //after problem fix
                                    var now = new Date();
                                    newNote.created_at = now.setSeconds(now.getSeconds() - 1);

                                    $scope.$parent.notes.unshift(newNote);
                                    $scope.$parent.$parent.notesCount = $scope.$parent.notesCount + 1;
                                    $scope.noteCreating = false;
                                    $scope.newNoteForm = false;
                                    $scope.note = null;
                                })
                                .catch(function () {
                                    $scope.noteCreating = false;
                                });
                        }
                    };

                    $scope.addComment = function (noteModel) {

                        var subText = noteModel.subNote.languages[$rootScope.globalization.Label]['text'];

                        if (!subText || !subText.trim())
                            return;

                        noteModel.noteCreating = true;

                        var subNote = {
                            languages: noteModel.subNote.languages
                        };
                        //subNote.text = noteModel.subNote.text.trim();
                        subNote.record_id = $scope.recordId;
                        subNote.note_id = noteModel.id;
                        subNote.module_id = $scope.$parent.module.id; //if chatter, please beware of this.

                        $rootScope.languageStringify(subNote);

                        if (subText.length > 3800) {
                            mdToast.warning($filter('translate')('Note.LimitWarn'));
                            noteModel.noteCreating = false;
                        } else {
                            NoteService.create(subNote)
                                .then(function (newSubNote) {
                                    if (!noteModel.notes)
                                        noteModel.notes = [];
                                    //$scope.$parent.$parent.notesCount++;
                                    noteModel.notes.unshift(newSubNote.data);
                                    $rootScope.processLanguage(noteModel);
                                    noteModel.noteCreating = false;
                                    noteModel.showForm = false;
                                    $scope.$parent.allNotesLoaded = false;
                                });
                        }
                    };

                    $scope.updateNote = function (noteModel) {

                        var text = noteModel.languages[$rootScope.globalization.Label]['text'];

                        if (!text || !text.trim())
                            return;

                        noteModel.noteUpdating = true;

                        var note = {
                            languages: noteModel.languages
                        };

                        note.id = noteModel.id;
                        //note.text = noteModel.text.trim();
                        $rootScope.languageStringify(note);

                        NoteService.update(note)
                            .then(function (response) {
                                noteModel.noteUpdating = false;
                                noteModel.showFormEdit = false;
                            });
                    };

                    $scope.deleteNote = function (ev, note, parentNote) {

                        var confirm = $mdDialog.confirm()
                            .title($filter('translate')('Common.AreYouSure'))
                            .targetEvent(ev)
                            .ok($filter('translate')('Common.Yes'))
                            .cancel($filter('translate')('Common.No'));

                        $mdDialog.show(confirm).then(function () {
                            NoteService.delete(note.id)
                                .then(function () {
                                    if (parentNote) {
                                        parentNote.notes.splice(parentNote.notes.indexOf(note), 1)
                                    } else {
                                        $scope.$parent.notes.splice($scope.$parent.notes.indexOf(note), 1)
                                    }

                                    $scope.$parent.$parent.notesCount = $scope.$parent.notesCount - 1;
                                });
                        }, function () {
                            //cancel
                        });
                    };

                    $scope.like = function (note, type) {
                        var request = {
                            note_id: note.id,
                            user_id: $scope.user.id
                        };

                        NoteService.like(request)
                            .then(function () {
                                $scope.likeButton = true;
                                var id = note.id;
                                NoteService.get(id)
                                    .then(function (noteResponse) {
                                        var currentNote = noteResponse.data;
                                        $rootScope.processLanguage(currentNote);
                                        $scope.likeButton = false;
                                        if (type === 'sub') {
                                            var note = $filter('filter')($scope.$parent.notes, {
                                                id: currentNote.note_id
                                            }, true)[0];
                                            for (var i = 0; i < note.notes.length; i++) {
                                                var subNote = note.notes[i];
                                                if (subNote.id === id)
                                                    subNote.likes = currentNote.likes;
                                            }
                                        } else
                                            $filter('filter')($scope.$parent.notes, {
                                                id: currentNote.id
                                            }, true)[0].likes = currentNote.likes;
                                    });
                            });
                    };

                    // $scope.refresh=function () {
                    //
                    //     var request = {
                    //         module_id: $scope.$parent.module.id,
                    //         record_id: $scope.recordId,
                    //         limit: $scope.limit,
                    //         offset: 0
                    //     };
                    //
                    //     NoteService.count(request).then(function (totalCount) {
                    //         NoteService.find(request)
                    //             .then(function (notes) {
                    //                 $scope.$parent.notes = notes.data;
                    //                 $scope.$parent.loadingNotes = false;
                    //                 $scope.$parent.allNotesLoaded = false;
                    //                 $scope.currentPage = 1;
                    //                 $scope.limit = 10;
                    //                 ngToast.create({ content: $filter('translate')('Note.NoteRefresh'), className: 'success' });
                    //                 $scope.$parent.$parent.notesCount = totalCount.data;
                    //
                    //                 if (totalCount.data <= $scope.limit)
                    //                     $scope.hidePaging = true;
                    //             });
                    //     });
                    // }

                    $scope.noteLikesList = function (ev, likes) {
                        $scope.likes = likes;
                        var parentEl = angular.element(document.body);
                        $mdDialog.show({
                            parent: parentEl,
                            templateUrl: 'view/app/note/noteLikes.html',
                            clickOutsideToClose: false,
                            scope: $scope,
                            preserveScope: true

                        });
                    };

                    $scope.close = function () {
                        $mdDialog.hide();
                    };

                    $scope.imgUpload = {
                        settings: {
                            multi_selection: false,
                            url: 'storage/upload',
                            headers: {
                                'Authorization': 'Bearer ' + $localStorage.read('access_token'),
                                'Accept': 'application/json',
                                'X-Tenant-Id': $cookies.get(preview ? 'preview_tenant_id' : 'tenant_id'),
                                'X-App-Id': $cookies.get(preview ? 'preview_app_id' : 'app_id')
                            },
                            multipart_params: {
                                container: dialog_uid,
                                type: "note",
                                upload_id: 0,
                                response_list: ""
                            },
                            filters: {
                                mime_types: [{
                                    title: "Image files",
                                    extensions: "jpg,gif,png"
                                },],
                                max_file_size: "2mb"
                            },
                            resize: {
                                quality: 90
                            }
                        },
                        events: {
                            filesAdded: function (uploader, files) {
                                uploader.start();
                                tinymce.activeEditor.windowManager.open({
                                    title: $filter('translate')('Common.PleaseWait'),
                                    width: 50,
                                    height: 50,
                                    body: [{
                                        type: 'container',
                                        name: 'container',
                                        label: '',
                                        html: '<span>' + $filter('translate')('EMail.UploadingAttachment') + '</span>'
                                    },],
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
                                uploadSuccessCallback(config.storage_host + resp.public_url, {
                                    alt: file.name
                                });
                                uploadSuccessCallback = null;
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
                    $scope.fileUpload = {
                        settings: {
                            multi_selection: false,
                            unique_names: false,
                            url: 'storage/upload',
                            headers: {
                                'Authorization': 'Bearer ' + $localStorage.read('access_token'),
                                'Accept': 'application/json',
                                'X-Tenant-Id': $cookies.get(preview ? 'preview_tenant_id' : 'tenant_id'),
                                'X-App-Id': $cookies.get(preview ? 'preview_app_id' : 'app_id')
                            },
                            multipart_params: {
                                container: dialog_uid,
                                type: "note",
                                upload_id: 0,
                                response_list: ""
                            },
                            filters: {
                                mime_types: [{
                                    title: "Email Attachments",
                                    extensions: "pdf,doc,docx,xls,xlsx,csv"
                                },],
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
                                    body: [{
                                        type: 'container',
                                        name: 'container',
                                        label: '',
                                        html: '<span>' + $filter('translate')('EMail.UploadingAttachment') + '</span>'
                                    },],
                                    buttons: []
                                });
                            },
                            uploadProgress: function (uploader, file) {
                            },
                            fileUploaded: function (uploader, file, response) {
                                this.settings.multipart_params.response_list = "";
                                this.settings.multipart_params.upload_id = 0;

                                var resp = JSON.parse(response.response);
                                uploadSuccessCallback(config.storage_host + resp.public_url, {
                                    alt: file.name
                                });
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


                    $scope.tinymceTemplate = {
                        onChange: function (e) {
                            
                            // put logic here for keypress and cut/paste changes
                        },
                        valid_elements: "@[id|class|title|style]," +
                            "a[name|href|target|title|alt|ng-click]," +
                            "#p,blockquote,-ol,-ul,-li,br,img[src|height|width],-sub,-sup,-b,-i,-u," +
                            "-span,hr",
                        inline: false,
                        height: 125,
                        language: $rootScope.language,
                        plugins: [
                            "advlist autolink lists link image charmap print preview anchor placeholder",
                            "searchreplace visualblocks code fullscreen",
                            "insertdatetime table contextmenu paste imagetools wordcount textcolor colorpicker"
                        ],
                        toolbar: " link image imagetools ",
                        menubar: 'false',
                        placeholder_attrs: {
                            style: {
                                position: 'absolute',
                                top: '5px',
                                left: 0,
                                color: 'lightgrey',
                                padding: '1%',
                                width: '98%',
                                overflow: 'hidden',
                                'white-space': 'pre-wrap'
                            }
                        },
                        skin: 'custom',
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
                        resize: false,
                        width: '99,9%',
                        toolbar_items_size: 'small',
                        statusbar: false
                    };

                    var arr = [{
                        code: 'pazartesi',
                        type: 'day',
                        value: 1
                    },
                    {
                        code: 'salı',
                        type: 'day',
                        value: 2
                    },
                    {
                        code: '&ccedil;arşamba',
                        type: 'day',
                        value: 3
                    },
                    {
                        code: 'perşembe',
                        type: 'day',
                        value: 4
                    },
                    {
                        code: 'cuma',
                        type: 'day',
                        value: 5
                    },
                    {
                        code: 'cumartesi',
                        type: 'day',
                        value: 6
                    },
                    {
                        code: 'pazar',
                        type: 'day',
                        value: 7
                    },
                    {
                        code: 'bug&uuml;n',
                        type: 'dayTime',
                        value: 0
                    },
                    {
                        code: 'yarın',
                        type: 'dayTime',
                        value: 1
                    },
                    {
                        code: 'ocak',
                        type: 'month',
                        value: 1
                    },
                    {
                        code: 'şubat',
                        type: 'month',
                        value: 2
                    },
                    {
                        code: 'mart',
                        type: 'month',
                        value: 3
                    },
                    {
                        code: 'nisan',
                        type: 'month',
                        value: 4
                    },
                    {
                        code: 'mayıs',
                        type: 'month',
                        value: 5
                    },
                    {
                        code: 'haziran',
                        type: 'month',
                        value: 6
                    },
                    {
                        code: 'temmuz',
                        type: 'month',
                        value: 7
                    },
                    {
                        code: 'ağustos',
                        type: 'month',
                        value: 8
                    },
                    {
                        code: 'eyl&uuml;l',
                        type: 'month',
                        value: 9
                    },
                    {
                        code: 'ekim',
                        type: 'month',
                        value: 10
                    },
                    {
                        code: 'kasım',
                        type: 'month',
                        value: 11
                    },
                    {
                        code: 'aralık',
                        type: 'month',
                        value: 12
                    }
                    ];

                }]
            };
        }
    ])

    .directive('noteForm', ['$filter', '$localStorage', 'FileUploader', 'config', 'convert', 'entityTypes', 'NoteService', '$cookies',
        function ($filter, $localStorage, FileUploader, config, convert, entityTypes, NoteService, $cookies) {
            return {
                restrict: 'EA',
                scope: {
                    recordId: '=',
                    show: '='
                },
                templateUrl: 'view/app/note/noteForm.html',
                controller: ['$scope', '$rootScope',
                    function ($scope, $rootScope) {
                    
                        $scope.noteCreating = false;
                        $scope.globalization = $rootScope.globalization;
                        var uploadSuccessCallback,
                            uploadFailedCallback;
                        var dialog_uid = plupload.guid();

                        $scope.create = function (noteModel) {

                            if (!noteModel || !noteModel.languages[$rootScope.globalization.Label]['text'].trim())
                                return;

                            var text = noteModel.languages[$rootScope.globalization.Label]['text'];
                            $scope.noteCreating = true;

                            var note = {
                                languages: noteModel.languages
                            };
                            //note.text = noteModel.text.trim();
                            note.record_id = $scope.recordId;
                            note.module_id = $scope.$parent.module.id;

                            $rootScope.languageStringify(note);

                            if (text.length > 3800) {
                                mdToast.warning($filter('translate')('Note.LimitWarn'));
                                $scope.noteCreating = false;
                            } else {
                                NoteService.create(note)
                                    .then(function (newNote) {
                                        newNote = newNote.data;
                                        $rootScope.processLanguage(newNote);
                                        //after problem fix
                                        var now = new Date();
                                        newNote.created_at = now.setSeconds(now.getSeconds() - 1);

                                        $scope.$parent.notes.unshift(newNote);
                                        $scope.$parent.$parent.notesCount = $scope.$parent.notesCount + 1;
                                        $scope.noteCreating = false;
                                        $scope.show = false;
                                        $scope.note = null;
                                    })
                                    .catch(function () {
                                        $scope.noteCreating = false;
                                    });
                            }

                        };

                        $scope.imgUploadForm = {
                            settings: {
                                multi_selection: false,
                                url: 'storage/upload',
                                multipart_params: {
                                    container: dialog_uid,
                                    type: "note",
                                    upload_id: 0,
                                    response_list: ""
                                },
                                filters: {
                                    mime_types: [{
                                        title: "Image files",
                                        extensions: "jpg,gif,png"
                                    }],
                                    max_file_size: "2mb"
                                },
                                resize: {
                                    quality: 90
                                }
                            },
                            events: {
                                filesAdded: function (uploader, files) {
                                    uploader.start();
                                    tinymce.activeEditor.windowManager.open({
                                        title: $filter('translate')('Common.PleaseWait'),
                                        width: 50,
                                        height: 50,
                                        body: [{
                                            type: 'container',
                                            name: 'container',
                                            label: '',
                                            html: '<span>' + $filter('translate')('EMail.UploadingAttachment') + '</span>'
                                        },],
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
                                    uploadSuccessCallback(config.storage_host + resp.public_url, {
                                        alt: file.name
                                    });
                                    uploadSuccessCallback = null;
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

                        $scope.fileUploadForm = {
                            settings: {
                                multi_selection: false,
                                unique_names: false,
                                url: 'storage/upload',
                                headers: {
                                    'Authorization': 'Bearer ' + $localStorage.read('access_token'),
                                    'Accept': 'application/json',
                                    'X-Tenant-Id': $cookies.get(preview ? 'preview_tenant_id' : 'tenant_id'),
                                    'X-App-Id': $cookies.get(preview ? 'preview_app_id' : 'app_id')
                                },
                                multipart_params: {
                                    container: dialog_uid,
                                    type: "note",
                                    upload_id: 0,
                                    response_list: ""
                                },
                                filters: {
                                    mime_types: [{
                                        title: "Email Attachments",
                                        extensions: "pdf,doc,docx,xls,xlsx,csv"
                                    },],
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
                                        body: [{
                                            type: 'container',
                                            name: 'container',
                                            label: '',
                                            html: '<span>' + $filter('translate')('EMail.UploadingAttachment') + '</span>'
                                        },],
                                        buttons: []
                                    });
                                },
                                uploadProgress: function (uploader, file) { },
                                fileUploaded: function (uploader, file, response) {
                                    this.settings.multipart_params.response_list = "";
                                    this.settings.multipart_params.upload_id = 0;

                                    var resp = JSON.parse(response.response);
                                    uploadSuccessCallback(resp.public_url, {
                                        alt: file.name
                                    });
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

                        $scope.tinymceTemplate = {
                            onChange: function (e) {
                                // put logic here for keypress and cut/paste changes
                            },
                            inline: false,
                            height: 80,
                            language: $rootScope.language,
                            plugins: [
                                "advlist autolink lists link image charmap print preview anchor placeholder",
                                "searchreplace visualblocks code fullscreen",
                                "insertdatetime table contextmenu paste imagetools wordcount textcolor colorpicker"
                            ],
                            toolbar: " link image imagetools ",
                            menubar: 'false',
                            placeholder_attrs: {
                                style: {
                                    position: 'absolute',
                                    top: '5px',
                                    left: 0,
                                    color: 'lightgrey',
                                    padding: '1%',
                                    width: '98%',
                                    overflow: 'hidden',
                                    'white-space': 'pre-wrap'
                                }
                            },
                            skin: 'custom',
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
                                $scope.imgUploadForm.uploader.addFile(blob);
                                //TODO: in future will be implemented to upload pasted data images into server.
                            },
                            resize: false,
                            width: '99,9%',
                            toolbar_items_size: 'small',
                            statusbar: false
                        };
                    }
                ]
            };
        }
    ])

    .directive('compile', ['$compile', function ($compile) {
        return function (scope, element, attrs) {
            scope.$watch(
                function (scope) {
                    // watch the 'compile' expression for changes
                    return scope.$eval(attrs.compile);
                },
                function (value) {
                    // when the 'compile' expression changes
                    // assign it into the current DOM
                    element.html(value);

                    // compile the new DOM and link it to the current
                    // scope.
                    // NOTE: we only compile .childNodes so that
                    // we don't get into infinite loop compiling ourselves
                    $compile(element.contents())(scope);
                }
            );
        };
    }])

    .directive('onEnter', function () {
        return function (scope, element, attrs) {
            element.bind("keydown keypress", function (event) {
                if (event.which === 13) {
                    scope.$apply(function () {
                        scope.$eval(attrs.onEnter, {
                            'event': event
                        });
                    });

                    event.preventDefault();
                }
            });
        };
    });