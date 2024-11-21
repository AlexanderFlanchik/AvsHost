<script setup lang="ts">
import { inject, onMounted, reactive, Ref, ref, watch } from 'vue';
import Tag, { TagData } from '../tags/Tag.vue';
import moment from 'moment';
import { API_CLIENT, AUTH_SERVICE, CONFIG_PROVIDER } from '../../common/diKeys';
import TagsSelectList from '../tags/TagsSelectList.vue';
import UserInfo from '../layout/UserInfo.vue';
import NavigationMenu from '../layout/NavigationMenu.vue';
import { SiteContextManager } from '../../services/SiteContextManager';
import { useRouter } from 'vue-router';
const stateManager = new SiteContextManager();

interface DashboardModel {
    isAdmin: boolean;
    sites: Array<any>;
    page: number;
    pageSize:  number;
    pageSizes: Array<number>;
    totalFound: number;
    activeFound: number;
    sortState: SortState | null;
    sortField: string;
    tags: string[];
    nameFilterShown: boolean;
    nameFilter: string | null;
}

class SiteData {
    id: string;
    name: string;
    description: string;
    launchedOn: string | null;
    landingPage: string;
    isActive: boolean;
    tags: Array<TagData>;
    owner: {
        id: string,
        name: string
    };

    constructor(siteData: any) {
        this.id = siteData.id;
        this.name = siteData.name;
        this.description = siteData.description;
        this.launchedOn = siteData.launchedOn ? moment(siteData.launchedOn).format('MM/DD/YYYY hh:mm:ss A') : null;
        this.landingPage = siteData.landingPage;
        this.isActive = siteData.isActive;
        this.tags = siteData.tags;
        this.owner = {
            id: siteData.owner.id,
            name: siteData.owner.userName
        }
    }
}

class SortState {
    order: string = "";
    next: SortState | null = null;

    setState(order: string, next: SortState) {
        this.order = order;
        this.next = next;
    }
}

// sort states
const NoSort = new SortState();
const Asc = new SortState();
const Desc = new SortState();

const model = reactive<DashboardModel>({
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
});

const apiClient = inject(API_CLIENT)!;
const authService = inject(AUTH_SERVICE)!;
const configProvider = inject(CONFIG_PROVIDER)!; 

const loadSiteData = () => {
    let apiUrl = `api/sites?page=${model.page}&pageSize=${model.pageSize}`;

    if (model.sortField && model.sortState?.order) {
        apiUrl += `&sortOrder=${model.sortState.order}&sortField=${model.sortField}`;
    }

    let tags = "";
    for (let tag of model.tags) {
        tags += `&tagIds=${tag}`;
    }

    if (tags.length) {
        apiUrl += tags;
    }

    if (model.nameFilter) {
        apiUrl += `&siteName=${model.nameFilter}`;
    }

    apiClient.getAsync(apiUrl).then((response: any) => {
        // @ts-ignore
        const siteRows = response.data.map((s: any) => new SiteData(s));
        model.sites = siteRows;
        model.totalFound = Number(response.headers["total-rows-amount"]);
        model.activeFound = Number(response.headers["active-sites-amount"]);
    });
};

const clearNameFilter = () => {
    model.nameFilter = null;
    loadSiteData();
    model.nameFilterShown = false;
};

const tagsFilterRef = ref<typeof TagsSelectList | null>(null);
const openTagsFilter = () => tagsFilterRef.value?.openOptions();
const applySelectedTags = (selectedTags: Ref<Array<TagData>>) => {
    model.tags = selectedTags.value.map(t => t.id);
    loadSiteData();
};

watch(() => model.page, () => loadSiteData());

const toggleSiteStatus = async (siteId: string) => {
    const toggleResult: any = await apiClient.postAsync('api/dashboardoperations/toggleSiteStatus', { siteId });
    const site = model.sites.find((s: any) => s.id == siteId);
    if (site) {
        let newIsActive = toggleResult.data;

        if (!newIsActive) {
            model.activeFound -= 1;
        } else {
            model.activeFound += 1;
        }

        site.isActive = newIsActive;
    }
};

