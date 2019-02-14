! function (e, t) {
    "use strict";
    return "function" == typeof define && define.amd ? void define(["angular"], function (e) {
        return t(e)
    }) : t(e)
}(angular || null, function (e) {
    "use strict";
    var t = e.module("ngTable", []);
    t.value("ngTableDefaults", {
        params: {},
        settings: {}
    }), t.factory("ngTableParams", ["$q", "$log", "ngTableDefaults", function (t, a, n) {
        var r = function (e) {
            return !isNaN(parseFloat(e)) && isFinite(e)
        },
            i = function (i, s) {
                var l = this,
                    o = function () {
                        u.debugMode && a.debug && a.debug.apply(this, arguments)
                    };
                this.data = [], this.parameters = function (t, a) {
                    if (a = a || !1, e.isDefined(t)) {
                        for (var n in t) {
                            var i = t[n];
                            if (a && n.indexOf("[") >= 0) {
                                for (var s = n.split(/\[(.*)\]/).reverse(), l = "", u = 0, p = s.length; p > u; u++) {
                                    var g = s[u];
                                    if ("" !== g) {
                                        var m = i;
                                        i = {}, i[l = g] = r(m) ? parseFloat(m) : m
                                    }
                                }
                                "sorting" === l && (c[l] = {}), c[l] = e.extend(c[l] || {}, i[l])
                            } else c[n] = r(t[n]) ? parseFloat(t[n]) : t[n]
                        }
                        return o("ngTable: set parameters", c), this
                    }
                    return c
                }, this.settings = function (t) {
                    return e.isDefined(t) ? (e.isArray(t.data) && (t.total = t.data.length), u = e.extend(u, t), o("ngTable: set settings", u), this) : u
                }, this.page = function (t) {
                    return e.isDefined(t) ? this.parameters({
                        page: t
                    }) : c.page
                }, this.total = function (t) {
                    return e.isDefined(t) ? this.settings({
                        total: t
                    }) : u.total
                }, this.count = function (t) {
                    return e.isDefined(t) ? this.parameters({
                        count: t,
                        page: 1
                    }) : c.count
                }, this.filter = function (t) {
                    return e.isDefined(t) ? this.parameters({
                        filter: t,
                        page: 1
                    }) : c.filter
                }, this.sorting = function (t) {
                    if (2 == arguments.length) {
                        var a = {};
                        return a[t] = arguments[1], this.parameters({
                            sorting: a
                        }), this
                    }
                    return e.isDefined(t) ? this.parameters({
                        sorting: t
                    }) : c.sorting
                }, this.isSortBy = function (t, a) {
                    return e.isDefined(c.sorting[t]) && e.equals(c.sorting[t], a)
                }, this.orderBy = function () {
                    var e = [];
                    for (var t in c.sorting) e.push(("asc" === c.sorting[t] ? "+" : "-") + t);
                    return e
                }, this.getData = function (t, a) {
                    return e.isArray(this.data) && e.isObject(a) ? t.resolve(this.data.slice((a.page() - 1) * a.count(), a.page() * a.count())) : t.resolve([]), t.promise
                }, this.getGroups = function (a, n) {
                    var r = t.defer();
                    return r.promise.then(function (t) {
                        var r = {};
                        e.forEach(t, function (t) {
                            var a = e.isFunction(n) ? n(t) : t[n];
                            r[a] = r[a] || {
                                data: []
                            }, r[a].value = a, r[a].data.push(t)
                        });
                        var i = [];
                        for (var s in r) i.push(r[s]);
                        o("ngTable: refresh groups", i), a.resolve(i)
                    }), this.getData(r, l)
                }, this.generatePagesArray = function (e, t, a) {
                    var n, r, i, s, l, o;
                    if (n = 11, o = [], l = Math.ceil(t / a), l > 1) {
                        o.push({
                            type: "prev",
                            number: Math.max(1, e - 1),
                            active: e > 1
                        }), o.push({
                            type: "first",
                            number: 1,
                            active: e > 1,
                            current: 1 === e
                        }), i = Math.round((n - 5) / 2), s = Math.max(2, e - i), r = Math.min(l - 1, e + 2 * i - (e - s)), s = Math.max(2, s - (2 * i - (r - s)));
                        for (var c = s; r >= c;) c === s && 2 !== c || c === r && c !== l - 1 ? o.push({
                            type: "more",
                            active: !1
                        }) : o.push({
                            type: "page",
                            number: c,
                            active: e !== c,
                            current: e === c
                        }), c++;
                        o.push({
                            type: "last",
                            number: l,
                            active: e !== l,
                            current: e === l
                        }), o.push({
                            type: "next",
                            number: Math.min(l, e + 1),
                            active: l > e
                        })
                    }
                    return o
                }, this.url = function (t) {
                    t = t || !1;
                    var a = t ? [] : {};
                    for (var n in c)
                        if (c.hasOwnProperty(n)) {
                            var r = c[n],
                                i = encodeURIComponent(n);
                            if ("object" == typeof r) {
                                for (var s in r)
                                    if (!e.isUndefined(r[s]) && "" !== r[s]) {
                                        var l = i + "[" + encodeURIComponent(s) + "]";
                                        t ? a.push(l + "=" + r[s]) : a[l] = r[s]
                                    }
                            } else e.isFunction(r) || e.isUndefined(r) || "" === r || (t ? a.push(i + "=" + encodeURIComponent(r)) : a[i] = encodeURIComponent(r))
                        }
                    return a
                }, this.reload = function () {
                    var e = t.defer(),
                        a = this,
                        n = null;
                    if (u.$scope) return u.$loading = !0, n = u.groupBy ? u.getGroups(e, u.groupBy, this) : u.getData(e, this), o("ngTable: reload data"), n || (n = e.promise), n.then(function (e) {
                        return u.$loading = !1, o("ngTable: current scope", u.$scope), u.groupBy ? (a.data = e, u.$scope && (u.$scope.$groups = e)) : (a.data = e, u.$scope && (u.$scope.$data = e)), u.$scope && (u.$scope.pages = a.generatePagesArray(a.page(), a.total(), a.count())), u.$scope.$emit("ngTableAfterReloadData"), e
                    })
                }, this.reloadPages = function () {
                    var e = this;
                    u.$scope.pages = e.generatePagesArray(e.page(), e.total(), e.count())
                };
                var c = this.$params = {
                    page: 1,
                    count: 1,
                    filter: {},
                    sorting: {},
                    group: {},
                    groupBy: null
                };
                e.extend(c, n.params);
                var u = {
                    $scope: null,
                    $loading: !1,
                    data: null,
                    total: 0,
                    defaultSort: "desc",
                    filterDelay: 750,
                    counts: [10, 25, 50, 100],
                    getGroups: this.getGroups,
                    getData: this.getData
                };
                return e.extend(u, n.settings), this.settings(s), this.parameters(i, !0), this
            };
        return i
    }]);
    var a = ["$scope", "ngTableParams", "$timeout", function (e, t, a) {
        var n = !0;
        e.$loading = !1, e.hasOwnProperty("params") || (e.params = new t, e.params.isNullInstance = !0), e.params.settings().$scope = e;
        (function () {
            var e = 0;
            return function (t, n) {
                a.cancel(e), e = a(t, n)
            }
        })();
        e.$watch("params.load", function (t, a) {
            t !== a && (e.params.settings().$scope = e, e.params.isNullInstance || (n = !1), e.params.reload())
        }, !0), e.sortBy = function (t, a) {
            var n = e.parse(t.sortable);
            if (n) {
                var r = e.params.settings().defaultSort,
                    i = "asc" === r ? "desc" : "asc",
                    s = e.params.sorting() && e.params.sorting()[n] && e.params.sorting()[n] === r,
                    l = a.ctrlKey || a.metaKey ? e.params.sorting() : {};
                l[n] = s ? i : r, e.params.parameters({
                    sorting: l
                })
            }
        }
    }];
    return t.directive("ngTable", ["$compile", "$q", "$parse", function (t, n, r) {
        return {
            restrict: "A",
            priority: 1001,
            scope: !0,
            controller: a,
            compile: function (a) {
                var n = [],
                    i = 0,
                    s = null,
                    l = a.find("thead");
                return e.forEach(e.element(a.find("tr")), function (t) {
                    t = e.element(t), t.hasClass("ng-table-group") || s || (s = t)
                }), s ? (e.forEach(s.find("td"), function (t) {
                    var a = e.element(t);
                    if (!a.attr("ignore-cell") || "true" !== a.attr("ignore-cell")) {
                        var s = function (e, t) {
                            return function (i) {
                                return r(a.attr("x-data-" + e) || a.attr("data-" + e) || a.attr(e))(i, {
                                    $columns: n
                                }) || t
                            }
                        },
                            l = s("title", " "),
                            o = s("header", !1),
                            c = s("filter", !1)(),
                            u = !1,
                            p = !1;
                        c && c.$$name && (p = c.$$name, delete c.$$name), c && c.templateURL && (u = c.templateURL, delete c.templateURL), a.attr("data-title-text", l()), n.push({
                            id: i++,
                            title: l,
                            sortable: s("sortable", !1),
                            "class": a.attr("x-data-header-class") || a.attr("data-header-class") || a.attr("header-class"),
                            filter: c,
                            filterTemplateURL: u,
                            filterName: p,
                            headerTemplateURL: o,
                            filterData: a.attr("filter-data") ? a.attr("filter-data") : null,
                            show: a.attr("ng-show") ? function (e) {
                                return r(a.attr("ng-show"))(e)
                            } : function () {
                                return !0
                            }
                        })
                    }
                }), function (a, i, s) {
                    if (a.$loading = !1, a.$columns = n, a.$filterRow = {}, a.$watch(s.ngTable, function (t) {
                        e.isUndefined(t) || (a.paramsModel = r(s.ngTable), a.params = t)
                    }, !0), a.parse = function (t) {
                        return e.isDefined(t) ? t(a) : ""
                    }, s.showFilter && a.$parent.$watch(s.showFilter, function (e) {
                        a.show_filter = e
                    }), s.disableFilter && a.$parent.$watch(s.disableFilter, function (e) {
                        a.$filterRow.disabled = e
                    }), e.forEach(n, function (t) {
                        var n;
                        if (t.filterData) return n = r(t.filterData)(a, {
                            $column: t
                        }), e.isObject(n) && e.isObject(n.promise) ? (delete t.filterData, n.promise.then(function (a) {
                            e.isArray(a) || e.isFunction(a) || e.isObject(a) ? e.isArray(a) && a.unshift({
                                title: "-",
                                id: ""
                            }) : a = [], t.data = a
                        })) : t.data = n
                    }), !i.hasClass("ng-table")) {
                        a.templates = {
                            header: s.templateHeader ? s.templateHeader : "ng-table/header.html",
                            pagination: s.templatePagination ? s.templatePagination : "ng-table/pager.html"
                        };
                        var o = l.length > 0 ? l : e.element(document.createElement("thead")).attr("ng-include", "templates.header"),
                            c = e.element(document.createElement("div")).attr({
                                "ng-table-pagination": "params",
                                "template-url": "templates.pagination"
                            });
                        i.find("thead").remove(), i.addClass("ng-table").prepend(o).after(c), t(o)(a), t(c)(a)
                    }
                }) : void 0
            }
        }
    }]), t.directive("ngTablePagination", ["$compile", function (t) {
        return {
            restrict: "A",
            scope: {
                params: "=ngTablePagination",
                templateUrl: "="
            },
            replace: !1,
            link: function (a, n, r) {
                a.params.settings().$scope.$on("ngTableAfterReloadData", function () {
                    a.pages = a.params.generatePagesArray(a.params.page(), a.params.total(), a.params.count()), a.count = a.params.count()
                }, !0), a.$watch("templateUrl", function (r) {
                    if (!e.isUndefined(r)) {
                        var i = e.element(document.createElement("div"));
                        i.attr({
                            "ng-include": "templateUrl"
                        }), n.append(i), t(i)(a)
                    }
                })
            }
        }
    }]), e.module("ngTable").run(["$templateCache", function (e) {
        e.put("ng-table/filters/select-multiple.html", '<select ng-options="data.id as data.title for data in column.data" ng-disabled="$filterRow.disabled" multiple ng-multiple="true" ng-model="params.filter()[name]" ng-show="filter==\'select-multiple\'" class="filter filter-select-multiple form-control" name="{{column.filterName}}"> </select>'), e.put("ng-table/filters/select.html", '<select ng-options="data.id as data.title for data in column.data" ng-disabled="$filterRow.disabled" ng-model="params.filter()[name]" ng-show="filter==\'select\'" class="filter filter-select form-control" name="{{column.filterName}}"> </select>'), e.put("ng-table/filters/text.html", '<input type="text" name="{{column.filterName}}" ng-disabled="$filterRow.disabled" ng-model="params.filter()[name]" ng-if="filter==\'text\'" class="input-filter form-control"/>'), e.put("ng-table/header.html", '<tr> <th ng-repeat="column in $columns" ng-class="{ \'sortable\': parse(column.sortable), \'sort-asc\': params.sorting()[parse(column.sortable)]==\'asc\', \'sort-desc\': params.sorting()[parse(column.sortable)]==\'desc\' }" ng-click="sortBy(column, $event)" ng-show="column.show(this)" ng-init="template=column.headerTemplateURL(this)" class="header {{column.class}}"> <div ng-if="!template" ng-show="!template" ng-bind="parse(column.title)"></div> <div ng-if="template" ng-show="template" ng-include="template"></div> </th> </tr> <tr ng-show="show_filter" class="ng-table-filters"> <th ng-repeat="column in $columns" ng-show="column.show(this)" class="filter"> <div ng-repeat="(name, filter) in column.filter"> <div ng-if="column.filterTemplateURL" ng-show="column.filterTemplateURL"> <div ng-include="column.filterTemplateURL"></div> </div> <div ng-if="!column.filterTemplateURL" ng-show="!column.filterTemplateURL"> <div ng-include="\'ng-table/filters/\' + filter + \'.html\'"></div> </div> </div> </th> </tr> '), e.put("ng-table/pager.html", '<div class="ng-cloak ng-table-pager"> <div ng-if="params.settings().counts.length" class="ng-table-counts btn-group pull-right"> <button ng-repeat="count in params.settings().counts" type="button" ng-class="{\'active\':params.count()==count}" ng-click="params.count(count)" class="btn btn-default"> <span ng-bind="count"></span> </button> </div> <ul class="pagination ng-table-pagination"> <li ng-class="{\'disabled\': !page.active && !page.current, \'active\': page.current}" ng-repeat="page in pages" ng-switch="page.type"> <a ng-switch-when="prev" ng-click="params.page(page.number)" href="">&laquo;</a> <a ng-switch-when="first" ng-click="params.page(page.number)" href=""><span ng-bind="page.number"></span></a> <a ng-switch-when="page" ng-click="params.page(page.number)" href=""><span ng-bind="page.number"></span></a> <a ng-switch-when="more" ng-click="params.page(page.number)" href="">&#8230;</a> <a ng-switch-when="last" ng-click="params.page(page.number)" href=""><span ng-bind="page.number"></span></a> <a ng-switch-when="next" ng-click="params.page(page.number)" href="">&raquo;</a> </li> </ul> </div> ')
    }]), t
});