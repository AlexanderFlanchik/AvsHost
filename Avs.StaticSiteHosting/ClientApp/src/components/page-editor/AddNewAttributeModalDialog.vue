<script setup lang="ts">
import { reactive, ref } from 'vue';
import ModalDialog from '../ModalDialog.vue';

const model = reactive<{ error: string | null, name: string, value: string }>({
    error: null,
    name: '',
    value: ''
});

const props = defineProps<{ attributes: Array<{ name: string, value: string }> }>();
const dialogRef = ref<typeof ModalDialog | null>();

const clear = () => {
    model.error = null;
    model.name = '';
    model.value = '';
};

const okClick = () => {
    let name = model.name;
    if (!name) {
        model.error = "'Name' field is required.";
        return;   
    }

    let existingAttribute = (props.attributes || []).find((a: any)=> a.name == name);
    if (existingAttribute) {
        model.error = `The attribute with name '${name}' already exists.`;
        return;
    }

    const newAttribute = { name: name, value: model.value };
    props.attributes.push(newAttribute);
                            
    clear();

    dialogRef.value?.close();
};

const open = () => {
    clear();
    dialogRef.value?.open();
};

defineExpose({ open });
</script>
<template>
 <ModalDialog ref="dialogRef" title="Add New Attribute" :ok="okClick" ok-label="Add">
    <div v-if="model.error">
        <span class="validation-error">{{model.error}}</span> 
    </div>
    <div class="attribute-grid">
        <span>Name:</span>
        <input type="text" v-model="model.name" @onchange="() => model.error = null" />
        <span>Value:</span>
        <input type="text" v-model="model.value" @onchange="() => model.error = null" />
    </div>
 </ModalDialog>
</template>
<style scoped>
   .attribute-grid {
        display: grid; 
        grid-template-columns: 50px 1fr; 
        row-gap: 5px;
   }
</style>