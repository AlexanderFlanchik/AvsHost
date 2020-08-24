import AuthService from "./AuthService";
import axios from 'axios';

export default class ApiClient {    
    constructor(private authService: AuthService) {      
    }

    async getAsync(url: string, headers: object): Promise<object> {
        let authToken = this.authService.getToken();
        if (authToken) {
            headers = headers || {};                
            headers['Authorization'] =`Bearer ${authToken}`;
        }
        console.log(headers);
        return axios.get(url, { headers: headers });
    }
}