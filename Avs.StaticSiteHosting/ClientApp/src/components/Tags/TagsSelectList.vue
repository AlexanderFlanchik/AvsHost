<script setup lang="ts">
import { computed, inject, onMounted, reactive } from 'vue';
import Tag, { TagData } from './Tag.vue';
import { API_CLIENT } from '../../common/diKeys';

interface TagsSelectListProps {
    tagIds: Array<string>;
    onTagsChanged: (selected: any) => void;
    hideSelectedTags?: boolean;
}

interface TagsSelectListModel {
    tagRemoving: boolean;
    showOptions: boolean;
    allTags: TagData[];
}

const props = defineProps<TagsSelectListProps>();
const model = reactive<TagsSelectListModel>({
    tagRemoving: false,
    showOptions: false,
    allTags: []
});

const apiClient = inject(API_CLIENT)!;

onMounted(async () => {
    const allTagsResponse: any = await apiClient.getAsync("api/tags");
    model.allTags = allTagsResponse.data || [];
});

const openOptions = () => {
    if (!model.tagRemoving) {
        model.showOptions = true;
    } else {
        model.tagRemoving = false;
    }
};
const selected = computed(() => {
    if (!props.tagIds?.length) {
        return [];
    }

    return model.allTags.filter((t: any) => props.tagIds.indexOf(t.id) >= 0);
});

const clear = () => {
    if (!props.tagIds?.length) {
        return;
    }

    let t = props.tagIds.pop();
    while (t) {
        t = props.tagIds.pop();
    }

   props.onTagsChanged(selected);
};

const removeTag = (tag: TagData) => {
    model.tagRemoving = true;
    const foundTag = selected.value.find((t: TagData) => t.id == tag.id);
    if (foundTag) {
        const tagIdx = selected.value.indexOf(foundTag);
        selected.value.splice(tagIdx, 1);
        props.onTagsChanged(selected);
    }
};

const addOrRemoveTag = (tag: TagData) => {
    const foundTag = selected.value.find((t: TagData) => t.id == tag.id);
    if (!foundTag) {
        selected.value.push(tag);
        props.onTagsChanged(selected);
    } else {
        removeTag(tag);
    }
};

const tagSelected = (tag: TagData) => {
    const isSelected = selected.value.indexOf(tag) >= 0;
    return {
        "selected-tag": isSelected
    };
};

defineExpose({ openOptions });
</script>
<template>
<div class="tag-list-container">
        <div @click="() => openOptions()">
            <div class="no-selected-tags" v-if="!selected.length && !hideSelectedTags">-- Click here to add --</div>
            <ul class="selected-tags-container" v-if="selected.length && !hideSelectedTags">
                <li v-for="tag of selected" :key="tag.id">
                    <Tag :tagData="tag" />
                    <a href="#" @click="removeTag(tag)">X</a>
                </li>
            </ul>
        </div>
        <div v-if="model.showOptions" class="tag-options-container">
           <div class="options-list-container" v-if="model.allTags.length">
                <ul class="options-list">
                    <li v-for="tag of model.allTags" :key="tag.id" @click="addOrRemoveTag(tag)" v-bind:class="tagSelected(tag)">
                        <Tag :tagData="tag" />
                    </li>
                </ul>
            </div>
            <div class="btn-bar" v-if="model.allTags.length">
                <button class="btn btn-primary clear-selected-tags-btn" @click="clear" v-if="selected.length">Clear</button>
                <button class="btn btn-primary" @click="model.showOptions = false">OK</button>
            </div>
            <div v-if="!model.allTags.length">
                <span class="no-tags-message">--No tags found.</span>
            </div>
            <div v-if="!model.allTags.length" class="btn-bar">
                <button class="btn btn-primary" @click="model.showOptions = false">OK</button>
            </div>
        </div>
    </div>
</template>
<style scoped>
    .no-selected-tags {
        color: navy;
        font-style: italic;
        cursor: pointer;
        margin-top: 10px;
   }
   .tag-list-container {
        position: relative;
   }
   .tag-options-container {
        background-color: white;
        border: 1px solid darkblue;
        border-radius: 5px;
        padding-right: 2px;
        width: 100%;
        top: 45px;
        position: absolute;
        z-index: 1000;
   }
   .no-tags-message {
        color:navy;
        font-weight: bold;
    }
   .options-list {
        list-style-type: none;
        padding-top: 2px;
        padding-left: 2px;
        padding-right: 2px;
        padding-bottom: 3px;
   }
   .options-list-container {
      max-height: 400px;
      overflow-y: auto;
   }
   .options-list li {
      cursor: pointer;
      margin-bottom: 2px;
    }
    .selected-tags-container {
        width: 100%;
        cursor: pointer;
        list-style-type: none;
        padding-top: 3px;
        padding-bottom: 3px;
        min-height: 45px;
        padding-left: 0px;
    }
    .selected-tags-container li {
        padding-left: 5px;
        margin-bottom: 2px;
        display: flex;
    }
    .selected-tags-container li a {
        margin-left: 5px;
        margin-top: 8px;
    }
    .btn-bar {
        border-top: 1px solid darkblue;
        text-align: right;
        margin-top: 2px;
        padding-top: 2px;
        padding-right: 2px;
        padding-bottom: 2px;
    }

    .clear-selected-tags-btn {
        margin-right: 2px;
    }

    .selected-tag {
        background-color: darkgrey;
    }
</style>