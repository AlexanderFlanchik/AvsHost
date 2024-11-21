<script setup lang="ts">
import { reactive, inject, onMounted } from 'vue';
import { Subject } from 'rxjs';
import { API_CLIENT } from './../../common/diKeys';

interface ConversationsListModel {
    selectedConversationId: string;
    pageNumber: number;
    pageSize: number;
    conversations: Array<any>;
    totalConversations: number;
    conversationsToLoad: number;
}

const props = defineProps<{
    selectedConversationIdSubject: Subject<any>; 
    onNewConversationsLoadedCallback?: (ids: Array<string>) => void  
 }>();

const model = reactive<ConversationsListModel>({
    selectedConversationId: '',
    pageNumber: 1,
    pageSize: 50,
    conversations: [],
    totalConversations: 0,
    conversationsToLoad: 0
});

const getConversationIds = () => model.conversations.map((c: { id: string }) => c.id);
const apiClient = inject(API_CLIENT)!;

const loadConversations = () => {
    apiClient.getAsync(`api/conversationlist?pageSize=${model.pageSize}&pageNumber=${model.pageNumber}`)
        .then((response: any) => {
            if (!model.totalConversations) {
                model.totalConversations = Number(response.headers["total-conversations"]);
                model.conversationsToLoad = model.totalConversations;
            }

            const ids = [];
            const rows = response.data;
            for (const row of rows) {
                model.conversations.push(row);
                ids.push(row.id);
            }

            props.onNewConversationsLoadedCallback && props.onNewConversationsLoadedCallback(ids);
            model.conversationsToLoad -= rows.length;
        });
};

const loadMore = () => {
    model.pageNumber++;
    loadConversations();
};

const selectConversation = (conversationId: string) => {
    console.log(conversationId + ' selected');
    model.selectedConversationId = conversationId;
    if (props.selectedConversationIdSubject) {
        props.selectedConversationIdSubject.next(model.selectedConversationId);
    }
};

const onNewMessage = (messageData: any) => {
    const conversationId = messageData.conversationId;
    const foundConversation = model.conversations.find((c: { id: string }) => c.id == conversationId);
    if (foundConversation) {
        const unread = foundConversation.unreadMessages || 0;
        foundConversation.unreadMessages = unread + 1;
        const i = model.conversations.indexOf(foundConversation);
        model.conversations.splice(i, 1);
        model.conversations.unshift(foundConversation);
    } else {
        apiClient.getAsync(`api/conversation/${conversationId}`).then((response: any) => {
            const conversation = response.data.conversation;
            model.conversations.unshift({ id: conversationId, name: conversation.name, unreadMessages: 1 });
        });
    }
};

const updateConversation = (conversationId: string, readMessagesCount: number) => {
    const conversation = model.conversations.find((c: any) => c.id == conversationId);
    if (!conversation) {
        return;
    }

    setTimeout(() => {
        conversation.unreadMessages = conversation.unreadMessages >= readMessagesCount ? conversation.unreadMessages - readMessagesCount : 0;
    });
};

onMounted(() => loadConversations());

defineExpose({ getConversationIds, onNewMessage, updateConversation });

</script>
<template>
    <div class="unread-conversations-container">
        <div v-if="model.conversations && model.conversations.length">
            <div class="conversation-row" v-for="conversation in model.conversations" :key="conversation" 
                 v-bind:class="{'selected-conversation': conversation.id == model.selectedConversationId }"
                 @click="selectConversation(conversation.id)">
                <span class="conversation-name">{{conversation.name}}</span>
                <span class="badge badge-pill badge-primary" style="margin-left: 5px;" v-if="conversation.unreadMessages">{{conversation.unreadMessages}}</span>
            </div>
            <div v-if="model.conversationsToLoad > 0">
                <a href="javascript:void(0)" @click="loadMore()">Load more..</a>
            </div>
        </div>
        <div v-if="!model.conversations.length">
            <span class="no-conversation-message">No unread conversations</span>
        </div>
    </div>
</template>
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