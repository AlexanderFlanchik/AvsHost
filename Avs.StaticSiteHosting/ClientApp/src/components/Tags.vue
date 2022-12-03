<template>
    <div class="content-block-container">
        <div class="general-page-title">
            <span>Tags</span>
        </div>
        <UserInfo />
        <NavigationMenu />
        <div class="tag-parent">
            <TagList ref="tag-list" class="tag-list-container" />
            <NewTag :tagCreated$="tagCreatedSubject" />
        </div>
    </div>
</template>
<script>
    import TagList from './Tags/TagList.vue';
    import NewTag from './Tags/NewTag.vue';
    import UserInfo from './UserInfo.vue';
    import NavigationMenu from './NavigationMenu.vue';
    import { Subject } from 'rxjs';

    export default {
        data: function () {
            return {
                tagCreatedSubject: new Subject()
            };
        },
        mounted: function () {
            this.tagCreatedSubject.subscribe(() => this.$refs["tag-list"].loadTags());
        },
        components: {
            TagList,
            NewTag,
            UserInfo,
            NavigationMenu
        }
    }
</script>
<style scoped>
    .tag-parent {
        display: flex;
        background-color: azure;
        height: calc(100vh - 195px);
    }
    .tag-list-container {
        min-width: 450px;
        padding-left: 5px;
        padding-top: 2px;
        height: 100%; 
        overflow-y: auto;
    }
</style>