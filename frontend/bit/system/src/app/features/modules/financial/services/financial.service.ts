import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BaseService } from '../../../../core/services/base.service';
import { catchError, map, Observable } from 'rxjs';
import { FinancialOriginModel } from '../models/financial.model';

@Injectable({
  providedIn: 'root',
})
export class FinancialService extends BaseService {
  constructor(private httpClient: HttpClient) {
    super();
  }

  getFinancialOrign(): Observable<FinancialOriginModel[]> {
    const url = `${this.UrlServiceApi}FinancialOrigin`;

    return this.httpClient.get<any>(url, this.GetAuthHeaderJson()).pipe(
      map((response) => this.extractData(response)),
      catchError((error) => {
        throw error;
      }),
    );
  }
}
