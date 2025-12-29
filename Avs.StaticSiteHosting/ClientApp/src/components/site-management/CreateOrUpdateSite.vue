<script setup lang="ts">
import { computed, inject, onMounted, reactive, Ref, watch } from 'vue';
import { ContentFile } from '../../common/ContentFile';
import { useRoute, useRouter } from 'vue-router';
import { ResourceMapping, SiteContextManager } from '../../services/SiteContextManager';
import { CreateOrUpdateSiteContextHolder } from './ContextHolder';
import { API_CLIENT } from '../../common/diKeys';
import { delay } from './../../common/DelayAync';
//@ts-ignore
import { AxiosRequestHeaders } from 'axios';
import Loader from '../Loader.vue';
import TagsSelectList from './../tags/TagsSelectList.vue';
import ResourceMappingsSection from './ResourceMappingsSection.vue';
import UploadedContentList from './UploadedContentList.vue';
import { PageContext, PageContextProvider } from '../page-editor/PageContext';
import { NewCreatedContentHolder } from '../../services/NewCreatedContentHolder';
import DatabaseNameInput from './DatabaseNameInput.vue';
import { CustomRouteHandler } from './CustomRouteHandler';
import CustomerHandlerList from './CustomerHandlerList.vue';

interface CreateOrUpdateSiteModel {
    title: string;
    siteName: string;
    siteId: string | null | undefined;
    description: string;
    isActive: boolean;
    landingPage: string;
    databaseName: string | null;
    processError: string;
    tagIds: Array<string>;
    resourceMappings: Array<ResourceMapping>
    uploadSessionId: string;
    uploaded: Array<ContentFile>;
    customRouteHandlers: Array<CustomRouteHandler>;
    processing: boolean;
    validation: {
        siteName: {
            touched: boolean,
            valid: boolean;
            inProcess: boolean;
        }
    }
}

const model = reactive<CreateOrUpdateSiteModel>({
    title: '',
    siteName: '',
    siteId: null,
    description: '',
    isActive: true,
    landingPage: '',
    databaseName: null,
    processError: '',
    tagIds: [],
    resourceMappings: [],
    uploadSessionId: '',
    uploaded: [],
    customRouteHandlers: [],
    processing: false,
    validation: {
        siteName: {
            touched: false,
            valid: true,
            inProcess: false
        }
    }
});

const route = useRoute();
const router = useRouter();
const stateManager = new SiteContextManager();
const contextHolder = new CreateOrUpdateSiteContextHolder();

const apiClient = inject(API_CLIENT)!;
const newContentHolder = new NewCreatedContentHolder();

const getFormContext = () => {
    const rm = new Map();
    for (let mapping of model.resourceMappings) {
        rm.set(mapping.name, mapping.value);
    }

    return {
        siteName: model.siteName,
        description: model.description,
        isActive: model.isActive,
        landingPage: model.landingPage,
        tagIds: model.tagIds,
        resourceMappings: rm        
    };
};

const mapUploaded = (uploaded: Array<any>) : Array<ContentFile> => {
    const lst = [];
    for (let u of uploaded) {
        let uploadedFile = new ContentFile(
            u.id,
            u.fileName,
            u.destinationPath,
            typeof u.isNew == 'undefined' ? false : u.isNew,
            u.size,
            u.isEditable,
            u.isViewable,
            u.uploadedAt,
            u.updateDate,
            u.cacheDuration
        );
                    
        lst.push(uploadedFile);
    }

    return lst;
};

watch(() => model.siteId, (newSiteId) => {
    model.title = model.siteId ? "Edit Site" : "Create New Site";
});

