<template>
    <fieldset>
        <legend>Add New Tag</legend>
        <div>Tag name:</div>
        <div class="tag-input-container">
            <b-form-input type="text" v-model="name" @change="onTagNameChanged"></b-form-input>
            <span class="validation-error" v-if="error">{{error}}</span>
        </div>
        <div>Background color:</div>
        <div class="tag-input-container">
            <input type="color" v-model="backgroundColor" />
        </div>
        <div>Text color:</div>
        <div class="tag-input-container">
            <input type="color" v-model="textColor" />
        </div>
        <div class="tag-input-container">
            <button class="btn btn-primary" :disabled="error && error.length || !name || !name.length" @click="createTag">Create</button>
        </div>
    </fieldset>
</template>
<script>
    export default {
        props: {
            tagCreated$: Object
        },

        data: function () {
            return {
                name: '',
                textColor: '#FFFFFF',
                backgroundColor: '#000000',
                error: null
            };
        },
        methods: {
            resetForm: function () {
                this.name = null;
                this.textColor = '#FFFFFF';
                this.backgroundColor = '#000000';
            },

            onTagNameChanged: function () {
                this.error = null;
                this.$apiClient.getAsync(`api/tagvalidation/check-new-tag?tagName=${this.name}`)
                    .then(response => {
                        if (response.data) {
                            this.error = `A tag with name '${this.name}' already exists.`;
                        }
                    }).catch(() => this.error = 'Unable to validate tag data because of server error.');
            },

            createTag: async function () {
                if (!this.name) {
                    this.error = 'Name field is required.';
                    return;
                }

                await this.$apiClient.postAsync('api/tags', {
                    name: this.name,
                    textColor: this.textColor,
                    backgroundColor: this.backgroundColor
                });

                this.resetForm();
                this.tagCreated$?.next(true);
            }
        }
    }
</script>
<style scoped>
 fieldset {
     width: 450px;
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