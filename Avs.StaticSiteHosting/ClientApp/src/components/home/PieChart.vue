<script setup lang="ts">
    import { Subject } from 'rxjs';
    import { onMounted, ref } from 'vue';
    import * as highcharts from 'highcharts';

    const props = defineProps({
        title: String,
        seriesName: String
    });

    const data$ = new Subject();
    const chart = ref<HTMLElement | null>(null);
    const loadData = (data: any) => data$.next(data);

    defineExpose({ loadData });
    
    onMounted(() => {
        data$.subscribe((data: any) => {
            const chartDiv = chart.value;
            if (!chartDiv) {
                return;
            }

            // @ts-ignore
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
                        text: props.title as string
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
                        series: {
                            allowPointSelect: true,
                            cursor: 'pointer',
                            dataLabels: {
                                enabled: true,
                                format: '<b>{point.name}</b>: {point.percentage:.1f} %'
                            }
                        }
                    },
                    series: [{
                        name: props.seriesName!.toString(),
                        colorByPoint: true,
                        data: data || []
                    }]
                })
        });

    });
</script>                                                                                                                                                                                                                                                                                                   
<template>
    <div ref="chart">
    </div>
</template>
<style scoped>

</style>