import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, NavigationEnd } from '@angular/router';
import { BehaviorSubject, Observable, from, switchMap, catchError, tap, of } from 'rxjs';
import { filter } from 'rxjs/operators';
import { ethers } from 'ethers';
import { StorageKeys } from '../types/storage-keys';
import { Token } from '../types/auth-types';
import { TokenService } from './token.service';
import { User } from '../types/user';
import { environment } from '../../../environments/environment';

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    private apiUrl = `${environment.apiBaseUrl}auth`;
    private userSubject = new BehaviorSubject<any>(null);
    public user$ = this.userSubject.asObservable();

    private guardedRoute: string[] = [];
    private currentRoute = '';

    constructor(
        private http: HttpClient,
        private tokenService: TokenService,
        private router: Router
    ) {
        this.router.events.pipe(
            filter(event => event instanceof NavigationEnd)
        )
        .subscribe((event: NavigationEnd) => {
            this.currentRoute = event.urlAfterRedirects;
        });


        const storedToken = localStorage.getItem(StorageKeys.AuthToken);
        if (this.tokenService.getToken()) {
            this.restoreUserFromToken().subscribe({
                next: (response) => {
                    if (response) {
                        this.userSubject.next(response.user);
                    } else {
                        this.logout();
                    }
                },
                error: (error) => {
                    console.error('Failed to restore user form token:', error);
                    this.logout();
                }
            });
        }
    }

    private restoreUserFromToken(): Observable<any> {
        return this.http.get<any>(`${this.apiUrl}/me`).pipe(
            catchError(error => {
                console.error('Error restoring user:', error);
                return of(null);
            })
        )
    }

    async connectWallet(): Promise<string | null> {
        if (typeof window.ethereum !== 'undefined') {
            try {
                const accounts = await window.ethereum.request({ method: 'eth_requestAccounts' });
                return accounts[0];
            } catch (error) {
                console.error('Wallet connection failed:', error);
                return null;
            }
        } else {
            console.error('MetaMask not detected');
            return null;
        }
    }

    login(): Observable<any> {
        return from(this.connectWallet()).pipe(
            switchMap(walletAddress => {
                if (!walletAddress) {
                     throw new Error('No wallet address');
                }
                return this.getNonce(walletAddress).pipe(
                    switchMap(nonce => {
                        return this.signMessage(walletAddress, nonce);
                    }),
                    switchMap(signatureData => {
                        return this.verifySignature(signatureData);
                    }),
                    tap(response => {
                        this.userSubject.next(response.user);
                        const token = response.token;
                        if (token) {
                            this.tokenService.setToken(token);
                        }
                    })
                );
            }),
            catchError(error => {
                console.error('Login error:', error);
                throw error;
            })
        );
    }

    private getNonce(walletAddress: string): Observable<string> {
        return this.http.get(`${this.apiUrl}/nonce/${walletAddress}`, { responseType: 'text'});
    }

    private async signMessage(walletAddress: string, nonce: string): Promise<{ walletAddress: string, nonce: string, signature: string }> {
        const message = `Login to Marketplace with nonce: ${nonce}`;
        const provider = new ethers.BrowserProvider(window.ethereum);
        const signer = await provider.getSigner();
        const signature = await signer.signMessage(message);
        return { walletAddress, nonce, signature };
    }

    private verifySignature(data: { walletAddress: string, nonce: string, signature: string }) {
        return this.http.post<any>(`${this.apiUrl}/verify`, data);
    }

    logout() {
        const isOnGuardedRoute = this.guardedRoute.some(route => this.currentRoute.startsWith(route));

        this.userSubject.next(null);
        this.tokenService.clearToken();

        if (isOnGuardedRoute) {
            this.router.navigate(['/']);
        }
    }

    isLoggedIn(): boolean {
        return !!this.userSubject.value;
    }

    updateUser(user: User): void {
        this.userSubject.next(user);
    }

    addGaurdedRoute(route: string): void {
        if (!this.guardedRoute.some(i => i === route)) {
            this.guardedRoute.push(route);
        }
    }
}
