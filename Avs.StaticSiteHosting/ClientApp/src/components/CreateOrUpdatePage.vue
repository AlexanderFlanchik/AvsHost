<template>
    <div class="content-block-container">
        <div class="general-page-title">
            <span>{{getTitle}}</span>
        </div>
        <div class="page-editor-container">
            <div class="button-bar">
                <button class="btn btn-primary" @click="toSite">&lt;&lt; To Site</button>&nbsp;
                <button class="btn btn-primary" @click="save">Save</button>
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
                            <input type="text" v-model="contentDestinationPath" v-if="!contentId" />
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
        <b-modal size="xl" ref="edit-element-modal" hide-footer :title="elementEditor.getEditorTitle">
            <div>
                <div v-if="elementEditor.error">
                    <span class="validation-error">{{elementEditor.error}}</span>
                </div>
                <div v-if="!elementEditor.isNewElement">
                    <span>Element: </span>
                    <span><strong>{{elementEditor.tag}}</strong></span>
                    <span v-if="elementEditor.elementId">
                       ({{elementEditor.elementId}})
                    </span>
                </div>
                <div v-if="elementEditor.isNewElement">
                    <span>Element: </span>
                    <select v-model="elementEditor.tag">
                        <option v-for="at in elementEditor.tagsAvailable" :key="at" :value="at.tag">
                            {{at.displayName}} ({{at.tag}})
                        </option>
                    </select>
                </div>
                <div>
                    <span>Attributes: </span>
                    <ul v-if="elementEditor.attributes.length" class="attributes-list">
                        <li v-for="attribute in elementEditor.attributes" :key="attribute">
                            <span>Name: {{attribute.name}}, value: {{attribute.value}}</span>
                            <a href="javascript:void(0)" class="delete-element-lnk" 
                                @click="() => { elementEditor.removeAttribute(attribute); elementEditor.error = null; }">X</a>
                        </li>
                    </ul>
                    <span v-if="!elementEditor.attributes.length">(none)</span>
                    <span>&nbsp;<a href="javascript:void(0)" 
                                @click="() => this.$refs['add-attribute-modal'].show()">Add attribute...</a></span>
                </div>
                <div>
                    <span>CSS Classes: </span>
                    <ul v-if="this.elementEditor.cssClasses.length" class="attributes-list">
                         <li v-for="cssClass in elementEditor.cssClasses" :key="cssClass">
                            <span>{{cssClass}}</span>
                            <a href="javascript:void(0)" class="delete-element-lnk" 
                                @click="() => { elementEditor.removeCssClass(cssClass); elementEditor.error = null; }">X</a>
                        </li>
                    </ul>
                    <span v-if="!elementEditor.cssClasses.length">(none) </span>
                    <span><a href="javascript:void(0)" @click="() => this.$refs['add-css-class-modal'].show()">Add CSS class...</a></span>
                </div>
                <div>
                    <span>Inner content:</span>
                    <br/>
                    <b-form-textarea class="resource-content-area" v-model="elementEditor.innerHtml" rows="10" cols="10" @change="() => elementEditor.error = null"></b-form-textarea>
                </div>
            </div>
            <div class="modal-btn-holder">
                <button class="btn btn-primary" @click="() => elementEditor.ok()">OK</button>
                <button class="btn btn-default" @click="() => this.$refs['edit-element-modal'].hide()">Cancel</button>
            </div>
        </b-modal>
        
        <b-modal ref="add-attribute-modal" hide-footer title="Add New Attribute">
           <div v-if="elementEditor.addNewAttributeDlg.error">
               <span class="validation-error">{{elementEditor.addNewAttributeDlg.error}}</span> 
            </div>
            <div>
                <span>Name:</span> <br />
                <b-form-input v-model="elementEditor.addNewAttributeDlg.name" @change="() => this.elementEditor.addNewAttributeDlg.error = null"></b-form-input>
            </div>
            <div>
                <span>Value:</span> <br />
                <b-form-input v-model="elementEditor.addNewAttributeDlg.value" @change="() => this.elementEditor.addNewAttributeDlg.error = null"></b-form-input>
            </div>
            <div class="modal-btn-holder">
                <button class="btn btn-primary" @click="() => this.elementEditor.addNewAttributeDlg.ok()">OK</button>
                <button class="btn btn-default" @click="() => this.$refs['add-attribute-modal'].hide()">Cancel</button>
            </div>
        </b-modal>
        
        <b-modal ref="add-css-class-modal" hide-footer title="Add CSS Class">
            <div v-if="elementEditor.addNewCssClassDlg.error">
               <span class="validation-error">{{elementEditor.addNewCssClassDlg.error}}</span> 
            </div>
            <div>
                <span>Name:</span> <br />
                <b-form-input v-model="elementEditor.addNewCssClassDlg.name" @change="() => this.elementEditor.addNewCssClassDlg.error = null"></b-form-input>
            </div>
            <div class="modal-btn-holder">
                <button class="btn btn-primary" @click="() => this.elementEditor.addNewCssClassDlg.ok()">OK</button>
                <button class="btn btn-default" @click="() => this.$refs['add-css-class-modal'].hide()">Cancel</button>
            </div>
        </b-modal>
        <b-modal  size="xl" ref="add-script-content-modal" hide-footer title="Add Script or Stylesheet">
            <div v-if="contentResourceEditor.error">
                <span class="validation-error">{{contentResourceEditor.error}}</span>
            </div>
            <div>
                <b-form-group label="Resource type" v-slot="{ ariaDescribedby }">
                    <b-form-radio-group id="contentResourceType" v-model="contentResourceEditor.contentResourceType" :aria-describedby="ariaDescribedby" name="scriptTypeList">
                        <b-form-radio value="js">JavaScript</b-form-radio>
                        <b-form-radio value="css">Cascade Stylesheet (CSS)</b-form-radio>
                    </b-form-radio-group>
                </b-form-group>
            </div>
            <div>
                <b-form-group label="Resource content" stacked>
                    <b-form-radio-group v-model="contentResourceEditor.fromFile">
                        <b-form-radio value="true">From file</b-form-radio> <br/>
                        <select class="resource-files-select" v-model="contentResourceEditor.contentFile" :disabled="contentResourceEditor.fromFile != 'true'">
                            <option v-for="file in contentResourceEditor.contentList" :key="file" :value="file">
                                {{file.contentFilePath}}
                            </option>
                        </select> <br/>
                        <b-form-radio value="false">From content:</b-form-radio> <br/>
                        <b-form-textarea class="resource-content-area" v-model="content" :disabled="contentResourceEditor.fromFile == 'true'"></b-form-textarea>
                    </b-form-radio-group>
                </b-form-group>
            </div>
            <div class="modal-btn-holder">
                <button class="btn btn-primary" @click="() => this.contentResourceEditor.ok()">OK</button>
                <button class="btn btn-default" @click="() => this.$refs['add-script-content-modal'].hide()">Cancel</button>
            </div>
        </b-modal>
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
    
    const marginLeft1 = '10px';
    const marginLeft2 = '20px';
    const editIconSrc = '../icons8-edit-16.png';
    const removeIconSrc = '../icons8-remove-16.png';
    const addNewIconSrc = '../icons8-add-16.png';
    const addScriptOrStylesheetSrc = '../icons8-document-16.png';
    const contentFilePlaceHolder = '--Please select a file--';
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
                elementEditor: {
                    isNewElement: false,
                    innerCode: null,
                    elementId: null,
                    parentElementInnerCode: null,
                    tag: '',
                    tagsAvailable: [],
                    attributes: [],
                    cssClasses: [],
                    error: null,
                    innerHtml: '',
                    outerHtml: function () {
                        if (!this.tag) {
                            return '';
                        }

                        let attrs = this.attributes.map(a => `${a.name}="${a.value}"`);
                        if (this.cssClasses.length) {
                            attrs.push({ name: 'class', value: this.cssClasses.join(' ') });
                        }

                        let attr = this.attributes.map(a => `${a.name}="${a.value}"`).join(' ');

                        if (this.tag != 'img' && this.tag != 'br' && this.innerHtml) {
                            return attr.length ? `<${this.tag} ${attr}>${this.innerHtml}</${this.tag}>` : `<${this.tag}>${this.innerHtml}</${this.tag}>`;
                        } else {
                            return attr.length ? `<${this.tag} ${attr} />` : `<${this.tag} />`;
                        }
                    },
                    getEditorTitle: '',
                    onOpen: function() {
                        this.getEditorTitle = this.isNewElement ? "Add New Element" : "Edit Element";
                        this.attributes = [];
                        this.cssClasses = [];
                    },
                    removeAttribute: function(attr) {
                        let idx = this.attributes.indexOf(attr);
                        if (idx >= 0) {
                            this.attributes.splice(idx, 1);
                        }
                    },
                    removeCssClass: function(cl) {
                        let idx = this.cssClasses.indexOf(cl);
                        if (idx >= 0) {
                            this.cssClasses.splice(idx, 1);
                        }
                    },
                    validateInnerHtml : function() {
                        let parser = new DOMParser();
                        let result = parser.parseFromString(this.outerHtml(), 'application/xml');
                        let errorNode = result.querySelector('parsererror');
                        if (errorNode) {
                            return { isValid: false, errorMessage: errorNode.textContent };
                        }
                        return { isValid: true, htmlDocument: result };
                    },
                    addNewAttributeDlg: {
                        error: '',
                        name: '',
                        value: '',
                        ok: () => { },
                    },
                    addNewCssClassDlg: {
                        error: '',
                        name: '',
                        ok: () => { }
                    }
                },
                contentResourceEditor: {
                    element: null,
                    error: null,
                    contentResourceType: 'js',
                    fromFile: 'true',
                    contentFile: null,
                    content: '',
                    contentList: [],
                    onOpen: function() {
                        this.error = null;
                        this.content = null;
                        this.fromFile = 'true';
                        this.contentResourceType = "js";
                    }
                }
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
                        
                        document.body.removeChild(frame);
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
                this.contentResourceEditor.element = element;
                this.contentResourceEditor.onOpen();

                let filesUrl = 'api/ResourceContent';
                let queryParameterSet =  false;
                if (this.siteId) {
                    filesUrl += `?siteId=${this.siteId}`;
                    queryParameterSet = true;
                }

                if (this.uploadSessionId) {
                    filesUrl += (queryParameterSet ? '&' : '?') + `uploadSessionId=${this.uploadSessionId}`;
                    queryParameterSet = true;
                }

                let contentResourceType = this.contentResourceEditor.contentResourceType;
                filesUrl += (queryParameterSet ? '&' : '?') + `contentExtension=${contentResourceType}`;
                try {
                    let filesResponse = await this.$apiClient.getAsync(filesUrl);
                    if (filesResponse.status == 200) {
                        this.contentResourceEditor.contentList = filesResponse.data;
                    }
                } catch {
                    // no-op
                    this.contentResourceEditor.error = 'Unable to get files list from the server due to server error.';
                }
                
                // filter resources which have already been added to the page
                this.contentResourceEditor.contentList = this.contentResourceEditor.contentList.filter(
                        i => !this.htmlTree.head.scripts.find(s => s.src && s.src.indexOf(i.contentFilePath) >= 0) && 
                                !this.htmlTree.head.links.find(l => l.href.indexOf(i.contentFilePath) >= 0) && 
                                this.htmlTree.body.outerHtml.indexOf(i.contentFilePath) < 0);
        
                this.contentResourceEditor.contentList.unshift({ id: null, contentFilePath: contentFilePlaceHolder });
                this.contentResourceEditor.contentFile = this.contentResourceEditor.contentList[0];
                this.contentResourceEditor.ok = this.contentResourceEditor_Ok;
                this.$refs['add-script-content-modal'].show();
            },

            contentResourceEditor_Ok: async function() {
                let head = this.htmlTree.head;
                let body = this.htmlTree.body;
                let tag = this.contentResourceEditor.element.tag;
                let fromFile = this.contentResourceEditor.fromFile === 'true';
                
                // Validation
                if (fromFile) {
                   let selectedFile = this.contentResourceEditor.contentFile;
                   if (!selectedFile || selectedFile.contentFilePath === contentFilePlaceHolder) {
                     // no script or css selected
                     this.contentResourceEditor.error = 'Please select a file to continue.';
                     return;
                   }
                } else {
                    let content = this.contentResourceEditor.content;
                    if (!content) {
                        this.contentResourceEditor.error = 'The content is required.';
                        return;
                    }
                }     
                
                const getContentSrc = (contentResourceType) => {
                    let selectedFile = this.contentResourceEditor.contentFile;
                    let contentPath = selectedFile.contentFilePath;
                    let exists;
                    if (tag == 'head') {
                        exists = contentResourceType == 'js' ?
                            head.scripts.find(s => s.src.endsWith(contentPath)) :
                            head.links.find(l => l.href.endsWith(contentPath)); 
                    } else {
                        // attaching script to body section (for scripts only)
                        exists = body.children.find(e => 
                            e.tag === 'script' && e.attributes.get('src') && 
                            e.attributes.get('src').endsWith(contentPath));
                    }

                    if (exists) {
                        return null;
                    }

                    let isNew = !selectedFile.contentId;
                    let resourceUrl = isNew ? `${newResourcePlaceHolder}/${contentPath}?uploadSessionId=${this.uploadSessionId}`
                        : `${existResourcePlaceHolder}/${contentPath}?siteId=${this.siteId}`;
                        
                    return `/${resourceUrl}&__accessToken=${this.$authService.getToken()}`;
                };

                let contentResourceType = this.contentResourceEditor.contentResourceType;
                if (contentResourceType === 'js') {
                    let script = new Script();
                    script.type = 'text/javascript';
                    if (fromFile) {
                        let src = getContentSrc(contentResourceType);
                        if (!src) {
                            this.contentResourceEditor.error = 'This script already exists.';
                            return;
                        }
                        script.src = src;
                    } else {
                        script.body = this.contentResourceEditor.content;
                    }
                    if (tag === 'head') {
                        head.scripts.push(script);
                    } else {
                        // attach script to the end of body section
                        let scriptElement = new GenericElement();
                        scriptElement.tag = 'script';
                        if (fromFile) {
                            scriptElement.attributes.set('src', script.src);
                        }
                    }
                } else {
                    if (fromFile) {
                        let link = new Link();
                        let src = getContentSrc(contentResourceType);
                        if (!src) {
                            this.contentResourceEditor.error = 'This stylesheet already exists.';
                            return;
                        }
                        link.type = "text/css";
                        link.rel = "stylesheet";
                        link.href = src;
                        head.links.push(link);
                    } else {
                        head.styles.push(this.contentResourceEditor.content);                       
                    }
                }

                this.contentResourceEditor.error = null;
                
                // update HTML tree & page preview
                await this.updatePageState();

                this.$refs['add-script-content-modal'].hide();
            },

            elementEditor_newAttributeDlgOk: function() {
                let name = this.elementEditor.addNewAttributeDlg.name;
                if (!name) {
                    this.elementEditor.addNewAttributeDlg.error = "'Name' field is required.";
                    return;   
                }

                let existingAttribute = this.elementEditor.attributes.find(a => a.name == name);
                if (existingAttribute) {
                    this.elementEditor.addNewAttributeDlg.error = `The attribute with name '${name}' already exists.`;
                    return;
                }

                let newAttribute = { name: name, value: this.elementEditor.addNewAttributeDlg.value };
                this.elementEditor.attributes.push(newAttribute);
                            
                this.elementEditor.addNewAttributeDlg.name = null;
                this.elementEditor.addNewAttributeDlg.value = null;
                this.elementEditor.addNewAttributeDlg.error = null;

                this.$refs['add-attribute-modal'].hide();
            },
            
            elementEditor_newCssClassDlgOk: function() {
                let name = this.elementEditor.addNewCssClassDlg.name;
                if (!name) {
                    this.elementEditor.addNewCssClassDlg.error = "'Name' field is required.";
                    return;   
                }

                let names = name.split(' ');
                for (let nm of names) {
                    let cssClass = this.elementEditor.cssClasses.find(c => c == nm);
                    if (!cssClass) {
                        this.elementEditor.cssClasses.push(nm);
                    }
                }

                this.elementEditor.addNewCssClassDlg.name = null;
                this.elementEditor.addNewCssClassDlg.error = null;

                this.$refs['add-css-class-modal'].hide();
            },

            elementEditorOk: async function() {
                // Main ElementEditor logic
                // Collect all data from the ElemenetEditor instance
                // And update HtmlTree instance (tree, innerHtml & outerHtml fields)
                           
                let innerCode = this.elementEditor.innerCode;
                let element = this.htmlTree.body.getElementByInnerCode(innerCode);
                
                let previousOuterHtml = element.outerHtml;
                if (!element) {
                    this.$refs['edit-element-modal'].hide();
                    return;
                }
                
                let parseContentResult = this.elementEditor.validateInnerHtml();
                if (!parseContentResult.isValid) {
                    this.elementEditor.error = parseContentResult.errorMessage;
                    return;
                }
                            
                let elementInnerHtml = this.elementEditor.innerHtml;
                let classes = this.elementEditor.cssClasses.join(' ');
                let elementIdAttr = this.elementEditor.attributes.find(a => a.name.toLowerCase() == 'id');
                if (elementIdAttr) {
                    // validate element Id entered
                    let id = elementIdAttr.value;
                    let existingElement = this.htmlTree.body.getElementById(id);
                    if (existingElement && existingElement.innerCode != element.innerCode) {
                        this.elementEditor.error = `Id '${id}' is already used.`;
                        return;
                    }
                    element.id = id;
                } else {
                    element.id = null;
                }

                element.innerHtml = elementInnerHtml;
                element.attributes = new Map();
                for (let attr of this.elementEditor.attributes) {
                    element.attributes.set(attr.name, attr.value);
                }

                if (classes && classes.length) {
                    element.attributes.set('class', classes);
                }
               
                // parse element new children
                this.elementParseChildren(element, parseContentResult);
                                     
                // Update element HTML content in all parent innerHtml & outerHtml refs
                let parentElement = element.parent;
                while (parentElement) {
                    let parentInnerHtml = parentElement.innerHtml;

                    parentElement.innerHtml = parentInnerHtml.replace(previousOuterHtml, element.outerHtml);
                    parentElement = parentElement.parent;
                }
                
                this.elementEditor.error = null;
                await this.updatePageState();

                this.$refs['edit-element-modal'].hide();
            },
            elementParseChildren: function(element, parseContentResult) {
                element.children = [];
                let elementDoc = parseContentResult.htmlDocument.children[0];
                if (elementDoc) {
                    this.parseChildren(element, elementDoc.children);
                }
            },
            elementEditorAddNewOk: async function () {
                // 1. Create element in the HTML Tree object
                // 2. re-create the tree
                // 3. Update page preview
                if (!this.elementEditor.tag) {
                    this.elementEditor.error = 'Please select a tag for new element.';
                    return;
                }

                let newElement = new GenericElement();
                newElement.tag = this.elementEditor.tag;
                
                const getParentElement = () => this.htmlTree.body.getElementByInnerCode(this.elementEditor.parentElementInnerCode);
                newElement.parent = getParentElement();
                newElement.innerCode = uuid();

                let attributes = this.elementEditor.attributes || [];
                let elementIdAttr = attributes.find(a => a.name.toLowerCase() == 'id');
                if (elementIdAttr) {
                    newElement.id = elementIdAttr.value;
                }

                newElement.attributes = new Map();
                for (let a of attributes) {
                    newElement.attributes.set(a.name, a.value);
                }

                let classes = this.elementEditor.cssClasses.join(' ');
                if (classes) {
                    newElement.attributes.set('class', classes);
                }

                let parseContentResult = this.elementEditor.validateInnerHtml();
                if (!parseContentResult.isValid) {
                    this.elementEditor.error = parseContentResult.errorMessage;
                    return;
                }

                newElement.innerHtml = this.elementEditor.innerHtml;
                
                // parse element new children
                this.elementParseChildren(newElement, parseContentResult);

                // Update element HTML content in all parent innerHtml refs
                let parentElement = newElement.parent;
                let isFirstParent = true;
                let oldHtml, newHtml;
                let element = parentElement;
                while (element) {
                    if (isFirstParent) {
                        oldHtml = element.outerHtml;
                        element.innerHtml = `${element.innerHtml || ""}${newElement.outerHtml}`;
                        newHtml = element.outerHtml;
                        isFirstParent = false;
                    } else {
                        element.innerHtml = element.innerHtml.replace(oldHtml, newHtml);
                    }

                    element = element.parent;
                }
                
                parentElement = getParentElement();
                if (parentElement) {
                    parentElement.children.push(newElement);
                }

                this.elementEditor.error = null;
                await this.updatePageState();

                this.$refs["edit-element-modal"].hide();
            },

            addNewElementClick: async function (element) {
                this.elementEditor.isNewElement = true;
                this.elementEditor.onOpen();
                this.elementEditor.parentElementInnerCode = element ? element.innerCode : null;
                this.elementEditor.innerHtml = '';

                let tagsAvailable = getAvailableTags(element.tag);
                let tgsResponse = await fetch('TagNames.json');
                let tgsJson = await tgsResponse.json();
                let allTags = tgsJson || [];
                this.elementEditor.tagsAvailable = allTags.filter(t => tagsAvailable.indexOf(t.tag) >= 0);

                this.elementEditor.addNewAttributeDlg.ok = this.elementEditor_newAttributeDlgOk;
                this.elementEditor.addNewCssClassDlg.ok = this.elementEditor_newCssClassDlgOk;
                this.elementEditor.ok = async () => await this.elementEditorAddNewOk();
                this.$refs["edit-element-modal"].show();
            },

            editClick: function(element) {
                this.elementEditor.isNewElement = false;
                this.elementEditor.onOpen();
                this.elementEditor.innerCode = element.innerCode;
                this.elementEditor.tag = element.tag;
                this.elementEditor.elementId = element.id;
                this.elementEditor.innerHtml = element.innerHtml;

                let attributes = element.attributes;
                for (let attribute of attributes) {
                    if (attribute[0] != 'class') {
                        this.elementEditor.attributes.push({ name: attribute[0], value: attribute[1] });
                    }
                }

                let classes = element.attributes.get('class');
                if (classes) {
                    for (let cl of classes.split(' ')) {
                        this.elementEditor.cssClasses.push(cl);
                    }
                }

                // child dialog event handlers
                this.elementEditor.addNewAttributeDlg.ok = this.elementEditor_newAttributeDlgOk;
                this.elementEditor.addNewCssClassDlg.ok = this.elementEditor_newCssClassDlgOk;
                this.elementEditor.ok = async () => await this.elementEditorOk();
               
                this.$refs["edit-element-modal"].show();
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
                            scriptDescription = script.src.indexOf(newResourcePlaceHolder) >= 0 ? "(NEW)" :
                                `(src="${script.src}")`;
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
                if (!this.contentId) {
                    // For a new page, validate fileName (required, pattern, is unique, etc.)
                    if (!this.contentName) {
                        this.error = "File name is required.";
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
                    console.log(response);
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
                                this.destinationPath,
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
            }
        },
        computed: {
            getTitle: function() {
                return this.contentId ? 'Edit Page' : 'Create New Page';
            }
        }
    }
</script>
<style scoped>
.button-bar {
    width: 100%;
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
</style>