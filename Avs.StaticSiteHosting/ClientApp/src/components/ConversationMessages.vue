<template>
    <div class="messages-list-container">
        <div v-if="rows && rows.length">
            <div class="message-row-base" v-for="row in rows" :key="row" v-bind:class="{ 'message-row-own' : userId == row.authorID, 'message-row' : userId != row.authorID }" :id="row.id" :isViewed="(row.viewedBy.indexOf(userId) >= 0).toString()">
                <div class="message-date-base" v-bind:class="{ 'message-date-own' : userId == row.authorID, 'message-date' : userId != row.authorID }" >{{formatDate(row.dateAdded)}}</div>
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
    const moment = require('moment');

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
            console.log(this.userId);
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
            formatDate: function (date) {
                return moment(date).format('MM/DD/YYYY hh:mm:ss A');
            },

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
                            this.firstLoaded = true;
                        }
                    });
            },

            addNewRow: function (msgData) {
                const { content, dateAdded } = msgData;
                this.rows.unshift({ content, dateAdded, viewedBy: [this.userId], authorID: this.userId });
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
                this.firstLoadedCallback = firstLoadedCallback;
            },

            onNewRow: function (msg) {
                let m = {
                    id: msg.id,
                    content: msg.content,
                    dateAdded: msg.dateAdded,
                    viewedBy: [],
                    authorID: msg.authorID
                };

                this.rows.unshift(m);
            }
        }
    }
</script>
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
</style>