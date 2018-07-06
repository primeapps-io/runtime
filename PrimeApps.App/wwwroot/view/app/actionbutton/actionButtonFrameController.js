angular.module('primeapps')
    .controller('ActionButtonFrameController', ['$rootScope', '$scope', '$location','$sce',
        function ($rootScope, $scope, $location,$sce) {

            $scope.url = $sce.trustAsResourceUrl($scope.$parent.$parent.frameUrl);
        }
    ]);