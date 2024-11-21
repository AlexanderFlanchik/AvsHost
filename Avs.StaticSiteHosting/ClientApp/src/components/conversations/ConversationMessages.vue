<script setup lang="ts">
    import { Subject } from 'rxjs';
    import { reactive, inject, onMounted } from 'vue';
    import { formatDate } from './../../common/DateFormatter';
    import { API_CLIENT } from './../../common/diKeys';

    const apiClient = inject(API_CLIENT)!;

    interface ConversationMessagesModel {
        conversationId: string;
        conversationId$: typeof Subject;
        completed: boolean;
        rows: Array<any>;
        firstLoaded: boolean;
        firstLoadedCallback: (() => {}) | null;
        pageNumber: number;
        userId: string;
    }
    
    const props = defineProps<{ pageSize: number }>();
    const model = reactive<ConversationMessagesModel>({
        conversationId: '',
        //@ts-ignore
        conversationId$: new Subject(),
        completed: false,
        rows: [],
        firstLoaded: false,
        firstLoadedCallback: null,
        pageNumber: 0,
        userId: ''
    });

    //@ts-ignore
    const conversationReady = (convId: any) => model.conversationId$.next(convId);
    const loadNextPage = () => {
        if (!model.conversationId || model.completed) {
            return;
        }

        model.pageNumber++;
        apiClient.getAsync(`api/conversationmessages/${model.conversationId}?pageNumber=${model.pageNumber}&pageSize=${props.pageSize}`)
            .then((response: any) => {
                const newRows = response.data;
                if (!newRows.length) {
                    model.completed = true;
                    console.log("all messages loaded.");
                    return;
                }

                for (let i = 0; i < newRows.length; i++) {
                    let newRow = newRows[i];
                    model.rows.push(newRow);
                }

                if (!model.firstLoaded) {
                    setTimeout(() => model.firstLoadedCallback && model.firstLoadedCallback(), 600);
                    model.firstLoaded = true;
                }
            });
    };

    const addNewRow =(msgData: { content: string, dateAdded: Date }) => {
        const { content, dateAdded } = msgData;
        model.rows.unshift({ content, dateAdded, viewedBy: [model.userId], authorID: model.userId });
    };

    const markAsViewed = async (rowIds: any) => {
        return new Promise((resolve) => {
            setTimeout(() => {
                const rowsToUpdate = model.rows.filter((r: { id: string }) => rowIds.indexOf(r.id) >= 0);
                if (rowsToUpdate && rowsToUpdate.length) {
                    for (var i = 0; i < rowsToUpdate.length; i++) {
                        const r = rowsToUpdate[i];
                        r.viewedBy.push(model.userId);
                    }
                }

                resolve(rowsToUpdate);
            }, 500);
        });
    };

    const onFirstLoaded = (firstLoadedCallback: any) => model.firstLoadedCallback = firstLoadedCallback;
    const onNewRow = (msg: { id: string, content: string, dateAdded: Date, authorID: string }) => {
        const m = {
            id: msg.id,
            content: msg.content,
            dateAdded: msg.dateAdded,
            viewedBy: [],
            authorID: msg.authorID
        };

        model.rows.unshift(m);
    };

    defineExpose({ markAsViewed, addNewRow, onFirstLoaded, onNewRow, conversationReady  });

    onMounted(() => {
        model.userId = localStorage.getItem('user-id') || '';
        //@ts-ignore
        model.conversationId$.subscribe((convId: any) => {
                model.conversationId = convId;
                if (model.rows.length) {
                    model.rows = [];                   
                }

                model.completed = false;
                model.pageNumber = 0;

                loadNextPage();
            });
    });
</script>
<template>
    <div class="messages-list-container">
        <div v-if="model.rows && model.rows.length">
            <div class="message-row-base" v-for="row in model.rows" :key="row" v-bind:class="{ 'message-row-own' : model.userId == row.authorID, 'message-row' : model.userId != row.authorID, 'new-message' : row.viewedBy.indexOf(model.userId) < 0 }" :id="row.id" :isViewed="(row.viewedBy.indexOf(model.userId) >= 0).toString()">
                <div class="message-date-base" v-bind:class="{ 'message-date-own' : model.userId == row.authorID, 'message-date' : model.userId != row.authorID }" >{{formatDate(row.dateAdded)}}</div>
                <div class="message-content">{{row.content}}</div>
            </div>
        </div>
        <div class="no-records-msg" v-if="!model.rows || !model.rows.length">
            No messages yet.
        </div>
    </div>
</template>
<style scoped>
    .messages-list-container {
        padding-top: 5px;
        max-height: inherit;
    }

    .message-row-base {
        margin-top: 10px;
        padding: 5px;
        margin-bottom: 10px;
        border-radius: 8px;
    }

    .message-row-own {
        background-color: darkcyan;
        color: white;
    }

    .message-row {
        background-color: #ECC9FF;        
    }

    .message-date-base {
        padding-top: 5px;
        padding-bottom: 5px;
        padding-left: 5px;
        color: white;
    }

    .message-date {
        background-color:darkorchid;  
    }

    .message-date-own {
        background-color: darkslategrey;        
    }

    .message-content {
        padding-top: 5px;
        padding-left: 5px;
    }

    .no-records-msg {
        text-align: center;
        padding-top: 50px;
        color: navy;
        font-weight: bold;
    }

    .new-message {
        border: 3px solid navy;
    }
</style>