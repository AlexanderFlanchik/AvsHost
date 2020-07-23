import * as moment from 'moment';
import axios from 'axios';

export default class AuthService {
    constructor() { }
    tokenKey = 'auth_token';

    isAuthenticated() {
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

    async tryGetAccessToken(userLogin: String, password: String) : Promise<boolean> {
        try {
            let response = await axios.post('/auth/token', {
                login: userLogin,
                password: password
            });

            if (response && response.data && response.data.token) {
                let token = response.data.token;
                let expires_at = response.data.expiresAt;
                localStorage.setItem(this.tokenKey, JSON.stringify({ token, expires_at }));

                return true;
            }
        } catch (e) {
            console.log(e);        
        }

        return false;
    }
}