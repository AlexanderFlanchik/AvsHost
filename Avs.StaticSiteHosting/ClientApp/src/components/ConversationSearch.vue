<template>
    <div>
        <fieldset class="search-fieldset">
            <legend class="conversation-search-title">Conversation search</legend>
            <dl>
                <dd class="name-label">Enter a name:</dd>
                <dt>
                    <v-select v-model="search.selected" label="name" :filterable="false" :options="search.options" @search="onSearch" @input="onInput">
                        <template slot="no-options">
                            <div class="no-options-placeholder">Start enter a conversation name</div>
                        </template>

                        <template slot="option" slot-scope="option">
                            <div>{{option.name}}</div>
                        </template>

                        <template slot="selected-option" slot-scope="option">
                            <div class="option-selected">{{option.name}}</div>
                        </template>
                    </v-select>
                </dt>
            </dl>
            <span class="found-label" v-if="conversations.length">Found:</span>
            <div class="conversation-row"
                 v-bind:class="{'selected-conversation' : selectedConversationId == conversation.id }"
                 v-for="conversation in conversations"
                 :key="conversation"
                 @click="selectConversation(conversation.id)">
                <span class="conversation-label">{{conversation.name}}</span>
                <span class="badge badge-pill badge-primary" style="margin-left: 5px;" v-if="conversation.unreadMessages">{{conversation.unreadMessages}}</span>
            </div>
        </fieldset>
    </div>
</template>
<script>
    export default {
        props: {
            selectedConversationIdSubject: Object,
            unreadConversationsSubject: Object
        },

        data: function () {
            return {
                selectedConversationId: null,
                conversations: [],
                ignoreIds: [],
                search: {
                    selected: null,
                    options: [],
                }
            };
        },

        mounted: function () {
            if (this.unreadConversationsSubject) {
                this.unreadConversationsSubject.subscribe((ids) => {
                    this.ignoreIds = this.ignoreIds.concat(ids);
                    console.log('ignoredIds:');
                    console.log(this.ignoreIds);
                });
            } else {
                console.log('no unreadConversationsSubject passed to search component.');
            }
        },

        methods: {
            getConversationIds: function () {
                return this.conversations.map(c => c.id);
            },

            onInput: function (value) {
                if (value && !this.conversations.find(c => c.id == value.id)) {
                    let conversation = { id: value.id, name: value.name };
                    this.conversations.unshift(conversation);
                    this.search.selected = null;
                    this.search.options = [];
                }
            },

            onSearch: function (search, loading) {
                if (search && search.length) {
                    loading(true);
                    let ignoreConversationIds = (this.conversations.map(c => c.id)).concat(this.ignoreIds);
                    this.$apiClient.postAsync('api/conversation/search', { searchName: search, ignoreConversationIds: ignoreConversationIds })
                        .then((response) => {
                            this.search.options = response.data;
                            loading(false);
                        });
                }
            },

            selectConversation: function (conversationId) {
                this.selectedConversationId = conversationId;
                if (this.selectedConversationIdSubject) {
                    this.selectedConversationIdSubject.next(conversationId);
                }
            },

            onNewMessage: function (messageData) {
                let conversationId = messageData.conversationId;
                let foundConversation = this.conversations.find(c => c.id == conversationId);

                // we have already checked if the foundConversation exists.
                let unread = foundConversation.unreadMessages || 0;
                foundConversation.unreadMessages = unread + 1;

                let i = this.conversations.indexOf(foundConversation);
                this.conversations.splice(i, 1);
                this.conversations.unshift(foundConversation);
            },

            ignoreConversation: function (conversationId) {
                if (this.ignoreIds.indexOf(conversationId) < 0) {
                    this.ignoreIds.push(conversationId);
                }
            }
        }
    }
</script>
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
        color: Highlight;
        font-style: italic;        
    }

    .conversation-row {
        cursor: pointer;
    }

    .selected-conversation {
        background-color: darkgoldenrod;
        font-weight: bold;
    }
</style>