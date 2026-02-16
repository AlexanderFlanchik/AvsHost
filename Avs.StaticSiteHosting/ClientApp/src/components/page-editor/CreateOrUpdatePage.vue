<script setup lang="ts">
import { computed, inject, onMounted, reactive, ref } from 'vue';
import { SiteContextManager } from '../../services/SiteContextManager';
import { GenericElement, Html, Link, Metadata, Script } from './html-elements';
import { API_CLIENT, AUTH_SERVICE } from '../../common/diKeys';
import { mapTree } from './htmlTreeMapper';
//@ts-ignore
import { v4 as uuid } from 'uuid';
import ContentResourceEditor from './ContentResourceEditor.vue';
import ElementEditor from './ElementEditor.vue';
import getAvailableTags from './TagsProvider';
import { ContentFile } from '../../common/ContentFile';
import { delay } from 'rxjs';
import { useRouter } from 'vue-router';
import EditContentDialog from '../site-management/EditContentDialog.vue';
import Loader from '../Loader.vue';
import { PageContextProvider } from './PageContext';
import { NewCreatedContentHolder } from '../../services/NewCreatedContentHolder';
import TimeSpanInput from '../site-management/TimeSpanInput.vue';

const editIconSrc = '../icons8-edit-16.png';
const removeIconSrc = '../icons8-remove-16.png';
const addScriptOrStylesheetSrc = '../icons8-document-16.png';    
const addNewIconSrc = '../icons8-add-16.png';
const marginLeft1 = '10px';
const marginLeft2 = '20px';
const newResourcePlaceHolder = '%NEW_RESOURCE%';
const existResourcePlaceHolder = '%EXIST_RESOURCE%';

const siteContextManager = new SiteContextManager();
const router = useRouter();

const contentResourceEditorRef = ref<typeof ContentResourceEditor | null>(null);
const elementEditorRef = ref<typeof ElementEditor | null>(null);
const editContentDialogRef = ref<typeof EditContentDialog | null>(null);

const apiClient = inject(API_CLIENT)!;
const authService = inject(AUTH_SERVICE)!;
const newPageCreated = ref(false);

interface CreateOrUpdatePageModel {
    siteId: string | null;
    contentId: string |  null;
    contentName: string | null;
    contentDestinationPath: string | null;
    uploadSessionId: string | null;
    previewSessionId: string | null;
    htmlTree: Html | null;
    error: string | null;
    processing: boolean;
    cacheDuration: string | undefined;
}

const model = reactive<CreateOrUpdatePageModel>({
    siteId: null,
    contentId: null,
    contentName: null,
    contentDestinationPath: null,
    uploadSessionId: null,
    previewSessionId: null,
    htmlTree: null,
    error: null,
    processing: false,
    cacheDuration: undefined
});

const createEmptyTree = () => {
    model.htmlTree = new Html();
    model.htmlTree.head.title = "New Page";
};

const parseChildren = (element: GenericElement, contentNodes: HTMLCollection) => {
    for (const o of contentNodes) {
        const e = new GenericElement();
        e.id = o.id;
        e.parent = element;
        e.tag = o.nodeName.toLowerCase();
        e.innerHtml = o.innerHTML;
                   
        const attributes = o.attributes || [];
        for (const attr of attributes) {
            e.attributes.set(attr.name, attr.value);
        }

        if (o.children && o.children.length) {
            parseChildren(e, o.children);
        }
                    
        element.children.push(e);
    }
};

