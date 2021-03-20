<template>
    <div class="content-block-container">
        <div class="general-page-title">
            <!--<img src="../../public/profile.png" /> &nbsp;-->
            <span>Conversations</span>
        </div>
        <UserInfo />
        <NavigationMenu />
        <div class="conversations-content">
            <div class="left conversations-list">
                <ConversationsList :selectedConversationIdSubject="selectedConversationId$"/>
            </div>

            <div class="left conversation-message-list">
                <div class="conversation-messages-container">
                    <ConversationMessages ref="conversationMessagesList" pageSize="50" />
                </div>
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
    import ConversationMessages from '@/components/ConversationMessages.vue';
    import { Subject } from 'rxjs';

    export default {
        data: function() {
            return {
                selectedConversationId$: new Subject(),
                selectedConversationId: null,
                buttonsEnabled: false,
                newMessage: '',
                messagesScrollTop: 0,
            };
        },
        
        mounted: function () {
            this.selectedConversationId$.subscribe((id) => {
                console.log("New conversation selected. Id = " + id);
                this.selectedConversationId = id;
                this.buttonsEnabled = true;
                this.$refs.conversationMessagesList.conversationReady(id);
            });

            let messagesContainer = document.getElementsByClassName('conversation-messages-container')[0];
            var $this = this;

            messagesContainer.addEventListener('scroll', function (evt) {
                let currentScrollTop = evt.target.scrollTop;
                let direction = $this.messagesScrollTop - currentScrollTop >= 0 ? 'up' : 'down';

                $this.messagesScrollTop = currentScrollTop;
                if (direction == 'down' && messagesContainer.clientHeight + currentScrollTop >= evt.target.scrollHeight) {
                    $this.$refs.conversationMessagesList.loadNextPage();
                }
            });
        },
        methods: {
            sendMessage: function () {
                console.log('Sending message: ' + this.newMessage);
                let message = { conversationId: this.selectedConversationId, content: this.newMessage, isAdminMessage: true };
                this.$apiClient.postAsync('api/conversationmessages', message)
                    .then((response) => {
                        let messageData = response.data;
                        console.log('Message sent.');
                        console.log(messageData);
                        this.clearMessage();
                        this.$refs.conversationMessagesList.addNewRow(messageData.content, messageData.dateAdded);
                    });                
            },

            clearMessage: function () {
                this.newMessage = '';
            }
        },
        components: {
            UserInfo,
            NavigationMenu,
            ConversationMessages,
            ConversationsList
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

    .left {
        float: left;
    }

    .conversations-list {
        margin-top: 5px;
        width: 300px;
        padding-right: 3px;
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