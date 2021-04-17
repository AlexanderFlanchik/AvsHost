<template>
    <div class="conversation-messages-container">
        <ConversationMessages ref="conversationMessagesList" pageSize="50" />
    </div>
</template>
<script>
    import ConversationMessages from '@/components/ConversationMessages.vue';
    import MarkReadMessagesQueue from '@/services/MarkReadMessagesQueue';

    export default {
        props: {
            height: String,
            messagesToMakeReadHandler: Object,
            onUnreadConversation: Object,
            conversationFilter: Object
        },
        data: function () {
            return {
                messagesScrollTop: 0,
                messagesToMakeRead: null,
                getUnreadRows: null,
            };
        },
        mounted: function () {
            this.messagesToMakeRead = new MarkReadMessagesQueue(async (msgs) => {
                return new Promise((resolve, reject) => {
                    let ids = msgs.map(m => m.id);
                    this.$apiClient.postAsync('api/conversationmessages/makeread', ids)
                        .then(() => {
                            this.$refs.conversationMessagesList.markAsViewed(ids);
                            this.messagesToMakeReadHandler && this.messagesToMakeReadHandler(ids);

                            resolve(ids);
                        })
                        .catch((err) => reject(err));
                });
            });

            const $this = this;
            let messagesContainer = document.getElementsByClassName('conversation-messages-container')[0];
            messagesContainer.setAttribute("style", `height: ${this.height}`);

            this.getUnreadRows = () => {
                let containerTop = messagesContainer.getBoundingClientRect().top;
                let inner = messagesContainer.children[0];
                let messageRowsList = inner.children[0];
                let messagesRows = messageRowsList.children;
                let containerBottom = containerTop + messagesContainer.clientHeight;

                //For debug
                //console.log(`getUnreadRows: containerTop = ${containerTop}, containerBottom = ${containerBottom}`);
                //let arr = Array.from(messagesRows).filter(m => m.getAttribute('isviewed') == "false")
                //    .map(m => {
                //        let rect = m.getBoundingClientRect();
                //        return {
                //            id : m.getAttribute("id"),
                //            content: m.innerHTML,
                //            rect: rect,
                //            containerTop: containerTop,
                //            rectTopHigherThanContainerTop: rect.top >= containerTop,
                //            rectTopLowerThanContainerBottom: rect.top < containerBottom,
                //            rectBottomHigherThanContainerTop: rect.bottom > containerTop,
                //            rectBottomLowerThanContainerBottom: rect.bottom <= containerBottom,
                //            rectHeightCondition: rect.height > messagesContainer.clientHeight && rect.top <= containerTop && rect.bottom >= containerBottom
                //        };
                //    });
                
                //if (arr.length) {
                //    console.log(arr);
                //}

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

            this.$userNotificationService.subscribeForUnreadConversation((msg) => {
                let conversationId = msg.conversationId;
                this.onUnreadConversation && this.onUnreadConversation(msg);

                const subscribeForNewMessage = () => {
                    this.$refs.conversationMessagesList.onNewRow(msg);
                    this.messagesToMakeRead.addMessage({ id: msg.id });
                };

                if (typeof this.conversationFilter != 'undefined') {
                    this.conversationFilter(conversationId) && subscribeForNewMessage();
                } else {
                    subscribeForNewMessage();
                }
            });

            messagesContainer.addEventListener('scroll', function (evt) {
                let currentScrollTop = evt.target.scrollTop;
                let direction = $this.messagesScrollTop - currentScrollTop >= 0 ? 'up' : 'down';
                let visibleUnreadMessages = $this.getUnreadRows();
                for (let m of visibleUnreadMessages) {
                    console.log(m);
                    $this.messagesToMakeRead.addMessage({ id: m.getAttribute('id') });
                }

                $this.messagesScrollTop = currentScrollTop;

                if (direction == 'down' && messagesContainer.clientHeight + currentScrollTop >= evt.target.scrollHeight) {
                    console.log('Load next page');
                    $this.$refs.conversationMessagesList.loadNextPage();
                }
            });
        },

        methods: {
            dispatch: function (method, args) {
                this.$refs.conversationMessagesList[method](args);
            },

            processVisibleUnreadMessages: function () {
                this.$refs.conversationMessagesList.onFirstLoaded(() => {
                    let visibleUnreadMessages = this.getUnreadRows();
                    for (let m of visibleUnreadMessages) {
                        this.messagesToMakeRead.addMessage({ id: m.getAttribute('id') });
                    }
                });
            }
        },

        beforeDestroy: function () {
            let channel = this.$userNotificationService.NewConversationMessage;
            this.$userNotificationService.unsubscribe(channel);
        },

        components: {
            ConversationMessages
        }
    }
</script>
<style scoped>
    .conversation-messages-container {
        margin-top: 5px;
        /*height: calc(100vh - 375px);*/
        overflow-y: auto;
        padding-bottom: 5px;
    }
</style>