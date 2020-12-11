"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
Object.defineProperty(exports, "__esModule", { value: true });
var moment = require("moment");
var axios_1 = require("axios");
var AuthService = /** @class */ (function () {
    function AuthService() {
        this.tokenKey = 'auth_token';
        this.isLocked = false;
    }
    AuthService.prototype.IsUserLocked = function () {
        return this.isLocked;
    };
    AuthService.prototype.lockUser = function () {
        this.isLocked = true;
    };
    AuthService.prototype.unLockUser = function () {
        this.isLocked = false;
    };
    AuthService.prototype.isAuthenticated = function () {
        var tokenJson = localStorage.getItem(this.tokenKey);
        if (!tokenJson) {
            return false;
        }
        var token = JSON.parse(tokenJson);
        var utcNow = new Date(moment().utc().format());
        var expDate = new Date(token.expires_at);
        if (utcNow > expDate) {
            localStorage.removeItem(this.tokenKey);
            console.log('Auth token expired.');
            return false;
        }
        // token is valid
        return true;
    };
    AuthService.prototype.getToken = function () {
        var tokenData = localStorage.getItem(this.tokenKey);
        if (!tokenData) {
            return null;
        }
        return JSON.parse(tokenData)['token'];
    };
    AuthService.prototype.getUserInfo = function () {
        var tokenData = localStorage.getItem(this.tokenKey);
        if (!tokenData) {
            return null;
        }
        var userInfo = JSON.parse(tokenData)['userInfo'];
        return userInfo;
    };
    AuthService.prototype.signOut = function () {
        var tokenData = localStorage.getItem(this.tokenKey);
        if (!tokenData) {
            return;
        }
        localStorage.removeItem(this.tokenKey);
    };
    AuthService.prototype.tryGetAccessToken = function (userLogin, password) {
        return __awaiter(this, void 0, void 0, function () {
            var response, token, expires_at, userInfo, e_1;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        _a.trys.push([0, 2, , 3]);
                        return [4 /*yield*/, axios_1.default.post('/auth/token', {
                                login: userLogin,
                                password: password
                            })];
                    case 1:
                        response = _a.sent();
                        if (response && response.data && response.data.token) {
                            token = response.data.token;
                            expires_at = response.data.expires_at;
                            userInfo = response.data.userInfo;
                            localStorage.setItem(this.tokenKey, JSON.stringify({ token: token, expires_at: expires_at, userInfo: userInfo }));
                            return [2 /*return*/, true];
                        }
                        return [3 /*break*/, 3];
                    case 2:
                        e_1 = _a.sent();
                        console.log(e_1);
                        return [3 /*break*/, 3];
                    case 3: return [2 /*return*/, false];
                }
            });
        });
    };
    AuthService.prototype.register = function (email, userName, password) {
        return __awaiter(this, void 0, void 0, function () {
            var response, e_2;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        _a.trys.push([0, 2, , 3]);
                        return [4 /*yield*/, axios_1.default.post('/auth/register', {
                                email: email,
                                userName: userName,
                                password: password
                            })];
                    case 1:
                        response = _a.sent();
                        return [2 /*return*/, response.status === 200];
                    case 2:
                        e_2 = _a.sent();
                        console.log(e_2);
                        return [3 /*break*/, 3];
                    case 3: return [2 /*return*/, false];
                }
            });
        });
    };
    AuthService.prototype.validateUserData = function (userName, email) {
        return __awaiter(this, void 0, void 0, function () {
            var url, response, e_3;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        _a.trys.push([0, 2, , 3]);
                        url = '/auth/validateUserData';
                        if (!userName && !email) {
                            return [2 /*return*/, false];
                        }
                        if (userName) {
                            url += "?userName=" + userName;
                            if (email) {
                                url += "&email=" + email;
                            }
                        }
                        else {
                            url += "?email=" + email;
                        }
                        return [4 /*yield*/, axios_1.default.get(url)];
                    case 1:
                        response = _a.sent();
                        console.log(response);
                        return [2 /*return*/, response.data];
                    case 2:
                        e_3 = _a.sent();
                        console.log(e_3);
                        return [3 /*break*/, 3];
                    case 3: return [2 /*return*/, false];
                }
            });
        });
    };
    return AuthService;
}());
exports.default = AuthService;
//# sourceMappingURL=AuthService.js.map