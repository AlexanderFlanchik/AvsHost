"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var MarkReadMessagesQueue = function (postHandler) {
    var self = this;
    var messages = [];
    var interval = null;
    self.addMessage = function (msg) {
        var ids = messages.map(function (m) { return m.id; });
        if (ids.indexOf(msg.id) >= 0) {
            return;
        }
        messages.push(msg);
        if (!interval) {
            startProcessing();
        }
    };
    function startProcessing() {
        interval = setInterval(function () {
            postHandler(messages)
                .then(function (ids) {
                messages = messages.filter(function (m) { return ids.indexOf(m.id) < 0; });
                if (!messages.length) {
                    clearInterval(interval);
                    interval = null;
                }
            });
        }, 1000);
    }
};
exports.default = MarkReadMessagesQueue;
//# sourceMappingURL=MarkReadMessagesQueue.js.map