const processLoadedContent = (contentDocument: Document) => {
    const frameHead = contentDocument.head;
    const frameBody = contentDocument.body;

    model.htmlTree = new Html();
    const head = model.htmlTree.head;

    for (const node of frameHead.children) {
        const tagName = node.nodeName.toLowerCase();
                    
        switch (tagName) {
            case "title": {
                head.title = node.textContent ?? '';
                break;
            }
            case "style": {
                if (node.textContent) {
                    head.styles.push(node.textContent);
                }
                break;
            }
            case "script": {
                const script = new Script();
                const typeAttr = node.attributes.getNamedItem('type'),
                srcAttr = node.attributes.getNamedItem('src');
                script.src = srcAttr != null ? srcAttr.value : undefined;
                script.type = typeAttr != null ? typeAttr.value : undefined;
                
                if (!script.src) {
                    script.body = node.textContent!;
                }

                head.scripts.push(script);
                break;
            }
            case "meta": {            
                const metadata = new Metadata();
                const nameAttr = node.attributes.getNamedItem('name'),
                    charsetAttr = node.attributes.getNamedItem('charset'),
                    contentAttr = node.attributes.getNamedItem('content');
                metadata.name = nameAttr != null ? nameAttr.value : undefined;
                metadata.charset = charsetAttr != null ? charsetAttr.value : undefined;
                metadata.content = contentAttr != null ? contentAttr.value : undefined;
                head.metadatas.push(metadata);
                break;
            }
            case "link": {
                const link = new Link();
                const hrefAttr = node.attributes.getNamedItem('href'),
                    relAttr = node.attributes.getNamedItem('rel'),
                    typeAttr = node.attributes.getNamedItem('type');

                link.href = hrefAttr != null ? hrefAttr.value : undefined;
                link.rel = relAttr != null ? relAttr.value : undefined;
                link.type = typeAttr != null ? typeAttr.value : undefined;
                head.links.push(link);
                break;
            }
        }
    }

    const body = model.htmlTree.body;
    body.innerHtml = (frameBody.innerHTML || "").trim();
    if (body.innerHtml?.length && body.innerHtml?.endsWith("\n")) {
        body.innerHtml = body.innerHtml.substring(0, body.innerHtml.length - 1);
    }

    const attributes = frameBody.attributes;
    for (const attr of attributes) {
        body.attributes.set(attr.name, attr.value);
    }

    // create children elements
    parseChildren(body, frameBody.children);
};

const generatePagePreview = async () => {
    if (!model.previewSessionId) {
        model.previewSessionId = uuid();
    }

    await apiClient.postAsync(
        `api/contenteditor/store-preview-session/${model.previewSessionId}`,
        mapTree(model.htmlTree!)
    );

    const previewFrame = document.getElementsByTagName('iframe').namedItem('preview-frame');
    if (!previewFrame) {
        return;
    }

    let previewUrl = `api/ContentEditor/page-preview?__accessToken=${authService.getToken()}`;
    if (model.contentId) {
        previewUrl += `&contentId=${model.contentId}`;
    }

    if (model.siteId) {
        previewUrl += `&siteId=${model.siteId}`;
    }

    if (model.previewSessionId) {
        previewUrl += `&previewSessionId=${model.previewSessionId}`;
    }

    previewFrame.src = previewUrl;
};

const addScriptOrStylesheetClick = async (element: any) => {
    await contentResourceEditorRef.value?.addScriptOrStylesheet(element);
};

const deleteClick = async (element: any) => {
    if (!confirm('Are you sure to delete this element from your page?')) {
        return;
    }
                       
    deleteElement(element);
    let elementHtml = element.outerHtml;
    if (elementHtml && /\/>$/.test(elementHtml)) {
        const tag = elementHtml.replace(/[^a-zA-Z0-9]/g, "");
        elementHtml = `<${tag}></${tag}>`;
    }

    let parentElement = element.parent;
                        
    while (parentElement) {
        let parentInnerHtml = parentElement.innerHtml;
                            
        parentElement.innerHtml = parentInnerHtml.replace(elementHtml, '');
        parentElement = parentElement.parent;
    }

    await generatePagePreview();

    function deleteElement(e: any) {
        let parent = e.parent;
        if (!parent) {
            return;
        }

        let idx = parent.children.indexOf(e);
        parent.children.slice(idx, 1);

        let htmlElement = document.getElementsByName(e.innerCode)[0];
        if (htmlElement) {
            htmlElement.remove();
        }
                           
        for (let ch of e.children) {
            deleteElement(ch);
        }
    }
};

const editClick = async (element: any) => await elementEditorRef.value?.editElement(element);
const addNewElementClick = async (element: any) => await elementEditorRef.value?.addNewElement(element);

const elementParseChildren = (element: any, parseContentResult: any) => {
    element.children = [];
    const elementDoc = parseContentResult.htmlDocument.children[0];
    if (elementDoc) {
        parseChildren(element, elementDoc.children);
    }
};

