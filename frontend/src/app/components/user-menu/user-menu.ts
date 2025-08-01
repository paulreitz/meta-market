import { Component, signal, Signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';

import { AuthService } from '../../shared/services/auth.service';
import { User } from '../../shared/types/user';

@Component({
  selector: 'app-user-menu',
  imports: [RouterLink, MatProgressSpinnerModule, MatButtonModule, MatIconModule, MatMenuModule, MatDividerModule],
  templateUrl: './user-menu.html',
  styleUrl: './user-menu.scss'
})
export class UserMenu {
    user: Signal<User | null>;
    isLoading = signal(false);

    constructor(private authService: AuthService) {
        this.user = toSignal(this.authService.user$, { initialValue: null });
    }

    login() {
        this.isLoading.set(true);
        this.authService.login().subscribe({
            next: () => this.isLoading.set(false),
            error: () => this.isLoading.set(false),
            complete: () => this.isLoading.set(false)
        });
    }

    logout() {
        this.isLoading.set(true);
        this.authService.logout();
        setTimeout(() => this.isLoading.set(false), 300);
    }
}
