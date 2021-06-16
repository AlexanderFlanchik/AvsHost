<template>
    <div ref="chart">
    </div>
</template>
<script>
    import * as highcharts from 'highcharts';
    import { Subject } from 'rxjs';

    export default {
        props: {
            title: String,
            seriesName: String
        },
        data: function () {
            return {                
                data$: new Subject()
            };
        },

        mounted: function () {
            this.data$.subscribe((data) => {
                let chartDiv = this.$refs.chart;
                highcharts.chart(chartDiv, {
                    chart: {
                        type: 'pie',
                        width: 450,
                        plotBackgroundColor: null,
                        plotBorderWidth: null,
                        plotShadow: false,
                        backgroundColor: 'azure'
                    },
                    title: {
                        text: this.title
                    },
                    tooltip: {
                        pointFormat: '{series.name}: <b>{point.percentage:.1f}%</b>'
                    },
                    accessibility: {
                        point: {
                            valueSuffix: '%'
                        }
                    },
                    plotOptions: {
                        pie: {
                            allowPointSelect: true,
                            cursor: 'pointer',
                            dataLabels: {
                                enabled: true,
                                format: '<b>{point.name}</b>: {point.percentage:.1f} %'
                            }
                        }
                    },
                    series: [{
                        name: this.seriesName,
                        colorByPoint: true,
                        data: data || []
                    }]
                });
            });
        },

        methods: {
            loadData: function (data) {
                this.data$.next(data);               
            }
        }
    }
</script>
<style scoped>

</style>