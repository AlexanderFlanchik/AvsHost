<script setup lang="ts">
import { reactive, ref } from 'vue';
import { GenericElement, Html } from './html-elements';
import { v4 as uuid } from 'uuid';
import ModalDialog from '../ModalDialog.vue';
import getAvailableTags from './TagsProvider';
import AddNewAttributeModalDialog from './AddNewAttributeModalDialog.vue';
import AddNewCssClassModalDialog from './AddNewCssClassModalDialog.vue';
import { CodeEditor } from 'monaco-editor-vue3';

interface ElementEditorProps {
    htmlTree: Html;
    elementParseChildren: (element: GenericElement, parseContentResult: any) => void;
    onProcess: (status: boolean) => void;
    onComplete: () => Promise<void>;
}

interface ElementEditorModel {
    isNewElement: boolean;
    innerCode: string | null;
    elementId: string | null;
    parentElementInnerCode: string | null;
    tag: string;
    tagsAvailable: Array<any>;
    attributes: Array<any>;
    cssClasses: Array<string>;
    error: string | null;
    innerHtml: string;
    ok: () => void;
    getEditorTitle: string;
}

const editorOptions = {
    fontSize: 14,
    minimap: { enabled: false },
    automaticLayout: true
};

const props = defineProps<ElementEditorProps>();
const model = reactive<ElementEditorModel>({
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
    getEditorTitle: ''
});

const editElementModalRef = ref<typeof ModalDialog | null>(null);
const addAttributeModalRef = ref<typeof AddNewAttributeModalDialog | null>(null);
const addCssClassModalRef = ref<typeof AddNewCssClassModalDialog | null>(null);

const outerHtml = () => {
    if (!model.tag) {
        return '';
    }

    const attrs = model.attributes.map((a: { name: string, value: string}) => `${a.name}="${a.value}"`);
    if (model.cssClasses.length) {
        attrs.push(`class="${model.cssClasses.join(' ')}"`);
    }

    const  attr = model.attributes.map((a: { name: string, value: string}) => `${a.name}="${a.value}"`).join(' ');

    if (model.tag != 'img' && model.tag != 'br' && model.innerHtml) {
        return attr.length ? `<${model.tag} ${attr}>${model.innerHtml}</${model.tag}>` : `<${model.tag}>${model.innerHtml}</${model.tag}>`;
    } else {
        return attr.length ? `<${model.tag} ${attr} />` : `<${model.tag} />`;
    }
};

const onOpen = () => {
    model.getEditorTitle = model.isNewElement ? "Add New Element" : "Edit Element";
    model.attributes = [];
    model.cssClasses = [];
};

const removeAttribute = (attr: any) => {
    const idx = model.attributes.indexOf(attr);
    if (idx >= 0) {
        model.attributes.splice(idx, 1);
    }
};

const removeCssClass = (cl: any) => {
    let idx = model.cssClasses.indexOf(cl);
    if (idx >= 0) {
        model.cssClasses.splice(idx, 1);
    }
};

const validateInnerHtml = () => {
    const parser = new DOMParser();
    const result = parser.parseFromString(outerHtml(), 'application/xml');
    const errorNode = result.querySelector('parsererror');
                
    if (errorNode) {
        return { isValid: false, errorMessage: errorNode.textContent };
    }
    
    return { isValid: true, htmlDocument: result };
};

