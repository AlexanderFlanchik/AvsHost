export default function getAvailableTags(elementTag: string) {
    switch (elementTag.toLowerCase()) {
        case "body":
        case "div": {
            return ['div', 'p', 'h1', 'h2', 'h3', 'h4', 'fieldset', 'label', 'ul', 'ol', 'a', 'img', 'span', 'table', 'pre', 'dl'];
        }
        case "p": {
            return ['p', 'h1', 'h2', 'h3', 'h4', 'span', 'img', 'a', 'table', 'pre', 'dl'];
        }
        case 'fieldset': {
            return ['legend', 'div', 'p', 'span', 'dl', 'ol', 'ul'];
        }
        case "ul":
        case "ol": {
            return ['li'];
        }
        case "dl": {
            return ['dd', 'dt'];
        }
        case "table": {
            return ["tr", "tbody", "thead"];
        }
        case "thead":
        case "tbody": {
            return ["tr"];
        }
        case "tr": {
            return ["td", "th"];
        }
        case "td":
        case "li":
        case "dt": {
            return ['div', 'p', 'h1', 'h2', 'h3', 'h4', 'fieldset', 'label', 'ul', 'ol', 'a', 'img', 'span', 'table', 'pre', 'dl'];
        }
        case "a": {
            return ["img", "span", "i"];
        }
        default:
            return [];
    }
}