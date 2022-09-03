import { Html } from "./html-elements";

export function mapTree(htmlTree: Html) {
    return {
        head: htmlTree.head,
        body: {
            attributes: htmlTree.body.attributes,
            innerHtml: htmlTree.body.innerHtml
        }
    };
}