const elementEditorAddNewOk = async () => {
    // 1. Create element in the HTML Tree object
    // 2. re-create the tree
    // 3. Update page preview
    if (!model.tag) {
        model.error = 'Please select a tag for a new element.';
        return true;
    }

    const newElement = new GenericElement();
    newElement.tag = model.tag;

    const getParentElement = () => props.htmlTree.body.getElementByInnerCode(model.parentElementInnerCode);
    newElement.parent = getParentElement();
    newElement.innerCode = uuid();

    const attributes = model.attributes || [];
    let elementIdAttr = attributes.find((a: any) => a.name.toLowerCase() == 'id');
    if (elementIdAttr) {
        newElement.id = elementIdAttr.value;
    }

    newElement.attributes = new Map();
    for (let a of attributes) {
        newElement.attributes.set(a.name, a.value);
    }

    const classes = model.cssClasses.join(' ');
        if (classes && classes.length) {
            newElement.attributes.set('class', classes);
    }

    const parseContentResult = validateInnerHtml();
    if (!parseContentResult.isValid) {
        model.error = parseContentResult.errorMessage!;
        return;
    }

    newElement.innerHtml = model.innerHtml;

    // parse element new children
    props.elementParseChildren(newElement, parseContentResult);
    
    // Update element HTML content in all parent innerHtml refs
    let parentElement = newElement.parent;
    let isFirstParent = true;
    let oldHtml: string  = '', newHtml: string  = '';
    let element = parentElement;
                
    while (element) {
        if (isFirstParent) {
            oldHtml = element.outerHtml.toString();
            element.innerHtml = `${element.innerHtml || ""}${newElement.outerHtml}`;
            newHtml = element.outerHtml.toString();
            isFirstParent = false;
        } else if (element.innerHtml) {
            element.innerHtml = element.innerHtml.replace(oldHtml, newHtml);
        }

        element = element.parent;
    }

    parentElement = getParentElement();
    if (parentElement) {
        parentElement.children.push(newElement);
    }

    model.error = null;
    await props.onComplete();

    editElementModalRef.value?.close();
};

const addNewElement = async (element: GenericElement) => {
    model.isNewElement = true;
    onOpen();

    model.parentElementInnerCode = element ? element.innerCode : null;
    model.innerHtml = '';
                
    const tagsAvailable = getAvailableTags(element.tag!);
    const tgsResponse = await fetch('TagNames.json');
    const tgsJson = await tgsResponse.json();
    const allTags = tgsJson || [];
    model.tagsAvailable = allTags.filter((t: any) => tagsAvailable.indexOf(t.tag) >= 0);

    model.ok = async () => { 
        try {
            props.onProcess(true);
            await elementEditorAddNewOk();
        } finally {
            props.onProcess(false);
        }
    };

    editElementModalRef.value?.open();
};

const elementEditorOk = async () => {
    // Main ElementEditor logic
    // Collect all data from the ElemenetEditor instance
    // And update HtmlTree instance (tree, innerHtml & outerHtml fields)
    const innerCode = model.innerCode;
    const element = props.htmlTree.body.getElementByInnerCode(innerCode);
    if (!element) {
        editElementModalRef.value?.close();
        return;
    }

    const previousOuterHtml = element.outerHtml;
    let parseContentResult = validateInnerHtml();
    if (!parseContentResult.isValid) {
        model.error = parseContentResult.errorMessage!;
        return;
    }

    let elementInnerHtml = model.innerHtml;
    let classes = model.cssClasses.join(' ');
    let elementIdAttr = model.attributes.find((a: { name: string}) => a.name.toLowerCase() == 'id');
    
    if (elementIdAttr) {
        // validate element Id entered
        let id = elementIdAttr.value;
        let existingElement = props.htmlTree.body.getElementById(id);
        if (existingElement && existingElement.innerCode != element.innerCode) {
            model.error = `Id '${id}' is already used.`;
            return;
        }
                    
        element.id = id;
    } else {
        element.id = null;
    }

    element.innerHtml = elementInnerHtml;
    element.attributes = new Map();
    for (let attr of model.attributes) {
        element.attributes.set(attr.name, attr.value);
    }

    if (classes && classes.length) {
        element.attributes.set('class', classes);
    }

    // parse element new children
    props.elementParseChildren(element, parseContentResult);
                                     
    // Update element HTML content in all parent innerHtml & outerHtml refs
    let parentElement = element.parent;
    while (parentElement) {
        let parentInnerHtml = parentElement.innerHtml!;
                     
        parentElement.innerHtml = parentInnerHtml.replace(previousOuterHtml, element.outerHtml);
        parentElement = parentElement.parent;
    }
                                     
    model.error = null;
                                     
    await props.onComplete();
    
    editElementModalRef.value?.close();
};

