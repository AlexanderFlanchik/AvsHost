<template>
    <div class="unread-conversations-container">
        <div v-if="conversations && conversations.length">
            <div class="conversation-row" v-for="conversation in conversations" :key="conversation" 
                 v-bind:class="{'selected-conversation': conversation.id == selectedConversationId }"
                 @click="selectConversation(conversation.id)">
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
        props: {
            selectedConversationIdSubject: Object,
            onNewConversationsLoadedCallback: Object
        },
        data: function () {
            return {
                selectedConversationId: '',
                pageNumber: 1,
                pageSize: 50,
                conversations: [],
                totalConversations: 0,
                conversationsToLoad: 0,
                onNewConversationsLoadedCallback: null
            };
        },
        mounted: function () {
            this.loadConversations();
        },
        methods: {
            getConversationIds: function () {
                return this.conversations.map(c => c.id);
            },

            loadConversations: function () {
                this.$apiClient.getAsync(`api/conversationlist?pageSize=${this.pageSize}&pageNumber=${this.pageNumber}`)
                    .then((response) => {
                        if (!this.totalConversations) {
                            this.totalConversations = Number(response.headers["total-conversations"]);
                            this.conversationsToLoad = this.totalConversations;
                        }

                        let ids = [];
                        let rows = response.data;
                        for (let row of rows) {
                            this.conversations.push(row);
                            ids.push(row.id);
                        }

                        if (this.onNewConversationsLoadedCallback) {
                            this.onNewConversationsLoadedCallback(ids);
                        }

                        this.conversationsToLoad -= rows.length;
                        console.log('Current page: ' + this.pageNumber + ' Conversations to load:' + this.conversationsToLoad);
                    });
            },
            loadMore: function () {
                this.pageNumber++;
                this.loadConversations();
            },

            selectConversation: function (conversationId) {
                console.log(conversationId + ' selected');
                this.selectedConversationId = conversationId;
                if (this.selectedConversationIdSubject) {
                    this.selectedConversationIdSubject.next(this.selectedConversationId);
                }
            },
            onNewMessage: function (messageData) {
                let conversationId = messageData.conversationId;
                let foundConversation = this.conversations.find(c => c.id == conversationId);
                if (foundConversation) {
                    let unread = foundConversation.unreadMessages || 0;
                    foundConversation.unreadMessages = unread + 1;
                    let i = this.conversations.indexOf(foundConversation);
                    this.conversations.splice(i, 1);
                    this.conversations.unshift(foundConversation);
                } else {
                    this.$apiClient.getAsync(`api/conversation/${conversationId}`).then((response) => {
                        console.log(response.data);
                        let conversation = response.data.conversation;
                        this.conversations.unshift({ id: conversationId, name: conversation.name, unreadMessages: 1 });
                    });
                }
            },

            updateConversation: function (conversationId, readMessagesCount) {
                let conversation = this.conversations.find(c => c.id == conversationId);
                if (!conversation) {
                    return;
                }

                conversation.unreadMessages = conversation.unreadMessages >= readMessagesCount ? conversation.unreadMessages - readMessagesCount : 0;
            }
        }       
    }
</script>
<style scoped>
    .unread-conversations-container {
        min-width: 200px;
    }

    .selected-conversation {
        background-color: darkgoldenrod;
        font-weight: bold;
    }

    .conversation-row {
        margin-top: 5px;
        margin-bottom: 5px;
        cursor: pointer;
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