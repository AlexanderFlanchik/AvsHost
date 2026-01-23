import type { Container } from "./DependcyInjection";
import client from "../services/MongoDbClient";
import { MongoClient } from "mongodb";
import CustomRouteHandlerProvider from "../services/CustomRouteHandlerProvider";
import CustomRouteProcessor from "../services/CustomRouteProcessor";

const configureServices = (container: Container) => {
    // Register all services here
    container.registerSingleton(MongoClient, () => client);
    container.registerSingleton(CustomRouteHandlerProvider, (ctx) => new CustomRouteHandlerProvider(ctx.get(MongoClient)));
    container.registerTransient(CustomRouteProcessor, (ctx) => new CustomRouteProcessor(ctx.get(CustomRouteHandlerProvider)));
};

export default configureServices;
