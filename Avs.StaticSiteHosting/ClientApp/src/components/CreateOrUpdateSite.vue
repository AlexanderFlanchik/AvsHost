<template>
    <div class="content-block-container">
        <div class="general-page-title">
            <span>{{title}}</span>
        </div>
        <div class="site-form-holder">
            <div class="site-form-holder-left">
                <span class="form-title">Site info</span>
                <table class="site-form">
                    <tr>
                        <td>Site name:</td>
                        <td>
                            <b-form-input type="text" class="site-name-editor" maxlength="30" v-model="siteName"></b-form-input>
                        </td>
                    </tr>
                    <tr>
                        <td class="vertical-tex-align">Description:</td>
                        <td>
                            <b-form-textarea v-model="description" class="site-name-editor" rows="3"></b-form-textarea>
                        </td>
                    </tr>
                    <tr>
                        <td>Is active:</td>
                        <td>
                            <input type="checkbox" v-model="isActive" />
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
                            <button class="btn btn-primary" @click="uploadContentFile">Upload..</button>
                        </div>
                    </div>                  
                </div>
                <div class="uploaded-content-holder">
                    <ul v-if="uploaded.length > 0">
                        <li v-for="file in uploaded" :key="file">
                            {{file.name}}
                        </li>
                    </ul>
                </div>
            </div>
        </div> 
    </div>
</template>
<script>
    export default {
        data: function () {
            return {
                title: '',
                siteName: '',
                description: '',
                isActive: true,
                resourceMappings: [
                    { name: 'about-us', value: 'about.html' },
                    { name: 'dashboard', value: 'public/dashboard.html' },
                    { name: 'help', value: 'help.html' },
                    { name: 'privacy-policy', value: 'common/policy.html'}
                ],
                // upload form view model
                upload: {
                    contentFile: null,
                    useDestinationPath: false,
                    destinationPath: '',
                    clear: function () {
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
                }
            }
        },
        mounted: function () {
            this.title = this.$route.query.siteId ? "Edit Site" : "Create New Site";
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
                console.log(file);

                this.uploaded.push(file);
                this.upload.clear();
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
        height: calc(100vh - 95px);
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

    .uploaded-content-holder {
        clear: both;
        padding-top: 10px;
    }
</style>