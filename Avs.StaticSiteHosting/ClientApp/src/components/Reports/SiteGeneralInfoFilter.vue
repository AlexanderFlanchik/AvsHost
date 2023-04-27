<template>
    <div class="no-op"></div>
</template>
<script lang="ts">
    import reportFilterTypeNames from './ReportFilterTypes';
    
    export default {
        props: {
            filterHasChanged$: Object
        },
        mounted: function () {
            console.log("SiteGeneralInfoFilter: ready");
            setTimeout(() =>
                this.filterHasChanged$.next({ _filterType: reportFilterTypeNames.SiteGeneralInfoFilter }), 10);
        }
    }
</script>
<style scoped>
    .no-op {
        display: none;
    }
</style>