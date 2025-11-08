<script setup lang="ts">
 import { onMounted, reactive } from 'vue';
 
 export interface ModalDialogProps {
    title?: string;
    okLabel?: string;
    cancelLabel?: string;
    ok?: () => any;
    onClose?: () => void;
    dialogWidth?: string;
    dialogHeight?: string;
 } 
 
 const props = defineProps<ModalDialogProps>();
 const model = reactive<{ isOpen: boolean, dialogWidth: string, dialogHeight: string }>({
    isOpen: false,
    dialogWidth: "450px",
    dialogHeight: "auto"
 });

 const close = () => {
    props.onClose && props.onClose();
    model.isOpen = false;
 };

 const open = () => model.isOpen = true;
 const clickOk = () => {
    if (props.ok) {
        const result = props.ok();
        if (result instanceof Boolean) {
            model.isOpen = <boolean>result;
        }
    } else {
        model.isOpen = false;
    } 
};

onMounted(() => {
   if (props.dialogWidth) {
     model.dialogWidth = props.dialogWidth;
   }

   if (props.dialogHeight) {
    model.dialogHeight = props.dialogHeight;
   }
});

 defineExpose({ close, open });

</script>
<template>
    <div v-if="model.isOpen" class="modal-dialog-wrapper">
        <div class="modal-dialog-container" ref="dialogContainerRef" :style="{ width: model.dialogWidth, height: model.dialogHeight }">
            <div class="modal-dialog-header">
                {{ props.title }}
            </div>
            <div class="model-dialog-content">
                <slot></slot>
            </div>
            <div class="model-dialog-footer">
                <button v-if="props.ok" class="btn btn-primary" @click="clickOk">{{ props.okLabel ? props.okLabel : "OK" }}</button> &nbsp;
                <button class="btn btn-default" @click="close">{{ props.cancelLabel ? props.cancelLabel : "Cancel" }}</button>
            </div>
        </div>
    </div>
</template>
<style scoped>
    .modal-dialog-header {
        background-color: navy;
        color: white;
        padding: 5px;
        line-height: 16px;
        font-size: 16px;
    }

    .modal-dialog-wrapper {
        position: fixed;
        z-index: 9998;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background-color: rgba(0.5, 0.5, 0.5, 0.5);
    }

    .modal-dialog-container {
        border: 1px solid navy;
        margin: 150px auto;
        background-color: #fff;
        border-radius: 2px;
        box-shadow: 0 2px 8px rgba(0, 0, 0, 0.33);
    }

    .model-dialog-content {
        padding: 5px;
    }

    .model-dialog-footer {
        padding-top: 5px;
        padding-bottom: 5px;
        margin-top: 5px;
        border-top: 1px solid navy;
        text-align: right;
        display: flex;
        justify-content: right;
        padding-right: 5px;
        gap: 1px;
    }
</style>