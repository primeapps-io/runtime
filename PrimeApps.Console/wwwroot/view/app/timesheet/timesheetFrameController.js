angular.module('primeapps')
    .controller('TimesheetFrameController', ['$rootScope', '$scope', '$location', '$sce',
        function ($rootScope, $scope, $location, $sce) {

            $sce.trustAsResourceUrl($scope.$parent.$parent.timesheetUrl);
            $sce.trustAsResourceUrl("https://timesheet.projectgroup.com.tr/");            
        }
    ]);