<template>
    <div class="unread-conversations-container">
        <div v-if="conversations && conversations.length">
            <div class="conversation-row" v-for="conversation in conversations" :key="conversation">
                <span class="conversation-name">{{conversation.name}}</span>
                <span class="badge badge-pill badge-primary" style="margin-left: 5px;" v-if="conversation.unreadMessages">{{conversation.unreadMessages}}</span>
            </div>
            <div v-if="conversationsToLoad > 0">
                <a href="javascript:void(0)" @click="loadMore()">Load more..</a>
            </div>
        </div>
        <div v-if="!conversations.length">
            <span class="no-conversation-message">No unread conversations</span>
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
                conversations: [],
                totalConversations: 0,
                conversationsToLoad: 0,
            };
        },
        mounted: function () {
            this.loadConversations();
        },
        methods: {
            loadConversations: function () {
                this.$apiClient.getAsync(`api/conversationlist?pageSize=${this.pageSize}&pageNumber=${this.pageNumber}`)
                    .then((response) => {
                        if (!this.totalConversations) {
                            this.totalConversations = Number(response.headers["total-conversations"]);
                            this.conversationsToLoad = this.totalConversations;
                        }

                        let rows = response.data;
                        for (let row of rows) {
                            this.conversations.push(row);
                        }

                        this.conversationsToLoad -= rows.length;
                        console.log('Current page: ' + this.pageNumber + ' Conversations to load:' + this.conversationsToLoad);
                    });
            },
            loadMore: function () {
                this.pageNumber++;
                this.loadConversations();
            }
        }
    }
</script>
<style scoped>
    .unread-conversations-container {
        min-width: 200px;
    }

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