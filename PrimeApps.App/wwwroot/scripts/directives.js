'use strict';

angular.module('primeapps')

    .directive('focus', ['$timeout',
        function ($timeout) {
            return function (scope, elem, attrs) {
                scope.$watch(attrs.focus, function (newval) {
                    if (newval) {
                        $timeout(function () {
                            elem[0].focus();
                        }, 0, false);
                    }
                });
            };
        }])

    .directive('blur', function () {
        return function (scope, elem, attrs) {
            elem.bind('blur', function () {
                scope.$apply(attrs.blur);
            });
        };
    })

    // .directive('userImage', ['$rootScope', 'config', '$filter',
    //     function ($rootScope, config, $filter) {
    //         return {
    //             restrict: 'A',
    //             link: function (scope, element, attrs) {
    //                 if (!attrs.userImage)
    //                     return;
    //
    //
    //                 var avatar = $filter('filter')($rootScope.avatars, { UserID: parseInt(attrs.userImage) }, true)[0];
    //
    //                 if (!avatar || !avatar.FullUrl)
    //                     return;
    //
    //                 element.attr('src', avatar.FullUrl);
    //             }
    //         };
    //     }])

    .directive('customBackground', function () {
        return {
            restrict: 'A',
            controller: ['$scope', '$element', '$location', '$localStorage',
                function ($scope, $element, $location, $localStorage) {
                    var path = function () {
                        return $location.path();
                    };

                    var addBackground = function (path) {
                        $element.removeClass('app auth self authingo');

                        if (path.indexOf('/app/') > -1) {
                            return $element.addClass('app');
                        }
                        else if (path.indexOf('/auth/') > -1) {

                            $scope.backgroundShow = false;
                            if (path.indexOf('/auth/register') > -1 || path.indexOf('/auth/verify') > -1) {
                                $scope.backgroundShow = true;
                                $scope.language = $localStorage.read('NG_TRANSLATE_LANG_KEY') || 'tr';
                            }

                            return $element.addClass('auth');
                        }
                        else if (path.indexOf('/paymentform') > -1 || path.indexOf('/join') > -1) {
                            return $element.addClass('self');
                        }
                    };

                    addBackground($location.path());

                    return $scope.$watch(path, function (newVal, oldVal) {
                        if (newVal === oldVal) {
                            return;
                        }

                        return addBackground(newVal);
                    });
                }
            ]
        };
    })

    .directive('languageClass', function () {
        return {
            restrict: 'A',
            controller: ['$rootScope', '$scope', '$element',
                function ($rootScope, $scope, $element) {
                    var language = function () {
                        return $rootScope.language;
                    };

                    var addClass = function (newLanguage, oldLanguage) {
                        if (oldLanguage)
                            $element.removeClass(oldLanguage);

                        return $element.addClass(newLanguage);
                    };

                    addClass($rootScope.language);

                    return $scope.$watch(language, function (newVal, oldVal) {
                        if (newVal === oldVal) {
                            return;
                        }

                        return addClass(newVal, oldVal);
                    });
                }
            ]
        };
    })

    .directive('appClass', function () {
        return {
            restrict: 'A',
            controller: ['$rootScope', '$scope', '$element',
                function ($rootScope, $scope, $element) {
                    var getApp = function () {
                        return $rootScope.app;
                    };

                    var addClass = function (newApp, oldApp) {
                        if (oldApp)
                            $element.removeClass(oldApp);

                        return $element.addClass(newApp);
                    };

                    addClass($rootScope.app);

                    return $scope.$watch(getApp, function (newVal, oldVal) {
                        if (newVal === oldVal) {
                            return;
                        }

                        return addClass(newVal, oldVal);
                    });
                }
            ]
        };
    })

    .directive('confirmClick', ['$popover', '$timeout',
        function ($popover, $timeout) {
            return {
                restrict: 'A',
                scope: {
                    'action': '&'
                },
                link: function (scope, element, attrs) {
                    element.bind('click', function () {
                        $timeout(function () {
                            var placement = attrs.placement || 'bottom';
                            var message = attrs.confirmMessage || 'Are you sure?';
                            var yesText = attrs.confirmYes || 'Yes';
                            var noText = attrs.confirmNo || 'No';
                            var popover = $popover(element, {
                                title: message,
                                placement: placement,
                                trigger: 'manual',
                                autoClose: true,
                                contentTemplate: 'views/common/confirm.html' + '?v=' + version
                            });

                            popover.$promise.then(function () {
                                popover.$scope.yesText = yesText;
                                popover.$scope.noText = noText;
                                popover.show();
                            });

                            popover.$scope.confirm = function () {
                                popover.$scope.confirming = true;
                                scope.action();
                            };
                        }, 0);
                    });
                }
            }
        }])
    .directive('customOnChange', function () {
        return {
            require: 'ngModel',
            link: function (scope, element, attrs, ngModel) {
                element.bind('change', function () {
                    scope.$apply(function () {
                        if (element.length > 0) {
                            ngModel.$setViewValue(element[0].files[0].name);
                            ngModel.$render();
                        }

                    });
                });
            }
        };
    })

    .directive('autoGrow', function () {
        return function (scope, element, attr) {
            var update = function () {
                element.css('height', 'auto');

                var offsetHeight = element[0].offsetHeight;
                var scrollHeight = element[0].scrollHeight;

                if (scrollHeight > offsetHeight) {
                    element.css('height', scrollHeight + 'px');
                }
            };

            scope.$watch(attr.ngModel, function () {
                update();
            });

            element.bind('focus', function () {
                update();
            });

            attr.$set('ngTrim', 'false');
        };
    })

    .directive('resetField', ['$compile', '$timeout',
        function ($compile, $timeout) {
            return {
                require: 'ngModel',
                scope: {},
                link: function (scope, element, attrs, ctrl) {
                    var inputTypes = /text|search|tel|url|email|password/i;

                    if (element[0].nodeName !== 'INPUT')
                        throw new Error('resetField is limited to input elements');

                    if (!inputTypes.test(attrs.type))
                        throw new Error('Invalid input type for resetField: ' + attrs.type);

                    var template = $compile('<i ng-show="enabled" ng-mousedown="reset()" class="fa fa-times-circle"></i>')(scope);
                    element.after(template);

                    scope.reset = function () {
                        ctrl.$setViewValue(null);
                        ctrl.$render();
                        $timeout(function () {
                            element[0].focus();
                        }, 0, false);
                    };

                    element.bind('input', function () {
                        scope.enabled = !ctrl.$isEmpty(element.val());
                    })
                        .bind('focus', function () {
                            scope.enabled = !ctrl.$isEmpty(element.val());
                            scope.$apply();
                        })
                        .bind('blur', function () {
                            scope.enabled = false;
                            scope.$apply();
                        });
                }
            };
        }])

    .directive('compareTo', function () {
        return {
            require: 'ngModel',
            scope: {
                otherModelValue: '=compareTo'
            },
            link: function (scope, element, attributes, ngModel) {

                ngModel.$validators.compareTo = function (modelValue) {
                    return modelValue == scope.otherModelValue;
                };

                scope.$watch('otherModelValue', function () {
                    ngModel.$validate();
                });
            }
        };
    })

    .directive('ngThumb', ['$window',
        function ($window) {
            var helper = {
                support: !!($window.FileReader && $window.CanvasRenderingContext2D),
                isFile: function (item) {
                    return angular.isObject(item) && item instanceof $window.File;
                },
                isImage: function (file) {
                    var type = '|' + file.type.slice(file.type.lastIndexOf('/') + 1) + '|';
                    return '|jpg|png|jpeg|bmp|gif|'.indexOf(type) > -1;
                }
            };

            return {
                restrict: 'A',
                template: '<canvas/>',
                link: function (scope, element, attributes) {
                    if (!helper.support) return;

                    var params = scope.$eval(attributes.ngThumb);

                    if (!helper.isFile(params.file)) return;
                    if (!helper.isImage(params.file)) return;

                    var canvas = element.find('canvas');
                    var reader = new FileReader();

                    reader.onload = onLoadFile;
                    reader.readAsDataURL(params.file);

                    function onLoadFile(event) {
                        var img = new Image();
                        img.onload = onLoadImage;
                        img.src = event.target.result;
                    }

                    function onLoadImage() {
                        var width = params.width || this.width / this.height * params.height;
                        var height = params.height || this.height / this.width * params.width;
                        canvas.attr({ width: width, height: height });
                        canvas[0].getContext('2d').drawImage(this, 0, 0, width, height);
                    }
                }
            };
        }])

    .directive('numeric', function () {
        return {
            require: 'ngModel',
            scope: {
                min: '=minValue',
                max: '=maxValue',
                ngRequired: '=ngRequired'
            },
            link: function (scope, element, attrs, modelCtrl) {
                modelCtrl.$parsers.push(function (inputValue) {
                    if (inputValue === undefined || inputValue.indexOf(' ').length > -1)
                        return '';

                    var transformedInput = inputValue.replace(/[^0-9]/g, '');
                    if (transformedInput != inputValue) {
                        modelCtrl.$setViewValue(transformedInput);
                        modelCtrl.$render();
                    }

                    modelCtrl.$validators.min = function (cVal) {
                        if (!scope.ngRequired && isNaN(cVal)) {
                            return true;
                        }
                        if (typeof scope.min !== 'undefined') {
                            return cVal >= parseInt(scope.min);
                        }
                        return true;
                    };

                    scope.$watch('min', function (val) {
                        modelCtrl.$validate();
                    });

                    modelCtrl.$validators.max = function (cVal) {
                        if (!scope.ngRequired && isNaN(cVal)) {
                            return true;
                        }
                        if (typeof scope.max !== 'undefined') {
                            return cVal <= parseInt(scope.max);
                        }
                        return true;
                    };

                    scope.$watch('max', function (val) {
                        modelCtrl.$validate();
                    });

                    return transformedInput;
                });
            }
        };
    })

    .directive('restrict', ['$parse',
        function ($parse) {
            return {
                restrict: 'A',
                require: 'ngModel',
                link: function (scope, element, attrs) {
                    scope.$watch(attrs.ngModel, function (value) {
                        if (!value) {
                            return;
                        }

                        $parse(attrs.ngModel).assign(scope, value.replace(new RegExp(attrs.restrict, 'g'), ''));
                    });
                }
            }
        }])

    .directive('placeholder', ['$timeout',
        function ($timeout) {
            var inp = document.createElement('input');

            if ('placeholder' in inp) {
                return {}
            }

            return {
                link: function (scope, elm, attrs) {
                    $timeout(function () {
                        elm.val(attrs.placeholder);

                        elm.bind('focus', function () {
                            if (elm.val() == attrs.placeholder) {
                                elm.val('');
                            }
                        }).bind('blur', function () {
                            if (elm.val() == '') {
                                elm.val(attrs.placeholder);
                            }
                        });
                    });
                }
            }
        }])

    .directive('ngEnter', function () {
        return function (scope, element, attrs) {
            element.bind('keydown keypress', function (event) {
                if (event.which === 13) {
                    scope.$apply(function () {
                        scope.$eval(attrs.ngEnter);
                    });

                    event.preventDefault();
                }
            });
        };
    })

    .directive('subTable', ['$rootScope', 'ngTableParams', 'ngToast', 'blockUI', '$filter', '$cache', 'helper', 'exportFile', 'operations', 'ModuleService',
        function ($rootScope, ngTableParams, ngToast, blockUI, $filter, $cache, helper, exportFile, operations, ModuleService) {
            return {
                restrict: 'EA',
                scope: {
                    relatedModule: '=',
                    parentModule: '=',
                    reload: '=',
                    showFilter: '=',
                    isSelectable: '@',
                    disableSelectAll: '@',
                    disableLinks: '@'
                },
                templateUrl: 'views/common/subtable.html?v=' + version,
                controller: ['$scope',
                    function ($scope) {
                        $scope.loading = true;
                        $scope.relatedModule.loading = true;
                        $scope.module = $filter('filter')($rootScope.modules, { name: $scope.relatedModule.related_module }, true)[0];
                        $scope.type = $scope.relatedModule.related_module;
                        $scope.readonly = $scope.relatedModule.readonly || false;
                        $scope.parentType = $scope.relatedModule.relation_field;
                        $scope.parentId = $scope.$parent.id;
                        $scope.language = $rootScope.language;
                        $scope.operations = operations;
                        $scope.hasPermission = helper.hasPermission;
                        $scope.lookupUser = helper.lookupUser;
                        $scope.relatedModuleInModal = ($scope.$parent.selectedRelatedModule && $scope.$parent.selectedRelatedModule.relatedModuleInModal) ? true : false;
                        $scope.previousParentType = $scope.$parent.previousParentType;
                        $scope.previousParentId = $scope.$parent.previousParentId;
                        $scope.previousReturnTab = $scope.$parent.previousReturnTab;
                        $scope.isAdmin = $rootScope.user.profile.HasAdminRights;

                        var counts = [10, 25, 50, 100];
                        var displayFields = $scope.relatedModule.display_fields;

                        var parent = $scope.parentType + (!$scope.isSelectable ? $scope.parentId : '');
                        $scope.cacheKey = parent + '_' + $scope.module.name;
                        $scope.$parent['selectedRows' + $scope.type] = [];
                        var isManyToMany = $scope.relatedModule.relation_type === 'many_to_many';
                        var filters = [];
                        if ($scope.isSelectable === undefined || $scope.isSelectable === null)
                            $scope.isSelectable = false;

                        if (!$scope.isSelectable) {
                            var fieldName = $scope.parentType;

                            if (isManyToMany)
                                fieldName = $scope.parentModule != $scope.module.name ? $scope.parentModule + '_id' : $scope.parentModule + '1_id';

                            filters.push({
                                field: fieldName,
                                operator: 'equals',
                                value: $scope.parentId,
                                no: 1
                            });

                            if ($scope.parentType === 'related_to') {
                                var parentModule = $filter('filter')($rootScope.modules, { name: $scope.parentModule }, true)[0];
                                $scope.parentType = $scope.parentModule;

                                filters.push({
                                    field: 'related_module',
                                    operator: 'is',
                                    value: parentModule['label_' + $rootScope.user.tenant_language + '_singular'],
                                    no: 1
                                });
                            }
                        }

                        //should be true for tables and subtables to delete selecteds
                        $scope.isSelectable = true;

                        if (isManyToMany)
                            $scope.cacheKey = parent + '_' + $scope.relatedModule.relation_field + '_' + $scope.relatedModule.related_module;

                        var tableBlockUI = blockUI.instances.get('tableBlockUISubTable' + $scope.cacheKey);

                        ModuleService.setTable($scope, tableBlockUI, counts, 10, angular.copy(filters), parent, $scope.type, $scope.isSelectable, (!$scope.disableLinks ? $scope.parentId : null), (!$scope.disableLinks ? $scope.parentType : null), displayFields, $scope.relatedModule, $scope.parentModule, $scope.relatedModule.id, $scope.previousParentType, $scope.previousParentId, $scope.previousReturnTab, $scope.$parent);

                        $scope.tableParams.disableSelectAll = $scope.disableSelectAll;

                        $scope.isManyToManyModal = true;

                        $scope.refresh = function (clearFilter) {
                            $cache.remove($scope.cacheKey);

                            if (clearFilter) {
                                $scope.tableParams.filterList = filters;
                                $scope.tableParams.refreshing = true;
                            }

                            $scope.tableParams.reloading = true;
                            $scope.tableParams.reload();
                        };

                        $scope.$watch('reload', function (value) {
                            if (!value)
                                return;

                            $scope.refresh(false);
                        });

                        $scope.$watch('showFilter', function (value) {
                            if (!value)
                                return;

                            $scope.tableParams.showFilter = !$scope.tableParams.showFilter
                        });

                        $scope.delete = function (id) {
                            ModuleService.deleteRecord($scope.module.name, id)
                                .then(function (deletedRecordCount) {
                                    setTimeout(function () {
                                        ModuleService.getRecord($scope.parentModule, $scope.parentId)
                                            .then(function (response) {
                                                var record = ModuleService.processRecordSingle(response.data, $scope.$parent.$parent.module, $scope.$parent.$parent.picklistsModule);
                                                ModuleService.formatRecordFieldValues(record, $scope.$parent.$parent.module, $scope.$parent.$parent.picklistsModule);
                                                $scope.$parent.$parent.$parent.record = record;
                                                var parentCacheKey = $scope.parentModule + '_' + $scope.parentModule;
                                                $cache.remove(parentCacheKey);
                                                $scope.tableParams.reload();
                                            });
                                    }, 1000);

                                });
                        };

                        $scope.deleteRelation = function (id) {
                            var relation = {};
                            relation[$scope.parentModule + '_id'] = parseInt($scope.parentId);
                            relation[$scope.relatedModule.related_module + '_id'] = id;

                            ModuleService.deleteRelation($scope.parentModule, $scope.relatedModule.related_module, relation)
                                .then(function (deletedRecordCount) {
                                    $cache.remove($scope.cacheKey);
                                    $scope.tableParams.reload();
                                });
                        };

                        $scope.multiselect = function (searchTerm, field) {
                            var picklistItems = [];

                            angular.forEach($scope.tableParams.picklists[field.picklist_id], function (picklistItem) {
                                if (picklistItem.inactive)
                                    return;

                                if (picklistItem.labelStr.toLowerCase().indexOf(searchTerm) > -1)
                                    picklistItems.push(picklistItem);
                            });

                            return picklistItems;
                        };

                        $scope.selectAllModal = function (event, data) {
                            $scope.$parent['selectedRows' + $scope.type] = [];
                            if ($scope.isAllSelectedModal) {
                                $scope.isAllSelectedModal = false;
                            } else {
                                $scope.isAllSelectedModal = true;
                                angular.forEach(data, function (record) {
                                    record.fields.forEach(function (field) {
                                        /*find primary field and get its value*/
                                        if (field.primary == true) {
                                            /*add selected record*/
                                            $scope.$parent['selectedRows' + $scope.type].push({
                                                id: record.id,
                                                displayName: field.valueFormatted
                                            });
                                        }
                                    });
                                });
                            }
                        };

                        $scope.selectRow = function ($event, record) {
                            /*selects or unselects records*/
                            if ($event.target.checked) {
                                record.fields.forEach(function (field) {
                                    /*find primary field and get its value*/
                                    if (field.primary == true) {
                                        /*add selected record*/
                                        $scope.$parent['selectedRows' + $scope.type].push({
                                            id: record.id,
                                            displayName: field.valueFormatted
                                        });
                                    }
                                });
                            }
                            else {
                                $scope.$parent['selectedRows' + $scope.type] = $scope.$parent['selectedRows' + $scope.type].filter(function (selectedItem) {
                                    return selectedItem.id != record.id;
                                });

                            }

                            $scope.isAllSelectedModal = false;
                        };

                        $scope.isRowSelected = function (id) {
                            return $scope.$parent['selectedRows' + $scope.type].filter(function (selectedItem) {
                                return selectedItem.id == id;
                            }).length > 0;
                        };

                        $scope.$parent.$parent.$parent.isManyToMany = isManyToMany;

                        //global delete selecteds function
                        $scope.deleteSelectedsSubTable = function () {
                            if (!$scope.relatedModuleInModal) {
                                if (!$scope.$parent['selectedRows' + $scope.type] || !$scope.$parent['selectedRows' + $scope.type].length)
                                    return;

                                var records = [];
                                $scope.$parent['selectedRows' + $scope.type].filter(function (itm) {
                                    records.push(itm.id)
                                });

                                ModuleService.deleteRecordBulk($scope.module.name, records)
                                    .then(function (response) {
                                        $cache.remove($scope.cacheKey);
                                        $scope.tableParams.reloading = true;
                                        $scope.tableParams.reload();
                                        $scope.$parent['selectedRows' + $scope.type] = [];
                                        $scope.isAllSelectedModal = false;
                                        var deletedCount = {};
                                        deletedCount.data = records.length;
                                    });
                            }
                        };

                        //global export function
                        $scope.export = function () {
                            if (!$scope.relatedModuleInModal) {
                                if ($scope.tableParams.total() < 1)
                                    return;

                                var isFileSaverSupported = false;

                                try {
                                    isFileSaverSupported = !!new Blob;
                                } catch (e) {
                                }

                                if (!isFileSaverSupported) {
                                    ngToast.create({
                                        content: $filter('translate')('Module.ExportUnsupported'),
                                        className: 'warning',
                                        timeout: 8000
                                    });
                                    return;
                                }

                                if ($scope.tableParams.total() > 3000) {
                                    ngToast.create({
                                        content: $filter('translate')('Module.ExportWarning'),
                                        className: 'warning',
                                        timeout: 8000
                                    });
                                    return;
                                }

                                var fileName = $scope.module['label_' + $rootScope.language + '_plural'] + '-' + $filter('date')(new Date(), 'dd-MM-yyyy') + '.xls';
                                $scope.exporting = true;

                                ModuleService.getCSVData($scope, $scope.type)
                                    .then(function (csvData) {
                                        ngToast.create({
                                            content: $filter('translate')('Module.ExcelExportSuccess'),
                                            className: 'success',
                                            timeout: 5000
                                        });
                                        exportFile.excel(csvData, fileName);
                                        $scope.exporting = false;
                                    });
                            }

                        };
                    }]
            };
        }])

    .directive('numberCurrency', ['$rootScope', '$filter', '$locale', 'helper',
        function ($rootScope, $filter, $locale, helper) {
            return {
                restrict: 'A',
                require: 'ngModel',
                scope: {
                    minValue: '=',
                    maxValue: '=',
                    currencySymbol: '=',
                    ngRequired: '=',
                    places: '=',
                    rounding: '='
                },
                link: function (scope, element, attrs, ngModel) {
                    if (attrs.numberCurrency === 'false') return;

                    var places = (typeof scope.places !== 'undefined' && scope.places != null) ? scope.places : 2;
                    var rounding = (typeof scope.rounding !== 'undefined' && scope.rounding != null) ? scope.rounding : 'none';

                    function decimalRex(dChar) {
                        return RegExp("\\d|\\-|\\" + dChar, 'g');
                    }

                    function clearRex(dChar) {
                        return RegExp("\\-{0,1}((\\" + dChar + ")|([0-9]{1,}\\" + dChar + "?))&?[0-9]{0," + 100 + "}", 'g');
                    }

                    function parseValue(value) {
                        value = String(value);
                        var dSeparator = $locale.NUMBER_FORMATS.DECIMAL_SEP;
                        var valueFloat = null;

                        // Replace negative pattern to minus sign (-)
                        var neg_dummy = $filter('currency')("-1", getCurrencySymbol(), scope.places);
                        var neg_idx = neg_dummy.indexOf("1");
                        var neg_str = neg_dummy.substring(0, neg_idx);
                        value = value.replace(neg_str, "-");

                        if (RegExp("^-[\\s]*$", 'g').test(value)) {
                            value = "-0";
                        }

                        if (decimalRex(dSeparator).test(value)) {
                            var cleared = value.match(decimalRex(dSeparator)).join("").match(clearRex(dSeparator));

                            if (cleared)
                                cleared = cleared[0].replace(dSeparator, ".");

                            if (!cleared)
                                return null;

                            valueFloat = parseFloat(cleared);

                            switch (rounding) {
                                case 'off':
                                    valueFloat = helper.roundBy(Math.round, valueFloat, places);
                                    break;
                                case 'down':
                                    valueFloat = helper.roundBy(Math.floor, valueFloat, places);
                                    break;
                                case 'up':
                                    valueFloat = helper.roundBy(Math.ceil, valueFloat, places);
                                    break;
                            }
                        }

                        return valueFloat;
                    }

                    function clearValue(value) {
                        value = String(value);
                        var dSeparator = $locale.NUMBER_FORMATS.DECIMAL_SEP;
                        var cleared = null;

                        // Replace negative pattern to minus sign (-)
                        var neg_dummy = $filter('currency')("-1", getCurrencySymbol(), scope.places);
                        var neg_idx = neg_dummy.indexOf("1");
                        var neg_str = neg_dummy.substring(0, neg_idx);
                        value = value.replace(neg_str, "-");

                        if (RegExp("^-[\\s]*$", 'g').test(value)) {
                            value = "-0";
                        }

                        if (decimalRex(dSeparator).test(value)) {
                            cleared = value.match(decimalRex(dSeparator))
                                .join("").match(clearRex(dSeparator));
                            cleared = cleared ? cleared[0].replace(dSeparator, ".") : null;
                        }

                        return cleared;
                    }

                    function getCurrencySymbol() {
                        if (scope.currencySymbol) {
                            if (scope.currencySymbol === 'false')
                                scope.currencySymbol = '';

                            return scope.currencySymbol;
                        } else {
                            if (!$rootScope.currencySymbol)
                                return $locale.NUMBER_FORMATS.CURRENCY_SYM;
                            else
                                return $rootScope.currencySymbol;
                        }
                    }

                    function reformatViewValue() {
                        var formatters = ngModel.$formatters,
                            idx = formatters.length;

                        var viewValue = ngModel.$$rawModelValue;

                        while (idx--) {
                            viewValue = formatters[idx](viewValue);
                        }

                        ngModel.$setViewValue(viewValue);
                        ngModel.$render();
                    }

                    ngModel.$parsers.push(function (viewValue) {
                        var cVal = parseValue(viewValue);
                        //return parseFloat(cVal);
                        // Check for fast digitation (-. or .)
                        if (cVal == "." || cVal == "-.") {
                            cVal = ".0";
                        }
                        return parseFloat(cVal);
                    });

                    element.on('blur', function () {
                        ngModel.$commitViewValue();
                        reformatViewValue();
                    });

                    ngModel.$formatters.unshift(function (value) {
                        return $filter('currency')(value, getCurrencySymbol(), scope.places);
                    });

                    ngModel.$validators.min = function (cVal) {
                        if (!scope.ngRequired && isNaN(cVal)) {
                            return true;
                        }
                        if (typeof scope.minValue !== 'undefined') {
                            return cVal >= parseFloat(scope.minValue);
                        }
                        return true;
                    };

                    ngModel.$validators.max = function (cVal) {
                        if (!scope.ngRequired && isNaN(cVal)) {
                            return true;
                        }
                        if (typeof scope.maxValue !== 'undefined') {
                            return cVal <= parseFloat(scope.maxValue);
                        }
                        return true;
                    };

                    scope.$watch('maxValue', function (val) {
                        ngModel.$validate();
                    });

                    scope.$watch('minValue', function (val) {
                        ngModel.$validate();
                    });

                    scope.$watch('currencySymbol', function (val) {
                        ngModel.$commitViewValue();
                        reformatViewValue();
                    });

                    ngModel.$validators.places = function (cVal) {
                        if (!!cVal && isNaN(cVal)) {
                            return false;
                        }

                        return true;
                    };
                }
            }
        }])

    .directive('numberDecimal', ['$rootScope', '$filter', '$locale', 'helper',
        function ($rootScope, $filter, $locale, helper) {
            return {
                restrict: 'A',
                require: 'ngModel',
                scope: {
                    min: '=minValue',
                    max: '=maxValue',
                    ngRequired: '=',
                    places: '=',
                    rounding: '='
                },
                link: function (scope, element, attrs, ngModel) {
                    if (attrs.numberDecimal === 'false') return;

                    var places = (typeof scope.places !== 'undefined' && scope.places != null) ? scope.places : 2;
                    var rounding = (typeof scope.rounding !== 'undefined' && scope.rounding != null) ? scope.rounding : 'none';

                    function decimalRex(dChar) {
                        return RegExp("\\d|\\-|\\" + dChar, 'g');
                    }

                    function clearRex(dChar) {
                        return RegExp("\\-{0,1}((\\" + dChar + ")|([0-9]{1,}\\" + dChar + "?))&?[0-9]{0," + 100 + "}", 'g');
                    }

                    function parseValue(value) {
                        value = String(value);
                        var dSeparator = $locale.NUMBER_FORMATS.DECIMAL_SEP;
                        var valueFloat = null;

                        // Replace negative pattern to minus sign (-)
                        var neg_dummy = $filter('number')("-1", scope.places);
                        var neg_idx = neg_dummy.indexOf("1");
                        var neg_str = neg_dummy.substring(0, neg_idx);
                        value = value.replace(neg_str, "-");

                        if (RegExp("^-[\\s]*$", 'g').test(value)) {
                            value = "-0";
                        }

                        if (decimalRex(dSeparator).test(value)) {
                            var cleared = value.match(decimalRex(dSeparator)).join("").match(clearRex(dSeparator));

                            if (cleared)
                                cleared = cleared[0].replace(dSeparator, ".");

                            if (!cleared)
                                return null;

                            valueFloat = parseFloat(cleared);

                            switch (rounding) {
                                case 'off':
                                    valueFloat = helper.roundBy(Math.round, valueFloat, places);
                                    break;
                                case 'down':
                                    valueFloat = helper.roundBy(Math.floor, valueFloat, places);
                                    break;
                                case 'up':
                                    valueFloat = helper.roundBy(Math.ceil, valueFloat, places);
                                    break;
                            }
                        }

                        return valueFloat;
                    }

                    function reformatViewValue() {
                        var formatters = ngModel.$formatters,
                            idx = formatters.length;

                        var viewValue = ngModel.$$rawModelValue;

                        while (idx--) {
                            viewValue = formatters[idx](viewValue);
                        }

                        ngModel.$setViewValue(viewValue);
                        ngModel.$render();
                    }

                    ngModel.$parsers.push(function (viewValue) {
                        var cVal = parseValue(viewValue);
                        //return parseFloat(cVal);
                        // Check for fast digitation (-. or .)
                        if (cVal == "." || cVal == "-.") {
                            cVal = ".0";
                        }
                        return parseFloat(cVal);
                    });

                    element.on('blur', function () {
                        ngModel.$commitViewValue();
                        reformatViewValue();
                    });

                    ngModel.$formatters.unshift(function (value) {
                        return $filter('number')(value, scope.places);
                    });

                    ngModel.$validators.min = function (cVal) {
                        if (!scope.ngRequired && isNaN(cVal)) {
                            return true;
                        }
                        if (typeof scope.min !== 'undefined') {
                            return cVal >= parseFloat(scope.min);
                        }
                        return true;
                    };

                    scope.$watch('min', function (val) {
                        ngModel.$validate();
                    });

                    ngModel.$validators.max = function (cVal) {
                        if (!scope.ngRequired && isNaN(cVal)) {
                            return true;
                        }
                        if (typeof scope.max !== 'undefined') {
                            return cVal <= parseFloat(scope.max);
                        }
                        return true;
                    };

                    scope.$watch('max', function (val) {
                        ngModel.$validate();
                    });

                    ngModel.$validators.places = function (cVal) {
                        if (!!cVal && isNaN(cVal)) {
                            return false;
                        }

                        return true;
                    };
                }
            }
        }])

    .directive('tooltip', function () {
        return {
            restrict: 'A',
            link: function (scope, element, attrs) {
                $(element).hover(function () {
                    // on mouseenter
                    $(element).tooltip('show');
                }, function () {
                    // on mouseleave
                    $(element).tooltip('hide');
                });
            }
        };
    })

    .directive('editableCustomSelect', ['$rootScope', 'editableDirectiveFactory',
        function ($rootScope, editableDirectiveFactory) {
            var label = 'Seçiniz -->';

            if ($rootScope.language === 'en')
                label = 'Please select -->';

            return editableDirectiveFactory({
                directiveName: 'editableCustomSelect',
                inputTpl: '<select><option value="">' + label + '</option></select>'
            });
        }])

    .directive('customScripting', ['$timeout', 'ngToast', 'ModuleService', '$modal', '$http', 'config',
        function ($timeout, ngToast, ModuleService, $modal, $http, config) {
            return {
                restrict: 'A',
                link: function (scope, element, attrs) {
                    element.bind('click', function () {
                        scope.toast = function (message, type, timeout, dismissButton) {
                            $timeout(function () {
                                ngToast.create({
                                    content: message,
                                    className: type,
                                    timeout: timeout || 5000,
                                    dismissButton: dismissButton || false,
                                    dismissOnClick: !dismissButton,
                                    dismissOnTimeout: !dismissButton
                                });
                            });
                        };

                        try {
                            scope.running = true;

                            $timeout(function () {
                                var customScript = attrs['customScripting'];
                                eval(customScript);
                            });
                        }
                        catch (e) {
                            scope.running = false;
                            return null;
                        }
                    });
                }
            };
        }])

    .directive('webHook', ['$http', 'ngToast', '$filter',
        function ($http, ngToast, $filter) {
            return {
                restrict: 'A',
                link: function (scope, element, attrs) {
                    element.bind('click', function () {
                        var actionDetails = angular.fromJson(attrs['webHook']);
                        if (actionDetails.template && actionDetails.url) {
                            scope.loading = true;
                            var templateData = actionDetails.template.split(',');
                            var data = {};
                            var recordData = scope.$parent.$parent.record;

                            angular.forEach(templateData, function (value, key) {
                                var inputvalue = recordData[value];
                                if (inputvalue) {
                                    if (inputvalue.length > 0) {//object check
                                        data[value] = inputvalue;
                                    }
                                    else {
                                        data[value] = inputvalue['labelStr'];
                                    }
                                }
                            });

                            $http.post(actionDetails.url, data)
                                .then(function (data) {
                                    ngToast.create({ content: $filter('translate')('Common.ProcessTriggerSuccess'), className: 'success' });
                                })
                                .error(function () {
                                    ngToast.create({ content: $filter('translate')('Common.Error'), className: 'danger' });
                                    scope.loading = false;
                                });
                        }

                    });
                }
            };
        }])

    .directive('customModalFrame', [function () {
        return {
            restrict: 'A',
            link: function (scope, element, attrs) {
                element.bind('click', function () {
                    var actionDetails = angular.fromJson(attrs['customModalFrame']);
                    //Future usage
                });
            }
        };
    }])
    .directive('uiTinymceMulti', ['$rootScope', 'uiTinymceConfig', function ($rootScope, uiTinymceConfig) {
        uiTinymceConfig = uiTinymceConfig || {};
        var generatedIds = 0;
        return {
            require: 'ngModel',
            link: function (scope, elm, attrs, ngModel) {
                var expression, options, tinyInstance;

                // generate an ID if not present
                if (!attrs.id) {
                    attrs.$set('id', 'uiTinymce' + generatedIds++);
                }

                options = {
                    // Update model when calling setContent (such as from the source editor popup)
                    setup: function (ed) {
                        ed.on('init', function (args) {
                            ngModel.$render();
                        });
                        // Update model on button click
                        ed.on('ExecCommand', function (e) {
                            ed.save();
                            ngModel.$setViewValue(elm.val());
                            if (!scope.$$phase) {
                                scope.$apply();
                            }
                        });
                        // Update model on keypress
                        ed.on('KeyUp', function (e) {
                            ed.save();
                            ngModel.$setViewValue(elm.val());
                            if (!scope.$$phase) {
                                scope.$apply();
                            }
                        });
                    },
                    style_formats: [
                        {
                            title: ($rootScope.language === 'tr' ? 'Yazı Boyutu' : 'Font Size'), items: [
                                { title: ($rootScope.language === 'tr' ? 'Çok Büyük' : 'Very Big'), block: 'h2', styles: { fontWeight: 'normal' } },
                                { title: ($rootScope.language === 'tr' ? 'Büyük' : 'Big'), block: 'h3', styles: { fontWeight: 'normal' } },
                                { title: ($rootScope.language === 'tr' ? 'Normal' : 'Normal'), block: 'h4', styles: { fontWeight: 'normal' } },
                                { title: ($rootScope.language === 'tr' ? 'Küçük' : 'Small'), block: 'h5', styles: { fontWeight: 'normal' } },
                                { title: ($rootScope.language === 'tr' ? 'Çok Küçük' : 'Very Small'), block: 'h6', styles: { fontWeight: 'normal' } }
                            ]
                        }
                    ],
                    mode: 'exact',
                    elements: attrs.id,
                    language: $rootScope.language,
                    menubar: false,
                    statusbar: false,
                    plugins: 'fullscreen paste',
                    paste_as_text: true,
                    toolbar: 'bold italic bullist numlist | styleselect | fullscreen',
                    skin: 'lightgray',
                    theme: 'modern',
                    height: '200'
                };

                if (attrs.uiTinymce) {
                    expression = scope.$eval(attrs.uiTinymce);
                } else {
                    expression = {};
                }

                angular.extend(options, uiTinymceConfig, expression);

                setTimeout(function () {
                    tinymce.init(options);
                });

                ngModel.$render = function () {
                    if (!tinyInstance) {
                        tinyInstance = tinymce.get(attrs.id);
                    }
                    if (tinyInstance) {
                        tinyInstance.setContent(ngModel.$viewValue || '');
                    }
                };
            }
        };
    }])

    .directive('location', ['$rootScope', 'config', '$filter', '$timeout',
        function ($rootScope, config, $filter, $timeout) {
            return {
                restrict: 'E',
                require: '^?ngModel',

                link: function (scope, element, attrs, ngModel) {

                    var conf = {
                        latitude: 39.93948807471046,
                        longitude: 32.85907745361328,
                        zoom: 5
                    };

                    function success(pos) {
                        conf.latitude = pos.coords.latitude;
                        conf.longitude = pos.coords.longitude;
                        conf.zoom = 10;
                        getlocation();
                    };

                    function error() {
                        getlocation();
                    }

                    navigator.geolocation.getCurrentPosition(success, error);

                    function getlocation() {
                        var mapCanvas = element[0];

                        if (scope.addres && !scope.location) {
                            var geocoder = new google.maps.Geocoder();
                            geocoder.geocode({ 'address': scope.addres }, function (results, status) {
                                $timeout(function () {
                                    if (status === 'OK') {
                                        conf.latitude = results[0].geometry.location.lat();
                                        conf.longitude = results[0].geometry.location.lng();
                                        map(mapCanvas);

                                    } else {
                                        map(mapCanvas);
                                    }
                                });

                            });
                        } else {
                            map(mapCanvas);
                        }

                    }

                    function map(mapCanvas) {
                        if (scope.location) {
                            var location = scope.location.split(",");
                            conf.latitude = location[0];
                            conf.longitude = location[1];
                            conf.zoom = 17;
                        }
                        var defaultCoord = new google.maps.LatLng(conf.latitude, conf.longitude);
                        var mapOptions = { center: defaultCoord, zoom: conf.zoom };
                        var map = new google.maps.Map(mapCanvas, mapOptions);

                        var marker = new google.maps.Marker({
                            draggable: true,
                            animation: google.maps.Animation.DROP,
                            position: defaultCoord,
                        });
                        ngModel.$setViewValue(conf.latitude + "," + conf.longitude);
                        var coordInfoWindow = new google.maps.InfoWindow();
                        map.addListener('mouseup', function (e) {
                            var latlng = e.latLng;

                            ngModel.$setViewValue(latlng.lat() + "," + latlng.lng());
                            coordInfoWindow.setContent(latlng.lat() + "," + latlng.lng());
                            coordInfoWindow.open(map, marker);

                        });
                        map.addListener('click', function (e) {
                            var latlng = e.latLng;
                            var latlng = e.latLng;
                            scope.ngModel = latlng.lat() + "," + latlng.lng();
                            coordInfoWindow.setContent(latlng.lat() + "," + latlng.lng());
                            marker.setPosition(latlng);
                            coordInfoWindow.open(map, marker);
                            ngModel.$setViewValue(latlng.lat() + "," + latlng.lng());
                        });
                        marker.setMap(map);
                    }
                }

            };
        }])

    .directive('trial', ['$rootScope', '$modal', '$http', 'config', '$filter', 'ngToast',
        function ($rootScope, $modal, $http, config, $filter, ngToast) {
            return {
                restrict: 'EA',
                templateUrl: 'views/app/trial/trial-box.html?v=' + version,
                controller: ['$scope',
                    function ($scope) {
                        if (window.host.indexOf("ofisim.com") > -1 || window.host.indexOf("localhost") > -1) {
                            $scope.promotion = {
                                fullName: $rootScope.user.full_name,
                                phoneNumber: $rootScope.user.phone,
                                email: $rootScope.user.email,
                                useCount: "",
                                sector: ""
                            };

                            var toDay = new Date();
                            var userCreateDate = new Date($rootScope.user.created_at);
                            var diff = (toDay - userCreateDate) / 1000;
                            var diff = Math.abs(Math.floor(diff));
                            $scope.day = 15 - Math.floor(diff / (24 * 60 * 60));
                            $scope.isPaid = $rootScope.user.is_paid_customer;
                            $scope.trailMessage = $filter('translate')('Trial.DaysRemainingForYourTrial', { remaining: $scope.day });

                            $scope.sector = [
                                {
                                    label_tr: "Ağaç İşleri, Kağıt ve Kağıt Ürünleri",
                                    label_en: "Woodworking Industry",
                                    value: "Ağaç İşleri, Kağıt ve Kağıt Ürünleri"
                                },
                                {
                                    label_tr: "Banka, Finans",
                                    label_en: "Banking & Finance",
                                    value: "Banka, Finans"
                                },
                                {
                                    label_tr: "Bilişim Teknolojileri",
                                    label_en: "Information Technology",
                                    value: "Bilişim Teknolojileri"
                                },
                                {
                                    label_tr: "Çevre",
                                    label_en: "Environmental",
                                    value: "Çevre"
                                },
                                {
                                    label_tr: "Diğer",
                                    label_en: "Çevre",
                                    value: "Diğer"
                                },
                                {
                                    label_tr: "Eğitim",
                                    label_en: "Education",
                                    value: "Eğitim"
                                },
                                {
                                    label_tr: "Elektrik, Elektronik",
                                    label_en: "Electronics",
                                    value: "Elektrik, Elektronik"
                                },
                                {
                                    label_tr: "Enerji",
                                    label_en: "Energy",
                                    value: "Enerji"
                                },
                                {
                                    label_tr: "Gıda",
                                    label_en: "Food & Beverage",
                                    value: "Gıda"
                                },
                                {
                                    label_tr: "Hukuk Firmaları",
                                    label_en: "Law Firms",
                                    value: "Hukuk Firmaları"
                                },
                                {
                                    label_tr: "İnşaat",
                                    label_en: "Construction",
                                    value: "İnşaat"
                                },
                                {
                                    label_tr: "Kamu Kurumları",
                                    label_en: "Government",
                                    value: "Kamu Kurumları"
                                },
                                {
                                    label_tr: "Kar Amacı Gütmeyen Kurumlar",
                                    label_en: "Non Profit Organizations",
                                    value: "Kar Amacı Gütmeyen Kurumlar"
                                },
                                {
                                    label_tr: "Kimya, Petrol, Lastik ve Plastik",
                                    label_en: "Chemicals",
                                    value: "Enerji"
                                },
                                {
                                    label_tr: "Kültür, Sanat",
                                    label_en: "Çevre",
                                    value: "Kültür, Sanat"
                                },
                                {
                                    label_tr: "Madencilik",
                                    label_en: "Mining",
                                    value: "Madencilik"
                                },
                                {
                                    label_tr: "Medya, İletişim",
                                    label_en: "Media & Press",
                                    value: "Medya, İletişim"
                                },
                                {
                                    label_tr: "Otomotiv",
                                    label_en: "Automotive",
                                    value: "Otomotiv"
                                },
                                {
                                    label_tr: "Perakende",
                                    label_en: "Retail",
                                    value: "Perakende"
                                },
                                {
                                    label_tr: "Sağlık ve Sosyal Hizmetler",
                                    label_en: "Healthcare",
                                    value: "Sağlık ve Sosyal Hizmetler"
                                },
                                {
                                    label_tr: "Tarım, Avcılık, Balıkçılık",
                                    label_en: "Agriculture",
                                    value: "Tarım, Avcılık, Balıkçılık"
                                },
                                {
                                    label_tr: "Tekstil, Hazır Giyim, Deri",
                                    label_en: "Textile",
                                    value: "Tekstil, Hazır Giyim, Deri"
                                },
                                {
                                    label_tr: "Telekomünikasyon",
                                    label_en: "Telecommunication",
                                    value: "Telekomünikasyon"
                                },
                                {
                                    label_tr: "Ticaret (Satış ve Pazarlama)",
                                    label_en: "Sales & Marketing",
                                    value: "Ticaret (Satış ve Pazarlama)"
                                },
                                {
                                    label_tr: "Turizm, Konaklama",
                                    label_en: "Hospitality",
                                    value: "Turizm, Konaklama"
                                },
                                {
                                    label_tr: "Ulaştırma, Lojistik ve Haberleşme",
                                    label_en: "Transportation & Logistics",
                                    value: "Ulaştırma, Lojistik ve Haberleşme"
                                },
                                {
                                    label_tr: "Üretim",
                                    label_en: "Manufacturing",
                                    value: "Üretim"
                                }
                            ];
                            $scope.language = $rootScope.language;
                            $scope.showPromotionModal = function (type) {
                                $scope.trailType = type;
                                $scope.promotionModal = $scope.promotionModal || $modal({
                                    scope: $scope,
                                    templateUrl: '/views/app/trial/promotionFormModal.html',
                                    size: 'modal-sm',
                                    controller: function () {

                                        $scope.sendEmail = function (promotionForm) {
                                            if (promotionForm.$valid) {
                                                var requestMail = {};
                                                if ($scope.trailType == "promotion")
                                                    requestMail.Subject = "Tanıtım İsteği";
                                                else
                                                    requestMail.Subject = "Satın Alma Talebi";

                                                requestMail.TemplateWithBody = '<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"><html xmlns="http://www.w3.org/1999/xhtml" xmlns:v="urn:schemas-microsoft-com:vml" xmlns:o="urn:schemas-microsoft-com:office:office"><head> <title></title> <meta http-equiv="Content-Type" content="text/html; charset=utf-8" /> <style type="text/css"> body, .maintable { height: 100% !important; width: 100% !important; margin: 0; padding: 0; } img, a img { border: 0; outline: none; text-decoration: none; } .imagefix { display: block; } p { margin-top: 0; margin-right: 0; margin-left: 0; padding: 0; } .ReadMsgBody { width: 100%; } .ExternalClass { width: 100%; } .ExternalClass, .ExternalClass p, .ExternalClass span, .ExternalClass font, .ExternalClass td, .ExternalClass div { line-height: 100%; } img { -ms-interpolation-mode: bicubic; } body, table, td, p, a, li, blockquote { -ms-text-size-adjust: 100%; -webkit-text-size-adjust: 100%; } </style> <style type="text/css"> @media only screen and (max-width: 600px) { .rtable { width: 100% !important; table-layout: fixed; } .rtable tr { height: auto !important; display: block; } .contenttd { max-width: 100% !important; display: block; } .contenttd:after { content: ""; display: table; clear: both; } .hiddentds { display: none; } .imgtable, .imgtable table { max-width: 100% !important; height: auto; float: none; margin: 0 auto; } .imgtable.btnset td { display: inline-block; } .imgtable img { width: 100%; height: auto; display: block; } table { float: none; table-layout: fixed; } } </style> <!--[if gte mso 9]><xml> <o:OfficeDocumentSettings> <o:AllowPNG/> <o:PixelsPerInch>96</o:PixelsPerInch> </o:OfficeDocumentSettings></xml><![endif]--></head><body style="overflow: auto; padding:0; margin:0; font-size: 14px; font-family: arial, helvetica, sans-serif; cursor:auto; background-color:#444545"> <table cellspacing="0" cellpadding="0" width="100%" bgcolor="#444545"> <tr> <td style="FONT-SIZE: 0px; HEIGHT: 20px; LINE-HEIGHT: 0"></td> </tr> <tr> <td valign="top"> <table class="rtable" style="WIDTH: 600px; MARGIN: 0px auto" cellspacing="0" cellpadding="0" width="600" align="center" border="0"> <tr> <td class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent"> <table style="WIDTH: 100%" cellspacing="0" cellpadding="0" align="left"> <tr class="hiddentds"> <td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 367px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td> <td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 233px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td> </tr> <tr style="HEIGHT: 10px"> <th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: middle; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 20px; TEXT-ALIGN: left; PADDING-TOP: 20px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 15px; BACKGROUND-COLOR: transparent"></th> <th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: middle; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 20px; TEXT-ALIGN: left; PADDING-TOP: 20px; PADDING-LEFT: 15px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: transparent"></th> </tr> </table> </td> </tr> <tr> <td class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: #feffff"> <table style="WIDTH: 100%" cellspacing="0" cellpadding="0" align="left"> <tr class="hiddentds"> <td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td> </tr> <tr style="HEIGHT: 20px"> <th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: bottom; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 0px; TEXT-ALIGN: left; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: #1296f7"> <p style="FONT-SIZE: 36px; MARGIN-BOTTOM: 1em; FONT-FAMILY: arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #fffeff; LINE-HEIGHT: 36px; BACKGROUND-COLOR: transparent; mso-line-height-rule: exactly" align="center"><br /> ' + requestMail.Subject + '</p> </th> </tr> </table> </td> </tr> <tr> <td class="contenttd" style="BORDER-TOP: #e73d11 5px solid; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: #feffff"> <table style="WIDTH: 100%" cellspacing="0" cellpadding="0" align="left"> <tr class="hiddentds"> <td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly" colspan="2"></td> </tr> <tr style="HEIGHT: 71px"> <th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: middle; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 5px; TEXT-ALIGN: left; PADDING-TOP: 5px; PADDING-LEFT: 15px; BORDER-LEFT: medium none; PADDING-RIGHT: 15px; BACKGROUND-COLOR: transparent" colspan="2"> <div> <p style="FONT-SIZE: 18px; MARGIN-BOTTOM: 1em; FONT-FAMILY: geneve, arial, helvetica, sans-serif; MARGIN-TOP: 0px; COLOR: #2d2d2d; TEXT-ALIGN: justify; PADDING-LEFT: 110px; LINE-HEIGHT: 29px; BACKGROUND-COLOR: transparent; mso-line-height-rule: exactly" align="justify"><strong><br />&#304;sim Soyisim</strong>:' + $scope.promotion.fullName + '<br /> <strong>Telefon Numaras&#305;</strong>:' + $scope.promotion.phoneNumber + '<br /> <strong>E-Posta</strong>: ' + $scope.promotion.email + '<br /> <strong>Uygulamay&#305; Kullanacak Ki&#351;i Say&#305;s&#305;</strong>: ' + $scope.promotion.useCount + '<br /> <strong>Sekt&ouml;r</strong> :' + $scope.promotion.sector.value + '<br /> <strong>M&uuml;&#351;teri Epostas&#305;</strong>: ' + $rootScope.user.email + '</p> </div> </th> </tr> </table> </td> </tr> <tr> <td class="contenttd" style="BORDER-TOP: #e73d11 5px solid; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; PADDING-BOTTOM: 0px; PADDING-TOP: 0px; PADDING-LEFT: 0px; BORDER-LEFT: medium none; PADDING-RIGHT: 0px; BACKGROUND-COLOR: #feffff"> <table style="WIDTH: 100%" cellspacing="0" cellpadding="0" align="left"> <tr class="hiddentds"> <td style="FONT-SIZE: 0px; HEIGHT: 0px; WIDTH: 600px; LINE-HEIGHT: 0; mso-line-height-rule: exactly"></td> </tr> <tr style="HEIGHT: 20px"> <th class="contenttd" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; VERTICAL-ALIGN: top; BORDER-BOTTOM: medium none; FONT-WEIGHT: normal; PADDING-BOTTOM: 20px; TEXT-ALIGN: left; PADDING-TOP: 20px; PADDING-LEFT: 15px; BORDER-LEFT: medium none; PADDING-RIGHT: 15px; BACKGROUND-COLOR: transparent"></th> </tr> </table> </td> </tr> </table> </td> </tr> <tr> <td style="FONT-SIZE: 0px; HEIGHT: 8px; LINE-HEIGHT: 0">&nbsp;</td> </tr> </table> <!-- Created with MailStyler 2.0.1.300 --></body></html>';
                                                requestMail.ToAddresses = ["info@ofisim.com"];
                                                $http.post(config.apiUrl + 'messaging/send_external_email', requestMail).then(function (response) {
                                                    ngToast.create({
                                                        content: $filter('translate')('Trial.RequestMessage'),
                                                        className: 'success',
                                                        timeout: 5000
                                                    });
                                                    $scope.promotionModal.hide();
                                                })
                                            }

                                        }
                                    },
                                    backdrop: 'static',
                                    show: false
                                });
                                $scope.promotionModal.$promise.then($scope.promotionModal.show);
                            };
                            if ($scope.day >= 0 && $scope.day <= 15 && $rootScope.user.id === $rootScope.user.tenant_id && !$scope.isPaid && !$rootScope.preview) {
                                $rootScope.trial = true;
                            }
                        }

                    }]
            };
        }])
    .directive('helpPage', ['$rootScope', '$modal', '$http', 'config', '$filter', 'ngToast', 'HelpService', '$sce', '$cache', '$localStorage',
        function ($rootScope, $modal, $http, config, $filter, ngToast, HelpService, $sce, $cache, $localStorage) {
            return {
                restrict: 'EA',
                scope: {
                    moduleId: '=',
                    route: '=',
                },
                controller: ['$scope',
                    function ($scope) {
                        if ($rootScope.isMobile()) {
                            return false;
                        }
                        $scope.openHelpModal = function () {
                            $scope.helpModal = $scope.helpModal || $modal({
                                scope: $scope,
                                templateUrl: 'views/setup/help/helpPageModal.html',
                                animation: 'am-fade',
                                backdrop: true,
                                show: false,
                                tag: 'helpModal',
                                container: 'body'
                            });

                            $scope.helpModal.$promise.then($scope.helpModal.show);
                        };


                        $scope.selectedClose = true;
                        $scope.selectedCloseModalForRoute = 2;
                        $scope.selectedCloseModalForModule = true;


                        if ($localStorage.read("startPage")) {
                            $scope.startPage = JSON.parse($localStorage.read("startPage"));
                            if ($localStorage.read("routeShow")) {
                                $scope.selectedCloseStartPage = JSON.parse($localStorage.read("routeShow"));
                                var routeShowControl = $filter('filter')($scope.selectedCloseStartPage, { name: $scope.route })[0];
                            }
                            var startPageFilter = $filter('filter')($scope.startPage, { name: $scope.route })[0];
                            if (routeShowControl)
                                $scope.selectedCloseModalForRoute = routeShowControl.value;
                            else if (startPageFilter) {
                                $scope.selectedCloseModalForRoute = startPageFilter.value;
                            }
                        }

                        if ($localStorage.read("moduleShow")) {
                            $scope.selectedCloseModal = JSON.parse($localStorage.read("moduleShow"));
                            var showControl = $filter('filter')($scope.selectedCloseModal, { name: $scope.moduleId })[0];
                            if (showControl) {
                                $scope.selectedCloseModalForModule = showControl.value;
                            }
                        }

                        if ($localStorage.read("routeShow")) {
                            $scope.selectedCloseRoute = JSON.parse($localStorage.read("routeShow"));
                            var routeShowControl = $filter('filter')($scope.selectedCloseRoute, { name: $scope.route })[0];
                            if (routeShowControl) {
                                $scope.selectedCloseModalForRoute = routeShowControl.value;
                            }
                            else {
                                $scope.selectedCloseModalForRoute = 2;
                            }

                        }

                        $scope.openModal = function () {
                            if ($scope.helpTemplatesModal && $scope.helpTemplatesModal.show_type === "publish") {
                                $scope.helpTemplate = $sce.trustAsHtml($scope.helpTemplatesModal.template);

                                if ($localStorage.read('ModalShow')) {

                                    $scope.selectedClose = JSON.parse($localStorage.read('ModalShow'));
                                }

                                if ($scope.selectedClose === true) {
                                    if ($scope.moduleId && $scope.selectedCloseModalForModule) {
                                        $scope.openHelpModal();
                                        if ($localStorage.read("moduleShow")) {
                                            $scope.modalModules = JSON.parse($localStorage.read("moduleShow"));
                                            $scope.modulShowArray = {
                                                name: $scope.moduleId,
                                                value: false
                                            };
                                            var sameModal = $filter('filter')($scope.modalModules, { name: $scope.modulShowArray.name })[0];
                                            if (!sameModal) {
                                                $scope.modalModules.push($scope.modulShowArray);
                                                $localStorage.write("moduleShow", JSON.stringify($scope.modalModules));
                                            }
                                        }
                                        else {
                                            $scope.modalModules = [];
                                            $scope.modulShowArray = {
                                                name: $scope.moduleId,
                                                value: false
                                            };
                                            $scope.modalModules.push($scope.modulShowArray);
                                            $localStorage.write("moduleShow", JSON.stringify($scope.modalModules));
                                        }
                                    }

                                    if ($scope.route && $scope.selectedCloseModalForRoute === 1) {
                                        if ($localStorage.read("startPage")) {
                                            $scope.startPage = JSON.parse($localStorage.read("startPage"));
                                            var startPageFilter = $filter('filter')($scope.startPage, { name: $scope.route })[0];
                                            if (startPageFilter && startPageFilter.value === 1) {
                                                var routes = [];
                                                var routeShow = {
                                                    name: $rootScope.currentPath,
                                                    value: 2
                                                };
                                                routes.push(routeShow);
                                                $localStorage.write("startPage", JSON.stringify(routes));
                                            }
                                        }
                                    }


                                    if ($scope.route && $scope.selectedCloseModalForRoute === 2) {
                                        $scope.openHelpModal();
                                        if ($localStorage.read("routeShow")) {
                                            $scope.routes = JSON.parse($localStorage.read("routeShow"));
                                            $scope.routeShowArray = {
                                                name: $scope.route,
                                                value: 3
                                            };
                                            $scope.routes.push($scope.routeShowArray);
                                            $localStorage.write("routeShow", JSON.stringify($scope.routes));

                                        }
                                        else {
                                            $scope.routes = [];
                                            $scope.routeShowArray = {
                                                name: $scope.route,
                                                value: 3
                                            };
                                            $scope.routes.push($scope.routeShowArray);
                                            $localStorage.write("routeShow", JSON.stringify($scope.routes));
                                        }
                                    }
                                }
                            }
                        };

                        var cacheKey = 'help-';

                        if ($scope.moduleId)
                            cacheKey += '-' + $scope.moduleId;

                        if ($scope.route) {
                            $scope.route.replace('/', '--');
                            cacheKey += $scope.route;

                        }

                        if ($cache.get(cacheKey)) {
                            $scope.helpTemplatesModal = $cache.get(cacheKey);
                            if ($scope.selectedClose) {

                                $scope.openModal();
                            }
                        }


                        if (!$scope.helpTemplatesModal) {
                            HelpService.getByType('modal', $scope.moduleId, $scope.route)
                                .then(function (response) {
                                    if (!response.data)
                                        return;

                                    $scope.helpTemplatesModal = response.data;
                                    $cache.put(cacheKey, response.data);
                                    if ($scope.selectedClose) {

                                        $scope.openModal();
                                    }
                                });
                        }

                        $scope.showModal = function () {

                            if ($scope.moduleId || $scope.route) {
                                $localStorage.write('ModalShow', false);
                            }

                        };


                    }]
            };
        }])

    .directive('zxPasswordMeter', ['$filter', function ($filter) {
        return {
            scope: {
                value: "=",
                max: "@?"
            },
            templateUrl: 'views/common/password-meter.html?v=' + version,
            link: function (scope) {
                scope.type = '';
                scope.max = (!scope.max) ? 4 : scope.max;
                scope.firstRun = true;

                scope.$watch('value.password', function (password) {
                    if (password || password.length > 0 || scope.visible) {
                        scope.visible = true;
                    }
                });

                scope.$watch('value.score', function (newValue) {
                    var strenghPercent = newValue / scope.max;
                    if (strenghPercent === 0) {
                        scope.type = 'progress-bar-danger';
                        scope.text = $filter('translate')('Common.Awful');
                        scope.width = 25;
                    } else if (strenghPercent <= 0.25) {
                        scope.type = 'progress-bar-warning';
                        scope.text = $filter('translate')('Common.Weak');
                        scope.width = 40;
                    } else if (strenghPercent <= 0.50) {
                        scope.type = 'progress-bar-info';
                        scope.text = $filter('translate')('Common.Moderate');
                        scope.width = 50;
                    } else if (strenghPercent <= 0.75) {
                        scope.type = 'progress-bar-info';
                        scope.text = $filter('translate')('Common.Strong');
                        scope.width = 80;
                    } else {
                        scope.type = 'progress-bar-success';
                        scope.text = $filter('translate')('Common.Perfect');
                        scope.width = 100;
                    }

                });
            }
        };
    }])
    .directive('inputStars', [function () {
        function isFloat(n) {
            return Number(n) === n && n % 1 !== 0;
        }

        var directive = {
            restrict: 'EA',
            replace: true,
            template: '<ul ng-class="listClass">' +
            '<li ng-touch="paintStars($index)" ng-mouseenter="paintStars($index, true, $event)" ng-mouseleave="unpaintStars($index, false)" ng-repeat="item in items track by $index">' +
            '<i  ng-class="getClass($index)" ng-click="setValue($index, $event)"></i>' +
            '</li>' +
            '</ul>',
            require: 'ngModel',
            scope: {
                bindModel: '=ngModel'
            },
            link: link
        };

        return directive;

        function link(scope, element, attrs, ngModelCtrl) {
            var computed = {
                get allowHalf() {
                    return typeof attrs.allowHalf == 'string' && attrs.allowHalf != 'false'
                },
                get readonly() {
                    return attrs.readonly != 'false' && (attrs.readonly || attrs.readonly === '');
                },
                get fullIcon() {
                    return attrs.iconFull || 'fa-star';
                },
                get halfIcon() {
                    return attrs.iconHalf || 'fa-star-half-o';
                },
                get emptyIcon() {
                    return attrs.iconEmpty || 'fa-star-o';
                },
                get iconBase() {
                    return attrs.iconBase || 'fa fa-fw';
                },
                get iconHover() {
                    return attrs.iconHover || 'angular-input-stars-hover';
                }
            };

            scope.items = new Array(+attrs.max);
            scope.listClass = attrs.listClass || 'angular-input-stars';

            ngModelCtrl.$render = function () {
                if (isFloat(ngModelCtrl.$viewValue)) {
                    scope.lastValue = (Math.round(parseFloat(ngModelCtrl.$viewValue) * 2) / 2)
                } else {
                    scope.lastValue = parseFloat(ngModelCtrl.$viewValue) || 0;
                }
            };

            scope.getClass = function (index) {
                var icon;

                if (index >= scope.lastValue) {
                    icon = computed.iconBase + ' ' + computed.emptyIcon;
                } else {
                    var isHalf = index + 0.5;
                    if (computed.allowHalf && isHalf === scope.lastValue) {
                        icon = computed.iconBase + ' ' + computed.halfIcon + ' active ';
                    } else {
                        icon = computed.iconBase + ' ' + computed.fullIcon + ' active ';
                    }
                }
                return computed.readonly ? icon + ' readonly' : icon;
            };

            scope.unpaintStars = function ($index, hover) {
                scope.paintStars(scope.lastValue - 1, hover);
            };

            scope.paintStars = function ($index, hover, $event) {
                // ignore painting if readonly
                if (computed.readonly) {
                    return;
                }

                var items = element.find('li').find('i');

                for (var index = 0; index < items.length; index++) {
                    var $star = angular.element(items[index]);
                    var classesToRemove;
                    var classesToAdd;

                    if ($index >= index) {
                        classesToRemove = [computed.emptyIcon, computed.halfIcon]
                        classesToAdd = [computed.iconHover, computed.fullIcon, 'active']
                    } else {
                        classesToRemove = [computed.fullIcon, computed.iconHover, computed.halfIcon, 'active']

                        // isHalf
                        if (computed.allowHalf && $index + 0.5 === index) {
                            classesToAdd = [computed.halfIcon, 'active']
                        } else {
                            classesToAdd = [computed.emptyIcon]
                        }
                    }

                    $star.removeClass(classesToRemove.join(' '));
                    $star.addClass(classesToAdd.join(' '));
                }

                if (!hover) {
                    items.removeClass(computed.iconHover);
                }
            };

            /**
             * Returns whether the user is hovering the first half of the star or not.
             *
             * @param {MouseEvent} e The mouse event.
             * @param {HTMLLIElement} starDOMNode The scope "star" dom node.
             * @returns {boolean}
             */
            function isHoveringFirstHalf(e, starDOMNode) {
                return e.pageX < starDOMNode.getBoundingClientRect().left + starDOMNode.offsetWidth / 2
            }

            scope.setValue = function (index, e) {
                // ignore setting value if readonly
                if (computed.readonly) {
                    return;
                }

                var star = e.target,
                    newValue;

                if (computed.allowHalf && isHoveringFirstHalf(e, star)) {
                    newValue = index + 0.5;
                } else {
                    newValue = index + 1;
                }

                // sets to 0 if the user clicks twice on the first "star"
                // the user should be allowed to give a 0 score
                if (newValue === scope.lastValue) {
                    newValue = 0;
                }

                scope.lastValue = newValue;

                ngModelCtrl.$setViewValue(newValue);
                ngModelCtrl.$render();

                //Execute custom trigger function if there is one
                if (attrs.onStarClick) {
                    try {
                        scope.$parent.$eval(attrs.onStarClick, { $event: e });
                    } catch (e) {
                        console.error(e)
                    }
                }

            };
        }


    }]);


