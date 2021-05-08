<template>
    <li>
        <span>
            <b-icon icon="caret-right-fill" v-if="!section.expanded && section.sections && section.sections.length" class="expand-collapse-btn" @click="expand"></b-icon>
            <b-icon icon="caret-down-fill" v-if="section.expanded && section.sections && section.sections.length" class="expand-collapse-btn" @click="collapse"></b-icon>
            <a href="#" @click="click" v-bind:class="{'section-selected' : section.selected }">{{section.name}}</a>
        </span>
        <ul class="list-without-bullets" v-if="section.expanded && section.sections && section.sections.length">
            <HelpSection v-for="sec of section.sections" :section="sec" :key="sec"></HelpSection>
        </ul>
    </li>
</template>
<script>
    export default {      
        name: "HelpSection",
        props: {
            section: Object,             
        },
        methods: {
            expand: function () {
                this.section.expanded = true;
            },
            collapse: function () {
                this.section.expanded = false;
            },
            click: function () {                
                if (this.section && this.section.clickHandler) {
                    this.section.clickHandler();                    
                }                
            }
        }
    }
</script>
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