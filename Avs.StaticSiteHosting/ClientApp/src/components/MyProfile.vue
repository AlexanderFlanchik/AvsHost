<template>
    <div class="content-block-container">
        <div class="general-page-title">
            <img src="../../public/profile.png" /> &nbsp;
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
                            <b-form-input type="text" v-model="userName" @blur="validation.userName.touched=true;" @input="validateUserName"></b-form-input>
                            <span class="validation-error" v-if="userName && validation.userName.touched && !validation.userName.success">This name is already in use.</span>
                            <span class="validation-error" v-if="validation.userName.touched && !userName">The name is required.</span>
                        </dd>

                        <dt>
                            Email:
                        </dt>
                        <dd>
                            <b-form-input type="text" v-model="userEmail" @blur="validation.userEmail.touched=true;" @input="validateUserEmail"></b-form-input>
                            <span class="validation-error" v-if="userEmail && validation.userEmail.touched && !validation.userEmail.success">This e-mail is already in use.</span>
                            <span class="validation-error" v-if="validation.userEmail.touched && !userEmail">The e-mail is required.</span>
                        </dd>
                    </dl>
                    <div class="button-container">
                        <span class="update-result-msg err-msg" v-if="updateResultShown">{{updateResult}}</span>
                        <button class="btn btn-primary" :disabled="isUpdateDisabled" @click="update">Update</button>
                    </div>
                </fieldset>

                <fieldset class="profile-fields-container">
                    <legend>Password Management</legend>
                    <dl>
                        <dt>Current password:</dt>
                        <dd>
                            <b-form-input type="password" v-model="userPassword" @blur="validation.userPassword.touched=true;"></b-form-input>
                            <span class="validation-error" v-if="validation.userPassword.touched && !userPassword">Current password is required.</span>
                        </dd>
                        <dt>New password:</dt>
                        <dd>
                            <b-form-input type="password" v-model="newPassword" @blur="validation.newPassword.touched=true;"></b-form-input>
                            <span class="validation-error" v-if="validation.newPassword.touched && !newPassword">New password is required.</span>
                        </dd>
                        <dt>New password confirm:</dt>
                        <dd>
                            <b-form-input type="password" v-model="confirmNewPassword" @blur="validation.confirmNewPassword.touched=true;"></b-form-input>
                            <span class="validation-error" v-if="validation.confirmNewPassword.touched && !confirmNewPassword">Confirm new password is required.</span>
                            <span class="validation-error"
                                  v-if="newPassword && confirmNewPassword && validation.confirmNewPassword.touched && confirmNewPassword != newPassword">New and confirmed passwords must be the same.</span>
                        </dd>
                    </dl>
                    <div class="button-container">
                        <span class="update-result-msg" v-if="changePasswordResultShown" v-bind:class="{'err-msg': changePasswordFailed, 'normal-msg': !changePasswordFailed }">{{changePasswordResult}}</span>
                        <button class="btn btn-primary" :disabled="isChangePasswordDisabled" @click="changePassword">Change Password..</button>
                    </div>
                </fieldset>
            </div>
         
            <div class="left with-left-margin-20 conversation-msg-container" v-if="!isAdmin">
                <fieldset class="profile-fields-container conversation-fieldset">
                    <legend>Communication with Administrator</legend>
                    <dl>
                        <dt>Type a message here:</dt>
                        <dd>
                            <div class="newMessage-input-container">
                                <b-form-textarea v-model="newMessage" rows="5"></b-form-textarea>
                            </div>

                            <div class="button-container">
                                <button class="btn btn-primary" :disabled="!newMessage" @click="sendMessage">Send..</button>
                            </div>                          
                        </dd>
                    </dl>
                    <dl>
                        <dt>Messages history:</dt>
                        <dd>                            
                            <ConversationMessagesWrapper ref="conversationMessagesList" :height="'calc(100vh - 555px)'"  />
                        </dd>
                    </dl>
                </fieldset>
            </div>
        </div>
    </div>
