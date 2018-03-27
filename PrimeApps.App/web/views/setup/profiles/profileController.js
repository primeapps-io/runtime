'use strict';

angular.module('ofisim')

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

                $scope['deletePopover' + profile.ID] = $scope['deletePopover' + profile.ID] || $popover(angular.element(document.getElementById('deleteButton' + profile.ID)), {
                        templateUrl: 'web/views/setup/profiles/profileDelete.html',
                        placement: 'left',
                        scope: $scope,
                        autoClose: true,
                        show: true
                    });
            };

            $scope.delete = function (transferProfileId) {
                if (!transferProfileId)
                    transferProfileId = $scope.transferProfiles[0].ID;

                $scope.profileDeleting = true;

                ProfileService.remove($scope.selectedProfile.ID, transferProfileId, $rootScope.workgroup.instanceID)
                    .then(function () {
                        $scope.profileDeleting = false;
                        ngToast.create({content: $filter('translate')('Setup.Profiles.DeleteSuccess'), className: 'success'});
                        $scope['deletePopover' + $scope.selectedProfile.ID].hide();

                        getProfiles();
                    })
                    .catch(function () {
                        $scope.profileDeleting = false;
                    });
            }
        }
    ]);