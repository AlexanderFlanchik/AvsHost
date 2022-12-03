<template>
    <div>
        <ul class="tag-list" v-if="tags && tags.length">
            <li v-for="tag in tags" :key="tag.id">
                <Tag :tagData="tag"/>
            </li>
        </ul>
        <div class="no-tags-message" v-if="!tags || !tags.length">
            No tags found. Use a form on the right to add a tag.
        </div>
    </div>
</template>
<script>
    import Tag from './Tag.vue';
    
    export default {
        data: function () {
            return {
                tags: []
            };
        },
        mounted: function () {
            this.loadTags();
        },
        methods: {
            loadTags: function () {
                this.$apiClient.getAsync('api/tags')
                    .then(response => this.tags = response.data)
                    .catch(err => console.log(err));
            }
        },
        components: {
            Tag
        }
    }
</script>
<style scoped>
    .tag-list {
        list-style-type: none;
        padding-left: 2px;
    }
    .tag-list li {
        padding-bottom: 5px;
    }
    .no-tags-message {
        padding-top: 15px;
        color: navy;
        font-weight: bold;
    }
</style>