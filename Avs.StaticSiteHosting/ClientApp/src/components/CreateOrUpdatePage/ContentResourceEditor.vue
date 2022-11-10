<template>
    <b-modal  size="xl" ref="add-script-content-modal" hide-footer title="Add Script or Stylesheet">
        <div v-if="error">
            <span class="validation-error">{{error}}</span>
        </div>
        <div>
            <b-form-group label="Resource type" v-slot="{ ariaDescribedby }">
                <b-form-radio-group id="contentResourceType" v-model="contentResourceType" :aria-describedby="ariaDescribedby" name="scriptTypeList">
                    <b-form-radio value="js">JavaScript</b-form-radio>
                    <b-form-radio value="css">Cascade Stylesheet (CSS)</b-form-radio>
                </b-form-radio-group>
            </b-form-group>
        </div>
        <div>
            <b-form-group label="Resource content" stacked>
                <b-form-radio-group v-model="fromFile">
                    <b-form-radio value="true">From file</b-form-radio> <br/>
                    <select class="resource-files-select" v-model="contentFile" :disabled="fromFile != 'true'">
                        <option v-for="file in contentList" :key="file" :value="file">
                            {{file.contentFilePath}}
                        </option>
                    </select> <br/>
                    <b-form-radio value="false">From content:</b-form-radio> <br/>
                    <b-form-textarea class="resource-content-area" v-model="content" :disabled="fromFile == 'true'"></b-form-textarea>
                </b-form-radio-group>
            </b-form-group>
        </div>
        <div class="modal-btn-holder">
            <button class="btn btn-primary" @click="() => this.ok()">OK</button>
            <button class="btn btn-default" @click="() => this.$refs['add-script-content-modal'].hide()">Cancel</button>
        </div>
    </b-modal>
</template>
<script>
    const contentFilePlaceHolder = '--Please select a file--';
    const newResourcePlaceHolder = '%NEW_RESOURCE%';
    const existResourcePlaceHolder = '%EXIST_RESOURCE%';

    import { GenericElement, Script, Link } from '../../content-creation/html-elements';

    export default {
        props: {
            htmlTree: Object,
            uploadSessionId: String,
            siteId: String,
            onComplete: Object
        },

        data: function() {
            return {
                element: null,
                error: null,
                contentResourceType: 'js',
                fromFile: 'true',
                contentFile: null,
                content: '',
                contentList: [],
                ok: () => {}
            };
        },
        methods: {
            onOpen: function() {
                this.error = null;
                this.content = null;
                this.fromFile = 'true';
                this.contentResourceType = "js";
            },

            addScriptOrStylesheet: async function(element) {
                this.element = element;
                this.onOpen();

                let filesUrl = 'api/ResourceContent';
                let queryParameterSet =  false;
                if (this.siteId) {
                    filesUrl += `?siteId=${this.siteId}`;
                    queryParameterSet = true;
                }

                if (this.uploadSessionId) {
                    filesUrl += (queryParameterSet ? '&' : '?') + `uploadSessionId=${this.uploadSessionId}`;
                    queryParameterSet = true;
                }

                let contentResourceType = this.contentResourceType;
                filesUrl += (queryParameterSet ? '&' : '?') + `contentExtension=${contentResourceType}`;
                try {
                    let filesResponse = await this.$apiClient.getAsync(filesUrl);
                    if (filesResponse.status == 200) {
                        this.contentList = filesResponse.data;
                    }
                } catch {
                    // no-op
                    this.error = 'Unable to get files list from the server due to server error.';
                }
                
                // filter resources which have already been added to the page
                this.contentList = this.contentList.filter(
                        i => !this.htmlTree.head.scripts.find(s => s.src && s.src.indexOf(i.contentFilePath) >= 0) && 
                                !this.htmlTree.head.links.find(l => l.href.indexOf(i.contentFilePath) >= 0) && 
                                this.htmlTree.body.outerHtml.indexOf(i.contentFilePath) < 0);
        
                this.contentList.unshift({ id: null, contentFilePath: contentFilePlaceHolder });
                this.contentFile = this.contentList[0];
                this.ok = this.contentResourceEditor_Ok;
                this.$refs['add-script-content-modal'].show();
            },

            contentResourceEditor_Ok: async function() {
                let head = this.htmlTree.head;
                let body = this.htmlTree.body;
                let tag = this.element.tag;
                let fromFile = this.fromFile === 'true';
                
                // Validation
                if (fromFile) {
                   let selectedFile = this.contentFile;
                   if (!selectedFile || selectedFile.contentFilePath === contentFilePlaceHolder) {
                     // no script or css selected
                     this.error = 'Please select a file to continue.';
                     return;
                   }
                } else {
                    let content = this.content;
                    if (!content) {
                        this.error = 'The content is required.';
                        return;
                    }
                }     
                
                const getContentSrc = (contentResourceType) => {
                    let selectedFile = this.contentFile;
                    let contentPath = selectedFile.contentFilePath;
                    let exists;
                    if (tag == 'head') {
                        exists = contentResourceType == 'js' ?
                            head.scripts.find(s => s.src.endsWith(contentPath)) :
                            head.links.find(l => l.href.endsWith(contentPath)); 
                    } else {
                        // attaching script to body section (for scripts only)
                        exists = body.children.find(e => 
                            e.tag === 'script' && e.attributes.get('src') && 
                            e.attributes.get('src').endsWith(contentPath));
                    }

                    if (exists) {
                        return null;
                    }

                    let isNew = !selectedFile.contentId;
                    let resourceUrl = isNew ? `${newResourcePlaceHolder}/${contentPath}?uploadSessionId=${this.uploadSessionId}`
                        : `${existResourcePlaceHolder}/${contentPath}?siteId=${this.siteId}`;
                        
                    return `/${resourceUrl}&__accessToken=${this.$authService.getToken()}`;
                };

                let contentResourceType = this.contentResourceType;
                if (contentResourceType === 'js') {
                    let script = new Script();
                    script.type = 'text/javascript';
                    if (fromFile) {
                        let src = getContentSrc(contentResourceType);
                        if (!src) {
                            this.error = 'This script already exists.';
                            return;
                        }
                        script.src = src;
                    } else {
                        script.body = this.content;
                    }
                    if (tag === 'head') {
                        head.scripts.push(script);
                    } else {
                        // attach script to the end of body section
                        let scriptElement = new GenericElement();
                        scriptElement.tag = 'script';
                        if (fromFile) {
                            scriptElement.attributes.set('src', script.src);
                        }
                    }
                } else {
                    if (fromFile) {
                        let link = new Link();
                        let src = getContentSrc(contentResourceType);
                        if (!src) {
                            this.error = 'This stylesheet already exists.';
                            return;
                        }
                        link.type = "text/css";
                        link.rel = "stylesheet";
                        link.href = src;
                        head.links.push(link);
                    } else {
                        head.styles.push(this.content);                       
                    }
                }

                this.error = null;
                
                // update HTML tree & page preview
                await this.onComplete();

                this.$refs['add-script-content-modal'].hide();
            }

        }
    }
</script>
<style scoped>
    .resource-files-select {
        width: -webkit-fill-available;
    }
    
    .resource-content-area {
    height: 400px;
    }
</style>