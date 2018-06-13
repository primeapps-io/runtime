'use strict';

angular.module('primeapps')

    .controller('UserCustomShareFormController', ['$rootScope', '$location', '$scope', '$filter', 'ngToast', 'guidEmpty', 'blockUI', '$state', 'UserCustomShareService', 'helper',
        function ($rootScope, $location, $scope, $filter, ngToast, guidEmpty, blockUI, $state, UserCustomShareService, helper) {

            $scope.id = parseInt($location.search().id);
            $scope.language = $rootScope.language;
            $scope.userOwner = {};
            $scope.users = angular.copy($rootScope.workgroup.users);
            $scope.lookupUserAndGroup = helper.lookupUserAndGroup;

            UserCustomShareService.getAll()
                .then(function (response) {
                    $scope.userCustomShares = response.data;

                    if(!$scope.id){
                        for(var i =0 ; i < response.data.length ; i++){
                            var item = response.data[i];
                            if($filter('filter')($scope.users, { Id: item.user_id }, true)[0]){
                                var user = $filter('filter')($scope.users, { Id: item.user_id }, true)[0].email;
                                $scope.users = $filter('filter')($scope.users, { email: '!'+user }, true);
                            }
                        }
                    }
                });

            $scope.multiselect = function () {
                return $filter('filter')($rootScope.modules, function(value, index, array) {
                    return (value.order !== 0);
                }, true);
            };

            if($scope.id){
                $scope.loading = true;
                UserCustomShareService.get($scope.id)
                    .then(function (response) {
                        $scope.userOwner.user = response.data.user_id;
                        $scope.userOwner.shared_user = response.data.shared_user_id;
                        var owners = [];
                        var moduleList = [];

                        //user groups
                        if(response.data.user_groups_list.length > 0 && response.data.user_groups_list[0] !== '{}'){
                            UserCustomShareService.getUserGroups()
                                .then(function (userGroups) {
                                    var user_groups = response.data.user_groups.replace('{', '').replace('}', '').split(',');
                                    for(var i=0; i<response.data.user_groups_list.length ; i++){
                                        var item = $filter('filter')(userGroups.data, { id: parseInt(user_groups[i]) }, true)[0];
                                        var userGroup = {
                                            description: 'User Group',
                                            id : item.id,
                                            name : item.name,
                                            type : "group"
                                        };
                                        owners.push(userGroup);
                                    }
                                })
                        }

                        //users
                        if(response.data.users_list.length > 0){
                            for(var i=0; i<response.data.users_list.length ; i++){
                                var item = $filter('filter')($rootScope.users, { Id: parseInt(response.data.users_list[i]) }, true)[0];
                                var user = {
                                    description: item.Email,
                                    id : item.Id,
                                    name : item.FullName,
                                    type : "user"
                                };
                                owners.push(user);
                            }
                        }

                        //modules
                        if(response.data.module_list.length > 0){
                            for(var k=0; k<response.data.module_list.length ; k++){
                                var module = $filter('filter')($rootScope.modules, { name: response.data.module_list[k] }, true)[0];
                                moduleList.push(module);
                            }
                        }

                        $scope.userOwner.modules = moduleList;
                        $scope.userOwner.owners = owners;
                        $scope.loading = false;
                    });
            }

            $scope.setFormValid = function () {
                $scope.userOwnerForm.shared.$setValidity('minTags', true);
            };

            $scope.save = function () {
                if ($scope.userOwnerForm.$valid) {
                    $scope.saving = true;
                    var modules = null;

                    if ($scope.userOwner.owners && $scope.userOwner.owners.length) {
                        $scope.shared_users = null;
                        $scope.shared_user_groups = null;

                        for (var l = 0; l < $scope.userOwner.owners.length; l++) {
                            var shared = $scope.userOwner.owners[l];

                            if (shared.type === 'user'){
                                if($scope.shared_users === null)
                                    $scope.shared_users = shared.id;
                                else
                                    $scope.shared_users += ',' + shared.id;
                            }

                            if (shared.type === 'group'){
                                if($scope.shared_user_groups === null)
                                    $scope.shared_user_groups = shared.id;
                                else
                                    $scope.shared_user_groups += ',' + shared.id;
                            }
                        }
                    }

                    if($scope.userOwner.modules && $scope.userOwner.modules.length){
                        for (var j = 0; j < $scope.userOwner.modules.length; j++){
                            var module = $scope.userOwner.modules[j];
                            if(modules === null)
                                modules = module.name;
                            else
                                modules += ',' + module.name;
                        }
                    }

                    var obj = {
                        user_id : $scope.userOwner.user,
                        shared_user_id : $scope.userOwner.shared_user,
                        users : $scope.shared_users,
                        user_groups : $scope.shared_user_groups,
                        modules : modules
                    };

                    if (!$scope.id) {
                        UserCustomShareService.create(obj)
                            .then(function onSuccess() {
                                ngToast.create({
                                    content: $filter('translate')('Setup.UserCustomShares.SaveSuccess'),
                                    className: 'success'
                                });
                                $state.go('app.setup.usercustomshares');
                            })
                            .catch(function onError() {
                                error(data, status);
                                $scope.saving = false;
                            });
                    }
                    else {
                        UserCustomShareService.update($scope.id, obj)
                            .then(function onSuccess() {
                                ngToast.create({
                                    content: $filter('translate')('Setup.UserCustomShares.EditSuccess'),
                                    className: 'success'
                                });
                                $state.go('app.setup.usercustomshares');
                            })
                            .catch(function onError() {
                                error(data, status);
                                $scope.saving = false;
                            });
                    }
                }
            };

            $scope.cancel = function () {
                $state.go('app.setup.usercustomshares');
            };
        }
    ]);