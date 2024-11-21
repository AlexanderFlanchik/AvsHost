<script setup lang="ts">
import { Subject } from 'rxjs';
import { inject, onBeforeUnmount, onMounted, reactive } from 'vue';
import { API_CLIENT } from '../../common/diKeys';

interface ReportPreviewModel {
    preview$: any;
    previewId: string | null;
    previewReport: string | null;
    loading: boolean
}

const apiClient = inject(API_CLIENT)!;
const props = defineProps<{ generatePreview$: Subject<any> }>();
const model = reactive<ReportPreviewModel>({
    preview$: null,
    previewId: null,
    previewReport: null,
    loading: false
});

onMounted(() => {
    model.preview$ = props.generatePreview$?.subscribe(({ filter, reportType }: { filter: any, reportType: { value: string }}) => {                
        model.loading = true;
        apiClient.postAsync('api/reportpreview', { filter, reportType: reportType.value })
            .then((response: any) => {
                const isPreviewReady = Boolean(response.headers["x-report-preview-ready"]);
                if (!isPreviewReady) {
                    props.generatePreview$.next({ isPreviewReady: false });
                }
                const htmlPreview = response.data;
                model.previewReport = htmlPreview;
                model.loading = false;
            }).catch((err: Error) => {
                console.log(err);
                model.previewReport = "Unable to generate report preview due to server error.";
                model.loading = false;
            });
    });
});

onBeforeUnmount(() => model.preview$?.unsubscribe());

</script>
<template>
    <div class="report-preview-container">
        <div class="report-preview-container-internal" v-html="model.previewReport" v-if="model.previewReport && model.previewReport.length">
        </div>
    </div>
</template>
<style scoped>
    .report-preview-container {
        padding: 2px;
    }
</style>