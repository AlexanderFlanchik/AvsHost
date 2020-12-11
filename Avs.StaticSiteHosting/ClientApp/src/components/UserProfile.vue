<template>
    <div class="content-block-container">
        <div class="general-page-title">
            <!--<img src="../../../ClientApp/public/dashboard.png" /> &nbsp;-->
            <span>User Profile</span>
        </div>
        <UserInfo />
        <div class="profile-form-container">
            <div class="general-info">
                <div class="general-info-left">
                    <dl class="info-list">
                        <dt>Name:</dt>
                        <dd><span>{{name}}</span></dd>
                        <dt>Email:</dt>
                        <dd><span>{{email}}</span></dd>
                        <dt>Status:</dt>
                        <dd>
                            <b-form-select v-model="status" style="width: 200px;" :options="statuses" size="sm"></b-form-select>
                        </dd>
                    </dl>
                </div>
                <div class="general-info-right">
                    <dl class="info-list">
                        <dt>Date of join:</dt>
                        <dd><span>{{joinDate}}</span></dd>
                        <dt>Last locked:</dt>
                        <dd><span>{{lastLocked ? lastLocked : '-'}}</span></dd>
                    </dl>
                </div>
            </div>
            <div class="comment-container">
                <span>Comment:</span> <br/>
                <b-form-textarea v-model="comment" class="comment-text-area" rows="5"></b-form-textarea>
                <div class="button-container">
                    <button class="btn btn-primary" @click="updateProfile">Save..</button>
                </div>
            </div>
        </div>
    </div>
</template>
<script>
    import UserInfo from '@/components/UserInfo.vue';
    const moment = require('moment');

    export default {
        data: function () {
            return {
                name: '',
                email: '',
                status: '',
                statuses: ['Active', 'Locked'],
                joinDate: null,
                lastLocked: null,
                comment: ''
            };
        },
        mounted: function () {
            this.loadProfile();
        },
        methods: {
            loadProfile: function () {
                let userId = this.$route.params.userId;
                this.$apiClient.getAsync(`api/profile/user/${userId}`)
                    .then((response) => {
                        let userInfo = response.data;
                        this.name = userInfo.name;
                        this.email = userInfo.email;
                        this.comment = userInfo.comment;
                        this.status = this.statuses[userInfo.status];
                        this.joinDate = userInfo.dateJoined ? moment(userInfo.dateJoined).format('MM/DD/YYYY') : ' - ';
                        this.lastLocked = userInfo.lastLocked ? moment(userInfo.lastLocked).format('MM/DD/YYYY') : ' - ';
                    }).catch(() => this.$router.replace('/'));
            },
            updateProfile: function () {
                let userId = this.$route.params.userId;
                let updateRequest = {
                    status: this.statuses.indexOf(this.status),
                    comment: this.comment
                };

                this.$apiClient.putAsync(`api/profile/UpdateUserProfile/${userId}`, updateRequest)
                    .then(() => this.loadProfile())
                    .catch((e) => console.log(e));
            }
        },
        components: {
            UserInfo
        }
    }
</script>
<style scoped>
    .profile-form-container {
        background-color: azure;
        height: calc(100% - 98px);
        padding: 5px;
    }

    .general-info {
        padding-left: 5px;
    }

    .general-info-left {
        float: left;
        width: 400px;
    }

    .general-info-right {
        float: left;
        width: 550px;
        padding-left: 25px;
    }
   
    .info-list > dt {
        padding-right: 25px;
    }
    .comment-container {
        clear: both;
        padding-left: 5px;
    }
    .info-list > dd {
    }

    .comment-text-area {
        width: 600px;
    }

    .button-container{
        padding-top: 5px;
        text-align: right;
        max-width: 600px;
    }
</style>