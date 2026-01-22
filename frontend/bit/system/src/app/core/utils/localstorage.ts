import { loggUser } from '../../shared/models/loggUser';

export class LocalStorageUtils {
  user: loggUser = new loggUser();

  //shared
    //gets    
    public getUser() {
      const userJson = localStorage.getItem('BITADMIN.user');
      return userJson ? JSON.parse(userJson) : null;
    }

    public getUserToken(): string | null {
      return localStorage.getItem('BITADMIN.token');
    }


    public getUserType(): string | null {
      const claimsString = localStorage.getItem('BITADMIN.user');

      if (!claimsString) return null;

      const claims = JSON.parse(claimsString);

      if (!claims?.claims || !Array.isArray(claims.claims)) return null;

      const userTypeClaim = claims.claims.find(
        (claim: any) => claim.type === 'typeUser'
      );

      return userTypeClaim?.value?.toUpperCase() || null;
    }


    //saves
    public saveLocaleDataUser(
      response: any,
      negocioId: any,
      claims: any
    ) {
      this.saveUserToken(response.accessToken);
      this.saveUserRefreshToken(response.refreshToken);
      this.saveUserClaims(claims);
      this.saveUser(response);
    }

    public saveUserToken(token: string) {
      localStorage.setItem('BITADMIN.token', token);
    }

    public saveUserRefreshToken(refreshToken: string) {
      localStorage.setItem('BITADMIN.refreshtoken', refreshToken);
    }

    public saveUser(response: any) {
      this.user.name = String(response.userToken.nome);
      this.user.document = String(response.userToken.documento);
      this.user.email = String(response.userToken.email);
      this.user.claims = response.userToken.claims;

      localStorage.setItem('BITADMIN.user', JSON.stringify(this.user) || '');
    }

    public saveUserClaims(claims: any) {
      localStorage.setItem('BITADMIN.claims', JSON.stringify(claims));
    }

    //clear
    public clearLocaleUserData() {
      localStorage.removeItem('BITADMIN.token');
      localStorage.removeItem('BITADMIN.refreshtoken');
      localStorage.removeItem('BITADMIN.user');
      localStorage.removeItem('BITADMIN.claims');
    }
  //
}
