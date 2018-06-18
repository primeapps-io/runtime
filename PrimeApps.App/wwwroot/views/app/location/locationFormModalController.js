angular.module('primeapps')
    .controller('locationFormModalController', ['$rootScope', '$scope',
        function ($rootScope, $scope) {

            if($scope.$parent.$parent)
                var moduleScope=$scope.$parent.$parent;

            var filedName = moduleScope.filedName;

            if(moduleScope.record && moduleScope.record[filedName])
            {
                $scope.location=moduleScope.record[filedName];
            }else{

                $scope.addres="";
                angular.forEach(moduleScope.module.fields,function (field) {
                    if(field.address_type===null && field.deleted != false)
                        return false;
                    switch (field.address_type){
                        case 'country':
                            if(moduleScope.record[field.name])
                                $scope.addres+=moduleScope.record[field.name].labelStr;
                            break;
                            case 'city':
                            if(moduleScope.record[field.name])
                              $scope.addres+=  " "+moduleScope.record[field.name];
                            break;
                            case 'disrict':
                            if(moduleScope.record[field.name])
                              $scope.addres+=  " "+moduleScope.record[field.name];
                            break;
                            case 'street':
                            if(moduleScope.record[field.name])
                              $scope.addres+=  " "+moduleScope.record[field.name];
                            break;
                    }

                });

            }

         $scope.setCoordinat=function () {
             moduleScope.record[filedName]=$scope.location;
         }
        }
    ]);