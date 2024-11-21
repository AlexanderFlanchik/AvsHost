<script setup lang="ts">
 import { inject, onMounted, reactive } from 'vue';
 import Tag, { TagData } from './Tag.vue';
 import { API_CLIENT } from '../../common/diKeys';
 
 interface TagListModel {
    tags: TagData[];
 }

 const model = reactive<TagListModel>({
    tags: []
 });

 const apiClient = inject(API_CLIENT)!;
 const loadTags = () => 
    apiClient.getAsync('api/tags')
        .then((response: any) => model.tags = response.data)
        .catch((e: Error) => console.log(e));

 onMounted(loadTags);
 
 defineExpose({ loadTags });
 
 const deleteTag = async(tagId: string) => {
    const response = await apiClient.getAsync(`api/tagvalidation/check-tag-use?tagId=${tagId}`) as any;
    if (response?.data && !confirm('This tag is used in some sites. Do you confirm to delete it?')) {
        return;
    }

    await apiClient.deleteAsync(`api/tags/${tagId}`);
    let idx = model.tags.findIndex((t: any) => t.id == tagId);
    model.tags.splice(idx, 1);
 };

</script>
<template>
    <div>
        <ul class="tag-list" v-if="model.tags && model.tags.length">
            <li v-for="tag in model.tags" :key="tag.id">
                <Tag :tagData="tag"/>
                <div class="delete-link-container">
                    <a href="javascript:void(0)" @click="deleteTag(tag.id)">X</a>
                </div>
            </li>
        </ul>
        <div class="no-tags-message" v-if="!model.tags || !model.tags.length">
            No tags found. Use a form on the right to add a tag.
        </div>
    </div>
</template>
<style scoped>
    .tag-list {
        list-style-type: none;
        padding-left: 2px;
    }
    .tag-list li {
        padding-bottom: 5px;
        display: flex;
    } 
    .delete-link-container {
        margin-left: 3px;
    }
    .delete-link-container a {
        padding-top: 5px;
        color: red;
        text-decoration: none;
        cursor: pointer;
        font-weight: bold;
    }
    .no-tags-message {
        padding-top: 15px;
        color: navy;
        font-weight: bold;
    }
</style>