<template>
    <div class="content-block-container">
        <div class="general-page-title">
            <span>{{title}}</span>
        </div>
        <div class="site-form-header">
            <div>
                <button class="btn btn-primary" @click="goToDashboard" id="dashboardBtn">Dashboard</button>
                <button class="btn btn-primary" :disabled="saveButtonDisabled" @click="createOrUpdateSite">{{ siteId ? 'Save' : 'Create'}}</button>  
            </div>
            <Loader :processing="this.processing"/>
            <div class="process-error-bar" v-if="processError && processError.length">
                <span class="validation-error">{{processError}}</span>
            </div>
        </div>
        <div class="site-form-holder">
            <div class="site-form-holder-left">
                <span class="form-title">Site info</span>
                <table class="site-form">
                    <tr>
                        <td>Site name:</td>
                        <td>
                            <div>
                                <b-form-input type="text"
                                              class="site-name-editor"
                                              maxlength="30"
                                              v-model="siteName"
                                              @input="validateSiteName"
                                              @blur="validation.siteName.touched=true;"
                                              v-bind:class="applyValidationErrorClass"></b-form-input>
                            </div>
                            <div class="validation-error" v-if="isSiteNameInvalid">
                                This site name already exists.
                            </div>
                            <div class="validation-error" v-if="isSiteNameReserved">
                                This site name is reserved. Please choose another one.
                            </div>
                            <div class="validation-error" v-if="!siteName.length && validation.siteName.touched">
                                The site name is required.
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td class="vertical-tex-align">Description:</td>
                        <td>
                            <b-form-textarea v-model="description" class="site-name-editor" rows="3"></b-form-textarea>
                        </td>
                    </tr>
                    <tr>
                        <td class="vertical-tex-align">Landing page:</td>
                        <td>
                            <b-form-input type="text" v-model="landingPage"></b-form-input>
                        </td>
                    </tr>
                    <tr>
                        <td>Is active:</td>
                        <td>
                            <b-form-checkbox v-model="isActive"></b-form-checkbox>
                        </td>
                    </tr>
                    <tr>
                        <td class="tags-label">Tags:</td>
                        <td>
                            <TagsSelectList :tagIds="tagIds" :onTagsChanged="(selectedTags) => tagsChanged(selectedTags)" />
                        </td>
                    </tr>
                </table>
                <ResourceMappingsSection :resourceMappings="resourceMappings"/>
            </div>
            <div class="site-form-holder-right">
                <UploadSiteContent :uploadSessionId="uploadSessionId" :uploaded="uploaded" :onNewSessionHanlder="(newSessionId)=>this.uploadSessionId = newSessionId"/>

                <div class="editor-buttons-container">
                    <button class="btn btn-primary" @click="newHtmlPage">New HTML Page</button>
                </div>
                <div class="uploaded-content-holder">
                    <span class="form-title">Site content</span>
                    <UploadedContentList :uploadSessionId="uploadSessionId" :uploaded="uploaded" :openPageEditor="openPageEditor" />  
                </div>
            </div>
        </div> 
    </div>
