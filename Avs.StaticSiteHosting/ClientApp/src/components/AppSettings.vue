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
                        <td><input type="checkbox" v-model="cloudStorage.enabled" /></td>
                    </tr>
                    <tr>
                        <td>Bucket:</td>
                        <td>
                            <input type="text" v-model="cloudStorage.bucketName" class="field-w-380" /> &nbsp;
                            <span class="validation-error" v-if="bucketNameEmpty">Bucket name is required.</span>
                        </td>
                    </tr>
                    <tr>
                        <td>Region:</td>
                        <td>
                            <v-select class="field-w-280 region-select"
                                      ref="regionSelect" v-model="cloudStorage.region" label="text"
                                      :clearable="false" :filterable="false"
                                      :reduce="region => region.value"
                                      :options="regions">
                                <template slot="no-options">
                                    <div class="no-options-placeholder">Start enter a region name</div>
                                </template>

                                <template slot="option" slot-scope="option">
                                    <div>{{option.text}}</div>
                                </template>

                                <template slot="selected-option" slot-scope="option">
                                    <div class="option-selected">{{option.text}}</div>
                                </template>
                            </v-select>
                        </td>
                    </tr>
                    <tr>
                        <td>Access key:</td>
                        <td>
                            <input type="text" v-model="cloudStorage.accessKey" class="field-w-280" /> &nbsp;
                            <span class="validation-error" v-if="accessKeyEmpty">Access key is required.</span>
                        </td>
                    </tr>
                    <tr>
                        <td>Secret:</td>
                        <td>
                            <input type="text" v-model="cloudStorage.secret" class="field-w-420" /> &nbsp;
                            <span class="validation-error" v-if="secretEmpty">Secret is required.</span>
                        </td>
                    </tr>
                </table>
            </fieldset>
        </div>
    </div>
</template>
<script>
    import UserInfo from '@/components/UserInfo.vue';
    import NavigationMenu from '@/components/NavigationMenu.vue';

    export default {
        data: function () {
            return {
                cloudStorage: {
                    enabled: false,
                    bucketName: '',
                    region: '',
                    accessKey: '',
                    secret: ''
                },
                errors: [],
                regions: []
            };
        },
        mounted: function () {
            this.load();
        },
        methods: {
            fieldIsEmpty: function(field) {
                let error = this.errors.find(f => f.field == field) || [];
                            
                return error.length != 0;
            },
            validateRequired: function(field) {
                if (!this.cloudStorage[field]) {
                    this.errors.push({ field });
                } else {
                    let e = this.errors.find(f => f.field == field);
                    if (e) {
                        this.errors.splice(this.errors.indexOf(e), 1);
                    }
                }
            },
            save: async function () {
                this.errors = [];
               
                this.validateRequired('bucketName');
                this.validateRequired('accessKey');
                this.validateRequired('secret');

                if (this.errors.length) {
                    return;
                }

                let data = {
                    cloudStorage: {
                        enabled: this.cloudStorage.enabled,
                        bucketName: this.cloudStorage.bucketName,
                        region: this.cloudStorage.region,
                        accessKey: this.cloudStorage.accessKey,
                        secret: this.cloudStorage.secret
                    }
                };
             
                this.$apiClient.postAsync('/api/appsettings', data)
                    .then(() => console.log('Settings saved.'))
                    .catch((e) => console.log('Unable to save settings. Error: ' + e));
            },
            load: async function () {
                this.$apiClient.getAsync('api/appsettings')
                    .then((response) => {
                        let allSettings = response.data;
                        let cloudStorage = allSettings.cloudStorage;
                        this.regions = allSettings.cloudRegions;
                        this.cloudStorage.enabled = cloudStorage.enabled;
                        this.cloudStorage.bucketName = cloudStorage.bucketName;
                        this.cloudStorage.region = cloudStorage.region;
                        this.cloudStorage.accessKey = cloudStorage.accessKey;
                        this.cloudStorage.secret = cloudStorage.secret;
                    });
            }
        },
        computed: {
            bucketNameEmpty: function() {
                return this.fieldIsEmpty('bucketName');
            },
            accessKeyEmpty: function() {
               return this.fieldIsEmpty('accessKey');
            },
            secretEmpty: function() {
                return this.fieldIsEmpty('secret');
            }
        },
        components: {
            UserInfo,
            NavigationMenu
        }
    }
</script>
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