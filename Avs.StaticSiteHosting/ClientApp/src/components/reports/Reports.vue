<script setup lang="ts">
import UserInfo from '../layout/UserInfo.vue';
import NavigationMenu from '../layout/NavigationMenu.vue';
import SiteGeneralInfoFilter from './SiteGeneralInfoFilter.vue';
import SiteEventsFilter from './SiteEventsFilter.vue';
import ReportPreview from './ReportPreview.vue';
import SitesAndDatesFilter from './SitesAndDatesFilter.vue';
import reportFilterTypeNames from './ReportFilterTypes';
import FileDownloadSection from './FileDownloadSection.vue';
import { Subject } from 'rxjs';
import { onBeforeUnmount, onMounted, reactive, watch } from 'vue';

interface ReportType {
    name: string;
    value: number;
    filterType: string;
}

interface ReportsModel {
    reportType: ReportType;
    filterHasChanged$: any;
    filterHasChangedSubject: Subject<any>;
    generatePreviewSubject: Subject<any>;
    reportPreviewShown: boolean;
    reportTypes: Array<ReportType>;
    currentFilterValue: any;
}

const model = reactive<ReportsModel>({
    reportType: { name: 'General Sites Report', value: 0, filterType: reportFilterTypeNames.SiteGeneralInfoFilter },
    filterHasChanged$: null,
    filterHasChangedSubject: new Subject<any>(),
    generatePreviewSubject: new Subject<any>(),
    reportPreviewShown: true,
    reportTypes: [
        { name: 'General Sites Information', value: 0, filterType: reportFilterTypeNames.SiteGeneralInfoFilter },
        { name: 'Site Events', value: 1, filterType: reportFilterTypeNames.SiteEventsFilter },
        { name: 'Visit Statistics', value: 2, filterType: reportFilterTypeNames.SitesAndDatesFilter },
        { name: 'Site Errors', value: 3, filterType: reportFilterTypeNames.SitesAndDatesFilter }
    ],
    currentFilterValue: null
});

onMounted(() => 
    model.filterHasChanged$ = model.filterHasChangedSubject.subscribe((filter: any) => {
        model.currentFilterValue = filter;
        setTimeout(() => model.generatePreviewSubject.next({ reportType: model.reportType, filter, isPreviewReady: true }), 10);
        model.reportPreviewShown = true;
    })
);

onBeforeUnmount(() => model.filterHasChanged$?.unsubscribe());

const onReportTypeChanged = () => {
    if (!model.currentFilterValue) {
        return;
    }
    
    const filterNotChanged = model.currentFilterValue._filterType === model.reportType.filterType;

    if (filterNotChanged) {
        model.generatePreviewSubject.next({ reportType: model.reportType, filter: model.currentFilterValue });
    } else {
        model.reportPreviewShown = false;
    }
};

watch(() => model.reportType, onReportTypeChanged);
</script>
<template>
    <div class="content-block-container">
        <div class="general-page-title">
            <span>Reports</span>
        </div>
        <UserInfo />
        <NavigationMenu />
        <div class="reports-content">
            <div class="reports-top-container">
                <div class="report-type-container">
                    <span>Report type:</span>
                    <div class="v-select-outer">
                    <v-select v-model="model.reportType" label="name" :clearable="false" :filterable="false"
                            :options="model.reportTypes">
                        <template #option="{ name }">
                            <div>{{name}}</div>
                        </template>

                        <template #selected-option="{ name }">
                            <div class="option-selected">{{name}}</div>
                        </template>
                    </v-select>
                    </div>
                </div>
                <FileDownloadSection :generateReport$="<Subject<any>>model.generatePreviewSubject" v-if="model.reportPreviewShown" />
            </div>
            <SiteGeneralInfoFilter v-if="model.reportType.value === 0" :filterHasChanged$="<Subject<any>>model.filterHasChangedSubject" />
            <SiteEventsFilter v-if="model.reportType.value === 1" :filterHasChanged$="<Subject<any>>model.filterHasChangedSubject" />
            <SitesAndDatesFilter ref="siteVisitsFilter" v-if="model.reportType.value === 2" :filterHasChanged$="<Subject<any>>model.filterHasChangedSubject" />
            <SitesAndDatesFilter ref="siteErrorsFilter" v-if="model.reportType.value === 3" :filterHasChanged$="<Subject<any>>model.filterHasChangedSubject" />
        
            <div class="preview-report-container" v-bind:class="{'preview-report-container-higher': model.reportType.value === 0 }">
                <ReportPreview :generatePreview$="<Subject<any>>model.generatePreviewSubject" v-if="model.reportPreviewShown" />
            </div>
        </div>
    </div>
</template>
<style scoped>
    .reports-content {
        background-color: azure;
        height: calc(100vh - 166px);
    }
    .reports-top-container {
        display: flex;
        justify-content: space-between;
        align-items: center;
    }
    .v-select-outer {
        flex-grow: 2;
    }
    .report-type-container {
        margin-top: 5px;
        margin-left: 5px;
        padding: 5px;
        display: flex;
        width: 550px;
        align-items: center;
        gap: 5px;
    }
    .report-type-container span {
        font-weight: bold;
    }
    .report-preview-container {
        background-color: lightgray;
    }
    .preview-report-container {
        width: 1080px;
        margin-top: 10px;
        margin-inline: auto;
        height: calc(100vh - 336px);
        overflow-y: auto;
    }
    .preview-report-container-higher {
        height: calc(100vh - 278px) !important;
    }
</style>