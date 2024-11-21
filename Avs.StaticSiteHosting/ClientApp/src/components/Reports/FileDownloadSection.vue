<script setup lang="ts">
import { inject, onBeforeUnmount, onMounted, reactive } from 'vue';
import { AUTH_SERVICE } from '../../common/diKeys';
import { Subject } from 'rxjs';

interface FileDownloadSectionModel {
    reportType: any;
    reportParameters: string;
    filterReadySubscription: any;
    hideSection: boolean;
}

const formAction = "api/report";
const authService = inject(AUTH_SERVICE)!;
const props = defineProps<{ generateReport$: Subject<any>}>();

const model = reactive<FileDownloadSectionModel>({
    reportType: null,
    reportParameters: "{}",
    filterReadySubscription: null,
    hideSection: false
});

onMounted(() => {
    model.filterReadySubscription = props.generateReport$?.subscribe((report: any) => {
        if (report.isPreviewReady) {
            model.reportType = report.reportType?.value;
            model.reportParameters = JSON.stringify(Object.assign({}, report.filter));
        }
        model.hideSection = !report.isPreviewReady;
    });
});

onBeforeUnmount(() => model.filterReadySubscription?.unsubscribe());

const pdfLinkClick = () => {
    const form = document.getElementById("download-form")! as HTMLFormElement;
    form.action = `${formAction}/${model.reportType}/pdf?__accessToken=${authService.getToken()}`;
                
    form.submit();
};

const xlsxLinkClick = () => {
    const form = document.getElementById("download-form") as HTMLFormElement;
    form.action = `${formAction}/${model.reportType}/xlsx?__accessToken=${authService.getToken()}`;

    form.submit();
};

</script>
<template>
    <div class="file-download-links-container" v-if="!model.hideSection">
        <form id="download-form" method="POST" class="download-form">
            <a href="javascript:void(0)" @click="pdfLinkClick"><img src="./../../../public/icons8-pdf-32.png"> PDF</a>
            <a href="javascript:void(0)" @click="xlsxLinkClick"><img src="./../../../public/icons8-microsoft-excel-32.png"> Excel</a>
            <input type="hidden" name="reportParameters" v-model="model.reportParameters" />
        </form>
    </div>
</template>
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