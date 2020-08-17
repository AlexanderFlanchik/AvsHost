import * as moment from 'moment';
import axios from 'axios';

export default class AuthService {
    constructor() { }
    tokenKey = 'auth_token';

    public isAuthenticated() {
        let tokenJson = localStorage.getItem(this.tokenKey);
        if (!tokenJson) {
            return false;
        }

        let now = moment().toDate();
        let token = JSON.parse(tokenJson);

        if (now > token.expires_at) {
            localStorage.removeItem(this.tokenKey);
            console.log('Auth token expired.');
            return false;
        }

        // token is valid
        return true;
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
                let expires_at = response.data.expiresAt;
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