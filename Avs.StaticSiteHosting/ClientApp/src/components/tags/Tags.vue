<script setup lang="ts">
 import { reactive, ref, onMounted } from 'vue';
 import { Subject } from 'rxjs';
 import TagList from './TagList.vue';
 import NewTag from './NewTag.vue';
 import NavigationMenu from './../layout/NavigationMenu.vue';
 import UserInfo from './../layout/UserInfo.vue';

 const model = reactive<{ tagCreatedSubject: Subject<any> }>({
    tagCreatedSubject: new Subject<any>()
 });

 const tagListRef = ref<typeof TagList | null>(null);
   
 onMounted(() => model.tagCreatedSubject.subscribe(() => tagListRef.value?.loadTags()));
</script>
<template>
    <div class="content-block-container">
        <div class="general-page-title">
            <span>Tags</span>
        </div>
        <UserInfo />
        <NavigationMenu />
        <div class="tag-parent">
            <TagList ref="tagListRef" class="tag-list-container" />
            <NewTag :tagCreated$="model.tagCreatedSubject as Subject<any>" />
        </div>
    </div>
</template>
<style scoped>
    .tag-parent {
        display: flex;
        background-color: azure;
        height: calc(100vh - 167px);
    }
    .tag-list-container {
        min-width: 450px;
        padding-left: 5px;
        padding-top: 2px;
        height: calc(100% - 5px); 
        overflow-y: auto;
    }
</style>