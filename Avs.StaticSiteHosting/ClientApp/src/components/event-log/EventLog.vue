<script setup lang="ts">
import { inject, onMounted, reactive, watch } from 'vue';
import UserInfo from '../layout/UserInfo.vue';
import NavigationMenu from '../layout/NavigationMenu.vue';
import Datepicker from 'vue3-datepicker';
import "vue-select/dist/vue-select.css";
import { API_CLIENT } from '../../common/diKeys';
import { formatDate } from '../../common/DateFormatter';

interface TypeOption {
    name: string;
    value: string;
}

interface EventLogModel {
    dateFrom: Date | undefined;
    dateTo: Date | undefined;
    site: any;
    siteId: string | null;
    siteOptions: Array<any>;
    typeOptions: Array<TypeOption>;
    type: TypeOption;
    logs: Array<any>;
    page: number;
    pageSize: number;
    totalLogs: number;
}

const model = reactive<EventLogModel>({
    dateFrom: undefined,
    dateTo: undefined,
    site: null,
    siteId: null,
    siteOptions: [],
    typeOptions: [
        {"value": "", "name": "All"},
        { "value": "0", "name": "Information" },
        { "value": "1", "name": "Warning" },
        { "value": "2", "name": "Error" }
    ],
    type: { "value": "", "name": "All" }, // all types
    logs: [],
    page: 1,
    pageSize: 10,
    totalLogs: 0
});

const apiClient = inject(API_CLIENT)!;

const disabledDateFromAfter = (date: Date) => {
    const today = new Date().setHours(0, 0, 0, 0);
    if (date > new Date(today)) {
        return true;
    }

    return model.dateTo ? date >= model.dateTo : false;                
};

const disabledDateToBefore = (date: Date) => {
    const today = new Date().setHours(0, 0, 0, 0);
    if (date > new Date(today)) {
        return true;
    }

    return model.dateFrom ? date <= model.dateFrom : false;
};

const loadLogs = () => {
    const request = {
        page: model.page,
        pageSize: model.pageSize,                    
        siteId: model.site?.id,
        type:  model.type && model.type.value == "" ? null : model.type.value
    };

    if (model.dateFrom) {
        const df = new Date(model.dateFrom).setHours(0, 0, 0, 0);
        //@ts-ignore
        request.dateFrom = new Date(df);
    }

    if (model.dateTo) {
        const dt = new Date(model.dateTo).setHours(23, 59, 59, 999);
        //@ts-ignore
        request.dateTo = new Date(dt);
    }
    
    apiClient.postAsync('api/eventlog', request)
        .then((response: any) => {
            const logs = response.data;
            model.logs = logs;
            model.totalLogs = Number(response.headers["total-rows-amount"]);
    });
};

const resetDates = () => {
    model.dateFrom = undefined;
    model.dateTo = undefined;
    loadLogs();
};

const clearSiteName = () => {
    model.site = null;
};

