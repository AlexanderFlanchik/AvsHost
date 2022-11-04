<template>
    <div class="content-block-container">
        <div class="general-page-title">
            <img src="../../public/help.png" />
            <span>Help</span>
        </div>
        <UserInfo />
        <NavigationMenu />
        <div class="help-content">
           <div class="sections-container">
               <ul class="list-without-bullets">                   
                   <HelpSection v-for="section in sections" :key="section" :section="section"></HelpSection>
               </ul>
           </div>
           <div class="help-topic-content-container">
               <div class="help-topic-content left-line">
                   <div v-html="currentTopic.content"></div>
               </div>
               <div class="help-topic-navigation left-line">
                   <span v-if="topics > 0">Page <strong>{{currentTopic.ordinalNo}}/{{topics}}</strong></span>
                   <span class="current-page-and-buttons-container">
                       <button class="btn btn-primary btn-sm" v-if="currentTopic.ordinalNo > 1" @click="movePrevious">Previous</button> &nbsp;
                       <button class="btn btn-primary btn-sm" v-if="currentTopic.ordinalNo && topics > 0 && currentTopic.ordinalNo != topics" @click="moveNext">Next</button>
                   </span>
               </div>
           </div>
        </div>
    </div>
</template>
<script>
    import UserInfo from '@/components/UserInfo.vue';
    import NavigationMenu from '@/components/NavigationMenu.vue';
    import HelpSection from '@/components/HelpSection.vue';

    const Section = function (o) {
        const self = this;
        const clickHandlerFn = o.clickHandlerFn(); // fn (sId) => ...

        let sectionList = [];

        self.id = o.id;
        self.name = o.name;
        self.ordinalNo = o.ordinalNo;
        self.isRoot = o.isRoot;
        self.expanded = false;
        self.selected = false;

        self.setSectionList = function (lst) {
            sectionList = lst;
        };
        
        self.clickHandler = function () {
            clickHandlerFn && clickHandlerFn(self.id, self.isRoot);
            if (self.isRoot) {
                return;
            }

            for (let i = 0; i < sectionList.length; i++) {
                let sc = sectionList[i];
                sc.selected = sc.id == self.id;
            }
        };

        if (o.sections && o.sections.length) {
            self.sections = [];
            for (var s of o.sections) {
                if (clickHandlerFn) {
                    s.clickHandlerFn = o.clickHandlerFn;
                }
                let sc = new Section(s);
                self.sections.push(sc);
                sc.setSectionList(self.sections);
            }
        }
    };

    export default {
        data: function () {
            return {
                sections: [],
                topics: 0,
                currentTopic: {
                    id: '',
                    isRoot: false,
                    content: '<div>Select a section to view.</div>',
                    ordinalNo: 1
                }
            };
        },
        mounted: function () {
            this.$apiClient.getAsync("api/help/sections")
                .then((response) => {
                    let sections = response && response.data || [];
                    for (let s of sections) {
                        s.clickHandlerFn = () => {
                            return (sId, isRoot) => {
                                this.currentTopic.id = sId;
                                this.currentTopic.isRoot = isRoot;

                                this.loadHelpTopic();
                            };
                        };

                        let section = new Section(s);
                        this.sections.push(section);
                        section.setSectionList(this.sections);
                    }
                });              
        },
        methods: {
            moveNext: function () {
                if (this.currentTopic.ordinalNo < this.topics) {
                    this.currentTopic.ordinalNo++;
                    this.loadHelpTopic();
                }
            },

            movePrevious: function () {
                if (this.currentTopic.ordinalNo > 1) {
                    this.currentTopic.ordinalNo--;
                    this.loadHelpTopic();
                }
            },

            loadHelpTopic: async function () {
                if (this.currentTopic.isRoot) {
                    return;
                }

                let topicUrl = `api/help/GetHelpTopic?helpSectionId=${this.currentTopic.id}&ordinalNo=${this.currentTopic.ordinalNo}`;
                let resp = await this.$apiClient.getAsync(topicUrl, { 'content-type': 'text/html' });
                let topicContent = resp.data;

                if (topicContent && topicContent.length) {
                    this.currentTopic.content = topicContent;
                } else {
                    this.currentTopic.content = 'No content found. Please choose another help section to view.';
                }

                this.topics = Number(resp.headers["total-topics"]);
            }
        },
        components: {
            UserInfo,
            NavigationMenu,
            HelpSection
        }
    }
</script>
<style scoped>
    .help-content {
        background-color: azure;      
        height: calc(100% - 155px);
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