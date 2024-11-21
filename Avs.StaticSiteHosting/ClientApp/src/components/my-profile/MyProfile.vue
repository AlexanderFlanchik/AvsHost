<script setup lang="ts">
 import { inject, reactive, ref, onMounted, computed } from 'vue';
 import { useRouter } from 'vue-router';
 import { API_CLIENT, AUTH_SERVICE } from '../../common/diKeys';
 import ConversationMessagesWrapper from './../conversations/ConversationMessagesWrapper.vue';
 import UserInfo from '../layout/UserInfo.vue';
 import NavigationMenu from '../layout/NavigationMenu.vue';
 
 interface UserInfo {
    userName: string;
    userEmail: string;
    isAdmin: boolean;
 }

 interface MyProfileModel {
    isAdmin:  boolean;
    userName: string;
    userEmail: string;
    userPassword: string;
    newPassword: string;
    confirmNewPassword: string;
    originalUserInfo: UserInfo | null;   
    updateResult: string;
    updateResultShown: boolean;
    changePasswordResult: string;
    changePasswordResultShown: boolean;
    changePasswordFailed: boolean;
    conversationId: string;
    newMessage: string;
    validation: {
        userName: {
            inProcess: boolean,
            success: boolean,
            touched: boolean
        },
        userEmail: {
            inProcess: boolean,
            success: boolean,
            touched: boolean
        },
        userPassword: {
            success: boolean,
            touched: boolean
        },
        newPassword: {
            success: boolean,
            touched: boolean
        },
        confirmNewPassword: {
            success: boolean,
            touched: boolean
        }
    }
 }

 const model = reactive<MyProfileModel>({
    isAdmin: false,
    userName: '',
    userEmail: '',
    userPassword: '',
    newPassword: '',
    confirmNewPassword: '',
    originalUserInfo: null,
    updateResult: '',
    updateResultShown: false,
    changePasswordResult: '',
    changePasswordResultShown: false,
    changePasswordFailed: false,
    conversationId: '',
    newMessage: '',
    validation: {
        userName: {
            inProcess: false,
            success: true,
            touched: false
        },
        userEmail: {
            inProcess: false,
            success: true,
            touched: false
        },
        userPassword: {
            success: true,
            touched: false
        },
        newPassword: {
            success: true,
            touched: false
        },
        confirmNewPassword: {
            success: true,
            touched: false
        }
    }
 });

 const apiClient = inject(API_CLIENT)!;
 const authService = inject(AUTH_SERVICE)!;
 const conversationMessagesListRef = ref<typeof ConversationMessagesWrapper | null>(null);

 const showUpdateMessage = (msg: string) => {
    model.updateResult = msg;
    model.updateResultShown = true;
                    
    setTimeout(() => {
        model.updateResultShown = false;
        model.updateResult = '';
    }, 1000);
 };

 const showChangePasswordMessage = (msg: string, isError: boolean) => {
    model.changePasswordResult = msg;
    model.changePasswordResultShown = true;
    model.changePasswordFailed = isError;

    setTimeout(() => {
        model.changePasswordResult = '';
        model.changePasswordResultShown = false;
        model.changePasswordFailed = false;
    }, 1000);
 };

 onMounted(() => {
    const userInfo = authService.getUserInfo();
    if (userInfo) {
        model.userName = userInfo.name;
        model.userEmail = userInfo.email;
        model.isAdmin = userInfo.isAdmin;
    }
    
    model.originalUserInfo = Object.freeze({ userName: userInfo.name, userEmail: userInfo.email, isAdmin: userInfo.isAdmin });

    apiClient.getAsync('api/conversation').then((response: any) => {
        const conversation = response.data.conversation;
                
        if (conversation) {
            model.conversationId = conversation.id;
            conversationMessagesListRef.value?.dispatch('conversationReady', model.conversationId);
        }
    });
 });

 const validateUserName = async() => {
    if (model.userName == model.originalUserInfo!.userName) {
        return;
    }

    const userNameValidation = model.validation.userName;

    try {
        userNameValidation.inProcess = true;
        const validationResult = await authService.validateUserData(model.userName, null);
        userNameValidation.success = validationResult;
    } finally {
        userNameValidation.inProcess = false;
    }
 };

 const validateUserEmail = async () => {
    if (model.userEmail == model.originalUserInfo!.userEmail) {
        return;
    }

    const userEmailValidation = model.validation.userEmail;

    try {
        userEmailValidation.inProcess = true;
        const validationResult = await authService.validateUserData(null, model.userEmail);
        userEmailValidation.success = validationResult;
    } finally {
        userEmailValidation.inProcess = false;
    }
 };

 const update = () => {
    const updateData = {
        email: model.userEmail,
        userName: model.userName
    };

    apiClient.postAsync('api/profile/update', updateData).then(() => {
        authService.signOut();
        
        useRouter().replace('/');
    }).catch((e: any) => showUpdateMessage(e.response && e.response.data));
 };

 const changePassword = () => {
    const pwdData = {
        password: model.userPassword,
        newPassword: model.newPassword
    };

    apiClient.postAsync('api/profile/changepassword', pwdData).then(() => {
        showChangePasswordMessage("Password changed.", false);
    }).catch((e: any) => showChangePasswordMessage(e.response && e.response.data, true));
 };

 const sendMessage = () => {
    const postMessage = () => {
        const data = {
            content: model.newMessage,
            conversationId: model.conversationId                        
        };
                    
        console.log(data);
        apiClient.postAsync('api/conversationmessages', data).then((newRowResponse: any) => {
            const content = newRowResponse.data.content;
            const dateAdded = newRowResponse.data.dateAdded;

            conversationMessagesListRef.value?.dispatch('addNewRow', { content, dateAdded });
            model.newMessage = '';
        });
    };

    if (!model.conversationId) {
        console.log('No conversation id, creating...');
        apiClient.postAsync('api/conversation').then((response: any) => {
            model.conversationId = response.data.id;
            postMessage();
        });
    } else {
        postMessage();
    }                
 };

 const isUpdateDisabled = computed(() =>{
    if (model.originalUserInfo && model.originalUserInfo.userName == model.userName && 
        model.originalUserInfo.userEmail == model.userEmail) {
            return true;
        }

    const userNameValidation = model.validation.userName;
    const userEmailValidation = model.validation.userEmail;
                
    const userNameValidationResult = model.userName
            && model.userName.length
            && !userNameValidation.inProcess
            && userNameValidation.success;

    const userEmailValidationResult = model.userEmail
            && model.userEmail.length
            && !userEmailValidation.inProcess
            && userEmailValidation.success;

    return !userNameValidationResult || !userEmailValidationResult;
 });

 const isChangePasswordDisabled = computed(() => {
    if (model.newPassword == model.userPassword) {
        return true;
    }

    const passwordValidationResult = model.newPassword
        && model.newPassword.length
        && model.newPassword == model.confirmNewPassword;

    return !passwordValidationResult;
 });
 </script>
