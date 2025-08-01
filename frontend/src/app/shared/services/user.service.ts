import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { User, UpdateUsernameRequest, UpdateUserBioRequest, UpdateUserWebsiteRequest } from '../types/user';
import { AuthService } from './auth.service';
import { environment } from '../../../environments/environment';

@Injectable({
    providedIn: 'root'
})
export class UserService {
    private apiUrl = `${environment.apiBaseUrl}user`;

    constructor(private http: HttpClient, private authService: AuthService) {}

    getUserProfile(walletAddress: string): Observable<any> {
        return this.http.get<any>(`${this.apiUrl}/get/${walletAddress}`);
    }

    updateUsername(newUsername: string): Observable<any> {
        const body: UpdateUsernameRequest = { username: newUsername };
        return this.http.put<any>(`${this.apiUrl}/username`, body).pipe(
            tap(updatedUser => this.authService.updateUser(updatedUser))
        );
    }

    updateUserBio(newBio: string): Observable<any> {
        const body: UpdateUserBioRequest = { userBio: newBio };
        return this.http.put<any>(`${this.apiUrl}/bio`, body).pipe(
            tap(updatedUser => this.authService.updateUser(updatedUser))
        );
    }

    updateUserWebsite(newWebsite: string): Observable<any> {
        const body: UpdateUserWebsiteRequest = { userWebsite: newWebsite };
        return this.http.put<any>(`${this.apiUrl}/website`, body).pipe(
            tap(updatedUser => this.authService.updateUser(updatedUser))
        );
    }

    updatePfp(file: File): Observable<any> {
        const formData = new FormData();
        formData.append('file', file);

        return this.http.post<any>(`${this.apiUrl}/pfp`, formData).pipe(
            tap(updatedUser => this.authService.updateUser(updatedUser))
        );
    }

    updateBanner(file: File): Observable<any> {
        const formData = new FormData();
        formData.append('file', file);

        return this.http.post<any>(`${this.apiUrl}/banner`, formData).pipe(
            tap(updatedUser => this.authService.updateUser(updatedUser))
        );
    }
}
