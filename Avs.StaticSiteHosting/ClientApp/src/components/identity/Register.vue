<script setup lang="ts">
import { computed, inject, onMounted, reactive } from 'vue';
import { useRouter } from 'vue-router';
import { AUTH_SERVICE } from '../../common/diKeys';

interface FieldValidation {
    inProcess: boolean;
    success: boolean;    
}

interface RegisterModel {
    email: string;
    emailTouched: boolean;
    userName: string;
    userNameTouched: boolean;
    password: string;
    passwordTouched: boolean;
    confirmPassword: string;
    confirmPasswordTouched: boolean;
    errors: Array<string>;
    remoteValidation: {
        email: FieldValidation,
        userName: FieldValidation
    }
}

const model = reactive<RegisterModel>({
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
});

const authService = inject(AUTH_SERVICE)!;
const router = useRouter();

onMounted(() => {
    if (authService.isAuthenticated()) {
        router.push('/');
    }
});

const isUsernameEmpty = computed(() => !model.userName && model.userNameTouched);
const isEmailEmpty = computed(() => !model.email && model.emailTouched);
const isPasswordEmpty = computed(() => !model.password && model.passwordTouched);
const isEmailInvalid = computed(() => !model.remoteValidation.email.success);
const isUsernameInvalid = computed(() => !model.remoteValidation.userName.success);
const arePasswordsDifferent = computed(() => (model.password != model.confirmPassword) && model.confirmPasswordTouched);
const isFormInvalid = computed(() => {
    let notValid = !model.userName ||
                    !model.password ||
                    model.password != model.confirmPassword ||
                    !model.email ||
                    !model.remoteValidation.email.success ||
                    !model.remoteValidation.userName.success;

    return notValid;
});

const fieldBlur = (field: string) => {
    const key = field + "Touched";
    
    //@ts-ignore
    model[key] = true;
};

const resetForm = () => {
    const touchedProps = [model.userNameTouched, model.emailTouched, model.passwordTouched, model.confirmPasswordTouched];
    for (let i = 0; i < touchedProps.length; i++) {
        touchedProps[i] = false;
    }

    while (model.errors.length) {
        model.errors.pop();
    }             
};

const register = async (e: any) => {
    e.preventDefault();

    resetForm();
    try {
        const registrationResult = await authService.register(model.email, model.userName, model.password);
        if (registrationResult) {
            router.push('/registered');
        } else {
            throw new Error('Unregistered!');
        }
    } catch (ex) {
        console.log(ex);
        model.errors.push('Unable to register a new user. Please contact us.');
    }
};

const login = (e: any) => {
    e.preventDefault();
    router.push('/login');
};

const onUserNameChanged = async () => {
    model.remoteValidation.userName.inProcess = true;
    try {
        const validationResponse = await authService.validateUserData(model.userName, null);
        model.remoteValidation.userName.success = validationResponse;
    } finally {
        model.remoteValidation.userName.inProcess = false;
    }                
};

const onEmailChanged = async () => {
    model.remoteValidation.email.inProcess = true;
                
    try {
        const validationResponse = await authService.validateUserData(null, model.email);
        model.remoteValidation.email.success = validationResponse;
        console.log(model.remoteValidation.email.success);
    } finally {
        model.remoteValidation.email.inProcess = false;
    }
};

</script>
<template>
   <div class="centered">
      <h1>Register New Account</h1>
      <div>
         <table>
           <tr>
              <td>Your name:</td>
              <td>
                 <input type="text" v-model="model.userName" @change="onUserNameChanged" @blur="fieldBlur('userName')" />
                 <span v-if="isUsernameInvalid" class="validation-error">This user name is already registered.</span>
                 <span v-if="isUsernameEmpty" class="validation-error">User name is required.</span>
              </td>
           </tr>
           <tr>
               <td>Password:</td>
               <td>
                  <input type="password" v-model="model.password" @blur="fieldBlur('password')" />
                  <span v-if="isPasswordEmpty" class="validation-error">Password is required.</span>
               </td>
           </tr>
           <tr>
               <td>Confirm password:</td>
               <td>
                  <input type="password" v-model="model.confirmPassword" @blur="fieldBlur('confirmPassword')" />
                 <span v-if="arePasswordsDifferent" class="validation-error">Passwords must coincide.</span>
               </td>
            </tr>
            <tr>
                <td>Email:</td>
                <td>
                    <input type="email" v-model="model.email" @change="onEmailChanged" @blur="fieldBlur('email')" />
                    <span v-if="isEmailInvalid" class="validation-error">This email is already registered.</span>
                    <span v-if="isEmailEmpty" class="validation-error">Email is required.</span>
                </td>
             </tr>
         </table>
      </div>
      <div class="button-bar">
        <button v-on:click="register" :disabled="isFormInvalid" class="btn btn-primary">Register..</button> &nbsp;
        <button v-on:click="login" class="btn btn-primary">Login..</button>
      </div>
   </div>
</template>
<style scoped>
    .button-bar {
        margin-top: 5px;
    }
</style>