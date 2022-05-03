<template>
    <div class="nav-menu-container">
        <ul class="navigation-menu">
            <li class="navigation-menu-item" v-if="!isAdmin">
                <router-link to="/" class="navigation-menu-item-link" active-class="link-active" exact>Home</router-link>
            </li>
            <li class="navigation-menu-item">
                <router-link to="/dashboard" class="navigation-menu-item-link" active-class="link-active" exact>Dashboard</router-link>
            </li>
            <li class="navigation-menu-item" v-if="isAdmin">
                <router-link to="/settings" class="navigation-menu-item-link" active-class="link-active" exact>Settings</router-link>
            </li>
            <li class="navigation-menu-item">
                <router-link to ="/profile" class="navigation-menu-item-link" active-class="link-active">My Profile</router-link>
            </li>
            <li class="navigation-menu-item" v-if="isAdmin">
                <router-link to ="/conversations" class="navigation-menu-item-link" active-class="link-active">Conversations
                    <span v-if="unreadConversations" class="badge badge-info">NEW</span>
                </router-link>
            </li>
            <li class="navigation-menu-item">
                <router-link to="/event-log" class="navigation-menu-item-link" active-class="link-active">
                    Site Event Logs
                </router-link>
            </li>
            <li class="navigation-menu-item">
                <router-link to="/help" class="navigation-menu-item-link" active-class="link-active">Help</router-link>
            </li>
        </ul>
    </div>
</template>
<script>
    export default {
        data: function () {
            return {
                isAdmin: false,
                unreadConversations: false,                
            };
        },

        mounted: function () {
            let userInfo = this.$authService.getUserInfo();
            this.isAdmin = userInfo ? userInfo.isAdmin : false;

            // on start - check if there are unread conversations
            // and subscribe for new unread messages
            if (this.isAdmin && this.$route.path != "/conversations") {
                this.$userNotificationService.subscribeForUnreadConversation(() => this.unreadConversations = true);               
                this.$apiClient.getAsync('api/conversation/unread').then((response) => {
                    this.unreadConversations = response.data;
                });  
            }
        },
        beforeDestroy: function () {
            let channel = this.$userNotificationService.NewConversationMessage;
            this.$userNotificationService.unsubscribe(channel);            
        }
    }
</script>
<style>
    .link-active {       
        color: red !important;
        font-weight: bold;
    }
    .nav-menu-container {
    }

    .navigation-menu {
        list-style-type: none;
        margin: 0;
        padding: 0;
        color: white;
        overflow: hidden;
        background-color: darkblue;
    }

    .navigation-menu-item {
        float: left;
    }

        .navigation-menu-item:hover {
            background-color: #060343;
        }

        .navigation-menu-item-link {
            text-decoration: none;
            display: block;
            color: white;
            text-align: center;
            padding: 16px;
        }

        .navigation-menu-item-link:hover {
            color: yellow !important;
            text-decoration: none;
        }
</style>