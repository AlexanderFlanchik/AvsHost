<script setup lang="ts">
import { Subject } from 'rxjs';
import { inject, onMounted, reactive, watch } from 'vue';
import reportFilterTypeNames from './ReportFilterTypes';
import { API_CLIENT } from '../../common/diKeys';

const apiClient = inject(API_CLIENT)!;
const props = defineProps<{ filterHasChanged$: Subject<any> }>();
const model = reactive<{ site: any; siteOptions: Array<any> }>({
    site: null,
    siteOptions: []
});

watch(() => model.site, () => props.filterHasChanged$.next(
    { 
        _filterType: reportFilterTypeNames.SiteEventsFilter, 
        siteId: model.site.id 
    })
);

const onSearch = (search: string, loading: (status: boolean) => void) => {
    if (search && search.length) {
        loading(true);
        apiClient.getAsync(`api/sitesearch/${search}`)
            .then((response: any) => {
                loading(false);
                model.siteOptions = response.data.sites;
            }).catch((e: Error) => {
                console.log(e);
                loading(false);
            });
    }
};

onMounted(() => {
    setTimeout(() => props.filterHasChanged$?.next({ _filterType: reportFilterTypeNames.SiteEventsFilter }), 10);
});

</script>
<template>
    <div class="site-filter-container">
        <span>Select a site:</span>
        <v-select ref="siteNameSelect" 
                  v-model="model.site" label="name" 
                  :clearable="false" 
                  :filterable="false" 
                  :options="model.siteOptions" 
                  @search="onSearch">
            <template #no-options="{}">
                <div class="no-options-placeholder">Start enter a site name</div>
            </template>

            <template #option="{ name }">
                <div>{{name}}</div>
            </template>

            <template #selected-option="{ name }">
                <div class="option-selected">{{name}}</div>
            </template>
        </v-select>
    </div>
</template>
<style scoped>
    .site-filter-container {
        max-width: 650px;
        padding: 5px;
    }
</style>