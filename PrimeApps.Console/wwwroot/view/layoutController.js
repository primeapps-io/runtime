'use strict';

angular.module('primeapps').controller('LayoutController', ['$rootScope', '$scope', '$location', '$state', '$cookies', '$localStorage', '$window', '$filter', '$anchorScroll', 'config', '$popover', 'entityTypes', 'guidEmpty', 'component', 'convert', 'helper', 'operations', 'blockUI', '$cache', 'helps', 'LayoutService', 'AuthService', '$sessionStorage', '$sce', '$modal', 'FileUploader',
    function ($rootScope, $scope, $location, $state, $cookies, $localStorage, $window, $filter, $anchorScroll, config, $popover, entityTypes, guidEmpty, component, convert, helper, operations, blockUI, $cache, helps, LayoutService, AuthService, $sessionStorage, $sce, $modal, FileUploader) {
        angular.element($window).on('load resize', function () {
            if ($window.innerWidth < 1200) {
                $scope.$apply(function () {
                    $scope.toggleClass = 'full-toggled toggled';
                    $rootScope.subtoggleClass = 'full-toggled2';
                });
            }
            else {
                $scope.$apply(function () {
                    $scope.toggleClass = '';
                    $rootScope.subtoggleClass = '';
                });
            }
        });
        $rootScope.toggledSubMenu = function () {
            $rootScope.subtoggleClass = $rootScope.subtoggleClass === 'full-toggled2' ? '' : 'full-toggled2';
        };

        $scope.organizationShortnameValid = null;
        $scope.isOrganizationShortnameBlur = false;
        $scope.hasPermission = helper.hasPermission;
        $scope.entityTypes = entityTypes;
        $scope.operations = operations;
        $scope.sidebar = angular.element(document.getElementById('wrapper'));
        $scope.navbar = angular.element(document.getElementById('navbar-wrapper'));
        $scope.bottomlinks = angular.element(document.getElementsByClassName('sidebar-bottom-link'));
        $scope.appLauncher = angular.element(document.getElementById('app-launcher'));
        $rootScope.menuOpen = [];

        var getMyOrganizations = function () {
            LayoutService.myOrganizations()
                .then(function (response) {
                    if (response.data) {
                        $rootScope.organizations = response.data;
                        //$scope.menuOpen[$scope.organizations[0].id] = true;
                    }
                });
        };

        $scope.changeOrganization = function (organization) {
            $rootScope.menuOpen = [];
            $rootScope.currentOrganization = organization;
        };

        $rootScope.isMobile = function () {
            var check = false;
            (function (a) {
                if (/(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino/i.test(a) || /1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-/i.test(a.substr(0, 4))) check = true;
            })(navigator.userAgent || navigator.vendor || window.opera);
            return check;
        };

        $scope.preview = function () {
            $rootScope.previewActivating = true;
            LayoutService.getPreviewToken()
                .then(function (response) {
                    $scope.previewActivating = false;
                    $window.open(config.appUrl + '?preview=' + encodeURIComponent(response.data), '_blank');
                })
                .catch(function (response) {
                    $scope.previewActivating = false;
                });
        };

        $scope.logout = function () {
            blockUI.start();

            AuthService.logout()
                .then(function (response) {
                    AuthService.logoutComplete();
                    window.location = response.data['redirect_url'];
                    blockUI.stop();
                });
        };

        $scope.go = function (link) {
            $window.location.href = link;
        };

        $scope.gotoSetup = function (link) {
            $rootScope.selectedSetupMenuLink = link;
            $window.location.href = link;
            $location.hash('top');
            $anchorScroll();
        };

        var windowWidth = window.innerWidth || document.documentElement.clientWidth || document.body.clientWidth;

        $scope.isAvailableForSmallDevice = function () {
            return windowWidth < 1024;
        };

        $scope.isAvailableForSmallDevice();

        $scope.routingPrism = function (url, canReload) {
            if (windowWidth < 1024) {
                $scope.toggleLeftMenu();
            }

            var currentUrl = $state.$current.url.source;

            angular.forEach($state.params, function (value, key) {
                var index = currentUrl.indexOf(key);

                if (currentUrl.charAt(index - 1) === ':') {
                    currentUrl = value ? currentUrl.replace(':' + key, value) : currentUrl.replace(':' + key, '');
                }
                else if (currentUrl.charAt(index - 1) === '?') {
                    currentUrl = value ? currentUrl.replace('?' + key, '?' + key + '=' + value) : currentUrl.replace('?' + key, '');
                }
                else if (currentUrl.charAt(index - 1) === '&') {
                    currentUrl = value ? currentUrl.replace('&' + key, '&' + key + '=' + value) : currentUrl.replace('&' + key, '');
                }
            });

            if (canReload && url.includes(currentUrl)) {
                $state.reload();
            }
        };

        $scope.toggleLeftMenu = function () {
            angular.element($scope.sidebar).toggleClass('toggled');
            angular.element($scope.sidebar).toggleClass('full-toggled');
            angular.element($scope.navbar).toggleClass('toggled');
            angular.element($scope.navbar).toggleClass('full-toggled');
            angular.element($scope.bottomlinks).toggleClass('hidden');

            $scope.isAvailableForSmallDevice();

            $scope.toggled = !$scope.toggled;
        };

        $scope.toggleFullLeftMenu = function () {
            angular.element($scope.sidebar).toggleClass('full-toggled');
            angular.element($scope.sidebar).toggleClass('toggled');
            angular.element($scope.navbar).toggleClass('full-toggled');
            angular.element($scope.navbar).toggleClass('toggled');
            angular.element($scope.bottomlinks).toggleClass('hidden');

            var dropdownMenus = angular.element(document.getElementsByClassName('dropdown-menu'));

            for (var i = 0; i < dropdownMenus.length; i++) {
                angular.element(document.getElementsByClassName('dropdown-menu'))[i].click();
            }
        };

        $scope.toggleAppMenu = function ($timeout) {
            angular.element($scope.appLauncher).toggleClass('toggled');
        };

        $scope.dropdownHide = function () {
            angular.element(document.getElementsByClassName('dropdown-menu'))[0].click();
            angular.element(document.getElementsByClassName('dropdown-menu'))[1].click();
        };

        $scope.reload = function () {
            $scope.reloading = true;
        };

        $scope.newOrganization = function () {
            $scope.icons = LayoutService.getIcons();
            $scope.organizationFormModal = $scope.organizationFormModal || $modal({
                scope: $scope,
                templateUrl: 'view/organization/newOrganization.html',
                animation: 'am-fade-and-slide-right',
                backdrop: 'static',
                show: false
            });
            $scope.organizationFormModal.$promise.then(function () {
                $scope.organizationFormModal.show();

            });

        };

        $scope.closeNewOrganizationModal = function () {
            $scope.organization = {};
            $scope.organizationShortnameValid = null;
            $scope.isOrganizationShortnameBlur = false;
            $scope.organizationFormModal.hide();
        };

        $scope.organizationShortnameBlur = function (organization) {
            $scope.isOrganizationShortnameBlur = true;
            $scope.checkOrganizationShortname(organization ? organization : "");
        };

        $scope.checkOrganizationShortname = function (organization) {
            if (!organization || !organization.name)
                return;

            organization.name = organization.name.replace(/\s/g, '');
            organization.name = organization.name.replace(/[^a-zA-Z0-9\_\-]/g, '');

            organization.name = organization.name.replace(/\s/g, '');
            organization.name = organization.name.replace(/[^a-zA-Z0-9\_\-]/g, '');

            if (!$scope.isOrganizationShortnameBlur)
                return;

            $scope.organizationShortnameChecking = true;
            $scope.organizationShortnameValid = null;

            if (!organization.name || organization.name === '') {
                $scope.organizationShortnameChecking = false;
                $scope.organizationShortnameValid = false;
                return;
            }

            LayoutService.isOrganizationShortnameUnique(organization.name)
                .then(function (response) {
                    $scope.organizationShortnameChecking = false;
                    if (response.data) {
                        $scope.organizationShortnameValid = true;
                    }
                    else {
                        $scope.organizationShortnameValid = false;
                    }
                })
                .catch(function () {
                    $scope.organizationShortnameValid = false;
                    $scope.organizationShortnameChecking = false;
                });
        };

        $scope.generateOrganizationShortName = function (organization) {
            if (!organization || !organization.label || $scope.isOrganizationShortNameChanged)
                return;

            organization.name = helper.getSlug(organization.label, '-');
        };

        $scope.saveOrganization = function (organizationForm) {
            if (!organizationForm.$valid)
                return;
            $scope.organizationSaving = true;

            if (!$scope.organization.icon)
                $scope.organization.icon = 'fas fa-building';

            if (angular.isObject($scope.organization.icon))
                $scope.organization.icon = $scope.organization.icon.value;

            LayoutService.createOrganization($scope.organization)
                .then(function (response) {
                    var copyOrganization = angular.copy($scope.organization);
                    copyOrganization.id = response.data;

                    getMyOrganizations();
                    $scope.changeOrganization(copyOrganization);
                    $scope.menuOpen[response.data] = true;
                    toastr.success('Organization ' + $scope.organization.label + ' successfully created.');
                    $scope.organizationSaving = false;
                    $scope.organization = {};
                    $scope.organizationFormModal.hide();
                    $scope.organizationShortnameValid = null;
                    $scope.isOrganizationShortnameBlur = false;

                    $state.go('studio.apps', {orgId: response.data});

                })
                .catch(function () {
                    toastr.error('Organization ' + $scope.organization.label + ' not created.');
                    $scope.organizationSaving = false;
                    $scope.organizationShortnameValid = null;
                    $scope.isOrganizationShortnameBlur = false;
                });
        };

        var uploader = $scope.uploader = new FileUploader({
            url: 'upload.php'
        });

        uploader.filters.push({
            name: 'imageFilter',
            fn: function (item /*{File|FileLikeObject}*/, options) {
                var type = '|' + item.type.slice(item.type.lastIndexOf('/') + 1) + '|';
                return '|jpg|png|jpeg|bmp|gif|'.indexOf(type) !== -1;
            }
        });

        $scope.organizationSave = function (newOrganizationForm) {
            if (!newOrganizationForm.$valid)
                return false;
        };


    }

]);