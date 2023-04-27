<template>
    <div class="file-download-links-container">
        <form id="download-form" method="POST" class="download-form">
            <a href="javascript:void(0)" @click="pdfLinkClick"><img src="../../../public/icons8-pdf-32.png"> PDF</a>
            <a href="javascript:void(0)" @click="xlsxLinkClick"><img src="../../../public/icons8-microsoft-excel-32.png"> Excel</a>
            <input type="hidden" name="reportParameters" v-model="reportParameters" />
        </form>
    </div>
</template>
<script>
    const formAction = "api/report";

    export default {
        props: {
            generateReport$: Object
        },
        data: function () {
            return {
                reportType: null,
                reportParameters: "{}",
                filterReadySubscription: null
            };
        },
        mounted: function() {
            this.filterReadySubscription = this.generateReport$?.subscribe(report => {
                this.reportType = report.reportType?.value;
                this.reportParameters = JSON.stringify(Object.assign({}, report.filter));
            });
        },
        beforeDestroy: function() {
            this.filterReadySubscription?.unsubscribe();
        },
        methods: {
            pdfLinkClick: function() {
                let form = document.getElementById("download-form");
                form.action = `${formAction}/${this.reportType}/pdf?__accessToken=${this.$authService.getToken()}`;
                
                form.submit();
            },
            xlsxLinkClick: function() {
                let form = document.getElementById("download-form");
                form.action = `${formAction}/${this.reportType}/xlsx?__accessToken=${this.$authService.getToken()}`;

                form.submit();
            }
        }
    }
</script>
<style scoped>
    .file-download-links-container {
        padding: 5px;
        width: 300px;
    }
    .download-form {
        display: flex;
        gap: 10px;
        justify-content: center;
    }
</style>