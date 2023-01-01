<template>
    <div class="content-block-container">
        <div class="general-page-title">
            <img src="../../../ClientApp/public/dashboard.png" />
            <span>Home</span>
        </div>
        <UserInfo />
        <NavigationMenu />
        <div class="home-page-content" v-if="areSites">
            <div class="charts-container">
                <div>
                    <div class="cell-left chart-cell">
                        <PieChart ref="sitesChart" :title="sitesTitle" seriesName="Sites"></PieChart>
                    </div>
                    <div class="cell-left chart-cell">
                        <PieChart ref="storageUsedChart" :title="spaceUsedTitle" seriesName="Storage used"></PieChart>
                    </div>
                </div>
            </div>            
            <div>
               <div class="statistics-header">
                  <span>Last 24 Hours Statistics</span>
               </div>
               <div class="statistics-container">
                 <div class="cell-left error-info-cell">
                    <span v-if="errors > 0" class="errors-title">Total errors: <strong>{{errors}}</strong></span>
                    <span v-if="errors > 0">
                        <b-icon icon="caret-right-fill" v-if="!errorsListExpanded" @click="expandErrors"></b-icon>
                        <b-icon icon="caret-down-fill" v-if="errorsListExpanded" @click="collapseErrors"></b-icon>
                    </span>
                    <span v-if="errors == 0" class="no-errors-title">No errors!</span>
                    <div v-if="errors > 0 && errorsListExpanded">
                        <ErrorSitesList ref="site-errors"/>
                    </div>
                 </div>
                 <div class="cell-left visits-info-cell">
                     <div>
                         <span class="visits-info">Your sites were visited <strong>{{visits}}</strong> times.</span>
                         <span v-if="visits > 0">
                             Show latest visits &nbsp;
                             <b-icon icon="caret-right-fill" v-if="!lastVisitsExpaned" @click="expandVisits"></b-icon>
                             <b-icon icon="caret-down-fill" v-if="lastVisitsExpaned" @click="collapseVisits"></b-icon>
                         </span>
                     </div>
                     <div v-if="visits > 0 && lastVisitsExpaned" class="last-site-visits-container">
                         <LastSiteVisits  ref="last-site-visits" />
                     </div>
                 </div>
               </div>
            </div>            
        </div>
        <div class="home-page-content" v-if="!areSites">
            <div class="no-sites-message">You do not have sites yet.</div>
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
                totalSites: 0,
                totalSpaceKb: 0,
                areSites: false,
                errors: 0,
                visits: 0,
                errorsListExpanded: false,
                lastVisitsExpaned: false
            };
        },

        mounted: function () {
            console.log("Home Page mounted started...");
            let userInfo = this.$authService.getUserInfo();
            if (userInfo && userInfo.isAdmin) {
                // Prevent admin from home page view
                this.$router.replace('/dashboard');
                return;
            }

            this.$userNotificationService.subscribeToSiteEvents(
                () => {
                    this.visits++;
                    if (this.lastVisitsExpaned) {  
                       this.$refs["last-site-visits"].loadData();
                    }
                }, 
                () => {
                    this.errors++;
                    if (this.errorsListExpanded) {
                        this.$refs["site-errors"].loadData();
                    }
                }
            );

            this.$apiClient.getAsync('api/home')
                .then((response) => {
                    let vm = response.data;
                    this.areSites = vm.totalSites > 0;
                    this.totalSites = vm.totalSites;
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
                        
                        this.$nextTick(() => this.$refs.sitesChart.loadData(sitesData));
                    }

                    let totalContentBytes = vm.totalContentSize;
                    if (totalContentBytes) {
                        let storageData = [];
                        let otherSites = { name: 'Others', y: 0 };

                        for (let storageUsedInfo of vm.storageUsedInfos) {
                            let ratio = storageUsedInfo.bytes / totalContentBytes;
    
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

                        this.$nextTick(() => this.$refs.storageUsedChart.loadData(storageData));
                    }

                    this.totalSpaceKb = vm.totalContentSize / 1024;
                    this.errors = vm.errors;
                    this.visits = vm.totalSiteVisits;
                }).catch(err => console.log(err));
        },

        beforeDestroy: function() {
            this.$userNotificationService.unsubscribe(this.$userNotificationService.SiteErrorEvent);
            this.$userNotificationService.unsubscribe(this.$userNotificationService.NewSiteVisited);
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
        computed: {
            sitesTitle: function() {
                return `Your Sites (${this.totalSites} total)`;
            },
            spaceUsedTitle: function() {
                return `Storage Used ${this.totalSpaceKb.toFixed(2)} Kb`;
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
        height: 400px;
    }

    .chart-cell {        
        padding-left: calc(25% - 225px);
    }

    .home-page-content {
        height: calc(100% - 155px);
        background-color: azure;
        overflow-y: auto;
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
    }

    .statistics-container {      
        height: calc(100vh - 645px);
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
</style>