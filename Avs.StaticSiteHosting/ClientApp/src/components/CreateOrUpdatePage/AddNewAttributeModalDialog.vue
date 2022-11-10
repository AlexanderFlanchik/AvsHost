<template>
    <b-modal ref="add-attribute-modal" hide-footer title="Add New Attribute">
           <div v-if="error">
               <span class="validation-error">{{error}}</span> 
            </div>
            <div>
                <span>Name:</span> <br />
                <b-form-input v-model="name" @change="() => this.error = null"></b-form-input>
            </div>
            <div>
                <span>Value:</span> <br />
                <b-form-input v-model="value" @change="() => this.error = null"></b-form-input>
            </div>
            <div class="modal-btn-holder">
                <button class="btn btn-primary" @click="() => this.ok()">OK</button>
                <button class="btn btn-default" @click="() => this.$refs['add-attribute-modal'].hide()">Cancel</button>
            </div>
        </b-modal>
</template>
<script>
    export default {
        props: {
            attributes: Object
        },

        data: function() {
            return {
                error: null,
                name: '',
                value: ''
            };
        },

        methods: {
            ok: function() {
                let name = this.name;
                if (!name) {
                    this.error = "'Name' field is required.";
                    return;   
                }

                let existingAttribute = this.attributes.find(a => a.name == name);
                if (existingAttribute) {
                    this.error = `The attribute with name '${name}' already exists.`;
                    return;
                }

                let newAttribute = { name: name, value: this.value };
                this.attributes.push(newAttribute);
                            
                this.clear();

                this.$refs['add-attribute-modal'].hide();
            },

            open: function() {
                this.clear();
                this.$refs['add-attribute-modal'].show();
            },

            clear: function() {
                this.name = null;
                this.value = null;
                this.error = null;
            }
        }
    }
</script>