const deleteSite = async (siteId: string) => {
    if (confirm(`This will delete the site with all its content. Continue?`)) {
        await apiClient.deleteAsync(`api/dashboardoperations/${siteId}`);

        const site = model.sites.find((s: any) => s.id == siteId);
        const idx = model.sites.indexOf(site);

        model.sites.splice(idx, 1);
        model.totalFound--;
    }
};

const pageSizeChanged = () => loadSiteData();
const sort = (field: string) => {
    if (field != model.sortField) {
        model.sortField = field;
        model.sortState = NoSort;
    }

    model.sortState = model.sortState ? model.sortState.next : null;
    if (!model.sortState?.order.length) {
        model.sortField = '';
    }

    loadSiteData();
};

const clearSiteContext = () => stateManager.delete();
const router = useRouter();

const createNewSite = () =>  {
    clearSiteContext();
    router.push({ name: 'create-site' });
};

const updateSite = (siteId: string) => {
    clearSiteContext();
    router.push({ path: '/sites/update/' + siteId });
};

const browseLinkClick = (site: SiteData) => {
    const contentHostUrl = configProvider.get().contentHostUrl;
    window.open(`${contentHostUrl}/${site.name}/${site.landingPage}`, '_blank');
};

onMounted(async () => {
    NoSort.setState('', Desc);
    Asc.setState('Asc', NoSort);
    Desc.setState('Desc', Asc);

    const userInfo = authService.getUserInfo();
    model.isAdmin = userInfo ? userInfo.isAdmin : false;
    
    await configProvider.LoadConfig();
    loadSiteData();
});

