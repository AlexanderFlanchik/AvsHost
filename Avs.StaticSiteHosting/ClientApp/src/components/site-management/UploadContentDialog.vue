<template>
    <ModalDialog ref="modalDialogRef" title="Upload Content" :ok="onOk" ok-label="Upload.." :width="600" :height="600">        
        <UploadSiteContent 
            ref="uploadSiteContentRef"
            :uploadSessionId="props.uploadSessionId" 
            :uploaded="fileListRef"
            :onNewSessionHanlder="props.onNewSessionHanlder"/>
    </ModalDialog>
</template>
<script setup lang="ts">
import { ref, watch } from 'vue';
import { ContentFile } from '../../common/ContentFile';
import ModalDialog from '../ModalDialog.vue';
import UploadSiteContent from './UploadSiteContent.vue';

interface UploadContentDialogProps {
    uploadSessionId: string;
    uploaded: ContentFile[];
    onNewSessionHanlder: (newSessionId: string) => void;
    onClose: (contentFiles: ContentFile[]) => void;
}

const uploadSiteContentRef = ref<typeof UploadSiteContent | null>(null);
const props = defineProps<UploadContentDialogProps>();
const fileListRef = ref<ContentFile[]>(props.uploaded);
const modalDialogRef = ref<typeof ModalDialog | null>(null);

watch(() => props.uploaded, (newVal) => fileListRef.value = newVal);

const open = () => {
    modalDialogRef.value?.open();
};

const onOk = async () => {
    const canClose = await uploadSiteContentRef.value?.uploadFile();
    if (!canClose) {
        return;
    }
    
    props.onClose(fileListRef.value);
    modalDialogRef.value?.close();
};

defineExpose({ open });
</script>
<style scoped>

</style>