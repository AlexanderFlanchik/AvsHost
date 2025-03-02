<script setup lang="ts">
import { inject, reactive } from 'vue';
import { AUTH_SERVICE, USER_NOTIFICATIONS_SERVICE } from '../../common/diKeys';
import { useRouter } from 'vue-router';

 interface LoginModel {
    userLogin: string;
    password: string;
    errors: Array<string>
 }

 const model = reactive<LoginModel>({
    userLogin: '',
    password: '',
    errors: []
 });

 const authService = inject(AUTH_SERVICE)!;
 const userNotificationsService = inject(USER_NOTIFICATIONS_SERVICE)!;
 const router = useRouter();
  
 const login = async (e: any) => {
    e.preventDefault();
    while (model.errors.length) {
        model.errors.pop();
    }

    if (!model.userLogin) {
        model.errors.push('Enter user login!');                    
    }

    if (!model.password) {
        model.errors.push('Enter a password!');
    }

    if (model.errors.length) {                    
        return;
    }

    const result = await authService.tryGetAccessToken(model.userLogin, model.password);
    if (result) {
        console.log('Login succeeded.');
        userNotificationsService.init();
        const query = router.currentRoute?.value?.query;
        const redirectUrl = query && query["redirectUrl"] ? <string>query["redirectUrl"] : "/";
        router.push(redirectUrl);
    } else {
        model.errors.push('Unable to login: invalid login or password, or service is not available.');
    }
 };

const register = (e: any) => {
    e.preventDefault();
    router.push('/register');
};
</script>
<template>
    <div class="centered">
        <div id="form-holder">
            <form id="login-form">
                <div>
                    <h1>Welcome to Static Site Hosting!</h1>
                    <div class="error-bar" v-if="model.errors.length">
                        <ul>
                            <li v-for="error in model.errors" :key="error">
                                {{error}}
                            </li>
                        </ul>
                    </div>
                    <div>To access dashboard, please log in.</div>                    
                    <div>
                        <table>
                            <tr>
                                <td>Login: </td>
                                <td><input type="text" v-model="model.userLogin" /></td>
                            </tr>

                            <tr>
                                <td>Password: </td>
                                <td><input type="password" v-model="model.password" /></td>
                            </tr>
                        </table>
                    </div>
                </div>
                <div class="button-bar">
                    <button v-on:click="login" class="btn btn-primary">Login</button>
                    <button v-on:click="register" class="btn btn-primary">Register..</button>
                </div>
            </form>
        </div>
    </div>
</template>
<style scoped>
.error-bar {
    margin-top: 1px;
    margin-bottom: 1px;
    padding-top: 2px;
    padding-bottom: 2px;
    background-color: lightpink;
    color: crimson;
    font-weight: bold;
 }
.button-bar {
     margin-top: 5px;
    display: flex;
    gap: 3px;
}
</style>