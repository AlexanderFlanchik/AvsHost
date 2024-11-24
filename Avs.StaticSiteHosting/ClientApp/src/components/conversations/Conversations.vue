<script setup lang="ts">
 import { Subject } from 'rxjs';
 import { reactive, ref, onMounted, inject } from 'vue';
 import { API_CLIENT } from './../../common/diKeys';
 import ConversationMessagesWrapper from './ConversationMessagesWrapper.vue';
 import ConversationSearch from './ConversationSearch.vue';
 import ConversationsList from './ConversationsList.vue';

 interface ConversationsModel {
    selectedConversationId$: Subject<any>;
    unreadConversations$: Subject<any>;
    selectedConversationId: string | null;
    buttonsEnabled: boolean;
    newMessage: string;
 }

 const model = reactive<ConversationsModel>({
    selectedConversationId$: new Subject<any>(),
    unreadConversations$: new Subject<any>(),
    selectedConversationId: null,
    buttonsEnabled: false,
    newMessage: ''
 });

 const conversationMessagesListRef = ref<typeof ConversationMessagesWrapper | null>(null);
 const conversationSearchRef = ref<typeof ConversationSearch | null>(null);
 const conversationsListRef = ref<typeof ConversationsList | null>(null);

 onMounted(() => {
    model.selectedConversationId$.subscribe((id: string) => {
        model.selectedConversationId = id;
        model.buttonsEnabled = true;
        conversationMessagesListRef.value?.processVisibleUnreadMessages();
        conversationMessagesListRef.value?.dispatch('conversationReady', id);
    });
 });

 const messagesToMakeReadHandler = (ids: Array<string>) => {
    if (conversationSearchRef.value?.getConversationIds().indexOf(model.selectedConversationId) < 0) {
        conversationsListRef.value?.updateConversation(model.selectedConversationId, ids.length);
    } else {
        conversationSearchRef.value?.updateConversation(model.selectedConversationId, ids.length);
    }
 };

 const conversationFilter = (msgConversationId: string) => model.selectedConversationId == msgConversationId;
 const onUnreadConversation = (msg: any) => {
    const conversationId = msg.conversationId;
    if (conversationSearchRef.value?.getConversationIds().indexOf(conversationId) < 0) {
        // now conversation in search list, show it in "Unread" conversations
        conversationsListRef.value?.onNewMessage(msg);
        conversationSearchRef.value?.ignoreConversation(conversationId);
    } else {
        conversationSearchRef.value?.onNewMessage(msg);
    }
};

const onNewUnreadConversations = (ids: Array<string>) => model.unreadConversations$.next(ids);
const apiClient = inject(API_CLIENT)!;
const clearMessage = () => model.newMessage = '';
const sendMessage = () => {
    const message = { conversationId: model.selectedConversationId, content: model.newMessage, isAdminMessage: true };
    apiClient.postAsync('api/conversationmessages', message)
        .then((response: any) => {
            const messageData = response.data;
            clearMessage();
            conversationMessagesListRef.value?.dispatch('addNewRow', messageData);
        });                
};
</script>
<template>
    <div class="content-block-container">
        <div class="general-page-title">
            <img src="./../../../public/icons8-chat-32.png" /> &nbsp;
            <span>Conversations</span>
        </div>
        <UserInfo />
        <NavigationMenu />
        <div class="conversations-content">
            <div class="left conversations-list">
                <ConversationsList :selectedConversationIdSubject="<Subject<any>>model.selectedConversationId$" ref="conversationsListRef" :onNewConversationsLoadedCallback="onNewUnreadConversations"/>
                <hr class="component-splitter" />
                <ConversationSearch :selectedConversationIdSubject="<Subject<any>>model.selectedConversationId$" :unreadConversationsSubject="<Subject<any>>model.unreadConversations$" ref="conversationSearchRef" />
            </div>

            <div class="left conversation-message-list">
                <ConversationMessagesWrapper ref="conversationMessagesListRef" :messagesToMakeReadHandler="messagesToMakeReadHandler" :conversationFilter="conversationFilter" :onUnreadConversation="onUnreadConversation" :height="'calc(100vh - 375px)'" />
                <div class="send-message-form-container">
                    <div>Enter a message:</div> 
                    
                    <div class="newMessage-input-container">
                        <textarea v-model="model.newMessage" rows="3" :disabled="!model.buttonsEnabled"></textarea>
                    </div>                    
                </div>
                <div class="button-bar">
                    <button class="btn btn-primary" :disabled="!model.newMessage || !model.buttonsEnabled" @click="sendMessage">Send..</button> &nbsp;
                    <button class="btn btn-default" :disabled="!model.newMessage|| !model.buttonsEnabled" @click="clearMessage">Clear</button>
                </div>
            </div>
        </div>
    </div>
</template>
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
        margin-top: 3px;
        height: auto;
    }

    .newMessage-input-container {
        margin-bottom: 15px;
    }

    .button-bar {
        padding-bottom: 5px;
    }
</style>