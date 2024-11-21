<script setup lang="ts">
    import { ref } from 'vue';
    import ResourceMappingDialog from './ResourceMappingDialog.vue';
    const newResourceMappingDlgRef = ref<typeof ResourceMappingDialog | null>(null);

    const props = defineProps<{ resourceMappings: Array<any> }>();
    let newMappingSubject: any;

    const addResourceMapping = () => {
        newMappingSubject?.unsubscribe();
        newMappingSubject = newResourceMappingDlgRef.value?.open(props.resourceMappings);
        newMappingSubject.subscribe((mapping: any) => props.resourceMappings?.push(mapping));
    };

    const removeResourceMapping = (mapping: any) => {
        const ix = props.resourceMappings.indexOf(mapping);
        props.resourceMappings.splice(ix, 1);
    };
</script>
<template>
    <div>
        <ResourceMappingDialog ref="newResourceMappingDlgRef" />
        <div class="resource-mapping-title-container">
            <div class="resource-mapping-container-left">
                <span class="form-title">Resource mappings</span>
            </div>
            <div class="resource-mapping-container-right">
                <a href="javascript:void(0)" @click="addResourceMapping">Add new...</a>
            </div>
        </div>
        <div class="resource-mapping-details">
            You can use resource mapping to rename your content files in URLs. For example,
            you can create &quot;About&quot; mapping which will point to &quot;About.html&quot; resource.
        </div>
        <table class="table table-striped rm-table" v-if="props.resourceMappings?.length > 0">
            <thead>
                <tr>
                    <th>Name</th>
                    <th>Value</th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="rm in props.resourceMappings" :key="rm.name">
                    <td>{{rm.name}}</td>
                    <td>{{rm.value}} &nbsp;<a href="javascript:void(0)" class="delete-link" title="Delete" @click="removeResourceMapping(rm)">X</a></td>
                </tr>
            </tbody>
        </table>
        <div class="no-mappings-message" v-if="props.resourceMappings?.length === 0">
            No resource mappings yet.
        </div>
    </div>
</template>
<style scoped>
    .resource-mapping-title-container {
        width: 100%;
        display: flex;
        margin-top: 20px;
    }

    .resource-mapping-container-left {
        width: 50%;
    }

    .resource-mapping-container-right {
        width: 50%;
        text-align: right;
    }
    .delete-link {
        color: red;
        font-weight: bold;
        text-decoration: none;
    }
    .resource-mapping-details {
        margin-top: 10px;
        font-size: smaller;
        padding-left: 5px;
        padding-right: 5px;
    }
    .no-mappings-message {
        text-align: center;
        margin-top: 25px;
        color: navy;
        font-weight: bold;
    }
    .rm-table {
        width: 100%;
        margin-left: 5px;
        margin-right: 5px;
        margin-top: 10px;
    }
    
    .rm-table th {
        text-align: left;
    }
</style>