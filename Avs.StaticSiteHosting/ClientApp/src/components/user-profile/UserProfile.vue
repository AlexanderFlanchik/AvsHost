<script setup lang="ts">
import { inject, onMounted, reactive } from 'vue';
import { useRoute } from 'vue-router';
import { API_CLIENT } from '../../common/diKeys';
import UserInfo from '../layout/UserInfo.vue';
import moment from 'moment';

interface UserProfileModel {
    name: string;
    email: string;
    status: string;
    statuses: Array<string>;
    joinDate: string | null;
    lastLocked: string | null;
    comment: string;
}

const model = reactive<UserProfileModel>({
    name: '',
    email: '',
    status: '',
    statuses: [],
    joinDate: null,
    lastLocked: null,
    comment: ''
});

const route = useRoute();
const apiClient = inject(API_CLIENT)!;

const loadProfile = () => {
    const userId = route.params["userId"] as string;
    apiClient.getAsync(`api/profile/user/${userId}`)
        .then((response: any) => {
            const userInfo = response.data;
            model.name = userInfo.name;
            model.email = userInfo.email;
            model.comment = userInfo.comment;
            model.status = model.statuses[userInfo.status];
            model.joinDate = userInfo.dateJoined ? moment(userInfo.dateJoined).format('MM/DD/YYYY') : ' - ';
            model.lastLocked = userInfo.lastLocked ? moment(userInfo.lastLocked).format('MM/DD/YYYY') : ' - ';
        });
};

const updateProfile = () => {
    const userId = route.params["userId"] as string;
    const updateRequest = {
        status: model.statuses.indexOf(model.status),
        comment: model.comment
    };

    apiClient.putAsync(`api/profile/UpdateUserProfile/${userId}`, updateRequest)
        .then(() => loadProfile())
        .catch((e: Error) => console.log(e));
};

onMounted(() => loadProfile());
</script>
<template>
    <div class="content-block-container">
        <div class="general-page-title">
            <img src="./../../../public/dashboard.png" />
            <span>User Profile</span>
        </div>
        <UserInfo />
        <div class="profile-form-container">
            <div class="general-info">
                <div class="general-info-left">
                    <dl class="info-list">
                        <dt>Name:</dt>
                        <dd><span>{{model.name}}</span></dd>
                        <dt>Email:</dt>
                        <dd><span>{{model.email}}</span></dd>
                        <dt>Status:</dt>
                        <dd>
                            <select v-model="model.status" style="width: 200px;" :options="model.statuses" size="sm"></select>
                        </dd>
                    </dl>
                </div>
                <div class="general-info-right">
                    <dl class="info-list">
                        <dt>Date of join:</dt>
                        <dd><span>{{model.joinDate}}</span></dd>
                        <dt>Last locked:</dt>
                        <dd><span>{{model.lastLocked ? model.lastLocked : '-'}}</span></dd>
                    </dl>
                </div>
            </div>
            <div class="comment-container">
                <span>Comment:</span> <br/>
                <textarea v-model="model.comment" class="comment-text-area" rows="5"></textarea>
                <div class="button-container">
                    <button class="btn btn-primary" @click="updateProfile">Save..</button>
                </div>
            </div>
        </div>
    </div>
</template>
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
    
    .comment-text-area {
        width: 600px;
    }

    .button-container{
        padding-top: 5px;
        text-align: right;
        max-width: 600px;
    }
</style>