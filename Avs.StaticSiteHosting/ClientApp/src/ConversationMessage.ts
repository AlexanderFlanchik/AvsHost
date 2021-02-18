class ConversationMessage {
    constructor(
        public Id: String,
        public DateAdded: Date,
        public Viewed: Boolean,
        public Content: String
    ) { }
}

export default ConversationMessage;