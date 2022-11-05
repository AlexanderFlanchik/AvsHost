<template>
    <div class="upload-form">
        <span class="form-title">Upload files</span>
        <div>
            <b-form-file v-model="contentFile"
                :state="Boolean(contentFile)"
                placeholder="Choose a file or drop it here..."
                drop-placeholder="Drop file here..."></b-form-file>
        </div>
        <div class="mrg-tp-10x">
                <b-form-checkbox v-model="useDestinationPath">Use destination path</b-form-checkbox>
        </div>
            <div class="upload-bottom-bar">
                <div class="upload-bottom-bar-left">
                    <b-form-input v-model="destinationPath" :disabled="!useDestinationPath"></b-form-input>
                </div>
                <div class="upload-bottom-bar-right">
                    <button class="btn btn-primary" @click="uploadContentFile" :disabled="!Boolean(contentFile)">Upload..</button>
                </div>
        </div>
        <div v-if="errorMessage" class="upload-error-holder">
            {{errorMessage}}
        </div>
    </div>
</template>
<script>
    import { ContentFile } from '../../common/ContentFile';
    
    export default {
        props: {
            uploaded: Object,
            uploadSessionId: String,
            onNewSessionHanlder: Object
        },

        data: function() {
            return {
                errorMessage: '',
                contentFile: null,
                useDestinationPath: false
            };
        },
        methods: {
            uploadContentFile: async function() {
                let file = this.contentFile;
                
                if (!this.uploadSessionId) {
                    let sessionResponse = await this.$apiClient.getAsync('api/contentupload/session');
                    this.uploadSessionId = sessionResponse.headers['upload-session-id'];
                    if (this.onNewSessionHanlder) {
                        this.onNewSessionHanlder(this.uploadSessionId);
                    }
                }

                let formData = new FormData();               
                formData.append('contentFile', file);

                let uploadUrl = `api/contentupload?uploadSessionId=${this.uploadSessionId}`;
                if (this.useDestinationPath && this.destinationPath) {
                    uploadUrl += `&destinationPath=${this.destinationPath}`;
                }

                try {
                    await this.$apiClient.postAsync(uploadUrl, formData);

                    if (!this.uploaded.find(u => u.name == file.name && u.destinationPath == this.destinationPath)) {
                        let cf = new ContentFile(
                                    null,
                                    file.name,
                                    this.destinationPath,
                                    true,
                                    file.size,
                                    false,
                                    false,
                                    new Date(),
                                    null
                                );
                            
                        this.uploaded.push(cf);
                    }

                    this.clear();
                } catch {
                    let msg = `Unable to upload ${file.name}.`;
                    if (this.destinationPath) {
                        msg += 'Please check destination path.';
                    }
                    this.errorMessage = msg;
                }
            },

            clear: function() {
                this.errorMessage = '';
                this.contentFile = null;
                this.useDestinationPath = false;
                this.destinationPath = '';
            }
        }
    }
</script>
<style scoped>
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
</style>