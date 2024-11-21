"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var vue_1 = require("vue");
var App_vue_1 = require("./App.vue");
var vue_awesome_paginate_1 = require("vue-awesome-paginate");
require("vue-awesome-paginate/dist/style.css");
var vue_select_1 = require("vue-select");
var router_1 = require("./router");
var auth_service_1 = require("./services/auth-service");
var authService = new auth_service_1.default();
var app = (0, vue_1.createApp)(App_vue_1.default)
    .component("v-select", vue_select_1.default);
app.use(vue_awesome_paginate_1.default);
router_1.default.beforeEach(function (to, _from, next) {
    if (to.path != '/login' &&
        to.path != '/register' &&
        to.path != '/registered' &&
        !authService.isAuthenticated()) {
        next({ name: 'login', query: { returnUrl: to.fullPath } });
    }
    else {
        next();
    }
});
app.use(router_1.default);
app.mount('#app');
//# sourceMappingURL=main.js.map