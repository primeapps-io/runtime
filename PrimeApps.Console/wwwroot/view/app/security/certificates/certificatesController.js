'use strict';

angular.module('primeapps')

    .controller('CertificatesController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', 'ngToast', '$modal', '$timeout', 'helper', 'dragularService', 'CertificatesService', 'LayoutService', '$http', 'config',
        function ($rootScope, $scope, $filter, $state, $stateParams, ngToast, $modal, $timeout, helper, dragularService,CertificatesService, LayoutService, $http, config) {

            //$rootScope.modules = $http.get(config.apiUrl + 'module/get_all');

            $scope.$parent.menuTopTitle = "Security";
            //$scope.$parent.activeMenu = 'security';
            $scope.$parent.activeMenuItem = 'certificates';
            $rootScope.breadcrumblist[2].title = 'Certificates';

            console.log("CertificatesController");

        }
    ]);