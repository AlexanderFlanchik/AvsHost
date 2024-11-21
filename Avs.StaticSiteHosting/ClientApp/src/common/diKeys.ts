import type { InjectionKey } from 'vue';
import ApiClient from '../services/api-client';
import AuthService from '../services/auth-service';
import UserNotificationService from '../services/user-notification-service';
import { ConfigProvider } from '../services/confg-provider';

export const API_CLIENT = Symbol() as InjectionKey<ApiClient>;
export const AUTH_SERVICE = Symbol() as InjectionKey<AuthService>;
export const USER_NOTIFICATIONS_SERVICE = Symbol() as InjectionKey<UserNotificationService>;
export const CONFIG_PROVIDER = Symbol() as InjectionKey<ConfigProvider>;
