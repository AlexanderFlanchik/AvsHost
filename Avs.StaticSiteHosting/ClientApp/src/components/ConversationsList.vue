<template>
    <div>
        <div v-if="conversations && conversations.length">
            <div class="conversation-row" v-for="conversation in conversations" :key="conversation">
                <span class="conversation-name">{{conversation.name}}</span>
                <span class="badge badge-pill badge-primary" style="margin-left: 5px;" v-if="conversation.unreadMessages">{{conversation.unreadMessages}}</span>
            </div>
        </div>
        <div v-if="!conversations.length">
            <span class="no-conversation-message">No conversations yet</span>
        </div>
    </div>
</template>
<script>
    export default {
        data: function () {
            return {
                selectedConversationId: '',
                pageNumber: 1,
                pageSize: 50,
                conversations: []
            };
        },
        mounted: function () {
            this.conversations.push({ name: "Alex", unreadMessages: 1 });
            this.conversations.push({ name: "Хитрый жопель", unreadMessages: 10 });
            this.conversations.push({ name: "Ошень Хитрый жопель", unreadMessages: 1 });
        },
        methods: {
            loadConversations: function() {

            }
        }
    }
</script>
<style scoped>
    .conversation-row {
        margin-top: 5px;
        margin-bottom: 5px;
    }

    .no-conversation-message {        
        font-weight: bold;
        color: navy;
    }
    
    .conversation-name {
        font-style: italic;
        color: Highlight;
    }
</style>