'use strict';

angular.module('primeapps')

    .controller('AllAppsController', ['$rootScope', '$scope', 'guidEmpty', 'entityTypes', 'helper', 'config', '$http', '$localStorage', 'operations', '$filter', '$cache', 'activityTypes', 'AllAppsService', '$window', '$state', '$modal', 'dragularService', '$timeout', '$interval', '$aside',
        function ($rootScope, $scope, guidEmpty, entityTypes, helper, config, $http, $localStorage, operations, $filter, $cache, activityTypes, AllAppsService, $window, $state, $modal, dragularService, $timeout, $interval, $aside) {
            console.log("Allapps")

            $scope.appListe=[
                {
                    appName:"Ofisim İK",
                    appDescripton:"Ofisim apps",
                    appImage:"https://ofisimcomprod.blob.core.windows.net/app-logo/08409435-9bff-4e49-a9ee-a82900c3f2ee_21afb109-0627-4f70-9e5a-e27eccd5f103.PNG",
                    appStatus:"2"
                },
                {
                    appName:"Ofisim Crm",
                    appDescripton:"Ofisim apps",
                    appImage:"https://ofisimcomprod.blob.core.windows.net/app-logo/08409435-9bff-4e49-a9ee-a82900c3f2ee_21afb109-0627-4f70-9e5a-e27eccd5f103.PNG",
                    appStatus:"1"
                },
                {
                    appName:"Ofisim Kobi",
                    appDescripton:"Ofisim apps",
                    appImage:"https://ofisimcomprod.blob.core.windows.net/app-logo/08409435-9bff-4e49-a9ee-a82900c3f2ee_21afb109-0627-4f70-9e5a-e27eccd5f103.PNG",
                    appStatus:"3"
                }
            ];

        }
    ]);