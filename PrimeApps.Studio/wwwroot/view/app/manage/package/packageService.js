'use strict';

angular.module('primeapps')

	.factory('PackageService', ['$rootScope', '$http', 'config', '$filter', '$q', 'ModuleService',
		function ($rootScope, $http, config, $filter, $q, ModuleService) {
			return {
				create: function (data) {
					return $http.post(config.apiUrl + 'package/create', data);
				},
				count: function () {
					return $http.get(config.apiUrl + 'package/count');
				},
				find: function (request) {
					return $http.post(config.apiUrl + 'package/find', request);
				},
				get: function (id) {
					return $http.get(config.apiUrl + 'package/get/' + id);
				},
				log: function (id) {
					return $http.get(config.apiUrl + 'package/log/' + id);
				},
				getActiveProcess: function () {
					return $http.get(config.apiUrl + 'package/get_active_process');
				},
				getErrorText: function (errorList, packageModules) {

					var message = "";
					var copyErrorList = angular.copy(errorList);

					for (var i = 0; i < copyErrorList.length; i++) {
						var item = copyErrorList[i];
						message += item.module["label_" + $rootScope.language + "_plural"] + " has a lookup field in ";

						var modules = $filter('filter')(copyErrorList, function (error) {
							return error.module.name === item.module.name;
						});

						for (var j = 0; j < modules.length; j++) {
							var relatedModule = $filter('filter')(packageModules, { name: modules[j].lookup_type }, true)[0];
							message += relatedModule["label_" + $rootScope.language + "_plural"] + (j === modules.length - 1 ? ". <br/>" : ", ");
							var errorModule = $filter('filter')(copyErrorList, function (error) {
								return error.module.name === modules[j].module.name;
							})[0];
							var index = copyErrorList.indexOf(errorModule);
							if (index > -1)
								copyErrorList.splice(index, 1);
						}
						i--;
					}

					return message;
				},
				checkModules: function (selectedModules, errorList, packageModulesRelations) {

					if (selectedModules.length === 0)
						errorList = [];
					else {
						for (var i = 0; i < selectedModules.length; i++) {

							var module = selectedModules[i];
							var lookupList = packageModulesRelations[module.name];

							if (lookupList) {
								for (var o = 0; o < lookupList.length; o++) {

									var index = -1;
									var relatedModuleIndex = -1;
									var isExistModule = $filter('filter')(selectedModules, { name: lookupList[o].lookup_type }, true)[0];
									var relatedModule = $filter('filter')(errorList, function (error) {
										return error.lookup_type === lookupList[o].lookup_type && error.module.name === lookupList[o].module.name;
									})[0];

									if (relatedModule)
										relatedModuleIndex = errorList.indexOf(relatedModule);

									index = lookupList.indexOf(lookupList[o]);
									//if we didn't add that, we will add that in this case
									if (!isExistModule && index === -1) {
										errorList.push({
											name: lookupList[o].name,
											lookup_type: lookupList[o].lookup_type,
											module: lookupList[o].module
										});
									}
									//Seçilen moduller arasında olmayıp,lookup olanı ekliyoruz
									else if (!isExistModule && index > -1 && relatedModuleIndex === -1) {
										errorList.push({
											name: lookupList[o].name,
											lookup_type: lookupList[o].lookup_type,
											module: lookupList[o].module
										});
									}
									//if we added that before we have to splice that from array 
									else if (isExistModule && index > -1 && relatedModuleIndex > -1) {
										errorList.splice(relatedModuleIndex, 1);
									}
								}
							}

							for (var j = 0; j < errorList.length; j++) {
								var isExistInSelectedModules = $filter('filter')(selectedModules, { name: errorList[j].module.name }, true)[0];
								if (!isExistInSelectedModules) {
									errorList.splice(j, 1);
									j--;
								}
							}
						}
					}
					return errorList;
				},
				getModulesFields: function (packageModules, packageModulesRelations, allModulesRelations) {
					angular.forEach(packageModules, function (module) {
						if (!module.fields)
							ModuleService.getModuleFields(module.name).then(function (response) {
								module.fields = response.data;
								var lookupList = $filter('filter')(module.fields, function (field) {
									return field.data_type === 'lookup' && field.lookup_type !== 'users' && field.lookup_type !== 'profiles' && field.lookup_type !== 'roles';
								});

								packageModulesRelations[module.name] = [];
								allModulesRelations[module.name] = [];

								for (var k = 0; k < lookupList.length; k++) {
									packageModulesRelations[module.name].push(lookupList[k]);
									allModulesRelations[module.name].push({
										name: lookupList[k].name,
										lookup_type: lookupList[k].lookup_type
									});
								}
							});
					});
				},
				preparePackage: function (selectedModulesArray, packageModules) {
					var selectedModules = [];
					for (var index in selectedModulesArray) {
						if (index !== 'getUnique') {
							var moduleName = selectedModulesArray[index];
							var packageModule = $filter('filter')(packageModules, { name: moduleName }, true)[0];
							var moduleIndex = selectedModules.indexOf(packageModule);
							if (moduleIndex < 0)
								selectedModules.push(packageModule);
						}
					}
					return selectedModules;
				}
			};
		}]);

