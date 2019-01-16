'use strict';

angular.module('primeapps')

    .controller('MenusController', ['$rootScope', '$scope', '$filter', '$state', 'ngToast', '$modal', 'helper', 'MenusService', 'config', '$location', 'ModuleService', '$dropdown',
        function ($rootScope, $scope, $filter, $state, ngToast, $modal, helper, MenusService, config, $location, ModuleService, $dropdown) {

            //$rootScope.modules = $http.get(config.apiUrl + 'module/get_all');

            $scope.$parent.menuTopTitle = "Xbrand";
            $scope.$parent.activeMenu = 'app';
            $scope.$parent.activeMenuItem = 'menus';
            $scope.wizardStep = 0;
            $scope.loading = true;


            $scope.menuModuleList = null;
            $scope.menuLists = [];
            $scope.counter = 1;
            $scope.updateArray = [];
            $scope.deleteArray = [];
            $scope.createArray = [];
            $scope.firstMenuName = '';
            $scope.firstMenuDescription = '';
            $scope.firstDefaultMenu = null;
            $scope.firstProfileId = null;
            // $scope.newProfiles = angular.copy($scope.profiles);
            $scope.defaultMenu = false;
            $scope.description = null;
            $scope.menuName = null;
            // $scope.icons = ModuleService.getIcons();
            var isUpdate = false; // up and down menu is click
            //var systemSubscriberProfileId = $filter('filter')($scope.users, { IsSubscriber: true }, true)[0].ProfileId;
            var menuUpdate = false;

            $scope.requestModel = {
                limit: '10',
                offset: 0
            };

            MenusService.count().then(function (response) {
                $scope.pageTotal = response.data;
            });
            MenusService.find($scope.requestModel).then(function (response) {
                $scope.menuList = response.data;
                $scope.loading = false;
            });

            $scope.changePage = function (page) {
                $scope.loading = true;
                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;

                MenusService.find(requestModel).then(function (response) {
                    $scope.loading = true;
                    $scope.menuList = response.data;
                    $scope.loading = false;
                });
            };

            var isUpdate = false; // up and down menu is click
            var menuUpdate = false;

            var customModules = [
                {
                    label_tr_singular: 'Pano',
                    label_tr_plural: 'Pano',
                    label_en_singular: 'Pano',
                    label_en_plural: 'Pano',
                    name: 'dashboard',
                    route: "dashboard",
                    custom: true,
                    menu_icon: 'fa fa-pie-chart',
                    display: true
                },
                {
                    label_tr_singular: "Haber Akışı",
                    label_tr_plural: "Haber Akışları",
                    label_en_singular: 'News Feed',
                    label_en_plural: 'News Feed',
                    name: 'newsfeed',
                    route: "newsfeed",
                    custom: true,
                    menu_icon: 'fa fa-comments',
                    display: true
                },
                {
                    label_tr_singular: "Takvim",
                    label_tr_plural: "Takvim",
                    label_en_singular: 'Calendar',
                    label_en_plural: 'Calendar',
                    name: 'calendar',
                    route: "calendar",
                    custom: true,
                    menu_icon: 'fa fa-calendar',
                    display: true
                },
                {
                    label_tr_singular: "İş Listesi",
                    label_tr_plural: "İş Listesi",
                    label_en_singular: 'Task',
                    label_en_plural: 'Task',
                    name: 'tasks',
                    route: "tasks",
                    custom: true,
                    menu_icon: 'fa fa-check-square-o',
                    display: true
                },
                {
                    label_tr_singular: "Raporlar",
                    label_tr_plural: "Raporlar",
                    label_en_singular: 'Reports',
                    label_en_plural: 'Reports',
                    name: 'reports',
                    route: "reports",
                    custom: true,
                    menu_icon: 'fa fa-bar-chart',
                    display: true
                },
                {
                    label_tr_singular: "Masraf",
                    label_tr_plural: "Masraflarım",
                    label_en_singular: 'Expense',
                    label_en_plural: 'Expenses',
                    name: 'expense',
                    route: "expense",
                    custom: true,
                    menu_icon: 'fa-credit-card',
                    display: true
                },
                {
                    label_tr_singular: "Timesheet",
                    label_tr_plural: "Timesheet",
                    label_en_singular: "Timesheet",
                    label_en_plural: "Timesheet",
                    name: "timesheet",
                    route: "timesheet",
                    custom: true,
                    menu_icon: "fa fa-calendar-o",
                    display: true
                }, {
                    label_tr_singular: "Zaman Çizelgem",
                    label_tr_plural: "Zaman Çizelgem",
                    label_en_singular: "Timetracker",
                    label_en_plural: "Timetracker",
                    name: "timetracker",
                    route: "timetracker",
                    custom: true,
                    menu_icon: "fa fa-calendar-o",
                    display: true
                },
                {
                    label_tr_singular: "İş Zekası",
                    label_tr_plural: "İş Zekası",
                    label_en_singular: "Analytic",
                    label_en_plural: "Analytics",
                    name: "analytics",
                    route: "analytics",
                    custom: true,
                    menu_icon: "fa fa-line-chart",
                    display: true
                },
                {
                    label_tr_singular: "Döküman Ara",
                    label_tr_plural: "Döküman Ara",
                    label_en_singular: "Document Search",
                    label_en_plural: "Document Search",
                    name: "documentSearch",
                    route: "documentSearch",
                    custom: true,
                    menu_icon: "fa fa-search",
                    display: true
                }
            ];

            $scope.openDropdown = function (menuItem) {
                $scope['dropdown' + menuItem.name] = $scope['dropdown' + menuItem.name] || $dropdown(angular.element(document.getElementById('actionButton-' + menuItem.name)), {
                    placement: 'bottom-right',
                    scope: $scope,
                    animation: '',
                    show: true
                });

                var menuItems = [
                    {
                        'text': $filter('translate')('Common.Edit'),
                        'click': 'showFormModal(\'' + menuItem.id + '\')' //'#/app/setup/menu?id=' + menuItem.id
                    },
                    {
                        'text': $filter('translate')('Common.Copy'),
                        'click': 'showFormModal(\'' + menuItem.id + '\',true)' //'#app/setup/menu?id=' + menuItem.id + '&clone=true'
                    }
                ];

                if (!menuItem.default) {
                    menuItems.push(
                        {
                            'text': $filter('translate')('Common.Delete'),
                            'click': 'delete(\'' + menuItem.id + '\')',
                        }
                    );
                }
                $scope['dropdown' + menuItem.name].$scope.content = menuItems;
            };

            // $scope.newModuleList = angular.copy($scope.modules);
            // //push customModules to modules
            // angular.forEach(customModules, function (customModule) {
            //     $scope.newModuleList.push(customModule);
            // });

            $scope.showFormModal = function (menu) {
                if (menu) {

                } else {
                    $scope.item = {};
                }


                $scope.addNewMenuFormModal = $scope.addNewMenuFormModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/menus/menuForm.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
                });

                $scope.addNewMenuFormModal.$promise.then(function () {
                    $scope.addNewMenuFormModal.show();
                });
            };

            $scope.validate = function (menuForm) {
                menuForm.$submitted = true;
                if (menuForm.$valid)
                    return true;

                return false;
            };


            $scope.addItem = function (moduleItem, menuItem, menu_icon) {

                var menuList = {};
                menuList.no = $scope.counter;
                /**Tanım Giriş yoksa Modüldür
                 * menuItem-> Tanım Giriş
                 * moduleItem->modül picklist
                 * moduleItem.menu_icon-> modülün iconu
                 * menu_icon -> Tanım giriş için seçilen icon
                 * */
                menuList.menuModuleType = (menuItem && menuItem.length > 0) ? "Tanım Giriş" : "Mevcut Modül";
                menuList.name = (menuItem && menuItem.length > 0) ? menuItem : $scope.language === 'tr' ? moduleItem.label_tr_plural : moduleItem.label_en_plural;
                menuList.menuName = (menuItem && menuItem.length > 0) ? '' : moduleItem.name;
                menuList.id = 0;//null;
                menuList.isDynamic = $scope.moduleItem ? $scope.moduleItem.custom ? false : true : false;
                menuList.route = $scope.moduleItem != null ? $scope.moduleItem.route ? $scope.moduleItem.route : '' : '';
                menuList.menuId = menuList.no;
                menuList.icon = moduleItem != null ? moduleItem.menu_icon ? moduleItem.menu_icon : '' : menu_icon != null ? menu_icon : 'fa fa-square';
                $scope.counter += 1;
                menuList.parentId = 0;
                menuList.items = [];
                $scope.menuLists.push(menuList);

                if ($scope.id)
                    $scope.createArray.push(menuList);

                $scope.menuItem = null;
                $scope.moduleItem = null;
                $scope.menu_icon = null;

            };

            $scope.addModule = function (menuNo) {

                var menu = $filter('filter')($scope.menuLists, { no: menuNo }, true)[0];
                var labelMenu = {};
                labelMenu.no = menu.items.length > 0 ? menu.items.length + 1 : 1;
                labelMenu.name = $scope.menuModuleList != null ? $scope.language === 'tr' ? $scope.menuModuleList.label_tr_plural : $scope.menuModuleList.label_en_plural : '';
                //labelMenu.route = $scope.moduleItem != null ? $scope.moduleItem.route : '';
                labelMenu.id = 0;//null;
                labelMenu.menuId = menu.no;
                labelMenu.parentId = menu.id;
                menu.items.push(labelMenu);
            };

            $scope.selectModule = function (menuNo, labelNo, module) {

                var menu = $filter('filter')($scope.menuLists, { no: menuNo }, true)[0];
                var menuItem = $filter('filter')(menu.items, { no: labelNo }, true)[0];
                menuItem.name = module != null ? $scope.language === 'tr' ? module.label_tr_plural : module.label_en_plural : '';
                menuItem.menuName = module != null ? module.name : '';
                menuItem.route = module != null ? module.route ? module.route : '' : '';
                menuItem.icon = module != null ? module.menu_icon ? module.menu_icon : '' : '';
                menuItem.no = labelNo;
                menuItem.menuId = menu.no;
                menuItem.menuNo = menuNo;
                menuItem.isDynamic = module.custom ? false : true;
                menuItem.parentId = menu.id != null ? menu.id : 0;
                menuItem.items = [];
                //menuItems.menuParent = [];


                /**If Update, if have labelNo we pushed  in menu, else this is main menu
                 * menuItems.parentId >0, if chield have parentID
                 */
                if ($scope.id && menuItem.parentId > 0)
                    $scope.createArray.push(menuItem);
            };

            $scope.save = function (menu) {

                var resultPromise;
                $scope.loading = true;
                //If update
                if ($scope.id && !clone) {

                    //Update Menu
                    var updateList = {
                        name: $scope.menuName,
                        description: $scope.description ? $scope.description : '',
                        default: $scope.defaultMenu ? $scope.defaultMenu : false,
                        //profileId: $scope.defaultMenu ? systemSubscriberProfileId : $scope.profileItem.Id
                    };
                    //we will check  first values for update
                    var menuListIsUpdate = false;
                    if (!angular.equals($scope.firstMenuName, $scope.menuName)) {
                        updateList.name = $scope.menuName;
                        menuListIsUpdate = true;
                    }

                    if ($scope.firstProfileId != $scope.profileItem.Id && !$scope.defaultMenu) {
                        updateList.profileId = $scope.profileItem.Id;
                        menuListIsUpdate = true;
                    }

                    if ($scope.firstDefaultMenu != $scope.defaultMenu) {
                        updateList.default = $scope.defaultMenu;
                        menuListIsUpdate = true;
                    }


                    if ($scope.firstMenuDescription != $scope.description) {
                        updateList.description = $scope.description;
                        menuListIsUpdate = true;
                    }

                    $scope.updateArray.push(updateList);

                    // if ($scope.updateArray.length > 0 && menuListIsUpdate) {
                    //     resultPromise = MenusService.update($scope.id, $scope.updateArray);
                    //     menuUpdate = true;
                    // }

                    //Create MenuItem
                    if ($scope.createArray.length > 0) {
                        //we just check if createArray item isUpdate
                        if (isUpdate)
                            angular.forEach($scope.createArray, function (createItem) {
                                createItem.no = $filter('filter')($scope.menuLists, { parentId: 0, name: createItem.name, menuModuleType: createItem.menuModuleType }, true)[0].no;
                            });
                        // MenusService.createMenuItems($scope.createArray, !$scope.defaultMenu ? $scope.profileItem.Id : systemSubscriberProfileId)
                        //     .then(function onSuccess() {
                        //         if (isUpdate) {
                        //             //Update
                        //             MenusService.updateMenuItems(updateMenuItem()).then(function onSuccess() {
                        //                 //Delete
                        //                 if ($scope.deleteArray.length > 0)
                        //                     MenusService.deleteMenuItems(deleteMenuItem()).then(function onSuccess() {
                        //                         AppService.getMyAccount(true);
                        //                         ngToast.create({ content: $filter('translate')('Menu.UpdateSucces'), className: 'success' });
                        //                         $state.go('app.setup.menu_list');
                        //                     });
                        //                 else {
                        //                     AppService.getMyAccount(true);
                        //                     ngToast.create({ content: $filter('translate')('Menu.UpdateSucces'), className: 'success' });
                        //                     $state.go('app.setup.menu_list');
                        //                 }
                        //             });
                        //         }
                        //         else {
                        //             AppService.getMyAccount(true);
                        //             ngToast.create({ content: $filter('translate')('Menu.UpdateSucces'), className: 'success' });
                        //             $state.go('app.setup.menu_list');
                        //         }
                        //     });
                    }

                    //if !create -> Update menuList
                    else if (isUpdate)//$scope.updateMenuItemArray.length > 0)
                    {
                        // MenusService.updateMenuItems(updateMenuItem()).then(function onSuccess() {
                        //     //Delete
                        //     if ($scope.deleteArray.length > 0)
                        //         MenusService.deleteMenuItems(deleteMenuItem()).then(function onSuccess() {
                        //             AppService.getMyAccount(true);
                        //             ngToast.create({ content: $filter('translate')('Menu.UpdateSucces'), className: 'success' });
                        //             $state.go('app.setup.menu_list');
                        //         });
                        //     else {
                        //         AppService.getMyAccount(true);
                        //         ngToast.create({ content: $filter('translate')('Menu.UpdateSucces'), className: 'success' });
                        //         $state.go('app.setup.menu_list');
                        //     }
                        // });
                    }
                    else if (menuUpdate) {
                        resultPromise.then(function onSuccess() {
                            $state.go('app.setup.menu_list');
                        });
                    }
                    else
                        $state.go('app.setup.menu_list');
                }
                //If first create
                else {
                    var menu = [
                        {
                            // profileId: $scope.defaultMenu ? systemSubscriberProfileId : $scope.profileItem.Id,
                            name: $scope.menuName,
                            default: $scope.defaultMenu,
                            description: $scope.description,
                        }];
                    //
                    // MenusService.create(menu).then(function () {
                    //     MenusService.createMenuItems($scope.menuLists, menu[0].profileId).then(function onSuccess() {
                    //         ngToast.create({ content: $filter('translate')('Menu.MenuSaving'), className: 'success' });
                    //         $scope.loading = false;
                    //         //AppService.getMyAccount(true);
                    //         //$state.go('app.setup.menu_list');
                    //     });
                    // });
                }
            };

            $scope.edit = function (menuNo, subMenuNo) {

                if (!subMenuNo) {
                    var menu = $filter('filter')($scope.menuLists, { no: menuNo }, true)[0];
                    menu.isEdit = true;
                }
                else {
                    var menu = $filter('filter')($scope.menuLists, { no: menuNo }, true)[0];
                    var menuItem = $filter('filter')(menu.items, { no: subMenuNo }, true)[0];
                    menuItem.isEdit = true;
                }

            };

            $scope.update = function (menuNo, sub_menu_icon, subMenuNo) {
                if (!subMenuNo) {
                    var menu = $filter('filter')($scope.menuLists, { no: menuNo }, true)[0];
                    menu.icon = sub_menu_icon;
                    menu.isEdit = false;
                    //$scope.updateMenuItemArray.push(menu);
                }
                else {
                    var menu = $filter('filter')($scope.menuLists, { no: menuNo }, true)[0];
                    var menuItem = $filter('filter')(menu.items, { no: subMenuNo }, true)[0];
                    menuItem.icon = sub_menu_icon;
                    menuItem.isEdit = false;
                }

                isUpdate = true;
            };

            $scope.remove_ = function (menuId, moduleId, index, menuModuleType) {
                //If main parent or child
                if (index != null && menuModuleType) {
                    var deleteItem = '';
                    var menu = null;
                    var menuItem = null;
                    //id varsa updatetir
                    if ($scope.id) {
                        deleteItem = $scope.menuLists[menuId - 1];
                        menu = $filter('filter')($scope.createArray, { menuId: menuId }, true)[0];
                        menuItem = menu ? $filter('filter')(menu.items, { name: deleteItem.name }, true)[0] : $filter('filter')($scope.menuLists[menuId - 1].items, { name: deleteItem.name }, true)[0];

                        /**(!menuItem && !menu) ise Daha önceden eklenmiş olan Label ve itemları vardır, direkt deleteItem'ı child'larıyla pushluyoruz
                         * (menuItem && !menu ) ise Daha Önceden eklenmiş olan Label'ın Child'ı silinecekse menuItem'ı pusluyoruz
                         */
                        if (!menuItem && !menu)
                            $scope.deleteArray.push(deleteItem);
                        if ((menuItem && !menu))
                            $scope.deleteArray.push(menuItem);
                    }

                    //Delete from menuLists
                    $scope.menuLists.splice(index, 1);

                    angular.forEach($scope.menuLists, function (moduleList) {
                        moduleList.no = moduleList.no > menuId ? (moduleList.no - 1) : moduleList.no;
                        moduleList.menuId = moduleList.no;
                        //If have childs we just update menuId
                        if (moduleList.items.length > 0)
                            angular.forEach(moduleList.items, function (menuItem) {
                                menuItem.menuId = moduleList.menuId;
                            })
                    });

                    $scope.counter = $scope.counter - 1;
                    $scope.counterMenuItem = 1;
                }

                //If Chield
                else {
                    //id varsa updatetir
                    if ($scope.id) {
                        deleteItem = $scope.menuLists[menuId - 1].items[moduleId - 1];
                        menu = $filter('filter')($scope.createArray, { menuId: menuId }, true)[0];
                        menuItem = menu ? $filter('filter')(menu.items, { name: deleteItem.name }, true)[0] : $filter('filter')($scope.menuLists[menuId - 1].items, { name: deleteItem.name }, true)[0];
                        if (!menuItem && !menu)
                            $scope.deleteArray.push(menuItem);
                        if (menuItem && !menu && deleteItem.name != "")
                            $scope.deleteArray.push(menuItem);
                    }

                    $scope.menuLists[menuId - 1].items.splice((moduleId === 1 ? 0 : (moduleId - 1)), 1);

                    angular.forEach($scope.menuLists[menuId - 1].items, function (item) {
                        item.no = item.no > moduleId ? (item.no - 1) : item.no;
                    });
                }

                isUpdate = true;
            };

            $scope.radioChange = function () {
                /**moduleItem, Mevcut modül
                 * If choice value True and moduleItem was select, we will clear module picklist
                 * */
                if ($scope.module.display && $scope.moduleItem)
                    $scope.moduleItem = '';

                else {
                    $scope.menuItem = '';
                    $scope.menu_icon = null;
                }
            };

            $scope.up = function (index, no, menuItemNo) {

                var menuList = $filter('orderBy')($scope.menuLists, 'no');

                if (!menuItemNo) {
                    var prev = angular.copy(menuList[index - 1]);
                    menuList[index - 1] = angular.copy(menuList[index]);
                    menuList[index - 1].no = prev.no;
                    menuList[index - 1].menuId = prev.no;
                    if (menuList[index - 1].items.length > 0)
                        angular.forEach(menuList[index - 1].items, function (menuItem) {
                            menuItem.menuId = prev.no;
                        });
                    menuList[index] = prev;
                    menuList[index].no = no;
                    menuList[index].menuId = no;
                    if (menuList[index].items.length > 0)
                        angular.forEach(menuList[index].items, function (menuItem) {
                            menuItem.menuId = no;
                        });
                    $scope.menuLists = menuList;
                }
                else {

                    var menu = $filter('filter')($scope.menuLists, { no: no }, true)[0];
                    var menuItem = $filter('filter')(menu.items, { no: menuItemNo }, true)[0];
                    var prev = angular.copy(menu.items[index - 1]);
                    menu.items[index - 1] = angular.copy(menu.items[index]);
                    menu.items[index - 1].no = prev.no;

                    menu.items[index] = prev;
                    menu.items[index].no = menuItemNo;
                }
                isUpdate = true;
                $scope.menuLists = menuList;
            };

            $scope.down = function (index, no, menuItemNo) {

                var menuList = $filter('orderBy')($scope.menuLists, 'no');

                if (!menuItemNo) {
                    var prev = angular.copy(menuList[index + 1]);
                    menuList[index + 1] = angular.copy(menuList[index]);
                    menuList[index + 1].no = prev.no;
                    menuList[index + 1].menuId = prev.no;
                    if (menuList[index + 1].items.length > 0)
                        angular.forEach(menuList[index + 1].items, function (menuItem) {
                            menuItem.menuId = prev.no;
                        });

                    menuList[index] = prev;
                    menuList[index].no = no;
                    menuList[index].menuId = no;
                    if (menuList[index].items.length > 0)
                        angular.forEach(menuList[index].items, function (menuItem) {
                            menuItem.menuId = no;
                        });
                }
                else {
                    var menu = $filter('filter')($scope.menuLists, { no: no }, true)[0];
                    var menuItem = $filter('filter')(menu.items, { no: menuItemNo }, true)[0];
                    var prev = angular.copy(menu.items[index + 1]);
                    menu.items[index + 1] = angular.copy(menu.items[index]);
                    menu.items[index + 1].no = prev.no;

                    menu.items[index] = prev;
                    menu.items[index].no = menuItemNo;
                }
                isUpdate = true;
                $scope.menuLists = menuList;
            };

            function updateMenuItem() {

                var copyMenuList = angular.copy($scope.menuLists);
                angular.forEach(copyMenuList, function (menuItem) {
                    var filterItem = $filter('filter')(copyMenuList, { id: 0 }, true)[0];
                    var filterSubItem = $filter('filter')(menuItem.items, { id: 0 }, true); // if we added new item under the old label
                    /** we sorted descending items, because when we splice the menuItem we need a index
                     * items[{
                     *    no    :1,2,3,4,5,6
                     *    index:0,1,2,3,4,5
                     * }]
                     * example splice items 5,6, if we splice no:5
                     * no    :1,2,3,4,6
                     * index :0,1,2,3,4 than we cant match the index(4) === (no:6 -1)
                     * -------------
                     * splice items 5,6, if we splice no:6 match the index(5) === (no:6 -1)
                     * no    :1,2,3,4,5
                     * index :0,1,2,3,4 than we cant match the index(4) === (no:5 -1)
                     */
                    filterSubItem = $filter('orderBy')(filterSubItem, 'no', true);
                    if (filterItem) {

                        angular.forEach(filterItem.items, function (subItem) {
                            var filterSubItem = $filter('filter')(subItem, { id: 0 }, true)[0];
                            if (filterSubItem)
                                copyMenuList[filterItem.no - 1].items.splice(filterSubItem.no - 1, 1);
                        });
                        copyMenuList.splice(filterItem.no - 1, 1); //we deleted this item, because this item will create
                    }
                    // !filterItem -> we check this case previous step, with chield
                    else if (!filterItem && filterSubItem.length > 0) {
                        angular.forEach(filterSubItem, function (subItem) {
                            copyMenuList[menuItem.no - 1].items.splice(subItem.no - 1, 1);
                        });
                    }
                });
                return copyMenuList;
            }

            function deleteMenuItem() {
                var ids = [];
                angular.forEach($scope.deleteArray, function (deleteLabel) {
                    if (isUpdate) {
                        ids.push(deleteLabel.id);
                        if (deleteLabel.items && deleteLabel.items.length > 0) {
                            //Then, We was deleting Label's childs
                            angular.forEach(deleteLabel.items, function (deleteItem) {
                                ids.push(deleteItem.id);
                            });
                        }
                    }
                    else {
                        //First Level Label was deleting
                        if (deleteLabel.items && deleteLabel.items.length > 0) {
                            ids.push(deleteLabel.id);
                            //Then, We was deleting Label's childs
                            angular.forEach(deleteLabel.items, function (deleteItem) {
                                ids.push(deleteItem.id);
                            });
                        }
                    }
                });
                return ids;
            }

        }
    ]);