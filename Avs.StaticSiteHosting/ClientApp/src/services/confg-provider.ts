import ApiClient from "./api-client";

export interface ClientConfig {
    contentHostUrl: string;
}

export class ConfigProvider {
    private clientConfig : ClientConfig = {} as ClientConfig;
    private loaded = false;

    constructor(private apiClient: ApiClient) {
    }

    public async LoadConfig() {
        if (this.loaded) {
            return;
        }

        const response = await this.apiClient.getAsync("/well-known/config") as any;
        this.clientConfig = response.data;
        this.loaded = true;
    }

    public get(): ClientConfig {
        return this.clientConfig;
    }
}