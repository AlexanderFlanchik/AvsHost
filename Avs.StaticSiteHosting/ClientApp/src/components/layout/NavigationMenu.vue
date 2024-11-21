<script setup lang="ts">
    import { inject, onBeforeUnmount, onMounted, reactive } from 'vue';
    import { API_CLIENT, AUTH_SERVICE, USER_NOTIFICATIONS_SERVICE } from '../../common/diKeys';
    import { useRoute } from 'vue-router';

    const nm = reactive({
        isAdmin: false,
        unreadConversations: false,    
    });

    const authService = inject(AUTH_SERVICE)!;
    const userNotificationsService = inject(USER_NOTIFICATIONS_SERVICE)!;
    const apiClient = inject(API_CLIENT)!;
    const currentRoute = useRoute();

    onMounted(() => {
        const userInfo = authService.getUserInfo();
            nm.isAdmin = userInfo ? userInfo.isAdmin : false;

            // on start - check if there are unread conversations
            // and subscribe for new unread messages
            if (nm.isAdmin && currentRoute.path != "/conversations") {
                userNotificationsService.subscribeForUnreadConversation(() => nm.unreadConversations = true);               
                apiClient.getAsync('api/conversation/unread').then((response: any) => {
                    nm.unreadConversations = response.data;
                });  
            }
    });

    onBeforeUnmount(() => {
        let channel = userNotificationsService.NewConversationMessage;
        userNotificationsService.unsubscribe(channel); 
    });

</script>
<template>
 <div class="nav-menu-container">
        <ul class="navigation-menu">
            <li class="navigation-menu-item" v-if="!nm.isAdmin">
                <router-link to="/" class="navigation-menu-item-link" active-class="link-active" exact>Home</router-link>
            </li>
            <li class="navigation-menu-item">
                <router-link to="/dashboard" class="navigation-menu-item-link" active-class="link-active" exact>Dashboard</router-link>
            </li>
            <li class="navigation-menu-item" v-if="nm.isAdmin">
                <router-link to="/settings" class="navigation-menu-item-link" active-class="link-active" exact>Settings</router-link>
            </li>
            <li class="navigation-menu-item">
                <router-link to ="/profile" class="navigation-menu-item-link" active-class="link-active">My Profile</router-link>
            </li>
            <li class="navigation-menu-item" v-if="nm.isAdmin">
                <router-link to ="/conversations" class="navigation-menu-item-link" active-class="link-active">Conversations
                    <span v-if="nm.unreadConversations" class="badge badge-info">NEW</span>
                </router-link>
            </li>
            <li class="navigation-menu-item">
                <router-link to="/event-log" class="navigation-menu-item-link" active-class="link-active">
                    Site Event Logs
                </router-link>
            </li>
            <li class="navigation-menu-item" v-if="!nm.isAdmin">
                <router-link to="/tags" class="navigation-menu-item-link" active-class="link-active">
                    Tags
                </router-link>
            </li>
            <li class="navigation-menu-item" v-if="!nm.isAdmin">
                <router-link to="/reports" class="navigation-menu-item-link" active-class="link-active">
                    Reports
                </router-link>
            </li>
            <li class="navigation-menu-item">
                <router-link to="/help" class="navigation-menu-item-link" active-class="link-active">Help</router-link>
            </li>
        </ul>
    </div>
</template>
<style scoped>
  .link-active {       
        color: red !important;
        font-weight: bold;
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