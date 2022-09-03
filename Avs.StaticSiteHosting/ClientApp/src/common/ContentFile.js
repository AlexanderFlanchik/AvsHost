"use strict";
exports.__esModule = true;
exports.ContentFile = void 0;
var ContentFile = /** @class */ (function () {
    function ContentFile(id, name, destinationPath, isNew, size, isEditable, isViewable, uploadedAt, updateDate) {
        this.id = id;
        this.name = name;
        this.destinationPath = destinationPath;
        this.isNew = isNew;
        this.size = size;
        this.isEditable = isEditable;
        this.isViewable = isViewable;
        this.uploadedAt = uploadedAt;
        this.updateDate = updateDate;
    }
    ContentFile.prototype.fullName = function () {
        if (!this.destinationPath) {
            return this.name;
        }
        var delimeter = '/';
        if (this.destinationPath.indexOf('\\') > 0) {
            delimeter = '\\';
        }
        return "".concat(this.destinationPath).concat(delimeter).concat(this.name);
    };
    return ContentFile;
}());
exports.ContentFile = ContentFile;
