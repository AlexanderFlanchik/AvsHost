"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.UserNotificationService = void 0;
var signalR = require("@microsoft/signalr");
var UserNotificationService = /** @class */ (function () {
    function UserNotificationService(authService) {
        this.authService = authService;
    }
    UserNotificationService.prototype.init = function () {
        var _this = this;
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl("/user-notification", { accessTokenFactory: function () { return _this.authService.getToken(); } })
            .withAutomaticReconnect()
            .build();
        this.connection.start().then(function () { return console.log('Realtime notification initalized.'); }).catch(function (err) { return console.log(err); });
    };
    UserNotificationService.prototype.notify = function (handler) {
        if (!this.connection) {
            this.init();
        }
        this.connection.on("UserStatusChanged", function (data) { return handler(data.currentStatus); });
    };
    UserNotificationService.prototype.subscribeForUnreadConversation = function (handler) {
        if (!this.connection) {
            this.init();
        }
        this.connection.on("new-conversation-message", function (msg) { return handler(msg); });
    };
    return UserNotificationService;
}());
exports.UserNotificationService = UserNotificationService;
//# sourceMappingURL=UserNotificationService.js.map