<script setup lang="ts">
import { reactive, onMounted, inject, watch } from 'vue';
import { Subject } from 'rxjs';
import { API_CLIENT } from './../../common/diKeys';

interface ConversationSearchModel {
    selectedConversationId: string | null;
    conversations: Array<any>;
    ignoreIds: Array<string>;
    search: {
        selected: any;
        options: Array<any>;
    }
}

const props = defineProps<{ selectedConversationIdSubject?: Subject<any>, unreadConversationsSubject?: Subject<any> }>();
const model = reactive<ConversationSearchModel>({
    selectedConversationId: null,
    conversations: [],
    ignoreIds: [],
    search: {
        selected: null,
        options: []
    }
});

onMounted(() => {
    if (props.unreadConversationsSubject) {
        props.unreadConversationsSubject.subscribe((ids: any) => model.ignoreIds = model.ignoreIds.concat(ids));
    }
});

const getConversationIds =  () => model.conversations.map((c: { id: string}) => c.id);
const apiClient = inject(API_CLIENT)!;

const onSearch = (search: string, loading:(val: boolean) => void) => {
    if (search && search.length) {
        loading(true);
        const ignoreConversationIds = (model.conversations.map((c: any) => c.id)).concat(model.ignoreIds);
        apiClient.postAsync('api/conversation/search', { searchName: search, ignoreConversationIds: ignoreConversationIds })
            .then((response: any) => {
                model.search.options = response.data;
                loading(false);
            });
    }
};

const selectConversation = (conversationId: string) => {
    model.selectedConversationId = conversationId;
    if (props.selectedConversationIdSubject) {
        props.selectedConversationIdSubject.next(conversationId);
    }
};

const onNewMessage = (messageData: { conversationId: string }) => {
    const conversationId = messageData.conversationId;
    const foundConversation = model.conversations.find((c: { id: string }) => c.id == conversationId);

    // we have already checked if the foundConversation exists.
    const unread = foundConversation.unreadMessages || 0;
    foundConversation.unreadMessages = unread + 1;

    const i = model.conversations.indexOf(foundConversation);
    model.conversations.splice(i, 1);
    model.conversations.unshift(foundConversation);
};

const ignoreConversation = (conversationId: string) => {
    if (model.ignoreIds.indexOf(conversationId) < 0) {
        model.ignoreIds.push(conversationId);
    }
};

const updateConversation = (conversationId: string, readMessagesCount: number) => {                
    const conversation = model.conversations.find((c: { id: string }) => c.id == conversationId);
    if (!conversation) {
        return;
    }

    setTimeout(() => {
        conversation.unreadMessages = conversation.unreadMessages >= readMessagesCount ? conversation.unreadMessages - readMessagesCount : 0;
        console.log(conversation.unreadMessages);
    });
};

defineExpose({ ignoreConversation, onNewMessage, updateConversation, getConversationIds })
watch(() => model.search.selected, (value: { id: string, name: string } | null | undefined) => {
    if (value && !model.conversations.find((c: any) => c.id == value.id)) {
        const conversation = { id: value.id, name: value.name };
        
        model.conversations.unshift(conversation);
        model.search.selected = null;
        model.search.options = [];
    }
});
</script>
<template>
    <div>
        <fieldset class="search-fieldset">
            <legend class="conversation-search-title">Conversation search</legend>
            <dl>
                <dd class="name-label">Enter a name:</dd>
                <dt>
                    <v-select v-model="model.search.selected" label="name" :filterable="false" :options="model.search.options" @search="onSearch">
                        <template slot="no-options">
                            <div class="no-options-placeholder">Start enter a conversation name</div>
                        </template>

                        <template #option="option" slot="option" slot-scope="option">
                            <div>{{option.name}}</div>
                        </template>

                        <template #selectedOption="option" slot="selected-option" slot-scope="option">
                            <div class="option-selected">{{option.name}}</div>
                        </template>
                    </v-select>
                </dt>
            </dl>
            <span class="found-label" v-if="model.conversations.length">Found:</span>
            <div class="conversation-row"
                 v-bind:class="{'selected-conversation' : model.selectedConversationId == conversation.id }"
                 v-for="conversation in model.conversations"
                 :key="conversation"
                 @click="selectConversation(conversation.id)">
                <span class="conversation-label">{{conversation.name}}</span>
                <span class="badge badge-pill badge-primary" style="margin-left: 5px;" v-if="conversation.unreadMessages">{{conversation.unreadMessages}}</span>
            </div>
        </fieldset>
    </div>
</template>
<style scoped>
    .search-fieldset {
        padding-top: 5px;
        padding-left: 5px;
    }

    .found-label {
        color: navy;
    }

    .no-options-placeholder {
        font-size: smaller;
        color: navy;
        font-weight: bold;
    }

    .conversation-search-title {
        font-size: small;
        color: navy;
        font-weight: bold;
    }

    .name-label {
        font-size: smaller;
        color: navy;
    }

    .option-selected {
        background-color: beige;
        color: navy;
    }

    .conversation-label {
        color: Navy;
        font-style: italic;
        padding-left: 5px;
    }

    .conversation-row {
        cursor: pointer;
    }

    .selected-conversation {
        background-color: darkgoldenrod;
        font-weight: bold;
    }
</style>