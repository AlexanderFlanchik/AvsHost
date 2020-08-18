<template>
    <div class="content-block-container">
        <div class="general-page-title">
            <img src="../../public/help.png"/> &nbsp; 
            <span>Help</span>
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
</style>