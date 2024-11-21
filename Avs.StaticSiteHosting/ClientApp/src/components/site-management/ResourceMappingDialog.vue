<script setup lang="ts">
 import { reactive, ref } from 'vue';
 import ModalDialog from './../ModalDialog.vue';
 import { Subject } from 'rxjs';

 interface ResourceMappingDialogModel {
    resultSubject: Subject<any>,
    name: string;
    nameErrorMessage: string | null;
    value: string;
    valueErrorMessage: string | null;
    resourceMappings: Array<any>;
 }

 const model = reactive<ResourceMappingDialogModel>({
    resultSubject: new Subject(),   
    name: '',
    nameErrorMessage: null,
    value: '',
    valueErrorMessage: null,
    resourceMappings: []
 });

 const okClick = () => {
    let valid = true;
    if (!model.name) {
        valid = false;
        model.nameErrorMessage = "The name field is required.";
    }

    if (!model.value) {
        valid = false;
        model.valueErrorMessage = "The value field is required.";
    }

    if (model.resourceMappings.find((m:{ name: string }) => m.name == model.name)) {
        valid = false;
        model.nameErrorMessage = "This mapping already exists.";
    }

    if (!valid) {
        return;
    }

    model.resultSubject.next({ name: model.name, value: model.value });
    modalDialogRef.value?.close();
 };

 const modalDialogRef = ref<typeof ModalDialog | null>(null);
 const open = (resourceMappings: Array<any>) => {
    clear();
    model.resourceMappings = resourceMappings;
    modalDialogRef.value?.open();

    return model.resultSubject as Subject<any>;
 };
 
 const clear = () => {
    model.resultSubject = new Subject();
    model.name = '';
    model.value = '';
    model.nameErrorMessage = null;
    model.valueErrorMessage = null;
 };

 defineExpose({ open });
</script>
<template>
    <ModalDialog ref="modalDialogRef" title="New Resource Mapping" :ok="okClick" :onClose="clear" okLabel="Add">
        <table class="site-form">
            <tr>
                <td>Name:</td>
                <td>
                    <input type="text" maxlength="30" v-model="model.name" @change="() => model.nameErrorMessage = null" />
                </td>
            </tr>
            <tr v-if="model.nameErrorMessage">
                <td colspan="2">
                    <span class="validation-error">{{model.nameErrorMessage}}</span> 
                </td>
            </tr>
            <tr>
                <td>Value:</td>
                <td>
                    <input type="text" maxlength="30" v-model="model.value" @change="model.valueErrorMessage = null" />
                </td>
            </tr>
            <tr v-if="model.valueErrorMessage">
                <td colspan="2">
                    <span class="validation-error">{{model.valueErrorMessage}}</span>
                </td>
            </tr>
        </table>
    </ModalDialog>
</template>
<style scoped>

</style>