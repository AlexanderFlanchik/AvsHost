import { v4 as uuid } from  'uuid';

export class Html {
    head: Head;
    body: GenericElement;

    constructor() {
        this.head = new Head();
        this.body = new GenericElement();
        this.body.tag = 'body';
    }

    toString() {
        if (!this.head && !this.body) {
            return "";
        }

        const lst = [];
        lst.push("<html>");
        
        if (this.head) {
            lst.push("<head>");
            if (this.head.title) {
                lst.push(`<title>${this.head.title}</title>`);
            }

            if (this.head.styles?.length) {
                lst.push("<style>");

                for (const st of this.head.styles) {
                    lst.push(st);
                }

                lst.push("</style>");
            }

            if (this.head.metadatas?.length) {
                for (const m of this.head.metadatas) {
                    const metaParts = [];
                    if (m.charset) {
                        metaParts.push(`charset="${m.charset}"`);
                    }

                    if (m.name) {
                        metaParts.push(`name="${m.name}"`);
                    }

                    if (m.content) {
                        metaParts.push(`content="${m.content}"`);
                    }

                    lst.push(`<meta ${metaParts.join(" ")}>`);
                }
            }

            if (this.head.links?.length) {
                for (const lnk of this.head.links) {
                    lst.push(`<link href="${lnk.href}" rel="${lnk.rel}" type="${lnk.type}">`);
                }
            }

            if (this.head.scripts?.length) {
                for (const scr of this.head.scripts) {
                    if (scr.body) {
                        lst.push(`<script type="${scr.type}">`);
                        lst.push(scr.body);
                        lst.push('</script>');
                    } else {
                        lst.push(`<script type="${scr.type}" src="${scr.src}"></script`);
                    }
                }
            }

            lst.push("</head>");
        }

        lst.push(this.body ? this.body.outerHtml : "<body/>");

        return lst.join('\n');
    }
}

export class Head {
    title?: string;
    metadatas: Array<Metadata>;
    scripts: Array<Script>;
    styles: Array<string>;
    links: Array<Link>;

    constructor() {
        this.metadatas = new Array<Metadata>();
        this.scripts = new Array<Script>();
        this.styles = new Array<string>();
        this.links = new Array<Link>();
    }
}

export class Script {
    type?: string;
    src?: string;
    body?: string;
}

export class Metadata {
    charset?: string;
    name?: string;
    content?: string;
}

export class Link {
    rel?: string;
    href?: string;
    type?: string;
}

export class GenericElement {
    innerCode: string;
    tag: string | null = null;
    id: string | null = null;
    parent: GenericElement | null = null;
    attributes: Map<string, string>;
    children: Array<GenericElement>;
    innerHtml: string | null = null;

    get outerHtml(): string {
        let attrs: Array<{name: string, value: string}> = [];
        this.attributes.forEach((val: string, key: string) => {
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
        this.attributes = new Map<string, string>();
        this.children = new Array<GenericElement>();
        this.innerCode = uuid();
    }

    getElementByInnerCode(innerCode: string | null) : GenericElement | null {
        if (!innerCode) {
            return null;
        }

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

    getElementById(id: String): GenericElement | null {
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