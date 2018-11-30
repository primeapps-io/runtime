'use strict';

angular.module('primeapps')

    .controller('UserCustomShareController', ['$rootScope', '$scope', '$filter', 'ngToast', '$popover', 'helper', 'UserCustomShareService',
        function ($rootScope, $scope, $filter, ngToast, $popover, helper, UserCustomShareService) {
            $scope.loading = true;

            function getUserOwners() {
                UserCustomShareService.getAll()
                    .then(function (response) {
                        $scope.userowners = response.data;
                        for(var i =0 ; i < response.data.length ; i++){
                            var item = response.data[i];
                            $scope.userowners[i].userName = $filter('filter')($rootScope.users, { id: item.user_id }, true)[0].full_name;
                        }
                        $scope.loading = false;
                    })
                    .catch(function () {
                        $scope.loading = false;
                    });
            }

            getUserOwners();

            $scope.delete = function (id) {
                UserCustomShareService.delete(id)
                    .then(function () {
                        ngToast.create({ content: $filter('translate')('Setup.UserGroups.DeleteSuccess'), className: 'success' });
                        getUserOwners();
                    });
            };
        }
    ]);