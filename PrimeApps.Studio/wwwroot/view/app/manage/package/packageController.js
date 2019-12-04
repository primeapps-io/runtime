'use strict';

angular.module('primeapps')

    .controller('PackageController', ['$rootScope', '$scope', '$state', 'PackageService', '$timeout', '$sce', '$location', '$filter', '$localStorage',
        function ($rootScope, $scope, $state, PackageService, $timeout, $sce, $location, $filter, $localStorage) {
            $scope.loading = true;
            $scope.activePage = 1;
            $rootScope.runningPackages[$rootScope.currentApp.name] = { status: true };

            $scope.$parent.activeMenu = 'app';
            $scope.$parent.activeMenuItem = 'packages';
            $rootScope.breadcrumblist[2].title = 'Packages';
  
            $scope.$on('package-created', function (event, args) {
                $scope.reload();
            });
 

            $scope.app = $rootScope.currentApp;

            PackageService.getActiveProcess()
                .then(function (response) {
                    var activeProcess = response.data;

                    if (activeProcess) {
                        $rootScope.runningPackages[$scope.app.name] = { status: true };
                        $scope.openWS(activeProcess.id);
                    }
                    else {
                        $rootScope.runningPackages[$scope.app.name] = { status: false };
                    }
                })
                .catch(function (response) {
                    $rootScope.runningPackages[$scope.app.name] = { status: false };
                });

            $scope.openWS = function (id) {
                if ($rootScope.sockets && $rootScope.sockets[$scope.app.name] && $rootScope.sockets[$scope.app.name].readyState !== WebSocket.CLOSED)
                    return;

                if (!$rootScope.sockets)
                    $rootScope.sockets = {};

                var isHttps = location.protocol === 'https:';
                $rootScope.sockets[$scope.app.name] = new WebSocket((isHttps ? 'wss' : 'ws') + '://' + location.host + '/log_stream');
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
                                    $scope.grid.dataSource.read();

                                $rootScope.runningPackages[$scope.app.name] = { status: true };
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


            //For Kendo UI
            $scope.goUrl = function (item) {
                var selection = window.getSelection();
                if (selection.toString().length === 0) {
                    //click event.
                }
            };

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
                            url: "/api/package/find",
                            type: 'GET',
                            dataType: "json",
                            beforeSend: function (req) {
                                req.setRequestHeader('Authorization', 'Bearer ' + accessToken);
                                req.setRequestHeader('X-App-Id', $rootScope.currentAppId);
                                req.setRequestHeader('X-Organization-Id', $rootScope.currentOrgId);
                            },
                            complete: function () {
                                $scope.loadingDeployments = false;
                                $scope.loading = false;
                            },
                        }
                    },
                    schema: {
                        data: "items",
                        total: "count",
                        model: {
                            id: "id",
                            fields: {
                                Version: { type: "string" },
                                StartTime: { type: "date" },
                                EndTime: { type: "date" },
                                Status: { type: "enums" }
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
                rowTemplate: function (e) {
                    var trTemp = '<tr>';
                    trTemp += '<td> <span>' + e.version + '</span></td > ';
                    trTemp += '<td><span>' + $scope.getTime(e.start_time) + '</span></td>';
                    trTemp += '<td> <span>' + $scope.getTime(e.end_time) + '</span></td > ';
                    trTemp += '<td style="text-align: center;" ng-bind-html="getIcon(dataItem.status)"></td></tr>';
                    return trTemp;
                },
                altRowTemplate: function (e) {
                    var trTemp = '<tr class="k-alt">';
                    trTemp += '<td> <span>' + e.version + '</span></td > ';
                    trTemp += '<td><span>' + $scope.getTime(e.start_time) + '</span></td>';
                    trTemp += '<td> <span>' + $scope.getTime(e.end_time) + '</span></td > ';
                    trTemp += '<td style="text-align: center;" ng-bind-html="getIcon(dataItem.status)"></td></tr>';
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
                        field: 'Version',
                        title: 'Version'
                    },
                    {
                        field: 'StartTime',
                        title: 'Start Time',
                        filterable: {
                            ui: function (element) {
                                element.kendoDatePicker({
                                    format: '{0: dd-MM-yyyy}'
                                })
                            }
                        }
                    },
                    {
                        field: 'EndTime',
                        title: 'End Time',
                        filterable: {
                            ui: function (element) {
                                element.kendoDatePicker({
                                    format: '{0: dd-MM-yyyy}'
                                })
                            }
                        }
                    },
                    {
                        field: 'Status',
                        title: 'Status',
                        values: [
                            { text: 'Running', value: 'Running' },
                            { text: 'Failed', value: 'Failed' },
                            { text: 'Succeed', value: 'Succeed' }
                        ]
                    }]
            };

            //For Kendo UI
        }
    ]);