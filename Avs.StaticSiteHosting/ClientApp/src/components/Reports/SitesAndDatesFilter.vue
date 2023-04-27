<template>
    <div class="site-dates-filters-container">
        <div class="dates-filter-container">
            <div>
                <span>Date from:</span> &nbsp;
                <date-picker v-model="dateFrom" :disabled-date="disabledDateFromAfter" valueType="format" :editable="false" @change="() => this.apply()"></date-picker>
            </div>
            <div>
                <span>Date to:</span> &nbsp;
                <date-picker v-model="dateTo" :disabled-date="disabledDateToBefore" valueType="format" :editable="false" @change="() => this.apply()"></date-picker>
            </div>
        </div>
        <div class="sites-filter-container">
            <div>Site(s):</div>
            <multiselect 
                v-model="sitesSelected" 
                :options="sites" 
                :multiple="true" 
                :searchable="searchable"
                @input="sitesChanged"
                @search-change="searchSites"
                placeholder="Start entering site name.."
                label="name"
                track-by="id">
            </multiselect>
        </div>
    </div>
</template>
<script lang="ts">
    import Multiselect from 'vue-multiselect';
    import DatePicker from 'vue2-datepicker';
    import 'vue2-datepicker/index.css';
    import reportFilterTypeNames from './ReportFilterTypes';
    
    const moment = require('moment');
    const defFormat = 'YYYY-MM-DD';

    const today = new Date();
    const yesterday = new Date(new Date().setDate(today.getDate() - 1));

    export default {
        props: {
            filterHasChanged$: Object
        },
        data: function () {
            return {
                sitesSelected: [],
                sites: [],
                dateFrom: moment(yesterday).format(defFormat),
                dateTo: moment(today).format(defFormat)
            };
        },
        mounted: function () {
            setTimeout(() => this.apply(), 10);
        },
        methods: {
            sitesChanged: function() {
                this.apply();
            },
            searchSites: function(query) {
                if (!query) {
                    return;
                }

                this.$apiClient.getAsync(`api/sitesearch/${query}`)
                    .then(response => this.sites = response.data.sites);
            },
            apply: function() {
                this.filterHasChanged$.next({
                    _filterType: reportFilterTypeNames.SitesAndDatesFilter,
                    siteIds: this.sitesSelected.map(s => s.id),
                    dateFrom: this.dateFrom,
                    dateTo: this.dateTo
                });
            }
        },
        components: {
            Multiselect,
            DatePicker
        }
    }
</script>
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
        align-items: baseline;
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