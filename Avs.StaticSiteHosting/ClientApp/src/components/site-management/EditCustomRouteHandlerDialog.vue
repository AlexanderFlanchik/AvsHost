<template>
    <ModalDialog ref="dialogRef" :title="dialogTitle" ok-label="OK" :ok="onOk">
        <div class="form-holder">
            <div class="form-row">
                <label for="route-name">Route Name:</label>
                <input id="route-name" type="text" v-model="handlerRef!.name" />
            </div>
            <div class="form-row-validation-message" v-if="nameValidatorMessage">
              {{ nameValidatorMessage }}
            </div>
            <div class="form-row">
                <label for="route-path">Path:</label>
                <input id="route-path" type="text" v-model="handlerRef!.path" />
            </div>
            <div class="form-row-validation-message" v-if="pathValidatorMessage">
              {{ pathValidatorMessage }}
            </div>
            <div class="form-row">
                <label for="method">Method:</label>
                <select id="method" v-model="handlerRef!.method">
                    <option value="GET">GET</option>
                    <option value="POST">POST</option>
                    <option value="PUT">PUT</option>
                    <option value="DELETE">DELETE</option>
                </select>
            </div>           
            <div class="form-handler-body-holder">
                <div class="form-row-validation-message" v-if="bodyValidatorMessage">
                  {{ bodyValidatorMessage }}
                </div>
                <div class="handler-body-container">
                    <label for="handler-body">Handler Body:</label>
                    <textarea id="handler-body" v-model="handlerRef!.body" rows="10"></textarea>
                </div>
            </div>
        </div>
    </ModalDialog>
</template>
<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import ModalDialog from '../ModalDialog.vue';
import { CustomRouteHandler } from './CustomRouteHandler';

interface EditCustomRouteHandlerDialogProps {
    handler: CustomRouteHandler | null;
    handlerValidator: (handler: CustomRouteHandler | null, field: string, value: any) => string | null;
    onHandlerChanged: (newHandlerState: CustomRouteHandler) => void;
}

const defaultHandlerInstance: CustomRouteHandler = {
    id: '',
    name: '',
    path: '',
    method: 'GET',
    body: ''
};

const props = defineProps<EditCustomRouteHandlerDialogProps>();
const handlerRef = ref<CustomRouteHandler | null>(props.handler ?? defaultHandlerInstance);
const dialogRef = ref<InstanceType<typeof ModalDialog> | null>(null);
const dialogTitle = computed(() => handlerRef.value?.id ? 'Edit Custom Route Handler' : 'Add Custom Route Handler');

const nameValidatorMessage = ref<string | null>(null);
const pathValidatorMessage = ref<string | null>(null);
const bodyValidatorMessage = ref<string | null>(null);
const formValid = () => !nameValidatorMessage.value && 
                        !pathValidatorMessage.value && 
                        !bodyValidatorMessage.value;

watch(() => props.handler, (newHandler) => {
    handlerRef.value = newHandler ?? defaultHandlerInstance;
});

const open = () => {
    dialogRef.value?.open();
};

const clear = () => {
    if (!handlerRef.value || handlerRef.value.id) {
        return;
    }

    handlerRef.value.id = '';
    handlerRef.value.name = '';
    handlerRef.value.path = '';
    handlerRef.value.method = 'GET';
    handlerRef.value.body = '';

    nameValidatorMessage.value = null;
    pathValidatorMessage.value = null;
    bodyValidatorMessage.value = null;
}

const onOk = () => {
    nameValidatorMessage.value = props.handlerValidator(handlerRef.value, 'name', handlerRef.value?.name);
    pathValidatorMessage.value = props.handlerValidator(handlerRef.value, 'path', handlerRef.value?.path);
    bodyValidatorMessage.value = props.handlerValidator(handlerRef.value, 'body', handlerRef.value?.body);
    
    if (!formValid()) {
        return false;
    }

    props.onHandlerChanged(handlerRef.value!);
    
    return true;
};

defineExpose({ open, clear });
</script>
<style scoped>
input {
    flex-grow: 1;
}
label {
    width: 150px;
}
.form-holder {
    display: flex;
    flex-direction: column;
    gap: 1rem;
}
.form-row {
    display: flex;
    flex-direction: row;
    gap: 0.5rem;
}
.form-handler-body-holder {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
}
.form-row-validation-message {
    padding: 5px;
    color: red;
    font-weight: bold;
    font-size: 0.9rem;
}
.handler-body-container {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
}
</style>