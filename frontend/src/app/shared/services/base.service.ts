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

  protected serviceError(response: Response | any) {
    let customErrors: string[] = [];

    if (response instanceof HttpErrorResponse) {
      // Tratamento para erro de comunicação desconhecida
      if (response.statusText === 'Unknown Error') {
        return throwError(
          () => 'Falha na comunicação - tente novamente mais tarde',
        );
      }
      // Tratamento para status 400 (Bad Request)
      else if (response.status === 400) {
        if (response.error && response.error.errors) {
          // Se houver uma chave específica 'Mensagens', por exemplo, com a mensagem "Usuário não encontrado"
          if (response.error.errors.Mensagens) {
            customErrors = response.error.errors.Mensagens;
          } else {
            // Adiciona uma mensagem genérica para erros de validação
            customErrors.push('Erros de validação');
          }
        } else {
          customErrors.push('Erro não especificado');
        }
        return throwError(() => customErrors.join(', '));
      }
      // Tratamento para erros de autorização
      else if (response.status === 401) {
        this.LocalStorage.clearLocaleUserData();
        //window.location.href = '/login-form';
        return throwError(() => ({
          status: 401,
          message: '401 - Sem autorização',
        }));
      } else if (response.status === 403) {
        return throwError(() => ({
          status: 403,
          message: '403 - Sem autorização',
        }));
      }
    }

    // Caso o erro não se enquadre nas condições acima, loga o erro e retorna o erro bruto
    console.error(response.error);
    return throwError(() => response.error);
  }
}
