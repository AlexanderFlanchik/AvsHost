<template>
    <div class="centered">
        
        <form>
            <div>
            <h1>Welcome to Static Site Hosting!</h1>
            <div>To access dashboard, please log in.</div>
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
        <div>
            <button v-on:click="login">Login</button>
            <button>Register..</button>
        </div>
        </form>
    </div>  
</template>

<script>
    export default {
        userLogin: '',
        password: '',
        methods: {
            login: async function (e) {
                // TODO: apply form validation
                e.preventDefault();

                if (!this.userLogin) {
                    alert('Enter user login!');
                    return;
                }

                if (!this.password) {
                    alert('Enter a password!');
                    return;
                }

                let result = await this.$authService.tryGetAccessToken(this.userLogin, this.password);
                if (result) {
                    console.log('Login succeeded.');
                    this.$router.push(this.$route.query.returnUrl || '/');
                } else {
                    alert('Unable to login.');
                }                           
            },
            register : async function() {

            }
        }
}
</script>
<style scoped>
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