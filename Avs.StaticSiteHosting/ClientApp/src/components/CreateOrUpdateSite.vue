<template>
    <div class="content-block-container">
        <div class="general-page-title">
            <span>{{title}}</span>
        </div>
        <div class="site-form-header">
            <button class="btn btn-primary" :disabled="saveButtonDisabled" @click="createOrUpdateSite">{{ siteId ? 'Save' : 'Create'}}</button>
            <button class="btn btn-primary" @click="cancel" id="cancelBtn">Cancel</button>
            <span class="validation-error" v-if="processError && processError.length">{{processError}}</span>
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
                </table>
                <b-modal ref="new-resource-mapping-dlg" hide-footer title="New Resource Mapping">
                    <div>
                        <table class="site-form">
                            <tr>
                                <td>Name:</td>
                                <td>
                                    <b-form-input type="text" maxlength="30" v-model="rm.name" @input="checkNewResourceMappingName"></b-form-input>
                                    <span v-if="rm.nameError" class="error-validation-message">This mapping already exists.</span> 
                                </td>
                            </tr>
                            <tr>
                                <td>Value:</td>
                                <td>
                                    <b-form-input type="text" maxlength="30" v-model="rm.value" @input="checkNewResourceMappingValue"></b-form-input>
                                    <span v-if="rm.valueError" class="error-validation-message">The value is required.</span>
                                </td>
                            </tr>
                        </table>
                    </div>
                    <button class="btn btn-primary" @click="addResourceMappingApply" :disabled="rm.nameError">Add</button>
                    <button class="btn btn-default" @click="newResourceMappingCancel">Cancel</button>
                </b-modal>
                <div class="resource-mapping-title-container">
                    <div class="resource-mapping-container-left">
                        <span class="form-title">Resource mappings</span>
                    </div>
                    <div class="resource-mapping-container-right">
                        <a href="#" @click="addResourceMapping">Add new...</a>
                    </div>
                </div>
                <table class="table resource-mapping-header">
                    <thead>
                        <tr>
                            <th>Name</th>
                            <th>Value</th>
                        </tr>
                    </thead>
                </table>
                <table class="table table-striped" v-if="resourceMappings.length > 0">
                    <tbody>
                        <tr v-for="rm in resourceMappings" :key="rm">
                            <td>{{rm.name}}</td>
                            <td>{{rm.value}} &nbsp;<a href="#" class="delete-link" title="Delete" @click="removeResourceMapping(rm)">X</a></td>
                        </tr>
                    </tbody>
                </table>
                <div class="no-mappings-message" v-if="resourceMappings.length === 0">
                    No resource mappings yet.
                </div>
            </div>
            <div class="site-form-holder-right">
                <div class="upload-form">
                    <span class="form-title">Upload files</span>
                    <div>
                        <b-form-file v-model="upload.contentFile"
                                     :state="Boolean(upload.contentFile)"
                                     placeholder="Choose a file or drop it here..."
                                     drop-placeholder="Drop file here..."></b-form-file>
                    </div>
                    <div class="mrg-tp-10x">
                        <b-form-checkbox v-model="upload.useDestinationPath">Use destination path</b-form-checkbox>
                    </div>
                    <div class="upload-bottom-bar">
                        <div class="upload-bottom-bar-left">
                            <b-form-input v-model="upload.destinationPath" :disabled="!upload.useDestinationPath"></b-form-input>
                        </div>
                        <div class="upload-bottom-bar-right">
                            <button class="btn btn-primary" @click="uploadContentFile" :disabled="!Boolean(upload.contentFile)">Upload..</button>
                        </div>
                    </div>                  
                </div>
                <div v-if="upload.errorMessage" class="upload-error-holder">
                    {{upload.errorMessage}}
                </div>
                <div class="editor-buttons-container">
                        <button class="btn btn-primary" @click="newHtmlPage">New HTML Page</button>
                </div>
                <div class="uploaded-content-holder">
                    <span class="form-title">Site content</span>           
                    <div v-if="uploaded.length > 0">
                        <table class="table table-striped">
                            <thead>
                                <tr>
                                    <th>Name</th>
                                    <th>Size, kB</th>
                                    <th>Uploaded Date</th>
                                    <th>Update Date</th>
                                    <th>&nbsp;</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr v-for="file in uploaded" :key="file">
                                    <td>
                                        <span class="badge badge-secondary" v-if="file.isNew">New</span>&nbsp;
                                        {{file.fullName()}}
                                    </td>
                                    <td>{{file.size}}</td>
                                    <td>{{file.uploadedAt}}</td>
                                    <td>{{file.updateDate}}</td>
                                    <td>
                                        <span v-if="!file.isNew"><a :href="downloadLink(file)">Download</a> | </span>
                                        <span v-if="file.isEditable"><a href="javascript:void(0)" @click="() => edit(file)">Edit Content</a> | </span>
                                        <span v-if="file.isViewable"><a href="javascript:void(0)" @click="() => view(file)">View</a> | </span>
                                        <span v-if="file.isEditable"><a href="javascript:void(0)" @click="() => openPageEditor(file)">Open in editor</a> | </span>
                                        <span><a href="javascript:void(0)" @click="() => deleteContentItem(file)">Remove</a></span>
                                    </td>
                                </tr>
                            </tbody>
                        </table> 
                    </div>    
                    <div class="no-content-message" v-if="uploaded.length === 0">
                        No content files found. Upload or create some content to continue.
                    </div>
                </div>
                <b-modal ref="view-content-dlg" hide-footer size="xl" :title="viewContent.fileName">
                    <div class="content-centered">
                        <img :src="viewContent.src" />
                    </div>
                    <div class="content-centered dlg-btn-container">
                        <button class="btn btn-primary" @click="closeViewContent">Close</button>
                    </div>
                </b-modal>
                <b-modal ref="edit-content-dlg" hide-footer size="xl" :title="editContent.fileName">
                    <div class="content-centered">
                       <b-form-textarea v-model="editContent.content" class="content-editor"></b-form-textarea>
                    </div>
                    <div class="dlg-btn-container dlg-btn-container-right">
                        <button class="btn btn-primary" @click="updateContent">OK</button>
                        <button class="btn btn-default" @click="closeEditContent">Cancel</button>
                    </div>
                </b-modal>
            </div>
        </div> 
    </div>
