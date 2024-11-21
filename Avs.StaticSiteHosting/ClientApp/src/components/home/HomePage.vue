<script setup lang="ts">
import { computed, inject, nextTick, onBeforeUnmount, onMounted, reactive, ref } from 'vue';
import { API_CLIENT, AUTH_SERVICE, USER_NOTIFICATIONS_SERVICE } from '../../common/diKeys';
import { useRouter } from 'vue-router';
import LastSiteVisits from './LastSiteVisits.vue';
import ErrorSitesList from './ErrorSitesList.vue';
import PieChart from './PieChart.vue';
import UserInfo from '../layout/UserInfo.vue';
import NavigationMenu from '../layout/NavigationMenu.vue';
import ExpandCollapseIcon from '../ExpandCollapseIcon.vue';
import { IconKind } from '../../common/IconKind';
 
interface HomePageModel {
    totalSites: number;
    totalSpaceKb: number;
    errors: number;
    visits: number;
    areSites: boolean;
    errorsListExpanded: boolean;
    lastVisitsExpanded: boolean;
 }

 const model = reactive<HomePageModel>({
    totalSites: 0,
    totalSpaceKb: 0,
    errors: 0,
    visits: 0,
    areSites: false,
    errorsListExpanded: false,
    lastVisitsExpanded: false
 });

 const authService = inject(AUTH_SERVICE)!;
 const userNotificationsService = inject(USER_NOTIFICATIONS_SERVICE)!;
 const apiClient = inject(API_CLIENT)!;
 const router = useRouter();
 const lastSiteVisitsRef = ref<typeof LastSiteVisits | null>(null);
 const siteErrorsRef = ref<typeof ErrorSitesList | null>(null);
 const sitesChartRef = ref<typeof PieChart | null>(null);
 const storageUsedChartRef = ref<typeof PieChart | null>(null);

 onMounted(() => {
    const userInfo = authService.getUserInfo();
    if (userInfo && userInfo.isAdmin) {
        // Prevent admin from home page view
        router.replace('/dashboard');
        return;
    }

    userNotificationsService.subscribeToSiteEvents(
        () => {
            model.visits++;
            if (model.lastVisitsExpanded) {  
                lastSiteVisitsRef.value?.loadData();
            }
        }, 
        () => {
            model.errors++;
            if (model.errorsListExpanded) {
                siteErrorsRef.value?.loadData();
            }
        }
    );

    apiClient.getAsync('api/home')
        .then(async (response: any) => {
            const vm = response.data;
            model.areSites = vm.totalSites > 0;
            model.totalSites = vm.totalSites;
            const inactiveSites = vm.totalSites - vm.activeSites;
            if (vm.totalSites) {
                const sitesData = [
                    {
                        name: 'Active',
                        y: vm.activeSites / vm.totalSites
                    },
                    {
                        name: 'Inactive',
                        y: inactiveSites / vm.totalSites
                    }
                ];

                await nextTick(() => sitesChartRef.value?.loadData(sitesData));
            }

            const totalContentBytes = vm.totalContentSize;
            if (totalContentBytes) {
                //@ts-ignore
                const storageData = [];
                const otherSites = { name: 'Others', y: 0 };
                for (const storageUsedInfo of vm.storageUsedInfos) {
                    const ratio = storageUsedInfo.size / totalContentBytes;
                    if (ratio <= 0.01) {
                        otherSites.y += ratio;
                    } else {
                        let o = {
                            name: storageUsedInfo.siteName,
                            y: ratio
                        };

                        storageData.push(o);
                    }
                }

                if (otherSites.y > 0) {
                    storageData.push(otherSites);
                }

                //@ts-ignore
                await nextTick(() => storageUsedChartRef.value?.loadData(storageData));
            }

            model.totalSpaceKb = vm.totalContentSize;
            model.errors = vm.errors;
            model.visits = vm.totalSiteVisits;
        }).catch((e: Error) => console.log(e));
 });

onBeforeUnmount(() => {
    userNotificationsService.unsubscribe(userNotificationsService.SiteErrorEvent);
    userNotificationsService.unsubscribe(userNotificationsService.NewSiteVisited);
});

