<template>
    <div class="messages-list-container">
        <div v-if="rows && rows.length">
            <div v-for="row in rows" :key="row" class="message-row">
                <div class="message-date">{{row.dateAdded}}</div>
                <div class="message-content">{{row.content}}</div>
            </div>
        </div>
        <div class="no-records-msg" v-if="!rows || !rows.length">
            No messages yet.
        </div>
    </div>
</template>
<script>
    import { Subject } from 'rxjs';

    export default {
        props: {
            pageSize: Object,           
        },
        data: function () {
            return {
                conversationId: '',
                conversationId$: new Subject(),
                completed: false,
                rows: [],
                pageNumber: 0
            };
        },
        mounted: function () {
            this.conversationId$.subscribe((convId) => {
                this.conversationId = convId;
                if (this.rows.length) {
                    this.rows = [];
                    this.pageNumber = 0;
                }

                this.loadNextPage();
            });
        },
        methods: {           
            conversationReady: function (convId) {
                this.conversationId$.next(convId);
            },

            loadNextPage: function () {
                if (!this.conversationId || this.completed) {
                    return;
                }

                this.pageNumber++;
                this.$apiClient.getAsync(`api/conversationmessages/${this.conversationId}?pageNumber=${this.pageNumber}&pageSize=${this.pageSize}`)
                    .then((response) => {
                        let newRows = response.data;
                        if (!newRows.length) {
                            this.completed = true;
                            return;
                        }

                        for (let i = 0; i < newRows.length; i++) {
                            let newRow = newRows[i];
                            this.rows.push(newRow);
                        }                       
                    });                
            },

            addNewRow: function (content, dateAdded) {
                this.rows.unshift({ content, dateAdded });
            }
        }
    }
</script>
<style scoped>
    .messages-list-container {
        padding-top: 5px;
        max-height: inherit;
    }

    .message-row {
        margin-top: 10px;
        padding: 5px;
        background-color: #ECC9FF;
        margin-bottom: 10px;
    }

    .message-date {
        padding-top: 5px;
        padding-bottom: 5px;
        background-color:darkorchid;
        color: white;
    }

    .message-content {
        padding-top: 5px;
    }

    .no-records-msg {
        text-align: center;
        padding-top: 50px;
        color: navy;
        font-weight: bold;
    }
</style>