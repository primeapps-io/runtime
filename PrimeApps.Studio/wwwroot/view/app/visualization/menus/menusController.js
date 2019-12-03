'use strict';

angular.module('primeapps')

    .controller('MenusController', ['$rootScope', '$scope', '$filter', '$state', '$modal', 'helper', 'MenusService', 'config', '$location', 'ModuleService', 'ProfilesService', '$localStorage',
        function ($rootScope, $scope, $filter, $state, $modal, helper, MenusService, config, $location, ModuleService, ProfilesService, $localStorage) {

            $scope.$parent.activeMenu = 'app';
            $scope.$parent.activeMenuItem = 'menus';
            $scope.newProfiles = angular.copy($rootScope.appProfiles);

            $rootScope.breadcrumblist[2].title = 'Menus';

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

            var isUpdate = false; // up and down menu is click
            var menuUpdate = false;
            var customModules = [
                {
                    label_tr_singular: 'Pano',
                    label_tr_plural: 'Pano',
                    label_en_singular: 'Dashboard',
                    label_en_plural: 'Dashboard',
                    name: 'dashboard',
                    route: "dashboard",
                    custom: true,
                    menu_icon: 'fa fa-pie-chart',
                    display: true,
                    isDynamic: false
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
                    display: true,
                    isDynamic: false
                }];

            $scope.newModuleList = angular.copy($rootScope.appModules);

            angular.forEach(customModules, function (customModule) {
                $scope.newModuleList.push(customModule);
            });
            /* {
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
                 label_tr_singular: "Masraf",
                 label_tr_plural: "Masraflarım",
                 label_en_singular: 'Expense',
                 label_en_plural: 'Expenses',
                 name: 'expense',
                 route: "expense",
                 custom: true,
                 menu_icon: 'fa fa-credit-card',
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
         ];*/

            $scope.showFormModal = function (id, cloneSettings) {
                $scope.loadingModal = true;
                $scope.id = id;
                $scope.wizardStep = 0;
                $scope.data = [];
                $scope.menu = {};
                $scope.counter = 1;
                $scope.clone = angular.copy(cloneSettings);
                $scope.updateArray = [];
                $scope.deleteArray = [];
                $scope.createArray = [];
                /**
                 * Profile picklist filter, If exist delete from picklist
                 * Yapılacaklar
                 * */
                angular.forEach($scope.menuList, function (menu) {
                    $filter('filter')($scope.newProfiles, { id: menu.profile_id }, true)[0].deleted = true;
                });

                if (id) {
                    setMenuList(id);
                } else {
                    for (var i = 0; i < $scope.newModuleList.length; i++) {
                        $scope.addItem($scope.newModuleList[i]);
                    }
                    $scope.loadingModal = false;
                }
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

                $scope.data = [];
                /* $scope.updateArray = [];
                 $scope.deleteArray = [];
                 $scope.createArray = [];*/
                $scope.index = $scope.createArray.length;

                if (id) {
                    MenusService.getMenuById(id)
                        .then(function (response) {
                            $scope.menu = response.data;
                            $scope.menu.name = $scope.clone ? $scope.menu.name + '(Copy)' : $scope.menu.name;
                            $scope.menu.default = $scope.clone ? false : $scope.menu.default;

                            //If update, we added again all profiles
                            //Then, get selected profile and menuitems
                            $scope.menu.profile = $scope.clone ? null : $filter('filter')($scope.newProfiles, { id: response.data.profile_id }, true)[0];
                            //If clone deleted true
                            if ($scope.menu.profile) {
                                $scope.menu.profile.deleted = false;
                            }
                            // $scope.menu.profile.deleted = $scope.clone ? true : false;

                            $scope.loadingModal = true;
                            //We use firstprofileId because maybe user was changed
                            MenusService.getMenuItem($scope.menu.profile_id)
                                .then(function onSuccess(response) {
                                    $scope.data = [];
                                    for (var i = 0; i < response.data.length; i++) {
                                        var menuList = {};
                                        menuList.menuModuleType = response.data[i].route ? 'Mevcut Modül' : 'Tanım Giriş';
                                        menuList.name = $scope.language === 'tr' ? response.data[i].label_tr : response.data[i].label_en;
                                        menuList.id = response.data[i].id;
                                        menuList.isDynamic = response.data[i].is_dynamic;
                                        menuList.no = i + 1;//response.data[i].order;
                                        menuList.menuId = menuList.no;
                                        menuList.parentId = 0;
                                        menuList.disabled = response.data[i].deleted;
                                        menuList.isEdit = false;
                                        menuList.nodes = [];
                                        //menuList.newIcon = response.data[i].deleted ? "fa fa-eye-slash" : "fa fa-eye";
                                        menuList.route = response.data[i].route ? response.data[i].route.contains('modules/') ? '' : response.data[i].route : '';
                                        menuList.icon = response.data[i].menu_icon ? response.data[i].menu_icon : 'fa fa-square';
                                        menuList.menuName = response.data[i].route ? response.data[i].route.replace('modules/', '') : '';
                                        // menuList.menuParent = [];

                                        for (var j = 0; j < response.data[i].menu_items.length; j++) {
                                            // if (!response.data[i].menu_items[j].deleted) {
                                            var labelMenu = {};
                                            labelMenu.name = response.data[i].menu_items[j].label_tr;
                                            labelMenu.menuName = response.data[i].menu_items[j].route ? response.data[i].menu_items[j].route.replace('modules/', '') : '';
                                            labelMenu.no = j + 1;//response.data[i].menu_items[j].order;
                                            labelMenu.menuId = menuList.no;
                                            labelMenu.isEdit = false;
                                            labelMenu.nodes = [];
                                            labelMenu.disabled = response.data[i].menu_items[j].deleted;
                                            //labelMenu.newIcon = response.data[i].menu_items[j].deleted ? "fa fa-eye-slash" : "fa fa-eye";
                                            labelMenu.id = response.data[i].menu_items[j].id;
                                            labelMenu.isDynamic = response.data[i].menu_items[j].is_dynamic;
                                            labelMenu.parentId = $scope.clone ? 0 : response.data[i].menu_items[j].parent_id;
                                            labelMenu.icon = response.data[i].menu_items[j].menu_icon ? response.data[i].menu_items[j].menu_icon : 'fa fa-square';
                                            labelMenu.route = response.data[i].menu_items[j].route ? response.data[i].menu_items[j].route.contains('modules/') ? '' : response.data[i].menu_items[j].route : '';
                                            menuList.nodes.push(labelMenu);
                                            // }
                                        }
                                        $scope.data.push(menuList);
                                    }
                                    //Yeni eklenecek olan modülü +1'den başlatmamız gerekiyor
                                    $scope.counter = $scope.data.length + 1;
                                    $scope.loadingModal = false;
                                })
                                .catch(function () {
                                    $scope.loadingModal = false;
                                    $scope.addNewMenuFormModal.hide();
                                });
                        });
                }
            };

            $scope.validate = function (menuForm, next) {
                menuForm.$submitted = true;
                if (menuForm.$valid) {
                    //if (!$scope.clone) {
                    //	$scope.wizardStep += next ? 1 : $scope.wizardStep > 0 ? -1 : $scope.wizardStep;
                    //}
                    return true;
                } else if (menuForm.$invalid) {
                    if (menuForm.name_menu.$error.required && menuForm.profile_name.$error.required) {
                        toastr.error($filter('translate')('Menu.RequiredError'));
                    }
                    if (!menuForm.name_menu.$error.required && menuForm.profile_name.$error.required) {
                        toastr.error($filter('translate')('Menu.ProfileRequiredError'));
                    }
                    if (!menuForm.profile_name.$error.required && menuForm.name_menu.$error.required) {
                        toastr.error($filter('translate')('Menu.MenuNameRequiredError'));
                    }

                    return false;
                }

            };

            $scope.addItem = function (module) {
                if (!module) {
                    module = {
                        // label_tr_plural: "Yeni Kategori Adını Giriniz",
                        // label_en_plural: "Enter New Category Name",
                        menu_icon: "fa fa-square",
                        order: 0
                        // display: true,/
                    };
                }

                var menuList = {};
                menuList.no = $scope.counter;
                // menuList.newIcon = "fa fa-eye";
                /**Tanım Giriş yoksa Modüldür
                 * menuItem-> Tanım Giriş
                 * moduleItem->modül picklist
                 * moduleItem.menu_icon-> modülün iconu
                 * menu_icon -> Tanım giriş için seçilen icon
                 * */
                menuList.menuModuleType = !module.name ? "Tanım Giriş" : "Mevcut Modül";// !$scope.menu.moduleItem.id
                menuList.isEdit = !module.display ? true : false;
                menuList.name = $scope.language === 'tr' ? module.label_tr_plural : module.label_en_plural;
                menuList.menuName = module.name; //$scope.menu.moduleItem.name;
                menuList.id = 0;
                menuList.isDynamic = module.system_type === "system" ? true : false;//module.id ? true : false;
                menuList.route = module.route ? module.route : '';
                menuList.menuId = menuList.no;
                menuList.icon = module.menu_icon ? module.menu_icon : 'fa fa-square';
                $scope.counter += 1;
                menuList.parentId = 0;
                menuList.disabled = false;
                menuList.nodes = [];
                //menuList.newIcon = 'fa fa-eye';
                menuList.index = $scope.index;
                $scope.index += 1;
                //$scope.data.push(menuList);
                /*Eklenen kategori en üstte gelmeli*/
                $scope.data.splice(0, 0, menuList);

                /*$scope.menu.menuItem = null;
                $scope.menu.moduleItem = null;
                $scope.menu.menu_icon = null;*/

            };

            $scope.addModule = function (menuNo) {

                var menu = $filter('filter')($scope.data, { no: menuNo }, true)[0];
                var labelMenu = {};
                labelMenu.no = menu.nodes.length > 0 ? menu.nodes.length + 1 : 1;
                labelMenu.name = $scope.menuModuleList !== null ? $scope.language === 'tr' ? $scope.menuModuleList.label_tr_plural : $scope.menuModuleList.label_en_plural : '';
                labelMenu.id = 0;
                labelMenu.menuId = menu.no;
                labelMenu.parentId = menu.id;
                labelMenu.showModuleList = true;
                menu.nodes.push(labelMenu);
            };

            $scope.selectModule = function (menuNo, labelNo, module) {

                var menu = $filter('filter')($scope.data, { no: menuNo }, true)[0];
                var menuItem = $filter('filter')(menu.nodes, { no: labelNo }, true)[0];
                menuItem.name = module !== null ? $scope.language === 'tr' ? module.label_tr_plural : module.label_en_plural : '';
                menuItem.menuName = module !== null ? module.name : '';
                menuItem.route = module !== null ? module.route ? module.route : '' : '';
                menuItem.icon = module !== null ? module.menu_icon ? module.menu_icon : 'fa fa-square' : 'fa fa-square';
                menuItem.no = labelNo;
                menuItem.menuId = menu.no;
                menuItem.menuNo = menuNo;
                menuItem.isDynamic = module.custom ? false : true;
                menuItem.parentId = menu.id !== null ? menu.id : 0;
                menuItem.nodes = [];
                menuItem.index = $scope.index;
                menuItem.isEdit = true;
                menuItem.showModuleList = false;
                $scope.index += 1;
            };

            $scope.save = function (menu, menuForm) {

                var validResult = $scope.validate(menuForm);
                if (!validResult) {
                    return false;
                }
                var copyData = angular.copy($scope.data);
                $scope.saving = true;
                var resultPromise;
                var count = 0;
                //  var countChield = 0;
                for (var i = 0; i < copyData.length; i++) {

                    /*Her türlü burada menuId ve no'ları yeniden düzenlemeliyiz*/
                    copyData[i].no = count + 1;
                    copyData[i].menuId = count + 1;
                    copyData[i].menuNo = copyData[i].menuId;

                    /*eğer kategori ise alt kırılımında yeni eklenen child var mı ? varsa ekle*/
                    for (var j = 0; j < copyData[i].nodes.length; j++) {


                        var createItem = $filter('filter')(copyData[i].nodes, { id: 0 }, true)[0];
                        copyData[i].nodes[j].no = j + 1; // countChield + 1;
                        copyData[i].nodes[j].menuId = count + 1;//count + 1;
                        copyData[i].nodes[j].menuNo = copyData[i].nodes[j].menuId;
                        //parentId ana kırılımın id'si olacaktır
                        copyData[i].nodes[j].parentId = copyData[i].id;

                    }
                    //Eğer ana kırılım(modül ya da kategori) ilk defa ekleniyorsa nodelarla beraber ekle
                    if (copyData[i].id === 0) {
                        /*menu.id yoksa nodes'lara dokunulmayacak ilk create gerçekleşecek
                        * Eğer menu.id varsa bu menu daha önceden create edilmiştir.Bu yüzden nodesları tekrardan create etmemek için nodes arrayini temizliyoruz*/
                        $scope.createArray.push(copyData[i]);
                        copyData.splice(i, 1);
                        i--;
                    }

                    count++;

                }

                //If update
                if (menu.id && !$scope.clone) {

                    //we will check  first values for update
                    var menuListIsUpdate = isMenuDirty(menuForm);

                    if ($scope.updateArray.length > 0 && menuListIsUpdate) {
                        resultPromise = MenusService.update($scope.id, $scope.updateArray);
                        menuUpdate = true;
                    }

                    //Create MenuItem
                    if ($scope.createArray.length > 0) {

                        //we just check if createArray item isUpdate

                        MenusService.createMenuItems($scope.createArray, !$scope.menu.default ? $scope.menu.profile.id : 1)
                            .then(function onSuccess() {
                                if (isUpdate) {
                                    //Update
                                    MenusService.updateMenuItems(copyData).then(function onSuccess() { //updateMenuItem()
                                        //Delete
                                        if ($scope.deleteArray.length > 0)
                                            MenusService.deleteMenuItems($scope.deleteArray).then(function onSuccess() {
                                                toastr.success($filter('translate')('Menu.UpdateSucces'));
                                                $scope.addNewMenuFormModal.hide();
                                                $scope.changePage($scope.activePage);
                                                $scope.saving = false;
                                            }).catch(function () {
                                                $scope.saving = false;
                                                $scope.addNewMenuFormModal.hide();
                                            });
                                        else {

                                            toastr.success($filter('translate')('Menu.UpdateSucces'));
                                            $scope.saving = false;
                                            $scope.addNewMenuFormModal.hide();
                                            $scope.changePage($scope.activePage);
                                        }
                                    }).catch(function () {
                                        $scope.saving = false;
                                        $scope.addNewMenuFormModal.hide();
                                    });
                                } else {
                                    toastr.success($filter('translate')('Menu.UpdateSucces'));
                                    $scope.saving = false;
                                    $scope.addNewMenuFormModal.hide();
                                    $scope.changePage($scope.activePage);
                                }
                            });
                    }

                    //if !create -> Update menuList
                    else if (isUpdate) {
                        MenusService.updateMenuItems(copyData).then(function onSuccess() {
                            //Delete
                            if ($scope.deleteArray.length > 0)
                                MenusService.deleteMenuItems($scope.deleteArray).then(function onSuccess() {

                                    toastr.success($filter('translate')('Menu.UpdateSucces'));
                                    $scope.addNewMenuFormModal.hide();
                                    $scope.changePage($scope.activePage);
                                    $scope.saving = false;
                                }).catch(function () {
                                    $scope.saving = false;
                                    $scope.addNewMenuFormModal.hide();
                                });
                            else {

                                toastr.success($filter('translate')('Menu.UpdateSucces'));
                                $scope.saving = false;
                                $scope.addNewMenuFormModal.hide();
                                $scope.changePage($scope.activePage);
                            }
                        }).catch(function () {
                            $scope.saving = false;
                            $scope.addNewMenuFormModal.hide();
                        });
                    } else if (menuUpdate) {
                        resultPromise.then(function onSuccess() {
                            toastr.success($filter('translate')('Menu.UpdateSucces'));
                            $scope.saving = false;
                            $scope.addNewMenuFormModal.hide();
                            $scope.changePage($scope.activePage);
                        });
                    } else {
                        toastr.success($filter('translate')('Menu.UpdateSucces'));
                        $scope.addNewMenuFormModal.hide();
                        $scope.saving = false;
                        $scope.changePage($scope.activePage);
                    }
                }
                //If first create
                else {
                    menu = [
                        {
                            profile_id: $scope.menu.default ? 1 : $scope.menu.profile.id,
                            name: $scope.menu.name,
                            default: $scope.menu.default ? $scope.menu.default : false,
                            description: $scope.menu.description,
                        }];

                    MenusService.create(menu).then(function () {
                        if ($scope.createArray.length > 0) {
                            MenusService.createMenuItems($scope.createArray, menu[0].profile_id).then(function onSuccess() {
                                toastr.success($filter('translate')('Menu.MenuSaving'));
                                $scope.addNewMenuFormModal.hide();
                                $scope.grid.dataSource.read();
                                $scope.saving = false;
                            }).catch(function () {
                                $scope.saving = false;
                                $scope.addNewMenuFormModal.hide();
                            });
                        } else {
                            toastr.success($filter('translate')('Menu.MenuSaving'));
                            $scope.addNewMenuFormModal.hide();
                            $scope.changePage($scope.activePage);
                            $scope.pageTotal += 1;
                            $scope.saving = false;
                        }
                    });
                }
            };

            $scope.edit = function (node) {
                node.isEdit = true;
                /*Close edit mode'ta inputa müdahale edilemediğinden dolayı bu değişkene ihtiyaç var*/
                $scope.copyNodeName = node.name;
            };

            $scope.update = function (node) {
                node.isEdit = false;
                node.icon = angular.isObject(node.icon) ? node.icon.value : node.icon;
                isUpdate = true;
            };

            $scope.editModeClose = function (node) {
                node.isEdit = false;
                node.name = $scope.copyNodeName;
            };

            $scope.disable = function (node, parentList, parent, index) {


                $scope.copyData = angular.copy($scope.data);
                index = parent ? parentList.indexOf(node) : index;

                if (index > -1) {

                    /*Gelen ana kırılımdan ise parent yoktur*/
                    if (!parent) {
                        var count = 0;
                        //alt kırlım varsa, diğerlerininde iconunu disable'e çek
                        for (var k = 0; k < $scope.copyData[index].nodes.length; k++) {

                            node.nodes[k].no = k + 1;
                            node.nodes[k].disabled = node.disabled ? false : true;
                            // node.nodes[k].newIcon = node.nodes[k].disabled ? "fa fa-eye-slash" : 'fa fa-eye';
                        }
                        $scope.copyData[index] = node;
                    }

                    /*parent varsa alt kırılımdır
                   * Ana kırılım disable, alt kırılımda disabledır 
                   * User bunu visable'a çekecekse artık bunu ana kırılım disable olduğu için bunu ana kırılımdan çıkarıyoruz.*/
                    else {

                        if (parent.disabled && node.disabled) {
                            /*ana arrayin length'i +1 nosunu verir*/
                            node.menuId = $scope.data.length + 1;
                            node.no = node.menuId;
                            node.parentId = 0;
                            parentList.splice(index, 1);
                            $scope.copyData.push(node);
                            $scope.data.push(node);
                        }

                    }
                    //disable butonuna her tıklandığında her koşulda icon ve disable olma özelliği kontrol edilmeli
                    node.disabled = node.disabled ? false : true;
                    //  node.newIcon = node.disabled ? "fa fa-eye-slash" : 'fa fa-eye';
                }

                isUpdate = true;
            };

            //Menu Delete
            $scope.delete = function (id, event) {
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
                            var elem = angular.element(event.srcElement);
                            angular.element(elem.closest('tr')).addClass('animated-background');

                            MenusService.delete(id)
                                .then(function () {
                                    angular.element(document.getElementsByClassName('ng-scope animated-background')).remove();
                                    $scope.grid.dataSource.read();
                                    toastr.success($filter('translate')('Menu.DeleteSuccess'));
                                    $scope.saving = false;
                                })
                                .catch(function () {
                                    $scope.menuList = $scope.menuListState;
                                    angular.element(document.getElementsByClassName('ng-scope animated-background')).removeClass('animated-background');
                                    toastr.warning($filter('translate')('Common.Error'));
                                    if ($scope.addNewMenuFormModal) {
                                        $scope.addNewMenuFormModal.hide();
                                        $scope.saving = false;
                                    }
                                });
                        }
                    });
            };

            function isMenuDirty(menuForm) {
                var res = false;
                //Update Menu
                var updateList = {
                    name: $scope.menu.name,
                    description: $scope.description ? $scope.menu.description : '',
                    default: $scope.menu.default ? $scope.menu.default : false,
                    profile_id: $scope.menu.default ? 1 : $scope.menu.profile.id
                };

                if (menuForm.name_menu.$dirty) {
                    updateList.name = $scope.menu.name;
                    res = true;
                }

                if (menuForm.profile_name.$dirty && !$scope.menu.default) {
                    updateList.profile_id = $scope.menu.profile.id;
                    res = true;
                }

                if (menuForm.default.$dirty) {
                    updateList.default = $scope.menu.default;
                    res = true;
                }

                if (menuForm.menu_description.$dirty) {
                    updateList.description = $scope.menu.description;
                    res = true;
                }

                $scope.updateArray.push(updateList);

                return res;
            }

            $scope.treeOptions = {
                accept: function (sourceNodeScope, destNodesScope, destIndex) {

                    //modulü yer değiştirirken
                    if (!destNodesScope.$parent.$modelValue && sourceNodeScope.$modelValue.menuName) {
                        // isUpdate = true;
                        return true;
                    }
                    //kategoriyi yer değiştirirken
                    else if (!destNodesScope.$parent.$modelValue && !sourceNodeScope.$modelValue.menuName) {
                        // isUpdate = true;
                        return true;
                    }
                    //gideceği yer module değilse ve giden kategori değilse 
                    else if (destNodesScope.$parent.$modelValue && !destNodesScope.$parent.$modelValue.menuName && sourceNodeScope.$modelValue.menuName) {
                        return true;
                    }
                    return false;
                },

                dropped: function (e) {
                    var parent = e.dest.nodesScope.$parent.$modelValue;
                    var child = e.source.nodeScope.$modelValue;
                    isUpdate = true;
                    //eğer modülün gideceği kategori disabled ise module disabled olmalı
                    if (parent)
                        child.disabled = parent.disabled;
                    //Eğer module parent' altından çıkarılmışsa parentID olmayacak
                    child.parentId = parent ? parent.id : 0;
                    // child.newIcon  = parent.newIcon;
                }
            };

            $scope.deleteItem = function (node, parentList, index) {

                if (node.id > 0) {
                    $scope.deleteArray.push(node.id);
                }
                /*id:0 ise daha eklenmemiştir direkt arrayden siliyoruz*/
                // else {
                //eğer node'ları var ise array'e pushluyoruz, sadece var olan kategoriyi array'den kaldırıyoruz

                for (var i = 0; i < node.nodes.length; i++) {

                    node.nodes[i].menuId = $scope.data.length; //+1 eklememe sebebimiz, en altta ana kırılım array'den çıkarılacağı için eş değer oluyor
                    node.nodes[i].no = node.nodes[i].menuId;
                    node.nodes[i].parentId = 0;
                    parentList.push(node.nodes[i]);
                }
                parentList.splice(index, 1);
            };

            $scope.goUrl = function (menu) {
                var selection = window.getSelection();
                if (selection.toString().length === 0) {
                    $scope.showFormModal(menu.id);
                }
            };

            //For Kendo UI
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
                            url: "/api/menu/find",
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
                                Name: { type: "string" },
                                ProfileName: { type: "string" },
                                Default: { type: "string" }
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
                rowTemplate: function (menu) {
                    var trTemp = '<tr ng-click="goUrl(dataItem)">';
                    trTemp += '<td>' + menu.name + '</td>';
                    trTemp += '<td>' + menu.profile.name_en + '</td>';
                    trTemp += menu.default ? '<td><span>' + $filter('translate')('Common.Yes') + '</span></td>' : '<td><span>' + $filter('translate')('Common.No') + '</span></td>';
                    trTemp += '<td ng-click="$event.stopPropagation();"> <button ng-click="$event.stopPropagation(); delete(dataItem.id, $event);" type="button" class="action-button2-delete"><i class="fas fa-trash"></i></button></td></tr>';
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
                        field: 'Name',
                        title: $filter('translate')('Menu.MenuName'),
                    },

                    {
                        field: 'ProfileId',
                        title: $filter('translate')('Menu.ProfileName'),
                    },
                    {
                        field: 'Default',
                        title: $filter('translate')('Menu.DefaultMenu'),
                    },
                    {
                        field: '',
                        title: '',
                        width: "90px"
                    }]
            };

        }
    ]);