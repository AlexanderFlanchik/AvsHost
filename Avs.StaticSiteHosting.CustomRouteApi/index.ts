import { Container } from "./common/DependcyInjection";
import configureServices from "./common/ServiceConfig";
import type { RouteHandlerContext } from "./dtos/RouteHandlerContext";
import type { RouteHandlerRequest } from "./dtos/RouteHandlerDtos";
import { startMessaging } from "./messaging";
import CustomRouteHandlerProvider from "./services/CustomRouteHandlerProvider";
import CustomRouteProcessor from "./services/CustomRouteProcessor";
import { MongoClient } from "mongodb";

const servicePort = process.env.SERVICE_PORT ? parseInt(process.env.SERVICE_PORT) : 3032;
const container = new Container();
configureServices(container);

const handlerProvider = container.get<CustomRouteHandlerProvider>(CustomRouteHandlerProvider);
await handlerProvider.loadHandlers();
await startMessaging(container);

const server = Bun.serve({
    port: servicePort,
    hostname: "0.0.0.0",
    routes: {
        "/health": {
            GET: async (_) => Response.json({ status: "healthy" })
        },       
        "/handle": {
            POST: async (request) => {
                const requestProcessor = container.get<CustomRouteProcessor>(CustomRouteProcessor);
                const dbClient = container.get<MongoClient>(MongoClient);
                    
                const requestBody = await request.json() as RouteHandlerRequest;                      
                const handlerContext = { db: requestBody.dbName ? dbClient.db(requestBody.dbName) : undefined } as RouteHandlerContext;
                
                const response = await requestProcessor.routeAsync(requestBody, handlerContext);
                if (response.error) {
                    return Response.json({ error: response.error }, { status: response.status });
                }

                const getResponseHeaders = () => {
                    const headers = new Headers();
                    if (response.headers) {
                        for (const [key, value] of Object.entries(response.headers)) {
                            headers.set(key, value);
                        }
                    }
                    else {
                        const defaultContentType = response.stream ? "application/octet-stream" : "application/json";
                        headers.set("Content-Type", response.contentType || defaultContentType);
                    }

                    return headers;
                };

                if (response.content) {
                    return new Response(
                        typeof response.content === "string" ? response.content : JSON.stringify(response.content), {
                            status: response.status,
                            headers: getResponseHeaders()
                        }
                    );
                } else if (response.stream) {
                    return new Response(response.stream, {
                        status: response.status,
                        headers: getResponseHeaders()
                    });
                }
                
                return Response.json(null, { status: 204, headers: getResponseHeaders() });
            }
        }
    },
    fetch(request) {
        console.log(`Unknown route: ${request.method} ${request.url}`);
        return new Response("Not found", { status: 404 });
    }
});

console.log(`Server running at http://localhost:${server.port}/`);