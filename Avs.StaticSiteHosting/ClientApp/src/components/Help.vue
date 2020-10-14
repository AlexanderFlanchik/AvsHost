<template>
    <div class="content-block-container">
        <div class="general-page-title">
            <img src="../../public/help.png" /> &nbsp;
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
           <div class="help-topic-content">
               <div  v-html="currentTopic.content"></div>
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
        
        self.id = o.id;
        self.name = o.name;
        self.ordinalNo = o.ordinalNo;
        self.expanded = false;

        self.clickHandler = function () {
            clickHandlerFn && clickHandlerFn(self.id);
        };

        if (o.sections && o.sections.length) {
            self.sections = [];
            for (var s of o.sections) {
                if (clickHandlerFn) {
                    s.clickHandlerFn = o.clickHandlerFn;
                }
                self.sections.push(new Section(s));
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
                            return (sId) => {
                                this.currentTopic.id = sId;
                                this.loadHelpTopic();
                            };
                        };
                        let section = new Section(s);
                        this.sections.push(section);
                    }
                });              
        },
        methods: {
            loadHelpTopic: async function () {
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

    .help-topic-content {
        float: left;
        padding-left: 5px;
        padding-top: 10px;
        width: calc(100% - 355px);
        height: 100%;
        overflow-y: auto;
    }
</style>