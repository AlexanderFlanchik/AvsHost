export class ContentFile {
    constructor(
        public id: string | null,
        public name: string,
        public destinationPath: string,
        public isNew: boolean,
        public size: number,
        public isEditable: boolean,
        public isViewable: boolean,
        public uploadedAt: Date,
        public updateDate: Date |  null,
        public cacheDuration: string | undefined) { }

    public fullName(): String {
        if (!this.destinationPath) {
            return this.name;
        }

        const delimeter = this.destinationPath.indexOf('\\') > 0 ? '\\' : '/';

        return `${this.destinationPath}${delimeter}${this.name}`;
    }
}