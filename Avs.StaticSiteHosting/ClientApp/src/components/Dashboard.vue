<template>
    <div class="content-block-container">
        <div class="general-page-title">
            <img src="../../public/icons8-dashboard-28.png" />
            <span>Dashboard</span>
        </div>
        <UserInfo />
        <NavigationMenu />
        <div class="dashboard-content">
            <table class="w-100prc">
                <tr>
                    <td class="sites-amount-container">
                        {{isAdmin ? "All Sites" : "Your sites"}}:&nbsp;<strong>{{totalFound}}</strong>&nbsp; 
                        Active:&nbsp;<strong>{{activeFound}}</strong>
                    </td>
                    <td class="pager-conatiner" v-if="totalFound > 0">
                        <b-pagination v-model="page"
                                      :total-rows="totalFound"
                                      :per-page="pageSize"
                                      size="sm"
                                      @input="pageChanged">
                        </b-pagination>
                    </td>
                    <td v-if="totalFound > 0" class="w-160">
                        <span>Shown: </span>
                        <b-form-select v-model="pageSize" :options="pageSizes" @change="pageSizeChanged" size="sm" class="page-size-select"></b-form-select>                      
                    </td>
                    <td class="new-site-link-container">
                       <a href="#" @click="createNewSite">Add a new site...</a>
                    </td>
                </tr>
            </table>
            <table class="table table-striped columns-holder" v-if="totalFound > 0 || nameFilter || tags.length">
                <thead>
                    <tr>
                        <th class="w-300">
                            <div>
                                <a href="javascript:void(0)" 
                                        v-bind:class="{ asc: sortField == 'Name' && sortState.order == 'Asc', desc: sortField == 'Name' && sortState.order == 'Desc' }" 
                                        @click="sort('Name')">Name</a>
                                &nbsp;
                                <a href="#" @click="nameFilterShown = true">
                                    <img src="../../public/icons8-filter-empty-16.png" v-if="!nameFilter" />
                                    <img src="../../public/icons8-filter-16.png" v-if="nameFilter" />
                                </a>
                            </div>
                            <div class="name-filter-container" v-if="nameFilterShown">
                                <div class="name-filter-container-inner">
                                    <div class="name-filter-container-input-container">Site name contains: <b-form-input type="text" v-model="nameFilter" @input="() => loadSiteData()"></b-form-input></div>
                                    <div class="name-filter-btn-bar">
                                        <button class="btn btn-primary" @click="clearNameFilter">Clear</button>
                                        <button class="btn btn-primary" @click="nameFilterShown = false">OK</button>                                    
                                    </div>
                                </div>
                            </div>
                        </th>
                        <th class="w-300">Description</th>
                        <th class="w-300">
                            <a href="javascript:void(0)" 
                                v-bind:class="{ asc: sortField == 'LaunchedOn' && sortState.order == 'Asc', desc: sortField == 'LaunchedOn' && sortState.order == 'Desc' }"   
                                @click="sort('LaunchedOn')">Launched On</a></th>
                        <th class="w-300" v-if="isAdmin">
                            User Name
                        </th>
                        <th class="w-150">Is Active</th>
                        <th class="tags-column" v-if="!isAdmin">
                            <div>Tags <span v-if="tags.length">({{tags.length}})</span> &nbsp;
                                <a href="javascript:void(0)" @click="openTagsFilter">
                                    <img src="../../public/icons8-filter-empty-16.png" v-if="!tags.length" />
                                    <img src="../../public/icons8-filter-16.png" v-if="tags.length" />
                                </a>
                            </div>
                            <div class="tags-select-list-container">
                                <TagsSelectList ref="tags-filter" :tagIds="tags" :onTagsChanged="applySelectedTags" :hideSelectedTags="true" />
                            </div>
                        </th>
                        <th>Actions</th>
                    </tr>
                </thead>
            </table>

            <div class="site-list-container" v-if="totalFound > 0">
                <table class="table table-striped">
                    <tbody>
                        <tr v-for="site in sites" :key="site" class="site-row">
                            <td class="w-300">{{site.name}}</td>
                            <td class="w-300">{{site.description}}</td>
                            <td class="w-300">{{site.launchedOn}}</td>
                            <td class="w-300" v-if="isAdmin">
                                <router-link :to="{ path: '/user-profile/' + site.owner.id }">{{site.owner.name}}</router-link>
                            </td>
                            <td class="w-150">
                                <input type="checkbox" v-model="site.isActive" disabled="disabled" />
                            </td>
                            <td class="tags-column" v-if="!isAdmin">
                                <div class="tags-holder">
                                   <Tag v-for="tag in site.tags" :key="tag.id" :tagData="tag" />
                                </div>
                            </td>
                            <td>
                                <span><a href="javascript:void(0)" @click="toggleSiteStatus(site.id)">{{ site.isActive ? 'Turn Off' : 'Turn On&nbsp;'}}</a> | </span>
                                <span v-if="!isAdmin"><a href="#" @click="updateSite(site.id)">Update</a> | </span>
                                <span><a href="javascript:void(0)" @click="deleteSite(site.id)">Delete</a></span>
                                <span v-if="!isAdmin && site.landingPage"><span> | </span><a v-bind:href="'/' + site.name + '/' + site.landingPage" target="_blank">Browse</a></span>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
            <div class="no-sites-message" v-if="totalFound === 0">
                No sites found.
            </div>
        </div>
    </div>
