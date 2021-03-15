<template>
    <div class="content-block-container">
        <div class="general-page-title">
            <!--<img src="../../public/profile.png" /> &nbsp;-->
            <span>Conversations</span>
        </div>
        <UserInfo />
        <NavigationMenu />
        <div class="conversations-content">
            <div class="left">
                <ConversationsList />
            </div>

            <div class="left with-left-margin-20">
                Conversation messages for conversation selected
                <br/>
                form for sending messages to user selected
            </div>
        </div>
    </div>
</template>
<script>
    import UserInfo from '@/components/UserInfo.vue';
    import NavigationMenu from '@/components/NavigationMenu.vue';
    import ConversationsList from '@/components/ConversationsList.vue';

    export default {
        data: function() {
            return {};
        },
        
        mounted: function () {
            console.log("Conversation list");
        },

        components: {
            UserInfo,
            NavigationMenu,
            ConversationsList
        }
    }
</script>
<style scoped>
    .conversations-content {
        background-color: azure;
        height: calc(100% - 155px);
        padding-left: 10px;
        overflow-y: auto;
    }

    .left {
        float: left;
    }

    .with-left-margin-20 {
        margin-left: 20px;
        width: auto;
    }
</style>