<template>
<div class="content-block-container">
        <div class="general-page-title">
            <img src="../../../public/profile.png" />
            <span>Profile</span>
        </div>
        <UserInfo />
        <NavigationMenu />
        <div class="profile-content">
            <div class="left">
                <fieldset class="profile-fields-container">
                    <legend>Profile settings</legend>
                    <dl>
                        <dt>
                            Name:
                        </dt>
                        <dd>
                            <input type="text" v-model="model.userName" @blur="model.validation.userName.touched=true;" @input="validateUserName" />
                            <span class="validation-error" v-if="model.userName && model.validation.userName.touched && !model.validation.userName.success">This name is already in use.</span>
                            <span class="validation-error" v-if="model.validation.userName.touched && !model.userName">The name is required.</span>
                        </dd>

                        <dt>
                            Email:
                        </dt>
                        <dd>
                            <input type="text" v-model="model.userEmail" @blur="model.validation.userEmail.touched=true;" @input="validateUserEmail" />
                            <span class="validation-error" v-if="model.userEmail && model.validation.userEmail.touched && !model.validation.userEmail.success">This e-mail is already in use.</span>
                            <span class="validation-error" v-if="model.validation.userEmail.touched && !model.userEmail">The e-mail is required.</span>
                        </dd>
                    </dl>
                    <div class="button-container">
                        <span class="update-result-msg err-msg" v-if="model.updateResultShown">{{model.updateResult}}</span>
                        <button class="btn btn-primary" :disabled="isUpdateDisabled" @click="update">Update</button>
                    </div>
                </fieldset>

                <fieldset class="profile-fields-container">
                    <legend>Password Management</legend>
                    <dl>
                        <dt>Current password:</dt>
                        <dd>
                            <input type="password" v-model="model.userPassword" @blur="model.validation.userPassword.touched=true;" />
                            <span class="validation-error" v-if="model.validation.userPassword.touched && !model.userPassword">Current password is required.</span>
                        </dd>
                        <dt>New password:</dt>
                        <dd>
                            <input type="password" v-model="model.newPassword" @blur="model.validation.newPassword.touched=true;" />
                            <span class="validation-error" v-if="model.validation.newPassword.touched && !model.newPassword">New password is required.</span>
                        </dd>
                        <dt>New password confirm:</dt>
                        <dd>
                            <input type="password" v-model="model.confirmNewPassword" @blur="model.validation.confirmNewPassword.touched=true;" />
                            <span class="validation-error" v-if="model.validation.confirmNewPassword.touched && !model.confirmNewPassword">Confirm new password is required.</span>
                            <span class="validation-error"
                                  v-if="model.newPassword && model.confirmNewPassword && model.validation.confirmNewPassword.touched && model.confirmNewPassword != model.newPassword">New and confirmed passwords must be the same.</span>
                        </dd>
                    </dl>
                    <div class="button-container">
                        <span class="update-result-msg" v-if="model.changePasswordResultShown" v-bind:class="{'err-msg': model.changePasswordFailed, 'normal-msg': !model.changePasswordFailed }">{{model.changePasswordResult}}</span>
                        <button class="btn btn-primary" :disabled="isChangePasswordDisabled" @click="changePassword">Change Password..</button>
                    </div>
                </fieldset>
            </div>
         
            <div class="left with-left-margin-20 conversation-msg-container" v-if="!model.isAdmin">
                <fieldset class="profile-fields-container conversation-fieldset">
                    <legend>Communication with Administrator</legend>
                    <dl>
                        <dt>Type a message here:</dt>
                        <dd>
                            <div class="newMessage-input-container">
                                <textarea v-model="model.newMessage" rows="5"></textarea>
                            </div>

                            <div class="button-container">
                                <button class="btn btn-primary" :disabled="!model.newMessage" @click="sendMessage">Send..</button>
                            </div>                          
                        </dd>
                    </dl>
                    <dl>
                        <dt>Messages history:</dt>
                        <dd>                            
                            <ConversationMessagesWrapper ref="conversationMessagesListRef" :height="'calc(100vh - 390px)'"  />
                        </dd>
                    </dl>
                </fieldset>
            </div>
        </div>
    </div>
