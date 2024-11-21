<script setup lang="ts">
 import { reactive, inject, onMounted } from 'vue';
 import { API_CLIENT } from './../../common/diKeys';
 import Section from './Section';
 import HelpSection from './HelpSection.vue';
 import NavigationMenu from './../layout/NavigationMenu.vue';
 import UserInfo from './../layout/UserInfo.vue';

 interface HelpPageModel {
    sections: Array<Section>;
    topics: number;
    currentTopic: {
        id: string;
        isRoot: boolean;
        content: string;
        ordinalNo: number;
    }
 }

 const model = reactive<HelpPageModel>({
    sections: [],
    topics: 0,
    currentTopic: {
        id: '',
        isRoot: false,
        content: '<div>Select a section to view.</div>',
        ordinalNo: 1
    }
 });

 const apiClient = inject(API_CLIENT)!;

onMounted(() => {
    apiClient.getAsync("api/help/sections")
        .then((response: any) => {
            const sections = response && response.data || [];
            for (const s of sections) {
                s.clickHandlerFn = () => {
                    return (sId: string, isRoot: boolean) => {
                        model.currentTopic.id = sId;
                        model.currentTopic.isRoot = isRoot;
                        loadHelpTopic();
                    };
                };

                const section = new Section(s);
                model.sections.push(section);
                section.setSectionList(model.sections);
            }
        });
}); 

const loadHelpTopic = async () => {
    if (model.currentTopic.isRoot) {
         return;
    }

    const topicUrl = `api/help/GetHelpTopic?helpSectionId=${model.currentTopic.id}&ordinalNo=${model.currentTopic.ordinalNo}`;
    const resp: any = await apiClient.getAsync(topicUrl, { 'content-type': 'text/html' });
    const topicContent = resp.data;

    if (topicContent && topicContent.length) {
        model.currentTopic.content = topicContent;
    } else {
        model.currentTopic.content = 'No content found. Please choose another help section to view.';
    }

    model.topics = Number(resp.headers["total-topics"]);
};

const moveNext = () => {
    if (model.currentTopic.ordinalNo < model.topics) {
        model.currentTopic.ordinalNo++;
        loadHelpTopic();
    }
};

const movePrevious = () => {
    if (model.currentTopic.ordinalNo > 1) {
        model.currentTopic.ordinalNo--;
        loadHelpTopic();
    }
};
</script>
<template>
        <div class="content-block-container">
        <div class="general-page-title">
            <img src="./../../../public/help.png" />
            <span>Help</span>
        </div>
        <UserInfo />
        <NavigationMenu />
        <div class="help-content">
           <div class="sections-container">
               <ul class="list-without-bullets">                   
                   <HelpSection v-for="section in model.sections" :key="section.id" :section="<Section>section"></HelpSection>
               </ul>
           </div>
           <div class="help-topic-content-container">
               <div class="help-topic-content left-line">
                   <div v-html="model.currentTopic.content"></div>
               </div>
               <div class="help-topic-navigation left-line">
                   <span v-if="model.topics > 0">Page <strong>{{model.currentTopic.ordinalNo}}/{{model.topics}}</strong></span>
                   <span class="current-page-and-buttons-container">
                       <button class="btn btn-primary btn-sm" v-if="model.currentTopic.ordinalNo > 1" @click="movePrevious">Previous</button> &nbsp;
                       <button class="btn btn-primary btn-sm" v-if="model.currentTopic.ordinalNo && model.topics > 0 && model.currentTopic.ordinalNo != model.topics" @click="moveNext">Next</button>
                   </span>
               </div>
           </div>
        </div>
    </div>
</template>
<style scoped>
    .help-content {
        background-color: azure;      
        height: calc(100vh - 166px);
    }

    .sections-container {
        width: 350px;
        float: left;
        padding-top: 10px;        
    }

    .help-topic-content-container {
        float: left;
        width: calc(100% - 355px);
        height: 100%;
    }

    .help-topic-content {
        padding-left: 5px;
        padding-top: 10px;
        height: calc(100% - 40px);
        overflow-y: auto;       
    }

    .left-line {
        border-left-color: navy;
        border-left-style: solid;
        border-left-width: 2px;
    }

    .help-topic-navigation {
        padding-top: 2px;
        height: 37px;
        text-align: right;
        padding-right: 5px;
    }

    .current-page-and-buttons-container {
        margin-left: 5px;
    }

    .current-page-and-buttons-container > button {
        height: 32px;
    }
</style>