'use strict';

angular.module('primeapps')

    .controller('PackageController', ['$rootScope', '$scope', '$state', 'PackageService', '$timeout', '$sce', '$location', '$filter',
        function ($rootScope, $scope, $state, PackageService, $timeout, $sce, $location, $filter) {
            $scope.loading = true;
            $scope.activePage = 1;
            $rootScope.runningPackages[$rootScope.currentApp.name] = {status: true};

            $scope.$parent.activeMenu = 'app';
            $scope.$parent.activeMenuItem = 'packages';
            $rootScope.breadcrumblist[2].title = 'Packages';

            $scope.generator = function (limit) {
                $scope.placeholderArray = [];
                for (var i = 0; i < limit; i++) {
                    $scope.placeholderArray[i] = i;
                }
            };

            $scope.$on('package-created', function (event, args) {
                $scope.reload();
            });

            $scope.generator(10);

            $scope.requestModel = {
                limit: "10",
                offset: 0,
                order_column: 'version',
                order_type: 'desc'
            };

            $scope.app = $rootScope.currentApp;

            PackageService.getActiveProcess()
                .then(function (response) {
                    var activeProcess = response.data;

                    if (activeProcess) {
                        $rootScope.runningPackages[$scope.app.name] = {status: true};
                        $scope.openWS(activeProcess.id);
                    }
                    else {
                        $rootScope.runningPackages[$scope.app.name] = {status: false};
                    }
                })
                .catch(function (response) {
                    $rootScope.runningPackages[$scope.app.name] = {status: false};
                });

            $scope.openWS = function (id) {
                if ($rootScope.sockets && $rootScope.sockets[$scope.app.name] && $rootScope.sockets[$scope.app.name].readyState !== WebSocket.CLOSED)
                    return;

                if (!$rootScope.sockets)
                    $rootScope.sockets = {};

                var isHttps = location.href.includes('https');
                $rootScope.sockets[$scope.app.name] = new WebSocket(isHttps ? 'wss' : 'ws' + '://' + location.host + '/log_stream');
                $rootScope.sockets[$scope.app.name].onopen = function (e) {
                    $rootScope.sockets[$scope.app.name].send(JSON.stringify({
                        'X-App-Id': $scope.app.id,
                        'X-Tenant-Id': $rootScope.currentTenantId,
                        'X-Organization-Id': $scope.app.organization_id,
                        'package_id': id
                    }));
                };
                $rootScope.sockets[$scope.app.name].onclose = function (e) {
                    var packagesPageActive = $location.$$path.contains('/packages');

                    if ($rootScope.runningPackages[$scope.app.name] && $rootScope.runningPackages[$scope.app.name].logs && $rootScope.runningPackages[$scope.app.name].logs.contains('********** Package Created **********')) {
                        toastr.success("Your package is ready for app " + $scope.app.label + ".");

                        if (packagesPageActive) {
                            $rootScope.$broadcast('package-created');
                        }

                        $rootScope.runningPackages[$scope.app.name].status = false;
                        $rootScope.runningPackages[$scope.app.name].logs = "";
                        $timeout(function () {
                            $scope.$apply();
                        });
                    }
                    else {
                        if (id) {
                            PackageService.get(id)
                                .then(function (response) {
                                    if (response.data) {
                                        if (response.data.status !== 'running') {
                                            if (response.data.status === 'succeed') {
                                                toastr.success("Your package is ready for app " + $scope.app.label + ".");
                                            }
                                            else {
                                                toastr.error("An unexpected error occurred while creating a package for app " + $scope.app.label + ".");
                                            }

                                            if (packagesPageActive) {
                                                $rootScope.$broadcast('package-created');
                                            }
                                            $rootScope.runningPackages[$scope.app.name].status = false;
                                            $timeout(function () {
                                                $scope.$apply();
                                            });
                                        }
                                        else {
                                            $scope.openWS($scope.packageId);
                                        }
                                    }
                                });
                        }
                    }
                };
                $rootScope.sockets[$scope.app.name].onerror = function (e) {
                    console.log(e);
                    toastr.error($filter('translate')('Common.Error'));
                    $scope.loading = false;
                };
                $rootScope.sockets[$scope.app.name].onmessage = function (e) {
                    if (!$rootScope.runningPackages[$scope.app.name].status) {
                        $rootScope.runningPackages[$scope.app.name].status = true;
                    }

                    $rootScope.runningPackages[$scope.app.name].logs = e.data;
                    $timeout(function () {
                        $scope.$apply();
                    });

                };
            };

            $scope.createPackage = function () {
                Swal.fire({
                    html:
                        '<div style="\n' +
                        '    font-weight: 700;\n' +
                        '    color: #0d6faa;\n' +
                        '    font-size: 15px;\n' +
                        '    padding-top: 15px;\n' +
                        '    text-align: left;\n' +
                        '    padding-bottom: 15px;\n' +
                        '    padding-left: 10px;\n' +
                        '    border-bottom: 3px solid #80808017;\n' +
                        '">Create New Package!</div>' +
                        '<div style="    padding-top: 15px;\n' +
                        '    text-align: left;\n' +
                        '    font-weight: 600;\n' +
                        '    font-size: 13px;">We\'ll create a release package for you. You can publish your app using this package. Packaging process and logs will be shown in the list.</br> </br>' +
                        '<div class="form-group" ng-controller="PackageController">' +
                        '<div class="row">' +
                        //'<div class="col-sm-12" style="padding-left: 14px;">' +
                        //'<label class="radio-inline newinput go-live-input" style="padding-bottom: 10px;"><input name="type" type="radio" checked="" value="package"> Publish the app to PrimeApps Cloud after the package is prepared <span></span></label>' +
                        //'<label class="radio-inline newinput go-live-input">' +
                        //'<input name="type" type="radio" value="publish"> Automatically publish to PrimeApps cloud <span></span>' +
                        //'</label>' +
                        //'</div>' +
                        '</div>' +
                        '</div>' +
                        '</div>',
                    showCloseButton: true,
                    confirmButtonClass: 'go-live-confirm',
                    focusConfirm: false,
                    width: '60em',
                    customClass: {
                        container: 'go-live'
                    },
                    confirmButtonText: ' Create '
                }).then(function (evt) {
                    if (evt.value) {

                        PackageService.create(null)
                            .then(function (response) {
                                toastr.success("Package creation started.");
                                $scope.loading = false;
                                $scope.packageId = response.data;
                                $scope.openWS(response.data);
                                //$state.go('studio.app.packages');

                                if ($location.$$path.contains('/packages'))
                                    $scope.reload();

                                $rootScope.runningPackages[$scope.app.name] = {status: true};
                            })
                            .catch(function (response) {
                                $scope.loading = false;
                                if (response.status === 409) {
                                    toastr.error(response.data);
                                }
                                else {
                                    toastr.error($filter('translate')('Common.Error'));
                                    console.log(response);
                                }
                            });
                    }
                });
            };

            $scope.reload = function () {
                PackageService.count()
                    .then(function (response) {
                        $scope.pageTotal = response.data;

                        if ($scope.requestModel.offset != 0 && ($scope.requestModel.offset * $scope.requestModel.limit) >= $scope.pageTotal) {
                            $scope.requestModel.offset = $scope.requestModel.offset - 1;
                        }

                        PackageService.find($scope.requestModel)
                            .then(function (response) {
                                $scope.packages = response.data;
                                $scope.loading = false;
                            });
                    });
            };

            $scope.reload();

            $scope.changePage = function (page) {
                $scope.loading = true;
                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;

                $scope.page = requestModel.offset + 1;
                PackageService.find(requestModel)
                    .then(function (response) {
                        $scope.functions = response.data;
                        $scope.loading = false;
                    });
            };

            $scope.changeOffset = function () {
                $scope.changePage($scope.activePage);
            };

            $scope.getTime = function (time) {
                return moment(time).format("DD-MM-YYYY HH:ss");
            };

            $scope.asHtml = function () {
                return $sce.trustAsHtml($rootScope.runningPackages[$scope.app.name] ? $rootScope.runningPackages[$scope.app.name].logs : '');
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