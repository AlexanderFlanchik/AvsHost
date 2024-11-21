<script setup lang="ts">
import { inject, onMounted, reactive } from 'vue';
import { API_CLIENT } from '../../common/diKeys';
import { SiteContextManager } from '../../services/SiteContextManager';
import { useRouter } from 'vue-router';
import { formatDate } from '../../common/DateFormatter';

interface SiteInfo {
    siteId: string;
    siteName: string;
    timestamp: Date;
}

interface ErrorSiteListModel {
    sites: Array<SiteInfo>;
    pagination: {
        total: number;
        page: number;
        pageSize: number;
    }
}

const model = reactive<ErrorSiteListModel>({
    sites: [],
    pagination: {
        total: 0,
        page: 1,
        pageSize: 10
    }
 });

 const apiClient = inject(API_CLIENT)!;

 const loadData = () => {
    apiClient.getAsync(`api/HomePageStatistics/error-sites?page=${model.pagination.page}&pageSize=${model.pagination.pageSize}`)
        .then((response: any) => {
            model.pagination.total = Number(response.headers["total-rows-amount"]);
            model.sites = response.data;
        });
 };

 defineExpose({ loadData });

 const editSite = async(siteId: string) => {
    const sm = new SiteContextManager();
    sm.delete();

    const router = useRouter();
    router.push({ path: '/sites/update/' + siteId });
 };

 onMounted(() => {
    loadData();
 });

</script>
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
        <div v-if="model.sites.length">
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
                        <tr v-for="site in model.sites" :key="site.siteId">
                            <td class="w-200">{{site.siteName}}</td>
                            <td class="w-220">{{formatDate(site.timestamp)}}</td>
                            <td>
                                <a href="#" @click="editSite(site.siteId)">Edit..</a>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</template>
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