<template>
    <div>
        <b-modal size="xl" ref="edit-element-modal" hide-footer :title="getEditorTitle">
            <div>
                <div v-if="error">
                    <span class="validation-error">{{error}}</span>
                </div>
                <div v-if="!isNewElement">
                    <span>Element: </span>
                    <span><strong>{{tag}}</strong></span>
                    <span v-if="elementId">
                        ({{elementId}})
                    </span>
                </div>
                <div v-if="isNewElement">
                    <span>Element: </span>
                    <select v-model="tag">
                        <option v-for="at in tagsAvailable" :key="at" :value="at.tag">
                            {{at.displayName}} ({{at.tag}})
                        </option>
                    </select>
                </div>
            <div>
            <span>Attributes: </span>
            <ul v-if="attributes.length" class="attributes-list">
                <li v-for="attribute in attributes" :key="attribute">
                    <span>Name: {{attribute.name}}, value: {{attribute.value}}</span>
                        <a href="javascript:void(0)" class="delete-element-lnk" 
                            @click="() => { removeAttribute(attribute); error = null; }">X</a>
                </li>
            </ul>
            <span v-if="!attributes.length">(none)</span>
            <span>&nbsp;<a href="javascript:void(0)" 
                    @click="() => this.$refs['add-attribute-modal'].show()">Add attribute...</a></span>
        </div>
        <div>
            <span>CSS Classes: </span>
            <ul v-if="this.cssClasses.length" class="attributes-list">
                <li v-for="cssClass in cssClasses" :key="cssClass">
                    <span>{{cssClass}}</span>
                    <a href="javascript:void(0)" class="delete-element-lnk" 
                        @click="() => { removeCssClass(cssClass); error = null; }">X</a>
                </li>
            </ul>
            <span v-if="!cssClasses.length">(none) </span>
            <span><a href="javascript:void(0)" @click="() => this.$refs['add-css-class-modal'].show()">Add CSS class...</a></span>
        </div>
        <div>
            <span>Inner content:</span>
            <br/>
            <b-form-textarea class="resource-content-area" v-model="innerHtml" rows="10" cols="10" @change="() => error = null"></b-form-textarea>
        </div>
        </div>
        <div class="modal-btn-holder">
            <button class="btn btn-primary" @click="() => ok()">OK</button>
            <button class="btn btn-default" @click="() => this.$refs['edit-element-modal'].hide()">Cancel</button>
        </div>
        </b-modal>
        
        <b-modal ref="add-attribute-modal" hide-footer title="Add New Attribute">
           <div v-if="addNewAttributeDlg.error">
               <span class="validation-error">{{addNewAttributeDlg.error}}</span> 
            </div>
            <div>
                <span>Name:</span> <br />
                <b-form-input v-model="addNewAttributeDlg.name" @change="() => this.addNewAttributeDlg.error = null"></b-form-input>
            </div>
            <div>
                <span>Value:</span> <br />
                <b-form-input v-model="addNewAttributeDlg.value" @change="() => this.addNewAttributeDlg.error = null"></b-form-input>
            </div>
            <div class="modal-btn-holder">
                <button class="btn btn-primary" @click="() => this.addNewAttributeDlg.ok()">OK</button>
                <button class="btn btn-default" @click="() => this.$refs['add-attribute-modal'].hide()">Cancel</button>
            </div>
        </b-modal>
        
        <b-modal ref="add-css-class-modal" hide-footer title="Add CSS Class">
            <div v-if="addNewCssClassDlg.error">
               <span class="validation-error">{{addNewCssClassDlg.error}}</span> 
            </div>
            <div>
                <span>Name:</span> <br />
                <b-form-input v-model="addNewCssClassDlg.name" @change="() => this.addNewCssClassDlg.error = null"></b-form-input>
            </div>
            <div class="modal-btn-holder">
                <button class="btn btn-primary" @click="() => this.addNewCssClassDlg.ok()">OK</button>
                <button class="btn btn-default" @click="() => this.$refs['add-css-class-modal'].hide()">Cancel</button>
            </div>
        </b-modal>
        </div>
