import { HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
// import { environment } from '../../../environments/environment.prod';
import { LocalStorageUtils } from '../utils/localstorage';
import { throwError } from 'rxjs';

@Injectable({
  providedIn: 'root',
})

export abstract class BaseService {

  constructor() {}

  public LocalStorage = new LocalStorageUtils();

  // protected UrlServiceApi: string = environment.apiUrl;
  // protected UrlServiceLoginV1: string = environment.apiUrlLoginv1;
  // protected UrlServiceMapaAcesso: string = environment.apiUrlMapaAcesso;

  protected GetHeaderJson() {
    return {
      headers: new HttpHeaders({
        'Content-Type': 'application/json',
      }),
    };
  }

  protected GetAuthHeaderJson() {
    return {
      headers: new HttpHeaders({
        'Content-Type': 'application/json',
        Authorization: 'Bearer ' + this.LocalStorage.getUserToken(),
      }),
    };
  }

  protected GetAuthHeaderTokenJson(token: string) {
    return {
      headers: new HttpHeaders({
        'Content-Type': 'application/json',
        Authorization: 'Bearer ' + token,
      }),
    };
  }

  protected GetHeaderUnlercoded() {
    return {
      headers: new HttpHeaders({
        'Content-Type': 'application/x-www-form-urlencoded',
      }),
    };
  }

  protected extractData(response: any) {
    if (response?.data?.result && response?.status === 0) {
      return response.data.result;
    }

    return response || {};
  }

}
