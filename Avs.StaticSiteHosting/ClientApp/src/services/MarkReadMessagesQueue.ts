const MarkReadMessagesQueue = function (postHandler) {
    const self = this;
    let messages = [];
    let interval = null;

    self.addMessage = function (msg) {
        let ids = messages.map(m => m.id);
        if (ids.indexOf(msg.id) >= 0) {
            return;
        }

        messages.push(msg);

        if (!interval) {
            startProcessing();
        }
    };

    function startProcessing() {
        interval = setInterval(() => {
            postHandler(messages)
                .then((ids) => {
                    messages = messages.filter(m => ids.indexOf(m.id) < 0);
                    if (!messages.length) {
                        clearInterval(interval);
                        interval = null;
                    }
                });
        }, 1000);
    }
};

export default MarkReadMessagesQueue;