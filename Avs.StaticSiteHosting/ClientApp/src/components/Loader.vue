<template>
    <div class="loader" v-if="processing"></div>
</template>
<script>
    export default {
        props: {
            processing: Object
        }
    }
</script>
<style scoped>
    .loader {
        margin-top: 7px;
        margin-left: 6px;
        border: 4px solid #007bff;
        border-top: 4px solid navy; 
        border-bottom: 4px solid Navy;
        border-radius: 50%;
        width: 20px;
        height: 20px;
        animation: spin 2s linear infinite;
    }
  
    @keyframes spin {
        0% { transform: rotate(0deg); }
        100% { transform: rotate(360deg); }
    }
</style>