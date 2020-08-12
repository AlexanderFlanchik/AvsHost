﻿<template>
     <div class="centered">
         <h1>Register New Account</h1>
         <div>
             <table>
                 <tr>
                     <td>Your name:</td>
                     <td>
                        <input type="text" v-model="userName" @change="onUserNameChanged" @blur="fieldBlur('userName')" />
                        <span v-if="isUsernameInvalid" class="validation-error">This user name is already registered.</span>
                        <span v-if="isUsernameEmpty" class="validation-error">User name is required.</span>
                     </td>
                 </tr>
                 <tr>
                     <td>Password:</td>
                     <td>
                        <input type="password" v-model="password"  @blur="fieldBlur('password')" />
                        <span v-if="isPasswordEmpty" class="validation-error">Password is required.</span>
                     </td>
                 </tr>
                 <tr>
                     <td>Confirm password:</td>
                     <td>
                         <input type="password" v-model="confirmPassword"  @blur="fieldBlur('confirmPassword')"/>
                         <span v-if="arePasswordsDifferent" class="validation-error">Passwords must coincide.</span>
                     </td>                     
                 </tr>
                 <tr>
                     <td>Email:</td>
                     <td>
                         <input type="email" v-model="email"  @blur="fieldBlur('email')"  />
                         <span v-if="isEmailInvalid" class="validation-error">This email is already registered.</span>
                         <span v-if="isEmailEmpty" class="validation-error">Email is required.</span>
                     </td>
                 </tr>
             </table>
         </div>
         <div>
             <button v-on:click="register" :disabled="isFormInvalid">Register..</button>
             <button v-on:click="login">Login..</button>
         </div>
     </div>
</template>
<script>
    export default {
        data: function () {
            return {
                email: '',
                emailTouched: false,
                userName: '',
                userNameTouched: false,
                password: '',
                passwordTouched: false,
                confirmPassword: '',
                confirmPasswordTouched: false,
                errors: [],
                remoteValidation: {
                    email: {
                        inProcess: false,
                        success: true
                    },
                    userName: {
                        inProcess: false,
                        success: true
                    }
                }
            };
        },
        mounted: function () {
            if (this.$authService.isAuthenticated()) {
                console.log('User has logged in, redirecting to root..');
                this.$router.push('/');
            }
        },
        computed: {
            isUsernameEmpty: function () {
                return !this.userName && this.userNameTouched;
            },

            isEmailEmpty: function () {
                return !this.email && this.emailTouched;
            },

            isPasswordEmpty: function () {
                return !this.password && this.passwordTouched;
            },

            isEmailInvalid: function () {
                return !this.remoteValidation.email.success;
            },

            isUsernameInvalid: function () {
                return !this.remoteValidation.userName.success;
            },

            isValidationInProcess: function () {
                let validation = this.remoteValidation;
                return validation.email.inProcess || validation.userName.inProcess;
            },

            arePasswordsDifferent: function () {
                return (this.password != this.confirmPassword) && this.confirmPasswordTouched;
            },

            isFormInvalid: function () {
                let notValid = !this.userName ||
                    !this.password ||
                    this.password != this.confirmPassword ||
                    !this.email ||
                    !this.remoteValidation.email.success ||
                    !this.remoteValidation.userName.success;

                return notValid;
            }
        },
        methods: {
            fieldBlur: function (field) {
                let key = field + "Touched";
                this[key] = true;
            },

            register: async function (e) {
                e.preventDefault();

                this.resetForm();
                try {
                    let registrationResult = await this.$authService.register(this.email, this.userName, this.password);
                    if (registrationResult) {
                        this.$router.push('/registered');
                    } else {
                        throw new Error('Unregistered!');
                    }
                } catch (ex) {
                    console.log(ex);
                    this.errors.push('Unable to register a new user. Please contact us.');
                }
            },

            login: function (e) {
                e.preventDefault();
                this.$router.push('/login');
            },

            onUserNameChanged: async function () {
                this.remoteValidation.userName.inProcess = true;
                try {
                    let validationResponse = await this.$authService.validateUserData(this.userName, null);
                    this.remoteValidation.userName.success = validationResponse;
                    console.log(this.remoteValidation.userName.success);
                } finally {
                    this.remoteValidation.userName.inProcess = false;
                }                
            },

            resetForm: function () {
                let touchedProps = [this.userNameTouched, this.emailTouched, this.passwordTouched];
                for (let i = 0; i < touchedProps.length; i++) {
                    touchedProps[i] = false;
                }

                while (this.errors.length) {
                    this.errors.pop();
                }             
            }
        }
    }
</script>
<style scoped>
    .form-title {
        font-weight: bold;
        color: navy;
    }

    .centered {
        margin-left: 25%;
        margin-right: 25%;
        margin-top: 10%;
        margin-bottom: 10%;
        background-color: burlywood;
        padding: 10px;
        justify-content: center;
    }
    .error-bar {
        margin-top: 1px;
        margin-bottom: 1px;
        padding-top: 2px;
        padding-bottom: 2px;
        background-color: lightpink;
        color: crimson;
        font-weight: bold;
    }

    .validation-error {
        color: red;
        font-weight: bold;
    }
</style>