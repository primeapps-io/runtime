'use strict';

angular.module('primeapps')

    .controller('MenusController', ['$rootScope', '$scope', '$filter', '$state', '$modal', 'helper', 'MenusService', 'config', '$location', 'ModuleService', 'ProfilesService',
        function ($rootScope, $scope, $filter, $state, $modal, helper, MenusService, config, $location, ModuleService, ProfilesService) {

            $scope.$parent.activeMenu = 'app';
            $scope.$parent.activeMenuItem = 'menus';

            $rootScope.breadcrumblist[2].title = 'Menu';

            $scope.icons = ModuleService.getIcons();

            $scope.generator = function (limit) {
                $scope.placeholderArray = [];
                for (var i = 0; i < limit; i++) {
                    $scope.placeholderArray[i] = i;
                }
            };
            $scope.generator(10);

            $scope.loading = true;

            $scope.requestModel = {
                limit: '10',
                offset: 0
            };

            MenusService.count().then(function (response) {
                $scope.pageTotal = response.data;
            });

            MenusService.find($scope.requestModel).then(function (response) {
                var menuList = response.data;
                ProfilesService.getAllBasic().then(function (response) {
                    $scope.newProfiles = response.data;
                    angular.forEach(menuList, function (menu) {
                        menu.profile_name = $filter('filter')($scope.newProfiles, { id: menu.profile_id }, true)[0].name;
                    });
                    $scope.menuList = menuList;
                    $scope.menuListState = menuList;
                    $scope.loading = false;
                });
            });

            $scope.changePage = function (page) {
                $scope.loading = true;
                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;

                MenusService.find(requestModel).then(function (response) {
                    var menuList = response.data;
                    ProfilesService.getAllBasic().then(function (response) {
                        $scope.newProfiles = response.data;
                        angular.forEach(menuList, function (menu) {
                            menu.profile_name = $filter('filter')($scope.newProfiles, { id: menu.profile_id }, true)[0].name;
                        });
                        $scope.menuList = menuList;
                        $scope.menuListState = menuList;
                        $scope.loading = false;
                    });
                });
            };

            $scope.changeOffset = function () {
                $scope.changePage(1)
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

            $scope.newModuleList = angular.copy($rootScope.appModules);
            //push customModules to modules
            angular.forEach(customModules, function (customModule) {
                $scope.newModuleList.push(customModule);
            });

            $scope.showFormModal = function (id, _clone) {
                $scope.id = id;
                $scope.wizardStep = 0;
                $scope.menuLists = [];
                $scope.menu = {};
                $scope.counter = 1;
                $scope.clone = angular.copy(_clone);

                /**
                 * Profile picklist filter, If exist delete from picklist
                 * Yapılacaklar
                 * */
                angular.forEach($scope.menuList, function (menu) {
                    $filter('filter')($scope.newProfiles, { id: menu.profile_id }, true)[0].deleted = true;
                });

                if (id)
                    setMenuList(id);

                $scope.addNewMenuFormModal = $scope.addNewMenuFormModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/visualization/menus/menuForm.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
                });

                $scope.addNewMenuFormModal.$promise.then(function () {
                    $scope.addNewMenuFormModal.show();
                });
            };

            var setMenuList = function (id) {

                $scope.menuLists = [];
                $scope.updateArray = [];
                $scope.deleteArray = [];
                $scope.createArray = [];
                $scope.index = $scope.createArray.length;

                if (id) {
                    MenusService.getMenuById(id).then(function (response) {
                        $scope.menu = response.data;
                        $scope.menu.name = $scope.clone ? $scope.menu.name + '(Copy)' : $scope.menu.name;
                        //We will use this first values when click the next button, and find is update or not
                        $scope.firstMenuName = $scope.menu.name;
                        $scope.firstDefaultMenu = $scope.menu.default;
                        $scope.firstMenuDescription = $scope.menu.description;

                        //If update, we added again all profiles
                        //Then, get selected profile and menuitems
                        $scope.menu.profile = $filter('filter')($scope.newProfiles, { id: response.data.profile_id }, true)[0];
                        //If clone deleted true
                        $scope.menu.profile.deleted = $scope.clone ? true : false;

                        $scope.firstProfileId = $scope.menu.profile.id;

                        //We use firstprofileId because maybe user was changed
                        MenusService.getMenuItem($scope.menu.profile_id).then(function onSuccess(response) {
                            $scope.menuLists = [];
                            for (var i = 0; i < response.data.length; i++) {
                                var menuList = {};
                                menuList.menuModuleType = response.data[i].route ? 'Mevcut Modül' : 'Tanım Giriş';
                                menuList.name = $scope.language === 'tr' ? response.data[i].label_tr : response.data[i].label_en;
                                menuList.id = response.data[i].id;
                                menuList.isDynamic = response.data[i].is_dynamic;
                                menuList.no = i + 1;//response.data[i].order;
                                menuList.menuId = menuList.no;
                                menuList.parentId = 0;
                                menuList.items = [];
                                menuList.route = response.data[i].route ? response.data[i].route.contains('modules/') ? '' : response.data[i].route : '';
                                menuList.icon = response.data[i].menu_icon ? response.data[i].menu_icon : 'fa fa-square';
                                menuList.menuName = response.data[i].route ? response.data[i].route.replace('modules/', '') : '';
                                // menuList.menuParent = [];

                                for (var j = 0; j < response.data[i].menu_items.length; j++) {
                                    if (!response.data[i].menu_items[j].deleted) {
                                        var labelMenu = {};
                                        labelMenu.name = response.data[i].menu_items[j].label_tr;
                                        labelMenu.menuName = response.data[i].menu_items[j].route ? response.data[i].menu_items[j].route.replace('modules/', '') : '';
                                        labelMenu.no = j + 1;//response.data[i].menu_items[j].order;
                                        labelMenu.menuId = menuList.no;
                                        labelMenu.id = response.data[i].menu_items[j].id;
                                        labelMenu.isDynamic = response.data[i].menu_items[j].is_dynamic;
                                        labelMenu.parentId = $scope.clone ? 0 : response.data[i].menu_items[j].parent_id;
                                        labelMenu.icon = response.data[i].menu_items[j].menu_icon ? response.data[i].menu_items[j].menu_icon : 'fa fa-square';
                                        labelMenu.route = response.data[i].menu_items[j].route ? response.data[i].menu_items[j].route.contains('modules/') ? '' : response.data[i].menu_items[j].route : '';
                                        menuList.items.push(labelMenu);
                                    }
                                }
                                $scope.menuLists.push(menuList);
                            }
                            //Yeni eklenecek olan modülü +1'den başlatmamız gerekiyor
                            $scope.counter = $scope.menuLists.length + 1;

                        }).finally(function () {
                            $scope.loading = false;
                        });
                    });
                }
            }

            $scope.validate = function (menuForm, next) {
                menuForm.$submitted = true;
                if (menuForm.$valid) {
                    $scope.wizardStep += next ? 1 : -1;
                    return true;
                }
                return false;
            };


            $scope.addItem = function () {

                var menuList = {};
                menuList.no = $scope.counter;
                /**Tanım Giriş yoksa Modüldür
                 * menuItem-> Tanım Giriş
                 * moduleItem->modül picklist
                 * moduleItem.menu_icon-> modülün iconu
                 * menu_icon -> Tanım giriş için seçilen icon
                 * */
                menuList.menuModuleType = ($scope.menu.menuItem && $scope.menu.menuItem.length > 0) ? "Tanım Giriş" : "Mevcut Modül";
                menuList.name = ($scope.menu.menuItem && $scope.menu.menuItem.length > 0) ? $scope.menu.menuItem : $scope.language === 'tr' ? $scope.menu.moduleItem.label_tr_plural : $scope.menu.moduleItem.label_en_plural;
                menuList.menuName = ($scope.menu.menuItem && $scope.menu.menuItem.length > 0) ? '' : $scope.menu.moduleItem.name;
                menuList.id = 0;//null;
                menuList.isDynamic = $scope.menu.moduleItem ? $scope.menu.moduleItem.custom ? false : true : false;
                menuList.route = $scope.menu.moduleItem != null ? $scope.menu.moduleItem.route ? $scope.menu.moduleItem.route : '' : '';
                menuList.menuId = menuList.no;
                menuList.icon = $scope.menu.moduleItem != null ? $scope.menu.moduleItem.menu_icon ? $scope.menu.moduleItem.menu_icon : 'fa fa-square' : $scope.menu.menu_icon != null ? $scope.menu.menu_icon.value : 'fa fa-square';
                $scope.counter += 1;
                menuList.parentId = 0;
                menuList.items = [];
                menuList.index = $scope.index;
                $scope.index += 1;
                $scope.menuLists.push(menuList);

                if ($scope.id)
                    $scope.createArray.push(menuList);

                $scope.menu.menuItem = null;
                $scope.menu.moduleItem = null;
                $scope.menu.menu_icon = null;

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
                menuItem.icon = module != null ? module.menu_icon ? module.menu_icon : 'fa fa-square' : 'fa fa-square';
                menuItem.no = labelNo;
                menuItem.menuId = menu.no;
                menuItem.menuNo = menuNo;
                menuItem.isDynamic = module.custom ? false : true;
                menuItem.parentId = menu.id != null ? menu.id : 0;
                menuItem.items = [];
                menuItem.index = $scope.index;
                $scope.index += 1;
                //menuItems.menuParent = [];


                /**If Update, if have labelNo we pushed  in menu, else this is main menu
                 * menuItems.parentId >0, if chield have parentID
                 */
                if ($scope.id && menuItem.parentId > 0)
                    $scope.createArray.push(menuItem);
            };

            $scope.save = function (menu) {

                $scope.saving = true;
                var resultPromise;

                //If update
                if (menu.id && !$scope.clone) {

                    //Update Menu
                    var updateList = {
                        name: $scope.menu.name,
                        description: $scope.description ? $scope.menu.description : '',
                        default: $scope.menu.default ? $scope.menu.default : false,
                        profile_id: $scope.menu.default ? 1 : $scope.menu.profile.id
                    };
                    //we will check  first values for update
                    var menuListIsUpdate = false;
                    if (!angular.equals($scope.firstMenuName, $scope.menu.name)) {
                        updateList.name = $scope.menu.name;
                        menuListIsUpdate = true;
                    }

                    if ($scope.firstProfileId != $scope.menu.profile_id && !$scope.menu.default) { //profile.id
                        updateList.profile_id = $scope.menu.profile.id;
                        menuListIsUpdate = true;
                    }

                    if ($scope.firstDefaultMenu != $scope.menu.default) {
                        updateList.default = $scope.menu.default;
                        menuListIsUpdate = true;
                    }


                    if ($scope.firstMenuDescription != $scope.menu.description) {
                        updateList.description = $scope.menu.description;
                        menuListIsUpdate = true;
                    }

                    $scope.updateArray.push(updateList);

                    if ($scope.updateArray.length > 0 && menuListIsUpdate) {
                        resultPromise = MenusService.update($scope.id, $scope.updateArray);
                        menuUpdate = true;
                    }

                    //Create MenuItem
                    if ($scope.createArray.length > 0) {
                        //we just check if createArray item isUpdate
                        if (isUpdate)
                            angular.forEach($scope.createArray, function (createItem) {
                                var findItem = $filter('filter')($scope.menuLists, { parentId: 0, name: createItem.name, menuModuleType: createItem.menuModuleType }, true)[0];
                                if (findItem)
                                    createItem.no = findItem.no;
                                else
                                    for (var i = 0; i < $scope.menuLists.length; i++) {
                                        if ($scope.menuLists[i].items.length > 0) {
                                            findItem = $filter('filter')($scope.menuLists[i].items, { id: 0, name: createItem.name, menuModuleType: createItem.menuModuleType }, true)[0];
                                            if (findItem)
                                                createItem.no = findItem.no;
                                        }
                                    }
                            });
                        MenusService.createMenuItems($scope.createArray, !$scope.menu.default ? $scope.menu.profile.id : 1)
                            .then(function onSuccess() {
                                if (isUpdate) {
                                    //Update
                                    MenusService.updateMenuItems(updateMenuItem()).then(function onSuccess() {
                                        //Delete
                                        if ($scope.deleteArray.length > 0)
                                            MenusService.deleteMenuItems(deleteMenuItem()).then(function onSuccess() {
                                                toastr.success($filter('translate')('Menu.UpdateSucces'));
                                                $scope.addNewMenuFormModal.hide();
                                                $scope.changePage(1);
                                            }).finally(function () {
                                                $scope.saving = false;
                                            });
                                        else {

                                            toastr.success($filter('translate')('Menu.UpdateSucces'));
                                            $scope.saving = false;
                                            $scope.addNewMenuFormModal.hide();
                                            $scope.changePage(1);
                                        }
                                    }).finally(function () {
                                        $scope.saving = false;
                                    });
                                }
                                else {
                                    toastr.success($filter('translate')('Menu.UpdateSucces'));
                                    $scope.saving = false;
                                    $scope.addNewMenuFormModal.hide();
                                    $scope.changePage(1);
                                }
                            });
                    }

                    //if !create -> Update menuList
                    else if (isUpdate)//$scope.updateMenuItemArray.length > 0)
                    {
                        MenusService.updateMenuItems(updateMenuItem()).then(function onSuccess() {
                            //Delete
                            if ($scope.deleteArray.length > 0)
                                MenusService.deleteMenuItems(deleteMenuItem()).then(function onSuccess() {

                                    toastr.success($filter('translate')('Menu.UpdateSucces'));
                                    $scope.addNewMenuFormModal.hide();
                                    $scope.changePage(1);
                                }).finally(function () {
                                    $scope.saving = false;
                                });
                            else {

                                toastr.success($filter('translate')('Menu.UpdateSucces'));
                                $scope.saving = false;
                                $scope.addNewMenuFormModal.hide();
                                $scope.changePage(1);
                            }
                        }).finally(function () {

                            $scope.saving = false;
                        });
                    }
                    else if (menuUpdate) {
                        resultPromise.then(function onSuccess() {
                            toastr.success($filter('translate')('Menu.UpdateSucces'));
                            $scope.saving = false;
                            $scope.addNewMenuFormModal.hide();
                            $scope.changePage(1);
                        });
                    }
                    else {
                        toastr.success($filter('translate')('Menu.UpdateSucces'));
                        $scope.addNewMenuFormModal.hide();
                        $scope.saving = false;
                    }
                }
                //If first create
                else {
                    var menu = [
                        {
                            profile_id: $scope.menu.default ? 1 : $scope.menu.profile.id,
                            name: $scope.menu.name,
                            default: $scope.menu.default ? $scope.menu.default : false,
                            description: $scope.menu.description,
                        }];

                    MenusService.create(menu).then(function () {
                        MenusService.createMenuItems($scope.menuLists, menu[0].profile_id).then(function onSuccess() {

                            toastr.success($filter('translate')('Menu.MenuSaving'));
                            $scope.addNewMenuFormModal.hide();
                            $scope.changePage(1);
                            $scope.pageTotal += 1;
                        }).finally(function () {
                            $scope.saving = false;
                        });
                    });
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

                        if (menuItem && !menu)
                            $scope.deleteArray.push(menuItem);

                        if (deleteItem) {
                            $scope.createArray.splice(deleteItem.index, 1);
                            $scope.index = deleteItem.index ? $scope.index - 1 : $scope.index;
                        }
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

                        if (deleteItem) {
                            $scope.createArray.splice(deleteItem.index, 1);
                            $scope.index = deleteItem.index ? $scope.index - 1 : $scope.index;
                        }
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
                if ($scope.menu.display && $scope.menu.moduleItem)
                // if (moduleDisplay && moduleItem)
                    $scope.menu.moduleItem = '';

                else {
                    $scope.menu.menuItem = '';
                    $scope.menu.menu_icon = null;
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
                var index = undefined;
                var subIndex = undefined;

                var copyMenuList = angular.copy($scope.menuLists);
                angular.forEach(copyMenuList, function (menuItem) {
                    if (menuItem.items.length > 0)

                        for (var i = 0; i < menuItem.items.length; i++) {
                            if (menuItem.items[i].id === 0) {
                                subIndex = menuItem.items.findIndex(function (el) {
                                    return el.id === 0;
                                });
                                index = copyMenuList.findIndex(function (el) {
                                    return el.no === menuItem.no;
                                });
                                i = subIndex - 1;
                                copyMenuList[index].items.splice(subIndex, 1);
                            }
                        }

                    if (menuItem.id === 0) {
                        index = copyMenuList.findIndex(function (el) {
                            return el.no === menuItem.no;
                        });
                        i = index - 1;
                        copyMenuList.splice(index, 1); //we deleted this item, because this item will create
                    }
                });

                // var filterItem = $filter('filter')(copyMenuList, { id: 0 }, true)[0];
                // var filterSubItem = $filter('filter')(menuItem.items, { id: 0 }, true); // if we added new item under the old label
                // /** we sorted descending items, because when we splice the menuItem we need a index
                //  */
                // filterSubItem = $filter('orderBy')(filterSubItem, 'no', true);
                // var index = undefined;
                // var SubIndex = undefined;
                // if (filterItem) {
                //     //angular.forEach(filterItem.items, function (subItem) {
                //     if (filterItem.items.length > 0) {
                //         var filterSubItem = $filter('filter')(filterItem.items, { id: 0 }, true);
                //         if (filterSubItem) {
                //             for (var i = 0; i < filterSubItem.length; i++) {
                //                 SubIndex = filterItem.items.findIndex(function (el) {
                //                     return el.id === 0;
                //                 });//(x => x.id === 0);//filterSubItem.no);
                //                 index = copyMenuList.findIndex(function (el) {
                //                     return el.no === filterItem.no;
                //                 });//(x => x.no === filterItem.no);
                //                 copyMenuList[index].items.splice(SubIndex, 1);
                //             }
                //         }
                //     }
                //
                //     index = copyMenuList.findIndex(function (el) {
                //         return el.no === filterItem.no;
                //     });//(x => x.no === filterItem.no);
                //     copyMenuList.splice(index, 1); //we deleted this item, because this item will create
                // }
                // // !filterItem -> we check this case previous step, with chield
                // if (!filterItem && filterSubItem.length > 0) {
                //     index = copyMenuList.findIndex(function (el) {
                //         return el.no === menuItem.no;
                //     });//x => x.no === menuItem.no);
                //     for (var i = 0; i < copyMenuList[index].items.length; i++) {
                //         SubIndex = copyMenuList[index].items.findIndex(function (el) {
                //             return el.id === 0;
                //         });//(x => x.id === 0);
                //         copyMenuList[index].items.splice(SubIndex, 1);
                //     }
                // }
                // if (filterItem && filterSubItem.length > 0) {
                //     index = copyMenuList.findIndex(function (el) {
                //         return el.id === 0;
                //     });//(x => x.no === menuItem.no);
                //     for (var i = 0; i < copyMenuList[index].items.length; i++) {
                //         SubIndex = copyMenuList[index].items.findIndex(function (el) {
                //             return el.id === 0;
                //         });//(x => x.id === 0);
                //         copyMenuList[index].items.splice(SubIndex, 1);
                //     }
                // }
                // }
                //);
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

            //Menu Delete
            $scope.delete = function (id) {
                //First delete Menu
                var willDelete =
                    swal({
                        title: "Are you sure?",
                        text: " ",
                        icon: "warning",
                        buttons: ['Cancel', 'Yes'],
                        dangerMode: true
                    }).then(function (value) {
                        if (value) {
                            MenusService.delete(id).then(function () {
                                $scope.changePage(1);
                                $scope.pageTotal = $scope.pageTotal - 1;
                                toastr.success($filter('translate')('Menu.DeleteSuccess'));
                            }).catch(function () {
                                $scope.menuList = $scope.menuListState;

                                if ($scope.addNewMenuFormModal) {
                                    $scope.addNewMenuFormModal.hide();
                                    $scope.saving = false;
                                }
                            });
                        }
                    });
            };
        }

    ])
;