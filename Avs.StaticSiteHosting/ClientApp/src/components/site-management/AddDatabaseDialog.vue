<template>
  <ModalDialog ref="addDatabaseDlgRef" title="Add Database" :ok="onOk" dialog-width="300px">
    <div>
      <div v-if="validationFailed" class="error-message">
        <ul>
          <li class="error" v-for="(error, index) in errorRef" :key="index">{{ error }}</li>
        </ul>
      </div>
      <div class="database-dlg-inner" v-if="areOtherDatabases">
        <div class="new-database-inner">
          <div>
            <input type="radio" :checked="isNew" @click="isNewSetter(true)" id="new-database"/>
            <label for="new-database">New database:</label>          
          </div>
          <div class="database-name-container">
            <input type="text" class="new-database-name" ref="dbNameInputRef" :disabled="!isNew" @change="inputChange" />
          </div>
        </div>        
        <div class="existing-database-inner">
          <div>
            <input type="radio" :checked="!isNew" @click="isNewSetter(false)" id="existing-database"/>
            <label for="existing-database">Existing database:</label>
          </div>
          <div class="database-name-container">
            <select class="database-list" :disabled="isNew" @change="selectChange">
              <option v-for="db in otherDbs" :key="db" :value="db">{{ db }}</option>
            </select>
          </div>
        </div>
      </div>
      <div v-else class="database-dlg-inner">
        <div class="new-database-caption">
          <span>New database:</span>
        </div>
        <div class="new-database-input-holder">
          <input type="text" @change="inputChange" />
        </div>
      </div>
    </div>
  </ModalDialog>
</template>
<script setup lang="ts">
import { computed, inject, onMounted, ref } from 'vue';
import ModalDialog from '../ModalDialog.vue';
import { API_CLIENT } from '../../common/diKeys';

interface AddDatabaseDialogProps {
    siteId: string | null | undefined;    
    onValueChanged: (newDatabaseName: string) => void;
}

const apiClient = inject(API_CLIENT)!;
const props = defineProps<AddDatabaseDialogProps>();
const addDatabaseDlgRef = ref<typeof ModalDialog | null>(null);
const dbNameInputRef = ref<HTMLInputElement | null>(null);

const isNew = ref<boolean>(true);
const isNewSetter = (value: boolean) => { 
  isNew.value = value;
  if (!isNew.value) {
    databaseNameRef.value = otherDbs.value.length > 0 ? otherDbs.value[0] : null;
    dbNameInputRef.value!.value = '';
  }
};

const databaseNameRef = ref<string | null>(null);
const otherDbs = ref<string[]>([]);
const errorRef = ref<string[]>([]);

const areOtherDatabases = computed(() => {
    return otherDbs.value.length > 0;
});

const inputChange = (event: Event) => {
    const target = event.target as HTMLInputElement;
    databaseNameRef.value = target.value;
};

const validationFailed = computed(() => {
    return errorRef.value.length > 0;
});

const selectChange = (event: Event) => {
    const target = event.target as HTMLSelectElement;
    databaseNameRef.value = target.value;
};

const validateDatabaseName = async () => {
  errorRef.value = [];
  if (!databaseNameRef.value || !databaseNameRef.value.length) {
    errorRef.value.push('Database name cannot be empty.');
  }

  if (!isNew.value) {
    return;
  }

  const response = await apiClient.getAsync(`api/database/validate/${databaseNameRef.value!}`) as any;
  if (response.data.isValid) {
    return;
  }

  errorRef.value.push("Database name is already in use. Please choose a different name.");
};

onMounted(async () => {
  const response = await apiClient.getAsync(`api/database`) as any;
  otherDbs.value = response.data as Array<string>;
});

const open = () => {
    isNew.value = true;
    errorRef.value = [];
    addDatabaseDlgRef.value?.open();
}

const onOk = async () => {
  await validateDatabaseName();
  if (errorRef.value.length) {
    return;
  }

  props.onValueChanged(databaseNameRef.value!);
  addDatabaseDlgRef.value?.close();
};

defineExpose({ open });

</script>
<style scoped>
.database-dlg-inner {
    display: flex;
    flex-direction: column;
}
.error-message {    
    margin-bottom: 8px;
}

.error-message ul {
  list-style-type: none;
  padding-left: 1px;  
}

.error {
    color: red;
    font-weight: bold;
}
.database-name-container {
  padding-left: 5px;
  padding-right: 5px;
}
.database-dlg-inner {
  padding: 5px;
  display: flex;
  flex-direction: column;
  gap: 5px;
}

.new-database-name {
  width: calc(100% - 10px);
}

.database-list {
  width: 100%;
}
</style>