'use strict';

angular.module('primeapps')

    .controller('ManageController', ['$rootScope', '$scope', '$filter', '$location', 'helper', 'ManageService', 'ModuleService',
        function ($rootScope, $scope, $filter, $location, helper, ManageService, ModuleService) {

            $scope.orgModel = {};
            $scope.icons = ModuleService.getIcons();
            ManageService.get($scope.$parent.$parent.$parent.currentOrgId).then(function (response) {
                var data = response.data;
                $scope.orgModel.icon = data.icon;
                $scope.orgModel.label = data.label;
                $scope.orgModel.icon = data.icon;
                $scope.orgModel.name = data.name;
            });

            $scope.changeIcon = function () {
                $scope.orgModel.icon = $scope.orgModel.icon.value;
            };

            $scope.save = function () {
                if (angular.isObject($scope.orgModel.icon))
                    $scope.orgModel.icon = $scope.orgModel.icon.value;

                ManageService.update($scope.$parent.$parent.$parent.currentOrgId, $scope.orgModel)
                    .then(function (response) {
                        swal($filter('translate')('Güncelleme Başarılı'), "success");
                    });
            };
        }
    ]);