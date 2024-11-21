export class MarkReadMessagesQueue {
    private messages: Array<any> =  [];
    private interval: number | null = null;
    private postHandler: (messages: Array<any>) => Promise<string[]>;

    constructor (postHandler: (messages: Array<any>) => Promise<string[]>) {
        this.postHandler = postHandler;
    }

    private startProcessing() {
        this.interval = setInterval(() => {
            this.postHandler(this.messages)
                .then((ids) => {
                    this.messages = this.messages.filter(m => ids.indexOf(m.id) < 0);
                    if (!this.messages.length) {
                        clearInterval(this.interval!);
                        this.interval = null;
                    }
                });
        }, 1000);
    }

    public addMessage(msg: any) {
        const ids = this.messages.map(m => m.id);
        if (ids.indexOf(msg.id) >= 0) {
            return;
        }

        this.messages.push(msg);
        if (!this.interval) {
            this.startProcessing();
        }
    }
};
 