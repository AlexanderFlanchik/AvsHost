<template>
    <div class="user-info-bar" v-if="isUserInfoShown">
        Welcome, {{userName}}!<span class="badge badge-secondary banned" v-if="status != 'Active'">BANNED</span> | <a href="javascript:void(0)" class="sign-out-link" @click="signOff()">Sign out...</a>
    </div>
</template>
<script>
    export default {
        data: function () {
            return {
                userName: '',
                status: '',
                statuses: ['Active', 'Locked'],
                comment: ''
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
            if (userInfo && userInfo.name) {
                this.userName = userInfo.name;
            }

            this.$apiClient.getAsync('api/profile/profile-info')
                .then((infoResponse) => {
                    let info = infoResponse.data;
                    this.status = this.statuses[info.status];
                    this.comment = info.comment;
                    if (this.status != 'Active') {
                        this.$authService.lockUser();
                    } else {
                        this.$authService.unLockUser();
                    }

                    console.log('status:' + this.status);
                });
        },
        computed: {
            isUserInfoShown: function () {
                return this.$authService.isAuthenticated();
            }
        }
    }
</script>
<style>
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