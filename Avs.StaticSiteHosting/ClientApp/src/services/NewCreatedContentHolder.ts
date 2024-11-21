export class NewCreatedContentHolder {
    private readonly contentKeyPrefix = "content:";

    private getContentKey(contentName: string) {
        return `${this.contentKeyPrefix}-${contentName}`;
    }

    public getContent(contentName: string) {
        return localStorage.getItem(this.getContentKey(contentName));
    }

    public setContent(contentName: string, content: string) {
        localStorage.setItem(this.getContentKey(contentName), content);
    }

    public removeContent(contentName: string) {
        localStorage.removeItem(this.getContentKey(contentName));
    }
}