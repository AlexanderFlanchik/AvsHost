import * as moment from 'moment';
import axios from 'axios';

export default class AuthService {
    constructor() { }
    tokenKey = 'auth_token';

    private isLocked: boolean = false;
    public IsUserLocked() {
        return this.isLocked;
    }

    public lockUser() {
        this.isLocked = true;
    }

    public unLockUser() {
        this.isLocked = false;
    }

    public isAuthenticated() {
        let tokenJson = localStorage.getItem(this.tokenKey);
        if (!tokenJson) {
            return false;
        }

        let token = JSON.parse(tokenJson);
        let utcNow = new Date(moment().utc().format());
        var expDate = new Date(token.expires_at);
        
        if (utcNow > expDate) {
            localStorage.removeItem(this.tokenKey);
            return false;
        }

        // token is valid
        return true;
    }

    public getToken(): string {
        let tokenData = localStorage.getItem(this.tokenKey);
        if (!tokenData) {
            return null;
        }

        return JSON.parse(tokenData)['token'];
    }

    public getUserInfo() {
        let tokenData = localStorage.getItem(this.tokenKey);
        if (!tokenData) {
            return null;
        }

        let userInfo = JSON.parse(tokenData)['userInfo'];
        return userInfo;
    }

    public signOut() {
        let tokenData = localStorage.getItem(this.tokenKey);
        if (!tokenData) {
            return;
        }

        localStorage.removeItem(this.tokenKey);
    }

    public async tryGetAccessToken(userLogin: String, password: String) : Promise<boolean> {
        try {
            let response = await axios.post('/auth/token', {
                login: userLogin,
                password: password
            });

            if (response && response.data && response.data.token) {
                let token = response.data.token;
                let expires_at = response.data.expires_at;
                let userInfo = response.data.userInfo;

                localStorage.setItem(this.tokenKey, JSON.stringify({ token, expires_at, userInfo }));

                return true;
            }
        } catch (e) {
            console.log(e);        
        }

        return false;
    }   

    public async register(email: String, userName: String, password: String): Promise<boolean> {
        try {
            let response = await axios.post('/auth/register', {
                email,
                userName,
                password
            });

            return response.status === 200;
        } catch (e) {
            console.log(e);
        }

        return false;
    }

    public async validateUserData(userName: String, email: String): Promise<boolean> {
        try {
            let url = '/auth/validateUserData';
            if (!userName && !email) {
                return false;
            }

            if (userName) {
                url += `?userName=${userName}`;
                if (email) {
                    url += `&email=${email}`;
                }
            } else {
                url += `?email=${email}`;
            }
            
            let response = await axios.get(url);
            console.log(response);
            return response.data;
        } catch (e) {
            console.log(e);
        }

        return false;
    }
}