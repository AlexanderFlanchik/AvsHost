<template>
    <b-modal ref="edit-content-dlg" hide-footer size="xl" :title="fileName">
        <div class="validation-error" v-if="error">{{error}}</div>
        <div class="content-centered">
            <b-form-textarea v-model="content" class="content-editor" @change="() => this.error = null"></b-form-textarea>
        </div>
        <div class="dlg-btn-container dlg-btn-container-right">
            <button class="btn btn-primary" @click="updateContent">OK</button>
            <button class="btn btn-default" @click="closeEditContent">Cancel</button>
        </div>
    </b-modal>
</template>
<script>
    import { Subject } from 'rxjs';

    export default {
        data: function() {
            return {
                resultSubject : new Subject(),
                fileName: '',
                content: '',
                error: null,
                contentValidator: null
            };
        },

        methods: {
            showDialog: function(fileName, content, contentValidator) {
                this.fileName = fileName;
                this.content = content;
                this.contentValidator = contentValidator;

                this.$refs["edit-content-dlg"].show();
                
                return this.resultSubject;
            },
            updateContent: function() {
                if (this.contentValidator) {
                    let validationError = this.contentValidator(this.content);
                    if (validationError) {
                        this.error = validationError;
                        return;
                    }
                }

                this.resultSubject.next(this.content);
                this.$refs["edit-content-dlg"].hide();
            },
            closeEditContent: function() {
                this.$refs["edit-content-dlg"].hide();
            }
        }
    }
</script>
<style scoped>
   .dlg-btn-container-right {
        text-align: right;
    }
    .content-editor {
        min-height: 570px;
    }
</style>