<script setup lang="ts">
const props = defineProps<{ processing: boolean }>();
</script>
<template>
<div class="loader" v-if="props.processing"></div>
</template>
<style scoped>
    .loader {
        margin-top: 2px;
        margin-left: 6px;
        border: 4px solid #007bff;
        border-top: 4px solid navy; 
        border-bottom: 4px solid Navy;
        border-radius: 50%;
        width: 10px;
        height: 10px;
        animation: spin 2s linear infinite;
    }
  
    @keyframes spin {
        0% { transform: rotate(0deg); }
        100% { transform: rotate(360deg); }
    }
</style>