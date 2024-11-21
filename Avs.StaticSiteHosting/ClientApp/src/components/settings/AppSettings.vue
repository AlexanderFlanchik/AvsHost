<script setup lang="ts">
import { computed, inject, onMounted, reactive } from 'vue';
import { API_CLIENT } from '../../common/diKeys';
import UserInfo from '../layout/UserInfo.vue';
import NavigationMenu from '../layout/NavigationMenu.vue';

interface SettingsModel {
    cloudStorage: {
        enabled: boolean;
        bucketName: string;
        region: string;
        accessKey: string;
        secret: string;
    },
    errors: Array<any>;
    regions: Array<string>;
}

const model = reactive<SettingsModel>({
    cloudStorage: {
        enabled: false,
        bucketName: '',
        region: '',
        accessKey: '',
        secret: ''
    },
    errors: [],
    regions: []
});

const apiClient = inject(API_CLIENT)!;

const fieldIsEmpty = (field: string) => {
    let error = model.errors.find((f: { field: string }) => f.field == field) || [];
                            
    return error.length != 0;
};

const validateRequired = (field: string) => {
    //@ts-ignore
    if (!model.cloudStorage[field]) {
        model.errors.push({ field });
    } else {
        let e = model.errors.find((f: { field: string }) => f.field == field);
        if (e) {
            model.errors.splice(model.errors.indexOf(e), 1);
        }
    }
};

const save = async () => {
    model.errors = [];
    validateRequired('bucketName');
    validateRequired('accessKey');
    validateRequired('secret');

    if (model.errors.length) {
        return;
    }

    const data = {
        cloudStorage: {
            enabled: model.cloudStorage.enabled,
            bucketName: model.cloudStorage.bucketName,
            region: model.cloudStorage.region,
            accessKey: model.cloudStorage.accessKey,
            secret: model.cloudStorage.secret
        }
    };

    apiClient.postAsync('/api/appsettings', data)
        .then(() => console.log('Settings saved.'))
        .catch((e: Error) => console.log('Unable to save settings. Error: ' + e));
};

const load = async () => {
    apiClient.getAsync('api/appsettings')
        .then((response: any) => {
            const allSettings = response.data;
            const cloudStorage = allSettings.cloudStorage;
            model.regions = allSettings.cloudRegions;
            model.cloudStorage.enabled = cloudStorage.enabled;
            model.cloudStorage.bucketName = cloudStorage.bucketName;
            model.cloudStorage.region = cloudStorage.region;
            model.cloudStorage.accessKey = cloudStorage.accessKey;
            model.cloudStorage.secret = cloudStorage.secret;
        });
};

const bucketNameEmpty = computed(() => fieldIsEmpty('bucketName'));
const accessKeyEmpty = computed(() => fieldIsEmpty('accessKey'));
const secretKeyEmpty = computed(() => fieldIsEmpty('secret'));

onMounted(() => load());

</script>
<template>
    <div class="content-block-container">
        <div class="general-page-title">
            App Settings
        </div>
        <UserInfo />
        <NavigationMenu />
        <div class="app-settings-content">
            <div class="button-bar">
                <button class="btn btn-primary" @click="save">Save..</button>
            </div>
            <fieldset class="cloud-storage-fieldset">
                <legend>Cloud Storage</legend>
                <table>
                    <tr>
                        <td>Enabled:</td>
                        <td><input type="checkbox" v-model="model.cloudStorage.enabled" /></td>
                    </tr>
                    <tr>
                        <td>Bucket:</td>
                        <td>
                            <input type="text" v-model="model.cloudStorage.bucketName" class="field-w-380" /> &nbsp;
                            <span class="validation-error" v-if="bucketNameEmpty">Bucket name is required.</span>
                        </td>
                    </tr>
                    <tr>
                        <td>Region:</td>
                        <td>
                            <v-select class="field-w-280 region-select"
                                      ref="regionSelect" v-model="model.cloudStorage.region" label="text"
                                      :clearable="false" :filterable="false"
                                      :reduce="(region: any) => region.value"
                                      :options="model.regions">
                                <template slot="no-options">
                                    <div class="no-options-placeholder">Start enter a region name</div>
                                </template>

                                <template #option="option" slot="option" slot-scope="option">
                                    <div>{{option.text}}</div>
                                </template>

                                <template #selectedOption="option" slot="selected-option" slot-scope="option">
                                    <div class="option-selected">{{option.text}}</div>
                                </template>
                            </v-select>
                        </td>
                    </tr>
                    <tr>
                        <td>Access key:</td>
                        <td>
                            <input type="text" v-model="model.cloudStorage.accessKey" class="field-w-280" /> &nbsp;
                            <span class="validation-error" v-if="accessKeyEmpty">Access key is required.</span>
                        </td>
                    </tr>
                    <tr>
                        <td>Secret:</td>
                        <td>
                            <input type="text" v-model="model.cloudStorage.secret" class="field-w-420" /> &nbsp;
                            <span class="validation-error" v-if="secretKeyEmpty">Secret is required.</span>
                        </td>
                    </tr>
                </table>
            </fieldset>
        </div>
    </div>
</template>
<style scoped>
    .button-bar {
        padding-left: 5px;
        padding-top: 3px;
        padding-bottom: 3px;
        background-color: lavender;
    }
    .region-select {
        background-color: white;
    }
    .app-settings-content {
        background-color: azure;
        height: calc(100% - 155px);
        overflow-y: auto;
    }
    .field-w-380 {
        width: 350px;
    }
    .field-w-420 {
        width: 420px;
    }
    .field-w-280 {
        width: 280px;
    }
    .cloud-storage-fieldset {
        margin-left: 10px;
    }
</style>