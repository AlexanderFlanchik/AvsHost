<script setup lang="ts">
import { IconKind } from '../common/IconKind';

const props = defineProps<{ kind: IconKind, click: () => void }>();
</script>
<template>
    <i class="icon-internal" @click="props.click">
        <svg v-if="props.kind == IconKind.Expanded" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="24" height="24">
            <polygon points="6,8 12,16 18,8" fill="navy"/>
        </svg>
        <svg v-if="props.kind === IconKind.Collapsed" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="24" height="24">
            <polygon points="6,16 12,8 18,16" fill="navy"/>
        </svg>
    </i>
</template>

<style scoped>
</style>