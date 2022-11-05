<template>
    <div>
        <ViewContentDialog ref="view-content-dialog" />    
        <EditContentDialog ref="edit-content-dlg" /> 
        <div v-if="uploaded.length > 0">
            <table class="table table-hover uploaded-files-table">
                <thead>
                    <tr>
                        <th>Name</th>
                        <th>Size, kB</th>
                        <th>Uploaded Date</th>
                        <th>Update Date</th>
                        <th>&nbsp;</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="file in uploaded" :key="file">
                        <td>
                            <span class="badge badge-secondary" v-if="file.isNew">New</span>&nbsp;
                            {{file.fullName()}}
                        </td>
                        <td>{{file.size}}</td>
                        <td>{{formatDate(file.uploadedAt)}}</td>
                        <td>{{formatDate(file.updateDate)}}</td>
                        <td>
                            <span v-if="!file.isNew"><a :href="downloadLink(file)">Download</a> | </span>
                            <span v-if="file.isEditable"><a href="javascript:void(0)" @click="edit(file)">Edit</a> | </span>
                            <span v-if="file.isViewable"><a href="javascript:void(0)" @click="view(file)">View</a> | </span>
                            <span><a href="javascript:void(0)" @click="deleteContentItem(file)">Remove</a></span>
                        </td>
                    </tr>
                </tbody>
            </table> 
        </div>    
        <div class="no-content-message" v-if="uploaded.length === 0">
            No content files found. Upload or create some content to continue.
        </div>
    </div>
</template>
<script>
    import { formatDate } from '../../common/DateFormatter';
    import ViewContentDialog from './ViewContentDialog.vue';
    import EditContentDialog from '../EditContentDialog.vue';
    
    export default {
        props: {
            uploaded: Object,
            uploadSessionId: String,
            openPageEditor: Object
        },
        methods: {
            formatDate: function (date) {
                return formatDate(date);
            },

            downloadLink: function (file) {
                if (!file || !file.id) {
                    return '#';
                }

                let url = `api/sitedetails/content-get?contentItemId=${file.id}&__accessToken=${this.$authService.getToken()}`;                
                return url;
            },

            deleteContentItem: async function (file) {
                if (!confirm(`Are you sure to delete file ${file.name}?`)) {
                    return;
                }

                let deleteUrl = `api/sitedetails/content-delete`;
                if (file.id) {
                    deleteUrl += `?contentItemId=${file.id}`;
                } else {
                    deleteUrl += `?contentItemName=${file.fullName()}&uploadSessionId=${this.uploadSessionId}`;
                }

                let response = await this.$apiClient.deleteAsync(deleteUrl);
              
                try {
                    if (response.status == 200) {
                        let itemDeleted = this.uploaded.find(i => i.name == file.name);
                        let ix = this.uploaded.indexOf(itemDeleted);
                        this.uploaded.splice(ix, 1);
                    }
                } catch {
                    alert(`Unable to delete ${file.name}. Server error or the file does not exist.`);
                }
            },

            edit: async function (file) {
                if (file.name.endsWith(".html") && this.openPageEditor) {
                    await this.openPageEditor(file);
                } else {
                    let fileResponse = await this.$apiClient.getAsync(
                        `api/sitedetails/content-get?contentItemId=${file.id}&__accessToken=${this.$authService.getToken()}`
                    );

                    let dlgSubject = this.$refs["edit-content-dlg"].showDialog(file.name, fileResponse.data);
                    dlgSubject.subscribe(async (newContent) => {
                        let updateResponse = await this.$apiClient.putAsync(`api/sitedetails/content-edit/${file.id}`, { content: newContent });
                        if (updateResponse.status == 200) {
                            let updatedItem = this.uploaded.find(i => i.id == file.id);
                            if (updatedItem) {
                                updatedItem.updateDate = new Date();
                            }
                        }
                    });    
                }
            },

            view: function (file) {
                this.$refs["view-content-dialog"].open(file.name,
                    `api/sitedetails/content-get?contentItemId=${file.id}&maxWidth=600&__accessToken=${this.$authService.getToken()}`);
            }
        },
        components: {
            ViewContentDialog,
            EditContentDialog
        }
    }
</script>
<style scoped>
    .uploaded-files-table {
        width: 99%;
        background-color: white;
    }
    .uploaded-files-table th {
        background-color: darkgrey;
    }
    .no-content-message {
        background-color: white;
        height: 150px;
        max-width: 550px;
        padding-top: 60px;
        font-weight: bold;
        text-align: center;
    }
</style>