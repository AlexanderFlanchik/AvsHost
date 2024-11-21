import { ContentFile } from "../common/ContentFile";

export interface SiteContext {
    siteId: string;
    siteName: string;
    description: string;
    landingPage: string;
    isActive: boolean;
    resourceMappings: Array<ResourceMapping>;
    uploadSessionId: string;
    uploadedFiles: Array<ContentFile>;
    tagIds: Array<string>
}

export interface ResourceMapping {
    name: string;
    value: string;
}

export class SiteContextManager {
    private siteContextKey = 'SiteContext';

    public save(siteContext: SiteContext) {
        localStorage.setItem(this.siteContextKey, JSON.stringify(siteContext));
    }

    public get(): SiteContext | null | undefined {
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