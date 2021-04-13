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
                <ConversationsList :selectedConversationIdSubject="selectedConversationId$" ref="conversationsList" :onNewConversationsLoadedCallback="onNewUnreadConversations"/>
                <hr class="component-splitter" />
                <ConversationSearch :selectedConversationIdSubject="selectedConversationId$" :unreadConversationsSubject="unreadConversations$" ref="conversationSearch" />
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
    import ConversationSearch from '@/components/ConversationSearch.vue';

    import { Subject } from 'rxjs';
    import MarkReadMessagesQueue from '@/services/MarkReadMessagesQueue';

    export default {
        data: function() {
            return {
                selectedConversationId$: new Subject(),
                unreadConversations$: new Subject(),
                selectedConversationId: null,
                buttonsEnabled: false,
                newMessage: '',
                messagesScrollTop: 0,
                messagesToMakeRead: new MarkReadMessagesQueue(async (msgs) => {
                    return new Promise((resolve, reject) => {
                        let ids = msgs.map(m => m.id);
                        this.$apiClient.postAsync('api/conversationmessages/makeread', ids)
                            .then(() => {                                                             
                                this.$refs.conversationMessagesList.markAsViewed(ids);                                
                                if (this.$refs.conversationSearch.getConversationIds().indexOf(this.selectedConversationId) < 0) {                                    
                                    this.$refs.conversationsList.updateConversation(this.selectedConversationId, ids.length);
                                } else {                                    
                                    this.$refs.conversationSearch.updateConversation(this.selectedConversationId, ids.length);
                                }

                                resolve(ids);
                            })
                            .catch((err) => reject(err));
                    });
                })
            };
        },
        
        mounted: function () {
            let messagesContainer = document.getElementsByClassName('conversation-messages-container')[0];
            const $this = this;

            const getUnreadRows = (containerTop) => {
                if (typeof containerTop == 'undefined') {
                    let containerRect = messagesContainer.getBoundingClientRect();
                    containerTop = containerRect.top;
                }

                let inner = messagesContainer.children[0];
                let messageRowsList = inner.children[0];
                let messagesRows = messageRowsList.children;
                let containerBottom = containerTop + messagesContainer.clientHeight;

                let visibleUnreadMessages = Array.from(messagesRows).filter(m => {
                    let rect = m.getBoundingClientRect();
                    let isVisible = (rect.top >= containerTop && rect.top < containerBottom) ||
                        (rect.bottom > containerTop && rect.bottom <= containerBottom) ||
                        (rect.height > messagesContainer.clientHeight && rect.top <= containerTop && rect.bottom >= containerBottom);

                    isVisible = isVisible && m.getAttribute('isviewed') == "false";

                    return isVisible;
                });

                return visibleUnreadMessages;
            };

            this.selectedConversationId$.subscribe((id) => {
                this.selectedConversationId = id;
                this.buttonsEnabled = true;
                this.$refs.conversationMessagesList.onFirstLoaded(() => {
                    let visibleUnreadMessages = getUnreadRows();
                    for (let m of visibleUnreadMessages) {
                        $this.messagesToMakeRead.addMessage({ id: m.getAttribute('id') });
                    }
                });
                this.$refs.conversationMessagesList.conversationReady(id);
            });

            this.$userNotificationService.subscribeForUnreadConversation((msg) => {
                // new message in conversations
                let conversationId = msg.conversationId;
                if (this.$refs.conversationSearch.getConversationIds().indexOf(conversationId) < 0) {
                    // now conversation in search list, show it in "Unread" conversations
                    this.$refs.conversationsList.onNewMessage(msg);
                    this.$refs.conversationSearch.ignoreConversation(conversationId);
                } else {
                    this.$refs.conversationSearch.onNewMessage(msg);
                }

                if (this.selectedConversationId == conversationId) {
                    this.$refs.conversationMessagesList.onNewRow(msg);
                    this.messagesToMakeRead.addMessage({ id: msg.id });
                }
            });

            messagesContainer.addEventListener('scroll', function (evt) {
                let currentScrollTop = evt.target.scrollTop;
                let direction = $this.messagesScrollTop - currentScrollTop >= 0 ? 'up' : 'down';
                let visibleUnreadMessages = getUnreadRows(currentScrollTop);

                for (let m of visibleUnreadMessages) {
                    $this.messagesToMakeRead.addMessage({ id: m.getAttribute('id') });
                }

                $this.messagesScrollTop = currentScrollTop;
                if (direction == 'down' && messagesContainer.clientHeight + currentScrollTop >= evt.target.scrollHeight) {
                    $this.$refs.conversationMessagesList.loadNextPage();
                }
            });
        },
        methods: {
            onNewUnreadConversations: function (ids) {                
                this.unreadConversations$.next(ids);
            },

            sendMessage: function () {
                let message = { conversationId: this.selectedConversationId, content: this.newMessage, isAdminMessage: true };
                this.$apiClient.postAsync('api/conversationmessages', message)
                    .then((response) => {
                        let messageData = response.data;
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