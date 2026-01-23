export type Token<T = any> = string | symbol | Newable<T>;
export type Factory<T = any> = (c: IContainer) => T;
export type Newable<T = any> = { new (...args: any[]): T };

export enum Scope {
    Singleton = "SINGLETON",
    Transient = "TRANSIENT",
    Scoped = "SCOPED",
}

interface Registration<T = any> {
    token: Token<T>;
    scope: Scope;
    factory: Factory<T>;
}

export interface IContainer {
    get<T>(token: Token<T>): T;
    tryGet<T>(token: Token<T>): T | undefined;
    createRequestScope(): IContainer;
}

export class Container implements IContainer {
    private registrations = new Map<Token, Registration>();
    private singletons = new Map<Token, any>();

    constructor(private parent?: Container, private scopedInstances?: Map<Token, any>) {
    }

    register<T>(token: Token<T>, factory: Factory<T>, scope: Scope = Scope.Transient) {
        if (!token) throw new Error("token is required");
        this.registrations.set(token, { token, scope, factory });
    }

    registerSingleton<T>(token: Token<T>, factoryOrCtor: Factory<T> | Newable<T>) {
        this.register(token, this.normalizeFactory(factoryOrCtor), Scope.Singleton);
    }

    registerTransient<T>(token: Token<T>, factoryOrCtor: Factory<T> | Newable<T>) {
        this.register(token, this.normalizeFactory(factoryOrCtor), Scope.Transient);
    }

    registerScoped<T>(token: Token<T>, factoryOrCtor: Factory<T> | Newable<T>) {
        this.register(token, this.normalizeFactory(factoryOrCtor), Scope.Scoped);
    }

    get<T>(token: Token<T>): T {
        const instance = this.tryGet(token);
        if (!instance) {
            throw new Error(`Service not registered: ${this.formatToken(token)}`);
        }
        return instance as T;
    }

    tryGet<T>(token: Token<T>): T | undefined {
        // If this is a request scope, prefer instances in this scope
        const registration = this.findRegistration(token);
        if (!registration) {
            return undefined;
        }

        switch (registration.scope) {
            case Scope.Singleton:
                return this.resolveSingleton(token, registration);
            case Scope.Transient:
                return registration.factory(this);
            case Scope.Scoped:
                return this.resolveScoped(token, registration);
            default:
                return registration.factory(this);
        }
    }

    createRequestScope(): IContainer {        
        const root = this.getRoot();
        return new Container(root, new Map<Token, any>());
    }

    private normalizeFactory<T>(factoryOrCtor: Factory<T> | Newable<T>): Factory<T> {
        if (typeof factoryOrCtor === "function" && (factoryOrCtor as any).prototype && (factoryOrCtor as any).prototype.constructor) {            
            const ctor = factoryOrCtor as Newable<T>;
            return (_c: IContainer) => new ctor();
        }
        
        return factoryOrCtor as Factory<T>;
    }

    private resolveSingleton<T>(token: Token<T>, registration: Registration<T>): T {
        const root = this.getRoot();
        if (!root.singletons.has(token)) {
            const instance = registration.factory(root);
            root.singletons.set(token, instance);
        }
        return root.singletons.get(token);
    }

    private resolveScoped<T>(token: Token<T>, registration: Registration<T>): T {        
        const scope = this.getScopeMap();
        if (!scope) {
            return registration.factory(this);
        }
        if (!scope.has(token)) {
            const instance = registration.factory(this);
            scope.set(token, instance);
        }
        return scope.get(token);
    }

    private findRegistration<T>(token: Token<T>): Registration<T> | undefined {
        let c: Container | undefined = this;
        while (c) {
            const reg = c.registrations.get(token);
            if (reg) {
                return reg as Registration<T>;
            }
            
            c = c.parent;
        }
        return undefined;
    }

    private getRoot(): Container {
        let c: Container = this;
        while (c.parent) {
            c = c.parent;
        }

        return c;
    }

    private getScopeMap(): Map<Token, any> | undefined {
        return this.scopedInstances;
    }

    private formatToken(token: Token) {
        if (typeof token === "string") return token;
        if (typeof token === "symbol") return token.toString();
        if (typeof token === "function") return token.name || "<constructor>";
        return String(token);
    }
}