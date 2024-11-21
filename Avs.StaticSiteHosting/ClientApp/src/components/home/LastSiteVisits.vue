<script setup lang="ts">
import { inject, onMounted, reactive } from 'vue';
import { formatDate } from '../../common/DateFormatter';
import { API_CLIENT } from '../../common/diKeys';

interface VisitsModel {
    visits: {
        siteName: string;
        visit: Date
    }[]
 }

 const model = reactive<VisitsModel>({ visits: [] });

const apiClient = inject(API_CLIENT)!;
const loadData = () => apiClient.getAsync('api/homepagestatistics/last-visits').then((response: any) => model.visits = response.data);
//
defineExpose( { loadData }); 
onMounted(loadData);
</script>
<template>
 <div v-if="model.visits.length">
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
                <tr v-for="row in model.visits" :key="row.siteName">
                    <td class="w-120">{{row.siteName}}</td>
                    <td class="w-220">{{formatDate(row.visit)}}</td>
                </tr>
            </tbody>
        </table>
    </div>
</template>
<style scoped>
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