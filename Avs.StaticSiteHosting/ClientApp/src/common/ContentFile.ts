export class ContentFile {
    constructor(
        public id: String,
        public name: String,
        public destinationPath: String,
        public isNew: Boolean,
        public size: Number,
        public isEditable: Boolean,
        public isViewable: Boolean,
        public uploadedAt: Date,
        public updateDate: Date) { }

    public fullName(): String {
        if (!this.destinationPath) {
            return this.name;
        }

        var delimeter = '/';
        if (this.destinationPath.indexOf('\\') > 0) {
            delimeter = '\\';
        }

        return `${this.destinationPath}${delimeter}${this.name}`;
    }
}