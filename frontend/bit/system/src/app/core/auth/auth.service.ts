import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BaseService } from '../../shared/services/base.service';
import { LocalStorageUtils } from '../../shared/utils/localstorage';
import { map, Observable } from 'rxjs';
import { Router } from '@angular/router';

export interface IUser {
  email: string;
  name?: string;
  avatarUrl?: string;
}

const defaultPath = '/';

@Injectable({
  providedIn: 'root',
})
export class AuthService extends BaseService {
  localStorageUtils = new LocalStorageUtils();

  constructor(private http: HttpClient, private router: Router) {
    super();
  }

  private _user: IUser | null = null;
  public _lastAuthenticatedPath: string = defaultPath;

  get loggedIn(): boolean {
    const loggedUser = this.localStorageUtils.getUser();
    const claims = localStorage.getItem('BIT.claims');

    if (loggedUser && claims) {
      const email = loggedUser.email;
      const name = loggedUser.nome;
      this._user = { email, name };
    } else {
      this._user = null;
    }

    return !!this._user;
  }

  async logOut() {
    this.localStorageUtils.clearLocaleUserData();
    this._user = null;
    await this.router.navigate(['/auth/login']);
  }

  isTokenValid(): boolean {
    const token = this.localStorageUtils.getUserToken();

    if (!token) return false;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));

      const exp = payload?.exp * 1000;
      return Date.now() < exp;
    } catch (e) {
      return false;
    }
  }

   verifyExitingDocument(document: string, type: string): Observable<{ exists: boolean }> {
    const url = ''
    console.log('s');
    
    return this.http.get<{ exists: boolean }>(url, this.GetAuthHeaderJson());
  }
}
