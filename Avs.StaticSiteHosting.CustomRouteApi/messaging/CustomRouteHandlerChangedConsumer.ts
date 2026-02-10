import type { CustomRouteHandlerChanged } from "../dtos/CustomRouteHandlerChanged";
import type CustomRouteHandlerProvider from "../services/CustomRouteHandlerProvider";

class CustomRouteHandlerChangedConsumer {
    constructor(private routeProvider: CustomRouteHandlerProvider) {}

    onHandlerChanged = (event: CustomRouteHandlerChanged) => {
        this.routeProvider.addOrUpdateHandler(event.HandlerId, event.HandlerBody);
    };
}

export default CustomRouteHandlerChangedConsumer;
