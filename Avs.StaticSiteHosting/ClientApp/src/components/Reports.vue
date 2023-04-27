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
                    <v-select ref="reportTypeSelect" v-model="reportType" label="name" :clearable="false" :filterable="false"
                            :options="reportTypes"
                              @input="onReportTypeChanged">
                        <template slot="option" slot-scope="option">
                            <div>{{option.name}}</div>
                        </template>

                        <template slot="selected-option" slot-scope="option">
                            <div class="option-selected">{{option.name}}</div>
                        </template>
                    </v-select>
                    </div>
                </div>
                <FileDownloadSection :generateReport$="generatePreviewSubject" v-if="reportPreviewShown" />
            </div>
            <SiteGeneralInfoFilter v-if="reportType.value === 0" :filterHasChanged$="filterHasChangedSubject" />
            <SiteEventsFilter v-if="reportType.value === 1" :filterHasChanged$="filterHasChangedSubject" />
            <SitesAndDatesFilter ref="siteVisitsFilter" v-if="reportType.value === 2" :filterHasChanged$="filterHasChangedSubject" />
            <SitesAndDatesFilter ref="siteErrorsFilter" v-if="reportType.value === 3" :filterHasChanged$="filterHasChangedSubject" />

            <div class="preview-report-container" v-bind:class="{'preview-report-container-higher': reportType.value === 0 }">
                <ReportPreview :generatePreview$="generatePreviewSubject" v-if="reportPreviewShown" />
            </div>
        </div>
    </div>
</template>
<script lang="ts">
    import UserInfo from '@/components/UserInfo.vue';
    import NavigationMenu from '@/components/NavigationMenu.vue';
    import SiteGeneralInfoFilter from './Reports/SiteGeneralInfoFilter.vue';
    import SiteEventsFilter from './Reports/SiteEventsFilter.vue';
    import ReportPreview from './Reports/ReportPreview.vue';
    import SitesAndDatesFilter from './Reports/SitesAndDatesFilter.vue';
    import reportFilterTypeNames from './Reports/ReportFilterTypes';
    import FileDownloadSection from './Reports/FileDownloadSection.vue';

    import { Subject } from 'rxjs';

    export default {
        data: function () {
            return {
                reportType: { name: 'General Sites Report', value: 0, filterType: reportFilterTypeNames.SiteGeneralInfoFilter },
                filterHasChanged$: null,
                filterHasChangedSubject: new Subject(),
                generatePreviewSubject: new Subject(),
                reportPreviewShown: true,
                reportTypes: [
                    { name: 'General Sites Information', value: 0, filterType: reportFilterTypeNames.SiteGeneralInfoFilter },
                    { name: 'Site Events', value: 1, filterType: reportFilterTypeNames.SiteEventsFilter },
                    { name: 'Visit Statistics', value: 2, filterType: reportFilterTypeNames.SitesAndDatesFilter },
                    { name: 'Site Errors', value: 3, filterType: reportFilterTypeNames.SitesAndDatesFilter }
                ],
                currentFilterValue: null
            };
        },
        mounted: function () {
            this.filterHasChanged$ = this.filterHasChangedSubject.subscribe(filter => {
                this.currentFilterValue = filter;
                setTimeout(() => this.generatePreviewSubject.next({ reportType: this.reportType, filter }), 10);
                this.reportPreviewShown = true;
            });
        },
        beforeDestroy: function() {
            this.filterHasChanged$.unsubscribe();
        },
        methods: {
            onReportTypeChanged: function () {
                if (!this.currentFilterValue) {
                    return;
                }

                let filterNotChanged = this.currentFilterValue._filterType === this.reportType.filterType;

                if (filterNotChanged) {
                    this.generatePreviewSubject.next({ reportType: this.reportType, filter: this.currentFilterValue });
                } else {
                    this.reportPreviewShown = false;
                }
            }
        },
        components: {
            UserInfo,
            NavigationMenu,
            SiteGeneralInfoFilter,
            SiteEventsFilter,
            ReportPreview,
            SitesAndDatesFilter,
            FileDownloadSection
        }
    }
</script>
<style scoped>
    .reports-content {
        background-color: azure;
        height: calc(100% - 155px);
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