</template>
<script>
    import getAvailableTags from '../../content-creation/TagsProvider';
    import { GenericElement } from '../../content-creation/html-elements';
    import { v4 as uuid } from 'uuid';

    export default {
        props: {
            htmlTree: Object,
            elementParseChildren: Object,
            onProcess: Object,
            onComplete: Object,
            elementAddNewHandler: Object,
            elementEditHandler: Object
        },
        
        data: function() {
            return {
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
                ok: () => {},
                getEditorTitle: '',
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
            };
        },
        methods: {
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

            elementEditor_newAttributeDlgOk: function() {
                let name = this.addNewAttributeDlg.name;
                if (!name) {
                    this.addNewAttributeDlg.error = "'Name' field is required.";
                    return;   
                }

                let existingAttribute = this.attributes.find(a => a.name == name);
                if (existingAttribute) {
                    this.addNewAttributeDlg.error = `The attribute with name '${name}' already exists.`;
                    return;
                }

                let newAttribute = { name: name, value: this.addNewAttributeDlg.value };
                this.attributes.push(newAttribute);
                            
                this.addNewAttributeDlg.name = null;
                this.addNewAttributeDlg.value = null;
                this.addNewAttributeDlg.error = null;

                this.$refs['add-attribute-modal'].hide();
            },

            elementEditor_newCssClassDlgOk: function() {
                let name = this.addNewCssClassDlg.name;
                if (!name) {
                    this.addNewCssClassDlg.error = "'Name' field is required.";
                    return;   
                }

                let names = name.split(' ');
                for (let nm of names) {
                    let cssClass = this.cssClasses.find(c => c == nm);
                    if (!cssClass) {
                        this.cssClasses.push(nm);
                    }
                }

                this.addNewCssClassDlg.name = null;
                this.addNewCssClassDlg.error = null;

                this.$refs['add-css-class-modal'].hide();
            },

            elementEditorAddNewOk: async function() {
                // 1. Create element in the HTML Tree object
                // 2. re-create the tree
                // 3. Update page preview
                if (!this.tag) {
                    this.error = 'Please select a tag for new element.';
                    return;
                }

                let newElement = new GenericElement();
                newElement.tag = this.tag;
                
                const getParentElement = () => this.htmlTree.body.getElementByInnerCode(this.parentElementInnerCode);
                newElement.parent = getParentElement();
                newElement.innerCode = uuid();

                let attributes = this.attributes || [];
                let elementIdAttr = attributes.find(a => a.name.toLowerCase() == 'id');
                if (elementIdAttr) {
                    newElement.id = elementIdAttr.value;
                }

                newElement.attributes = new Map();
                for (let a of attributes) {
                    newElement.attributes.set(a.name, a.value);
                }

                let classes = this.cssClasses.join(' ');
                if (classes) {
                    newElement.attributes.set('class', classes);
                }

                let parseContentResult = this.validateInnerHtml();
                if (!parseContentResult.isValid) {
                    this.error = parseContentResult.errorMessage;
                    return;
                }

                newElement.innerHtml = this.innerHtml;
                
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

                this.error = null;
                await this.onComplete();

                this.$refs["edit-element-modal"].hide();
            },

            addNewElement: async function(element) {
                this.isNewElement = true;
                this.onOpen();
                this.parentElementInnerCode = element ? element.innerCode : null;
                this.innerHtml = '';
                
                let tagsAvailable = getAvailableTags(element.tag);
                let tgsResponse = await fetch('TagNames.json');
                let tgsJson = await tgsResponse.json();
                let allTags = tgsJson || [];
                this.tagsAvailable = allTags.filter(t => tagsAvailable.indexOf(t.tag) >= 0);

                this.addNewAttributeDlg.ok = this.elementEditor_newAttributeDlgOk;
                this.addNewCssClassDlg.ok = this.elementEditor_newCssClassDlgOk;
                this.ok = async () => { 
                    try {
                        this.onProcess(true);
                        await this.elementEditorAddNewOk();
                    } finally {
                        this.onProcess(false);
                    }
                };
                this.$refs["edit-element-modal"].show();
            },

            editElement: async function(element) {
                this.isNewElement = false;
                this.onOpen();
                this.innerCode = element.innerCode;
                this.tag = element.tag;
                this.elementId = element.id;
                this.innerHtml = element.innerHtml;

                let attributes = element.attributes;
                for (let attribute of attributes) {
                    if (attribute[0] != 'class') {
                        this.attributes.push({ name: attribute[0], value: attribute[1] });
                    }
                }

                let classes = element.attributes.get('class');
                if (classes) {
                    for (let cl of classes.split(' ')) {
                        this.cssClasses.push(cl);
                    }
                }

                // child dialog event handlers
                this.addNewAttributeDlg.ok = this.elementEditor_newAttributeDlgOk;
                this.addNewCssClassDlg.ok = this.elementEditor_newCssClassDlgOk;
                this.ok = async () => { 
                    try {
                        this.onProcess(true);
                        await this.elementEditorOk();
                    } finally {
                        this.onProcess(false);
                    }
                };
               
                this.$refs["edit-element-modal"].show();
            },

            elementEditorOk: async function() {
                // Main ElementEditor logic
                // Collect all data from the ElemenetEditor instance
                // And update HtmlTree instance (tree, innerHtml & outerHtml fields)
                           
                let innerCode = this.innerCode;
                let element = this.htmlTree.body.getElementByInnerCode(innerCode);
                
                let previousOuterHtml = element.outerHtml;
                if (!element) {
                    this.$refs['edit-element-modal'].hide();
                    return;
                }
                
                let parseContentResult = this.validateInnerHtml();
                if (!parseContentResult.isValid) {
                    this.elementEditor.error = parseContentResult.errorMessage;
                    return;
                }
                            
                let elementInnerHtml = this.innerHtml;
                let classes = this.cssClasses.join(' ');
                let elementIdAttr = this.attributes.find(a => a.name.toLowerCase() == 'id');
                if (elementIdAttr) {
                    // validate element Id entered
                    let id = elementIdAttr.value;
                    let existingElement = this.htmlTree.body.getElementById(id);
                    if (existingElement && existingElement.innerCode != element.innerCode) {
                        this.error = `Id '${id}' is already used.`;
                        return;
                    }
                    element.id = id;
                } else {
                    element.id = null;
                }

                element.innerHtml = elementInnerHtml;
                element.attributes = new Map();
                for (let attr of this.attributes) {
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
                
                this.error = null;
                await this.onComplete();

                this.$refs['edit-element-modal'].hide();
            }
        }
    }
</script>
<style scoped>
    .delete-element-lnk {
        color: red;
        font-weight: bold;
        margin-left: 5px;
    }
</style>