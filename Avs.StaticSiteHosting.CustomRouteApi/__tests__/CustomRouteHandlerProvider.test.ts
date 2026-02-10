import { describe, it, expect, beforeEach } from "bun:test";
import CustomRouteHandlerProvider from "../services/CustomRouteHandlerProvider";

function makeMockClient(docs: any[]) {
    return {
        db: () => ({
            collection: () => ({
                find: () => ({
                    toArray: async () => docs,
                }),
            }),
        }),
    } as unknown;
}

describe("CustomRouteHandlerProvider", () => {
    let provider: InstanceType<typeof CustomRouteHandlerProvider>;

    beforeEach(() => {
        // start each test with a fresh provider with an empty client (no docs)
        provider = new CustomRouteHandlerProvider(makeMockClient([]) as any);
    });

    it("initially has no handlers", () => {
        expect(provider.getHandler("nope")).toBeUndefined();
    });

    it("addOrUpdateHandler adds and updates handlers", () => {
        provider.addOrUpdateHandler("route1", "body1");
        expect(provider.getHandler("route1")).toBe("body1");

        provider.addOrUpdateHandler("route1", "body2");
        expect(provider.getHandler("route1")).toBe("body2");
    });

    it("removeHandler deletes handler", () => {
        provider.addOrUpdateHandler("route2", "body");
        expect(provider.getHandler("route2")).toBe("body");

        provider.removeHandler("route2");
        expect(provider.getHandler("route2")).toBeUndefined();
    });

    it("loadHandlers loads documents from the database mapping _id.toString() to body", async () => {
        const docs = [
            { _id: "str-id", body: "b1" },
            { _id: { toString: () => "obj-id" }, body: "b2" },
            // a doc without body should be ignored
            { _id: "no-body" },
            // a doc without _id should be ignored
            { body: "no-id-body" },
        ];
        provider = new CustomRouteHandlerProvider(makeMockClient(docs) as any);
        await provider.loadHandlers();

        expect(provider.getHandler("str-id")).toBe("b1");
        expect(provider.getHandler("obj-id")).toBe("b2");
        expect(provider.getHandler("no-body")).toBeUndefined();
        expect(provider.getHandler("no-id-body")).toBeUndefined();
    });

    it("loadHandlers overwrites existing handlers with values from DB", async () => {
        // provider starts with one handler
        provider.addOrUpdateHandler("shared", "local");
        expect(provider.getHandler("shared")).toBe("local");

        const docs = [{ _id: "shared", body: "db" }];
        provider = new CustomRouteHandlerProvider(makeMockClient(docs) as any);
        await provider.loadHandlers();

        expect(provider.getHandler("shared")).toBe("db");
    });
});