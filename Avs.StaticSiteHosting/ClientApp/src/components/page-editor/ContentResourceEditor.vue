<script setup lang="ts">
import { inject, reactive, ref } from 'vue';
import { GenericElement, Html, Link, Script } from './html-elements';
import { API_CLIENT, AUTH_SERVICE } from '../../common/diKeys';
import ModalDialog from '../ModalDialog.vue';

const contentFilePlaceHolder = '--Please select a file--';
const newResourcePlaceHolder = '%NEW_RESOURCE%';
const existResourcePlaceHolder = '%EXIST_RESOURCE%';

interface ContentResourceEditorProps {
    htmlTree: Html;
    uploadSessionId: string;
    siteId: string;
    onComplete: () => Promise<void>;
}

interface ContentResourceEditorModel {
    element: GenericElement | null;
    error: string | null;
    contentResourceType: string;
    fromFile: string;
    contentFile: any;
    content: string;
    contentList: Array<any>;
    ok: () => Promise<void>;
}

const addScriptContentModal = ref<typeof ModalDialog | null>(null);
const props = defineProps<ContentResourceEditorProps>();
const model = reactive<ContentResourceEditorModel>({
    element: null,
    error: null,
    contentResourceType: 'js',
    fromFile: 'true',
    contentFile: null,
    content: '',
    contentList: [],
    ok: () => Promise.resolve()
});

const onOpen = () => {
    model.error = null;
    model.content = '';
    model.fromFile = 'true';
    model.contentResourceType = 'js';
};

const apiClient = inject(API_CLIENT)!;
const authService = inject(AUTH_SERVICE)!;

const contentResourceEditor_Ok = async () => {
    const head = props.htmlTree.head;
    const body = props.htmlTree.body;
    const tag = model.element?.tag;
    const fromFile = model.fromFile === 'true';

    // Validation
    if (fromFile) {
        const selectedFile = model.contentFile;
        if (!selectedFile || selectedFile.contentFilePath === contentFilePlaceHolder) {
            // no script or css selected
            model.error = 'Please select a file to continue.';
            return;
        }
    } else {
        let content = model.content;
        if (!content) {
            model.error = 'The content is required.';
            return;
        }
    }

    const getContentSrc = (contentResourceType: any) => {
        const selectedFile = model.contentFile;
        const contentPath = selectedFile.contentFilePath;
        let exists;
                    
        if (tag == 'head') {
            exists = contentResourceType == 'js' ?
                head.scripts.find((s: any) => s.src.endsWith(contentPath)) :
                head.links.find((l: any) => l.href.endsWith(contentPath)); 
        } else {
            // attaching script to body section (for scripts only)
            exists = body.children.find((e: any) => 
                e.tag === 'script' && e.attributes.get('src') && 
                e.attributes.get('src').endsWith(contentPath));
        }

        if (exists) {
            return null;
        }

        let isNew = !selectedFile.contentId;
        let resourceUrl = isNew ? `${newResourcePlaceHolder}/${contentPath}?uploadSessionId=${props.uploadSessionId}`
                        : `${existResourcePlaceHolder}/${contentPath}?siteId=${props.siteId}`;
                        
        return `/${resourceUrl}&__accessToken=${authService.getToken()}`;
    };

    let contentResourceType = model.contentResourceType;
    if (contentResourceType === 'js') {
        const script = new Script();
        script.type = "text/javascript";
        if (fromFile) {
            const src = getContentSrc(contentResourceType);
            
            if (!src) {
                model.error = 'This script already exists.';
                return;
            }
            script.src = src;
        } else {
            script.body = model.content;
        }

        if (tag === 'head') {
            head.scripts.push(script);
        } else {
            // attach script to the end of body section
            const scriptElement = new GenericElement();
            scriptElement.tag = 'script';
            if (fromFile) {
                scriptElement.attributes.set('src', script.src!);
            }
        }
    } else {
        if (fromFile) {
            const link = new Link();
            const src = getContentSrc(contentResourceType);
            
            if (!src) {
                model.error = 'This stylesheet already exists.';
                return;
            }
                        
            link.type = "text/css";
            link.rel = "stylesheet";
            link.href = src;
            head.links.push(link);
        } else {
            head.styles.push(model.content);                       
        }
    }

    model.error = null;

    // update HTML tree & page preview
    await props.onComplete();
    addScriptContentModal.value?.close();
}; 

const addScriptOrStylesheet = async (element: any) => {
    model.element = element;
    onOpen();

    let filesUrl = 'api/ResourceContent';
    let queryParameterSet =  false;
    if (props.siteId) {
        filesUrl += `?siteId=${props.siteId}`;
        queryParameterSet = true;
    }

    if (props.uploadSessionId) {
        filesUrl += (queryParameterSet ? '&' : '?') + `uploadSessionId=${props.uploadSessionId}`;
        queryParameterSet = true;
    }

    const contentResourceType = model.contentResourceType;
    filesUrl += (queryParameterSet ? '&' : '?') + `contentExtension=${contentResourceType}`;
                
    try {
        const filesResponse = await apiClient.getAsync(filesUrl) as any;
        if (filesResponse.status == 200) {
            model.contentList = filesResponse.data;
        }
    } catch {
        // no-op
        model.error = 'Unable to get files list from the server due to server error.';
    }

    model.contentList = model.contentList.filter(
        (i: any) => !props.htmlTree.head.scripts.find((s: any) => s.src && s.src.indexOf(i.contentFilePath) >= 0) && 
                    !props.htmlTree.head.links.find((l: any) => l.href.indexOf(i.contentFilePath) >= 0) && 
                    props.htmlTree.body.outerHtml.indexOf(i.contentFilePath) < 0);
        
    model.contentList.unshift({ id: null, contentFilePath: contentFilePlaceHolder });
    model.contentFile = model.contentList[0];
    model.ok = contentResourceEditor_Ok;
    addScriptContentModal.value?.open();
};

defineExpose({ addScriptOrStylesheet });
</script>
<template>
 <ModalDialog ref="addScriptContentModal" title="Add Script or Stylesheet" :ok="model.ok">
    <div v-if="model.error">
        <span class="validation-error">{{model.error}}</span>
    </div>
    <fieldset>
        <legend>Resource type</legend>
        <input type="radio" v-model="model.contentResourceType" value="js" name="js_type" />
        <label for="js_type">JavaScript</label>
        <input type="radio" v-model="model.contentResourceType" value="css" name="css_type" />
        <label for="css_type">Cascade Stylesheet (CSS)</label>
    </fieldset>
    <fieldset>
        <legend>Resource content</legend>
        <div class="mrgn-top-5">
            <input type="radio" v-model="model.fromFile" value="true" name="fromFile" />
            <label for="fromFile">From file</label> <br/>
            <select class="resource-files-select" v-model="model.contentFile" :disabled="model.fromFile != 'true'">
                <option v-for="file in model.contentList" :key="file" :value="file">
                    {{file.contentFilePath}}
                </option>
            </select>
        </div>
        <div class="mrgn-top-5">
            <input type="radio" v-model="model.fromFile" value="false" name="fromContent" />
            <label for="fromContent">From content:</label> <br/>
            <textarea class="resource-content-area" v-model="model.content" :disabled="model.fromFile === 'true'"></textarea>
        </div>
    </fieldset>
 </ModalDialog>
</template>
<style scoped>
    .resource-files-select {
        width: -webkit-fill-available;
    }
    
    .resource-content-area {
    height: 400px;
    }

    .mrgn-top-5 {
        margin-top: 5px;
    }

    .mrgn-top-5 textarea {
        width: -webkit-fill-available;
        resize: none;
    }
</style>