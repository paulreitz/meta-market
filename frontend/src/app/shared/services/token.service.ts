import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { StorageKeys } from '../types/storage-keys';
import { Token } from '../types/auth-types';

@Injectable({
    providedIn: 'root'
})
export class TokenService {
    private tokenSubject = new BehaviorSubject<Token>(null);
    public token$ = this.tokenSubject.asObservable();

    constructor() {
        const storedToken = localStorage.getItem(StorageKeys.AuthToken);
        if (storedToken) {
            this.tokenSubject.next(storedToken);
        }
    }

    setToken(token: Token): void {
        if (token) {
            localStorage.setItem(StorageKeys.AuthToken, token);
            this.tokenSubject.next(token);
        }
    }

    getToken(): Token {
        return this.tokenSubject.value;
    }

    clearToken(): void {
        this.tokenSubject.next(null);
        localStorage.removeItem(StorageKeys.AuthToken);
    }
}
