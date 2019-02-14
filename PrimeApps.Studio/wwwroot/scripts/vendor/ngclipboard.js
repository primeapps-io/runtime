/*! ngclipboard - v1.1.1 - 2016-02-26
* https://github.com/sachinchoolur/ngclipboard
* Copyright (c) 2016 Sachin; Licensed MIT */
(function () {
    'use strict';
    var MODULE_NAME = 'ngclipboard';
    var angular, Clipboard;

    // Check for CommonJS support
    if (typeof module === 'object' && module.exports) {
        angular = require('angular');
        Clipboard = require('clipboard');
        module.exports = MODULE_NAME;
    } else {
        angular = window.angular;
        Clipboard = window.Clipboard;
    }

    angular.module(MODULE_NAME, []).directive('ngclipboard', function () {
        return {
            restrict: 'A',
            scope: {
                ngclipboardSuccess: '&',
                ngclipboardError: '&'
            },
            link: function (scope, element) {
                var clipboard = new Clipboard(element[0]);

                clipboard.on('success', function (e) {
                    scope.$apply(function () {
                        setTooltip(e.trigger, 'Copied!');
                        hideTooltip(e.trigger);
                        scope.ngclipboardSuccess({
                            e: e
                        });
                    });
                });

                clipboard.on('error', function (e) {
                    scope.$apply(function () {
                        setTooltip(e.trigger, 'Failed!');
                        hideTooltip(e.trigger);
                        scope.ngclipboardError({
                            e: e
                        });
                    });
                });

                function setTooltip(btn, message) {
                    $(btn)
                        .attr('data-original-title', message)
                        .tooltip('show');
                };

                function hideTooltip(btn) {
                    setTimeout(function () {
                        $(btn).tooltip('hide')
                            .attr('data-original-title', "");
                    }, 1000);
                };

                // Tooltip
                $(element[0]).tooltip({
                    trigger: 'click'
                });
            }
        };
    });
}());
