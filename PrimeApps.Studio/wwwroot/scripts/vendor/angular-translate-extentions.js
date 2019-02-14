/*!
 * angular-translate - v2.16.0 - 2017-11-01
 * 
 * Copyright (c) 2017 The angular-translate team, Pascal Precht; Licensed MIT
 */
(function (root, factory) {
  if (typeof define === 'function' && define.amd) {
    // AMD. Register as an anonymous module unless amdModuleId is set
    define([], function () {
      return (factory());
    });
  } else if (typeof module === 'object' && module.exports) {
    // Node. Does not work with strict CommonJS, but
    // only CommonJS-like environments that support module.exports,
    // like Node.
    module.exports = factory();
  } else {
    factory();
  }
}(this, function () {

$translateLocalStorageFactory.$inject = ['$window', '$translateCookieStorage'];
angular.module('pascalprecht.translate')

/**
 * @ngdoc object
 * @name pascalprecht.translate.$translateLocalStorage
 * @requires $window
 * @requires $translateCookieStorage
 *
 * @description
 * Abstraction layer for localStorage. This service is used when telling angular-translate
 * to use localStorage as storage.
 *
 */
.factory('$translateLocalStorage', $translateLocalStorageFactory);

function $translateLocalStorageFactory($window, $translateCookieStorage) {

  'use strict';

  // Setup adapter
  var localStorageAdapter = (function(){
    var langKey;
    return {
      /**
       * @ngdoc function
       * @name pascalprecht.translate.$translateLocalStorage#get
       * @methodOf pascalprecht.translate.$translateLocalStorage
       *
       * @description
       * Returns an item from localStorage by given name.
       *
       * @param {string} name Item name
       * @return {string} Value of item name
       */
      get: function (name) {
        if(!langKey) {
          langKey = $window.localStorage.getItem(name);
        }

        return langKey;
      },
      /**
       * @ngdoc function
       * @name pascalprecht.translate.$translateLocalStorage#set
       * @methodOf pascalprecht.translate.$translateLocalStorage
       *
       * @description
       * Sets an item in localStorage by given name.
       *
       * @deprecated use #put
       *
       * @param {string} name Item name
       * @param {string} value Item value
       */
      set: function (name, value) {
        langKey=value;
        $window.localStorage.setItem(name, value);
      },
      /**
       * @ngdoc function
       * @name pascalprecht.translate.$translateLocalStorage#put
       * @methodOf pascalprecht.translate.$translateLocalStorage
       *
       * @description
       * Sets an item in localStorage by given name.
       *
       * @param {string} name Item name
       * @param {string} value Item value
       */
      put: function (name, value) {
        langKey=value;
        $window.localStorage.setItem(name, value);
      }
    };
  }());

  var hasLocalStorageSupport = 'localStorage' in $window;
  if (hasLocalStorageSupport) {
    var testKey = 'pascalprecht.translate.storageTest';
    try {
      // this check have to be wrapped within a try/catch because on
      // a SecurityError: Dom Exception 18 on iOS
      if ($window.localStorage !== null) {
        $window.localStorage.setItem(testKey, 'foo');
        $window.localStorage.removeItem(testKey);
        hasLocalStorageSupport = true;
      } else {
        hasLocalStorageSupport = false;
      }
    } catch (e){
      hasLocalStorageSupport = false;
    }
  }
  var $translateLocalStorage = hasLocalStorageSupport ? localStorageAdapter : $translateCookieStorage;
  return $translateLocalStorage;
}

$translateLocalStorageFactory.displayName = '$translateLocalStorageFactory';
return 'pascalprecht.translate';

}));

/*!
 * angular-translate - v2.16.0 - 2017-11-01
 * 
 * Copyright (c) 2017 The angular-translate team, Pascal Precht; Licensed MIT
 */
