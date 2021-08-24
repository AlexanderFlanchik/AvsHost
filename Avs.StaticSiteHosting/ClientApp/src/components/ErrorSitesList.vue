<template>
    <div>
        <div style="width: 95%">
            <div class="swe-header">
                Sites with Errors
            </div>
            <div class="ref-left"> <!--TODO: remove this when a real-time error check is added-->
                <a href="javascript:void(0)" @click="loadData">Refresh...</a>
            </div>
        </div>
        <div v-if="sites.length">
            <div>
                <table class="error-sites-table">
                    <thead>
                        <tr class="error-sites-header">
                            <th class="w-200">Site</th>
                            <th class="w-220">Timestamp</th>
                            <th>&nbsp;</th>
                        </tr>
                    </thead>
                </table>
            </div>
            <div class="error-sites-container">
                <table class="table table-striped error-sites-table">
                    <tbody>
                        <tr v-for="site in sites" :key="site">
                            <td class="w-200">{{site.siteName}}</td>
                            <td class="w-220">{{formatDate(site.timestamp)}}</td>
                            <td>
                                <router-link :to="{ path: '/sites/update/' + site.siteId }">Edit..</router-link>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</template>
<script>
    const moment = require('moment');

    export default {
        data: function () {
            return {
                sites: [],
                pagination: {
                    total: 0,
                    page: 1,
                    pageSize: 10
                }
            };
        },
        mounted: function () {
            this.loadData();
        },
        methods: {
            loadData: function () {
                this.$apiClient.getAsync(`api/HomePageStatistics/error-sites?page=${this.pagination.page}&pageSize=${this.pagination.pageSize}`)
                .then((response) => {
                    this.pagination.total = Number(response.headers["total-rows-amount"]);
                    this.sites = response.data;
                });
            },

            formatDate: function (date) {
                return date && moment(date).format('MM/DD/YYYY hh:mm:ss A');
            }
        }
    }
</script>
<style scoped>
    .swe-header {
        float: left;
        width: 250px;
    }
    .ref-left {
        text-align: right;
        margin-right: 5px;
    }
    .w-200 {
        width: 200px;
    }
    .w-220 {
        width: 220px;
    }
    .error-sites-container {
        height: calc(100vh - 780px);
        overflow-y: auto;
    }
    .error-sites-table {        
        width: 95%;
        margin-right: 5px;
    }
    .error-sites-header > th {
        background-color: navy;
        color: white;
        font-weight: bold;
    }   
</style>