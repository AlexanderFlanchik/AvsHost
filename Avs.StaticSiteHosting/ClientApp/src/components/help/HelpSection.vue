<script setup lang="ts">
import { IconKind } from '../../common/IconKind';
import ExpandCollapseIcon from '../ExpandCollapseIcon.vue';
import Section from './Section';

const props = defineProps<{ section: Section }>();
const expand = () => props.section.expanded = true;
const collapse = () => props.section.expanded = false;
const click = () => {
    if (props.section.clickHandler) {
        props.section.clickHandler();
    }
};
</script>
<template>
    <li>
        <span>
            <ExpandCollapseIcon :kind="IconKind.Expanded" v-if="props.section && !props.section.expanded && props.section.sections && props.section.sections.length" class="expand-collapse-btn" :click="expand" />
            <ExpandCollapseIcon :kind="IconKind.Collapsed" v-if="props.section && props.section.expanded && props.section.sections && props.section.sections.length" class="expand-collapse-btn" :click="collapse" />
            <a href="javascript:void(0)" @click="click" v-bind:class="{'section-selected' : props.section.selected }">{{props.section.name}}</a>
        </span>
        <ul class="list-without-bullets" v-if="props.section && props.section.expanded && props.section.sections && props.section.sections.length">
            <HelpSection v-for="sec of props.section && props.section.sections" :section="sec" :key="sec.name"></HelpSection>
        </ul>
    </li>
</template>
<style scoped>
    .expand-collapse-btn {
        cursor: pointer;
    }
    .section-selected {
        font-style: italic;
        color: sandybrown;
        text-decoration: underline;
    }
</style>