</template>
<script>
    import { ContentFile } from '../common/ContentFile';
    import { SiteContextManager } from '../services/SiteContextManager';
    import TagsSelectList from './Tags/TagsSelectList.vue';
    import Loader from './Loader.vue';
    import ResourceMappingsSection from './CreateOrUpdateSite/ResourceMappingsSection.vue';
    import UploadSiteContent from './CreateOrUpdateSite/UploadSiteContent.vue';
    import UploadedContentList from './CreateOrUpdateSite/UploadedContentList.vue';
    import { CreateOrUpdateSiteContextHolder } from './CreateOrUpdateSite/ContextHolder';
    import * as Q from 'q';

    const stateManager = new SiteContextManager();
    const contextHolder = new CreateOrUpdateSiteContextHolder();

    export default {
        data: function () {
            return {
                title: '',
                siteName: '',
                siteId: null,
                description: '',
                isActive: true,
                landingPage: '',
                processError: '',
                tagIds: [],
                resourceMappings: [],
                uploadSessionId: '',
                uploaded: [],
                processing: false,
                validation: {
                    siteName: {
                        touched: false,
                        valid: true,
                        inProcess: false
                    }                   
                }
            }
        },
        mounted: async function () {
            this.siteId = this.$route.params.siteId;
            this.title = this.siteId ? "Edit Site" : "Create New Site";

            const cachedSite = stateManager.get();
            const getSiteDataFromCache = () => {
                this.siteId = cachedSite.siteId;
                this.siteName = cachedSite.siteName;
                this.description = cachedSite.description;
                this.isActive = cachedSite.isActive;
                this.landingPage = cachedSite.landingPage;
                this.tagIds = cachedSite.tagIds;
                this.resourceMappings = cachedSite.resourceMappings;
                this.uploadSessionId = cachedSite.uploadSessionId;
                this.uploaded = cachedSite.uploadedFiles.map(
                    f => new ContentFile(
                            f.id,
                            f.name,
                            f.destinationPath,
                            f.isNew,
                            f.size,
                            f.isEditable,
                            f.isViewable,
                            f.uploadedAt,
                            f.updateDate
                    ));

                contextHolder.set(this.getFormContext());
            };
                       
            if (this.siteId) {
                // edit existing site, load site data
                if (cachedSite && cachedSite.siteId == this.siteId) {
                    getSiteDataFromCache();
                }
                else {
                    try {
                        this.processing = true;
                        let siteResponse = await this.$apiClient.getAsync(`api/sitedetails/${this.siteId}`);
                        let data = siteResponse.data;

                        this.siteName = data.siteName;
                        this.description = data.description;
                        this.isActive = data.isActive;
                        this.landingPage = data.landingPage;
                        console.log("check tags");
                        
                        this.tagIds = data.tagIds;
                        console.log(this.tagIds);
                        
                        let lst = [];
                        let uploaded = data.uploaded;
                        for (let u of uploaded) {
                            u.isNew = false;
                            let uploadedFile = new ContentFile(
                                u.id,
                                u.fileName,
                                u.destinationPath,
                                typeof u.isNew == 'undefined' ? false : u.isNew,
                                u.size,
                                u.isEditable,
                                u.isViewable,
                                u.uploadedAt,
                                u.updateDate
                            );
                            lst.push(uploadedFile);
                        }
                        this.uploaded = lst;

                        this.resourceMappings = [];
                        let mappings = data.resourceMappings;
                        if (mappings) {
                            let keys = Object.keys(mappings);
                            for (let key of keys) {
                                this.resourceMappings.push({ name: key, value: mappings[key] });
                            }
                        }

                        contextHolder.set(this.getFormContext());
                        await Q.delay(250);
                        this.processing = false;
                    }
                    catch (e) {
                        console.log(e);
                        this.processing = false;
                    }
                }
            } else {
                if (cachedSite && !cachedSite.siteId) {
                    getSiteDataFromCache();
                } else {
                    contextHolder.set(this.getFormContext());
                }
            }
        },

        methods: {          
            tagsChanged: function(selectedTags) {
                this.tagIds = selectedTags.map(t => t.id);
            },
            
            getFormContext: function() {
                let rm = new Map();
                for (let mapping of this.resourceMappings) {
                    rm.set(mapping.name, mapping.value);
                }

                return {
                    siteName: this.siteName,
                    description: this.description,
                    isActive: this.isActive,
                    landingPage: this.landingPage,
                    tagIds: this.tagIds,
                    resourceMappings: rm        
                };
            },

            validateSiteName: async function () {
                if (!this.siteName || !this.siteName.length) {
                    return;
                }

                if (this.siteName.toLowerCase() == '_error') { // its reserved
                    return;
                }

                this.validation.siteName.inProcess = true;
                let validationUrl = `api/SiteDetails/CheckSiteName?siteName=${this.siteName}`;
                if (this.siteId) {
                    validationUrl += `&siteId=${this.siteId}`;
                }

                let validationResult = await this.$apiClient.getAsync(validationUrl);
                this.validation.siteName.valid = validationResult.data;
                this.validation.siteName.inProcess = false;
            },

            goToDashboard: async function() {
                let isModified = contextHolder.isModified(this.getFormContext())
                    || this.uploaded.find(u => u.isNew);
                if (!isModified) {
                    this.$router.replace("/dashboard");
                    return;
                }

                if (confirm("You have made changes. Do you want to save them?")) {
                    if (!await this.createOrUpdateSite()) {
                        // Errors during save
                        return;
                    }
                }

                if (this.uploadSessionId) {
                    await this.$apiClient.postAsync('api/contentupload/cancelupload', null, { "upload-session-id" : this.uploadSessionId });
                }

                this.$router.replace("/dashboard");
            },

            createOrUpdateSite: async function () {
                this.processError = '';
                this.processing = true;

                const getResourceMappings = () => {
                    if (!this.resourceMappings || !this.resourceMappings.length) {
                        return null;
                    }

                    let result = {};
                    for (let i of this.resourceMappings) {
                        let key = i["name"];
                        let value = i["value"];
                        result[key] = value;
                    }

                    return result;
                };

                let siteDetailsModel = {
                    siteName: this.siteName,
                    uploadSessionId: this.uploadSessionId,
                    description: this.description,
                    isActive: this.isActive,
                    landingPage: this.landingPage,
                    tagIds: this.tagIds,
                    resourceMappings: getResourceMappings()
                };

                let isNew = !this.siteId;
                try {
                    if (isNew) {
                        let newSiteData = (await this.$apiClient.postAsync('api/sitedetails', siteDetailsModel))?.data;
                        this.siteId = newSiteData.id;
                        for (let up of this.uploaded) {
                            up.isNew = false;
                        }
                    } else {
                        await this.$apiClient.putAsync(`api/sitedetails/${this.siteId}`, siteDetailsModel);
                    }
                    
                    contextHolder.set(this.getFormContext());
                    await Q.delay(250);
                    this.processing = false;
                    if (isNew) {
                        this.$router.push({ path: '/sites/update/' + this.siteId });
                    }
                } catch {
                    let mode = this.siteId ? 'edit' : 'create';
                    let msg = `Unable to ${mode} your site due to server error. Please try again later.`;
                    this.processError = msg;
                    this.processing = false;
                    
                    return false;
                }

                return true;
            },
        
            getUploadSessionId: async function () {
                let sessionId = this.uploadSessionId;
                if (!sessionId) {
                    let sessionResponse = await this.$apiClient.getAsync('api/contentupload/session');
                    this.uploadSessionId = sessionResponse.headers['upload-session-id'];
                    sessionId = this.uploadSessionId;
                }
                return sessionId;
            },

            newHtmlPage: async function() {
                let sessionId = await this.getUploadSessionId();

                stateManager.save({
                    uploadSessionId: sessionId,
                    siteName: this.siteName,
                    siteId: this.siteId,
                    description: this.description,
                    isActive: this.isActive,
                    landingPage: this.landingPage,
                    resourceMappings: this.resourceMappings,
                    tagIds: this.tagIds,
                    uploadedFiles: this.uploaded
                });

                this.$router.push({ name: 'page-editor', params: { uploadSessionId: sessionId, siteId: this.siteId  } });
            },

            openPageEditor: async function (file) {
                let sessionId = await this.getUploadSessionId();

                stateManager.save({
                    uploadSessionId: sessionId,
                    siteName: this.siteName,
                    siteId: this.siteId,
                    description: this.description,
                    isActive: this.isActive,
                    landingPage: this.landingPage,
                    tagIds: this.tagIds,
                    resourceMappings: this.resourceMappings,
                    uploadedFiles: this.uploaded
                });

                this.$router.push({ 
                    name: 'page-editor', 
                    params:  { 
                        siteId: this.siteId, 
                        contentId: file.id,
                        contentName: file.name,
                        contentDestinationPath: file.destinationPath, 
                        uploadSessionId: this.uploadSessionId 
                    }});
            }
        },
        computed: {
            isSiteNameInvalid: function () {
                let siteNameValidation = this.validation.siteName;
                return !siteNameValidation.valid && siteNameValidation.touched;
            },

            isSiteNameReserved: function () {
                let siteNameValidation = this.validation.siteName;
                return this.siteName && this.siteName.toLowerCase() == '_error' && siteNameValidation.touched;
            },

            saveButtonDisabled: function () {
                let siteNameValidation = this.validation.siteName;
                return !this.siteName ||
                    this.siteName.toLowerCase() == '_error' ||
                    !this.siteName.length ||
                    (!siteNameValidation.valid && siteNameValidation.touched) ||
                    !this.uploaded.length;
            },

            applyValidationErrorClass: function () {
                let siteNameValidation = this.validation.siteName;
                let applied = !siteNameValidation.valid && siteNameValidation.touched;

                return {
                    'invalid-field': applied
                };
            }          
        },
        components: {
            Loader,
            UploadSiteContent,
            ResourceMappingsSection,
            UploadedContentList,
            TagsSelectList
        }
    }
