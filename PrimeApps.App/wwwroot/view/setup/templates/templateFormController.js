'use strict';

angular.module('primeapps')

    .controller('TemplateFormController', ['$rootScope', '$scope', '$filter', '$state', 'ngToast', '$location', 'helper', 'config', '$localStorage', 'TemplateService', '$cookies',
        function ($rootScope, $scope, $filter, $state, ngToast, $location, helper, config, $localStorage, TemplateService, $cookies) {
            $scope.id = $location.search().id;
            $scope.documentexcel = true;
            $scope.documentword = true;

            var template = {};
            $scope.addNewPermissions = function (template) {
                template.permissions = [];

                angular.forEach($rootScope.profiles, function (profile) {
                    if (profile.is_persistent && profile.has_admin_rights)
                        profile.name = $filter('translate')('Setup.Profiles.Administrator');

                    if (profile.is_persistent && !profile.has_admin_rights)
                        profile.name = $filter('translate')('Setup.Profiles.Standard');

                    template.permissions.push({ profile_id: profile.id, profile_name: profile.name, type: 'full', profile_is_admin: profile.has_admin_rights });

                    $scope.temp = template.permissions;
                });
            };


            template.isNew = true;

            if (template.isNew) {
                $scope.addNewPermissions(template);
            }
            else {
                if (template.permissions && template.permissions.length > 0) {
                    angular.forEach(template.permissions, function (permission) {
                        var profile = $filter('filter')($rootScope.profiles, { id: permission.profile_id }, true)[0];
                        permission.profile_name = profile.Name;
                        permission.profile_is_admin = profile.has_admin_rights
                    });
                }
                else {
                    $scope.addNewPermissions(template);
                }
            }

            $scope.template = template;

            if ($scope.id) {
                TemplateService.get($scope.id)
                    .then(function (template) {
                        $scope.template = template.data;
                        if ($scope.template.template_type === 'excel') {
                            $scope.documentword = false;
                            $scope.document = "excel";
                        }
                        else {
                            $scope.documentexcel = false;
                            $scope.document = "word";

                        }

                        $scope.template.module = $filter('filter')($rootScope.modules, { name: template.data.module }, true)[0];

                        if ($scope.template.permissions == 0) {
                            $scope.template.permissions = $scope.temp;
                            $scope.template.subject = "Word";
                        }

                        angular.forEach(template.data.permissions, function (permission) {
                            var profile = $filter('filter')($rootScope.profiles, { id: permission.profile_id }, true)[0];
                            permission.profile_name = profile.name;
                            permission.profile_is_admin = profile.has_admin_rights
                        });

                        $scope.getDownloadUrl();
                    });
            }


            var success = function () {
                $scope.saving = false;
                $state.go('app.setup.templates');
                ngToast.create({ content: $filter('translate')('Setup.Templates.SaveSuccess'), className: 'success' });
            };

            $scope.fileUpload = {
                settings: {
                    runtimes: 'html5',
                    url: config.apiUrl + 'document/Upload',
                    chunk_size: '5mb',
                    multipart: true,
                    unique_names: true,
                    headers: {
                        'Authorization': 'Bearer ' + $localStorage.read('access_token'),
                        'Accept': 'application/json',
                        'X-Tenant-Id': $cookies.get('tenant_id')
                    },
                    filters: {
                        mime_types: [
                            { title: 'Template Files', extensions: 'doc,docx' }
                        ],
                        max_file_size: '10mb'
                    }
                },
                events: {
                    fileUploaded: function (uploader, file, response) {
                        var resp = JSON.parse(response.response);
                        var template = {
                            name: $scope.template.name,
                            module: $scope.template.module.name,
                            template_type: 'module',
                            content: resp.unique_name,
                            content_type: resp.content_type,
                            chunks: resp.chunks,
                            subject: "Word",
                            active: $scope.template.active,
                            permissions: $scope.template.permissions
                        };


                        if (!$scope.id) {
                            TemplateService.create(template)
                                .then(function () {
                                    success();
                                })
                                .catch(function () {
                                    $scope.saving = false;
                                });
                        }
                        else {
                            template.id = $scope.template.id;

                            TemplateService.update(template)
                                .then(function () {
                                    success();
                                })
                                .catch(function () {
                                    $scope.saving = false;
                                });
                        }
                    }
                }
            };

            $scope.fileUploadExcel = {
                settings: {
                    runtimes: 'html5',
                    url: config.apiUrl + 'Document/Upload_Excel',
                    chunk_size: '5mb',
                    multipart: true,
                    unique_names: true,
                    headers: {
                        'Authorization': 'Bearer ' + $localStorage.read('access_token'),
                        'Accept': 'application/json',
                        'X-Tenant-Id': $cookies.get('tenant_id')
                    },
                    filters: {
                        mime_types: [
                            { title: 'Template Files', extensions: 'xls,xlsx' }
                        ],
                        max_file_size: '10mb'
                    }
                },
                events: {
                    fileUploaded: function (uploader, file, response) {
                        var resp = JSON.parse(response.response);
                        var template = {
                            name: $scope.template.name,
                            module: $scope.template.module.name,
                            template_type: 'excel',
                            content: resp.unique_name,
                            content_type: resp.content_type,
                            chunks: resp.chunks,
                            subject: "Excel",
                            active: $scope.template.active,
                            permissions: $scope.template.permissions
                        };


                        if (!$scope.id) {
                            TemplateService.create(template)
                                .then(function () {
                                    success();
                                })
                                .catch(function () {
                                    $scope.saving = false;
                                });
                        }
                        else {
                            template.id = $scope.template.id;

                            TemplateService.update(template)
                                .then(function () {
                                    success();
                                })
                                .catch(function () {
                                    $scope.saving = false;
                                });
                        }
                    }
                }
            };


            $scope.save = function () {
                if (!$scope.uploadForm.$valid)
                    return;

                $scope.saving = true;

                if (!$scope.id) {
                    $scope.fileUpload.uploader.start();
                }
                else {
                    if ($scope.templateFileCleared) {
                        $scope.fileUpload.uploader.start();
                    }
                    else {
                        $scope.template.module = $scope.template.module.name;

                        TemplateService.update($scope.template)
                            .then(function () {
                                success();
                            })
                            .catch(function () {
                                $scope.saving = false;
                            });
                    }
                }
            };

            $scope.saveExcel = function () {
                if (!$scope.uploadForm.$valid)
                    return;

                $scope.saving = true;

                if (!$scope.id) {
                    $scope.fileUploadExcel.uploader.start();
                }
                else {
                    if ($scope.templateFileCleared) {
                        $scope.fileUploadExcel.uploader.start();
                    }
                    else {
                        $scope.template.module = $scope.template.module.name;

                        TemplateService.update($scope.template)
                            .then(function () {
                                success();
                            })
                            .catch(function () {
                                $scope.saving = false;
                            });
                    }
                }
            };

            $scope.getDownloadUrl = function () {
                if (!$scope.template || !$scope.template.id)
                    return;

                $scope.templateDownloadUrl = 'attach/download_template?template_id=' + $scope.template.id + '&access_token=' + $localStorage.read('access_token');
            };

            $scope.clearTemplateFile = function () {
                if ($scope.fileUpload.uploader.files[0])
                    $scope.fileUpload.uploader.removeFile($scope.fileUpload.uploader.files[0]);

                if ($scope.template && $scope.template.content)
                    $scope.template.content = undefined;

                $scope.templateFileCleared = true;
            };

        }
    ]);