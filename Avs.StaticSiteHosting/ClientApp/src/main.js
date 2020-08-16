import Vue from 'vue';
import VueRouter from 'vue-router';
import App from './App.vue';
import Dashboard from './components/Dashboard.vue';
import Login from './components/Login.vue';
import AuthService from './services/AuthService';
import Register from './components/Register.vue';
import Registered from './components/Registered.vue';

Vue.use(VueRouter);
Vue.config.productionTip = false;

const authService = new AuthService();
Vue.prototype.$authService = authService;

const routes = [
    { path: '/', component: Dashboard },
    { path: '/login', component: Login, props: true },
    { path: '/register', component: Register },
    { path: '/registered', component: Registered }
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