</template>
<script lang="ts">
    import * as moment from 'moment';
    import { formatDate } from '../common/DateFormatter';
    import { ContentFile } from '../common/ContentFile';
    import { SiteContextManager } from '../services/SiteContextManager';

    const stateManager = new SiteContextManager();

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
                resourceMappings: [],
                // upload form view model
                upload: {
                    contentFile: null,
                    useDestinationPath: false,
                    destinationPath: '',
                    uploadSessionId: '',
                    errorMessage: '',
                    clear: function () {
                        this.errorMessage = '';
                        this.contentFile = null;
                        this.useDestinationPath = false;
                        this.destinationPath = '';
                    }
                },
                uploaded: [],
                // popup add new resource mapping view model
                rm: {
                    name: '',
                    value: '',
                    nameError: false,
                    valueError: false,
                    clear : function () {
                        this.name = '';
                        this.value = '',
                        this.nameError = false;
                        this.valueError = false;
                    },
                    checkNewResourceMappingValue: function () {
                        let val = this.value;
                        if (!val) {
                            this.valueError = true;
                        } else {
                            this.valueError = false;
                        }
                    },
                },
                validation: {
                    siteName: {
                        touched: false,
                        valid: true,
                        inProcess: false
                    }                   
                },
                viewContent: {
                    fileName: '',
                    src: ''                   
                },
                editContent: {
                    fileName: '',
                    content: ''
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
                this.resourceMappings = cachedSite.resourceMappings;
                this.uploaded = cachedSite.uploadedFiles.map(
                    f => new ContentFile(
                            f.id,
                            f.name,
                            f.destinationPath,
                            f.isNew,
                            f.size,
                            f.isEditable,
                            f.isViewable,
                            formatDate(f.uploadedAt),
                            formatDate(f.updateDate)
                    ));
            };
                       
            if (this.siteId) {
                // edit existing site, load site data
                if (cachedSite && cachedSite.siteId == this.siteId) {
                    getSiteDataFromCache();
                }
                else {
                    try {
                        let siteResponse = await this.$apiClient.getAsync(`api/sitedetails/${this.siteId}`);
                        let data = siteResponse.data;

                        this.siteName = data.siteName;
                        this.description = data.description;
                        this.isActive = data.isActive;
                        this.landingPage = data.landingPage;

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
                                formatDate(u.uploadedAt),
                                formatDate(u.updateDate)
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
                    }
                    catch (e) {
                        console.log(e);
                    }
                }
            } else {
                if (cachedSite && !cachedSite.siteId) {
                    getSiteDataFromCache();
                }
            }
        },

        methods: {
            addResourceMapping: function () {
                this.rm.clear();
                this.$refs['new-resource-mapping-dlg'].show();
            },
            newResourceMappingCancel: function () {
                this.$refs['new-resource-mapping-dlg'].hide();
            },
            addResourceMappingApply: function () {
                let name = this.rm.name;
                let value = this.rm.value;

                this.rm.checkNewResourceMappingValue();
                if (this.rm.valueError) {
                    return;
                }

                this.resourceMappings.push({ name, value });
                this.$refs['new-resource-mapping-dlg'].hide();
            },

            checkNewResourceMappingName: function () {
                let name = this.rm.name;
                if (this.resourceMappings.find(m => m.name == name)) {
                    this.rm.nameError = true;
                } else {
                    this.rm.nameError = false;
                } 
            },            

            checkNewResourceMappingValue: function () {
                this.rm.checkNewResourceMappingValue();
            },

            removeResourceMapping: function (mapping) {
                if (confirm(`Are you sure to delete '${mapping.name}' mapping?`)) {
                    let ix = this.resourceMappings.indexOf(mapping);
                    this.resourceMappings.splice(ix, 1);
                }
            },

            uploadContentFile: async function () {
                let file = this.upload.contentFile;
                
                if (!this.upload.uploadSessionId) {
                    let sessionResponse = await this.$apiClient.getAsync('api/contentupload/session');
                    this.upload.uploadSessionId = sessionResponse.headers['upload-session-id'];
                }

                let formData = new FormData();               
                formData.append('contentFile', file);

                let uploadUrl = `api/contentupload?uploadSessionId=${this.upload.uploadSessionId}`;
                if (this.upload.useDestinationPath && this.upload.destinationPath) {
                    uploadUrl += `&destinationPath=${this.upload.destinationPath}`;
                }

                try {
                    await this.$apiClient.postAsync(uploadUrl, formData);

                    if (!this.uploaded.find(u => u.name == file.name && u.destinationPath == this.upload.destinationPath)) {
                        let cf = new ContentFile(
                                    null,
                                    file.name,
                                    this.upload.destinationPath,
                                    true,
                                    file.size,
                                    false,
                                    false,
                                    formatDate(new Date()),
                                    null
                                );
                            
                        this.uploaded.push(cf);
                    }

                    this.upload.clear();
                } catch {
                    let msg = `Unable to upload ${file.name}.`;
                    if (this.upload.destinationPath) {
                        msg += 'Please check destination path.';
                    }
                    this.upload.errorMessage = msg;
                }
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

            cancel: async function () {
                if (confirm('Are you sure to cancel? Any unsaved data will be lost.')) {
                    if (this.upload.uploadSessionId) {
                        await this.$apiClient.postAsync('api/contentupload/cancelupload', null, { "upload-session-id" : this.upload.uploadSessionId });
                    }

                    this.$router.replace('/');
                }
            },
            createOrUpdateSite: async function () {
                this.processError = '';

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
                    uploadSessionId: this.upload.uploadSessionId,
                    description: this.description,
                    isActive: this.isActive,
                    landingPage: this.landingPage,
                    resourceMappings: getResourceMappings()
                };

                try {
                    if (!this.siteId) {
                        await this.$apiClient.postAsync('api/sitedetails', siteDetailsModel);
                    } else {
                        await this.$apiClient.putAsync(`api/sitedetails/${this.siteId}`, siteDetailsModel);
                    }
                    this.$router.replace('/');
                } catch {
                    let mode = this.siteId ? 'edit' : 'create';
                    let msg = `Unable to ${mode} your site due to server error. Please try again later.`;
                    this.processError = msg;
                }
            },
            downloadLink: function (file) {
                if (!file || !file.id) {
                    return '#';
                }

                let url = `api/sitedetails/content-get?contentItemId=${file.id}&__accessToken=${this.$authService.getToken()}`;                
                return url;
            },
            view: function (file) {               
                this.viewContent.src = `api/sitedetails/content-get?contentItemId=${file.id}&maxWidth=600&__accessToken=${this.$authService.getToken()}`;
                this.viewContent.fileName = file.name;

                this.$refs["view-content-dlg"].show();
            },
            edit: async function (file) {
                let fileResponse = await this.$apiClient.getAsync(`api/sitedetails/content-get?contentItemId=${file.id}&__accessToken=${this.$authService.getToken()}`);
                this.editContent.content = fileResponse.data;
                this.editContent.fileName = file.name;
                this.editContent.id = file.id;
                
                this.$refs["edit-content-dlg"].show();
            },
            updateContent: async function () {
                let updateResponse = await this.$apiClient.putAsync(`api/sitedetails/content-edit/${this.editContent.id}`, { content: this.editContent.content });
                console.log(updateResponse);
                if (updateResponse.status == 200) {
                    let updatedItem = this.uploaded.find(i => i.id == this.editContent.id);
                    if (updatedItem) {
                        updatedItem.updateDate = moment(new Date()).format('MM/DD/YYYY hh:mm:ss A');
                    }
                }
                this.$refs["edit-content-dlg"].hide();
            },
            closeViewContent: function () {
                this.$refs["view-content-dlg"].hide();
            },
            closeEditContent: function () {
                this.$refs["edit-content-dlg"].hide();
            },
            deleteContentItem: async function (file) {
                if (!confirm(`Are you sure to delete file ${file.name}?`)) {
                    return;
                }

                let deleteUrl = `api/sitedetails/content-delete`;
                if (file.id) {
                    deleteUrl += `?contentItemId=${file.id}`;
                } else {
                    deleteUrl += `?contentItemName=${file.fullName()}&uploadSessionId=${this.upload.uploadSessionId}`;
                }

                let response = await this.$apiClient.deleteAsync(deleteUrl);
                console.log(response);

                try {
                    if (response.status == 200) {
                        let itemDeleted = this.uploaded.find(i => i.name == file.name);
                        let ix = this.uploaded.indexOf(itemDeleted);
                        this.uploaded.splice(ix, 1);
                    }
                } catch {
                    alert(`Unable to delete ${file.name}. Server error or the file does not exist.`);
                }
            },
            getUploadSessionId: async function () {
                let sessionId = this.upload.uploadSessionId;
                if (!sessionId) {
                    let sessionResponse = await this.$apiClient.getAsync('api/contentupload/session');
                    this.upload.uploadSessionId = sessionResponse.headers['upload-session-id'];
                    sessionId = this.upload.uploadSessionId;
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
                        uploadSessionId: this.upload.uploadSessionId 
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
        }
    }
</script>
<style scoped>
    .form-title {
        font-style: italic;
    }
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

    table.resource-mapping-header {
        width: 100%;
    }

    .resource-mapping-title-container {
        width: 100%;
    }

    .resource-mapping-container-left {
        width: 50%;
        float: left;
    }

    .resource-mapping-container-right {
        width: 50%;
        float: left;
        text-align: right;
    }

    .delete-link {
        color: red;
        font-weight: bold;
        text-decoration: none;
    }

    .error-validation-message {
        color: red;
        font-weight: bold;
    }

    .no-mappings-message {
        text-align: center;
        padding-top: 10px;
        color: navy;
        font-weight: bold;
    }

    .upload-form {
        padding-top: 2px;
        max-width: 550px;
    }

    .mrg-tp-10x {
        margin-top: 10px;
    }

    .upload-bottom-bar {
        padding-top: 10px;
        width: 100%;
    }

    .upload-bottom-bar-left {
        width: calc(100% - 90px);
        min-width: 150px;
        float: left;
    }

    .upload-bottom-bar-right {
        width: 90px;
        float: left;
        text-align: right;
    }

    .upload-error-holder {
        padding-top: 3px;
        clear: both;
        color: red;
        font-weight: bold;
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
        width: 100%;
        background-color: lavender;
        padding-left: 10px;
        padding-top: 5px;
        padding-bottom: 5px;
        height: 50px;
    }

    .no-content-message {
        background-color: white;
        height: 150px;
        max-width: 550px;
        padding-top: 60px;
        font-weight: bold;
        text-align: center;
    }

    .invalid-field {
        background-color:#ffe6e6;
        border-color: red;
    }

    .validation-error {
        color: red;
        font-weight: bold;
    }

    .content-centered {
        text-align: center;
    }

    .dlg-btn-container {
        padding-top: 5px;
        background-color: azure;
    }

    .content-editor {
        min-height: 570px;
    }

    .dlg-btn-container-right {
        text-align: right;
    }

    #cancelBtn {
        margin-left: 2px;
    }
</style>