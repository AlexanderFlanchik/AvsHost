<template>
    <div class="content-block-container">
        <div class="general-page-title">
            <img src="../../public/icons8-chat-32.png" /> &nbsp;
            <span>Conversations</span>
        </div>
        <UserInfo />
        <NavigationMenu />
        <div class="conversations-content">
            <div class="left conversations-list">
                <ConversationsList :selectedConversationIdSubject="selectedConversationId$" ref="conversationsList" :onNewConversationsLoadedCallback="onNewUnreadConversations"/>
                <hr class="component-splitter" />
                <ConversationSearch :selectedConversationIdSubject="selectedConversationId$" :unreadConversationsSubject="unreadConversations$" ref="conversationSearch" />
            </div>

            <div class="left conversation-message-list">
                <ConversationMessagesWrapper ref="conversationMessagesList" :messagesToMakeReadHandler="messagesToMakeReadHandler" :conversationFilter="conversationFilter" :onUnreadConversation="onUnreadConversation" :height="'calc(100vh - 375px)'" />
                <div class="send-message-form-container">
                    <div>Enter a message:</div> 
                    
                    <div class="newMessage-input-container">
                        <b-form-textarea v-model="newMessage" rows="3" :disabled="!buttonsEnabled"></b-form-textarea>
                    </div>                    
                </div>
                <div class="button-bar">
                    <button class="btn btn-primary" :disabled="!newMessage || !buttonsEnabled" @click="sendMessage">Send..</button> &nbsp;
                    <button class="btn btn-default" :disabled="!newMessage|| !buttonsEnabled" @click="clearMessage">Clear</button>
                </div>
            </div>
        </div>
    </div>
</template>
<script>
    import UserInfo from '@/components/UserInfo.vue';
    import NavigationMenu from '@/components/NavigationMenu.vue';
    import ConversationsList from '@/components/ConversationsList.vue';
    import ConversationMessagesWrapper from '@/components/ConversationMessagesWrapper.vue';
    import ConversationSearch from '@/components/ConversationSearch.vue';

    import { Subject } from 'rxjs';

    export default {
        data: function() {
            return {
                selectedConversationId$: new Subject(),
                unreadConversations$: new Subject(),
                selectedConversationId: null,
                buttonsEnabled: false,
                newMessage: '',
            };
        },
        
        mounted: function () {           
            this.selectedConversationId$.subscribe((id) => {
                this.selectedConversationId = id;
                this.buttonsEnabled = true;
                this.$refs.conversationMessagesList.processVisibleUnreadMessages();
                this.$refs.conversationMessagesList.dispatch('conversationReady', id);
            });          
        },

        methods: {
            messagesToMakeReadHandler: function (ids) {
                if (this.$refs.conversationSearch.getConversationIds().indexOf(this.selectedConversationId) < 0) {
                    this.$refs.conversationsList.updateConversation(this.selectedConversationId, ids.length);
                } else {
                    this.$refs.conversationSearch.updateConversation(this.selectedConversationId, ids.length);
                }
            },

            conversationFilter: function (msgConversationId) {
                return this.selectedConversationId == msgConversationId;
            },

            onUnreadConversation: function (msg) {
                let conversationId = msg.conversationId;
                if (this.$refs.conversationSearch.getConversationIds().indexOf(conversationId) < 0) {
                    // now conversation in search list, show it in "Unread" conversations
                    this.$refs.conversationsList.onNewMessage(msg);
                    this.$refs.conversationSearch.ignoreConversation(conversationId);
                } else {
                    this.$refs.conversationSearch.onNewMessage(msg);
                }
            },

            onNewUnreadConversations: function (ids) {                
                this.unreadConversations$.next(ids);
            },

            sendMessage: function () {
                let message = { conversationId: this.selectedConversationId, content: this.newMessage, isAdminMessage: true };
                this.$apiClient.postAsync('api/conversationmessages', message)
                    .then((response) => {
                        let messageData = response.data;
                        this.clearMessage();
                        this.$refs.conversationMessagesList.dispatch('addNewRow', messageData);
                    });                
            },

            clearMessage: function () {
                this.newMessage = '';
            }
        },
        components: {
            UserInfo,
            NavigationMenu,
            ConversationMessagesWrapper,
            ConversationsList,
            ConversationSearch
        }
    }
</script>
<style scoped>
    .conversations-content {
        background-color: azure;
        height: calc(100% - 155px);
        padding-left: 10px;
        overflow-y: auto;
    }

    hr.component-splitter {
      border-top: 2px solid navy;
    }

    .left {
        float: left;
    }

    .conversations-list {
        margin-top: 5px;
        width: 310px;
        padding-right: 10px;
        border-right-color: navy;
        border-right-style: solid;
        height: calc(100vh - 205px);
        overflow-y: auto;
    }

    .conversation-message-list {
        width: calc(100% - 350px);
        margin-left: 20px;
    }

    .conversation-messages-container {
        margin-top: 5px;
        height: calc(100vh - 375px);
        overflow-y: auto;
    }

    .send-message-form-container {
        border-top-style: solid;
        border-top-color: navy;
        margin-top: 5px;
        height: auto;
    }

    .newMessage-input-container {
        margin-bottom: 15px;
    }

    .button-bar {
        padding-bottom: 5px;
    }
</style>