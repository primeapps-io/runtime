'use strict';

angular.module('primeapps')
    .controller('ModuleController',
        [
            '$rootScope', '$scope', '$filter', '$state', '$modal', 'ModuleService', '$localStorage',
            function ($rootScope, $scope, $filter, $state, $modal, ModuleService, $localStorage) {
                $scope.$parent.activeMenuItem = 'modules';
                $rootScope.breadcrumblist[2].title = 'Modules';

                $scope.delete = function (module, event) {
                    var willDelete =
                        swal({
                            title: "Are you sure?",
                            text: " ",
                            icon: "warning",
                            buttons: ['Cancel', 'Yes'],
                            dangerMode: true
                        }).then(function (value) {
                            if (value) {
                                ModuleService.delete(module.id)
                                    .then(function () {
                                        var index = $rootScope.appModules.indexOf(module);
                                        $rootScope.appModules.splice(index, 1);
                                        $scope.grid.dataSource.read();

                                        toastr.success("Module is deleted successfully.", "Deleted!");

                                    })
                                    .catch(function () {
                                    });

                            }
                        });
                };

                $scope.showEditModal = function (moduleId) {
                    $scope.modalLoading = true;
                    $scope.editModal = $scope.editModal ||
                        $modal({
                            scope: $scope,
                            templateUrl: 'view/app/model/modules/editForm.html',
                            animation: 'am-fade-and-slide-right',
                            backdrop: 'static',
                            show: false
                        });
                    $scope.icons = ModuleService.getIcons();
                    // $scope.module = $filter('filter')($scope.modules, {id: moduleId}, true)[0];
                    // $scope.module.is_component = angular.equals($scope.module.system_type, "component");
                    ModuleService.getModuleById(moduleId)
                        .then(function (result) {
                            $scope.module = result.data;
                            $scope.module.is_component = angular.equals($scope.module.system_type, "component");
                            $scope.modalLoading = false;
                        });
                    $scope.editModal.$promise.then($scope.editModal.show);
                };

                $scope.cancelModule = function () {
                    $scope.editModal.hide();
                };

                $scope.saveSettings = function (editForm) {
                    if (editForm.$invalid)
                        return;

                    $scope.saving = true;
                    if (angular.isObject($scope.module.menu_icon))
                        $scope.module.menu_icon = $scope.module.menu_icon.value;

                    ModuleService.moduleUpdate($scope.module, $scope.module.id)
                        .then(function () {
                            toastr.success($filter('translate')('Setup.Modules.SaveSuccess'));
                            $scope.editModal.hide();
                            //$scope.changePage($scope.activePage);
                            $scope.grid.dataSource.read();
                        }).finally(function () {
                            $scope.saving = false;

                        });
                }

                $scope.moduleListFilter = function (item) {
                    return item.name !== 'users' && item.name !== 'profiles' && item.name !== 'roles';
                };



                //For Kendo UI
                $scope.goUrl = function (id) {
                    var selection = window.getSelection();
                    if (selection.toString().length === 0) {
                        $state.go('studio.app.moduleDesigner', { id: id });
                    }
                };

                var accessToken = $localStorage.read('access_token');

                $scope.mainGridOptions = {
                    dataSource: {
                        type: "odata-v4",
                        pageSize: 10,
                        serverPaging: true,
                        serverFiltering: true,
                        serverSorting: true,
                        transport: {
                            read: {
                                url: "/api/module/find",
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
                                    MenuIcon: { type: "string" },
                                    LabelEnPlural: { type: "string" },
                                    SystemType: { type: "enums" },
                                    Display: { type: "boolean" },
                                    Sharing: { type: "enums" }
                                }
                            }
                        }
                    },
                    scrollable: false,
                    persistSelection: true,
                    sortable: true,
                    filterable: true,
                    filter: function (e) {
                        //var temps = [];
                        if (e.filter && e.field !== 'SystemType' && e.field !== 'Sharing') {
                            for (var i = 0; i < e.filter.filters.length; i++) {
                                e.filter.filters[i].ignoreCase = true;
                                //var temp = angular.copy(e.filter.filters[i]);
                                //temp.value = temp.value.toLowerCase();
                                //temps.push(temp);
                                //e.filter.filters[i].value = e.filter.filters[i].value.toLowerCase();
                            }
                            //var lastArray = e.filter.filters.concat(temps);
                            //e.preventDefault();
                            //e.sender.dataSource.filter(temps);
                        }
                    },
                    rowTemplate: function (e) {
                        var trTemp = '<tr ng-click="goUrl(dataItem.id)">';
                        trTemp += '<td><span><i class="' + e.menu_icon + '"></i ></span></td>';
                        trTemp += '<td class="text-left"><span>' + e['label_' + $scope.language + '_plural'] + '</span></td>';
                        trTemp += '<td><span>' + $filter('translate')('Setup.Modules.Type-' + e.system_type) + '</span></td>';
                        trTemp += e.display ? '<td><span>' + $filter('translate')('Common.Yes') + '</span></td>' : '<td><span>' + $filter('translate')('Common.No') + '</span></td>';
                        trTemp += e.sharing === "private" ? '<td ><span>' + $filter('translate')('Setup.Modules.SharingPrivate') + '</span></td>' : '<td><span>' + $filter('translate')('Setup.Modules.SharingPublic') + '</span></td>';
                        trTemp += '<td ng-click="$event.stopPropagation();">' + dropDownMenuTemp + '</td></tr>';
                        return trTemp;
                    },
                    altRowTemplate: function (e) {
                        var trTemp = '<tr class="k-alt" ng-click="goUrl(dataItem.id)">';
                        trTemp += '<td><span><i class="' + e.menu_icon + '"></i ></span></td>';
                        trTemp += '<td class="text-left"><span>' + e['label_' + $scope.language + '_plural'] + '</span></td>';
                        trTemp += '<td><span>' + $filter('translate')('Setup.Modules.Type-' + e.system_type) + '</span></td>';
                        trTemp += e.display ? '<td><span>' + $filter('translate')('Common.Yes') + '</span></td>' : '<td><span>' + $filter('translate')('Common.No') + '</span></td>';
                        trTemp += e.sharing === "private" ? '<td ><span>' + $filter('translate')('Setup.Modules.SharingPrivate') + '</span></td>' : '<td><span>' + $filter('translate')('Setup.Modules.SharingPublic') + '</span></td>';
                        trTemp += '<td ng-click="$event.stopPropagation();">' + dropDownMenuTemp + '</td></tr>';
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
                            field: '',
                            title: 'Icon',
                            width: "90px", 
                        },
                        {
                            field: 'LabelEnPlural',
                            title: $filter('translate')('Setup.Modules.Name'),
                            headerAttributes: {
                                'class': 'text-left'
                            },
                        },
                        {
                            field: 'SystemType',
                            title: $filter('translate')('Setup.Modules.Type'),
                            values: [
                                { text: 'System', value: 'System' },
                                { text: 'Custom', value: 'Custom' },
                                { text: 'Component', value: 'Component' }]
                        },
                        {
                            field: 'Display',
                            title: $filter('translate')('Setup.Modules.DisplayOnMenu'),
                            filterable: {
                                messages: { isTrue: "Yes <span></span>", isFalse: "No <span></span>" },
                            },
                        },
                        {
                            field: 'Sharing',
                            title: $filter('translate')('Setup.Modules.Sharing'),
                            values: [
                                { text: 'Private', value: 'Private' },
                                { text: 'Public', value: 'Public' }]
                        },
                        {
                            field: '',
                            title: '',
                            width: "90px"
                        }]
                };
                //For Kendo UI

                var dropDownMenuTemp = '<button ng-click="$event.stopPropagation();" type="button" data-toggle="dropdown"  ' +
                    '                               class="action-button2-more"  ' +
                    '                               placement="bottom-right"  ' +
                    '                               data-animation="am-flip-x"  ' +
                    '                               data-container="body"  ' +
                    '                               bs-dropdown  ' +
                    '                               data-trigger="click"  ' +
                    '                               aria-haspopup="true"  ' +
                    '                               aria-expanded="true">  ' +
                    '                           <i class="fas fa-ellipsis-v"></i>  ' +
                    '                       </button>  ' +
                    '                       <ul class="dropdown-menu" role="menu">  ' +
                    '                           <li>  ' +
                    '                         <a href="javascript:void(0);" ui-sref="studio.app.moduleDesigner({id:dataItem.id , clone:dataItem.name})"' +
                    '                                  ng-click="$event.stopPropagation();"> ' +
                    '                                   {{"Common.Copy" | translate}}  ' +
                    '                               </a>  ' +
                    '                           </li>  ' +
                    '                           <li>  ' +
                    '                               <a href="javascript:void(0);" ng-click="$event.stopPropagation(); showEditModal(dataItem.id);">  ' +
                    '                                   Settings  ' +
                    '                               </a>  ' +
                    '                           </li>  ' +
                    '                           <li>  ' +
                    '                               <a href="javascript:void(0);" ng-click="$event.stopPropagation(); delete(dataItem, $event);">  ' +
                    '                                   {{"Common.Remove" | translate}}  ' +
                    '                               </a>  ' +
                    '                           </li>  ' +
                    '                           <li>  ' +
                    '                               <a ng-click="$event.stopPropagation();"  ' +
                    '                                  href="/#/org/{{orgId}}/app/{{appId}}/relations?id={{dataItem.id}}">  ' +
                    '                                   {{"Setup.Modules.ModuleRelations" | translate}}  ' +
                    '                               </a>  ' +
                    '                           </li>  ' +
                    '                           <li>  ' +
                    '                               <a ng-click="$event.stopPropagation();"  ' +
                    '                                  href="/#/org/{{orgId}}/app/{{appId}}/dependencies?id={{dataItem.id}}">  ' +
                    '                                   Field Dependency  ' +
                    '                               </a>  ' +
                    '                           </li>  ' +
                    '                           <li>  ' +
                    '                               <a ng-click="$event.stopPropagation();"  ' +
                    '                                  href="/#/org/{{orgId}}/app/{{appId}}/views?id={{dataItem.id}}">  ' +
                    '                                   {{"Setup.Modules.TitleFilters" | translate}}  ' +
                    '                               </a>  ' +
                    '                           </li>  ' +
                    '                           <li>  ' +
                    '                               <a ng-click="$event.stopPropagation();"  ' +
                    '                                  href="/#/org/{{orgId}}/app/{{appId}}/actionButtons?id={{dataItem.id}}">  ' +
                    '                                   {{"Setup.Modules.TitleActionButtons" | translate}}  ' +
                    '                               </a>  ' +
                    '                           </li>  ' +
                    '                           <li>  ' +
                    '                               <a ng-click="$event.stopPropagation();"  ' +
                    '                                  href="/#/org/{{orgId}}/app/{{appId}}/moduleprofilesettings/{{dataItem.id}}">  ' +
                    '                                   Profile Settings  ' +
                    '                               </a>  ' +
                    '                           </li>  ' +
                    '                       </ul>  ' +
                    '                  </div>  ';



            }
        ]);