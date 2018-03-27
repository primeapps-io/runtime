'use strict';

angular.module('ofisim')

    .filter('msDate', ['convert',
        function (convert) {
            return function (time) {
                if (!time)
                    return;

                return convert.fromMsDate(time);
            };
        }])

    .filter('taskDateActive', ['convert', 'helper', '$filter', 'taskDate',
        function (convert, helper, $filter, taskDate) {
            return function (time) {
                if (!time)
                    return;

                var diff = helper.dateDiff(time);
                var now = new Date();
                now.setHours(0, 0, 0, 0);

                if (time < now) {
                    var expired = diff === 0 ? 'ExpiredSingle' : 'Expired';
                    return $filter('translate')('Tasks.' + expired, { days: diff + 1 });
                }
                else if (diff === 0) {
                    return $filter('translate')('Tasks.Today');
                }
                else if (diff === 1) {
                    return $filter('translate')('Tasks.Tomorrow');
                }
                else if (diff < 7) {
                    return $filter('date')(time, 'EEEE');
                }
                else if (time === taskDate.future) {
                    return $filter('translate')('Tasks.Future');
                }
                else {
                    if (now.getYear() === new Date(time).getYear())
                        return $filter('date')(time, 'd MMMM');
                    else
                        return $filter('date')(time, 'd MMMM yyyy');
                }
            };
        }])

    .filter('taskDateCompleted', ['convert', 'helper', '$filter',
        function (convert, helper, $filter) {
            return function (time) {
                if (!time)
                    return;

                var diff = helper.dateDiff(time);
                var now = new Date();
                now.setHours(0, 0, 0, 0);

                if (diff === 0) {
                    return $filter('translate')('Tasks.Today');
                }
                else if (diff === 1) {
                    return $filter('translate')('Tasks.Yesterday');
                }
                else if (diff <= 7) {
                    return $filter('translate')('Tasks.DaysAgo', { days: diff });
                }
                else {
                    if (now.getYear() === new Date(time).getYear())
                        return $filter('date')(time, 'd MMMM');
                    else
                        return $filter('date')(time, 'd MMMM yyyy');
                }
            };
        }])

    .filter('taskDateOptions', ['convert', 'helper', '$filter', 'taskDate',
        function (convert, helper, $filter, taskDate) {
            return function (time) {
                if (!time)
                    return;

                switch (time) {
                    case taskDate.future:
                        return $filter('translate')('Tasks.Future');
                        break;
                    case taskDate.today:
                        return $filter('translate')('Tasks.Today');
                        break;
                    case taskDate.tomorrow:
                        return $filter('translate')('Tasks.Tomorrow');
                        break;
                    case taskDate.nextWeek:
                        return $filter('translate')('Tasks.NextWeek');
                        break;
                    default :
                        return $filter('date')(time, 'd MMMM yyyy');
                        break;
                }
            };
        }])

    .filter('taskLabel', ['convert', 'helper',
        function (convert, helper) {
            return function (time) {
                if (!time)
                    return;

                var diff = helper.dateDiff(time);
                var now = new Date();
                now.setHours(0, 0, 0, 0);

                if (time < now) {
                    return 'danger';
                }
                else if (diff === 0) {
                    return 'warning';
                }
                else if (diff === 1) {
                    return 'success';
                }
                else {
                    return 'info';
                }
            };
        }])

    .filter('roundNumber', function () {
        return function (input, places) {
            if (isNaN(input))
                return input;

            var factor = '1' + new Array(+(places > 0 && places + 1)).join('0');

            return Math.round(input * factor) / factor;
        };
    })

    .filter('size', ['$filter',
        function ($filter) {
            return function (value) {
                if (!value)
                    return '-';

                var kilobyte = 1024;
                var megabyte = kilobyte * 1024;
                var gigabyte = megabyte * 1024;
                var terabyte = gigabyte * 1024;

                if ((value >= 0) && (value < kilobyte)) {
                    return value + ' B';
                }
                else if ((value >= kilobyte) && (value < megabyte)) {
                    return $filter('roundNumber')(value / kilobyte, 1) + ' KB';
                }
                else if ((value >= megabyte) && (value < gigabyte)) {
                    return $filter('roundNumber')(value / megabyte, 1) + ' MB';
                }
                else if ((value >= gigabyte) && (value < terabyte)) {
                    return $filter('roundNumber')(value / gigabyte, 1) + ' GB';
                }
                else if (value >= terabyte) {
                    return $filter('roundNumber')(value / terabyte, 1) + ' TB';
                }
                else {
                    return value + ' B';
                }
            };
        }])

    .filter('relativeTime', ['helper',
        function (helper) {
            return function (time) {
                var dt = moment.utc(time);
                return dt.fromNow();
            };
        }])

    .filter('storagePercent', function () {
        return function (available, used) {
            return Math.round(used * 100 / available);
        }
    })

    .filter('capitalize', ['helper',
        function (helper) {
            return function (value) {
                return helper.capitalize(value);
            }
        }])

    .filter('orderByLabel', function () {
        return function (items, language, reverse) {
            var filtered = angular.copy(items);

            filtered.sort(function (a, b) {
                var aa = a['label_' + language].toUpperCase();
                var bb = b['label_' + language].toUpperCase();

                return aa.localeCompare(bb);
            });

            if (reverse)
                filtered.reverse();

            return filtered;
        };
    })

    .filter('format', function () {
        return function (input) {
            var args = arguments;
            return input.replace(/\{(\d+)\}/g, function (match, capture) {
                return args[1 * capture + 1];
            });
        };
    })

    .filter('mask', function () {
        var cache = {};
        var escChar = '\\';
        var maskDefinitions = {
            '9': /\d/,
            'A': /[a-zA-Z]/,
            '*': /[a-zA-Z0-9]/
        };

        function getPlaceholderChar(i) {
            return '_';
        }

        function processRawMask(mask) {
            if (cache[mask]) return cache[mask];
            var characterCount = 0;
            var maskCaretMap = [];
            var maskPatterns = [];
            var maskPlaceholder = '';
            var minRequiredLength = 0;

            if (angular.isString(mask)) {
                var isOptional = false,
                    numberOfOptionalCharacters = 0,
                    splitMask = mask.split(''),
                    inEscape = false;

                angular.forEach(splitMask, function (chr, i) {
                    if (inEscape) {
                        inEscape = false;
                        maskPlaceholder += chr;
                        characterCount++;
                    }
                    else if (escChar === chr) {
                        inEscape = true;
                    }
                    else if (maskDefinitions[chr]) {
                        maskCaretMap.push(characterCount);

                        maskPlaceholder += getPlaceholderChar(i - numberOfOptionalCharacters);
                        maskPatterns.push(maskDefinitions[chr]);

                        characterCount++;
                        if (!isOptional) {
                            minRequiredLength++;
                        }

                        isOptional = false;
                    }
                    else if (chr === '?') {
                        isOptional = true;
                        numberOfOptionalCharacters++;
                    }
                    else {
                        maskPlaceholder += chr;
                        characterCount++;
                    }
                });
            }

            // Caret position immediately following last position is valid.
            maskCaretMap.push(maskCaretMap.slice().pop() + 1);
            return cache[mask] = { maskCaretMap: maskCaretMap, maskPlaceholder: maskPlaceholder };
        }

        function maskValue(unmaskedValue, maskDef) {
            unmaskedValue = unmaskedValue || '';
            var valueMasked = '',
                maskCaretMapCopy = maskDef.maskCaretMap.slice();

            angular.forEach(maskDef.maskPlaceholder.split(''), function (chr, i) {
                if (unmaskedValue.length && i === maskCaretMapCopy[0]) {
                    valueMasked += unmaskedValue.charAt(0) || '_';
                    unmaskedValue = unmaskedValue.substr(1);
                    maskCaretMapCopy.shift();
                }
                else {
                    valueMasked += chr;
                }
            });
            return valueMasked;

        }

        return function (value, mask) {
            var maskDef = processRawMask(mask);
            var maskedValue = maskValue(value, maskDef);
            return maskedValue;
        };
    })
    .filter('filterWithOr', function ($filter) {
        var comparator = function (actual, expected) {
            if (angular.isUndefined(actual)) {
                return false;
            }
            if ((actual === null) || (expected === null)) {
                return actual === expected;
            }
            if ((angular.isObject(expected) && !angular.isArray(expected)) || (angular.isObject(actual) && !hasCustomToString(actual))) {
                return false;
            }
            actual = ('' + actual).toLowerCase();
            if (angular.isArray(expected)) {
                var match = false;
                expected.forEach(function (e) {
                    e = ('' + e).toLowerCase();
                    if (actual.indexOf(e) !== -1) {
                        match = true;
                    }
                });
                return match;
            } else {
                expected = ('' + expected).toLowerCase();
                return actual.indexOf(expected) !== -1;
            }
        };
        return function (array, expression) {
            return $filter('filter')(array, expression, comparator);
        };
    })

    .filter('formatToMinuteAndSecond', function () {
        return function (input) {
            function z(n) {
                return (n < 10 ? '0' : '') + n;
            }

            var seconds = input % 60;
            var minutes = Math.floor(input / 60);
            var hours = Math.floor(minutes / 60);
            return (z(hours) + ':' + z(minutes) + ':' + z(seconds));
        };
    })

    .filter('orderNotZero', function () {
        return function (items) {
            var filteredItems = [];

            angular.forEach(items, function (item) {
                if (item.order != 0) {
                    filteredItems.push(item);
                }
            });

            return filteredItems;
        }
    })

    .filter('trustUrl', ['$sce', function ($sce) {
        return function (url) {
            return $sce.trustAsResourceUrl(url);
        };
    }]);