'use strict';
angular.module('primeapps')
    .controller('ExpenseController', ['$rootScope', '$scope', 'moment', '$modal', '$filter', '$location', 'ModuleService', 'config', '$http', '$state', 'helper', 'ngToast',
        function ($rootScope, $scope, moment, $modal, $filter, $location, ModuleService, config, $http, $state, helper, ngToast) {
            var that = $scope;
            $scope.module = $filter('filter')($rootScope.modules, { name: 'masraflar' }, true)[0];
            $scope.masrafItemModule = $filter('filter')($rootScope.modules, { name: 'masraf_kalemleri' }, true)[0];
            $scope.relationModule = $filter('filter')($scope.module.relations, { related_module: 'masraf_kalemleri' }, true)[0];
            $scope.owner = $filter('filter')($rootScope.users, { Id: ($location.search().user ? parseInt($location.search().user) : $rootScope.user.ID) }, true)[0];
            $scope.hasAdminRights = angular.copy($rootScope.user.profile.has_admin_rights);
            $scope.currentMonth = moment().month() + 1;
            $scope.spinnerShow = false;
            $scope.settings = {};
            $scope.settingsCheck = {};
            $scope.settingsCurrentYear = moment().year();
            $scope.yearsPicklist = {};
            $scope.hasManuelProcess = false;
            $scope.waitingForApproval = false;
            $scope.manuelApproveRequest = false;
            $scope.isApproved = false;
            $scope.currentUser = ModuleService.processUser($rootScope.user);
            $scope.masrafKalemleri = $filter('filter')($rootScope.modules, { name: 'masraf_kalemleri' }, true)[0];
            $scope.lookupTypes = $filter('filter')($scope.masrafKalemleri.fields, { data_type: 'lookup' }, true);
            $scope.ExpenseItemFreeze = true;
            $scope.sendApproveShow = false;
            $scope.Approved = true;
            $scope.runProcess = true;
            $scope.totalAmountShow = false;
            $scope.urlId = $location.search().id;
            $scope.ApproveFreeze = true;
            $scope.showFullName = false;
            $scope.payableTotalAmountShow = false;
            $scope.AnyExpenseItemShow = true;
            $scope.ExpenseItemFreezeYear = true;
            $scope.automaticApprovelButtonShow = true;

            //Masraf kalemlerinin sheet deki kolon isimleri burada setleniyor.
            $scope.labels = [];
            for (var i = 0; i < $scope.relationModule.display_fields_array.length; i++) {
                var relationItem = $scope.relationModule.display_fields_array[i];
                var field = $filter('filter')($scope.masrafItemModule.fields, { name: relationItem }, true)[0];
                if (field)
                    $scope.labels.push(field);
            }

            $scope.lookupTypeForLabel = $filter('filter')($scope.labels, { data_type: 'lookup' }, true);


            //Seçilen aya göre masrafın findrequest ile masraf kalemleri çekiliyor.
            $scope.getExpenseItem = function () {
                if ($scope.currentExpense) {
                    var requestMasrafItems = {};
                    requestMasrafItems.fields = [];
                    angular.forEach($scope.masrafItemModule.fields, function (field) {
                        if (!field.deleted)
                            requestMasrafItems.fields.push(field.name)
                    });

                    angular.forEach($scope.lookupTypes, function (field) {
                        angular.forEach($scope.lookupTypeForLabel, function (fieldLabel) {
                            if (field.name === fieldLabel.name)
                                requestMasrafItems.fields.push(fieldLabel.name + '.' + fieldLabel.lookup_type + '.' + fieldLabel.lookupModulePrimaryField.name)
                        });
                    });
                    //Mailden ilgili masrafa gelen veya Masraflar Modülünün detayından url de id setlenen şekilde gelenler için url deki id ye göre
                    //Masraf ve masraf kalemleri çekiliyor.
                    if ($scope.urlId) {
                        requestMasrafItems.filters = [{ field: 'owner', operator: 'equals', value: $scope.currentExpense.owner, no: 1 }, { field: 'masraf', operator: 'equals', value: $scope.currentExpense.id, no: 2 }];

                    }
                    else {
                        requestMasrafItems.filters = [{ field: 'owner', operator: 'equals', value: $scope.owner.id, no: 1 }, { field: 'masraf', operator: 'equals', value: $scope.currentExpense.id, no: 2 }];
                    }
                    requestMasrafItems.sort_field = 'created_at';
                    requestMasrafItems.sort_direction = 'desc';
                    requestMasrafItems.limit = 2000;
                    //Masrafın masraf kalemleri çekiliyor.
                    ModuleService.findRecords('masraf_kalemleri', requestMasrafItems)
                        .then(function (response) {
                            if (response.data.length > 0) {
                                $scope.expense_items = $filter('orderBy')(response.data, '-faturafis_tarihi');
                                $scope.AnyExpenseItemShow = false;
                                $scope.sendApproveShow = true;
                                //Masraf kalemleri sheet de ilgili kısma basılıyor.
                                for (var i = 0; i < response.data.length; i++) {
                                    var expenseItem = response.data[i];
                                    angular.forEach($scope.lookupTypeForLabel, function (fieldLabel) {
                                        if (expenseItem[fieldLabel.name] && expenseItem[fieldLabel.name] !== null)
                                            expenseItem[fieldLabel.name] = expenseItem[fieldLabel.name + '.' + fieldLabel.lookup_type + '.' + fieldLabel.lookupModulePrimaryField.name];
                                    });
                                }

                                var request = {};
                                request.fields = [];
                                angular.forEach($scope.module.fields, function (field) {
                                    if (!field.deleted)
                                        request.fields.push(field.name)
                                });
                                //Mailden ilgili masrafa gelen veya Masraflar Modülünün detayından url de id setlenen şekilde gelenler için url deki id ye göre
                                //işlem yapılıyor.
                                if ($scope.urlId) {
                                    request.filters = [
                                        { field: 'id', operator: 'equals', value: $scope.urlId, no: 1 },
                                    ];
                                }
                                else {
                                    request.filters = [
                                        { field: 'masraf_donemi_yil', operator: 'equals', value: $scope.currentYear, no: 1 },
                                        { field: 'masraf_donemi_ay', operator: 'equals', value: $scope.selectCurrentMonth.label_tr, no: 2 },
                                        { field: 'owner', operator: 'equals', value: $scope.owner.id, no: 3 }
                                    ];
                                }

                                request.limit = 1;
                                //Sheetde ki Toplam Tutar alanındaki tutarların toplanabilmesi için
                                //Masraflar çekiliyor ve toplam tutar alanı masraflardan alınıyor.
                                ModuleService.findRecords('masraflar', request)
                                    .then(function (response) {
                                        var data = response.data[0];
                                        if (data && $scope.selectCurrentMonth.labelStr == data.masraf_donemi_ay && $scope.selectCurrentYear.labelStr == data.masraf_donemi_yil && data.toplam_tutar > 0) {
                                            $scope.totalAmount = data.toplam_tutar;
                                            $scope.totalAmountShow = true;
                                        }
                                        if (data && $scope.selectCurrentMonth.labelStr == data.masraf_donemi_ay && $scope.selectCurrentYear.labelStr == data.masraf_donemi_yil && data.odenecek_toplam_tutar && data.odenecek_toplam_tutar > 0) {
                                            $scope.payableTotalAmount = data.odenecek_toplam_tutar;
                                            $scope.payableTotalAmountShow = true;
                                        }
                                        //Mailden ilgili masrafa gelen veya Masraflar Modülünün detayından url de id setlenen şekilde gelenler için url deki id ye göre
                                        //işlem yapılıyor.
                                        if ($scope.urlId) {
                                            var setSelectMonth = $filter('filter')($scope.monthList, { labelStr: data.masraf_donemi_ay })[0];
                                            $scope.filter = { selectMonth: setSelectMonth };
                                            if ($rootScope.language === "tr")
                                                $scope.currentMonthSet = $scope.filter.selectMonth.label_tr;
                                            else
                                                $scope.currentMonthSet = $scope.filter.selectMonth.label_en;

                                            //Url de id olduğunda masraf freeze oluyor.
                                            $scope.ApproveFreeze = false;
                                            $scope.showFullName = true;
                                            if ($scope.currentExpense.process_status == 3 && $scope.currentExpense.created_by === $scope.currentUser.id) {
                                                $scope.ExpenseItemFreeze = true;
                                            } else {
                                                $scope.ExpenseItemFreeze = false;
                                            }
                                            if (data && data.toplam_tutar && data.toplam_tutar > 0) {
                                                $scope.totalAmount = data.toplam_tutar;
                                                $scope.totalAmountShow = true;
                                            }
                                            if (data && data.odenecek_toplam_tutar && data.odenecek_toplam_tutar > 0) {
                                                $scope.payableTotalAmount = data.odenecek_toplam_tutar;
                                                $scope.payableTotalAmountShow = true;
                                            }
                                        }
                                        //Toplam tutar backende hesaplandığı için sonuç hesaplamadan önce dönüyordu çözümü için yapıldı.
                                        setTimeout(function () {
                                            ModuleService.findRecords('masraflar', request)
                                                .then(function (response) {
                                                    var data = response.data[0];
                                                    if (data && $scope.selectCurrentMonth.labelStr == data.masraf_donemi_ay && $scope.selectCurrentYear.labelStr == data.masraf_donemi_yil && data.toplam_tutar > 0) {
                                                        $scope.totalAmount = data.toplam_tutar;
                                                        $scope.totalAmountShow = true;
                                                    }
                                                    if (data && $scope.selectCurrentMonth.labelStr == data.masraf_donemi_ay && $scope.selectCurrentYear.labelStr == data.masraf_donemi_yil && data.odenecek_toplam_tutar && data.odenecek_toplam_tutar > 0) {
                                                        $scope.payableTotalAmount = data.odenecek_toplam_tutar;
                                                        $scope.payableTotalAmountShow = true;
                                                    }
                                                    $scope.spinnerShow = false;
                                                });
                                        }, 2000);
                                    });
                            }
                            else {
                                $scope.expense_items = null;
                                $scope.sendApproveShow = false;
                                $scope.totalAmountShow = false;
                                $scope.payableTotalAmountShow = false;
                                if ($scope.currentExpense.masraf_donemi_ay === $scope.selectCurrentMonth.labelStr)
                                    $scope.AnyExpenseItemShow = true;
                            }
                        });
                }
                else {
                    $scope.totalAmountShow = false;
                    $scope.payableTotalAmountShow = false;
                }
            };

            //Masraflara ait picklistler burada çekiliyor Aylar ve Yıllar picklistleri
            //Sayfa açılır açılmaz yapılmak istenen işlemler bu kısımda yapılıyor.
            ModuleService.getPicklists($scope.module)
                .then(function (picklists) {
                    //Picklistlerden Aylar Picklisti Çekiliyor.
                    $scope.monthList = picklists[11];
                    //Picklistlerden Yıllar Picklisti Çekiliyor.
                    var picklistField = $filter('filter')($scope.module.fields, { name: 'masraf_donemi_yil' })[0];
                    $scope.yearList = picklists[picklistField.picklist_id];
                    //Aylar Picklistine bulunduğumuz ay setleniyor.
                    $scope.setMonth = $filter('filter')($scope.monthList, { value: $scope.currentMonth })[0];
                    $scope.yearsPicklist.selectYear = $filter('filter')($scope.yearList, { labelStr: $scope.settingsCurrentYear.toString() })[0];
                    $scope.filter = { selectMonth: $scope.setMonth };

                    if ($rootScope.language === "tr")
                        $scope.currentMonthSet = $scope.filter.selectMonth.label_tr;
                    else
                        $scope.currentMonthSet = $scope.filter.selectMonth.label_en;

                    //Aylara girilen Masraf Ayarları çekiliyor.
                    $scope.getExpenseSettings = function () {
                        $http.get(config.apiUrl + 'settings/get_by_key/5/expense_settings').then(function (response) {
                            if (response.data) {
                                $scope.expenseSettingId = response.data.id;
                                var responseMonth = $filter('filter')($scope.monthList, { value: $scope.filter.selectMonth.value })[0];
                                $scope.relatedSetting = angular.fromJson(response.data.value);
                                $scope.settings = $filter('filter')($scope.relatedSetting, { systemCode: responseMonth.value }, true)[0];
                                var currentYearSettings = $filter('filter')($scope.yearList, { labelStr: $scope.settingsCurrentYear.toString() }, true)[0];
                                $scope.settings.expenseYear = currentYearSettings;
                                $scope.settings.expenseMonth = $scope.filter.selectMonth;
                                $scope.settingsCheck.expenseLastYearLastMonth = $scope.relatedSetting[12].LastYearLastMonth;
                                $scope.settingsCheck.automaticApprovel = $scope.relatedSetting[13].AutomaticApprovel;
                                $scope.expenseEntryEndDateControl();

                                //Ayarlardaki Geçen Yılın Son Ayı Kontrolü Yapılıyor.
                                if ($scope.settingsCurrentYear != parseInt($scope.yearsPicklist.selectYear.labelStr))
                                    $scope.ExpenseItemFreezeYear = false;
                                else
                                    $scope.ExpenseItemFreezeYear = true;

                                var lastYearControl = $scope.relatedSetting[12].LastYearLastMonth;
                                var lastYear = $scope.settingsCurrentYear - 1;
                                var lastMonth = parseInt($scope.filter.selectMonth.value);
                                var selectYear = parseInt($scope.yearsPicklist.selectYear.labelStr);
                                if (lastYearControl && lastYear == selectYear && lastMonth === 12)
                                    $scope.ExpenseItemFreezeYear = true;

                                var automaticApprovelControl = $scope.relatedSetting[13].AutomaticApprovel;
                                if (automaticApprovelControl)
                                    $scope.automaticApprovelButtonShow = false;
                                else
                                    $scope.automaticApprovelButtonShow = true;
                            }
                        });
                    };
                    //Method Çalıştırılıyor.
                    $scope.getExpenseSettings();
                    //Sayfadaki işlemlerin gerçekleştirilmesi için method çalıştırılıyor.
                    $scope.monthListChange();
                });

            //Ayarladaki Masraf Dönemi Bitiş Tarihi kontrolü yapılıyor
            $scope.expenseEntryEndDateControl = function () {
                if (Object.keys($scope.settings).length > 0) {
                    var nowYear = $scope.currentYear.toString();
                    var nowMonth = $scope.filter.selectMonth;
                    var expenseEntryEndDate = moment($scope.settings.expenseEntryEndDate).format("YYYY-MM-DD");
                    var formatDate = $filter('date')(new Date(), 'yyyy-MM-dd');
                    var currentDate = moment(formatDate).format("YYYY-MM-DD");
                    if ($scope.settings.expenseMonth.value === nowMonth.value && $scope.settings.expenseYear.labelStr === nowYear && expenseEntryEndDate < currentDate)
                        $scope.ExpenseItemFreeze = false;
                    else {
                        if (!$scope.urlId)
                            $scope.ExpenseItemFreeze = true;
                    }
                    //Ayarlardan Masraf Dönemi Bitiş Tarihi ileri bir tarih alındığında Masraf Onaylandıysa veya diğer onay süreçleri reddedildi, onay bekliyor
                    //Gibi süreçlerde masraf kalemi girişi açılıyor kayıt freeze den çıkıyordu.
                    if ($scope.currentExpense && $scope.currentExpense.masraf_donemi_ay === nowMonth.labelStr && $scope.currentExpense.masraf_donemi_yil === nowYear) {
                        if ($scope.currentExpense.process_status === 1 || $scope.currentExpense.process_status === 2)
                            $scope.ExpenseItemFreeze = false;
                    }
                }
            };

            //Ayarlardaki Yönetici Onay Tarihi kontrolü yapılıyor
            $scope.expenseApprovedControl = function () {
                if (Object.keys($scope.settings).length > 0) {
                    var nowYear = $scope.currentYear.toString();
                    var nowMonth = $scope.filter.selectMonth;
                    var expenseProcessEndDate = moment($scope.settings.expenseProcessDate).format("YYYY-MM-DD");
                    var formatDate = $filter('date')(new Date(), 'yyyy-MM-dd');
                    var currentDate = moment(formatDate).format("YYYY-MM-DD");
                    if ($scope.settings.expenseMonth.value === nowMonth.value && $scope.settings.expenseYear.labelStr === nowYear && expenseProcessEndDate < currentDate) {
                        ngToast.create({ content: $filter('translate')('Expenses.ExpenseApprovalExpired'), className: 'warning' });
                        $scope.runProcess = false;
                    }
                    else {
                        $scope.runProcess = true;
                    }
                }
            };

            //Aylar Picklistindeki değişen aylara göre işlemler yapılıyor.
            $scope.monthListChange = function () {
                $scope.waitingForApproval = false;
                $scope.sendApproveShow = false;
                $scope.isApproved = false;
                if ($rootScope.language === "tr")
                    $scope.currentMonthSet = $scope.filter.selectMonth.label_tr;
                else
                    $scope.currentMonthSet = $scope.filter.selectMonth.label_en;
                $scope.selectCurrentYear = $filter('filter')($scope.yearList, { labelStr: $scope.yearsPicklist.selectYear.labelStr })[0];
                $scope.selectCurrentMonth = $filter('filter')($scope.monthList, { value: $scope.filter.selectMonth.value })[0];
                $scope.currentYear = parseInt($scope.selectCurrentYear.labelStr);

                //Değişen aya göre Masraf Döneminin Ayarları çekiliyor.
                $scope.getExpenseSettings();
                //Değişen aya göre Masraf Dönemi Bitiş Tarihi ayarı kontrol ediliyor.
                $scope.expenseEntryEndDateControl();

                //Seçilen ayın masrafı veritabanına kayıtlı mı onun kontrolü için gerekli request oluşturuluyor.
                var request = {};
                request.fields = [];
                angular.forEach($scope.module.fields, function (field) {
                    if (!field.deleted)
                        request.fields.push(field.name)
                });
                //Mailden ilgili masrafa gelen veya Masraflar Modülünün detayından url de id setlenen şekilde gelenler için url deki id ye göre
                //işlem yapılıyor.
                if ($scope.urlId) {
                    request.filters = [
                        { field: 'id', operator: 'equals', value: $scope.urlId, no: 3 }
                    ];
                }
                else {
                    request.filters = [
                        { field: 'masraf_donemi_yil', operator: 'equals', value: $scope.currentYear, no: 1 },
                        { field: 'masraf_donemi_ay', operator: 'equals', value: $scope.selectCurrentMonth.label_tr, no: 2 },
                        { field: 'owner', operator: 'equals', value: $scope.owner.id, no: 3 }
                    ];
                }
                request.limit = 1;
                //Seçilen ayın masraf kontrolü yapılıyor.
                ModuleService.findRecords('masraflar', request)
                    .then(function (response) {
                        if (response.data.length < 1) {
                            var calisanEmail = $scope.owner.email;
                            var calisanRequest = {};
                            calisanRequest.fields = ['id'];
                            calisanRequest.filters = [
                                { field: 'e_posta', operator: 'is', value: calisanEmail, no: 1 }
                            ];
                            calisanRequest.limit = 1;
                            //Seçilen ayın masrafı yoksa masraf girmek isteyen çalışanın kontrolü yapılıyor eğer çalışan yoksa
                            //Dashboarda yönlendiriliyor eğer çalışan varsa masrafı önceden oluşmamışsa arkaplanda masraf oluşturuluyor.
                            ModuleService.findRecords('calisanlar', calisanRequest)
                                .then(function (responseCalisan) {
                                    if (responseCalisan.data.length > 0) {
                                        var masraflarRequest = {};
                                        masraflarRequest.masraf_donemi_yil = $scope.selectCurrentYear.id;
                                        masraflarRequest.masraf_donemi_ay = $scope.selectCurrentMonth.id;
                                        masraflarRequest.calisan = responseCalisan.data[0].id;
                                        masraflarRequest.owner = $scope.owner.id;
                                        masraflarRequest.toplam_tutar = 0;
                                        ModuleService.insertRecord('masraflar', masraflarRequest)
                                            .then(function (responseExpense) {
                                                $scope.currentExpense = responseExpense.data;
                                                $scope.totalAmountShow = false;
                                                $scope.payableTotalAmountShow = false;
                                                $scope.expense_items = null;
                                                $scope.getExpenseItem();
                                            });
                                    }
                                    else {
                                        ngToast.create({ content: $filter('translate')('Common.TimetrackerNotFound'), className: 'warning' });
                                        $state.go('app.dashboard');
                                    }
                                });
                        } else {
                            $scope.currentExpense = response.data[0];
                            if ($scope.currentExpense && $scope.selectCurrentMonth.labelStr == $scope.currentExpense.masraf_donemi_ay && $scope.selectCurrentYear.labelStr == $scope.currentExpense.masraf_donemi_yil && $scope.currentExpense.toplam_tutar > 0) {
                                $scope.totalAmount = response.data[0].toplam_tutar;
                                $scope.totalAmountShow = true;
                            }
                            if ($scope.currentExpense && $scope.selectCurrentMonth.labelStr == $scope.currentExpense.masraf_donemi_ay && $scope.selectCurrentYear.labelStr == $scope.currentExpense.masraf_donemi_yil && response.data[0].odenecek_toplam_tutar && response.data[0].odenecek_toplam_tutar > 0) {
                                $scope.payableTotalAmount = response.data[0].odenecek_toplam_tutar;
                                $scope.payableTotalAmountShow = true;
                            }
                            $scope.getExpenseItem();
                            //Onay kontrolleri için İlgili Methoda gidiliyor.
                            $scope.getItems();
                        }
                    });
            };

            $scope.yearListChange = function () {
                $scope.monthListChange();
            };

            //Onay sürecinin ilgili işlemleri yapılıyor.
            $scope.getItems = function () {
                if ($scope.currentExpense) {
                    $scope.currentExpense.process_id = $scope.currentExpense['process.process_requests.process_id'];
                    $scope.currentExpense.process_status = $scope.currentExpense['process.process_requests.process_status'];
                    $scope.currentExpense.process_status_order = $scope.currentExpense['process.process_requests.process_status_order'];
                    $scope.currentExpense.operation_type = $scope.currentExpense['process.process_requests.operation_type'];

                    //Masraf onaylandıysa veya reddedildiyse kayıt freeze ediliyor.
                    if ($scope.currentExpense.process_status && $scope.currentExpense.process_status === 1 || $scope.currentExpense.process_status === 2)
                        $scope.ExpenseItemFreeze = false;

                    for (var i = 0; i < $rootScope.approvalProcesses.length; i++) {
                        var currentProcess = $rootScope.approvalProcesses[i];
                        if (currentProcess.module_id === $scope.module.id && currentProcess.trigger_time === 'manuel')
                            $scope.hasManuelProcess = true;
                    }
                    //approval process
                    if ($scope.currentExpense.process_status) {
                        if ($scope.currentExpense.process_status === 2)
                            $scope.isApproved = true;

                        if ($scope.currentExpense.process_status === 1 || $scope.currentExpense.process_status === 2 || ($scope.currentExpense.updated_by !== $scope.currentUser.id))
                            $scope.freeze = true;

                        ModuleService.getProcess($scope.currentExpense.process_id)
                            .then(function (response) {
                                var customApprover = $scope.currentExpense.custom_approver;
                                if (customApprover === $rootScope.user.email) {
                                    $scope.isApprovalRecord = true;
                                    $scope.waitingForApproval = false;
                                }
                                else if (customApprover !== $rootScope.user.email && $scope.currentExpense.process_status === 1) {
                                    $scope.waitingForApproval = true;
                                    $scope.isApproved = false;
                                    $scope.isApprovalRecord = false;
                                }

                                if ($scope.currentExpense.operation_type === 0 && $scope.currentExpense.process_status === 2) {
                                    for (var i = 0; i < response.data.operations_array.length; i++) {
                                        var process = response.data.operations_array[i];

                                        if (process === "update")
                                            $scope.freeze = false;
                                    }
                                }

                                if ($scope.currentExpense.operation_type === 1 && $scope.currentExpense.process_status === 2) {
                                    $scope.freeze = false;
                                }
                            });
                    }
                }
                else {
                    $scope.isApproved = false;
                    $scope.freeze = false;
                }
            };

            $scope.openCreateModal = function () {
                $scope.content = [];
                $scope.content.push({ text: $rootScope.language === 'tr' ? 'Masraf' : 'Event', href: '', click: 'openCreateModal(null, null, "masraflar")' });
                var module = 'masraf_kalemleri';
                $scope.currentLookupField = { lookup_type: module };

                $scope.formModal = $scope.formModal || $modal({
                        scope: $scope,
                        templateUrl: 'views/app/module/moduleFormModal.html',
                        animation: '',
                        backdrop: 'static',
                        show: false,
                        tag: 'createModal'
                    });

                $scope.formModal.$promise.then($scope.formModal.show);
            };

            //ModuleFormController'ı kullanan yeni Modal yapısı yapıldı.
            $scope.openExpenseModal = function (item) {
                $scope.type = 'masraf_kalemleri';
                if (item)
                    $scope.id = item.id;
                else
                    $scope.id = null;

                $scope.formType = 'modal';
                $scope.item = item;
                $scope.editformModal = $scope.editformModal || $modal({
                        scope: $scope,
                        resolve: {
                            plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                                return $ocLazyLoad.load([
                                    cdnUrl + 'view/app/module/moduleFormModalController.js',
                                    cdnUrl + 'view/app/product/quoteProductsController.js',
                                    cdnUrl + 'view/app/module/moduleFormController.js',
                                    cdnUrl + 'view/app/product/quoteProductsService.js',
                                    cdnUrl + 'view/app/product/orderProductsController.js',
                                    cdnUrl + 'view/app/product/orderProductsService.js',
                                    cdnUrl + 'view/app/product/purchaseProductsController.js',
                                    cdnUrl + 'view/app/product/purchaseProductsService.js',
                                    cdnUrl + 'view/app/actionbutton/actionButtonFrameController.js',
                                    cdnUrl + 'view/app/product/salesInvoiceProductsController.js',
                                    cdnUrl + 'view/app/product/salesInvoiceProductsService.js',
                                    cdnUrl + 'view/app/product/purchaseInvoiceProductsController.js',
                                    cdnUrl + 'view/app/product/purchaseInvoiceProductsService.js'
                                ]);
                            }]
                        },
                        templateUrl: 'view/app/module/moduleForm.html',
                        backdrop: 'static',
                        show: false,
                        tag: 'editModal',
                        container: 'body',
                        controller: 'ModuleFormController'
                    });

                $scope.editformModal.$promise.then($scope.editformModal.show);
            };

            //ModuleFormController'ı kullanan yeni Modal yapısı yapıldı.
            $scope.openExpenseDetailModal = function (item) {
                $scope.type = 'masraf_kalemleri';
                $scope.freeze = true;
                if (item)
                    $scope.id = item.id;
                else
                    $scope.id = null;

                $scope.formType = 'modal';
                $scope.item = item;
                $scope.editformDetailModal = $scope.editformDetailModal || $modal({
                        scope: $scope,
                        resolve: {
                            plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                                return $ocLazyLoad.load([
                                    cdnUrl + 'view/app/module/moduleDetailController.js',
                                    cdnUrl + 'view/pp/product/quoteProductsController.js',
                                    cdnUrl + 'view/app/product/quoteProductsService.js',
                                    cdnUrl + 'view/app/product/orderProductsController.js',
                                    cdnUrl + 'view/app/product/orderProductsService.js',
                                    cdnUrl + 'view/app/product/purchaseProductsController.js',
                                    cdnUrl + 'view/app/product/purchaseProductsService.js',
                                    cdnUrl + 'view/app/actionbutton/actionButtonFrameController.js',
                                    cdnUrl + 'view/app/product/salesInvoiceProductsController.js',
                                    cdnUrl + 'view/app/product/salesInvoiceProductsService.js',
                                    cdnUrl + 'view/app/product/purchaseInvoiceProductsController.js',
                                    cdnUrl + 'view/app/product/purchaseInvoiceProductsService.js'
                                ]);
                            }]
                        },
                        templateUrl: 'view/app/module/moduleDetail.html',
                        backdrop: 'static',
                        show: false,
                        tag: 'editModal',
                        container: 'body',
                        controller: 'ModuleDetailController'
                    });
                $scope.editformDetailModal.$promise.then($scope.editformDetailModal.show);
            };

            //Masraf kalemi güncellenmesinden sonra çalışan method.
            $scope.afterSave = function () {
                that.editformModal.hide();
                $scope.spinnerShow = true;
                //Güncellendikten sonra masraf kaleminin yeni hali gelmesi için method çağırılıyor.
                $scope.getExpenseItem();
            };

            $scope.beforeSave = function (record) {
                var dateParts = moment(record['faturafis_tarihi']).format().split('+');
                var faturaTarih = dateParts[0];

                var faturaTarihSplit = faturaTarih.split('-');
                var aySplit = faturaTarihSplit[1].split('0');

                var faturaYil = faturaTarihSplit[0];
                var faturaAy = aySplit[1];
                if (!faturaAy)
                    faturaAy = aySplit[0];

                var donemAy = $scope.filter.selectMonth.value;
                var donemYil = $scope.currentExpense['masraf_donemi_yil'];

                if (faturaAy == donemAy && faturaYil == donemYil)
                    $scope.executeCode = false;
                else {
                    $scope.executeCode = true;
                    ngToast.create({ content: $filter('translate')('Lütfen masraf dönemine ait masraf kalemi giriniz.'), className: 'warning' });
                }
            };

            $scope.expenseSettingsModal = function () {
                $scope.settingsFormModal = $scope.settingsFormModal || $modal({
                        scope: $scope,
                        templateUrl: 'view/app/expensesheet/expenseSettingsModal.html',
                        animation: '',
                        backdrop: 'static',
                        show: false,
                        tag: 'createModal'
                    });

                $scope.settingsFormModal.$promise.then($scope.settingsFormModal.show);
            };

            //Masraf ayının ayarları kaydedilince çalışan method.
            $scope.expenseSubmitSettings = function () {
                //İlgili ayın ayarı varsa koşula giriyor
                if ($scope.settings) {
                    //İlgili Ayın ayarlar kısmına settingsden gelen ayarlar kaydediliyor.
                    //Her ayın sayısal değerini systemCode alanında tuttum Mayıs = 5 gibi
                    //Array 0 dan başladığı için ilgili aya system code değernin - 1.indeksini setledim.
                    $scope.relatedSetting[$scope.settings.systemCode - 1] = $scope.settings;
                    $scope.relatedSetting[12].LastYearLastMonth = $scope.settingsCheck.expenseLastYearLastMonth;
                    $scope.relatedSetting[13].AutomaticApprovel = $scope.settingsCheck.automaticApprovel;

                    var settingObj = {
                        key: 'expense_settings',
                        value: angular.toJson($scope.relatedSetting),
                        type: 5
                    };

                    $http.put(config.apiUrl + 'settings/update/' + $scope.expenseSettingId, settingObj).then(function (response) {
                        $scope.settings = response.data.value;
                        ngToast.create({ content: $filter('translate')('Expenses.SettingsUpdate'), className: 'success' });
                        $scope.getExpenseSettings();
                    });
                }
                that.settingsFormModal.hide();
            };

            //Masraf kalemi eklendikten sonra masraf kalemleri çekilmesi için ilgili method çalıştırılıyor.
            $scope.formModalSuccess = function () {
                $scope.getExpenseItem();
            };

            //Masraf kalemi silinmesi için yazılan method.
            $scope.delete = function (id) {
                $scope.spinnerShow = true;
                ModuleService.deleteRecord('masraf_kalemleri', id)
                    .then(function () {
                        ngToast.create({ content: $filter('translate')('Expenses.DeleteExpenseItem'), className: 'success' });
                        $scope.getExpenseItem();
                    });
            };

            //Onay Süreci işlemleri için oluşturulan methodlar.
            //Onay ile ilgili Freeze işlemleride methodlarda yapılıyor.
            $scope.sendToProcessApproval = function () {
                $scope.manuelApproveRequest = true;
                var isValid = true;

                if (isValid) {
                    var request = {
                        "record_id": $scope.currentExpense.id,
                        "module_id": $scope.module.id
                    };

                    ModuleService.sendApprovalManuel(request)
                        .then(function () {
                            $scope.hasManuelProcess = false;
                            $scope.waitingForApproval = true;
                            $scope.freeze = true;
                            $scope.manuelApproveRequest = false;
                            $scope.currentExpense.process_status = 1;
                            $scope.currentExpense.process_status_order++;
                            //Onaya gönderildiği anda onay bekliyor aktif oluyor ve kayıt freeze ediliyor.
                            if ($scope.currentExpense.process_status === 1)
                                $scope.ExpenseItemFreeze = false;
                        }).catch(function onError() {
                        $scope.manuelApproveRequest = false;
                    });
                } else {
                    ngToast.create({ content: $filter('translate')('Common.CanNotSendToProcess', { minSaat: $scope.settings.dayMinHour }), className: 'warning' });
                    $scope.manuelApproveRequest = false;
                }
            };

            //Ayarlardaki Yönetici Onay Tarihi kontrolü yapılıyor kontrolün sonuca göre runPocess true veya false olarak geliyor.
            //Yönetici onay tarihinin sonucuna göre süreç freeze ediliyor.
            $scope.approveProcess = function () {
                if ($scope.runProcess) {
                    $scope.approving = true;
                    $scope.ExpenseItemFreeze = false;

                    ModuleService.approveProcessRequest($scope.currentExpense.operation_type, $scope.module.name, $scope.currentExpense.id)
                        .then(function () {
                            $scope.isApproved = true;
                            $scope.freeze = true;
                            $scope.approving = false;
                            $scope.currentExpense.process_status = 2;
                            $scope.waitingForApproval = true;
                        }).catch(function onError() {
                        $scope.approving = false;
                    });
                }
            };

            //
            $scope.rejectProcess = function (message) {
                if ($scope.runProcess) {
                    $scope.rejecting = true;

                    ModuleService.rejectProcessRequest($scope.currentExpense.operation_type, $scope.module.name, message, $scope.currentExpense.id)
                        .then(function () {
                            $scope.isRejectedRequest = true;
                            $scope.rejecting = false;
                            $scope.currentExpense.process_status = 3;
                            $scope.ExpenseItemFreeze = true;
                            $scope.rejectModal.hide();
                        }).catch(function onError() {
                        $scope.rejecting = false;
                    });
                }
            };

            $scope.reApproveProcess = function () {
                $scope.reapproving = true;
                var isValid = true;
                if (isValid) {
                    ModuleService.send_approval($scope.currentExpense.operation_type, $scope.module.name, $scope.currentExpense.id)
                        .then(function () {
                            $scope.waitingForApproval = true;
                            $scope.freeze = true;
                            $scope.reapproving = false;
                            $scope.currentExpense.process_status = 1;
                            $scope.currentExpense.process_status_order++;
                            //Onaya gönderildiği anda onay bekliyor aktif oluyor ve kayıt freeze ediliyor.
                            if ($scope.currentExpense.process_status === 1)
                                $scope.ExpenseItemFreeze = false;
                        }).catch(function onError() {
                        $scope.reapproving = false;
                    });
                } else {
                    ngToast.create({ content: $filter('translate')('Common.CanNotSendToProcess', { minSaat: $scope.settings.dayMinHour }), className: 'warning' });
                    $scope.reapproving = false;
                }
            };

            $scope.openRejectApprovalModal = function () {
                if ($scope.runProcess) {
                    $scope.rejectModal = $scope.rejectModal || $modal({
                            scope: $scope,
                            templateUrl: 'view/app/module/rejectProcessModal.html',
                            animation: '',
                            backdrop: 'static',
                            show: false,
                            tag: 'createModal'
                        });
                    $scope.rejectModal.$promise.then($scope.rejectModal.show);
                }
            };
        }
    ]);
