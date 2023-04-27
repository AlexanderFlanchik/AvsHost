<template>
    <div class="content-block-container">
        <div class="general-page-title">
            <img src="../../public/icons8-logs-28.png" />
            <span>Site Events Log</span>
        </div>
        <UserInfo />
        <NavigationMenu />
        <div class="event-logs-content">
            <div class="dates-filter-container">
                <div class="left w-250">
                    Date from: &nbsp; <date-picker v-model="dateFrom" :disabled-date="disabledDateFromAfter" valueType="format" :editable="false"></date-picker>
                </div>
                <div class="left w-250">
                    Date to: &nbsp; <date-picker v-model="dateTo" :disabled-date="disabledDateToBefore" valueType="format" :editable="false"></date-picker>
                </div>
                <div class="button-bar">
                    <button class="btn btn-primary" @click="loadLogs">Search</button> &nbsp;
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
                                <div class="site-name-select-container w-200">
                                    <v-select ref="siteNameSelect" v-model="site" label="name" :clearable="false" :filterable="false" :options="siteOptions" @search="onSearch" @input="onInput">
                                        <template slot="no-options">
                                            <div class="no-options-placeholder">Start enter a site name</div>
                                        </template>

                                        <template slot="option" slot-scope="option">
                                            <div>{{option.name}}</div>
                                        </template>

                                        <template slot="selected-option" slot-scope="option">
                                            <div class="option-selected">{{option.name}}</div>
                                        </template>
                                    </v-select>
                                </div>
                                <div class="site-name-select-container-dismiss" v-if="!!site">
                                    <button class="btn btn-primary btn-sm clear-site-name-btn" title="Clear site name" @click="clearSiteName">X</button>
                                </div>
                            </th>
                            <th class="th-top w-250">Timestamp</th>
                            <th class="th-top w-200">
                                <span>Event Type</span> <br/>                                
                                <v-select class="type-select" v-model="type" label="name" :clearable="false" :searchable="false" :options="typeOptions" @input="typeChanged">
                                    <template slot="option" slot-scope="option">
                                        <div>{{option.name}}</div>
                                    </template>
                                    <template slot="selected-option" slot-scope="option">
                                        <div class="option-selected">{{option.name}}</div>
                                    </template>
                                </v-select>
                            </th>
                            <th class="th-top">Details</th>
                        </tr>
                    </thead>
                </table>
                <div class="event-logs-rows-container" v-if="logs.length">
                    <table class="table table-striped">
                        <tr v-for="log in logs" :key="log">
                            <td class="w-300">{{log.siteName}}</td>
                            <td class="w-250">{{formatDate(log.timestamp)}}</td>
                            <td class="w-200">{{log.type}}</td>
                            <td>{{log.details}}</td>
                        </tr>
                    </table>
                </div>
                <div v-if="logs.length">
                    <div class="left">
                        <b-pagination v-model="page" :total-rows="totalLogs" :per-page="pageSize" @input="loadLogs" size="sm"></b-pagination>
                    </div>
                    <div class="total-and-page-size-container">
                        <span>Logs found: <strong>{{totalLogs}}</strong></span> &nbsp;
                        <span style="margin-left: 35px;">Display per page: </span>
                        <select v-model="pageSize" @change="loadLogs">
                            <option>5</option>
                            <option>10</option>
                            <option>25</option>
                            <option>50</option>
                            <option>100</option>
                        </select>
                    </div>
                </div>
                <div class="no-logs-message" v-if="!logs.length">
                    No logs found
                </div>
            </div>
        </div>
    </div>
</template>
<script>
    import UserInfo from '@/components/UserInfo.vue';
    import NavigationMenu from '@/components/NavigationMenu.vue';

    import DatePicker from 'vue2-datepicker';
    import 'vue2-datepicker/index.css';

    const moment = require('moment');

    export default {
        data: function () {
            return {
                dateFrom: null,
                dateTo: null,
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
            };
        },
        mounted: function () {           
            this.loadLogs();
        },
        methods: {
            disabledDateFromAfter: function (date) {
                let today = new Date().setHours(0, 0, 0, 0);         
                if (date > new Date(today)) {
                    return true;
                }

                return this.dateTo ? date >= this.dateTo : false;                
            },

            disabledDateToBefore: function (date) {
                let today = new Date().setHours(0, 0, 0, 0);
                if (date > new Date(today)) {
                    return true;
                }

                return this.dateFrom ? date <= this.dateFrom : false;
            },

            resetDates: function () {
                this.dateFrom = null;
                this.dateTo = null;
                this.loadLogs();
            },

            clearSiteName: function () {
                this.site = null;
                this.siteId = null;
                this.siteOptions = [];
                this.loadLogs();
            },

            formatDate: function (date) {
                return date && moment(date).format('MM/DD/YYYY hh:mm:ss A');
            },

            onInput: function (value) {
                if (value && value.id) {
                    this.siteId = value.id;
                    this.page = 1;
                    this.loadLogs();
                } 
            },

            onSearch: function (search, loading) {
                if (search && search.length) {
                    loading(true);
                    this.$apiClient.getAsync(`api/sitesearch/${search}`)
                        .then((response) => {
                            loading(false);                            
                            this.siteOptions = response.data.sites;                            
                        }).catch((e) => {
                            console.log(e);
                            loading(false);
                        });
                }
            },

            loadLogs: function () {
                
                let request = {
                    page: this.page,
                    pageSize: this.pageSize,                    
                    siteId: this.siteId,
                    type:  this.type && this.type.value == "" ? null : this.type.value
                };

                if (this.dateFrom) {
                    let df = new Date(this.dateFrom).setHours(0, 0, 0, 0);
                    request.dateFrom = new Date(df);
                }

                if (this.dateTo) {
                    let dt = new Date(this.dateTo).setHours(23, 59, 59, 999);
                    request.dateTo = new Date(dt);
                }

                this.$apiClient.postAsync('api/eventlog', request)
                    .then((response) => {
                        let logs = response.data;
                        this.logs = logs;
                        this.totalLogs = Number(response.headers["total-rows-amount"]);
                    });
            },
            typeChanged: function () {
                this.page = 1;
                this.loadLogs();
            }
        },
        computed: {
         
        },
        components: {
            UserInfo,
            NavigationMenu,
            DatePicker
        }
    }
</script>
<style scoped>
    .header-row {
        background-color: gainsboro;
    }
    
    .dates-filter-container {
        margin-bottom: 5px;
    }
    
    .event-logs-content {
        background-color: azure;
        height: calc(100% - 155px);
        padding-left: 10px;
        overflow-y: auto;
    }

    .left {
        float: left;
        padding-left: 5px;
        padding-top: 5px;
        padding-bottom: 5px;
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
        padding-top: 25px;
        height: 35px;
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
        width: 150px;
        height: 34px;
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
    }

    .clear-site-name-btn {
        height: 34px;
    }
    .site-name-select-container {
        float: left;
    }

    .site-name-select-container-dismiss {
        margin-left: 5px;
        float: left;
    }

    .total-and-page-size-container {        
        margin-right: 20px;
        float: right;
        padding-top: 10px;
    }

    .total-and-page-size-container > select {
        height: 31px;
        border: 1px solid navy;
    }

</style>