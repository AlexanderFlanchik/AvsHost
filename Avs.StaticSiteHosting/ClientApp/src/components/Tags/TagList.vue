<template>
    <div>
        <ul class="tag-list" v-if="tags && tags.length">
            <li v-for="tag in tags" :key="tag.id">
                <Tag :tagData="tag"/>
                <div class="delete-link-container">
                    <a href="javascript:void(0)" @click="deleteTag(tag.id)">X</a>
                </div>
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
            },

            deleteTag: async function(tagId) {
                let response = await this.$apiClient.getAsync(`api/tagvalidation/check-tag-use?tagId=${tagId}`);
                if (response.data && !confirm('This tag is used in some sites. Do you confirm to delete it?')) {
                    return;
                }

                await this.$apiClient.deleteAsync(`api/tags/${tagId}`);
                let idx = this.tags.findIndex(t => t.id == tagId);
                this.tags.splice(idx, 1);
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