const renderHtmlTree = () => {
    const treeContainer = document.getElementById('html-tree');
    if (!treeContainer) {
        return;
    }

    treeContainer.innerHTML = '';

    const htmlLi = document.createElement('li');         
    htmlLi.textContent = 'html';
    treeContainer.appendChild(htmlLi);

    const head = model.htmlTree!.head;
    const headLi = document.createElement('li');
    const headSpan = document.createElement('span');
    headSpan.textContent ='head'+ (head.title ? ` (title="${head.title}")` : '');
    headLi.appendChild(headSpan);

    const addScriptOrCssLink = document.createElement('a');
    const addScriptOrCssIcon = document.createElement('img');
    addScriptOrCssIcon.setAttribute('class', 'pointer-lnk');
    addScriptOrCssIcon.title = 'Add a new JS script or cascade stylesheet';
    addScriptOrCssIcon.src = addScriptOrStylesheetSrc;
    addScriptOrCssLink.appendChild(addScriptOrCssIcon);
    addScriptOrCssLink.onclick = async () => await addScriptOrStylesheetClick({ tag: 'head'});
    headLi.appendChild(addScriptOrCssLink);

    headLi.style.marginLeft = marginLeft1;
    treeContainer.appendChild(headLi);

    if (head.metadatas && head.metadatas.length) {
        for (let i = 0; i < head.metadatas.length; i++) {
            const metadata = head.metadatas[i];
            const metadataLi = document.createElement('li');
            metadataLi.style.marginLeft = marginLeft2;
                    
            let metadataContent = 'meta';
            if (metadata.charset) {
                metadataContent +=` (charset="${metadata.charset}")`
            }  else if (metadata.name && metadata.content) {
                metadataContent += ` (name="${metadata.name}")`
            }
            
            metadataLi.textContent = metadataContent;
            treeContainer.appendChild(metadataLi);
        }
    }

    if (head.styles && head.styles.length) {
        const styleLi = document.createElement('li');
        styleLi.textContent = 'style';
        styleLi.style.marginLeft = marginLeft2;
        treeContainer.appendChild(styleLi);
    }

    if (head.links && head.links.length) {
        for (let i = 0; i < head.links.length; i++) {
            const link = head.links[i];
            const linkLi = document.createElement('li');
            linkLi.style.marginLeft = marginLeft2;
            linkLi.textContent = `link (href="${link.href}" rel="${link.rel}")`;
            treeContainer.appendChild(linkLi);
        }
    }

    if (head.scripts && head.scripts.length) {
        for (let i = 0; i < head.scripts.length; i++) {
            const script = head.scripts[i];
            const scriptLi = document.createElement('li');
                        
            let scriptDescription = "";
            if (script.src) {
                scriptDescription = (script.src.indexOf(newResourcePlaceHolder) >= 0
                    || script.src.indexOf(existResourcePlaceHolder) >= 0) 
                        ? "(NEW)" : `(src="${script.src}")`;
            }
            
            scriptLi.textContent = `script${scriptDescription}`;
            scriptLi.style.marginLeft = marginLeft2;
            treeContainer.appendChild(scriptLi);
        }
    }

    const body = model.htmlTree!.body;
    renderElementTree(body, '0px');
};

const renderElementTree = (element: any, parentMarginLeft: string) => {
    const elementLi = document.createElement('li');
    const span = document.createElement('span');
                    
    const editLnk = document.createElement('a');
    const editIcon = document.createElement('img');
                
    editIcon.src = editIconSrc;
    editIcon.setAttribute('class', 'pointer-lnk');
    editLnk.appendChild(editIcon);
    editLnk.onclick = async () => await editClick(element);
                
    const deleteLnk = document.createElement('a');
    const deleteIcon = document.createElement('img');
    deleteIcon.src = removeIconSrc;
    deleteLnk.appendChild(deleteIcon);
    deleteIcon.setAttribute('class', 'pointer-lnk');
    deleteIcon.onclick = async () => await deleteClick(element);
                
    elementLi.setAttribute('name', element.innerCode);

    const elementAttributes = element.attributes;
    let elementText = element.tag + (element.id ? `#${element.id}`: '');
    const cssClass = elementAttributes.get('class');
    if (cssClass) {
        elementText += `.${cssClass}`;
    }

    const pxIndex = parentMarginLeft.indexOf('px');
    const marginValue = parseInt(parentMarginLeft.substring(0, pxIndex));

    const newMargin = (marginValue + 10) + 'px';
    elementLi.style.marginLeft = newMargin;
    span.textContent = elementText;
    elementLi.appendChild(span);
    if (element.tag != 'br') {
        elementLi.appendChild(editLnk);
    }
    if (element.tag != 'body') {
        elementLi.appendChild(deleteLnk);
    }

    let availableTags = getAvailableTags(element.tag);
    if (availableTags.length) {
        const addNewLnk = document.createElement('a');
        const addNewIcon = document.createElement('img');
        addNewIcon.src = addNewIconSrc;
        addNewLnk.appendChild(addNewIcon);
        addNewIcon.setAttribute('class', 'pointer-lnk');
        addNewIcon.onclick = async () => await addNewElementClick(element);
        elementLi.appendChild(addNewLnk);
    }

    const treeElement = document.getElementById('html-tree')!;
    treeElement.appendChild(elementLi);

    const elementChildren = element.children || [];
    for (let c of elementChildren) {
        renderElementTree(c, newMargin);
    }
};

