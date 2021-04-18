<template>
    <div class="user-info-bar" v-if="isUserInfoShown">
        Welcome, {{userName}}!<span class="badge badge-secondary banned" v-if="status != 'Active' && loaded && !isAdmin">BANNED</span> <img v-if="isUnreadMessageNotificationShown" src="../../public/new-message.png" /><sup v-if="isUnreadMessageNotificationShown" class="unread-messages-count">({{unreadMessages}})</sup> | <a href="javascript:void(0)" class="sign-out-link" @click="signOff()">Sign out...</a>
    </div>
</template>
<script>
    export default {
        data: function () {
            return {
                isAdmin: false,
                userName: '',
                status: '',
                statuses: ['Active', 'Locked'],
                comment: '',
                unreadMessages: 0,
                loaded: false
            };
        },
        methods: {
            signOff: function () {
                this.$authService.signOut();
                this.$router.replace({ path: '/login', query: { returnUrl: '/' }});
            }
        },
        mounted: function () {
            let userInfo = this.$authService.getUserInfo();
            this.isAdmin = userInfo ? userInfo.isAdmin : false;
            if (userInfo && userInfo.name) {
                this.userName = userInfo.name;
                if (!this.isAdmin) {
                    // for usual user we apply these two subscriptions
                    this.$userNotificationService.notify((status) => {
                        console.log("New status code: " + status);
                        this.status = this.statuses[status];
                        if (this.status != 'Active') {
                            this.$authService.lockUser();
                        } else {
                            this.$authService.unLockUser();
                        }
                    });

                    this.$userNotificationService.subscribeForUnreadConversation(() => {
                        this.unreadMessages++;
                    });
                }
            }

            this.$apiClient.getAsync('api/profile/profile-info')
                .then((infoResponse) => {
                    this.loaded = true;
                    let info = infoResponse.data;
                    this.status = this.statuses[info.status];
                    this.comment = info.comment;
                    if (info.unreadMessages && this.$route.path != "/profile") {
                        this.unreadMessages = info.unreadMessages;
                    }
                    if (this.status != 'Active') {
                        this.$authService.lockUser();
                    } else {
                        this.$authService.unLockUser();
                    }
                    localStorage.setItem('user-id', info.userId);

                    console.log('status:' + this.status);
                });
        },

        beforeDestroy: function () {
            if (!this.isAdmin) {
                let channels = [this.$userNotificationService.UserStatusChanged, this.$userNotificationService.NewConversationMessage];
                for (let channel of channels) {
                    this.$userNotificationService.unsubscribe(channel);
                }
            }
        },

        computed: {
            isUserInfoShown: function () {
                return this.$authService.isAuthenticated();
            },
            isUnreadMessageNotificationShown: function () {
                console.log(this.$route.path);
                return this.$route.path != "/profile" && this.unreadMessages > 0 && !this.isAdmin;
            }
        }
    }
</script>
<style>
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