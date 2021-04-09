"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.UserNotificationService = void 0;
var signalR = require("@microsoft/signalr");
var UserNotificationService = /** @class */ (function () {
    function UserNotificationService(authService) {
        this.authService = authService;
        // Events
        this.UserStatusChanged = "UserStatusChanged";
        this.NewConversationMessage = "new-conversation-message";
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
        this.connection.on(this.UserStatusChanged, function (data) { return handler(data.currentStatus); });
    };
    UserNotificationService.prototype.subscribeForUnreadConversation = function (handler) {
        if (!this.connection) {
            this.init();
        }
        this.connection.on(this.NewConversationMessage, function (msg) { return handler(msg); });
    };
    UserNotificationService.prototype.unsubscribe = function (event) {
        if (!this.connection) {
            console.log('No connection');
            return;
        }
        this.connection.off(event);
    };
    return UserNotificationService;
}());
exports.UserNotificationService = UserNotificationService;
//# sourceMappingURL=UserNotificationService.js.map