const onSearch = (search: string, loading: (val: boolean) => void) => {
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

watch(() => model.site, () => loadLogs());
watch(() => model.type, () => loadLogs());
watch(() => model.page, () => loadLogs());

onMounted(() => loadLogs());
</script>
<template>
    <div class="content-block-container">
        <div class="general-page-title">
            <img src="./../../../public/icons8-logs-28.png" />
            <span>Site Events Log</span>
        </div>
        <UserInfo />
        <NavigationMenu />
        <div class="event-logs-content">
            <div class="dates-filter-container">
                <div class="left w-250">
                    Date from: &nbsp; <Datepicker v-model="model.dateFrom" :disabled-dates="{predicate: disabledDateFromAfter}" valueType="format" :editable="false" />
                </div>
                <div class="left w-250">
                    Date to: &nbsp; <Datepicker v-model="model.dateTo" :disabled-dates="{predicate: disabledDateToBefore}" valueType="format" :editable="false" />
                </div>
                <div class="button-bar">
                    <button class="btn btn-primary" @click="loadLogs">Search</button>
                    <button class="btn btn-default" @click="resetDates">Reset</button>
                </div>
            </div>
            <div class="event-logs-grid-container">
                <table class="table table-striped">
                    <thead>
                        <tr class="header-row">
                            <th class="w-300">
                                <div>  
                                    <span>Site</span> 
                                </div>
                                <div class="composite-filter-cell">
                                    <div class="site-name-select-container">
                                        <v-select ref="siteNameSelect" 
                                            v-model="model.site" label="name" 
                                            :clearable="false" 
                                            :filterable="false" 
                                            :options="model.siteOptions" 
                                            @search="onSearch">
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
                                    <div class="site-name-select-container-dismiss" v-if="!!model.site">
                                        <button class="btn btn-primary btn-sm clear-site-name-btn" title="Clear site name" @click="clearSiteName">X</button>
                                    </div>
                                </div>
                            </th>
                            <th class="th-top w-250">Timestamp</th>
                            <th class="th-top w-200">
                                <span>Event Type</span> <br/>                                
                                <v-select class="type-select"
                                    v-model="model.type" 
                                    label="name" 
                                    :clearable="false" 
                                    :searchable="false" 
                                    :options="model.typeOptions">
                                    <template #option="option" slot="option" slot-scope="option">
                                        <div>{{option.name}}</div>
                                    </template>
                                    <template #selectedOption="option" slot="selected-option" slot-scope="option">
                                        <div class="option-selected">{{option.name}}</div>
                                    </template>
                                </v-select>
                            </th>
                            <th class="th-top">Details</th>
                        </tr>
                    </thead>
                </table>
                <div class="event-logs-rows-container" v-if="model.logs.length">
                    <table class="table table-striped">
                        <tr v-for="log in model.logs" :key="log">
                            <td class="w-300">{{log.siteName}}</td>
                            <td class="w-250">{{formatDate(log.timestamp)}}</td>
                            <td class="w-200">{{log.type}}</td>
                            <td>{{log.details}}</td>
                        </tr>
                    </table>
                </div>
                <div v-if="model.logs.length" class="pagination-footer">
                    <div class="left">
                        <vue-awesome-paginate
                            :total-items="model.totalLogs"
                            :items-per-page="model.pageSize"
                            :max-pages-shown="5"
                            v-model="model.page"
                        />
                    </div>
                    <div class="total-and-page-size-container">
                        <span>Logs found: <strong>{{model.totalLogs}}</strong></span> &nbsp;
                        <span style="margin-left: 35px;">Display per page: </span>
                        <select v-model="model.pageSize" @change="loadLogs">
                            <option>5</option>
                            <option>10</option>
                            <option>25</option>
                            <option>50</option>
                            <option>100</option>
                        </select>
                    </div>
                </div>
                <div class="no-logs-message" v-if="!model.logs.length">
                    No logs found
                </div>
            </div>
        </div>
    </div>
</template>
<style scoped>
    .header-row {
        background-color: gainsboro;
    }

    .table {
        width: calc(100% - 5px);
    }
    
    .dates-filter-container {
        margin-top: 5px;
        margin-bottom: 5px;
    }
    
    .event-logs-content {
        background-color: azure;
        height: calc(100vh - 166px);
        padding-left: 10px;
        overflow-y: auto;
    }

    .left {
        float: left;
        padding-left: 5px;
    }

    .w-200 {
        width: 200px;
    }

    .w-250 {
        width: 250px;
    }

    .w-300 {
        width: 300px;
    }

    .button-bar {
        padding-top: 17px;
        height: 23px;
        display: flex;
        gap: 3px;
    }

    .btn-default {
        border: 1px solid navy;
    }

    .event-logs-grid-container {
        margin-top: 16px;
    }

    .event-logs-grid-container > table {
        margin-bottom: 0px !important;
    }

    .event-logs-rows-container {
        height: calc(100vh - 408px);
        overflow-y: auto;
    }

   .type-select {
        width: calc(100% - 10px);
        height: 34px;
        background-color: antiquewhite;
        margin: 5px;
    }

    .th-top {
        vertical-align: top;
    }

    .no-logs-message {
        text-align: center;
        font-size: x-large;
        padding-top: 25px;
        font-weight: bold;
        color: navy;
        height: calc(100vh - 335px);
    }

    .clear-site-name-btn {
        height: 34px;
    }
    .site-name-select-container {
        background-color: antiquewhite;
        flex-grow: 1;
    }

    .site-name-select-container-dismiss {
        margin-left: 5px;
        float: left;
    }

    .pagination-footer {
        margin-top: 45px;
    }

    .total-and-page-size-container {        
        margin-right: 20px;
        float: right;
    }

    .total-and-page-size-container > select {
        height: 31px;
        border: 1px solid navy;
    }

    .composite-filter-cell {
        display: flex;
        gap: 5px;
        justify-content: space-between;
        margin: 5px;
    }
</style>