const updatePageState = async () => {
    const treeElement = document.getElementById('html-tree');
    if (!treeElement) {
        return;
    }

    treeElement.innerHTML = '';
    renderHtmlTree();  // re-create the tree
    
    await generatePagePreview();    // Update page preview
};

const save = async () => {
    model.processing = true;
    try {
        if (!model.contentId) {
            // For a new page, validate fileName (required, pattern, is unique, etc.)
            if (!model.contentName) {
                model.error = "File name is required.";
                return;
            }

            /* eslint-disable */
            let contentNamePattern = /^([a-z_\-\s0-9\.]+)+\.(html|htm|xhtml)$/;
            if (!contentNamePattern.test(model.contentName)) {
                model.error = "Invalid content file name. Enter a name with extension .html, .htm or .xhtml without any path.";
                return;
            }

            let destinationPathPattern = /(^[a-z0-9]+)(\/[a-z0-9-]+)*([a-z0-9])$/;
            if (model.contentDestinationPath && !destinationPathPattern.test(model.contentDestinationPath!)) {
                model.error = "Invalid destination path entered.";
                return;
            }

            // Remote validation
            let checkFileNameUrl = `api/contenteditor/check-new-file-name?contentName=${model.contentName}&uploadSessionId=${model.uploadSessionId}`;
            if (model.contentDestinationPath) {
                checkFileNameUrl = `${checkFileNameUrl}&destinationPath=${model.contentDestinationPath}`;
            }

            if (model.siteId) {
                checkFileNameUrl = `${checkFileNameUrl}&siteId=${model.siteId}`;
            }

            try {
                const isContentNameUnique = (await apiClient.getAsync(checkFileNameUrl) as any).data;
                if (!isContentNameUnique) {
                    model.error = "These file name and destination path are already in use.";
                    return;
                }
            } catch {
                model.error = "Cannot validate input data because of server error.";
                return;
            }
        }

        if (!model.previewSessionId) {
            model.previewSessionId = uuid();
        }

        const saveData = {
            previewSessionId: model.previewSessionId,
            contentId: model.contentId,
            destinationPath: model.contentDestinationPath,
            fileName: model.contentName,
            uploadSessionId: model.uploadSessionId,
            cacheDuration: model.cacheDuration
        };
        const requestData = mapTree(model.htmlTree!);

        await apiClient.postAsync(
            `api/contenteditor/store-preview-session/${model.previewSessionId}`,
            requestData
        );

        const response = await apiClient.postAsync(`api/contenteditor/save`, saveData) as any;
        if (response.status != 200) {
            // Error during save, show error message
            alert('Unable to save your changes. Make sure that HTML is valid and try again or contact us.');
        } else {
            const context = siteContextManager.get();
            const uploadedFiles = context?.uploadedFiles || [];
            const data = response.data;
            const contentSize = data.size;
            if (data.id) {
                const item = uploadedFiles.find(i => i.id == data.id);
                if (item) {
                    item.size = contentSize;
                    item.updateDate = data.updateDate;
                    item.cacheDuration = model.cacheDuration;
                }
            } else {
                newPageCreated.value = true;
                const item = uploadedFiles.find(i => i.name == model.contentName && i.destinationPath == model.contentDestinationPath);
                        
                if (item) {
                    item.size = contentSize;
                    item.uploadedAt = data.uploadedAt;
                    item.cacheDuration = model.cacheDuration;
                } else {
                    const cf = new ContentFile(
                        null,
                        model.contentName!,
                        model.contentDestinationPath!,
                        true,
                        contentSize,
                        true,
                        false,
                        data.uploadedAt,
                        null,
                        model.cacheDuration
                    );
                            
                    uploadedFiles.push(cf);
                    new NewCreatedContentHolder().setContent(cf.name, model.htmlTree!.toString());
                }
            }

            context!.uploadedFiles = uploadedFiles;
            siteContextManager.save(context!);
        }
    } finally {
        await delay(250);
        model.processing = false;
    }
};