onMounted(async () => {
    model.siteId = route.params["siteId"] as string;
   
    const cachedSite = stateManager.get()!;
    const getSiteDataFromCache = () => {
        model.siteId = cachedSite.siteId;
        model.siteName = cachedSite.siteName;
        model.description = cachedSite.description;
        model.isActive = cachedSite.isActive;
        model.landingPage = cachedSite.landingPage;
        model.databaseName = cachedSite.databaseName;
        model.tagIds = cachedSite.tagIds;
        model.resourceMappings = cachedSite.resourceMappings;
        model.uploadSessionId = cachedSite.uploadSessionId;
        model.customRouteHandlers = cachedSite.customRouteHandlers;
        model.uploaded = cachedSite.uploadedFiles.map(
            f => new ContentFile(
                f.id,
                f.name,
                f.destinationPath,
                f.isNew,
                f.size,
                f.isEditable,
                f.isViewable,
                f.uploadedAt,
                f.updateDate,
                f.cacheDuration
        ));

        contextHolder.set(getFormContext());
    };

    if (model.siteId) {
        // edit existing site, load site data
        if (cachedSite && cachedSite.siteId == model.siteId) {
            getSiteDataFromCache();
        } else {
            try {
                model.processing = true;
                const siteResponse = await apiClient.getAsync(`api/sitedetails/${model.siteId}`) as any;
                const data = siteResponse.data;

                model.siteName = data.siteName;
                model.description = data.description;
                model.isActive = data.isActive;
                model.landingPage = data.landingPage;
                model.databaseName = data.databaseName;
                model.tagIds = data.tagIds;
                model.uploaded = mapUploaded(data.uploaded);
                model.customRouteHandlers = data.customRouteHandlers;
                model.resourceMappings = [];

                const mappings = data.resourceMappings;
                if (mappings) {
                    let keys = Object.keys(mappings);
                    for (let key of keys) {
                        model.resourceMappings.push({ name: key, value: mappings[key] });
                    }
                }

                contextHolder.set(getFormContext());
                await delay(250);

                model.processing = false;
            } catch (e) {
                console.log(e);
                model.processing = false;
            }
        }
    } else {
        if (cachedSite && !cachedSite.siteId) {
            getSiteDataFromCache();
        } else {
            contextHolder.set(getFormContext());
        }
    }
});

const tagsChanged = (selectedTags: Ref<Array<{ id: string }>>) => {
    model.tagIds = selectedTags.value.map(t => t.id);
};

const validateSiteName = async () => {
    if (!model.siteName || !model.siteName.length) {
        return;
    }

    if (model.siteName.toLowerCase() == '_error') { // its reserved
        return;
    }

    model.validation.siteName.inProcess = true;
    let validationUrl = `api/SiteDetails/CheckSiteName?siteName=${model.siteName}`;
    if (model.siteId) {
        validationUrl += `&siteId=${model.siteId}`;
    }

    const validationResult = await apiClient.getAsync(validationUrl) as any;
    model.validation.siteName.valid = validationResult.data;
    model.validation.siteName.inProcess = false;
};

const createOrUpdateSite = async () => {
    model.processError = '';
    model.processing = true;

    const getResourceMappings = () => {
        if (!model.resourceMappings || !model.resourceMappings.length) {
            return null;
        }

        const result = {};
        for (let i of model.resourceMappings) {
            const key = i["name"];
            const value = i["value"];
            
            //@ts-ignore
            result[key] = value;
        }

        return result;
    };

    const siteDetailsModel = {
        siteName: model.siteName,
        uploadSessionId: model.uploadSessionId,
        description: model.description,
        isActive: model.isActive,
        landingPage: model.landingPage,
        databaseName: model.databaseName,
        tagIds: model.tagIds,
        resourceMappings: getResourceMappings(),
        customRouteHandlers: model.customRouteHandlers
    };

    let isNew = !model.siteId;
    try {
        if (isNew) {
            const newSiteData = (await apiClient.postAsync('api/sitemanagement', siteDetailsModel) as any)?.data;
            model.siteId = newSiteData.siteId;
            for (const up of model.uploaded) {
                newContentHolder.removeContent(up.name);    
            }
            model.uploaded = mapUploaded(newSiteData.uploaded);
        } else {
            const uploadedDataResponse = (await apiClient.putAsync(`api/sitemanagement/${model.siteId}`, siteDetailsModel) as any)?.data;
            for (const up of model.uploaded) {
                newContentHolder.removeContent(up.name);
            }
            model.uploaded = mapUploaded(uploadedDataResponse.uploaded);
        }

        contextHolder.set(getFormContext());
        
        await delay(250);
        model.processing = false;
        
        if (isNew) {
            router.push({ path: '/sites/update/' + model.siteId });
        }
    } catch {
        let mode = model.siteId ? 'edit' : 'create';
        let msg = `Unable to ${mode} your site due to server error. Please try again later.`;
        model.processError = msg;
        model.processing = false;
                    
        return false;
    }

    return true;
};

