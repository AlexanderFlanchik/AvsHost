import { describe, it, expect, beforeEach } from "bun:test";
import CustomRouteProcessor from "../services/CustomRouteProcessor";

type MockProvider = {
    getHandler: (id: string) => string | undefined;
};

describe("CustomRouteProcessor (Bun)", () => {
    let provider: MockProvider;
    let processor: CustomRouteProcessor;

    beforeEach(() => {
        provider = {
            getHandler: (id: string) => undefined
        };
        // @ts-ignore - provider shape is enough for tests
        processor = new CustomRouteProcessor(provider);
    });

    it("returns 404 when handler not found", async () => {
        provider.getHandler = () => undefined;
        const request = { handlerId: "missing" };
        const context = {};

        const res = await processor.routeAsync(request as any, context as any);
        expect(res.status).toBe(404);
        expect(String(res.error)).toContain("missing");
        expect(res.contentType).toBe("application/json");
    });

    it("returns 204 when handler returns nothing (no return)", async () => {
        provider.getHandler = () => "const foo = params.request.x; /* no return */";
        const request = { handlerId: "noReturn", x: 1 };
        const context = {};

        const res = await processor.routeAsync(request as any, context as any);
        expect(res.status).toBe(204);
        expect(res.contentType).toBe("application/json");
    });

    it("applies default status (200) and default contentType when handler returns partial response", async () => {
        provider.getHandler = () => "return { body: 'ok' };";
        const request = { handlerId: "partial" };
        const context = {};

        const res = await processor.routeAsync(request as any, context as any);
        expect(res.status).toBe(200);
        expect(res.contentType).toBe("application/json");
        expect((res as any).body).toBe("ok");
    });

    it("respects provided status and contentType from handler", async () => {
        provider.getHandler = () => "return { status: 201, contentType: 'text/plain', body: 'created' };";
        const request = { handlerId: "created" };
        const context = {};

        const res = await processor.routeAsync(request as any, context as any);
        expect(res.status).toBe(201);
        expect(res.contentType).toBe("text/plain");
        expect((res as any).body).toBe("created");
    });

    it("catches handler exceptions and returns error with route id", async () => {
        provider.getHandler = () => "throw new Error('boom');";
        const request = { handlerId: "throws" };
        const context = {};

        const res = await processor.routeAsync(request as any, context as any);
        expect(res.status).toBe(500);
        expect(String(res.error)).toContain("boom");
        expect(String(res.error)).toContain("throws");
    });

    it("passes params into handler and supports async code", async () => {
        provider.getHandler = () => `
            const { request, context } = params;
            // simulate async work
            await Promise.resolve();
            return { body: request.value + ':' + context.value };
        `;
        const request = { handlerId: "async", value: "r" };
        const context = { value: "c" };

        const res = await processor.routeAsync(request as any, context as any);
        expect(res.status).toBe(200);
        expect(res.contentType).toBe("application/json");
        expect((res as any).body).toBe("r:c");
    });
});