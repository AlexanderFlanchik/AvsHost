import * as signalR from '@microsoft/signalr';
import AuthService from './AuthService';

export class UserNotificationService {
    private connection: signalR.HubConnection;

    // Events
    readonly UserStatusChanged = "UserStatusChanged";
    readonly NewConversationMessage = "new-conversation-message";
    readonly SiteErrorEvent = "site-error";
    readonly NewSiteVisited = "site-visited";

    constructor(private authService: AuthService) {}

    public init() {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl("/user-notification", { accessTokenFactory: () => this.authService.getToken() })
            .withAutomaticReconnect()
            .build();

        this.connection.start().then(() => console.log('Realtime notification initalized.')).catch(err => console.log(err));
    }

    public notify(handler: (status: any) => void) {
        if (!this.connection) {
            this.init();
        }

        this.connection.on(this.UserStatusChanged, (data: any) => handler(data.currentStatus));
    }

    public subscribeForUnreadConversation(handler: (data: any) => void) {
        if (!this.connection) {
            this.init();
        }

        this.connection.on(this.NewConversationMessage, (msg: any) => handler(msg));
    }

    public subscribeToSiteEvents(onNewVisit: () => void, onError: () => void) {
        if (!this.connection) {
            this.init();
        }

        this.connection.on(this.SiteErrorEvent, onError);
        this.connection.on(this.NewSiteVisited, onNewVisit);
    }
   
    public unsubscribe(event: string) {
        if (!this.connection) {
            console.log('No connection');
            return;
        }

        this.connection.off(event);
    }
}