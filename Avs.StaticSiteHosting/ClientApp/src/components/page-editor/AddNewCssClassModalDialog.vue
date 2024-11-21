<script setup lang="ts">
import { reactive, ref } from 'vue';
import ModalDialog from '../ModalDialog.vue';

const props = defineProps<{ cssClasses: Array<string> }>();
const model = reactive<{ error: string | null, name: string }>({
    error: null,
    name: ''
});

const addCssClassModalRef = ref<typeof ModalDialog | null>(null);
const clear = () => {
    model.error = null;
    model.name = '';
};

const okClick = () => {
    const name = model.name;
    if (!name || !name.length) {
        model.error = "'Name' field is required.";
        return;
    }

    const names = name.split(' ');
    for (const nm of names) {
        const cssClass = props.cssClasses.find((c: any) => c == nm);
        if (!cssClass) {
            props.cssClasses.push(nm);
        }
    }

    clear();
    addCssClassModalRef.value?.close();
};

const open = () => {
    clear();
    addCssClassModalRef.value?.open();
};

defineExpose({ open });
</script>
<template>
 <ModalDialog ref="addCssClassModalRef" title="Add CSS Class" :ok="okClick">
    <div v-if="model.error">
        <span class="validation-error">{{model.error}}</span> 
    </div>
    <div>
        <span>Name:</span> <br />
        <input type="text" v-model="model.name" @change="() => model.error = null" />
    </div>
 </ModalDialog>
</template>
<style scoped>

</style>