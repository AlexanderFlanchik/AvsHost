"use strict";
exports.__esModule = true;
exports.SiteContextManager = void 0;
var SiteContextManager = /** @class */ (function () {
    function SiteContextManager() {
        this.siteContextKey = 'SiteContext';
    }
    SiteContextManager.prototype.save = function (siteContext) {
        localStorage.setItem(this.siteContextKey, JSON.stringify(siteContext));
    };
    SiteContextManager.prototype.get = function () {
        var jsonContext = localStorage.getItem(this.siteContextKey);
        if (!jsonContext) {
            return null;
        }
        return JSON.parse(jsonContext);
    };
    SiteContextManager.prototype["delete"] = function () {
        localStorage.removeItem(this.siteContextKey);
    };
    return SiteContextManager;
}());
exports.SiteContextManager = SiteContextManager;