const goToDashboard = async () => {
    let isModified = contextHolder.isModified(getFormContext())
        || model.uploaded.find((u: any) => u.isNew);
    
    if (!isModified) {
        router.replace("/dashboard");
        return;
    }

    if (confirm("You have made changes. Do you want to save them?")) {
        if (!await createOrUpdateSite()) {
            // Errors during save
            return;
        }
    }

    if (model.uploadSessionId && model.uploadSessionId.length) {
        await apiClient.postAsync('api/contentupload/cancelupload', undefined, 
            { "upload-session-id" : model.uploadSessionId } as unknown as AxiosRequestHeaders);
    }

    router.replace("/dashboard");
};

const getUploadSessionId = async () => {
    let sessionId = model.uploadSessionId;
    if (!sessionId) {
        let sessionResponse = await apiClient.getAsync('api/contentupload/session') as any;
        model.uploadSessionId = sessionResponse.headers['upload-session-id'];
        sessionId = model.uploadSessionId;
    }
    
    return sessionId;
};

const newHtmlPage = async () => {
    const sessionId = await getUploadSessionId();

    stateManager.save({
        uploadSessionId: sessionId,
        siteName: model.siteName,
        siteId: model.siteId!,
        description: model.description,
        isActive: model.isActive,
        landingPage: model.landingPage,
        databaseName: model.databaseName,
        resourceMappings: model.resourceMappings,
        customRouteHandlers: model.customRouteHandlers,
        tagIds: model.tagIds,
        uploadedFiles: model.uploaded
    });

    PageContextProvider.set({
        uploadSessionId: sessionId,
        siteId: model.siteId
    } as PageContext);

    router.push({ name: 'page-editor' });
};

const openPageEditor = async (file: ContentFile) => {
    const sessionId = await getUploadSessionId();

    stateManager.save({
        uploadSessionId: sessionId,
        siteName: model.siteName,
        siteId: model.siteId!,
        description: model.description,
        isActive: model.isActive,
        landingPage: model.landingPage,
        databaseName: model.databaseName,
        tagIds: model.tagIds,
        resourceMappings: model.resourceMappings,
        customRouteHandlers: model.customRouteHandlers,
        uploadedFiles: model.uploaded
    });

    PageContextProvider.set({
        siteId: model.siteId,
        contentId: file.id,
        contentName: file.name,
        contentDestinationPath: file.destinationPath, 
        uploadSessionId: model.uploadSessionId,
        cacheDuration: file.cacheDuration
    } as PageContext);

    router.push({ 
        name: 'page-editor'
    });
};

const onCustomHandlersChanged = (handlers: Array<CustomRouteHandler>) => {
    model.customRouteHandlers = handlers;
};

const isSiteNameInvalid = computed(() => {
    let siteNameValidation = model.validation.siteName;
    
    return !siteNameValidation.valid && siteNameValidation.touched;
});

const isSiteNameReserved = computed(() => {
    let siteNameValidation = model.validation.siteName;
    return model.siteName && model.siteName.toLowerCase() == '_error' && siteNameValidation.touched;
});

const saveButtonDisabled = computed(() => {
    let siteNameValidation = model.validation.siteName;
                
    return !model.siteName ||
            model.siteName.toLowerCase() == '_error' ||
            !model.siteName.length ||
            (!siteNameValidation.valid && siteNameValidation.touched) ||
            !model.uploaded.length;
});

