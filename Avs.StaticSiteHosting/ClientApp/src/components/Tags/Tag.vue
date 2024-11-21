<script setup lang="ts">
import { onMounted, reactive } from 'vue';
const fontName = "18px serif";

export interface TagData {
    id: string;
    name: string;
    textColor: string;
    backgroundColor: string;
}

interface TagModel {
    tagLength: number;
    side: number;
    circleRadius: number;
    x: number;
    y: number;
}

const props = defineProps<{ tagData: TagData }>();
const model = reactive<TagModel>({
    tagLength: 0,
    side: 20,
    circleRadius: 8,
    x: 0,
    y: 0
});

const getTagWidth = (tagBody: string, font: string) => {
    const canvas = document.createElement('canvas');
    const context = canvas.getContext('2d')!;
    context.font = font;
    const result = Math.floor(context.measureText(tagBody).width) + 2 * model.circleRadius + model.side;

    return result;
};

onMounted(() => {
    model.x = model.side + model.circleRadius + 5;
    model.y = model.side + model.circleRadius / 2;
    model.tagLength = getTagWidth(props.tagData.name, fontName);
});

const wrapperStyle = () => `width: ${model.tagLength}px;`;
const tagViewBox = () => `0 0 ${model.tagLength} ${2 * model.side}`;
const points = () => `0 ${model.side}, ${model.side} 0, ${model.tagLength} 0, ${model.tagLength} ${2 * model.side}, ${model.side} ${2 * model.side}`;
const textStyle = () => {
    return {
        font: fontName,
        fill: props.tagData.textColor
    };
};
</script>
<template>
    <div :style="wrapperStyle()">
        <svg :viewBox="tagViewBox()" xmlns="http://www.w3.org/2000/svg">
            <polygon :points="points()" :fill="props.tagData.backgroundColor" :stroke="props.tagData.backgroundColor"></polygon>
            <circle :cx="model.side" :cy="model.side" :r="model.circleRadius" fill="white" stroke="navy"></circle>
            <text :x="model.x" :y="model.y" :style="textStyle()">{{props.tagData.name}}</text>
        </svg>
    </div>
</template>
<style scoped>

</style>