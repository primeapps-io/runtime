'use strict';

angular.module('primeapps')

    .controller('ComponentDetailController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', '$timeout', 'helper', 'dragularService', 'ComponentsService', 'componentPlaces', 'componentPlaceEnums', 'componentTypeEnums', '$localStorage', 'ComponentsDeploymentService', '$sce',
        function ($rootScope, $scope, $filter, $state, $stateParams, $modal, $timeout, helper, dragularService, ComponentsService, componentPlaces, componentPlaceEnums, componentTypeEnums, $localStorage, ComponentsDeploymentService, $sce) {
            $scope.modules = [];
            $scope.id = $state.params.id;
            $scope.orgId = $state.params.orgId;

            $scope.$parent.menuTopTitle = $scope.currentApp.label;
            $scope.$parent.activeMenu = 'app';
            $scope.$parent.activeMenuItem = 'components';

            $scope.app = $rootScope.currentApp;
            $scope.modules = $rootScope.appModules;

            if (!$scope.id) {
                $state.go('studio.app.components');
            }

            /*if (!$scope.orgId || !$scope.appId) {
             $state.go('studio.apps', { organizationId: $scope.orgId });
             }*/

            $scope.loading = true;
            $scope.generator = function (limit) {
                $scope.placeholderArray = [];
                for (var i = 0; i < limit; i++) {
                    $scope.placeholderArray[i] = i;
                }
            };

            $scope.generator(10);

            $scope.$parent.$parent.tabManage = {
                activeTab: "overview"
            };

            $scope.requestModel = {
                limit: "10",
                offset: 0
            };
            $scope.activePage = 1;

            ComponentsDeploymentService.count($scope.id)
                .then(function (response) {
                    $scope.pageTotal = response.data;
                    //$scope.changePage(1);
                });

            $scope.changePage = function (page) {
                $scope.loadingDeployments = true;

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

                ComponentsDeploymentService.find($scope.component.id, requestModel)
                    .then(function (response) {
                        $scope.deployments = response.data;
                        $scope.loadingDeployments = false;
                        //$scope.loading = false;
                    })
                    .catch(function (response) {
                        toastr.error($filter('translate')('Common.Error'));
                        $scope.loadingDeployments = false;
                    });
            };

            $scope.changeOffset = function () {
                $scope.changePage($scope.activePage)
            };

            //var currentOrganization = $localStorage.get("currentApp");
            $scope.organization = $filter('filter')($rootScope.organizations, {id: $scope.orgId})[0];
            $scope.giteaUrl = giteaUrl;

            $scope.deployments = [];

            $scope.getFileList = function () {
                $scope.filesLoading = true;
                ComponentsService.getFileList($scope.id)
                    .then(function (response) {
                        $scope.files = [];
                        angular.forEach(response.data, function (file) {
                            var path = {'path': file.path, 'value': file.path.replace('components/' + $scope.component.name + '/', '')};
                            $scope.files.push(path);
                            $scope.filesLoading = false;
                        });
                    })
                    .catch(function (response) {
                        $scope.filesLoading = false;
                    });
            };

            //$scope.getFileList();

            ComponentsService.get($scope.id)
                .then(function (response) {
                    if (!response.data) {
                        toastr.error('Component Not Found !');
                        $state.go('studio.app.components');
                    }
                    $scope.content = {};
                    $scope.componentCopy = angular.copy(response.data);
                    $scope.component = response.data;
                    $scope.content.url = $filter('filter')($scope.modules, {id: $scope.component.module_id})[0]['name'];

                    if ($scope.component.content) {
                        $scope.component.content = JSON.parse($scope.component.content);

                        if ($scope.component.content.files) {
                            $scope.component.content.files = $scope.component.content.files.join("\n");
                        }

                        var urlParameters = $scope.component.content.url.split('?');
                        $scope.content.url_parameters = urlParameters.length > 1 ? urlParameters[1] : null;

                        if ($scope.component.content.app) {
                            if ($scope.component.content.app.templateFile && $scope.component.content.app.templateFile.contains('http')) {
                                $scope.content.templateUrl = true;
                            }
                        }
                    }

                    $scope.changePage(1);
                    $scope.loading = false;
                });

            $scope.isTemplateFile = function () {
                return function (item) {
                    return item.value.contains('.html');
                };
            };

            $scope.save = function (componentFormValidation) {
                if (!componentFormValidation.$valid){
                    toastr.error($filter('translate')('Module.RequiredError'));
                    return;
                }
                    

                $scope.saving = true;

                $scope.copyComponent = angular.copy($scope.component);

                if (!$scope.component.content) {
                    $scope.copyComponent.content = {};
                }

                if ($scope.component.content.files) {
                    $scope.copyComponent.content.files = $scope.component.content.files.split("\n");
                }

                if (!$scope.content.templateUrl && $scope.component.content.app && $scope.component.content.app.templateFile) {
                    $scope.copyComponent.content.app.templateUrl = $scope.component.content.app.templateFile;
                }

                $scope.copyComponent.content.url = $scope.content.url + (($scope.content.url_parameters) ? '?' + $scope.content.url_parameters : '');

                $scope.copyComponent.content = JSON.stringify($scope.copyComponent.content);

                ComponentsService.update($scope.id, $scope.copyComponent)
                    .then(function (response) {
                        $scope.saving = false;
                        $scope.editing = false;
                        toastr.success("Component updated successfully.");
                    })
                    .catch(function () {
                        $scope.saving = false;
                        $scope.editing = false;
                        toastr.error("Component not updated successfully.");
                    })
            };

            $scope.runDeployment = function () {
                $scope.loadingDeployments = true;
                ComponentsService.deploy($scope.id)
                    .then(function (response) {
                        toastr.success("Deployment Started");
                        $scope.pageTotal = $scope.pageTotal + 1;
                        $scope.activePage = 1;
                        $scope.changePage(1);
                    })
                    .catch(function (response) {
                        $scope.loadingDeployments = false;

                        if (response.status === 409) {
                            toastr.warning(response.data);
                        }
                        else {
                            toastr.error($filter('translate')('Common.Error'));
                        }
                    });
            };

            $scope.getTime = function (time) {
                return moment(time).format("DD-MM-YYYY HH:ss");
            };

            $scope.getIcon = function (status) {
                switch (status) {
                    case 'running':
                        return $sce.trustAsHtml('<i style="color:#0d6faa;" class="fas fa-clock"></i>');
                    case 'failed':
                        return $sce.trustAsHtml('<i style="color:rgba(218,10,0,1);" class="fas fa-times"></i>');
                    case 'succeed':
                        return $sce.trustAsHtml('<i style="color:rgba(16,124,16,1);" class="fas fa-check"></i>');
                }
            };
        }
    ]);