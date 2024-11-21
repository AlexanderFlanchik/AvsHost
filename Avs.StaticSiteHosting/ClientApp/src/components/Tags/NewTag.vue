<script setup lang="ts">
import { Subject } from 'rxjs/internal/Subject';
import { inject, reactive } from 'vue';
import { API_CLIENT } from '../../common/diKeys';

const props = defineProps<{ tagCreated$: Subject<any> }>();

interface NewTagModel {
    name: string | null;
    textColor: string;
    backgroundColor: string;
    error: string | null;
}

const model = reactive<NewTagModel>({
    name: '',
    textColor: '#FFFFFF',
    backgroundColor: '#000000',
    error: null
});

const resetForm = () => {
    model.name = null;
    model.textColor = '#FFFFFF';
    model.backgroundColor = '#000000';
};

const apiClient = inject(API_CLIENT)!;

const onTagNameChanged = () => {
    model.error = null;
    apiClient.getAsync(`api/tagvalidation/check-new-tag?tagName=${model.name}`)
        .then((response: any) => {
                if (response.data) {
                    model.error = `A tag with name '${model.name}' already exists.`;
                }
        }).catch(() => model.error = 'Unable to validate tag data because of server error.');
};

const createTag = async () => {
    if (!model.name) {
            model.error = 'Name field is required.';
            return;
    }

    await apiClient.postAsync('api/tags', {
        name: model.name,
        textColor: model.textColor,
        backgroundColor: model.backgroundColor
    });

    resetForm();
    props.tagCreated$?.next(true);
};

</script>
<template>
  <fieldset>
        <legend>Add New Tag</legend>
        <div>Tag name:</div>
        <div class="tag-input-container">
            <input type="text" v-model="model.name" @change="onTagNameChanged" />
            <span class="validation-error" v-if="model.error">{{model.error}}</span>
        </div>
        <div>Background color:</div>
        <div class="tag-input-container">
            <input type="color" v-model="model.backgroundColor" />
        </div>
        <div>Text color:</div>
        <div class="tag-input-container">
            <input type="color" v-model="model.textColor" />
        </div>
        <div class="tag-input-container">
            <button class="btn btn-primary" :disabled="model.error && model.error.length > 0 || !model.name || !model.name.length" @click="createTag">Create</button>
        </div>
    </fieldset>
</template>
<style scoped>
    fieldset {
         width: 450px;
         margin-bottom: 2px;
         margin-top: 2px;
    }
    .tag-input-container {
        padding-bottom: 5px;
    }
 
    input[type="color"] {
        width: 100%;
        height: 42px;
        cursor: pointer;
    }
</style>