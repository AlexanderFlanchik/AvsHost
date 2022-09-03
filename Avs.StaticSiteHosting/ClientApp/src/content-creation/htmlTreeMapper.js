"use strict";
exports.__esModule = true;
exports.mapTree = void 0;
function mapTree(htmlTree) {
    return {
        head: htmlTree.head,
        body: {
            attributes: htmlTree.body.attributes,
            innerHtml: htmlTree.body.innerHtml
        }
    };
}
exports.mapTree = mapTree;
