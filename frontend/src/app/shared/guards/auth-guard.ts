import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);

    // Add the guarded path to the auth service so it knows to redirect when logging out from this page.
    if (route.url[0]?.path) {
        authService.addGaurdedRoute(`/${route.url[0].path}`);
    }

    if (authService.isLoggedIn()) {
        return true;
    } else {
        router.navigate(['/']);
        return false;
    }
};
