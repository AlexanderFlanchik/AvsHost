<template>
    <h1>Success</h1>
    <div>
        You have successfully registered in the system. <br/>
        Please <router-link to="login">log in</router-link> using your login and password. 
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

</style>