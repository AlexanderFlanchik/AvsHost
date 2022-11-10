<template>
    <div class="content-block-container">
        <div class="general-page-title">
            <span>{{getTitle}}</span>
        </div>
        <div class="page-editor-container">
            <div class="button-bar">
                <button class="btn btn-primary" @click="() => this.toSite()">&lt;&lt; To Site</button>&nbsp;
                <button class="btn btn-primary" @click="() => this.editHtml()">Edit HTML</button>&nbsp;
                <button class="btn btn-primary" @click="() => this.save()">Save</button>
                <div class="loader" v-if="processing"></div>
            </div>
            <div class="content-inputs-container">
                <table>
                    <tr>
                        <td>File name:</td>
                        <td>
                            <input type="text" v-model="contentName" v-if="!contentId" @change="()=>this.error = null"/>
                            <span class="content-label" v-if="contentId">{{contentName}}</span>
                        </td>
                        <td>Destination path:</td>
                        <td>
                            <input type="text" v-model="contentDestinationPath" @change="()=>this.error = null" v-if="!contentId" />
                            <span class="content-label" v-if="contentId">{{contentDestinationPath || '--'}}</span>
                        </td>
                        <td v-if="error" class="validation-error-container">
                            <span class="validation-error">{{error}}</span>
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

        <ElementEditor :htmlTree="htmlTree" ref="element-editor"
            :elementParseChildren="(element, pcr) => this.elementParseChildren(element, pcr)" 
            :onProcess="(isProcessing) => this.processing = isProcessing"
            :onComplete="async() => await this.updatePageState()"
            :elementAddNewHandler="async(element) => this.addNewElementClick(element)"
            :elementEditHandler="(element) => this.editClick(element)"
        />

        <ContentResourceEditor :htmlTree="htmlTree" ref="content-resource-editor"
            :uploadSessionId="uploadSessionId"
            :siteId="siteId"
            :onComplete="async() => await this.updatePageState()"
        />

        <EditContentDialog ref="content-edit-dlg" />
    </div>
