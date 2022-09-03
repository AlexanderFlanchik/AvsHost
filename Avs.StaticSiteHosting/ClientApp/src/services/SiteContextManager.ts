import { ContentFile } from "../common/ContentFile";

export interface SiteContext {
    siteId: String;
    siteName: String;
    description: String;
    landingPage: String;
    isActive: Boolean;
    resourceMappings: Array<ResourceMapping>;
    uploadSessionId: String;
    uploadedFiles: Array<ContentFile>;
}

export interface ResourceMapping {
    name: String;
    value: String;
}

export class SiteContextManager {
    private siteContextKey = 'SiteContext';

    public save(siteContext: SiteContext) {
        localStorage.setItem(this.siteContextKey, JSON.stringify(siteContext));
    }

    public get(): SiteContext {
        let jsonContext = localStorage.getItem(this.siteContextKey);
        if (!jsonContext) {
            return null;
        }

        return <SiteContext>JSON.parse(jsonContext);
    }

    public delete() {
        localStorage.removeItem(this.siteContextKey);
    }
}