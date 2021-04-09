<template>
    <div class="messages-list-container">
        <div v-if="rows && rows.length">
            <div v-for="row in rows" :key="row" class="message-row" :id="row.id" :isViewed="(row.viewedBy.indexOf(userId) >= 0).toString()">
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
                firstLoaded: false,
                firstLoadedCallback: null,
                pageNumber: 0,
                userId: '',
            };
        },
        mounted: function () {
            this.userId = localStorage.getItem('user-id');
            this.conversationId$.subscribe((convId) => {
                this.conversationId = convId;
                if (this.rows.length) {
                    this.rows = [];                   
                }
                this.completed = false;
                this.pageNumber = 0;

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

                        if (!this.firstLoaded) {
                            setTimeout(() => {
                                console.log('firstLoadedCallback fired.');
                                this.firstLoadedCallback && this.firstLoadedCallback();
                            }, 50);
                        }
                    });
            },

            addNewRow: function (content, dateAdded) {
                this.rows.unshift({ content, dateAdded, viewedBy: [ this.userId ] });
            },

            markAsViewed: async function (rowIds) {
                return new Promise((resolve) => {
                    let rowsToUpdate = this.rows.filter(r => rowIds.indexOf(r.id) >= 0);
                    if (rowsToUpdate && rowsToUpdate.length) {
                        for (var i = 0; i < rowsToUpdate.length; i++) {
                            let r = rowsToUpdate[i];
                            r.viewedBy.push(this.userId);
                        }
                    }

                    resolve(rowsToUpdate);
                });
            },

            onFirstLoaded: function (firstLoadedCallback) {
                console.log('firstLoadedCallback set');
                this.firstLoadedCallback = firstLoadedCallback;
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