import Vue from 'vue';
import vSelect from 'vue-select';
import 'bootstrap';
import 'bootstrap/dist/css/bootstrap.min.css';
import 'bootstrap-vue/dist/bootstrap-vue.min.css';
import 'vue-select/dist/vue-select.css';
import { BootstrapVue, BootstrapVueIcons } from 'bootstrap-vue';
import VueRouter from 'vue-router';
import App from './App.vue';
import Dashboard from './components/Dashboard.vue';
import Login from './components/Login.vue';
import AuthService from './services/AuthService';
import Register from './components/Register.vue';
import Registered from './components/Registered.vue';
import MyProfile from './components/MyProfile.vue';
import CreateOrUpdateSite from './components/CreateOrUpdateSite.vue';
import Help from './components/Help.vue';
import UserProfile from './components/UserProfile.vue';
import Conversations from './components/Conversations.vue';
import EventLog from './components/EventLog.vue';
import HomePage from './components/HomePage.vue';
import CreateOrUpdatePage from './components/CreateOrUpdatePage.vue';
import AppSettings from './components/AppSettings.vue';
import ApiClient from './services/ApiClient';
import { UserNotificationService } from './services/UserNotificationService';
import Tags from './components/Tags.vue';
import Reports from './components/Reports.vue';
import 'vue-multiselect/dist/vue-multiselect.min.css';

Vue.use(BootstrapVue);
Vue.use(BootstrapVueIcons);
Vue.component('v-select', vSelect);

Vue.use(VueRouter);
Vue.config.productionTip = false;

const authService = new AuthService();
Vue.prototype.$authService = authService;
Vue.prototype.$apiClient = new ApiClient(authService);
Vue.prototype.$userNotificationService = new UserNotificationService(authService);

const routes = [
    { path: '/', component: HomePage },
    { path: '/dashboard', component: Dashboard },
    { path: '/login', component: Login, props: true },
    { path: '/register', component: Register },
    { path: '/registered', component: Registered },
    { path: '/profile', component: MyProfile },
    { path: '/help', component: Help },
    { path: '/tags', component: Tags },
    { path: '/conversations', component: Conversations },
    { name: 'create-site', path: '/sites/create', component: CreateOrUpdateSite },
    { name: 'update-site', path: '/sites/update/:siteId', component: CreateOrUpdateSite, props: true },
    {
        name: 'page-editor',
        path: '/page-editor',
        component: CreateOrUpdatePage, 
        props: true 
    },
    { path: '/user-profile/:userId', component: UserProfile, props: true },
    { path: '/event-log', component: EventLog },
    { path: '/settings', component: AppSettings },
    { path: '/reports', component: Reports }
];

const router = new VueRouter({ routes });

router.beforeEach((to, _from, next) => {
    if (to.path != '/login' &&
        to.path != '/register' &&
        to.path != '/registered' &&
        !authService.isAuthenticated()) {
            next({ path: '/login', query: { returnUrl : to.fullPath } });
    } else {
        next();
    }
});

new Vue({
    el: '#app',
    router,
    render: h => h(App),    
});