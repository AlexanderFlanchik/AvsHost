import { createApp } from 'vue';

import App from './App.vue';
import VueAwesomePaginate from "vue-awesome-paginate";
import "vue-awesome-paginate/dist/style.css";
//@ts-ignore
import VueSelect from 'vue-select';
import AuthService from './services/auth-service';
import router from './router/index.ts';
//import Multiselect from '@vueform/multiselect';
const authService = new AuthService();

const app = createApp(App)
    .component("v-select", VueSelect);
    //.component("Multiselect", Multiselect);

app.use(VueAwesomePaginate);

router.beforeEach((to, _from, next) => {
    if (to.path != '/login' &&
        to.path != '/register' &&
        to.path != '/registered' &&
        !authService.isAuthenticated()) {
            next({ name: 'login', query: { returnUrl : to.fullPath } });
    } else {
        next();
    }
});

app.use(router);
app.mount('#app');
