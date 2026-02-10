<template>
    <div>
        <AddDatabaseDialog ref="addDatabaseDlgRef" :siteId="props.siteId" :onValueChanged="addDatabaseName"></AddDatabaseDialog>
        <span v-if="dbNameRef">
            {{ dbNameRef }}
            <a href="javascript:void(0)" class="remove-db-link" @click="removeDatabaseName">X</a>
        </span>
        <span v-if="!dbNameRef">
            <a href="javascript:void(0)" class="add-db-link" @click="openAddDatabaseDialog">-- Click here to add--</a>
        </span>
    </div>
</template>
<script setup lang="ts">
import { ref, watch } from 'vue';
import AddDatabaseDialog from './AddDatabaseDialog.vue';

interface DatabaseNameInputProps {
    databaseName: string | null;
    siteId: string | null | undefined;
}

const props = defineProps<DatabaseNameInputProps>();
const addDatabaseDlgRef = ref<typeof AddDatabaseDialog | null>(null);
const dbNameRef = ref<string | null>(props.databaseName || null);
const emit = defineEmits(['update:databaseName']);

watch(() => props.databaseName, (newVal) => dbNameRef.value = newVal);

const removeDatabaseName = () => {
     if(!confirm(`Are you sure to remove the database ${dbNameRef.value}?`)) {
        return;
     }
    
    dbNameRef.value = null;
    emit('update:databaseName', null);
};

const openAddDatabaseDialog = () => {
    addDatabaseDlgRef.value?.open();
};

const addDatabaseName = (newDatabaseName: string) => {
    dbNameRef.value = newDatabaseName;
    emit('update:databaseName', newDatabaseName);
}

</script>
<style scoped>
a {
    text-decoration: none;
    cursor: pointer;
}
.add-db-link {
    color: navy;
    font-style: italic;
 }
.remove-db-link {
    margin-left: 4px;
    color: red;
    font-weight: bold;
}
</style>