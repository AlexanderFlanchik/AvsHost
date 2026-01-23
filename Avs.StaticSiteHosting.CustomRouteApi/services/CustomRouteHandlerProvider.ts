import type { MongoClient } from "mongodb";

class CustomRouteHandlerProvider {    
    private handlers: Map<string, string>;

    constructor(private client: MongoClient) {
        this.handlers = new Map<string, string>();
    }

    public getHandler(id: string): string | undefined {
        return this.handlers.get(id);
    }
    
    public addOrUpdateHandler(id: string, body: string): void {
        this.handlers.set(id, body);
    }

    public removeHandler(id: string): void {
        this.handlers.delete(id);
    }

    public async loadHandlers(): Promise<void> {
        const db = this.client.db("StaticSiteDb");
        const collection = db.collection("CustomRouteHandlers");
        const docs = await collection.find().toArray();
        
        docs.forEach(doc => {
            if (doc._id && doc.body) {
                this.handlers.set(doc._id.toString(), doc.body);
            }
        });
    }
}

export default CustomRouteHandlerProvider;
