<script setup lang="ts">
    import { computed, provide } from 'vue';
    import ApiClient from './services/api-client';
    import AuthService from './services/auth-service';
    import UserNotificationService from './services/user-notification-service';
    import { API_CLIENT, AUTH_SERVICE, CONFIG_PROVIDER, USER_NOTIFICATIONS_SERVICE } from './common/diKeys';
    import {  useRouter } from 'vue-router';
import { ConfigProvider } from './services/confg-provider';
    
    const router = useRouter();
    
    const appClasses = computed(() => {
      const identityPages = ['login', 'register', 'registered'];
      const currentRoute = router.currentRoute?.value.path;
     
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
    });
    
    const authService = new AuthService();
    const apiClient = new ApiClient(authService);
    const clientConfigProvider = new ConfigProvider(apiClient);
    const userNotificationService = new UserNotificationService(authService);

    provide(API_CLIENT, apiClient);
    provide(AUTH_SERVICE, authService);
    provide(USER_NOTIFICATIONS_SERVICE, userNotificationService);
    provide(CONFIG_PROVIDER, clientConfigProvider);
</script>

<template>
  <div class="router-view-container" v-bind:class="appClasses">
    <router-view></router-view>
  </div>
</template>

<style scoped>
    .router-view-container {
        font-family: Avenir, Helvetica, Arial, sans-serif;
        -webkit-font-smoothing: antialiased;
        -moz-osx-font-smoothing: grayscale;
        color: #2c3e50;
        height: 100%;
    }
    .default-padding{
        padding-top: 150px;
    }
</style>