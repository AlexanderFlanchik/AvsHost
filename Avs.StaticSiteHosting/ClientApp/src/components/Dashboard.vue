<template>
  <div class="content-block-container">
      <div class="general-page-title">
         <img src="../../../ClientApp/public/dashboard.png" /> &nbsp;
         <span>Dashboard</span>
      </div>
      <UserInfo />
      <NavigationMenu />
      <div class="dashboard-container">
          <div class="site-list-container">
              <div class="before-table-content">
                  <div class="before-table-content-left">
                      <strong>Your sites</strong>
                  </div>
                  <div class="before-table-content-right">
                      <a href="javascript:void(0)">Add a new site..</a>
                  </div>
              </div>
              <table class="table table-striped">
                  <thead>
                      <tr>
                          <th>Name</th>
                          <th>Description</th>
                          <th>Launched On</th>
                          <th>Is Active</th>
                          <th>&nbsp;</th>
                      </tr>
                  </thead>
                  <tbody>
                      <tr v-for="site in sites">
                          <td>{{site.name}}</td>
                          <td>{{site.description}}</td>
                          <td>{{site.launchedOn}}</td>
                          <td>
                              <input type="checkbox" v-model="site.isActive" disabled="disabled" />
                          </td>
                          <td>
                              <a href="javascript:void(0)">Turn Off</a> |
                              <a href="javascript:void(0)">Update</a> |
                              <a href="javascript:void(0)">Delete</a>
                          </td>
                      </tr>
                  </tbody>
              </table>
              <div class="after-table-content">

              </div>
          </div>
      </div>
  </div>
</template>

<script>
    import UserInfo from '@/components/UserInfo.vue';
    import NavigationMenu from '@/components/NavigationMenu.vue';

    const Site = function (siteData) {
        const self = this;
        self.name = siteData.name;
        self.description = siteData.description;
        self.launchedOn = siteData.launchedOn;
        self.isActive = siteData.isActive;
    };

    export default {
        data: function () {
            return {
                sites: [],
                totalFound: 0
            }
        },
        props: {
        },
        mounted: function () {
            this.$apiClient.getAsync('api/sites').then((response) => {
                console.log(response);
                let siteRows = response.data.map(s => new Site(s));
                this.sites = siteRows;
                this.totalFound = siteRows.length;
            });
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

    .dashboard-container{
        background-color: azure;

    }

    .before-table-content {
        padding: 5px;
    }

    .before-table-content-left{
        width: 20%;
        float: left;
    }
    .before-table-content-right {
        width: 80%;
        float: left;
        text-align: right;
    }
</style>