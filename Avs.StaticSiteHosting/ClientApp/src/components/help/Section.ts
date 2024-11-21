class Section {
    id: string;
    name: string;
    ordinalNo: number;
    isRoot: boolean;
    expanded: boolean;
    selected: boolean;

    private sectionList: Array<any> = [];
    private clickHandlerFn: any;
    sections : Array<Section> = [];

    constructor (o: any) {
        this.id = o.id;
        this.name = o.name;
        this.ordinalNo = o.ordinalNo;
        this.isRoot = o.isRoot;
        this.expanded = o.expanded;
        this.selected = o.selected;
        this.clickHandlerFn = o.clickHandlerFn();

        if (o.sections && o.sections.length) {
           this.sections = [];
            for (var s of o.sections) {
                if (o.clickHandlerFn) {
                    s.clickHandlerFn = o.clickHandlerFn;
                }
                //@ts-ignore
                let sc = new Section(s);
                this.sections.push(sc);
                sc.setSectionList(this.sections);
            }
        }
    }

    setSectionList(lst: Array<any>) {
        this.sectionList = lst;
    }

    clickHandler() {
        this.clickHandlerFn && this.clickHandlerFn(this.id, this.isRoot);
        if (this.isRoot) {
            return;
        }

        for (let i = 0; i < this.sectionList.length; i++) {
            const sc = this.sectionList[i];
            sc.selected = sc.id === this.id;
        }
    }
 }

 export default Section;
