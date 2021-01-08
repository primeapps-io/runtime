angular.module('primeapps')
    .controller('locationFormModalController', ['$rootScope', '$scope', '$mdDialog',
        function ($rootScope, $scope, $mdDialog) {

            // if ($scope.$parent.$parent)
            //     var moduleScope = $scope.$parent.$parent;

            var filedName = $scope.filedName;

            if ($scope.record && $scope.record[filedName]) {
                $scope.location = $scope.record[filedName];
            } else {

                $scope.addres = "";
                for (var i = 0; i < $scope.module.fields.length; i++) {
                    field = $scope.module.fields[i];
                    if (field.address_type === null && field.deleted !== false)
                        return false;
                    switch (field.address_type) {
                        case 'country':
                            if ($scope.record[field.name])
                                $scope.addres += $scope.record[field.name].labelStr;
                            break;
                        case 'city':
                            if ($scope.record[field.name])
                                $scope.addres += " " + $scope.record[field.name];
                            break;
                        case 'disrict':
                            if ($scope.record[field.name])
                                $scope.addres += " " + $scope.record[field.name];
                            break;
                        case 'street':
                            if ($scope.record[field.name])
                                $scope.addres += " " + $scope.record[field.name];
                            break;
                    }
                }
            }

            $scope.setCoordinat = function () {
                $scope.record[filedName] = $scope.location;
            };

            $scope.close = function () {
                $mdDialog.hide();
            };
        }
    ]);