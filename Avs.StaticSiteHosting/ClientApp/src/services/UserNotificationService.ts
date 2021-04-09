import * as signalR from '@microsoft/signalr';
import AuthService from './AuthService';

export class UserNotificationService {
    private connection: signalR.HubConnection;

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

        this.connection.on("UserStatusChanged", (data: any) => handler(data.currentStatus));
    }

    public subscribeForUnreadConversation(handler: (data: any) => void) {
        if (!this.connection) {
            this.init();
        }

        this.connection.on("new-conversation-message", (msg: any) => handler(msg));
    }
}