const editElement = async (element: GenericElement) => {
    model.isNewElement = false;
    onOpen();
    
    model.innerCode = element.innerCode;
    model.tag = element.tag!;
    model.elementId = element.id;
    model.innerHtml = element.innerHtml!;

    const attributes = element.attributes;
    for (const attribute of attributes) {
        if (attribute[0] != 'class') {
            model.attributes.push({ name: attribute[0], value: attribute[1] });
        }
    }

    const classes = element.attributes.get('class');
    if (classes) {
        for (const cl of classes.split(' ')) {
            model.cssClasses.push(cl);
        }
    }

    model.ok = async () => { 
        try {
            props.onProcess(true);
            await elementEditorOk();
        } finally {
            props.onProcess(false);
        }
    };

    editElementModalRef.value?.open();
};

const addAttribute = () => addAttributeModalRef!.value?.open();
const addCssClass = () => addCssClassModalRef!.value?.open();

defineExpose({ addNewElement, editElement });
</script>
<template>
  <ModalDialog ref="editElementModalRef" :title="model.getEditorTitle" :ok="model.ok" :dialog-width="'650px'" :dialog-height="'550px'">
    <div v-if="model.error">
        <span class="validation-error">{{ model.error }}</span>
    </div>
    <div v-if="!model.isNewElement">
        <span>Element: </span>
        <span><strong>{{model.tag}}</strong></span>
        <span v-if="model.elementId">
            ({{model.elementId}})
        </span>
    </div>
    <div v-if="model.isNewElement">
        <span>Element: </span>
        <select v-model="model.tag">
            <option v-for="at in model.tagsAvailable" :key="at" :value="at.tag">
                {{at.displayName}} ({{at.tag}})
            </option>
        </select>
    </div>
    <div>
        <span>Attributes:</span>
        <ul v-if="model.attributes.length" class="attributes-list">
            <li v-for="attribute in model.attributes" :key="attribute">
                <span>Name: {{attribute.name}}, value: {{attribute.value}}</span>
                <a href="javascript:void(0)" class="delete-element-lnk" 
                    @click="() => { removeAttribute(attribute); model.error = null; }">X</a>
            </li>
        </ul>
        <span v-if="!model.attributes.length">(none)</span>
        <span>&nbsp;<a href="javascript:void(0)" @click="addAttribute">Add Attribute...</a></span>
    </div>
    <div>
        <span>CSS Classes: </span>
        <ul v-if="model.cssClasses.length" class="attributes-list">
            <li v-for="cssClass in model.cssClasses" :key="cssClass">
                <span>{{cssClass}}</span>
                <a href="javascript:void(0)" class="delete-element-lnk" 
                    @click="() => { removeCssClass(cssClass); model.error = null; }">X</a>
            </li>
        </ul>
        <span v-if="!model.cssClasses.length">(none) </span>
        <span><a href="javascript:void(0)" @click="addCssClass">Add CSS class...</a></span>
    </div>
    <div>
        <span>Inner content:</span>
        <br/>
        <CodeEditor class="resource-content-area" v-model:value="model.innerHtml" language="html" @change="() => model.error = null" :options="editorOptions"></CodeEditor>
    </div>
    <AddNewAttributeModalDialog :attributes="model.attributes" ref="addAttributeModalRef"></AddNewAttributeModalDialog>
    <AddNewCssClassModalDialog ref="addCssClassModalRef" :css-classes="model.cssClasses"></AddNewCssClassModalDialog>
  </ModalDialog>
</template>
<style scoped>
    .delete-element-lnk {
        color: red;
        font-weight: bold;
        margin-left: 5px;
    }
    .resource-content-area {
        width: -webkit-fill-available;
        height: 385px !important;
    }
</style>