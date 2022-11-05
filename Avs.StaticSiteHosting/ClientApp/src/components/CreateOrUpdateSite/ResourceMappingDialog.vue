<template>
    <b-modal ref="resource-mapping-dlg" hide-footer title="New Resource Mapping">
        <div>
            <table class="site-form">
                <tr>
                    <td>Name:</td>
                    <td>
                        <b-form-input type="text" maxlength="30" v-model="name" @change="() => nameErrorMessage = null"></b-form-input> 
                    </td>
                </tr>
                <tr v-if="nameErrorMessage">
                    <td colspan="2">
                        <span class="validation-error">{{nameErrorMessage}}</span> 
                    </td>
                </tr>
                <tr>
                    <td>Value:</td>
                    <td>
                        <b-form-input type="text" maxlength="30" v-model="value" @change="valueErrorMessage = null"></b-form-input>
                        
                    </td>
                </tr>
                <tr v-if="valueErrorMessage">
                    <td colspan="2">
                        <span class="validation-error">{{valueErrorMessage}}</span>
                    </td>
                </tr>
            </table>
        </div>
        <button class="btn btn-primary" @click="addResourceMappingApply" :disabled="nameError">Add</button>
        <button class="btn btn-default" @click="newResourceMappingCancel">Cancel</button>
    </b-modal>
</template>
<script>
    import { Subject } from 'rxjs';

    export default {
        data: function() {
            return {
                resultSubject: new Subject(),
                name: '',
                value: '',
                nameErrorMessage: null,
                valueErrorMessage: null,
                resourceMappings: [],
            };
        },
        
        methods: {
            addResourceMappingApply: function() {
                let valid = true;
                if (!this.name) {
                    valid = false;
                    this.nameErrorMessage = "The name field is required.";
                }

                if (!this.value) {
                    valid = false;
                    this.valueErrorMessage = "The value field is required.";
                }

                if (this.resourceMappings.find(m => m.name == name)) {
                    valid = false;
                    this.nameErrorMessage = "This mapping already exists.";
                }

                if (!valid) {
                    return;
                }

                this.resultSubject.next({ name: this.name, value: this.value });
                this.$refs["resource-mapping-dlg"].hide();
            },

            newResourceMappingCancel: function() {
                this.clear();
                this.$refs["resource-mapping-dlg"].hide();
            },

            open: function(resourceMappings) {
                this.clear();
                this.resourceMappings = resourceMappings;
                this.$refs["resource-mapping-dlg"].show();
                
                return this.resultSubject;
            },

            clear: function() {
                this.name = null;
                this.value = null;
                this.nameErrorMessage = null;
                this.valueErrorMessage = null;
            }
        }
    }
</script>