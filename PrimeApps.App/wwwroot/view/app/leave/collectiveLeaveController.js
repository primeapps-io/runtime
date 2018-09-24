'use strict';

angular.module('primeapps')
    .controller('CollectiveLeaveController', ['$rootScope', '$scope', 'ngToast', '$filter', 'helper', '$location', '$state', '$stateParams', '$q', '$window', '$localStorage', '$cache', 'config', 'ModuleService', 'TemplateService', '$timeout',
        function ($rootScope, $scope, ngToast, $filter, helper, $location, $state, $stateParams, $q, $window, $localStorage, $cache, config, ModuleService, TemplateService, $timeout) {
            $scope.loadingModal = true;
            $scope.submittingModal = false;
            $scope.type = $stateParams.type;
            $scope.module = $filter('filter')($rootScope.modules, { name: $scope.type }, true)[0];
            $scope.picklistsModule = null;
            $scope.customLeaveFields = {};
            $scope.record = {};
            $scope.record['hesaplanan_alinacak_toplam_izin'] = 0.0;
            $scope.submitted = false;
            var yoneticiEmail = null;

            ModuleService.getPicklists($scope.module)
                .then(function (picklists) {
                    $scope.picklistsModule = picklists;

                    if($scope.module && $scope.module.name === 'izinler'){
                        var toField = $filter('filter')($scope.module.fields, { name: 'to_entry_type' }, true)[0];
                        var fromField = $filter('filter')($scope.module.fields, { name: 'from_entry_type' }, true)[0];
                        $scope.customLeaveFields['to_entry_type'] = toField;
                        $scope.customLeaveFields['from_entry_type'] = fromField;
                        if(!$scope.record['to_entry_type'] && !$scope.record['from_entry_type'] && toField){
                            $scope.record['to_entry_type'] = picklists[toField.picklist_id][0];
                            $scope.record['from_entry_type'] = picklists[toField.picklist_id][0];
                        }
                    }
                });

            ModuleService.findRecords('izin_turleri', {filters: [{ field: "yillik_izin", operator: 'equals', value: true, no: 1 }],"limit":999999,"offset":0})
                .then(function(response){
                    $scope.record['izin_turu_data'] = response.data[0];
                    $scope.izinTuruData = response.data[0];
                });

            var request = {};
            request.limit = 99999;
            request.filters = null;
            request.fields = ['e_posta', 'ad_soyad', 'dogum_tarihi', 'ise_baslama_tarihi', 'kalan_izin_hakki', 'yoneticisi'];

            var calisanModule = $filter('filter')($rootScope.modules, { name: 'calisanlar' }, true);
            if(calisanModule.length === 0){
                calisanModule = $filter('filter')($rootScope.modules, { name: 'human_resources' }, true)[0];
                request.fields.push('yoneticisi.human_resources.e_mail1');
                yoneticiEmail = 'yoneticisi.human_resources.e_mail1';
            } else {
                calisanModule = calisanModule[0];
                request.fields.push('yoneticisi.calisanlar.e_posta');
                yoneticiEmail = 'yoneticisi.calisanlar.e_posta';
            }

            ModuleService.findRecords(calisanModule.name, request)
                .then(function (response) {
                    $scope.calisanlar = response.data;
                    $scope.loadingModal = false;
                });

            $scope.fieldValueChange = function(){
                if($scope.record['bitis_tarihi'] && $scope.record['baslangic_tarihi']){
                    ModuleService.setCustomCalculations($scope.module, $scope.record, $scope.picklistsModule, $scope);
                }
            };

            $scope.submitCollectiveLeave = function(){
                $scope.submittingModal = true;
                $scope.submitted = true;
                if($scope.calisanlar.length === 0){
                    ngToast.create({ content: $filter('translate')('Leave.NotFoundUser'), className: 'warning' });
                    $scope.submittingModal = false;
                    return;
                }
                //var record = angular.copy($scope.record);
                $scope.notValidUser = [];
                var validUsers = [];
                for(var i = 0; i < $scope.calisanlar.length; i++){
                    var calisan = $scope.calisanlar[i];
                    $scope.record['goreve_baslama_tarihi'] = calisan['ise_baslama_tarihi'];
                    $scope.record['mevcut_kullanilabilir_izin'] = calisan['kalan_izin_hakki'];
                    var recordCopy = angular.copy($scope.record);
                    var val = ModuleService.customValidations($scope.module, recordCopy);
                    if (val !== "" || (calisan['process.process_requests.process_id'] !== undefined && !calisan[yoneticiEmail])) {
                        $scope.notValidUser.push(calisan);
                    } else {
                        var calisanUser = $filter('filter')($rootScope.users, { Email:  calisan['e_posta']}, true)[0];
                        if(!calisanUser){
                            $scope.notValidUser.push(calisan);
                        } else {
                            var leaveRequest = {
                                baslangic_tarihi: $scope.record['baslangic_tarihi'],
                                bitis_tarihi: $scope.record['bitis_tarihi'],
                                calisan: calisan.id,
                                custom_approver: calisan[yoneticiEmail],
                                from_entry_type: $scope.record['from_entry_type'].id,
                                hesaplanan_alinacak_toplam_izin: $scope.record['hesaplanan_alinacak_toplam_izin'],
                                izin_turu: $scope.izinTuruData.id,
                                mevcut_kullanilabilir_izin: calisan['kalan_izin_hakki'],
                                owner: calisanUser.id,
                                shared_user_groups:null,
                                shared_user_groups_edit:null,
                                shared_users:null,
                                shared_users_edit:null,
                                to_entry_type: $scope.record['to_entry_type'].id
                            };

                            ModuleService.insertRecord('izinler', leaveRequest)
                                .then(function(response){
                                    validUsers.push(response.data.id);
                                    if($scope.calisanlar.length - $scope.notValidUser.length === validUsers.length && calisan['process.process_requests.process_id'] !== undefined){
                                        var approveRecords = function() {
											ModuleService.approveMultipleProcessRequest(validUsers, 'izinler')
                                                .then(function(){
                                                    $scope.submittingModal = false;
                                                });
                                        };
                                        $timeout(approveRecords, 5000);
                                    }
                                });
                        }
                    }
                }
                //$scope.submittingModal = false;
            };
        }
    ]);