const sitesTitle = computed(() => `Your Sites (${model.totalSites} total)`);
const spaceUsedTitle = computed(() => `Storage Used ${model.totalSpaceKb.toFixed(2)} Kb`);
const expandErrors = () => model.errorsListExpanded = true;
const collapseErrors = () => model.errorsListExpanded = false;
const expandVisits = () => model.lastVisitsExpanded = true;
const collapseVisits = () => model.lastVisitsExpanded = false;
</script>
<template>
 <div class="content-block-container">
        <div class="general-page-title">
            <img src="../../../public/dashboard.png" />
            <span>Home</span>
        </div>
        <UserInfo />
        <NavigationMenu />
        <div class="home-page-content" v-if="model.areSites">
            <div class="charts-container">
                <div class="top-charts-container">
                    <div class="cell-left chart-cell">
                        <PieChart ref="sitesChartRef" :title="sitesTitle" seriesName="Sites"></PieChart>
                    </div>
                    <div class="cell-left chart-cell">
                        <PieChart ref="storageUsedChartRef" :title="spaceUsedTitle" seriesName="Storage used"></PieChart>
                    </div>
                </div>
            </div>            
            <div>
               <div class="statistics-header">
                  <span>Last 24 Hours Statistics</span>
               </div>
               <div class="statistics-container">
                 <div class="cell-left error-info-cell">
                    <span v-if="model.errors > 0" class="errors-title">Total errors: <strong>{{model.errors}}</strong></span>
                    <span v-if="model.errors > 0">
                        <ExpandCollapseIcon v-if="!model.errorsListExpanded" title="Expand" :kind="IconKind.Expanded" :click="expandErrors" />
                        <ExpandCollapseIcon v-if="model.errorsListExpanded" title="Collapse" :kind="IconKind.Collapsed" :click="collapseErrors" />
                    </span>
                    <span v-if="model.errors == 0" class="no-errors-title">No errors!</span>
                    <div v-if="model.errors > 0 && model.errorsListExpanded">
                        <ErrorSitesList ref="siteErrorsRef"/>
                    </div>
                 </div>
                 <div class="cell-left visits-info-cell">
                     <div>
                         <span class="visits-info">Your sites were visited <strong>{{model.visits}}</strong> times.</span>
                         <span v-if="model.visits > 0">
                             Show latest visits &nbsp;
                             <ExpandCollapseIcon v-if="!model.lastVisitsExpanded" :kind="IconKind.Expanded" :click="expandVisits" />
                             <ExpandCollapseIcon v-if="model.lastVisitsExpanded" :kind="IconKind.Collapsed" :click="collapseVisits" />
                         </span>
                     </div>
                     <div v-if="model.visits > 0 && model.lastVisitsExpanded" class="last-site-visits-container">
                         <LastSiteVisits ref="lastSiteVisitsRef" />
                     </div>
                 </div>
               </div>
            </div>            
        </div>
        <div class="home-page-content" v-if="!model.areSites">
            <div class="no-sites-message">You do not have sites yet.</div>
        </div>
    </div>
</template>
<style scoped>
 .top-charts-container {
    display: flex;
 }

 .cell-left {
        float: left;
        width: 50%;
        height: 50%;
    }
    
    .charts-container {
        height: 400px;
    }

    .chart-cell {        
        padding-left: calc(25% - 225px);
    }

    .home-page-content {
        background-color: azure;
        overflow-y: auto;
        overflow-x: hidden;
    }

    .errors-title {
        color: red;
        font-style: italic;
    }

    .no-errors-title {
        color: navy;
    }

    .error-info-cell {
        padding-left: 50px;
        padding-top: 10px;
    }

    .error-info-cell > span {
        font-size: larger;
    }

    .visits-info-cell {
        padding-left: 10px;
        padding-top: 10px;
    }

    .visits-info {
        color: navy;
        font-size: larger;
    }

    .statistics-header {
        width: 100%;
        height: 35px;
        background-color: navy;
        color: white;
        padding-top: 5px;
        padding-left: 5px;
        line-height: 28px;
    }

    .statistics-container {      
        height: calc(100vh - 607px);
        display: flex;
    }

    .last-site-visits-container {
        height: calc(100vh - 680px);
        overflow-y: auto;
    }

    .no-sites-message {
        width: 100%;
        min-height: 200px;
        text-align: center;
        color: navy;
        font-size: 32pt;
        font-weight: bold;
        font-family: Garamond;
    }

    i {
        cursor: pointer;
    }
</style>