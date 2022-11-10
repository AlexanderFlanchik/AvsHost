<template>
    <b-modal ref="add-css-class-modal" hide-footer title="Add CSS Class">
        <div v-if="error">
            <span class="validation-error">{{error}}</span> 
        </div>
        <div>
            <span>Name:</span> <br />
            <b-form-input v-model="name" @change="() => this.error = null"></b-form-input>
        </div>
        <div class="modal-btn-holder">
            <button class="btn btn-primary" @click="() => this.ok()">OK</button>
            <button class="btn btn-default" @click="() => this.$refs['add-css-class-modal'].hide()">Cancel</button>
        </div>
    </b-modal>
</template>
<script>
    export default {
        props: {
            cssClasses: Object
        },

        data: function() {
            return {
                error: null,
                name: '',
            };
        },

        methods: {
            ok: function() {
                let name = this.name;
                if (!name) {
                    this.error = "'Name' field is required.";
                    return;   
                }

                let names = name.split(' ');
                for (let nm of names) {
                    let cssClass = this.cssClasses.find(c => c == nm);
                    if (!cssClass) {
                        this.cssClasses.push(nm);
                    }
                }

                this.clear();

                this.$refs['add-css-class-modal'].hide();
            },

            open: function() {
                this.clear();
                this.$refs['add-css-class-modal'].show();
            },

            clear: function() {
                this.name = null;
                this.error = null;
            }
        }
    }
</script>