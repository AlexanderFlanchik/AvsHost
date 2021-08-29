<template>
    <div v-if="visits.length">
        <div>
            <span>Top 5 latest visits:</span>
        </div>
        <table class="last-visits-table">
            <thead>
                <tr class="last-visits-header">
                    <th class="w-120">Site Name</th>
                    <th class="w-220">Visit Timestamp</th>
                </tr>
            </thead>
        </table>
        <table class="table table-striped last-visits-table">
            <tbody>
                <tr v-for="row in visits" :key="row.siteName">
                    <td class="w-120">{{row.siteName}}</td>
                    <td class="w-220">{{formatDate(row.visit)}}</td>
                </tr>
            </tbody>
        </table>
    </div>
</template>
<script>
    const moment = require('moment');

    export default {
        data: function () {
            return {
                visits: []
            };
        },
        mounted: function () {
            this.loadData();
        },
        methods: {
            loadData: async function () {
                this.$apiClient.getAsync('api/homepagestatistics/last-visits')
                    .then((response) => {
                        this.visits = response.data;
                    });
            },
            formatDate: function (date) {
                return date && moment(date).format('MM/DD/YYYY hh:mm:ss A');
            }
        }
    }
</script>
<style>
    .last-visits-table {
        width: 95%;
        margin-right: 5px;
    }
    .w-120 {
        width: 120px;
    }
    .w-220 {
        width: 220px;
    }
    .last-visits-header > th {
        background-color: navy;
        color: white;
    }

</style>