<template>
  <div class="content-block-container">
      <div class="general-page-title">
         <img src="../../../ClientApp/public/dashboard.png" /> &nbsp;
         <span>Dashboard</span>
      </div>
      <UserInfo />
      <NavigationMenu />
      <div class="site-list-container">

      </div>
  </div>
</template>

<script>
    import UserInfo from '@/components/UserInfo.vue';
    import NavigationMenu from '@/components/NavigationMenu.vue';

    export default {
        props: {
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

</style>