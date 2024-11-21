<script setup lang="ts">
    import { inject, computed, onBeforeUnmount, ref } from 'vue';
    import { API_CLIENT, AUTH_SERVICE, USER_NOTIFICATIONS_SERVICE } from '../../common/diKeys';
    import { useRoute, useRouter } from 'vue-router';

    let isAdmin = false;
    let userName: string;
    let status = ref<string | null>(null);
    let unreadMessages = ref(0);
    let loaded = ref(false);

    const statuses = ['Active', 'Locked'];
    const apiClient = inject(API_CLIENT)!;
    const authService = inject(AUTH_SERVICE)!;
    const userNotificationsService = inject(USER_NOTIFICATIONS_SERVICE)!;
    const currentRoute = useRoute();
    const router = useRouter();
    const userInfo = authService.getUserInfo();

    isAdmin = userInfo ? userInfo.isAdmin : false;
    if (userInfo && userInfo.name) {
        userName = userInfo.name;
        if (!isAdmin) {
            // for usual user we apply these two subscriptions
            userNotificationsService.notify((sts: number) => {
                status.value = statuses[sts];
                if (status.value != 'Active') {
                    authService.lockUser();
                } else {
                    authService.unLockUser();
                }
            });

            userNotificationsService.subscribeForUnreadConversation(() => unreadMessages.value++);
        }
    }

    apiClient.getAsync('api/profile/profile-info')
        .then((infoResponse: any) => {
            loaded.value = true;
            let info = infoResponse.data;
            status.value = statuses[info.status];

            if (info.unreadMessages && currentRoute.path != "/profile") {
                unreadMessages.value = info.unreadMessages;
            }
            if (status.value != 'Active') {
                authService.lockUser();
            } else {
                authService.unLockUser();
            }

            localStorage.setItem('user-id', info.userId);
        });

    const isUserInfoShown = computed(() => authService.isAuthenticated());
    const isUnreadMessageNotificationShown = computed(() => currentRoute.path != "/profile" && unreadMessages.value > 0 && !isAdmin);

    const signOff = () => {
        authService.signOut();
        router.replace({ path: '/login', query: { returnUrl: '/' }});
    };

    onBeforeUnmount(() => {
        if (!isAdmin) {
            let channels = [userNotificationsService.UserStatusChanged, userNotificationsService.NewConversationMessage];
            for (let channel of channels) {
                userNotificationsService.unsubscribe(channel);
            }
        }
    });
</script>
<template>
    <div class="user-info-bar" v-if="isUserInfoShown">
        Welcome, {{userName}}!<span class="badge badge-secondary banned" v-if="status != 'Active' && loaded && !isAdmin">BANNED</span> <img v-if="isUnreadMessageNotificationShown" src="./../../../public/new-message.png" /><sup v-if="isUnreadMessageNotificationShown" class="unread-messages-count">({{unreadMessages}})</sup> | <a href="javascript:void(0)" class="sign-out-link" @click="signOff()">Sign out...</a>
    </div>
</template>
<style scoped>
 .unread-messages-count {
        font-weight: bold;
        font-size: 10pt;
        color: red;
    }

    .user-info-bar {
        text-align: right;
        color: white;
        padding-left: 20px;
        padding-right: 20px;
        padding-top: 3px;
        padding-bottom: 3px;
    }
    .sign-out-link {
        color: white;
        text-decoration: none;
    }

    .sign-out-link:hover {
        color: white;
    }

    .banned {
        color: white;
        margin-left: 5px;
        background-color: red;
    }
</style>