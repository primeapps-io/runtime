'use strict';

angular.module('ofisim')

    .controller('UserGroupController', ['$rootScope', '$scope', '$filter', 'ngToast', '$popover', 'helper', 'UserGroupService',
        function ($rootScope, $scope, $filter, ngToast, $popover, helper, UserGroupService) {
            $scope.loading = true;

            function getUserGroups() {
                UserGroupService.getAll()
                    .then(function (userGroups) {
                        $scope.userGroups = userGroups.data;
                    })
                    .finally(function () {
                        $scope.loading = false;
                    });
            }

            getUserGroups();

            $scope.delete = function (id) {
                UserGroupService.delete(id)
                    .then(function () {
                        ngToast.create({ content: $filter('translate')('Setup.UserGroups.DeleteSuccess'), className: 'success' });
                        getUserGroups();
                    });
            }
        }
    ]);