import AuthService from "./auth-service";
import axios, { AxiosRequestHeaders } from 'axios';

export default class ApiClient {    
    constructor(private authService: AuthService) {      
    }

    private setAuthorization(headers: any) {
        let authToken = this.authService.getToken();
        if (authToken) {
            headers['Authorization'] = `Bearer ${authToken}`;
        }
    }

    async getAsync(url: string, headers?: any): Promise<object> {
        headers = (headers || {}) as unknown as AxiosRequestHeaders;
        this.setAuthorization(headers);

        return axios.get(url, { headers: headers });
    }

    async postAsync(url: string, data?: object, headers?: AxiosRequestHeaders): Promise<object> {
        headers = headers || {} as unknown as AxiosRequestHeaders;
        this.setAuthorization(headers);

        return axios.post(url, data, { headers: headers });
    }

    async putAsync(url: string, data: object, headers?: AxiosRequestHeaders): Promise<object> {
        headers = headers || {} as unknown as AxiosRequestHeaders;
        this.setAuthorization(headers);

        return axios.put(url, data, { headers: headers });
    }

    async deleteAsync(url: string, headers?: AxiosRequestHeaders): Promise<object> {
        headers = headers || {} as unknown as AxiosRequestHeaders;
        this.setAuthorization(headers);

        return axios.delete(url, { headers: headers });
    }
}