</template>
<script lang="ts">
    import { delay } from 'q';
    import { GenericElement, Html, Script, Link, Metadata } from  '../content-creation/html-elements';
    import { mapTree } from '../content-creation/htmlTreeMapper';
    import { v4 as uuid } from 'uuid';
    import { SiteContextManager } from '../services/SiteContextManager';
    import { ContentFile } from '../common/ContentFile';
    import getAvailableTags from '../content-creation/TagsProvider';
    
    import EditContentDialog from './EditContentDialog.vue';
    import ElementEditor from './CreateOrUpdatePage/ElementEditor.vue';
    import ContentResourceEditor from './CreateOrUpdatePage/ContentResourceEditor.vue';

    const marginLeft1 = '10px';
    const marginLeft2 = '20px';
    const editIconSrc = '../icons8-edit-16.png';
    const removeIconSrc = '../icons8-remove-16.png';
    const addNewIconSrc = '../icons8-add-16.png';
    const addScriptOrStylesheetSrc = '../icons8-document-16.png';
    const newResourcePlaceHolder = '%NEW_RESOURCE%';
    const existResourcePlaceHolder = '%EXIST_RESOURCE%';

    const siteContextManager = new SiteContextManager();

    export default {
        data: function () {
            return {
                siteId: null,
                contentId: null,
                contentName: null,
                contentDestinationPath: null,
                uploadSessionId: null,
                previewSessionId: null,
                htmlTree: null,
                error: null,
                processing: false
            };
        },
        mounted: async function () {
            this.siteId = this.$route.params.siteId;
            this.uploadSessionId = this.$route.params.uploadSessionId;
            this.contentId = this.$route.params.contentId;
            this.contentName = this.$route.params.contentName;
            this.contentDestinationPath = this.$route.params.contentDestinationPath;
           
           // Store or load context if the page has refreshed
            if (this.siteId || this.uploadSessionId) {
                await this.storeEditorContext();
            } else {
                await this.loadEditorContext();
            } 
            
            if (!this.siteId && !this.uploadSessionId) {
                // invalid route, return to Dashboard page.
                this.$router.push('/dashboard');
                return;
            }
            
            /* 
                1. Edit existing page => site Id and content Id are needed
                2. New content for site that has not been created yet
            */

            if (this.siteId) {
                // site exists
                if (this.contentId) {
                    // request content item content from the server and parse it into html tree object
                    let contentUrl = `api/ContentEditor/render-content?contentItemId=${this.contentId}&__accessToken=${this.$authService.getToken()}`;
                    let frame = document.createElement('iframe');
                    frame.src = contentUrl;
                    frame.style.display = 'none';
                    document.body.appendChild(frame);

                    let onContentLoaded = () => {
                        this.processLoadedContentFrame(frame);
                        document.body.removeChild(frame);
                        this.processing = false;
                    };

                    let loadContent = async () => {
                        for (;;) {
                            let head =  frame.contentDocument.head;
                            if (head.innerHTML) {
                                onContentLoaded();
                                break;
                            }
                            await delay(200);
                        }
                    }
                    this.processing = true;
                    await loadContent();
                } else {
                    // initiate creation of a new content item using the upload session ID passed from the CreateOrUpdateSite page
                    this.createEmptyTree();
                        
                }
           } else { // site does not exist, the content is being created using upload session Id only
                this.createEmptyTree();
           }

           this.renderHtmlTree();
           this.generatePagePreview();
        },
        beforeDestroy: function() {
            this.$apiClient.postAsync('/api/contenteditor/clear-editor-context', { previewSessionId: this.previewSessionId });
        },
        methods: {
            createEmptyTree: function() {
                this.htmlTree = new Html();
                this.htmlTree.head.title = 'New Page';
            },
            
            processLoadedContentFrame: function(frame) {
                let contentDocument = frame.contentDocument;
                let frameHead = contentDocument.head;
                let frameBody = contentDocument.body;

                this.htmlTree = new Html();
                let head = this.htmlTree.head;
                       
                for (let node of frameHead.children) {
                    let tagName = node.nodeName.toLowerCase();
                    switch (tagName) {
                        case "title" : {
                            head.title = node.textContent;
                            break;
                        }
                        case "style": {
                            head.styles.push(node.textContent);
                            break;
                        }
                        case "script": {
                            let script = new Script();
                            let typeAttr = node.attributes.getNamedItem('type'),
                            srcAttr = node.attributes.getNamedItem('src');
                            script.src = srcAttr != null ? srcAttr.value : null;
                            script.type = typeAttr != null ? typeAttr.value : null;
                            if (!script.src) {
                                script.body = node.textContent;
                            }

                            head.scripts.push(script);
                            break;
                        }
                        case "meta": {
                            let metadata = new Metadata();
                            let nameAttr = node.attributes.getNamedItem('name'),
                                charsetAttr = node.attributes.getNamedItem('charset'),
                                contentAttr = node.attributes.getNamedItem('content');
                            metadata.name = nameAttr != null ? nameAttr.value : null;
                            metadata.charset = charsetAttr != null ? charsetAttr.value : null;
                            metadata.content = contentAttr != null ? contentAttr.value : null;
                            head.metadatas.push(metadata);
                            break;
                        }
                        case "link": {
                            let link = new Link();
                            let hrefAttr = node.attributes.getNamedItem('href'),
                                relAttr = node.attributes.getNamedItem('rel'),
                                typeAttr = node.attributes.getNamedItem('type');

                            link.href = hrefAttr != null ? hrefAttr.value : null;
                            link.rel = relAttr != null ? relAttr.value : null;
                            link.type = typeAttr != null ? typeAttr.value : null;
                            head.links.push(link);
                            break;
                        }
                    }
                }

                let body = this.htmlTree.body;
                body.innerHtml = frameBody.innerHTML;

                let attributes = frameBody.attributes;
                for (let attr of attributes) {
                    body.attributes.set(attr.name, attr.value);
                }

                // create children elements
                this.parseChildren(body, frameBody.children);
            },

            parseChildren: function(element, contentNodes) {
                for (let o of contentNodes) {
                    let e = new GenericElement();
                    e.id = o.id;
                    e.parent = element;
                    e.tag = o.nodeName.toLowerCase();
                    e.innerHtml = o.innerHTML;
                   
                    let attributes = o.attributes || [];
                    for (let attr of attributes) {
                        e.attributes.set(attr.name, attr.value);
                    }

                    if (o.children && o.children.length) {
                        this.parseChildren(e, o.children);
                    }
                    
                    element.children.push(e);
                }
            },

            storeEditorContext: async function() {
                await this.$apiClient.postAsync('api/contenteditor/store-content-context', {
                    siteId: this.siteId,
                    uploadSessionId: this.uploadSessionId,
                    contentId : this.contentId,
                    contentName: this.contentName,
                    destinationPath: this.contentDestinationPath
                });
            },

            loadEditorContext: async function() {
                let ctxData = await this.$apiClient.getAsync('api/contenteditor/get-content-context');
                if (ctxData.data) {
                    this.siteId = ctxData.data['siteId'];
                    this.uploadSessionId = ctxData.data['uploadSessionId'];
                    this.contentId = ctxData.data['contentId'];
                    this.contentName = ctxData.data['contentName'];
                    this.contentDestinationPath = ctxData.data['destinationPath'];
                }
            },

            applyUiChanges: async function() {
                if (!this.previewSessionId) {
                    this.previewSessionId = uuid();
                }

                await this.$apiClient.postAsync(
                    `api/contenteditor/store-preview-session/${this.previewSessionId}`,
                    mapTree(this.htmlTree)
                );

                this.generatePagePreview();
            },

            updatePageState: async function() {
                document.getElementById('html-tree').innerHTML = '';
                this.renderHtmlTree();  // re-create the tree
                await this.applyUiChanges();    // Update page preview
            },

            addScriptOrStylesheetClick: async function(element) {
                await this.$refs["content-resource-editor"].addScriptOrStylesheet(element);
            },

            elementParseChildren: function(element, parseContentResult) {
                element.children = [];
                let elementDoc = parseContentResult.htmlDocument.children[0];
                if (elementDoc) {
                    this.parseChildren(element, elementDoc.children);
                }
            },

            addNewElementClick: async function (element) {
                await this.$refs["element-editor"].addNewElement(element);
            },

            editClick: async function(element) {
                await this.$refs["element-editor"].editElement(element);
            },

            deleteClick: async function(element) {
                if (!confirm('Are you sure to delete this element from your page?')) {
                    return;
                }
                       
                deleteElement(element);
                let elementHtml = element.outerHtml;
                let parentElement = element.parent;
                        
                while (parentElement) {
                    let parentInnerHtml = parentElement.innerHtml;
                            
                    parentElement.innerHtml = parentInnerHtml.replace(elementHtml, '');
                    parentElement = parentElement.parent;
                }

                await this.applyUiChanges();

                function deleteElement(e) {
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
            },

            renderElementTree: function(element, parentMarginLeft) {
                let elementLi = document.createElement('li');
                let span = document.createElement('span');
                    
                let editLnk = document.createElement('a');
                let editIcon = document.createElement('img');
                editIcon.src = editIconSrc;
                editIcon.setAttribute('class', 'pointer-lnk');
                editLnk.appendChild(editIcon);
                editLnk.onclick = () => this.editClick(element);
                
                let deleteLnk = document.createElement('a');
                let deleteIcon = document.createElement('img');
                deleteIcon.src = removeIconSrc;
                deleteLnk.appendChild(deleteIcon);
                deleteIcon.setAttribute('class', 'pointer-lnk');
                deleteIcon.onclick = async () => await this.deleteClick(element);
                
                elementLi.setAttribute('name', element.innerCode);
                    
                let elementAttributes = element.attributes;
                let elementText = element.tag + (element.id ? `#${element.id}`: '');
                let cssClass = elementAttributes.get('class');
                if (cssClass) {
                    elementText += `.${cssClass}`;
                }

                let pxIndex = parentMarginLeft.indexOf('px');
                let marginValue = parseInt(parentMarginLeft.substring(0, pxIndex));

                let newMargin = (marginValue + 10) + 'px';
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
                    let addNewLnk = document.createElement('a');
                    let addNewIcon = document.createElement('img');
                    addNewIcon.src = addNewIconSrc;
                    addNewLnk.appendChild(addNewIcon);
                    addNewIcon.setAttribute('class', 'pointer-lnk');
                    addNewIcon.onclick = async () => await this.addNewElementClick(element);
                    elementLi.appendChild(addNewLnk);
                }

                document.getElementById('html-tree').appendChild(elementLi);

                let elementChildren = element.children || [];
                for (let c of elementChildren) {
                    this.renderElementTree(c, newMargin);
                }
            },

            renderHtmlTree: function() {
                let treeContainer = document.getElementById('html-tree');
                let htmlLi = document.createElement('li');
               
                htmlLi.textContent = 'html';
                treeContainer.appendChild(htmlLi);
                
                let head = this.htmlTree.head;
                let headLi = document.createElement('li');
                let headSpan = document.createElement('span');
                headSpan.textContent ='head'+ (head.title ? ` (title="${head.title}")` : '');
                headLi.appendChild(headSpan);
                
                let addScriptOrCssLink = document.createElement('a');
                let addScriptOrCssIcon = document.createElement('img');
                addScriptOrCssIcon.setAttribute('class', 'pointer-lnk');
                addScriptOrCssIcon.title = 'Add a new JS script or cascade stylesheet';
                addScriptOrCssIcon.src = addScriptOrStylesheetSrc;
                addScriptOrCssLink.appendChild(addScriptOrCssIcon);
                addScriptOrCssLink.onclick = async () => await this.addScriptOrStylesheetClick({ tag: 'head'});
                headLi.appendChild(addScriptOrCssLink);

                headLi.style.marginLeft = marginLeft1;
                treeContainer.appendChild(headLi);

                if (head.metadatas && head.metadatas.length) {
                    for (let i = 0; i < head.metadatas.length; i++) {
                        let metadata = head.metadatas[i];
                        let metadataLi = document.createElement('li');
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
                    for (let i = 0; i < head.styles.length; i++) {
                        let styleLi = document.createElement('li');
                        styleLi.textContent = 'style';
                        styleLi.style.marginLeft = marginLeft2;
                        treeContainer.appendChild(styleLi);
                    }
                }
                
                if (head.links && head.links.length) {
                    for (let i = 0; i < head.links.length; i++) {
                        let link = head.links[i];
                        let linkLi = document.createElement('li');
                        linkLi.style.marginLeft = marginLeft2;
                        linkLi.textContent = `link (href="${link.href}" rel="${link.rel}")`;
                        treeContainer.appendChild(linkLi);
                    }
                }

                if (head.scripts && head.scripts.length) {
                    for (let i = 0; i < head.scripts.length; i++) {
                        let script = head.scripts[i];
                        let scriptLi = document.createElement('li');
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

                let body = this.htmlTree.body;
                this.renderElementTree(body, '0px');
            },

            generatePagePreview: function() {
                let previewFrame = document.getElementsByTagName('iframe').namedItem('preview-frame');
                let previewUrl = `api/ContentEditor/page-preview?__accessToken=${this.$authService.getToken()}`;
                if (this.contentId) {
                    previewUrl += `&contentId=${this.contentId}`;
                }

                if (this.siteId) {
                    previewUrl += `&siteId=${this.siteId}`;
                }

                if (this.previewSessionId) {
                    previewUrl += `&previewSessionId=${this.previewSessionId}`;
                }

                previewFrame.src = previewUrl;   
            },
            
            save: async function() {
                this.processing = true;
                try {
                    if (!this.contentId) {
                        // For a new page, validate fileName (required, pattern, is unique, etc.)
                            if (!this.contentName) {
                            this.error = "File name is required.";
                            return;
                        }

                        /* eslint-disable */
                        let contentNamePattern = /^([a-z_\-\s0-9\.]+)+\.(html|htm|xhtml)$/;
                        if (!contentNamePattern.test(this.contentName)) {
                            this.error = "Invalid content file name. Enter a name with extension .html, .htm or .xhtml without any path.";
                            return;
                        }

                        let destinationPathPattern = /(^[a-z0-9]+)(\/[a-z0-9-]+)*([a-z0-9])$/;
                        if (!destinationPathPattern.test(this.contentDestinationPath)) {
                            this.error = "Invalid destination path entered.";
                            return;
                        }

                        // Remote validation
                        let checkFileNameUrl = `api/contenteditor/check-new-file-name?contentName=${this.contentName}&uploadSessionId=${this.uploadSessionId}`;
                        if (this.contentDestinationPath) {
                            checkFileNameUrl = `${checkFileNameUrl}&destinationPath=${this.contentDestinationPath}`;
                        }

                        if (this.siteId) {
                            checkFileNameUrl = `${checkFileNameUrl}&siteId=${this.siteId}`;
                        }
                        try {
                            let isContentNameUnique = (await this.$apiClient.getAsync(checkFileNameUrl)).data;
                            if (!isContentNameUnique) {
                                this.error = "These file name and destination path are already in use.";
                                return;
                            }
                        } catch {
                            this.error = "Cannot validate input data because of server error.";
                            return;
                        }
                    }

                    if (!this.previewSessionId) {
                        this.previewSessionId = uuid();
                    }

                    let saveData = {
                        previewSessionId: this.previewSessionId,
                        contentId: this.contentId,
                        destinationPath: this.contentDestinationPath,
                        fileName: this.contentName,
                        uploadSessionId: this.uploadSessionId
                    };

                    await this.$apiClient.postAsync(
                        `api/contenteditor/store-preview-session/${this.previewSessionId}`,
                        mapTree(this.htmlTree)
                    );
                
                    let response = await this.$apiClient.postAsync(`api/contenteditor/save`, saveData);
                    if (response.status != 200) {
                        // Error during save, show error message
                        alert('Unable to save your changes. Make sure that HTML is valid and try again or contact us.');
                    } else {
                        let context = siteContextManager.get();
                        let uploadedFiles = context.uploadedFiles || [];
                        let data = response.data;
                        let contentSize = data.size;
                        if (data.id) {
                            let item = uploadedFiles.find(i => i.id == data.id);
                            if (item) {
                                item.size = contentSize;
                                item.updateDate = data.updateDate;
                            }
                        } else {
                            let item = uploadedFiles.find(i => i.name == this.contentName && i.destinationPath == this.contentDestinationPath);
                        
                            if (item) {
                                item.size = contentSize;
                                item.uploadedAt = data.uploadedAt;
                            } else {
                                let cf = new ContentFile(
                                    null,
                                    this.contentName,
                                    this.contentDestinationPath,
                                    true,
                                    contentSize,
                                    false,
                                    false,
                                    data.uploadedAt,
                                    null
                                );
                            
                                uploadedFiles.push(cf);
                            }
                        }

                        context.uploadedFiles = uploadedFiles;
                        siteContextManager.save(context);
                    }
                } finally {
                    await delay(250);
                    this.processing = false;
                }
            },

            toSite: function () {
                if (!this.siteId) {
                    this.$router.push({ name: 'create-site', 
                        params: { uploadSessionId: this.uploadSessionId } 
                    });
                } else {
                    this.$router.push({ name: 'update-site', 
                        params: {
                            siteId: this.siteId,
                            uploadSessionId: this.uploadSessionId
                        } 
                    });
                }
            },

            editHtml: async function() {
                let head = this.htmlTree.head;
                let body = this.htmlTree.body;

                let htmlContent = 
                `<html>
                <head>\n`;
                if (head.title) {
                    htmlContent += `<title>${head.title}</title>\n`
                }

                for (let metadata of head.metadatas) {
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
                    htmlContent += `<style>\n`;
                    for (let style of head.styles) {
                        htmlContent += `${style}\n`;
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

                    htmlContent += scr.outerHTML;
                }
                
                htmlContent += `</head>\n`;
                htmlContent += body.outerHtml + "\n";
                htmlContent += "</html>";

                let dlgSubject = this.$refs["content-edit-dlg"].showDialog(this.contentName, htmlContent, 
                    content => {
                        let parser = new DOMParser();
                        let result = parser.parseFromString(content, 'application/xml');
                        let errorNode = result.querySelector('parsererror');
                        if (errorNode) {
                            return errorNode.textContent;
                        }
                    });
                
                dlgSubject.subscribe(async(newContent) => {
                    this.processing = true;
                    let tempFrame = document.createElement('iframe');
                    
                    document.body.appendChild(tempFrame);              
                    tempFrame.srcdoc = newContent.toString();

                    setTimeout(async () => {
                        try {
                            this.processLoadedContentFrame(tempFrame);
                            document.body.removeChild(tempFrame);
                            await this.updatePageState();
                        } finally {
                            this.processing = false;
                        }
                    }, 500);
                });
            }
        },
        computed: {
            getTitle: function() {
                return this.contentId ? 'Edit Page' : 'Create New Page';
            }
        },
        components: {
            EditContentDialog,
            ElementEditor,
            ContentResourceEditor
        }
    }
</script>
<style scoped>
.button-bar {
    width: 100%;
    display: flex;
    padding-top: 5px;
    padding-bottom: 5px;
    padding-left: 3px;
    background-color: darkgrey;
}

.validation-error-container {
    padding-left: 10px;
}

.html-tree-container {
    width: 550px;
    float: left;    
}

.page-preview-container {
    float: left;
    width: calc(100vw - 620px);
    background-color: white;
    margin-left: 2px;
    margin-top: 5px;
}

#preview-frame {
    width: inherit;
    height: calc(100vh - 210px);
    border: 0px;
}

.content-inputs-container {
    width: 100%;
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
   height: calc(100vh - 200px);
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

.delete-element-lnk {
    color: red;
    font-weight: bold;
    margin-left: 5px;
}

.resource-files-select {
    width: -webkit-fill-available;
}

.resource-content-area {
    height: 400px;
}

.loader {
    margin-top: 7px;
    margin-left: 6px;
    border: 4px solid #007bff;
    border-top: 4px solid navy; 
    border-bottom: 4px solid Navy;
    border-radius: 50%;
    width: 20px;
    height: 20px;
    animation: spin 2s linear infinite;
  }
  
  @keyframes spin {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
  }
</style>