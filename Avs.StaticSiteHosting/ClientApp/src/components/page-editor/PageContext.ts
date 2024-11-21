export type NullableString = string | undefined | null;

export interface PageContext {
    contentId: NullableString;
    siteId: NullableString;
    uploadSessionId: NullableString;
    contentName: NullableString;
    contentDestinationPath: NullableString;
}

export class PageContextProvider {
    static readonly pageContextKey = "pageContext_current";

    static get() : PageContext {
        const pageContextJson = localStorage.getItem(this.pageContextKey);
        if (!pageContextJson) {
            return {} as PageContext;
        }

        const pageContext = <PageContext>JSON.parse(pageContextJson);
        return pageContext;
    };
    
    static set(pageContext: PageContext): void {
        localStorage.setItem(this.pageContextKey, JSON.stringify(pageContext));
    }
}