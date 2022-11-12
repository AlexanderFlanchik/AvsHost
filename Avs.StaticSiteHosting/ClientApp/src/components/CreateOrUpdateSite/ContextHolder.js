"use strict";
exports.__esModule = true;
exports.CreateOrUpdateSiteContextHolder = void 0;
var CreateOrUpdateSiteContextHolder = /** @class */ (function () {
    function CreateOrUpdateSiteContextHolder() {
    }
    CreateOrUpdateSiteContextHolder.prototype.set = function (siteFormData) {
        this._state = siteFormData;
    };
    CreateOrUpdateSiteContextHolder.prototype.isModified = function (siteFormData) {
        var _this = this;
        if (!this._state) {
            throw new Error("Originial state has not been specified.");
        }
        var areFieldsModified = this._state.description != siteFormData.description ||
            this._state.isActive != siteFormData.isActive ||
            this._state.landingPage != siteFormData.landingPage ||
            this._state.siteName != siteFormData.siteName;
        if (areFieldsModified) {
            return true;
        }
        if (this._state.resourceMappings && this._state.resourceMappings.size) {
            if (!siteFormData.resourceMappings || !siteFormData.resourceMappings.size ||
                this._state.resourceMappings.size != siteFormData.resourceMappings.size) {
                return true;
            }
            var rsChanged_1 = false;
            siteFormData.resourceMappings.forEach(function (val, key) {
                var rm = _this._state.resourceMappings.get(key);
                if (rm != val) {
                    rsChanged_1 = true;
                }
            });
            if (rsChanged_1) {
                return true;
            }
        }
        else {
            if (siteFormData.resourceMappings && siteFormData.resourceMappings.size) {
                return true;
            }
        }
        return false;
    };
    return CreateOrUpdateSiteContextHolder;
}());
exports.CreateOrUpdateSiteContextHolder = CreateOrUpdateSiteContextHolder;
