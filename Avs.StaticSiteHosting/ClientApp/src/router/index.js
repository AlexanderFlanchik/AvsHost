"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var vue_router_1 = require("vue-router");
var HomePage_vue_1 = require("./../components/home/HomePage.vue");
var Dashboard_vue_1 = require("./../components/dashboard/Dashboard.vue");
var Login_vue_1 = require("./../components/identity/Login.vue");
var Register_vue_1 = require("./../components/identity/Register.vue");
var Registered_vue_1 = require("./../components/identity/Registered.vue");
var MyProfile_vue_1 = require("./../components/my-profile/MyProfile.vue");
var Help_vue_1 = require("./../components/help/Help.vue");
var Tags_vue_1 = require("./../components/tags/Tags.vue");
var Conversations_vue_1 = require("./../components/conversations/Conversations.vue");
var CreateOrUpdateSite_vue_1 = require("../components/site-management/CreateOrUpdateSite.vue");
var CreateOrUpdatePage_vue_1 = require("../components/page-editor/CreateOrUpdatePage.vue");
var UserProfile_vue_1 = require("../components/user-profile/UserProfile.vue");
var EventLog_vue_1 = require("../components/event-log/EventLog.vue");
var AppSettings_vue_1 = require("../components/settings/AppSettings.vue");
var Reports_vue_1 = require("../components/reports/Reports.vue");
var router = (0, vue_router_1.createRouter)({
    history: (0, vue_router_1.createWebHistory)(),
    routes: [
        {
            path: '/',
            name: 'home',
            component: HomePage_vue_1.default
        },
        {
            path: '/dashboard',
            name: 'dashboard',
            component: Dashboard_vue_1.default
        },
        {
            path: '/login',
            name: 'login',
            component: Login_vue_1.default,
            props: true
        },
        {
            path: '/register',
            component: Register_vue_1.default
        },
        {
            path: '/registered',
            component: Registered_vue_1.default
        },
        {
            path: '/profile',
            component: MyProfile_vue_1.default
        },
        {
            path: '/help',
            component: Help_vue_1.default
        },
        {
            path: '/tags',
            component: Tags_vue_1.default
        },
        {
            path: '/conversations',
            component: Conversations_vue_1.default
        },
        {
            name: 'create-site',
            path: '/sites/create',
            component: CreateOrUpdateSite_vue_1.default
        },
        {
            name: 'update-site',
            path: '/sites/update/:siteId',
            component: CreateOrUpdateSite_vue_1.default,
            props: true
        },
        {
            name: 'page-editor',
            path: '/page-editor',
            component: CreateOrUpdatePage_vue_1.default,
            props: true
        },
        {
            path: '/user-profile/:userId',
            component: UserProfile_vue_1.default,
            props: true
        },
        {
            path: '/event-log',
            component: EventLog_vue_1.default
        },
        {
            path: '/settings',
            component: AppSettings_vue_1.default
        },
        {
            path: '/reports',
            component: Reports_vue_1.default
        }
    ]
});
exports.default = router;
//# sourceMappingURL=index.js.map