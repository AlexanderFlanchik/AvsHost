import type { CustomRouteHandlersDeleted } from "../dtos/CustomRouteHandlersDeleted";
import type CustomRouteHandlerProvider from "../services/CustomRouteHandlerProvider";

class CustomRouteHandlersDeletedConsumer {
    constructor(private routeProvider: CustomRouteHandlerProvider) {}

    onHandlersDeleted = (event: CustomRouteHandlersDeleted) => {        
        for (const handlerId of event.HandlerIds) {
            this.routeProvider.removeHandler(handlerId);
        }
    }
}

export default CustomRouteHandlersDeletedConsumer;
