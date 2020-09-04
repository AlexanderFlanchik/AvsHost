<template>
  <div class="content-block-container">
     <div class="general-page-title">
         <img src="../../../ClientApp/public/dashboard.png" /> &nbsp;
         <span>Dashboard</span>
      </div>
      <UserInfo />
      <NavigationMenu />
      <div class="dashboard-container">
          <div class="two-columns-content">
              <div class="column-left">
                  Your sites:&nbsp;<strong>{{totalFound}}</strong>
              </div>
              <div class="column-right">
                  <div class="two-columns-content" v-if="totalFound > 0">
                      <div class="column-left">
                          <b-pagination v-model="page"
                                        :total-rows="totalFound"
                                        :per-page="pageSize"
                                        size="sm"
                                        @input="pageChanged">
                          </b-pagination>
                      </div>
                      <div class="column-right">
                          <router-link to="/sites/create">Add a new site...</router-link>
                      </div>
                  </div>
                  <router-link to="/sites/create" v-if="totalFound === 0">Add a new site...</router-link>
              </div>
          </div>

          <table class="table table-striped columns-holder" v-if="totalFound > 0">
              <thead>
                  <tr>
                      <th class="w-300">Name</th>
                      <th class="w-300">Description</th>
                      <th class="w-300">Launched On</th>
                      <th class="w-150">Is Active</th>
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
                          <td class="w-150">
                              <input type="checkbox" v-model="site.isActive" disabled="disabled" />
                          </td>
                          <td>
                              <a href="javascript:void(0)">Turn Off</a> |
                              <router-link :to="{ path: '/sites/update/' + site.id }">Update</router-link> |
                              <a href="javascript:void(0)">Delete</a>
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
    import UserInfo from '@/components/UserInfo.vue';
    import NavigationMenu from '@/components/NavigationMenu.vue';

    const Site = function (siteData) {
        const self = this;
        self.id = siteData.id;
        self.name = siteData.name;
        self.description = siteData.description;
        self.launchedOn = siteData.launchedOn;
        self.isActive = siteData.isActive;
    };

    export default {
        data: function () {
            return {                
                sites: [],
                page: 1,
                pageSize: 10,
                totalFound: 0,
                sortOrder: 'Asc',
                sortField: '',
                loadSiteData: async function () {
                    let apiUrl = `api/sites?page=${this.page}&pageSize=${this.pageSize}`;

                    if (this.sortField) {
                        apiUrl += `&sortOrder=${this.sortOrder}&sortField=${this.sortField}`;
                    }

                    this.$apiClient.getAsync(apiUrl).then((response) => {
                        console.log(response);
                        let siteRows = response.data.map(s => new Site(s));
                        this.sites = siteRows;
                        this.totalFound = Number(response.headers["total-rows-amount"]);
                        console.log('Rows total: ' + this.totalFound);
                    });
                }
            }
        },
        props: {
        },
        mounted: function () {
            this.loadSiteData();
        },
        methods: {
            pageChanged: function () {
                console.log('page changed: ' + this.page);
                this.loadSiteData();
            }           
        },
        components: {
            UserInfo,
            NavigationMenu
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
    .dashboard-container {
        background-color: azure;
    }

    .two-columns-content {
        padding: 5px;
    }

    .column-left {
        width: 20%;
        float: left;
    }

    .column-right {
        width: 80%;
        float: left;
        text-align: right;
    }

    .w-300 {
        width: 300px;
    }

    .w-350 {
        width: 350px;
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

    .site-list-container {
        height: calc(100vh - 280px);
        overflow-y: auto;
    }

    .site-list-footer {
        position: absolute;
        height: 50px;
        background-color: azure;
        bottom: 25px;
        z-index: 9999;
        margin-top: 5px;
        padding-bottom: 5px;
        width: calc(100% - 50px);
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