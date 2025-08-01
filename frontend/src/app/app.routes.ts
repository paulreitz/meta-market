import { Routes } from '@angular/router';

import { authGuard } from './shared/guards/auth-guard';

import { Home } from './pages/home/home';
import { NotFound } from './pages/not-found/not-found';
import { Profile } from './pages/profile/profile';
import { Dashboard } from './pages/dashboard/dashboard';

export const routes: Routes = [
    { path: '', component: Home },
    { path: 'profile/:walletAddress', component: Profile },
    { path: 'dashboard', component: Dashboard, canActivate: [authGuard]},
    { path: '**', component: NotFound }
];