</script>
<template>
 <div class="content-block-container">
        <div class="general-page-title">
            <img src="../../../public/icons8-dashboard-28.png" />
            <span>Dashboard</span>
        </div>
        <UserInfo />
        <NavigationMenu />
        <div class="dashboard-content">
            <table class="w-100prc">
                <tr>
                    <td class="sites-amount-container">
                        {{model.isAdmin ? "All Sites" : "Your sites"}}:&nbsp;<strong>{{model.totalFound}}</strong>&nbsp; 
                        Active:&nbsp;<strong>{{model.activeFound}}</strong>
                    </td>
                    <td class="pager-conatiner" v-if="model.totalFound > 0">
                        <vue-awesome-paginate
                            :total-items="model.totalFound"
                            :items-per-page="model.pageSize"
                            :max-pages-shown="5"
                            v-model="model.page"
                        />
                    </td>
                    <td v-if="model.totalFound > 0" class="w-160">
                        <span>Shown: </span>
                        <select v-model="model.pageSize" @change="pageSizeChanged" size="sm" class="page-size-select">
                            <option v-for="ps in model.pageSizes" :key="ps" :value="ps">
                                {{ ps }}
                            </option>
                        </select>                      
                    </td>
                    <td class="new-site-link-container">
                       <a href="javascript:void(0)" @click="createNewSite">Add a new site...</a>
                    </td>
                </tr>
            </table>
            <table class="table table-striped columns-holder" v-if="model.totalFound > 0 || model.nameFilter || model.tags.length">
                <thead>
                    <tr>
                        <th class="w-300">
                            <div>
                                <a href="javascript:void(0)" 
                                        v-bind:class="{ asc: model.sortField == 'Name' && model.sortState?.order == 'Asc', desc: model.sortField == 'Name' && model.sortState?.order == 'Desc' }" 
                                        @click="sort('Name')">Name</a>
                                &nbsp;
                                <a href="javascript:void(0)" @click="model.nameFilterShown = true">
                                    <img src="../../../public/icons8-filter-empty-16.png" v-if="!model.nameFilter" />
                                    <img src="../../../public/icons8-filter-16.png" v-if="model.nameFilter" />
                                </a>
                            </div>
                            <div class="name-filter-container" v-if="model.nameFilterShown">
                                <div class="name-filter-container-inner">
                                    <div class="name-filter-container-input-container">Site name contains: <input type="text" v-model="model.nameFilter" @input="() => loadSiteData()"/></div>
                                    <div class="name-filter-btn-bar">
                                        <button class="btn btn-primary" @click="clearNameFilter">Clear</button>
                                        <button class="btn btn-primary" @click="model.nameFilterShown = false">OK</button>                                    
                                    </div>
                                </div>
                            </div>
                        </th>
                        <th class="w-300">Description</th>
                        <th class="w-300">
                            <a href="javascript:void(0)" 
                                v-bind:class="{ asc: model.sortField == 'LaunchedOn' && model.sortState?.order == 'Asc', desc: model.sortField == 'LaunchedOn' && model.sortState?.order == 'Desc' }"   
                                @click="sort('LaunchedOn')">Launched On</a></th>
                        <th class="w-300" v-if="model.isAdmin">
                            User Name
                        </th>
                        <th class="w-150">Is Active</th>
                        <th class="tags-column" v-if="!model.isAdmin">
                            <div>Tags <span v-if="model.tags.length">({{model.tags.length}})</span> &nbsp;
                                <a href="javascript:void(0)" @click="openTagsFilter">
                                    <img src="../../../public/icons8-filter-empty-16.png" v-if="!model.tags.length" />
                                    <img src="../../../public/icons8-filter-16.png" v-if="model.tags.length" />
                                </a>
                            </div>
                            <div class="tags-select-list-container">
                                <TagsSelectList ref="tagsFilterRef" :tagIds="model.tags" :onTagsChanged="applySelectedTags" :hideSelectedTags="true" />
                            </div>
                        </th>
                        <th>Actions</th>
                    </tr>
                </thead>
            </table>

            <div class="site-list-container" v-if="model.totalFound > 0">
                <table class="table table-striped sites-table">
                    <tbody>
                        <tr v-for="site in model.sites" :key="site" class="site-row">
                            <td class="w-300">{{site.name}}</td>
                            <td class="w-300">{{site.description}}</td>
                            <td class="w-300 centered-cell">{{site.launchedOn}}</td>
                            <td class="w-300" v-if="model.isAdmin">
                                <router-link :to="{ path: '/user-profile/' + site.owner.id }">{{site.owner.name}}</router-link>
                            </td>
                            <td class="w-150 centered-cell">
                                <input type="checkbox" v-model="site.isActive" :disabled="true" />
                            </td>
                            <td class="tags-column" v-if="!model.isAdmin">
                                <div class="tags-holder">
                                   <Tag v-for="tag in site.tags" :key="tag.id" :tagData="tag" />
                                </div>
                            </td>
                            <td class="centered-cell">
                                <span><a href="javascript:void(0)" @click="toggleSiteStatus(site.id)">{{ site.isActive ? 'Turn Off' : 'Turn On&nbsp;'}}</a> | </span>
                                <span v-if="!model.isAdmin"><a href="javascript:void(0)" @click="updateSite(site.id)">Update</a> | </span>
                                <span><a href="javascript:void(0)" @click="deleteSite(site.id)">Delete</a></span>
                                <span v-if="!model.isAdmin && site.landingPage"><span> | </span><a href="javascript:void(0)"  @click="() => browseLinkClick(site)">Browse</a></span>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
            <div class="no-sites-message" v-if="model.totalFound === 0">
                No sites found.
            </div>
        </div>
    </div>
</template>
<style scoped>
    h3 {
        margin: 40px 0 0;
    }

    .centered-cell {
        text-align: center;
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
    .sites-table {
        width: calc(100% - 10px);
        margin-left: 5px;
        margin-right: 5px;
    }
    .site-row:hover {
        background-color: lightskyblue;
    }

    .columns-holder {
        width: calc(100% - 10px);
        height: 40px;
        margin-left: 5px;
        margin-right: 5px;
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