<script setup lang="ts">
import { inject, onMounted, ref, reactive } from 'vue';
import { ContentFile } from '../../common/ContentFile';
import { API_CLIENT } from '../../common/diKeys';

interface UploadSiteContentProps {
    uploaded: Array<ContentFile>;
    onNewSessionHanlder?: (sessionId: string) => void;
    uploadSessionId?: string;
}

interface UploadSiteContentModel {
    sessionId?: string;
    errorMessage: string;
    useDestinationPath: boolean;
    destinationPath?: string;
    readyToUpload: boolean;
}

const props = defineProps<UploadSiteContentProps>();
const model = reactive<UploadSiteContentModel>({
    errorMessage: '',
    useDestinationPath: false,
    readyToUpload: false
});

const apiClient = inject(API_CLIENT)!;
const fileRef = ref<File | null>(null);

const clear = () => {
    model.errorMessage = '';
    fileRef.value = null;
    model.useDestinationPath = false;
    model.destinationPath = '';
};

const uploadFile = async() => {
    const file = fileRef.value!;
    
    if (!model.sessionId) {
        const sessionResponse = await apiClient.getAsync('api/contentupload/session') as any;
        model.sessionId = sessionResponse.headers['upload-session-id'];
        if (props.onNewSessionHanlder) {
            props.onNewSessionHanlder(model.sessionId!);
        }
    }

    const formData = new FormData();
    formData.append('contentFile', <any>file);
    
    let uploadUrl = `api/contentupload?uploadSessionId=${model.sessionId}`;
    if (model.useDestinationPath && model.destinationPath) {
        uploadUrl += `&destinationPath=${model.destinationPath}`;
    }

    try {
        await apiClient.postAsync(uploadUrl, formData);

        if (!props.uploaded.find((u: ContentFile) => u.name == file.name && u.destinationPath == model.destinationPath)) {
            let cf = new ContentFile(
                null,
                file.name,
                model.destinationPath ?? '',
                true,
                file.size,
                false,
                false,
                new Date(),
                null
            );
                            
            props.uploaded.push(cf);
        }

        clear();
    } catch {
        let msg = `Unable to upload ${file.name}.`;
        if (model.destinationPath) {
            msg += 'Please check destination path.';
        }
        
        model.errorMessage = msg;
    }

    model.readyToUpload = false;
};

onMounted(() => {
    model.sessionId = props.uploadSessionId;
});

const onFileChanged = ($event: Event) => {
    const target = $event.target as HTMLInputElement;
    if (target && target.files) {
        const file = target.files[0];
        fileRef.value = file;
        model.readyToUpload = true;
    }
};

</script>
<template>
    <div class="upload-form">
        <span class="form-title">Upload files</span>
        <div class="mrg-tp-10x">
            <input type="file" class="file-input" @change="onFileChanged($event)" />
        </div>
        <div class="mrg-tp-10x">
            <input type="checkbox" v-model="model.useDestinationPath" name="useDestinationPath"/>
            <label for="useDestinationPath">Use destination path</label>
        </div>
        <div class="upload-bottom-bar">
            <div class="upload-bottom-bar-left">
                <input v-model="model.destinationPath" :disabled="!model.useDestinationPath" />
            </div>
             <div class="upload-bottom-bar-right">
                <button class="btn btn-primary" @click="uploadFile" :disabled="!model.readyToUpload">Upload..</button>
            </div>
        </div>
        <div v-if="model.errorMessage" class="upload-error-holder">
            {{model.errorMessage}}
        </div>
    </div>
</template>
<style scoped>
    .upload-form {
        padding-top: 2px;
        max-width: 550px;
    }
    .file-input {
        background-color: yellow;
        border: 1px solid red;
        width: -webkit-fill-available;
        height: 30px;
        line-height: 30px;
        padding-left: 5px;
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
    .upload-bottom-bar-left input {
        width: 100%;
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
</style>