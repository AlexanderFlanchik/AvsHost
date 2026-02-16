<script setup lang="ts">
import { Subject } from 'rxjs';
import { reactive, ref } from 'vue';
import ModalDialog from '../ModalDialog.vue';
import TimeSpanInput from './TimeSpanInput.vue';
import { CodeEditor } from 'monaco-editor-vue3';

interface EditContentDialogModel {
    resultSubject: Subject<any>;
    fileName: string;
    content: string;
    error: string | null;
    contentType?: string;
    cacheDuration: string | undefined;
    contentValidator?: (content: string) => string;
};

const editorOptions = {
    lineNumbers: 'on',
    wordWrap: 'on',
    minimap: { enabled: false },
    scrollBeyondLastLine: false,
    automaticLayout: true
};

const model = reactive<EditContentDialogModel>({
    resultSubject: new Subject(),
    fileName: '',
    content: '',
    error: null,
    contentType: undefined,
    cacheDuration: undefined,
    contentValidator: undefined
});

const editContentDialogRef = ref<typeof ModalDialog | null>(null);
const showDialog = (
        fileName: string, 
        content: string,
        contentValidator?: (content: string) => string,
        cacheDuration: string | undefined = undefined,
        contentType: string | undefined = undefined
    ) => {
        model.fileName = fileName;
        model.content = content;
        model.contentValidator = contentValidator;
        model.cacheDuration = cacheDuration;
        model.contentType = contentType;
        editContentDialogRef.value?.open();

        return model.resultSubject;
    };

const updateContent = () => {
    if (model.contentValidator) {
        const validationError = model.contentValidator(model.content);
        if (validationError) {
            model.error = validationError;
            return;
        }
    }

    model.resultSubject.next({ newContent: model.content, cacheDuration: model.cacheDuration });
   
    return true;
};

const onEditorChanged = (_: string) => {    
    model.error = null;
};

defineExpose({ showDialog });
</script>
<template>
    <ModalDialog ref="editContentDialogRef" :title="model.fileName" :ok="updateContent">
        <div class="validation-error" v-if="model.error">{{model.error}}</div>
        <span>
            Cache duration:
            <TimeSpanInput v-model="model.cacheDuration"/>
        </span>
        <div class="content-centered content-editor" >
            <textarea v-if="!model.contentType" v-model="model.content"  @change="() => model.error = null"></textarea>
            <CodeEditor v-else v-model:value="model.content"                 
                :options="editorOptions"
                :language="model.contentType"
                @change="(value: string) => onEditorChanged(value)">
            </CodeEditor>
        </div>
    </ModalDialog>
</template>
<style scoped>
   .dlg-btn-container-right {
        text-align: right;
    }
    .content-editor {
        height: 450px;
        width: -webkit-fill-available;
        border: 0;
        resize: none;
    }
</style>