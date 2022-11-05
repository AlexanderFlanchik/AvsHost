<template>
    <div>
        <ResourceMappingDialog ref="new-resource-mapping-dlg" />
        <div class="resource-mapping-title-container">
            <div class="resource-mapping-container-left">
                <span class="form-title">Resource mappings</span>
            </div>
            <div class="resource-mapping-container-right">
                <a href="#" @click="addResourceMapping">Add new...</a>
            </div>
        </div>
        <table class="table resource-mapping-header">
            <thead>
                <tr>
                    <th>Name</th>
                    <th>Value</th>
                </tr>
            </thead>
        </table>
        <table class="table table-striped" v-if="resourceMappings.length > 0">
            <tbody>
                <tr v-for="rm in resourceMappings" :key="rm">
                    <td>{{rm.name}}</td>
                    <td>{{rm.value}} &nbsp;<a href="#" class="delete-link" title="Delete" @click="removeResourceMapping(rm)">X</a></td>
                </tr>
            </tbody>
        </table>
        <div class="no-mappings-message" v-if="resourceMappings.length === 0">
            No resource mappings yet.
        </div>
    </div>
</template>
<script>
    import ResourceMappingDialog from './ResourceMappingDialog.vue';

    export default {
        props: {
            resourceMappings: Object
        },

        methods: {
            addResourceMapping: function() {
                let newMappingSubject = this.$refs["new-resource-mapping-dlg"].open(this.resourceMappings);
                newMappingSubject.subscribe(mapping => this.resourceMappings.push(mapping));
            },

            removeResourceMapping: function(mapping) {
                if (confirm(`Are you sure to delete '${mapping.name}' mapping?`)) {
                    let ix = this.resourceMappings.indexOf(mapping);
                    this.resourceMappings.splice(ix, 1);
                }
            }
        },
        components: {
            ResourceMappingDialog
        }
    }
</script>
<style scoped>
    .resource-mapping-title-container {
        width: 100%;
    }

    .resource-mapping-container-left {
        width: 50%;
        float: left;
    }

    .resource-mapping-container-right {
        width: 50%;
        float: left;
        text-align: right;
    }
    .delete-link {
        color: red;
        font-weight: bold;
        text-decoration: none;
    }
    .no-mappings-message {
        text-align: center;
        padding-top: 10px;
        color: navy;
        font-weight: bold;
    }
</style>