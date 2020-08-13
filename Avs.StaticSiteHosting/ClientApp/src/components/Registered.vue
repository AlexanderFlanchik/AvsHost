<template>
    <div class="centered">
        <h1>Success</h1>
        <div>
            You have successfully registered in the system. <br />
            Please <router-link to="login">log in</router-link> using your login and password.
        </div>
    </div>
</template>
<script>
    export default {
        data: function () {
            return {};
        },
        mounted: function () {
            if (this.$authService.isAuthenticated()) {
                console.log('User has already logged in.');
                this.$router.push('/');
            }            
        }
    }
</script>
<style>
    .centered {
        margin-left: 25%;
        margin-right: 25%;
        margin-top: 10%;
        margin-bottom: 10%;
        background-color: burlywood;
        padding: 10px;
        justify-content: center;
    }
</style>