(function (root, factory) {
  if (typeof define === 'function' && define.amd) {
    // AMD. Register as an anonymous module unless amdModuleId is set
    define([], function () {
      return (factory());
    });
  } else if (typeof module === 'object' && module.exports) {
    // Node. Does not work with strict CommonJS, but
    // only CommonJS-like environments that support module.exports,
    // like Node.
    module.exports = factory();
  } else {
    factory();
  }
}(this, function () {

$translateStaticFilesLoader.$inject = ['$q', '$http'];
angular.module('pascalprecht.translate')
/**
 * @ngdoc object
 * @name pascalprecht.translate.$translateStaticFilesLoader
 * @requires $q
 * @requires $http
 *
 * @description
 * Creates a loading function for a typical static file url pattern:
 * "lang-en_US.json", "lang-de_DE.json", etc. Using this builder,
 * the response of these urls must be an object of key-value pairs.
 *
 * @param {object} options Options object, which gets prefix, suffix, key, and fileMap
 */
.factory('$translateStaticFilesLoader', $translateStaticFilesLoader);

function $translateStaticFilesLoader($q, $http) {

  'use strict';

  return function (options) {

    if (!options || (!angular.isArray(options.files) && (!angular.isString(options.prefix) || !angular.isString(options.suffix)))) {
      throw new Error('Couldn\'t load static files, no files and prefix or suffix specified!');
    }

    if (!options.files) {
      options.files = [{
        prefix: options.prefix,
        suffix: options.suffix
      }];
    }

    var load = function (file) {
      if (!file || (!angular.isString(file.prefix) || !angular.isString(file.suffix))) {
        throw new Error('Couldn\'t load static file, no prefix or suffix specified!');
      }

      var fileUrl = [
        file.prefix,
        options.key,
        file.suffix
      ].join('');

      if (angular.isObject(options.fileMap) && options.fileMap[fileUrl]) {
        fileUrl = options.fileMap[fileUrl];
      }

      return $http(angular.extend({
        url: fileUrl,
        method: 'GET'
      }, options.$http))
        .then(function(result) {
          return result.data;
        }, function () {
          return $q.reject(options.key);
        });
    };

    var promises = [],
        length = options.files.length;

    for (var i = 0; i < length; i++) {
      promises.push(load({
        prefix: options.files[i].prefix,
        key: options.key,
        suffix: options.files[i].suffix
      }));
    }

    return $q.all(promises)
      .then(function (data) {
        var length = data.length,
            mergedData = {};

        for (var i = 0; i < length; i++) {
          for (var key in data[i]) {
            mergedData[key] = data[i][key];
          }
        }

        return mergedData;
      });
  };
}

$translateStaticFilesLoader.displayName = '$translateStaticFilesLoader';
return 'pascalprecht.translate';

}));

/*!
 * angular-translate - v2.16.0 - 2017-11-01
 * 
 * Copyright (c) 2017 The angular-translate team, Pascal Precht; Licensed MIT
 */
(function (root, factory) {
  if (typeof define === 'function' && define.amd) {
    // AMD. Register as an anonymous module unless amdModuleId is set
    define([], function () {
      return (factory());
    });
  } else if (typeof module === 'object' && module.exports) {
    // Node. Does not work with strict CommonJS, but
    // only CommonJS-like environments that support module.exports,
    // like Node.
    module.exports = factory();
  } else {
    factory();
  }
}(this, function () {

$translateCookieStorageFactory.$inject = ['$injector'];
angular.module('pascalprecht.translate')

/**
 * @ngdoc object
 * @name pascalprecht.translate.$translateCookieStorage
 * @requires $cookieStore
 *
 * @description
 * Abstraction layer for cookieStore. This service is used when telling angular-translate
 * to use cookieStore as storage.
 *
 */
  .factory('$translateCookieStorage', $translateCookieStorageFactory);

function $translateCookieStorageFactory($injector) {

  'use strict';

  // Since AngularJS 1.4, $cookieStore is deprecated
  var delegate;
  if (angular.version.major === 1 && angular.version.minor >= 4) {
    var $cookies = $injector.get('$cookies');
    delegate = {
      get : function (key) {
        return $cookies.get(key);
      },
      put : function (key, value) {
        $cookies.put(key, value);
      }
    };
  } else {
    var $cookieStore = $injector.get('$cookieStore');
    delegate = {
      get : function (key) {
        return $cookieStore.get(key);
      },
      put : function (key, value) {
        $cookieStore.put(key, value);
      }
    };
  }

  var $translateCookieStorage = {

    /**
     * @ngdoc function
     * @name pascalprecht.translate.$translateCookieStorage#get
     * @methodOf pascalprecht.translate.$translateCookieStorage
     *
     * @description
     * Returns an item from cookieStorage by given name.
     *
     * @param {string} name Item name
     * @return {string} Value of item name
     */
    get : function (name) {
      return delegate.get(name);
    },

    /**
     * @ngdoc function
     * @name pascalprecht.translate.$translateCookieStorage#set
     * @methodOf pascalprecht.translate.$translateCookieStorage
     *
     * @description
     * Sets an item in cookieStorage by given name.
     *
     * @deprecated use #put
     *
     * @param {string} name Item name
     * @param {string} value Item value
     */
    set : function (name, value) {
      delegate.put(name, value);
    },

    /**
     * @ngdoc function
     * @name pascalprecht.translate.$translateCookieStorage#put
     * @methodOf pascalprecht.translate.$translateCookieStorage
     *
     * @description
     * Sets an item in cookieStorage by given name.
     *
     * @param {string} name Item name
     * @param {string} value Item value
     */
    put : function (name, value) {
      delegate.put(name, value);
    }
  };

  return $translateCookieStorage;
}

$translateCookieStorageFactory.displayName = '$translateCookieStorage';
return 'pascalprecht.translate';

}));