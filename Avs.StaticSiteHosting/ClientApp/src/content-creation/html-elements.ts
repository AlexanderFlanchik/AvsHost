import { v4 as uuid } from  'uuid';

export class Html {
    head: Head;
    body: GenericElement;

    constructor() {
        this.head = new Head();
        this.body = new GenericElement();
        this.body.tag = 'body';
    }
}

export class Head {
    title: String;
    metadatas: Array<Metadata>;
    scripts: Array<Script>;
    styles: Array<String>;
    links: Array<Link>;

    constructor() {
        this.metadatas = new Array<Metadata>();
        this.scripts = new Array<Script>();
        this.styles = new Array<String>();
        this.links = new Array<Link>();
    }
}

export class Script {
    type: String;
    src: String;
    body: String;
}

export class Metadata {
    charset: String;
    name: String;
    content: String;
}

export class Link {
    rel: String;
    href: String;
    type: String;
}

export class GenericElement {
    innerCode: String;
    tag: String;
    id: String;
    parent: GenericElement;
    attributes: Map<String, String>;
    children: Array<GenericElement>;
    innerHtml: String;

    get outerHtml(): String {
        let attrs = [];
        this.attributes.forEach((val: String, key: String) => {
            attrs.push({ name: key, value: val });
        });

        let attr = attrs.map(a => `${a.name}="${a.value}"`).join(' ');

        if (this.tag != 'img' && this.tag != 'br' && this.innerHtml) {
            return attr.length ? `<${this.tag} ${attr}>${this.innerHtml}</${this.tag}>` : `<${this.tag}>${this.innerHtml}</${this.tag}>`;
        } else {
            return attr.length ? `<${this.tag} ${attr} />` : `<${this.tag}/>`;
        }
    }
    
    constructor() {
        this.attributes = new Map<String, String>();
        this.children = new Array<GenericElement>();
        this.innerCode = uuid();
    }

    getElementByInnerCode(innerCode: String) : GenericElement {
        if (this.innerCode == innerCode) {
            return this;
        }

        if (this.children.length) {
            for (let ch of this.children) {
                let e = ch.getElementByInnerCode(innerCode);
                if (e) {
                    return e;
                }
            }
        }
        return null;
    }

    getElementById(id: String): GenericElement {
        if (this.id === id) {
            return this;
        }

        if (this.children.length) {
            for (let ch of this.children) {
                let e = ch.getElementById(id);
                if (e) {
                    return e;
                }
            }
        }

        return null;
    }
}