</template>

<script>
    import UserInfo from '../components/UserInfo.vue';
    import NavigationMenu from '../components/NavigationMenu.vue';
    import { SiteContextManager } from '../services/SiteContextManager';
    import Tag from './Tags/Tag.vue';
    import TagsSelectList from './Tags/TagsSelectList.vue';
    import * as moment from 'moment';
    const stateManager = new SiteContextManager();

    const Site = function (siteData) {
        const self = this;
        self.id = siteData.id;
        self.name = siteData.name;
        self.description = siteData.description;
        self.launchedOn = siteData.launchedOn ? moment(siteData.launchedOn).format('MM/DD/YYYY hh:mm:ss A') : null;
        self.landingPage = siteData.landingPage;
        self.isActive = siteData.isActive;
        self.tags = siteData.tags;
        self.owner = {
            id: siteData.owner.id,
            name: siteData.owner.userName
        };
    };

    const SortState = function () {
        const self = this;
        self.setState = function (order, next) {
            self.order = order;
            self.next = next;
        };
    };

    // sort states
    const NoSort = new SortState(), Asc = new SortState(), Desc = new SortState();
       
    export default {
        data: function () {
            return {
                isAdmin: false,
                sites: [],
                page: 1,
                pageSize: 10,
                pageSizes: [10, 25, 50, 100],
                totalFound: 0,
                activeFound: 0,
                sortState: NoSort,
                sortField: '',
                tags: [],
                nameFilterShown: false,
                nameFilter: null
            }
        },

        props: {
        },

        mounted: function () {
            NoSort.setState('', Desc);
            Asc.setState('Asc', NoSort);
            Desc.setState('Desc', Asc);

            let userInfo = this.$authService.getUserInfo();
            this.isAdmin = userInfo ? userInfo.isAdmin : false;
            this.loadSiteData();
        },

        methods: {
            loadSiteData: function () {
                let apiUrl = `api/sites?page=${this.page}&pageSize=${this.pageSize}`;

                if (this.sortField && this.sortState.order) {
                    apiUrl += `&sortOrder=${this.sortState.order}&sortField=${this.sortField}`;
                }

                let tags = "";
                for (let tag of this.tags) {
                    tags += `&tagIds=${tag}`;
                }

                if (tags.length) {
                    apiUrl += tags;
                }

                if (this.nameFilter) {
                    apiUrl += `&siteName=${this.nameFilter}`;
                }

                this.$apiClient.getAsync(apiUrl).then((response) => {
                    let siteRows = response.data.map(s => new Site(s));
                    this.sites = siteRows;
                    this.totalFound = Number(response.headers["total-rows-amount"]);
                    this.activeFound = Number(response.headers["active-sites-amount"]);
                });
            },

            clearNameFilter: function() {
                this.nameFilter = null;
                this.loadSiteData();
                this.nameFilterShown = false;
            },
            
            pageChanged: function () {
                this.loadSiteData();
            },

            openTagsFilter: function() {
                this.$refs["tags-filter"].openOptions();
            },

            applySelectedTags: function(selectedTags) {
                this.tags = selectedTags?.map(r => r.id);
                this.loadSiteData();
            },

            toggleSiteStatus: async function (siteId) {
                let toggleResult = await this.$apiClient.postAsync('api/dashboardoperations/toggleSiteStatus', { siteId });
                let site = this.sites.find(s => s.id == siteId);
                if (site) {
                    let newIsActive = toggleResult.data;

                    if (!newIsActive) {
                        this.activeFound -= 1;
                    } else {
                        this.activeFound += 1;
                    }

                    site.isActive = newIsActive;
                }
            },
            deleteSite: async function (siteId) {
                if (confirm(`This will delete the site with all its content. Continue?`)) {
                    await this.$apiClient.deleteAsync(`api/dashboardoperations/${siteId}`);

                    let site = this.sites.find(s => s.id == siteId);
                    let idx = this.sites.indexOf(site);

                    this.sites.splice(idx, 1);
                    this.totalFound--;
                }
            },

            pageSizeChanged: function () {
                this.loadSiteData();
            },

            sort: function (field) {
                if (field != this.sortField) {
                    this.sortField = field;
                    this.sortState = NoSort;
                }

                this.sortState = this.sortState.next;
                if (!this.sortState.order.length) {
                    this.sortField = '';
                }

                this.loadSiteData();
            },

            createNewSite: function () {
                this.clearSiteContext();

                this.$router.push({ name: 'create-site' });
            },

            updateSite: function (siteId) {
                this.clearSiteContext();

                this.$router.push({ path: '/sites/update/' + siteId });
            },

            clearSiteContext: function () {
                stateManager.delete();
            }
        },
        components: {
            UserInfo,
            NavigationMenu,
            Tag,
            TagsSelectList
        }
    }
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
    h3 {
        margin: 40px 0 0;
    }

    ul {
        list-style-type: none;
        padding: 0;
    }

    li {
        display: inline-block;
        margin: 0 10px;
    }

    a {
        color: #42b983;
    }
    .sites-amount-container {
        padding-left: 5px;
    }
    .name-filter-container {
        position: absolute;
    }

    .name-filter-container-inner {
        background-color: white;
        border: 1px solid darkblue;
        padding: 2px;
    }
    .name-filter-container-input-container {
        padding: 3px;
    }

    .name-filter-container-input-container input {
        border-color: darkblue;
    }
    .name-filter-btn-bar {
        border-top: 1px solid darkblue;
        display: flex;
        flex-direction: row-reverse;
        gap: 2px;
        padding: 2px;
    }
    .w-100prc {
        width: 100%;
    }

    .w-160 {
        width: 160px;
    }

    .w-300 {
        width: 300px;
    }

    .site-row td {
        padding: 0 0 0 8px;
        vertical-align: middle;
    }

    .new-site-link-container {
        width: 130px;
        text-align: right;
    }

    .pager-conatiner {
        padding-top: 10px;
        width: 300px;
    }

    .w-150 {
        width: 150px;
    }

    .site-row:hover {
        background-color: lightskyblue;
    }

    .columns-holder {
        margin-bottom: 0 !important;
    }
    .columns-holder th {
        background: beige;
    }
    .tags-select-list-container {
        position: relative;
        top: -30px;
    }
    .site-list-container {
        height: calc(100vh - 280px);
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

    .dashboard-content {
        background-color: azure;
    }

    .page-size-select {
        width: 65px;
    }

    .asc:after {
        content: "\2193";
    }

    .desc:after {
        content: "\2191";
    }
    .tags-holder {
        display: flex;
        min-height: 40px;
        flex-wrap: wrap;
        row-gap: 2px;
        column-gap: 2px;
    }

    @media screen and (max-width: 1024px)
    {
        .tags-column {
            width: 280px;
        }

        .tags-holder {
            width: 280px;
        }
    }

    @media screen and (min-width: 1025px)
    {
        .tags-column {
            width: 380px;
        }

        .tags-holder {
            width: 380px;
        }
    }
</style>