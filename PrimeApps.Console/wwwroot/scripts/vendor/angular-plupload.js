/**!
 * AngularJS Plupload directive
 * @author Ibrahim Varol(ibrahim.varol@ofisim.com)
 */

/* global plupload */
(function () {
    'use strict';

    angular.module('angular-plupload', [])
        .provider('pluploadOption', function () {
          /* jshint camelcase: false */
            var opts = {
                flash_swf_url: '/bower_components/plupload/js/Moxie.swf',
                silverlight_xap_url: '/bower_components/plupload/js/Moxie.xap',
                runtimes: 'html5, flash, silverlight, html4',
                max_file_size: '2mb'
            };
            return {
                setOptions: function (newOpts) {
                    angular.extend(opts, newOpts);
                },
                $get: function () {
                    return opts;
                }
            };
        })
        .directive('plupload', [
                '$timeout', 'pluploadOption',
                function ($timeout, pluploadOption) {
                    function lowercaseFirstLetter(string) {
                        return string.charAt(0).toLowerCase() + string.slice(1);
                    }
                    return {
                        scope: {
                            config: '=plupload'
                        },
                        controller: ['$scope', '$element',
                            function ($scope, $element) {
                                var opts = pluploadOption;
                                $scope.files=[];

                              /*Get the file upload element*/
                                opts.browse_button = $element[0].querySelector("[add-file]");

                              /*extend user config*/
                                angular.extend(opts, $scope.config.settings);

                              /*instantiate plupload*/
                                $scope.uploader = new plupload.Uploader(opts);

                                if ($scope.config.events) {

                                    var callbackMethods = ['Init', 'PostInit', 'OptionChanged',
                                        'Refresh', 'StateChanged', 'UploadFile', 'BeforeUpload', 'QueueChanged',
                                        'UploadProgress', 'FilesRemoved', 'FileFiltered', 'FilesAdded',
                                        'FileUploaded', 'ChunkUploaded', 'UploadComplete', 'Error', 'Destroy'];

                                    angular.forEach(callbackMethods, function (method) {

                                        var callback = ($scope.config.events[lowercaseFirstLetter(method)] || angular.noop);

                                        $scope.uploader.bind(method, function () {
                                            callback.apply(this, arguments);
                                            if (!$scope.$$phase && !$scope.$root.$$phase) {
                                                $scope.$apply();
                                            }
                                        });

                                    });
                                }

                              /*initialize plupload*/
                                $scope.uploader.init();
                                $scope.config.uploader = $scope.uploader;
                            }]
                    };
                }
            ]
        );
})();