const applyValidationErrorClass = computed(() => {
    let siteNameValidation = model.validation.siteName;
    let applied = !siteNameValidation.valid && siteNameValidation.touched;

    return {
        'invalid-field': applied
    };
});
</script>
<template>
<div class="content-block-container">
    <div class="general-page-title">
        <span>{{model.title}}</span>
    </div>
    
    <div class="site-form-header">
        <div>
            <button class="btn btn-primary" @click="goToDashboard" id="dashboardBtn">Dashboard</button>
            <button class="btn btn-primary" :disabled="saveButtonDisabled" @click="createOrUpdateSite">{{ model.siteId ? 'Save' : 'Create'}}</button>  
        </div>
        <Loader :processing="model.processing"/>
        <div class="process-error-bar" v-if="model.processError && model.processError.length">
            <span class="validation-error">{{model.processError}}</span>
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
                            <input type="text"
                                class="site-name-editor"
                                maxlength="30"
                                v-model="model.siteName"
                                @input="validateSiteName"
                                @blur="model.validation.siteName.touched=true;"
                                v-bind:class="applyValidationErrorClass"/>
                        </div>
                        <div class="validation-error" v-if="isSiteNameInvalid">
                            This site name already exists.
                        </div>
                        <div class="validation-error" v-if="isSiteNameReserved">
                            This site name is reserved. Please choose another one.
                        </div>
                        <div class="validation-error" v-if="!model.siteName.length && model.validation.siteName.touched">
                            The site name is required.
                        </div>
                    </td>
                </tr>
                <tr>
                    <td class="vertical-tex-align">Description:</td>
                    <td>
                        <textarea v-model="model.description" class="site-name-editor" rows="10"></textarea>
                    </td>
                </tr>
                <tr>
                    <td class="vertical-tex-align">Landing page:</td>
                    <td>
                        <input type="text" class="landing-page-input" v-model="model.landingPage" />
                    </td>
                </tr>
                <tr>
                    <td class="vertical-tex-align">Database:</td>
                    <td>
                        <DatabaseNameInput 
                            :databaseName="model.databaseName" 
                            :siteId="model.siteId"
                            @update:databaseName="(newDbName: string | null) => model.databaseName = newDbName"/>
                    </td>
                </tr>
                <tr>
                    <td>Is active:</td>
                    <td>
                        <input type="checkbox" v-model="model.isActive" />
                    </td>
                </tr>
                <tr>
                    <td class="tags-label">Tags:</td>
                    <td>
                        <TagsSelectList :tagIds="model.tagIds" :onTagsChanged="(selectedTags: any) => tagsChanged(selectedTags)" />
                    </td>
                </tr>
            </table>
            <div class="resource-mapping-section">
                <ResourceMappingsSection :resourceMappings="model.resourceMappings"/>
            </div>
        </div>
        <div class="site-form-holder-right">           
            <div class="editor-buttons-container">
                <button class="btn btn-primary" @click="newHtmlPage">New HTML Page</button>
            </div>
            <div class="uploaded-content-holder">
                <div>
                    <span class="form-title">Site content</span>
                    <UploadedContentList 
                        :uploadSessionId="model.uploadSessionId" 
                        :uploaded="model.uploaded" 
                        :openPageEditor="openPageEditor"
                        :onNewSessionHanlder="(newSessionId: string)=> model.uploadSessionId = newSessionId"
                        :onUploaded="(contentFiles: ContentFile[]) => model.uploaded = contentFiles" 
                    />
                </div>
                <div class="custom-handlers-container">
                    <span class="form-title">Custom Route Handlers</span>
                    <CustomerHandlerList :handlers="model.customRouteHandlers" :onHandlersChanged="onCustomHandlersChanged" />
                </div>
            </div>
        </div>
    </div> 
</div>
</template>
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
        float: left;
        width: calc(100% - 600px);
    }
    .landing-page-input {
        width: -webkit-fill-available;
    }
    .resource-mapping-section {
        margin-top: 10px;
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
        background-color: lavender;
        padding-left: 10px;
        padding-top: 5px;
        padding-bottom: 5px;
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
    
    textarea {
      resize: none;
    }

    .custom-handlers-container {
        margin-top: 25px;
        padding: 5px;
    }
</style>