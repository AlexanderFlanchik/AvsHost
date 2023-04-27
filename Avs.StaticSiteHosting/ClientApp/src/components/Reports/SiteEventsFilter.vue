<template>
    <div class="site-filter-container">
        <span>Select a site:</span>
        <v-select ref="siteNameSelect" 
                  v-model="site" label="name" 
                  :clearable="false" 
                  :filterable="false" 
                  :options="siteOptions" 
                  @search="onSearch" 
                  @input="onInput">
            <template slot="no-options">
                <div class="no-options-placeholder">Start enter a site name</div>
            </template>

            <template slot="option" slot-scope="option">
                <div>{{option.name}}</div>
            </template>

            <template slot="selected-option" slot-scope="option">
                <div class="option-selected">{{option.name}}</div>
            </template>
        </v-select>
    </div>
</template>
<script lang="ts">
    import reportFilterTypeNames from './ReportFilterTypes';

    export default {
        props: {
            filterHasChanged$: Object
        },
        data: function() {
            return {
                site: null,
                siteOptions: [],
            }
        },
        methods: {
            onInput: function (value) {
                this.filterHasChanged$.next({ _filterType: reportFilterTypeNames.SiteEventsFilter, siteId: value.id });
            },
            onSearch: function (search, loading) {
                if (search && search.length) {
                    loading(true);
                    this.$apiClient.getAsync(`api/sitesearch/${search}`)
                        .then((response) => {
                            loading(false);
                            this.siteOptions = response.data.sites;
                        }).catch((e) => {
                            console.log(e);
                            loading(false);
                        });
                }
            }
        }
    }
</script>
<style scoped>
    .site-filter-container {
        max-width: 650px;
        padding: 5px;
    }
</style>