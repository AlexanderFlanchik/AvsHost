<script setup lang="ts">
import { inject, ref } from 'vue';
import { ContentFile } from '../../common/ContentFile';
import { API_CLIENT, AUTH_SERVICE } from '../../common/diKeys';
import EditContentDialog from './EditContentDialog.vue';
import ViewContentDialog from './ViewContentDialog.vue';
import { formatDate } from '../../common/DateFormatter';
import { NewCreatedContentHolder } from '../../services/NewCreatedContentHolder';

interface UploadedContentListProps {
    uploaded: Array<ContentFile>;
    uploadSessionId: string;
    openPageEditor?: (file: ContentFile) => Promise<void>;
}

const authService = inject(AUTH_SERVICE)!;
const apiClient = inject(API_CLIENT)!;

const editContentDialogRef = ref<typeof EditContentDialog | null>(null);
const viewContentDialogRef = ref<typeof ViewContentDialog | null>(null);

const props = defineProps<UploadedContentListProps>();
const downloadLink = (file: ContentFile) => {
    if (!file) {
        return 'javascript:void(0)';
    }

    if (!file.id) {
        const fileContent = new NewCreatedContentHolder().getContent(file.name) as any;
        const blob = new Blob([fileContent], { type: "text/html" });
        return URL.createObjectURL(blob);
    }
                
    let url = `api/sitecontent?contentItemId=${file.id}&__accessToken=${authService.getToken()}`;                
    return url;
};

const deleteContentItem = async (file: ContentFile) => {
    if (!confirm(`Are you sure to delete file ${file.name}?`)) {
        return;
    }

    let deleteUrl = `api/sitecontent`;
    if (file.id) {
        deleteUrl += `?contentItemId=${file.id}`;
    } else {
        deleteUrl += `?contentItemName=${file.fullName()}&uploadSessionId=${props.uploadSessionId}`;
    }

    const response = await apiClient.deleteAsync(deleteUrl) as any;
              
    try {
        if (response.status == 200) {
            const itemDeleted = props.uploaded!.find((i: ContentFile) => i.name == file.name)!;
            const ix = props.uploaded!.indexOf(itemDeleted);
            props.uploaded!.splice(ix, 1);
        }
    } catch {
        alert(`Unable to delete ${file.name}. Server error or the file does not exist.`);
    }
};

const edit = async (file: ContentFile) => {
    if (file.name.endsWith(".html") && props.openPageEditor) {
        await props.openPageEditor(file);
    } else {
        const fileResponse = await apiClient.getAsync(
            `api/sitecontent/get?contentItemId=${file.id}&__accessToken=${authService.getToken()}`
        ) as any;

        const dlgSubject = editContentDialogRef.value?.showDialog(file.name, fileResponse.data, undefined, file.cacheDuration);
        dlgSubject.subscribe(async ({ content, cacheDuration } : { content: string, cacheDuration: string | undefined }) => {
            
            const updateResponse = await apiClient.putAsync(`api/sitecontent/content-edit/${file.id}`, { content, cacheDuration }) as any;
                if (updateResponse.status == 200) {
                    const updatedItem = props.uploaded!.find((i: ContentFile) => i.id == file.id);

                    if (updatedItem) {
                        updatedItem.updateDate = new Date();
                        updatedItem.cacheDuration = cacheDuration;
                    }
                }
            });    
    }
};

const view = (file: ContentFile) => 
    viewContentDialogRef.value?.open(file.name,
    `api/sitecontent?contentItemId=${file.id}&maxWidth=600&__accessToken=${authService.getToken()}`);
</script>
<template>
    <div>
        <ViewContentDialog ref="viewContentDialogRef" />    
        <EditContentDialog ref="editContentDialogRef" /> 
        <div v-if="props.uploaded?.length > 0">
            <table class="table table-hover uploaded-files-table">
                <thead>
                    <tr>
                        <th style="min-width: 150px;">Name</th>
                        <th>Size, kB</th>
                        <th>Uploaded Date</th>
                        <th>Update Date</th>
                        <th>&nbsp;</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="file in props.uploaded" :key="<PropertyKey>file.id!">
                        <td>
                            <span class="badge badge-secondary" v-if="file.isNew">New</span>&nbsp;
                            {{file.fullName()}}
                        </td>
                        <td>{{file.size}}</td>
                        <td>{{formatDate(file.uploadedAt)}}</td>
                        <td>{{formatDate(file.updateDate)}}</td>
                        <td>
                            <span><a :href="downloadLink(file)" :download="file.name">Download</a> | </span>
                            <span v-if="file.isEditable"><a href="javascript:void(0)" @click="edit(file)">Edit</a> | </span>
                            <span v-if="file.isViewable"><a href="javascript:void(0)" @click="view(file)">View</a> | </span>
                            <span><a href="javascript:void(0)" @click="deleteContentItem(file)">Remove</a></span>
                        </td>
                    </tr>
                </tbody>
            </table> 
        </div>    
        <div class="no-content-message" v-if="uploaded?.length === 0">
            No content files found. Upload or create some content to continue.
        </div>
    </div>
</template>
<style scoped>
    .uploaded-files-table td {
        text-align: center;
    }
    .uploaded-files-table {
        width: 99%;
        background-color: white;
    }
    .uploaded-files-table th {
        background-color: darkgrey;
    }
    .no-content-message {
        margin-top: 20px;
        padding: 10px;
        height: 35px;
        line-height: 35px;
        background-color: white;
        max-width: 550px;
        font-weight: bold;
        text-align: center;
    }
</style>