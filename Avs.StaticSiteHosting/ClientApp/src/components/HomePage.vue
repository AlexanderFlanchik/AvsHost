<template>
    <div class="content-block-container">
        <div class="general-page-title">
            <!--<img src="../../public/help.png" /> &nbsp;-->
            <span>Home</span>
        </div>
        <UserInfo />
        <NavigationMenu />
        <div class="home-page-content">
            <div class="charts-container">
                <div>
                    <div class="cell-left chart-cell">
                        <PieChart ref="sitesChart" title="Your Sites" seriesName="Sites"></PieChart>
                    </div>
                    <div class="cell-left chart-cell">
                        <PieChart ref="storageUsedChart" title="Storage Used" seriesName="Storage used"></PieChart>
                    </div>
                </div>
            </div>            
            <div>
               <div class="statistics-header">
                  <span>Last 24 Hours Statistics</span>
               </div>
               <div class="statistics-container">
                 <div class="cell-left error-info-cell">
                    <span v-if="errors > 0" class="errors-title">Sites errors: <strong>{{errors}}</strong></span>
                    <span v-if="errors > 0">
                        <b-icon icon="caret-right-fill" v-if="!errorsListExpanded" @click="expandErrors"></b-icon>
                        <b-icon icon="caret-down-fill" v-if="errorsListExpanded" @click="collapseErrors"></b-icon>
                    </span>
                    <span v-if="errors == 0" class="no-errors-title">No errors!</span>
                    <div v-if="errors > 0 && errorsListExpanded">
                        <ErrorSitesList/>
                    </div>
                 </div>
                 <div class="cell-left visits-info-cell">
                     <div>
                         <span class="visits-info">Your sites were visited <strong>{{visits}}</strong> times.</span>
                         <span v-if="errors > 0">
                             Show latest visits &nbsp;
                             <b-icon icon="caret-right-fill" v-if="!lastVisitsExpaned" @click="expandVisits"></b-icon>
                             <b-icon icon="caret-down-fill" v-if="lastVisitsExpaned" @click="collapseVisits"></b-icon>
                         </span>
                     </div>
                     <div v-if="visits > 0 && lastVisitsExpaned">
                         <LastSiteVisits />
                     </div>
                 </div>
               </div>
            </div>            
        </div>
    </div>
</template>
<script>
    import UserInfo from '@/components/UserInfo.vue';
    import NavigationMenu from '@/components/NavigationMenu.vue';
    import PieChart from '@/components/PieChart.vue';
    import ErrorSitesList from '@/components/ErrorSitesList.vue';
    import LastSiteVisits from '@/components/LastSiteVisits.vue';

    export default {
        data: function () {
            return {
                errors: 0,
                visits: 0,
                errorsListExpanded: false,
                lastVisitsExpaned: false
            };
        },

        mounted: function () {
            this.$apiClient.getAsync('api/home')
                .then((response) => {
                    let vm = response.data;
                    let inactiveSites = vm.totalSites - vm.activeSites;
                    if (vm.totalSites) {
                        let sitesData = [
                            {
                                name: 'Active',
                                y: vm.activeSites / vm.totalSites
                            },

                            {
                                name: 'Inactive',
                                y: inactiveSites / vm.totalSites
                            }
                        ];

                        this.$refs.sitesChart.loadData(sitesData);
                    }

                    let totalContentBytes = vm.totalContentSize;
                    if (totalContentBytes) {
                        let storageData = [];
                        for (let storageUsedInfo of vm.storageUsedInfos) {
                            let ratio = storageUsedInfo.bytes / totalContentBytes;
                            if (ratio < 0.001) {
                                continue;
                            }

                            let o = {
                                name: storageUsedInfo.siteName,
                                y: ratio
                            };

                            storageData.push(o);
                        }
                        this.$refs.storageUsedChart.loadData(storageData);
                    }

                    this.errors = vm.errors;
                    this.visits = vm.totalSiteVisits;

                }).catch(err => console.log(err));
        },

        methods: {
            expandErrors: function () {
                this.errorsListExpanded = true;
            },
            collapseErrors: function () {
                this.errorsListExpanded = false;
            },
            expandVisits: function () {
                this.lastVisitsExpaned = true;
            },
            collapseVisits: function () {
                this.lastVisitsExpaned = false;
            }
        },

        components: {
            UserInfo,
            NavigationMenu,
            PieChart,
            ErrorSitesList,
            LastSiteVisits
        }
    }
</script>
<style scoped>
    .cell-left {
        float: left;
        width: 50%;
        height: 50%;
    }
    
    .charts-container {
        height: 420px;
    }

    .chart-cell {        
        padding-left: calc(25% - 225px);
    }

    .home-page-content {
        height: calc(100% - 155px);
        background-color: azure;
    }

    .errors-title {
        color: red;
        font-style: italic;
    }

    .no-errors-title {
        color: navy;
    }

    .error-info-cell {
        padding-left: 10px;
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
    }

    .statistics-container {      
        height: calc(100vh - 645px);
    }
</style>