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
                                contentTemplate: 'view/common/confirm.html' + '?v=' + version
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

    .directive('subTable', ['$rootScope', 'ngTableParams', 'blockUI', '$filter', '$cache', 'helper', 'exportFile', 'operations', 'ModuleService',
        function ($rootScope, ngTableParams, blockUI, $filter, $cache, helper, exportFile, operations) {
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
                templateUrl: 'view/common/subtable.html?v=' + version,
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
                        $scope.isAdmin = $rootScope.user.profile.has_admin_rights;

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
                            ModuleService.getRecord($scope.module.name, id)
                                .then(function (recordResponse) {
                                    var record = ModuleService.processRecordSingle(recordResponse.data, $scope.$parent.$parent.module, $scope.$parent.$parent.picklistsModule);
                                    $scope.executeCode = false;
                                    components.run('BeforeDelete', 'Script', $scope, record);

                                    if ($scope.executeCode)
                                        return;

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
                                    if (field.primary == true && !field.isJoin) {
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
                                    toastr.warning($filter('translate')('Module.ExportUnsupported'));
                                    return;
                                }

                                if ($scope.tableParams.total() > 3000) {
                                    toastr.warning($filter('translate')('Module.ExportWarning'));
                                    return;
                                }

                                var fileName = $scope.module['label_' + $rootScope.language + '_plural'] + '-' + $filter('date')(new Date(), 'dd-MM-yyyy') + '.xls';
                                $scope.exporting = true;

                                ModuleService.getCSVData($scope, $scope.type)
                                    .then(function (csvData) {
                                        toastr.warning($filter('translate')('Module.ExcelExportSuccess'));

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
                                {
                                    title: ($rootScope.language === 'tr' ? 'Çok Büyük' : 'Very Big'),
                                    block: 'h2',
                                    styles: { fontWeight: 'normal' }
                                },
                                {
                                    title: ($rootScope.language === 'tr' ? 'Büyük' : 'Big'),
                                    block: 'h3',
                                    styles: { fontWeight: 'normal' }
                                },
                                {
                                    title: ($rootScope.language === 'tr' ? 'Normal' : 'Normal'),
                                    block: 'h4',
                                    styles: { fontWeight: 'normal' }
                                },
                                {
                                    title: ($rootScope.language === 'tr' ? 'Küçük' : 'Small'),
                                    block: 'h5',
                                    styles: { fontWeight: 'normal' }
                                },
                                {
                                    title: ($rootScope.language === 'tr' ? 'Çok Küçük' : 'Very Small'),
                                    block: 'h6',
                                    styles: { fontWeight: 'normal' }
                                }
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


    .directive('zxPasswordMeter', ['$filter', function ($filter) {
        return {
            scope: {
                value: "=",
                max: "@?"
            },
            templateUrl: 'view/common/password-meter.html?v=' + version,
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

