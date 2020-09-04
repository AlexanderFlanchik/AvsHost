import AuthService from "./AuthService";
import axios from 'axios';

export default class ApiClient {    
    constructor(private authService: AuthService) {      
    }

    private setAuthorization(headers: object) {
        let authToken = this.authService.getToken();
        if (authToken) {
            headers['Authorization'] = `Bearer ${authToken}`;
        }
    }

    async getAsync(url: string, headers: object): Promise<object> {
        headers = headers || {};
        this.setAuthorization(headers);

        return axios.get(url, { headers: headers });
    }

    async postAsync(url: string, data: object, headers: object): Promise<object> {
        headers = headers || {};
        this.setAuthorization(headers);

        return axios.post(url, data, { headers: headers });
    }

    async putAsync(url: string, data: object, headers: object): Promise<object> {
        headers = headers || {};
        this.setAuthorization(headers);

        return axios.put(url, data, { headers: headers });
    }
}