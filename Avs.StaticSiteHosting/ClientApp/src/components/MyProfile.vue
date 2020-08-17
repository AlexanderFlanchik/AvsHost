<template>
    <div class="my-profile-content">
        <div class="my-profile-title">
            <img src="../../public/profile.png" /> Profile
        </div>
        <UserInfo />
        <NavigationMenu />
    </div>
</template>
<script>
    import UserInfo from '@/components/UserInfo.vue';
    import NavigationMenu from '@/components/NavigationMenu.vue';

    export default {
        components: {
            UserInfo,
            NavigationMenu
        }
    }
</script>
<style>
    .my-profile-content {
        padding-top: 15px;
        padding-left: 25px;
        padding-right: 25px;
    }

    .my-profile-title {
        font-size: 46pt;
        font-family: Garamond;
        color: white;
        font-weight: bold;
    }
</style>