</script>
<style scoped>
    .site-name-editor {
        width: 450px;
    }
    .site-form-holder {
        width: 100%;  
        background-color: azure;
        height: calc(100vh - 135px);
        overflow-y: auto;
    }
    .site-form-holder-left {
        width: 550px;
        float: left;
        padding-left: 5px;
        padding-top: 5px;
    }
    .site-form-holder-right {
        padding-top: 5px;
        padding-left: 25px;
        min-width: 300px;
        width: calc(100% - 550px);
        float: left;
    }
    td.vertical-tex-align {
        vertical-align: top;
    }
    table.site-form td {
        padding-top: 5px;
        padding-bottom: 5px;
    }
    .editor-buttons-container {
        clear: both;
        padding-top: 55px;
        padding-right: 5px;
    }
    .uploaded-content-holder {
        clear: both;
        padding-top: 55px;
        padding-right: 5px;
    }
    .site-form-header {
        display: flex;
        width: 100%;
        background-color: lavender;
        padding-left: 10px;
        padding-top: 5px;
        padding-bottom: 5px;
        height: 50px;
    }
    .process-error-bar {
        padding-top: 7px;
        padding-left: 5px;
    }
    .invalid-field {
        background-color:#ffe6e6;
        border-color: red;
    }
    .validation-error {
        color: red;
        font-weight: bold;
    }
    #dashboardBtn {
        margin-right: 2px;
    }
    .tags-label {
        vertical-align: top;
        padding-top: 16px !important;
    }
</style>