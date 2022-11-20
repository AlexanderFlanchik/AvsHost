<template>
    <div>
        <ul class="tag-list" v-if="tags && tags.length">
            <li v-for="tag in tags" :key="tag.id">
                <span>{{tag.name}}</span>
            </li>
        </ul>
        <div class="no-tags-message">
            No tags found. Use a form on the right to add a tag.
        </div>
    </div>
</template>
<script>
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
        }
    }
</script>
<style scoped>
    .tag-list {
        list-style-type: none;
        padding-left: 2px;
    }
    .no-tags-message {
        padding-top: 15px;
        color: navy;
        font-weight: bold;
    }
</style>