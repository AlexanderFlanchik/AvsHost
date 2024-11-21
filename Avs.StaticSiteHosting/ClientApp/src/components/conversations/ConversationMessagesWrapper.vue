<script setup lang="ts">
import { inject, onBeforeUnmount, onMounted, reactive, ref } from 'vue';
import ConversationMessages from './ConversationMessages.vue';
import { MarkReadMessagesQueue } from './../../services/MarkReadMessagesQueue';
import { API_CLIENT, USER_NOTIFICATIONS_SERVICE } from '../../common/diKeys';

interface ConversationMessagesWrapperProps {
    height: string;
    messagesToMakeReadHandler: ((ids: Array<string>) => void) | null | undefined;
    onUnreadConversation: ((msg: any) => void) | null | undefined;
    conversationFilter: ((conversationId: string) => void) | null | undefined;
}

interface ConversationMessagesWrapperModel {
    messagesScrollTop: number;
    messagesToMakeRead?: MarkReadMessagesQueue;
    getUnreadRows?: () => Array<any>;
}

const props = withDefaults(defineProps<ConversationMessagesWrapperProps>(), {
    height: '',
    messagesToMakeReadHandler: null,
    onUnreadConversation: null,
    conversationFilter: null
});

const model = reactive<ConversationMessagesWrapperModel>({
    messagesScrollTop: 0
});

const apiClient = inject(API_CLIENT)!;
const userNotificationsService = inject(USER_NOTIFICATIONS_SERVICE)!;
const conversationMessagesListRef = ref<typeof ConversationMessages | null>(null);

onMounted(() => {
    model.messagesToMakeRead = new MarkReadMessagesQueue(async (msgs: Array<{ id: string }>) => {
        return new Promise<Array<string>>((resolve, reject) => {
            const ids = msgs.map((m: {id: string }) => m.id);
            apiClient.postAsync('api/conversationmessages/makeread', ids)
                .then(() => {
                    conversationMessagesListRef.value?.markAsViewed(ids);
                    props.messagesToMakeReadHandler && props.messagesToMakeReadHandler(ids);

                    resolve(ids);
                })
            .catch((err: Error) => reject(err));
        });
    });

    const messagesContainer = document.getElementsByClassName('conversation-messages-container')[0];
    messagesContainer.setAttribute("style", `height: ${props.height}`);

    model.getUnreadRows = () => {
        const containerTop = messagesContainer.getBoundingClientRect().top;
        const inner = messagesContainer.children[0];
        const messageRowsList = inner.children[0];
        const messagesRows = messageRowsList.children;
        const containerBottom = containerTop + messagesContainer.clientHeight;

        const visibleUnreadMessages = Array.from(messagesRows).filter(m => {
                    let rect = m.getBoundingClientRect();
                    let isVisible = (rect.top >= containerTop && rect.top < containerBottom) ||
                        (rect.bottom > containerTop && rect.bottom <= containerBottom) ||
                        (rect.height > messagesContainer.clientHeight && rect.top <= containerTop && rect.bottom >= containerBottom);

                    isVisible = isVisible && m.getAttribute('isviewed') == "false";

                    return isVisible;
                });

        return visibleUnreadMessages;
    };

    userNotificationsService?.subscribeForUnreadConversation((msg: any) => {
        const conversationId = msg.conversationId;
        props.onUnreadConversation && props.onUnreadConversation(msg);

        const subscribeForNewMessage = () => {
            conversationMessagesListRef.value?.onNewRow(msg);
            model.messagesToMakeRead!.addMessage({ id: msg.id });
        };

        if (props.conversationFilter) {
            props.conversationFilter(conversationId);  
        } 

        subscribeForNewMessage();
    });

    messagesContainer.addEventListener('scroll', function (evt: any) {
        const currentScrollTop = evt.target?.scrollTop;
        const direction = model.messagesScrollTop - currentScrollTop >= 0 ? 'up' : 'down';
        const visibleUnreadMessages = model.getUnreadRows!();
                
        for (const m of visibleUnreadMessages) {
            model.messagesToMakeRead!.addMessage({ id: m.getAttribute('id') });
        }

        model.messagesScrollTop = currentScrollTop;

        if (direction == 'down' && messagesContainer.clientHeight + currentScrollTop >= evt.target.scrollHeight) {
            conversationMessagesListRef.value?.conversationMessagesList.loadNextPage();
        } 
    });
});

const dispatch = (method: any, args: any) => {
    const messagesList = conversationMessagesListRef.value;
    if (!messagesList) {
        return;
    }

    messagesList[method](args);
};

const processVisibleUnreadMessages = () => {
    conversationMessagesListRef.value?.onFirstLoaded(() => {
        const visibleUnreadMessages = model.getUnreadRows!();
        for (let m of visibleUnreadMessages) {
            model.messagesToMakeRead!.addMessage({ id: m.getAttribute('id') });
        }
    });
};

defineExpose({ dispatch, processVisibleUnreadMessages });

onBeforeUnmount(() => {
    const channel = userNotificationsService.NewConversationMessage;
    userNotificationsService.unsubscribe(channel);
});

</script>
<template>
    <div class="conversation-messages-container">
        <ConversationMessages ref="conversationMessagesListRef" :pageSize="50" />
    </div>
</template>
<style scoped>
    .conversation-messages-container {
        margin-top: 5px;
        /*height: calc(100vh - 375px);*/
        overflow-y: auto;
        padding-bottom: 5px;
    }
</style>