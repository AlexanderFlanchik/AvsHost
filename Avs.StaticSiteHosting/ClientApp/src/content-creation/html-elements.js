"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.GenericElement = exports.Link = exports.Metadata = exports.Script = exports.Head = exports.Html = void 0;
var uuid_1 = require("uuid");
var Html = /** @class */ (function () {
    function Html() {
        this.head = new Head();
        this.body = new GenericElement();
        this.body.tag = 'body';
    }
    return Html;
}());
exports.Html = Html;
var Head = /** @class */ (function () {
    function Head() {
        this.metadatas = new Array();
        this.scripts = new Array();
        this.styles = new Array();
        this.links = new Array();
    }
    return Head;
}());
exports.Head = Head;
var Script = /** @class */ (function () {
    function Script() {
    }
    return Script;
}());
exports.Script = Script;
var Metadata = /** @class */ (function () {
    function Metadata() {
    }
    return Metadata;
}());
exports.Metadata = Metadata;
var Link = /** @class */ (function () {
    function Link() {
    }
    return Link;
}());
exports.Link = Link;
var GenericElement = /** @class */ (function () {
    function GenericElement() {
        this.attributes = new Map();
        this.children = new Array();
        this.innerCode = (0, uuid_1.v4)();
    }
    Object.defineProperty(GenericElement.prototype, "outerHtml", {
        get: function () {
            var attrs = [];
            this.attributes.forEach(function (val, key) {
                attrs.push({ name: key, value: val });
            });
            var attr = attrs.map(function (a) { return "".concat(a.name, "=\"").concat(a.value, "\""); }).join(' ');
            if (this.tag != 'img' && this.tag != 'br' && this.innerHtml) {
                return attr.length ? "<".concat(this.tag, " ").concat(attr, ">").concat(this.innerHtml, "</").concat(this.tag, ">") : "<".concat(this.tag, ">").concat(this.innerHtml, "</").concat(this.tag, ">");
            }
            else {
                return attr.length ? "<".concat(this.tag, " ").concat(attr, " />") : "<".concat(this.tag, "/>");
            }
        },
        enumerable: false,
        configurable: true
    });
    GenericElement.prototype.getElementByInnerCode = function (innerCode) {
        if (this.innerCode == innerCode) {
            return this;
        }
        if (this.children.length) {
            for (var _i = 0, _a = this.children; _i < _a.length; _i++) {
                var ch = _a[_i];
                var e = ch.getElementByInnerCode(innerCode);
                if (e) {
                    return e;
                }
            }
        }
        return null;
    };
    GenericElement.prototype.getElementById = function (id) {
        if (this.id === id) {
            return this;
        }
        if (this.children.length) {
            for (var _i = 0, _a = this.children; _i < _a.length; _i++) {
                var ch = _a[_i];
                var e = ch.getElementById(id);
                if (e) {
                    return e;
                }
            }
        }
        return null;
    };
    return GenericElement;
}());
exports.GenericElement = GenericElement;
