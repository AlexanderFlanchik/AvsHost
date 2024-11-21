import { Html } from "./html-elements";

export function mapTree(htmlTree: Html) {
    const attributesMap: any = {};
    for (const attr of htmlTree.body.attributes) {
        attributesMap[attr[0]] = attr[1];
    }

    return {
        head: htmlTree.head,
        body: {
            attributes: attributesMap,
            innerHtml: htmlTree.body.innerHtml?.trim()
        }
    };
}