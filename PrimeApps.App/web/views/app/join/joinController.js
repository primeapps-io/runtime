'use strict';

angular.module('ofisim')

    .controller('JoinController', ['$scope', '$state', '$localStorage', '$filter', 'helper', 'config', 'ngToast', 'WorkgroupService', 'AuthService',
        function ($scope, $state, $localStorage, $filter, helper, config, ngToast, WorkgroupService, AuthService) {
            helper.hideLoader();

            WorkgroupService.getWorkgoups()
                .then(function (data) {
                    $scope.workgroupsInvited = data.data.Invited;
                });

            $scope.join = function (instanceId, index) {
                $scope['workgroupJoining' + index] = true;

                WorkgroupService.join(instanceId)
                    .then(function () {
                        $localStorage.write('Workgroup', instanceId);
                        $state.go('app.crm.dashboard');
                    })
                    .catch(function () {
                        $scope['workgroupJoining' + index] = false;
                    });
            };

            $scope.upgrade = function () {
                $scope.upgrading = true;

                WorkgroupService.upgradeLicense(config.planIdMembers, 1)
                    .then(function () {
                        AuthService.logout()
                            .then(function () {
                                AuthService.logoutComplete();
                                ngToast.create({content: $filter('translate')('Join.JoinSuccess'), className: 'success', timeout: 8000});
                            })
                            .catch(function () {
                                $scope.upgrading = false;
                            });
                    })
                    .catch(function () {
                        $scope.upgrading = false;
                    });
            };

            $scope.logout = function () {
                AuthService.logout()
                    .then(function () {
                        AuthService.logoutComplete();
                    });
            };
        }
    ]);