const toSite = () => {
    if (!model.siteId) {
        router.push({ 
            name: 'create-site', 
            params: { uploadSessionId: model.uploadSessionId } 
        });
    } else {
        router.push({ name: 'update-site', 
            params: {
                siteId: model.siteId,
                uploadSessionId: model.uploadSessionId
            } 
        });
    }
};

const editHtml = async () => {
    const head = model.htmlTree!.head;
    const body = model.htmlTree!.body;

    let htmlContent = 
                `<html>
<head>\n`;
    if (head.title) {
        htmlContent += `<title>${head.title}</title>\n`
    }

    for (const metadata of head.metadatas) {
        if (metadata.charset) {
            htmlContent += `<meta charset="${metadata.charset}"/>\n`;
        } else {
            htmlContent += `<meta name="${metadata.name}" content="${metadata.content}"/>\n`;
        }
    }

    for (let link of head.links) {
        htmlContent += `<link rel="${link.rel}" type="${link.type}" href="${link.href}"/>\n`;
    }

    if (head.styles.length) {
        htmlContent += `<style>`;
        for (let style of head.styles) {
            htmlContent += `${style.trimEnd()}\n`;
        }
        htmlContent += `</style>\n`;
    }

    for (let script of head.scripts) {
        let scr = document.createElement('script');
        if (script.src) {
            scr.src = script.src;
        }
        if (script.type) {
            scr.type = script.type;
        }

        if (script.body) {
            scr.text = script.body;
        }

        htmlContent += `${scr.outerHTML}\n`;
    }

    htmlContent += `</head>\n`;
    htmlContent += `${body.outerHtml}\n`;
    htmlContent += "</html>";

    htmlContent = htmlContent.split("\n").filter(l => l && l.trim().length).join("\n");

    const dlgSubject = editContentDialogRef.value?.showDialog(model.contentName, htmlContent,
                    (content: string) => {
                        const parser = new DOMParser();
                        const result = parser.parseFromString(content, 'application/xml');
                        let errorNode = result.querySelector('parsererror');
                        if (errorNode) {
                            return errorNode.textContent;
                        }
                    }, model.cacheDuration, 'html');
                                    
    dlgSubject.subscribe(async({ newContent, cacheDuration } : { newContent: string, cacheDuration: string | undefined}) => {
        model.processing = true;
        const dom = new DOMParser().parseFromString(newContent.toString(), 'text/html');

        try {
             processLoadedContent(dom);
             model.cacheDuration = cacheDuration;
             await updatePageState();
        } finally {
            model.processing = false;
        }
    });
};

const getTitle = computed(() => model.contentId || newPageCreated.value ? "Edit Page" : "Create Page");

onMounted(async () => {
    const pageContext = PageContextProvider.get();
    model.siteId = pageContext.siteId as string;
    model.uploadSessionId = pageContext.uploadSessionId as string;
    model.contentId = pageContext.contentId as string;
    model.contentName = pageContext.contentName as string;
    model.contentDestinationPath = pageContext.contentDestinationPath as string;
    model.cacheDuration = pageContext.cacheDuration as string;
    
    let pageHtml: string;
    if (model.siteId && model.contentId) {
        const contentUrl = `api/ContentEditor/render-content?contentItemId=${model.contentId}&__accessToken=${authService.getToken()}`;
        const contentResponse = await apiClient.getAsync(contentUrl, { 'content-type': 'text/html'}) as any;
        pageHtml = <string>contentResponse.data;
    } else {
        if (!model.uploadSessionId) {
             // invalid route, return to Dashboard page.
            router.push('/dashboard');
            return;
        }
        
        const localContentCache = new NewCreatedContentHolder();
        pageHtml = localContentCache.getContent(model.contentName) as string;
    }

    if (pageHtml?.length) {
        const parser = new DOMParser();
        const dom = parser.parseFromString(pageHtml, 'text/html');
        processLoadedContent(dom);
    } else {
        createEmptyTree();
    }
    
    renderHtmlTree();
    await generatePagePreview();
});

