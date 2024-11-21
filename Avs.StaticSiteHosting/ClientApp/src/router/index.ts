import { createRouter, createWebHashHistory } from 'vue-router';
import HomePage from './../components/home/HomePage.vue';
import Dashboard from './../components/dashboard/Dashboard.vue';
import Login from './../components/identity/Login.vue';
import Register from './../components/identity/Register.vue';
import Registered from './../components/identity/Registered.vue';
import MyProfile from './../components/my-profile/MyProfile.vue';
import Help from './../components/help/Help.vue';
import Tags from './../components/tags/Tags.vue';
import Conversations from './../components/conversations/Conversations.vue';
import CreateOrUpdateSite from '../components/site-management/CreateOrUpdateSite.vue';
import CreateOrUpdatePage from '../components/page-editor/CreateOrUpdatePage.vue';
import UserProfile from '../components/user-profile/UserProfile.vue';
import EventLog from '../components/event-log/EventLog.vue';
import AppSettings from '../components/settings/AppSettings.vue';
import Reports from '../components/reports/Reports.vue';

const router = createRouter({
    history: createWebHashHistory(),
    routes: [
        {
            path: '/',
            name: 'home',
            component: HomePage
        },
        {
            path: '/dashboard',
            name: 'dashboard',
            component: Dashboard
        },
        {
            path: '/login',
            name: 'login',
            component: Login,
            props: true
        },
        {
            path: '/register',
            component: Register
        },
        {
            path: '/registered',
            component: Registered
        },
        {
            path: '/profile',
            component: MyProfile
        },
        {
            path: '/help',
            component: Help
        },
        { 
            path: '/tags', 
            component: Tags 
        },
        {
            path: '/conversations',
            component: Conversations
        },
        {
            name: 'create-site',
            path: '/sites/create',
            component: CreateOrUpdateSite
        },
        {
            name: 'update-site',
            path: '/sites/update/:siteId',
            component: CreateOrUpdateSite,
            props: true
        },
        /*
         siteId: model.siteId, 
            contentId: file.id,
            contentName: file.name,
            contentDestinationPath: file.destinationPath, 
            uploadSessionId: model.uploadSessionId 
        */
        {
            name: 'page-editor',
            path: '/page-editor',
            component: CreateOrUpdatePage, 
            props: true 
        },
        {
            path: '/user-profile/:userId',
            component: UserProfile,
            props: true
        },
        {
            path: '/event-log',
            component: EventLog
        },
        { 
            path: '/settings', 
            component: AppSettings 
        },
        { 
            path: '/reports', 
            component: Reports 
        }
    ]
});

export default router;
