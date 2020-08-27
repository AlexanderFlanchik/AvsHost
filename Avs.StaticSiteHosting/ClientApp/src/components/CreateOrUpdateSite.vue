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
                            <input type="text" class="site-name-editor" maxlength="30" v-model="siteName" />
                        </td>
                    </tr>
                    <tr>
                        <td class="vertical-tex-align">Description:</td>
                        <td>
                            <textarea v-model="description" class="site-name-editor" rows="3" />
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
                                    <input type="text" maxlength="30" v-model="rm.name" @input="checkNewResourceMappingName" />                                    
                                    <span v-if="rm.nameError" class="error-validation-message">This mapping already exists.</span> 
                                </td>
                            </tr>
                            <tr>
                                <td>Value:</td>
                                <td>
                                    <input type="text" maxlength="30" v-model="rm.value" @input="checkNewResourceMappingValue"/>
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
            </div>
            <div class="site-form-holder-right">
                <span class="form-title">Upload files</span>
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
            }
        }
    }
</script>
<style scoped>
    .form-title {
        font-style: italic;
    }
    .site-name-editor {
        width: 350px;
    }
    .site-form-holder {
        width: 100%;  
        background-color: azure;
        height: calc(100vh - 95px);
        overflow-y: auto;
    }

    .site-form-holder-left {
        width: 450px;
        float: left;
        padding-left: 5px;
        padding-top: 5px;
    }

    .site-form-holder-right {
        padding-top: 5px;
        padding-left: 10px;
        width: calc(100% - 450px);
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
</style>