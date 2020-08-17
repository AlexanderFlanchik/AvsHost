<template>
  <div class="dashboard-content">
      <div class="dashboard-title">
         <img src="../../../ClientApp/public/dashboard.png" /> Dashboard
      </div>
      <UserInfo />
  </div>
</template>

<script>
    import UserInfo from '@/components/UserInfo.vue';
    export default {
        props: {
        },
        components: {
            UserInfo
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

.dashboard-content {
    padding-top: 15px;
    padding-left: 25px;
    padding-right: 25px;
}

.dashboard-title {
    font-size: 46pt;
    font-family: Garamond;
    color: white;
    font-weight: bold;
}
</style>
