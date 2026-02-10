export interface RouteHandlerRequest {
    handlerId: string;
    dbName?: string;
    query?: Map<string, string>;
    body?: any;
    headers?: Map<string, string>;
}

export interface RouteHandlerResponse {
    status: number;
    contentType: string;
    content?: string;
    error?: string;
    redirectUrl?: string;
    headers?: Map<string, string>;
    stream?: ReadableStream<any>;
}