</script>
<template>
    <div class="content-block-container">
        <div class="general-page-title">
            <span>{{getTitle}}</span>
        </div>
        <div class="page-editor-container">
            <div class="button-bar">
                <button class="btn btn-primary" @click="() => toSite()">&lt;&lt; To Site</button>&nbsp;
                <button class="btn btn-primary" @click="() => editHtml()">Edit HTML</button>&nbsp;
                <button class="btn btn-primary" @click="() => save()">Save</button>
                <span class="cache-duration-container"> Cache duration: <TimeSpanInput v-model="model.cacheDuration" /></span>
                <Loader :processing="model.processing"/>
            </div>
            <div class="content-inputs-container">
                <table>
                    <tr>
                        <td>File name:</td>
                        <td>
                            <input type="text" v-model="model.contentName" v-if="!model.contentId" @change="()=>model.error = null"/>
                            <span class="content-label" v-if="model.contentId">{{model.contentName}}</span>
                        </td>
                        <td>Destination path:</td>
                        <td>
                            <input type="text" v-model="model.contentDestinationPath" @change="()=>model.error = null" v-if="!model.contentId" />
                            <span class="content-label" v-if="model.contentId">{{model.contentDestinationPath || '--'}}</span>
                        </td>
                        <td v-if="model.error" class="validation-error-container">
                            <span class="validation-error">{{model.error}}</span>
                        </td>
                    </tr>
                </table>
            </div>
            <div class="html-tree-and-preview-container">
                <div class="html-tree-container">
                    <!-- HTML tree goes here -->
                    <div class="html-tree-inner-container">
                        <ul class="html-tree" id="html-tree">
                        </ul>
                    </div>
                </div>
                <div class="page-preview-container">
                    <iframe id="preview-frame" />
                </div>
            </div>
        </div>

        <ElementEditor :htmlTree="model.htmlTree!" ref="elementEditorRef"
            :elementParseChildren="(element: any, pcr: any) => elementParseChildren(element, pcr)" 
            :onProcess="(isProcessing: boolean) => model.processing = isProcessing"
            :onComplete="async() => await updatePageState()"
            :elementAddNewHandler="async(element: any) => addNewElementClick(element)"
            :elementEditHandler="(element: any) => editClick(element)"    
        />

        <ContentResourceEditor :htmlTree="model.htmlTree!" ref="contentResourceEditorRef"
            :uploadSessionId="model.uploadSessionId!"
            :siteId="model.siteId!"
            :onComplete="async() => await updatePageState()"
        />

        <EditContentDialog ref="editContentDialogRef" />
    </div>
</template>
<style scoped>
.button-bar {
    display: flex;
    padding-top: 5px;
    padding-bottom: 5px;
    padding-left: 3px;
    background-color: darkgrey;
}

.validation-error-container {
    padding-left: 10px;
}

.pointer-lnk {
    cursor: pointer;
}

.html-tree-container {
    width: 550px; 
    float: left;    
}

.page-preview-container {
    float: left;
    width: calc(100vw - 602px);
    background-color: white;
    margin-left: 2px;
    margin-top: 5px;
}

#preview-frame {
    width: inherit;
    height: calc(100vh - 170px);
    border: 0px;
}

.content-inputs-container {
    padding: 5px;
    background-color: darkgrey;
}

.content-label {
    margin-left: 5px;
    margin-right: 5px;
    font-weight: bold;
    color: navy;
}

.html-tree-inner-container {
   margin-top: 5px;
   width: 540px;
   height: calc(100vh - 166px);
   clear: both;
   overflow: auto;
   background-color: floralwhite;
   color: navy;
}

#html-tree {
    list-style: none;
}

.page-editor-container {
    height: calc(100% - 68px);
    background-color: azure;
}

.cache-duration-container {
    margin-left: 15px;
}
</style>