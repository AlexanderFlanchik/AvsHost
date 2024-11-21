<script setup lang="ts">
import { reactive, ref } from 'vue';
import ModalDialog from '../ModalDialog.vue';
 
const model = reactive<{ fileName: string, src: string }>({
    fileName: '',
    src: ''
 });

const viewContentDialogRef = ref<typeof ModalDialog | null>(null);
const open = (fileName: string, src: string) => {
    model.fileName = fileName;
    model.src = src;
    viewContentDialogRef.value?.open();
};

defineExpose({ open });

</script>
<template>
    <ModalDialog ref="viewContentDialogRef" :title="model.fileName" cancelLabel="Close">
        <div class="content-centered">
            <img :src="model.src" />
        </div>
    </ModalDialog>
</template>
<style scoped>

</style>