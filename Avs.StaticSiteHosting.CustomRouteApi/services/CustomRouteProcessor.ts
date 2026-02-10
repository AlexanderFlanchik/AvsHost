import type { RouteHandlerContext } from "../dtos/RouteHandlerContext";
import type { RouteHandlerRequest, RouteHandlerResponse } from "../dtos/RouteHandlerDtos";
import type CustomRouteHandlerProvider from "./CustomRouteHandlerProvider";

class CustomRouteProcessor {
    constructor(private handlerProvider: CustomRouteHandlerProvider) { }

    public async routeAsync(request: RouteHandlerRequest, context: RouteHandlerContext): Promise<RouteHandlerResponse> {

        const handlerCode = this.handlerProvider.getHandler(request.handlerId);
        if (!handlerCode) {
            return this.error(`Route with ID ${request.handlerId} not found`, 404);            
        }

        const params = { request, context };
  
        try {
            const handlerFuncWrapper = new Function(
                "params",
                `return (async () => { ${handlerCode} })()`
            );

            const response = await handlerFuncWrapper(params) as RouteHandlerResponse;
            if (!response) {
                return {
                    status: 204,
                    contentType: "application/json"
                };
            }

            response.status = response.status || 200;
            response.contentType = response.contentType || "application/json";

            return response;
        }
        catch (error: any) {
            return this.error(`Error executing route handler: ${error.message}, route ID: ${request.handlerId}`);
        }       
    }

    private error(errorMessage: string, code: number = 500, contentType: string = "application/json"): RouteHandlerResponse {
        return {
            status: code,
            contentType: contentType,
            error: errorMessage
        };
    }
}

export default CustomRouteProcessor;