</template>
<script>
    import UserInfo from '@/components/UserInfo.vue';
    import NavigationMenu from '@/components/NavigationMenu.vue';
    import ConversationMessagesWrapper from '@/components/ConversationMessagesWrapper.vue';
    
    export default {
        data: function () {
            return {
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
                },

                showUpdateMessage: function (msg) {
                    this.updateResult = msg;
                    this.updateResultShown = true;
                    setTimeout(() => {
                        this.updateResultShown = false;
                        this.updateResult = '';
                    }, 1000);
                },

                showChangePasswordMessage: function (msg, isError) {
                    this.changePasswordResult = msg;
                    this.changePasswordResultShown = true;
                    this.changePasswordFailed = isError;

                    setTimeout(() => {
                        this.changePasswordResult = '';
                        this.changePasswordResultShown = false;
                        this.changePasswordFailed = false;
                    }, 1000);
                }
            };
        },

        mounted: function () {
            let userInfo = this.$authService.getUserInfo();
            if (userInfo) {
                this.userName = userInfo.name;
                this.userEmail = userInfo.email;
                this.isAdmin = userInfo.isAdmin;
            }
            this.originalUserInfo = Object.freeze(userInfo);

            this.$apiClient.getAsync('api/conversation').then((response) => {
                let conversation = response.data.conversation;
                if (conversation) {
                    this.conversationId = conversation.id;
                    this.$refs.conversationMessagesList.dispatch('conversationReady', this.conversationId);
                }
            });
        },

        methods: {            
            validateUserName: async function () {
                if (this.userName == this.originalUserInfo.name) {
                    return;
                }

                let userNameValidation = this.validation.userName;

                try {
                    userNameValidation.inProcess = true;
                    let validationResult = await this.$authService.validateUserData(this.userName, null);
                    userNameValidation.success = validationResult;
                } finally {
                    userNameValidation.inProcess = false;
                }
            },

            validateUserEmail: async function () {
                if (this.userEmail == this.originalUserInfo.email) {
                    return;
                }

                let userEmailValidation = this.validation.userEmail;

                try {
                    userEmailValidation.inProcess = true;
                    let validationResult = await this.$authService.validateUserData(null, this.userEmail);
                    userEmailValidation.success = validationResult;
                } finally {
                    userEmailValidation.inProcess = false;
                }
            },

            update: function () {
                let updateData = {
                    email: this.userEmail,
                    userName: this.userName
                };

                this.$apiClient.postAsync('api/profile/update', updateData).then(() => {
                    this.$authService.signOut();
                    this.$router.replace('/');
                }).catch((e) => this.showUpdateMessage(e.response && e.response.data));
            },

            changePassword: function () {
                let pwdData = {
                    password: this.userPassword,
                    newPassword: this.newPassword
                };

                this.$apiClient.postAsync('api/profile/changepassword', pwdData).then(() => {
                    this.showChangePasswordMessage("Password changed.", false);
                }).catch((e) => this.showChangePasswordMessage(e.response && e.response.data, true));
            },

            sendMessage: function () {
                const postMessage = () => {
                    const data = {
                        content: this.newMessage,
                        conversationId: this.conversationId                        
                    };
                    console.log(data);
                    this.$apiClient.postAsync('api/conversationmessages', data).then((newRowResponse) => {
                        let content = newRowResponse.data.content;
                        let dateAdded = newRowResponse.data.dateAdded;

                        this.$refs.conversationMessagesList.dispatch('addNewRow', { content, dateAdded });
                        this.newMessage = '';
                    });
                };

                if (!this.conversationId) {
                    console.log('No conversation id, creating...');
                    this.$apiClient.postAsync('api/conversation').then((response) => {
                        console.log(response);
                        this.conversationId = response.data.id;
                        console.log('conversation id ' + this.conversationId);
                        postMessage();
                    });
                } else {
                    postMessage();
                }                
            }           
        },

        computed: {
            isUpdateDisabled: function () {
                if (this.originalUserInfo && this.originalUserInfo.name == this.userName && this.originalUserInfo.email == this.userEmail) {
                    return true;
                }

                let userNameValidation = this.validation.userName;
                let userEmailValidation = this.validation.userEmail;
                
                let userNameValidationResult = this.userName
                    && this.userName.length
                    && !userNameValidation.inProcess
                    && userNameValidation.success;

                let userEmailValidationResult = this.userEmail
                    && this.userEmail.length
                    && !userEmailValidation.inProcess
                    && userEmailValidation.success;

                return !userNameValidationResult || !userEmailValidationResult;
            },

            isChangePasswordDisabled: function () {
                if (this.newPassword == this.userPassword) {
                    return true;
                }

                let passwordValidationResult = this.newPassword
                    && this.newPassword.length
                    && this.newPassword == this.confirmNewPassword;

                return !passwordValidationResult;
            }
        },

        components: {
            UserInfo,
            NavigationMenu,
            ConversationMessagesWrapper
        }
    }
</script>
<style scoped>
    .profile-content {
        background-color: azure;
        height: calc(100% - 155px);
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
        padding: 15px;
        border: 1px solid navy;
    }

    .conversation-msg-container {
        width: calc(100% - 520px) !important;
        min-width: 500px;
        padding-right: 10px;
        height: 100%;
        padding-bottom: 10px;
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
</style>