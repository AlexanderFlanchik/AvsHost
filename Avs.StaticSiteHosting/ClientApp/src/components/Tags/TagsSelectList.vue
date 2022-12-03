<template>
    <div class="tag-list-container">
        <div @click="() => openOptions()">
            <ul class="selected-tags-container">
                <li v-for="tag of selected" :key="tag.id">
                    <Tag :tagData="tag" />
                    <a href="#" @click="removeTag(tag)">X</a>
                </li>
            </ul>
        </div>
        <div v-if="showOptions" class="tag-options-container">
           <div class="options-list-container" v-if="allTags.length">
                <ul class="options-list">
                    <li v-for="tag of allTags" :key="tag.id" @click="addTag(tag)" v-bind:class="tagSelected(tag)">
                        <Tag :tagData="tag" />
                    </li>
                </ul>
            </div>
            <div class="btn-bar" v-if="allTags.length">
                <button class="btn btn-primary" @click="showOptions = false">OK</button>
            </div>
            <div v-if="!allTags.length">
                <span class="no-tags-message">--No tags found.</span>
            </div>
        </div>
    </div>
</template>
<script>
    import Tag from './Tag.vue';
   
    export default {
        props: {
            tagIds: Object,
            onTagsChanged: Object
        },
        data: function () {
            return {
                tagRemoving: false,
                showOptions: false,
                allTags: [],
            };
        },
        mounted: async function () {
            let allTagsResponse = await this.$apiClient.getAsync("api/tags");
            this.allTags = allTagsResponse.data || [];
        },
        methods: {
            openOptions: function() {
                if (!this.tagRemoving) {
                    this.showOptions = true;
                } else {
                    this.tagRemoving = false;
                }
            },
            addTag: function(tag) {
                let foundTag = this.selected.find(t => t.id == tag.id);
                if (!foundTag) {
                    this.selected.push(tag);
                    this.onTagsChanged(this.selected);
                }
            },

            removeTag: function(tag) {
                this.tagRemoving = true;
                let foundTag = this.selected.find(t => t.id == tag.id);
                if (foundTag) {
                    let tagIdx = this.selected.indexOf(foundTag);
                    this.selected.splice(tagIdx, 1);
                    this.onTagsChanged(this.selected);
                }
            },
            tagSelected: function(tag) {
                let isSelected = this.selected.indexOf(tag) >= 0;
                return {
                    "selected-tag": isSelected
                };
            }
        },
        computed: {
            selected: function() {
                return this.allTags.filter(t => this.tagIds.indexOf(t.id) >= 0);
            }
        },
        components: {
            Tag
        }
    }
</script>
<style scoped>
   .tag-list-container {
        position: relative;
   }
   .tag-options-container {
        background-color: white;
        border: 1px solid darkblue;
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
        border: 1px solid darkblue;
        list-style-type: none;
        padding-top: 3px;
        padding-bottom: 3px;
        min-height: 45px;
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
        text-align: right;
        padding-right: 5px;
    }

    .selected-tag {
        background-color: darkgrey;
    }
</style>