<script setup lang="ts">
import Datepicker from 'vue3-datepicker/src/datepicker/Datepicker.vue';
import reportFilterTypeNames from './ReportFilterTypes';
import moment from 'moment';
import { Subject } from 'rxjs';
import { inject, onMounted, reactive, watch } from 'vue';
import { API_CLIENT } from '../../common/diKeys';
import '@vueform/multiselect/themes/default.css';
const defFormat = 'YYYY-MM-DD';

const today = new Date();
const yesterday = new Date(new Date().setDate(today.getDate() - 1));

interface SitesAndDatesFilterModel {
    sitesSelected: Array<any>;
    sites: Array<any>;
    dateFrom: Date;
    dateTo: Date;
}

const model = reactive<SitesAndDatesFilterModel>({
    sitesSelected: [],
    sites: [],
    dateFrom: yesterday,
    dateTo: today
});

const apiClient = inject(API_CLIENT)!;
const props = defineProps<{ filterHasChanged$: Subject<any> }>();

const apply = () => {
    props.filterHasChanged$.next({
        _filterType: reportFilterTypeNames.SitesAndDatesFilter,
        siteIds: model.sitesSelected.map((s: any) => s.id),
        dateFrom: moment(model.dateFrom).format(defFormat),
        dateTo: moment(model.dateTo).format(defFormat)
    });
};

const searchSites = (query: string | null) => {
    if (!query) {
        return;
    }

    apiClient.getAsync(`api/sitesearch/${query}`)
        .then((response: any) => {
            model.sites = response.data.sites;
        });
};

const dateFromDisabled = (target: Date) => {
    const today = new Date().setHours(0, 0, 0, 0);
    if (target > new Date(today)) {
        return true;
    }

    return model.dateTo ? target >= model.dateTo : false;
};

const dateToDisabled = (target: Date) => {
    return model.dateFrom ? target <= model.dateFrom : false;
};

watch(() => model.dateFrom, apply);
watch(() => model.dateTo, apply);
watch(() => model.sitesSelected, apply);

onMounted(() => {
    setTimeout(() => props.filterHasChanged$?.next({ 
        _filterType: reportFilterTypeNames.SitesAndDatesFilter, 
        dateFrom: moment(model.dateFrom).format(defFormat), 
        dateTo: moment(model.dateTo).format(defFormat), 
        siteIds: model.sitesSelected?.map(s => s.id) 
    }), 10);
});
</script>
<template>
    <div class="site-dates-filters-container">
        <div class="dates-filter-container">
            <div>
                <span>Date from:</span> &nbsp;
                <Datepicker v-model="model.dateFrom" :disabledDates="{ predicate: dateFromDisabled }" valueType="format" :editable="false" @change="() => apply()"></Datepicker>
            </div>
            <div>
                <span>Date to:</span> &nbsp;
                <Datepicker v-model="model.dateTo" :disabledDates="{ predicate: dateToDisabled }" valueType="format" :editable="false" @change="() => apply()"></Datepicker>
            </div>
        </div>
        <div class="sites-filter-container">
            <div>Site(s):</div>
            <v-select ref="siteNameSelect" 
                v-model="model.sitesSelected" label="name" 
                :clearable="true" 
                :filterable="true"
                style="min-width: 180px;"
                multiple 
                :options="model.sites" 
                @search="searchSites">
                    <template slot="no-options">
                        <div class="no-options-placeholder">Start enter a site name</div>
                    </template>

                    <template #option="option" slot="option" slot-scope="option">
                        <div>{{option.name}}</div>
                    </template>

                    <template #selectedOption="option" slot="selected-option" slot-scope="option">
                        <div class="option-selected">{{option.name}}</div>
                    </template>
            </v-select>
        </div>
    </div>
</template>
<style scoped>
    .sites-filter-container {        
        width: 550px;
        display: flex;
        align-items: center;
        gap: 5px;
    }
    .site-dates-filters-container {
        padding: 5px;
        margin-left: 5px;
        display: flex;
        gap: 25px;
        flex-wrap: wrap;
        width: 100%;
    }
    .dates-filter-container {
        margin-top: 5px;
        display: flex;
        max-width: 650px;
        justify-content: space-between;
        gap: 5px;
    }
</style>