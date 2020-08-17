<template>
    <div class="user-info-bar" v-if="isUserInfoShown">
        Welcome, {{userName}}! | <a href="javascript:void(0)" class="sign-out-link" @click="signOff()">Sign out...</a>
    </div>
</template>
<script>
    export default {
        data: function () {
            return {
                userName: ''
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
</style>