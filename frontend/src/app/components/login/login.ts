import { Component } from '@angular/core';
import { AuthService } from '../../shared/services/auth.service';

@Component({
    selector: 'app-login',
    imports: [],
    templateUrl: './login.html',
    styleUrl: './login.scss'
})
export class Login {

    constructor(private authService: AuthService) {}

    onLogin() {
        this.authService.login().subscribe({
            next: (response) => console.log('logged in:', response),
            error: (err) => console.error('Login failed:', err)
        });
    }

}
