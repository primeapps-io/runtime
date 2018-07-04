'use strict';

angular.module('primeapps')

    .controller('ProfileController', ['$rootScope', '$scope', '$filter', 'ngToast', '$popover', 'helper', 'ProfileService',
        function ($rootScope, $scope, $filter, ngToast, $popover, helper, ProfileService) {
            $scope.loading = true;

            function getProfiles() {
                ProfileService.getAll()
                    .then(function (response) {
                        $scope.profiles = ProfileService.getProfiles(response.data, $rootScope.workgroup.instanceID, true);
                        $scope.loading = false;
                    })
                    .catch(function () {
                        $scope.loading = false;
                    });
            }

            getProfiles();

            $scope.showDeleteForm = function (profile) {
                $scope.selectedProfile = profile;
                var transferProfiles = angular.copy($scope.profiles);
                var deleteProfile = angular.copy(profile);
                var index = helper.arrayObjectIndexOf(transferProfiles, deleteProfile);
                transferProfiles.splice(index, 1);
                $scope.transferProfiles = transferProfiles;

                $scope['deletePopover' + profile.id] = $scope['deletePopover' + profile.id] || $popover(angular.element(document.getElementById('deleteButton' + profile.id)), {
                        templateUrl: 'view/setup/profiles/profileDelete.html',
                        placement: 'left',
                        scope: $scope,
                        autoClose: true,
                        show: true
                    });
            };

            $scope.delete = function (transferProfileId) {
                if (!transferProfileId)
                    transferProfileId = $scope.transferProfiles[0].id;

                $scope.profileDeleting = true;

                ProfileService.remove($scope.selectedProfile.id, transferProfileId, $rootScope.workgroup.instanceID)
                    .then(function () {
                        $scope.profileDeleting = false;
                        ngToast.create({content: $filter('translate')('Setup.Profiles.DeleteSuccess'), className: 'success'});
                        $scope['deletePopover' + $scope.selectedProfile.id].hide();

                        getProfiles();
                    })
                    .catch(function () {
                        $scope.profileDeleting = false;
                    });
            }
        }
    ]);