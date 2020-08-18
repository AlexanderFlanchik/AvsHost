<template>
    <div class="nav-menu-container">
        <ul class="navigation-menu">
            <li class="navigation-menu-item">
                <router-link to="/" class="navigation-menu-item-link" active-class="link-active" exact>Dashboard</router-link>
            </li>
            <li class="navigation-menu-item">
                <router-link to ="/profile" class="navigation-menu-item-link" active-class="link-active">My Profile</router-link>
            </li>
            <li class="navigation-menu-item">
                <router-link to="/help" class="navigation-menu-item-link" active-class="link-active">Help...</router-link>
            </li>
        </ul>
    </div>
</template>
<script>
    export default {

    }
</script>
<style>
    .link-active {       
        color: red !important;
        font-weight: bold;
    }
    .nav-menu-container {

    }

    .navigation-menu {
        list-style-type: none;
        margin: 0;
        padding: 0;
        color: white;
        overflow: hidden;
        background-color: darkblue;
    }

    .navigation-menu-item {
        float: left;
    }

        .navigation-menu-item:hover {
            background-color: #060343;
        }

        .navigation-menu-item-link {
            text-decoration: none;
            display: block;
            color: white;
            text-align: center;
            padding: 16px;
        }

        .navigation-menu-item-link:hover {
            color: yellow !important;
            text-decoration: none;
        }
</style>