<script setup lang="ts">
import { Subject } from 'rxjs';
import { reactive, ref } from 'vue';
import ModalDialog from '../ModalDialog.vue';

interface EditContentDialogModel {
    resultSubject: Subject<any>;
    fileName: string;
    content: string;
    error: string | null;
    contentValidator?: (content: string) => string;
}

const model = reactive<EditContentDialogModel>({
    resultSubject: new Subject(),
    fileName: '',
    content: '',
    error: null,
    contentValidator: (_: string) => ''
});

const editContentDialogRef = ref<typeof ModalDialog | null>(null);
const showDialog = (fileName: string, content: string, contentValidator: (content: string) => string) => {
    model.fileName = fileName;
    model.content = content;
    model.contentValidator = contentValidator;

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

    model.resultSubject.next(model.content);
    editContentDialogRef.value?.close();
};

defineExpose({ showDialog });
</script>
<template>
    <ModalDialog ref="editContentDialogRef" :title="model.fileName" :ok="updateContent">
    <div class="validation-error" v-if="model.error">{{model.error}}</div>
    <div class="content-centered">
        <textarea v-model="model.content" class="content-editor" @change="() => model.error = null"></textarea>
    </div>
    </ModalDialog>
</template>
<style scoped>
   .dlg-btn-container-right {
        text-align: right;
    }
    .content-editor {
        min-height: 570px;
        width: -webkit-fill-available;
        border: 0;
        resize: none;
    }
</style>