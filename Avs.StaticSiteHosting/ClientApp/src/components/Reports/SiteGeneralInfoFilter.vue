<script setup lang="ts">
import { Subject } from 'rxjs';
import reportFilterTypeNames from './ReportFilterTypes';
import { onMounted } from 'vue';
    
const props = defineProps<{ filterHasChanged$: Subject<any> }>();
onMounted(() => {
    setTimeout(() => props.filterHasChanged$?.next({ _filterType: reportFilterTypeNames.SiteGeneralInfoFilter }), 10);
});

</script>
<template>
    <div class="no-op"></div>
</template>
<style scoped>
    .no-op {
        display: none;
    }
</style>