<script setup lang="ts">
import { inject, onMounted } from 'vue';
import { AUTH_SERVICE } from '../../common/diKeys';
import { useRouter } from 'vue-router';

 onMounted(() => {
    const authService = inject(AUTH_SERVICE)!;

    if (authService.isAuthenticated()) {
        console.log('User has already logged in.');
        useRouter().push('/');
    }
 });
</script>
<template>
<div class="centered">
        <h1>Success</h1>
        <div>
            You have successfully registered in the system. <br />
            Please <router-link to="login">log in</router-link> using your login and password.
        </div>
    </div>
</template>
<style scoped>

</style>