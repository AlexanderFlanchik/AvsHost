<template>
    <div class="report-preview-container">
        <div class="report-preview-container-internal" v-html="previewReport" v-if="previewReport && previewReport.length">
        </div>
    </div>
</template>
<script>
    export default {
        props: {
            generatePreview$: Object
        },
        data: function() {
            return {
                preview$: null,
                previewId: null,
                previewReport: null,
                loading: false
            };
        },
        mounted: function() {
            this.preview$ = this.generatePreview$?.subscribe(({ filter, reportType }) => {                
                this.loading = true;
                this.$apiClient.postAsync('api/reportpreview', { filter, reportType: reportType.value })
                    .then(response => {
                        const htmlPreview = response.data;
                        this.previewReport = htmlPreview;
                        this.loading = false;
                    }).catch(err => {
                        console.log(err);
                        this.previewReport = "Unable to generate report preview due to server error.";
                        this.loading = false;
                    });
            });
        },
        beforeDestroy: function () {
            this.preview$?.unsubscribe();
        },
    }
</script>
<style scoped>
    .report-preview-container {
        padding: 2px;
    }
</style>