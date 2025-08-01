import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class CurrencyService {
    private readonly apiUrl: string = `${environment.apiBaseUrl}currency`;

    constructor(private httpClient: HttpClient) {}

    public getSupportedCurrencies(): Observable<string[]> {
        return this.httpClient.get<string[]>(`${this.apiUrl}/supported`);
    }
}