</template>
<style scoped>
    .content-block-container {
        height: calc(100% - 175px);
    }
    .profile-content {
        background-color: azure;
        height: calc(100vh - 167px);
        padding-left: 10px;
        overflow-y: auto;
    }

    .with-left-margin-20 {
        margin-left: 20px;
        width: auto;
    }

    .left {
        float: left;
    }

    .profile-fields-container {
        border-radius: 5px;
        width: 500px;
        border: 1px solid navy;
    }

    .conversation-msg-container {
        width: calc(100% - 575px) !important;
        min-width: 500px;
        padding-right: 10px;
        height: 100%;
    }

    .profile-fields-container > legend {
        width: auto;
    }

    .button-container {
        text-align: right;
    }

    .update-result-msg {
        padding-right: 10px;
        font-weight: bold;
    }

    .err-msg {
        color: red;
    }

    .normal-msg {
        color: darkgreen;
    }

    .newMessage-input-container {
        margin-bottom: 15px;
    }

    .conversation-fieldset {
        width: initial !important;
        padding-right: 20px;
        max-height: calc(100vh - 199px);       
    }

    .conversation-messages-list-container {
        height: calc(100vh - 550px); 
        overflow-y: auto;
    }

    dl {
        margin-block-start: 0;
        margin-block-end: 0;
    }

    dd {
        margin: 0
    }

    textarea {
        width: 100%;
        resize: none;
    }
</style>