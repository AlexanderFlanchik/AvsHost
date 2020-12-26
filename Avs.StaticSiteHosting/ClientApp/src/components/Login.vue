<template>
    <div class="centered">
        <div id="form-holder">
            <form id="login-form">
                <div>
                    <h1>Welcome to Static Site Hosting!</h1>
                    <div class="error-bar" v-if="errors.length">
                        <ul>
                            <li v-for="error in errors">
                                {{error}}
                            </li>
                        </ul>
                    </div>
                    <div>To access dashboard, please log in.</div>                    
                    <div>
                        <table>
                            <tr>
                                <td>Login: </td>
                                <td><input type="text" v-model="userLogin" /></td>
                            </tr>

                            <tr>
                                <td>Password: </td>
                                <td><input type="password" v-model="password" /></td>
                            </tr>
                        </table>
                    </div>
                </div>
                <div class="button-bar">
                    <button v-on:click="login" class="btn btn-primary">Login</button> &nbsp;
                    <button v-on:click="register" class="btn btn-primary">Register..</button>
                </div>
            </form>
        </div>
    </div>  
</template>

<script>
    export default {
        data: function () {
            return {
                userLogin: '',
                password: '',
                errors: [],
            };
        },
        methods: {
            login: async function (e) {
                e.preventDefault();
                while (this.errors.length) {
                    this.errors.pop();
                }

                if (!this.userLogin) {
                    this.errors.push('Enter user login!');                    
                }

                if (!this.password) {
                    this.errors.push('Enter a password!');
                }

                if (this.errors.length) {                    
                    return;
                }

                let result = await this.$authService.tryGetAccessToken(this.userLogin, this.password);
                if (result) {
                    console.log('Login succeeded.');
                    this.$userNotificationService.init();
                    this.$router.push(this.$route.query.returnUrl || '/');
                } else {
                    this.errors.push('Unable to login: invalid login or password, or service is not available.');
                }                           
            },
            register: function (e) {
                e.preventDefault();
                this.$router.push('/register');
            }
        }
}
</script>
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
    }
</style>