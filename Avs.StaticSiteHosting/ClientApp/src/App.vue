<template>
    <div id="app" v-bind:class="appClasses">
        <router-view />
    </div>
</template>

<script>
    const identityPages = ['login', 'register', 'registered'];
export default {
  name: 'App',
  components: {
        },
        data: function () {
            return {
            };
        },
        computed: {
            appClasses: function () {
                let currentRoute = this.$route.fullPath;
                let applied = false;
                for (let page of identityPages) {
                    if (currentRoute && currentRoute.startsWith('/' + page)) {
                        applied = true;
                        break;
                    }
                }

                return {
                    'default-padding': applied
                };
            }
        }
}
</script>

<style>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  color: #2c3e50;
}
    .default-padding{
        padding-top: 150px;
    }
</style>
