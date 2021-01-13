'use strict';

angular.module('primeapps')

    .factory('genericInterceptor', ['$q', '$injector', '$window', '$localStorage', '$filter', '$cookies', '$rootScope',
        function ($q, $injector, $window, $localStorage, $filter, $cookies, $rootScope) {
            return {
                request: function (config) {
                    config.headers = config.headers || {};
                    var accessToken = $localStorage.read('access_token');

                    if ((blobUrl && config.url.indexOf(blobUrl) > -1) || (containerDomain && config.url.indexOf(containerDomain) > -1) || (routeTemplateUrls && routeTemplateUrls.length > 0 && routeTemplateUrls.indexOf(config.url) > -1))
                        config.headers['Access-Control-Allow-Origin'] = '*';

                    if (accessToken && config.url.indexOf('/token') < 0 && (blobUrl === '' || config.url.indexOf(blobUrl) < 0))
                        config.headers['Authorization'] = 'Bearer ' + accessToken;

                    var appId = $cookies.get(preview ? 'preview_app_id' : 'app_id');
                    var tenantId = $cookies.get(preview ? 'preview_tenant_id' : 'tenant_id');

                    config.headers['X-App-Id'] = appId;
                    config.headers['X-Tenant-Id'] = tenantId;
                    config.headers['X-User-Id'] = account.user.id;

                    if (trustedUrls.length > 0) {
                        var getValue = function (key) {
                            switch (key) {

                                case 'X-Auth-Key':
                                case 'x-auth-key':
                                    return encryptedUserId;
                                case 'X-Branch-Id':
                                case 'x-branch-id':
                                case 'branch_id':
                                    return $rootScope.branchAvailable ? $rootScope.user.branchId : '';
                                case 'X-Tenant-Language':
                                case 'x-tenant-language':
                                    return tenantLanguage;
                                case 'X-User-Id':
                                case 'x-user-id':
                                case 'user_id':
                                    return account.user.id;
                                case 'X-Tenant-Id':
                                case 'x-tenant-id':
                                case 'tenant_id':
                                    return preview ? applicationId : tenantId;
                                case 'X-App-Id':
                                case 'x-app-id':
                                case 'app_id':
                                    return applicationId;
                            }
                        };

                        for (var i = 0; i < trustedUrls.length; i++) {
                            var trustedUrl = trustedUrls[i];
                            if (config.url.indexOf(trustedUrl.url) > -1) {
                                if (trustedUrl["headers"]) {
                                    angular.forEach(trustedUrl["headers"], function (headerObjValue, headerObjKey) {
                                        if (headerObjValue.indexOf("::dynamic") > -1) {
                                            config.headers[headerObjKey] = getValue(headerObjKey);
                                        }
                                        else {
                                            config.headers[headerObjKey] = headerObjValue;
                                        }
                                    });
                                }
                                config.headers['Access-Control-Allow-Origin'] = '*';
                            }
                        }
                    }

                    return config;
                },
                responseError: function (rejection) {
                    var message = '';
                    if (rejection.status === 401) {
                        if (rejection.config.url.indexOf('/token') > -1)
                            return $q.reject(rejection);

                        if (rejection.statusText === 'Unauthorized') {
                            $localStorage.remove('access_token');
                            $localStorage.remove('refresh_token');
                            $window.location.href = '/logout';
                        }
                        else {
                            $window.location.href = '/';
                        }

                        return;
                    }

                    if (rejection.status === 500 && rejection.config.url.indexOf('/User/MyAccount') > -1) {
                        $localStorage.remove('access_token');
                        $localStorage.remove('refresh_token');

                        $window.location.href = '/logout';

                        return $q.reject(rejection);
                    }

                    if (rejection.status === 402) {
                        $window.location.href = '#/paymentform';
                        return $q.reject(rejection);
                    }

                    if (rejection.status === 403) {
                        $window.location.href = '#/app/dashboard';

                        message = 'Common.Forbidden';
                        $rootScope.$broadcast('error', message);
                        // $mdToast.error($filter('translate')('Common.Forbidden'))
                        return $q.reject(rejection);
                    }

                    if (rejection.status === 404) {
                        if (!rejection.config.ignoreNotFound) {
                            $window.location.href = '#/app/dashboard';
                            message = rejection.config.url.indexOf('/module') > -1 ? 'Common.NotFoundRecord' : 'Common.NotFound';
                            $rootScope.$broadcast('error', message);
                            //$mdToast.warning( $filter('translate')(rejection.config.url.indexOf('/module') > -1 ? 'Common.NotFoundRecord' : 'Common.NotFound'));
                        }

                        return $q.reject(rejection);
                    }

                    if (!navigator.onLine || rejection.status === 421 || rejection.status === 429) {
                        message = 'Common.NetworkError';
                        $rootScope.$broadcast('error', message);
                        //$mdToast.warning( $filter('translate')('Common.NetworkError'));

                        return $q.reject(rejection);
                    }

                    if (rejection.status === 400 || rejection.status === 409) {
                        return $q.reject(rejection);
                    }

                    message = 'Common.Error';
                    $rootScope.$broadcast('error', message);
                    //$mdToast.warning( $filter('translate')('Common.Error'));

                    return $q.reject(rejection);
                }
            }
        }])

    .factory('dimensions', function () {

        var fn = {};

        /**
         * Test the element nodeName
         * @param element
         * @param name
         */
        var nodeName = fn.nodeName = function (element, name) {
            return element.nodeName && element.nodeName.toLowerCase() === name.toLowerCase();
        };

        /**
         * Returns the element computed style
         * @param element
         * @param prop
         * @param extra
         */
        fn.css = function (element, prop, extra) {
            var value;
            if (element.currentStyle) { // IE
                value = element.currentStyle[prop];
            }
            else if (window.getComputedStyle) {
                value = window.getComputedStyle(element)[prop];
            }
            else {
                value = element.style[prop];
            }
            return extra === true ? parseFloat(value) || 0 : value;
        };

        /**
         * Provides read-only equivalent of jQuery's offset function:
         * @required-by bootstrap-tooltip, bootstrap-affix
         * @url http://api.jquery.com/offset/
         * @param element
         */
        fn.offset = function (element) {
            var boxRect = element.getBoundingClientRect();
            var docElement = element.ownerDocument;
            return {
                width: boxRect.width || element.offsetWidth,
                height: boxRect.height || element.offsetHeight,
                top: boxRect.top + (window.pageYOffset || docElement.documentElement.scrollTop) - (docElement.documentElement.clientTop || 0),
                left: boxRect.left + (window.pageXOffset || docElement.documentElement.scrollLeft) - (docElement.documentElement.clientLeft || 0)
            };
        };

        /**
         * Provides set equivalent of jQuery's offset function:
         * @required-by bootstrap-tooltip
         * @url http://api.jquery.com/offset/
         * @param element
         * @param options
         * @param i
         */
        fn.setOffset = function (element, options, i) {
            var curPosition;
            var curLeft;
            var curCSSTop;
            var curTop;
            var curOffset;
            var curCSSLeft;
            var calculatePosition;
            var position = fn.css(element, 'position');
            var curElem = angular.element(element);
            var props = {};

            // Set position first, in-case top/left are set even on static elem
            if (position === 'static') {
                element.style.position = 'relative';
            }

            curOffset = fn.offset(element);
            curCSSTop = fn.css(element, 'top');
            curCSSLeft = fn.css(element, 'left');
            calculatePosition = (position === 'absolute' || position === 'fixed') &&
                (curCSSTop + curCSSLeft).indexOf('auto') > -1;

            // Need to be able to calculate position if either
            // top or left is auto and position is either absolute or fixed
            if (calculatePosition) {
                curPosition = fn.position(element);
                curTop = curPosition.top;
                curLeft = curPosition.left;
            }
            else {
                curTop = parseFloat(curCSSTop) || 0;
                curLeft = parseFloat(curCSSLeft) || 0;
            }

            if (angular.isFunction(options)) {
                options = options.call(element, i, curOffset);
            }

            if (options.top !== null) {
                props.top = (options.top - curOffset.top) + curTop;
            }
            if (options.left !== null) {
                props.left = (options.left - curOffset.left) + curLeft;
            }

            if ('using' in options) {
                options.using.call(curElem, props);
            }
            else {
                curElem.css({
                    top: props.top + 'px',
                    left: props.left + 'px'
                });
            }
        };

        /**
         * Provides read-only equivalent of jQuery's position function
         * @required-by bootstrap-tooltip, bootstrap-affix
         * @url http://api.jquery.com/offset/
         * @param element
         */
        fn.position = function (element) {

            var offsetParentRect = {top: 0, left: 0};
            var offsetParentEl;
            var offset;

            // Fixed elements are offset from window (parentOffset = {top:0, left: 0}, because it is it's only offset parent
            if (fn.css(element, 'position') === 'fixed') {

                // We assume that getBoundingClientRect is available when computed position is fixed
                offset = element.getBoundingClientRect();

            }
            else {

                // Get *real* offsetParentEl
                offsetParentEl = offsetParentElement(element);

                // Get correct offsets
                offset = fn.offset(element);
                if (!nodeName(offsetParentEl, 'html')) {
                    offsetParentRect = fn.offset(offsetParentEl);
                }

                // Add offsetParent borders
                offsetParentRect.top += fn.css(offsetParentEl, 'borderTopWidth', true);
                offsetParentRect.left += fn.css(offsetParentEl, 'borderLeftWidth', true);
            }

            // Subtract parent offsets and element margins
            return {
                width: element.offsetWidth,
                height: element.offsetHeight,
                top: offset.top - offsetParentRect.top - fn.css(element, 'marginTop', true),
                left: offset.left - offsetParentRect.left - fn.css(element, 'marginLeft', true)
            };

        };

        /**
         * Returns the closest, non-statically positioned offsetParent of a given element
         * @required-by fn.position
         * @param element
         */
        function offsetParentElement(element) {
            var docElement = element.ownerDocument;
            var offsetParent = element.offsetParent || docElement;
            if (nodeName(offsetParent, '#document')) return docElement.documentElement;
            while (offsetParent && !nodeName(offsetParent, 'html') && fn.css(offsetParent, 'position') === 'static') {
                offsetParent = offsetParent.offsetParent;
            }
            return offsetParent || docElement.documentElement;
        }

        /**
         * Provides equivalent of jQuery's height function
         * @required-by bootstrap-affix
         * @url http://api.jquery.com/height/
         * @param element
         * @param outer
         */
        fn.height = function (element, outer) {
            var value = element.offsetHeight;
            if (outer) {
                value += fn.css(element, 'marginTop', true) + fn.css(element, 'marginBottom', true);
            }
            else {
                value -= fn.css(element, 'paddingTop', true) + fn.css(element, 'paddingBottom', true) + fn.css(element, 'borderTopWidth', true) + fn.css(element, 'borderBottomWidth', true);
            }
            return value;
        };

        /**
         * Provides equivalent of jQuery's width function
         * @required-by bootstrap-affix
         * @url http://api.jquery.com/width/
         * @param element
         * @param outer
         */
        fn.width = function (element, outer) {
            var value = element.offsetWidth;
            if (outer) {
                value += fn.css(element, 'marginLeft', true) + fn.css(element, 'marginRight', true);
            }
            else {
                value -= fn.css(element, 'paddingLeft', true) + fn.css(element, 'paddingRight', true) + fn.css(element, 'borderLeftWidth', true) + fn.css(element, 'borderRightWidth', true);
            }
            return value;
        };

        return fn;

    })

    .factory('PriorityNavService', ['$timeout', '$window', '$document', '$rootScope', function ($timeout, $window, $document, $rootScope) {

        var
            service = {};

        // from underscore.js
        // Returns a function, that, as long as it continues to be invoked, will not
        // be triggered. The function will be called after it stops being called for
        // N milliseconds. If `immediate` is passed, trigger the function on the
        // leading edge, instead of the trailing.
        service.debounce = function (func, wait, immediate) {
            var timeout, args, context, timestamp, result;

            var later = function () {
                var last = service.now() - timestamp;

                if (last < wait && last >= 0) {
                    timeout = setTimeout(later, wait - last);
                }
                else {
                    timeout = null;
                    if (!immediate) {
                        result = func.apply(context, args);
                        if (!timeout) context = args = null;
                    }
                }
            };

            return function () {
                context = this;
                args = arguments;
                timestamp = service.now();
                var callNow = immediate && !timeout;
                if (!timeout) timeout = setTimeout(later, wait);
                if (callNow) {
                    result = func.apply(context, args);
                    context = args = null;
                }

                return result;
            };
        };

        service.now = Date.now || function () {
            return new Date().getTime();
        };

        service.style = function (element, styleName) {
            var style = element.currentStyle || window.getComputedStyle(element);
            return style[styleName].replace('px', '');
        };

        service.getMargins = function (element) {
            return parseInt(service.style(element, 'marginRight') || 0) + parseInt(service.style(element, 'paddingLeft') || 0);
        };

        service.getWidth = function (element, getNaturalWidth) {
            var width;
            if (getNaturalWidth) { //get natural/auto width
                var originalWidth = element.style.cssText;
                element.style.cssText = 'width:auto !important; display:inline-block !important;';
                width = element.getBoundingClientRect().width + service.getMargins(element);
                element.style.cssText = originalWidth;
            }
            else {
                width = element.getBoundingClientRect().width + service.getMargins(element);
            }
            return width;
        };

        service.addIds = function (children) {
            angular.forEach(children, function (child, key) {
                if (!angular.element(child).hasClass('vertical-nav')) {
                    angular.element(child).attr('data-priority-nav-index', key + 1);
                }
            });
        };

        service.getBreakPoint = function (children, breakPoint, horizontalNavWidth) {
            for (var i = 0; children.length > i; i++) { //go through horizontal items
                breakPoint += service.getWidth(children[i], true);
                if (breakPoint > horizontalNavWidth) {
                    break;
                }
            }
            return {
                breakPoint: breakPoint,
                breakPointIndex: i
            };
        };

        service.sortChildrenAndAppend = function (wrapperElem) {
            var children = wrapperElem.children();
            children.sort(function (a, b) {
                return parseInt(angular.element(a).attr('data-priority-nav-index')) > parseInt(angular.element(b).attr('data-priority-nav-index')) ? 1 : -1
            });
            for (var i = 0; i < children.length; ++i) {
                wrapperElem.append(children[i]);
            }
        };

        service.calculatebreakPoint = function (horizontalNav, verticalNav, verticalNavDropDown, verticalNavMoreLinkBubble) {
            if (horizontalNav.children().length > 0) {
                var horizontalNavWidth = service.getWidth(horizontalNav[0]), //width of whole nav
                    hBreakIndex = service.getBreakPoint(horizontalNav.children(), service.getWidth(verticalNav[0]), horizontalNavWidth),
                    vBreakIndex = service.getBreakPoint(verticalNavDropDown.children(), hBreakIndex.breakPoint, horizontalNavWidth),
                    breakIndex = (hBreakIndex.breakPointIndex + vBreakIndex.breakPointIndex < (horizontalNav.children().length + verticalNavDropDown.children().length)) ?
                        hBreakIndex.breakPointIndex + vBreakIndex.breakPointIndex - 1 : //minus 1 for more link
                        hBreakIndex.breakPointIndex + vBreakIndex.breakPointIndex;
                angular.forEach(horizontalNav.children(), function (childElem) {
                    if (angular.element(childElem).attr('data-priority-nav-index') && parseInt(angular.element(childElem).attr('data-priority-nav-index')) > breakIndex) {
                        verticalNavDropDown.append(angular.element(childElem));
                    }
                });
                angular.forEach(verticalNavDropDown.children(), function (childElem) {
                    if (angular.element(childElem).attr('data-priority-nav-index') && parseInt(angular.element(childElem).attr('data-priority-nav-index')) <= breakIndex) {
                        horizontalNav.append(angular.element(childElem));
                    }
                });

                service.sortChildrenAndAppend(verticalNavDropDown);
                service.sortChildrenAndAppend(horizontalNav);

                horizontalNav.append(verticalNav); //append it again, so that it is the last item
                if (verticalNavDropDown.children().length > 0) { // if we have vertical items (they dont all fit in horizontal menu)
                    verticalNav.removeClass('go-away'); //show it
                    verticalNavMoreLinkBubble.text((verticalNavDropDown.children().length > 9) ? '9+' : verticalNavDropDown.children().length); //add count
                }
                else { //if we have no vertical items (they all fit in horizontal menu)
                    verticalNav.addClass('go-away');
                    verticalNavMoreLinkBubble.text(''); //add count
                }
            }
        };

        return service;
    }]);
