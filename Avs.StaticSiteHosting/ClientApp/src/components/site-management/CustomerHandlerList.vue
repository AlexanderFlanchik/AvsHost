<template>
    <div>        
        <EditCustomRouteHandlerDialog
            ref="handlerDialogRef"
            :handler="handlerRef"
            :handlerValidator="handlerValidator"
            :onHandlerChanged="onOk" />
        <div class="add-new-handler-container">
            <a href="javascript:void(0)" @click="addNewRoute">Add New Route..</a>
        </div>
        <div class="no-handlers-message" v-if="handlerListRef.length === 0">
            <p>No custom route handlers defined.</p>
        </div>
        <table v-else class="table table-stripped">
            <thead>
                <tr>
                    <th>Route Name</th>
                    <th>Path</th>
                    <th>HTTP Method</th>
                    <th>Actions</th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="(handler, index) in handlerListRef" :key="index">
                    <td>{{ handler.name }}</td>
                    <td>{{ handler.path }}</td>
                    <td>{{ handler.method }}</td>
                    <td>
                        <a href="javascript:void(0)" @click="edit(handler)">Edit</a> |
                        <a href="javascript:void(0)" @click="remove(handler)">Remove</a>
                    </td>
                </tr>
            </tbody>
        </table>
    </div>
</template>
<script setup lang="ts">
import { ref, watch } from 'vue';
import { CustomRouteHandler } from './CustomRouteHandler';
import EditCustomRouteHandlerDialog from './EditCustomRouteHandlerDialog.vue';

interface CustomerHandlerListProps {
    handlers: Array<CustomRouteHandler>
    onHandlersChanged?: (handlers: Array<CustomRouteHandler>) => void;
}

const handlerRef = ref<CustomRouteHandler | null>(null);
const props = defineProps<CustomerHandlerListProps>();
const handlerListRef = ref<Array<CustomRouteHandler>>(props.handlers || []);
const handlerDialogRef = ref<InstanceType<typeof EditCustomRouteHandlerDialog> | null>(null);

watch(() => props.handlers, (newHandlers) => {
    handlerListRef.value = newHandlers || [];
});

const onOk = (newHandlerState: CustomRouteHandler) => {
    const existingHandler = handlerListRef.value.find(
        h => h.id === newHandlerState.id ||
            (h.name === newHandlerState.name && h.path === newHandlerState.path && h.method === newHandlerState.method)
    );
    
    if (existingHandler) {
        existingHandler.name = newHandlerState.name;
        existingHandler.path = newHandlerState.path;
        existingHandler.method = newHandlerState.method;
        existingHandler.body = newHandlerState.body;
    } else {
        const newHandler : CustomRouteHandler = {
            id: '',
            name: newHandlerState.name,
            path: newHandlerState.path,
            method: newHandlerState.method,
            body: newHandlerState.body
        };

        handlerListRef.value.push(newHandler);
    }

    props.onHandlersChanged?.(handlerListRef.value);
};

const handlerValidator = (handler: CustomRouteHandler | null, field: string, value: any): string | null => {
    if (!handler) {
        return null;
    }

    const nameRequiredValidator = {
        field: 'name',
        validate: (h: CustomRouteHandler, v: any) => {
            return (!v || v.trim().length === 0) ? 'Route name is required.' : null;
        }
    };

    const nameUniqueValidator = {
        field: 'name',
        validate: (h: CustomRouteHandler, v: any) => {
            const duplicate = handlerListRef.value.find((existingHandler) => {
                return existingHandler.name === v && existingHandler.method === h.method && existingHandler !== h;
            });
            
            return duplicate ? 'Route name must be unique.' : null;
        }
    };

    const pathRequiredValidator = {
        field: 'path',
        validate: (h: CustomRouteHandler, v: any) => {
            return (!v || v.trim().length === 0) ? 'Path is required.' : null;
        }
    };

    const pathUniqueValidator = {
        field: 'path',
        validate: (h: CustomRouteHandler, v: any) => {
            const duplicate = handlerListRef.value.find((existingHandler) => {
                return existingHandler.path === v && existingHandler.method === h.method && existingHandler !== h;
            });

            return duplicate ? 'Path must be unique.' : null;
        }
    };

    const bodyRequiredValidator = {
        field: 'body',
        validate: (h: CustomRouteHandler, v: any) => {
            return (!v || v.trim().length === 0) ? 'Response body is required.' : null;
        }
    };
    
    const validators = [
        nameRequiredValidator,
        nameUniqueValidator,
        pathRequiredValidator,
        pathUniqueValidator,
        bodyRequiredValidator
    ].filter(v => v.field === field);

    for (const v of validators) {
        const errorMessage = v.validate(handler, value);
        if (errorMessage) {
            return errorMessage;
        }
    }

    return null;
};

const addNewRoute = () => {
    handlerRef.value = null;
    handlerDialogRef.value?.clear();
    handlerDialogRef.value?.open();
};

const edit = (handler: CustomRouteHandler) => {
    handlerRef.value = handler;
    handlerDialogRef.value?.open();
};

const remove = (handler: CustomRouteHandler) => {
    const confirmed = confirm(`Are you sure you want to remove the route handler "${handler.name}"?`);
    if (confirmed) {
        handlerListRef.value = handlerListRef.value.filter(h => h !== handler);
        props.onHandlersChanged?.(handlerListRef.value);
    }
};

</script>
<style scoped>
    .add-new-handler-container {
        padding: 5px;
        text-align: right;
    }
    .no-handlers-message {
        color: navy;
        font-style: italic;
        text-align: center;
    }
    table {
        width: 100%;
        margin-top: 10px;
    }
    th {
        background-color: #a9a9a9;
    }
    td {
        text-align: center;
        background-color: rgb(248, 248, 248);
    }
</style>