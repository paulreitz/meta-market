import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { MatProgressSpinner } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { UserService } from '../../shared/services/user.service';
import { AuthService } from '../../shared/services/auth.service';
import { User } from '../../shared/types/user';
import { Subject, takeUntil } from 'rxjs';

type editState = 'display' | 'edit' | 'loading';

@Component({
  selector: 'app-profile',
  imports: [MatProgressSpinner, MatCardModule, MatButtonModule, MatIconModule, MatInputModule, FormsModule],
  templateUrl: './profile.html',
  styleUrl: './profile.scss'
})
export class Profile implements OnInit, OnDestroy {
    private activatedRoute = inject(ActivatedRoute);
    destroy$ = new Subject<void>();
    profile = signal<User | null>(null);
    isLoading = signal<boolean>(true);
    isOwnProfile = signal<boolean>(false);

    // Edit signals
    usernameState = signal<editState>('display');
    usernameValue = signal<string>('');

    bioState = signal<editState>('display');
    bioValue = signal<string>('');

    websiteState = signal<editState>('display');
    websiteValue = signal<string>('');

    bannerUploadLoading = signal<boolean>(false);
    pfpUploadLoading = signal<boolean>(false);

    constructor(private userService: UserService, private authService: AuthService) {}

    ngOnInit(): void {
        this.activatedRoute.paramMap.subscribe((paramMap) => {
            const address = paramMap.get('walletAddress');
            if (!!address) {
                this.userService.getUserProfile(address).subscribe({
                    next: (user: User) => {
                        this.profile.set(user);
                        this.isLoading.set(false);
                        this.subscribeToUser();
                    },
                    error: (err) => {
                        this.profile.set(null);
                        this.isLoading.set(false);
                        this.isOwnProfile.set(false);
                    }
                })
            } else {
                this.profile.set(null);
                this.isLoading.set(false);
                this.isOwnProfile.set(false);
            }
        })
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    private subscribeToUser(): void {
        this.authService.user$.pipe(takeUntil(this.destroy$))
            .subscribe((user: User) => {
                if (!user || !this.profile()) {
                    this.isOwnProfile.set(false);
                } else {
                    this.isOwnProfile.set(user.walletAddress === this.profile()?.walletAddress);

                    if (this.isOwnProfile()) {
                        this.usernameValue.set(user.username);
                        this.bioValue.set(user.bio);
                        this.websiteValue.set(user.website);
                    } else {
                        this.usernameState.set('display');
                        this.bioState.set('display');
                        this.websiteState.set('display');
                    }

                }
            });
    }

    enableEditUsername(): void {
        if (this.isOwnProfile()) {
            this.usernameState.set('edit');
        }
    }

    saveUsername(): void {
        if (this.isOwnProfile()) {
            this.usernameState.set('loading');
            this.userService.updateUsername(this.usernameValue()).subscribe({
                next: (user: User) => {
                    this.profile.set(user);
                },
                error: (err) => {
                    // let there be toast!
                },
                complete: () => {
                    this.usernameState.set('display');
                }
            })
        }
    }

    enableEditBio(): void {
        if (this.isOwnProfile()) {
            this.bioState.set('edit');
        }
    }

    saveBio(): void {
        if (this.isOwnProfile()) {
            this.bioState.set('loading');
            this.userService.updateUserBio(this.bioValue()).subscribe({
                next: (user: User) => {
                    this.profile.set(user);
                },
                error: (err) => {
                    // let there be toast!
                },
                complete: () => {
                    this.bioState.set('display');
                }
            })
        }
    }

    enableEditWebsite(): void {
        if (this.isOwnProfile()) {
            this.websiteState.set('edit');
        }
    }

    saveWebsite(): void {
        if (this.isOwnProfile()) {
            this.websiteState.set('loading');
            this.userService.updateUserWebsite(this.websiteValue()).subscribe({
                next: (user: User) => {
                    this.profile.set(user);
                },
                error: (err) => {
                    // let there be toast!
                },
                complete: () => {
                    this.websiteState.set('display');
                }
            })
        }
    }

    triggerUpload(uploadInput: HTMLInputElement): void {
        uploadInput.click();
    }

    uploadPfp(e: Event): void {
        const input = e.target as HTMLInputElement;
        if (input.files && input.files[0] && this.isOwnProfile()) {
            this.pfpUploadLoading.set(true);
            this.userService.updatePfp(input.files[0]).subscribe({
                next: (user: User) => {
                    this.profile.set(user);
                },
                error: (err) => {
                    // Let there be toast!
                },
                complete: () => {
                    this.pfpUploadLoading.set(false);
                    input.value = '';
                }
            })
        }
    }

    uploadBanner(e: Event): void {
        const input = e.target as HTMLInputElement;
        if (input.files && input.files[0] && this.isOwnProfile()) {
            this.bannerUploadLoading.set(true);
            this.userService.updateBanner(input.files[0]).subscribe({
                next: (user: User) => {
                    this.profile.set(user);
                },
                error: (err) => {
                    // Let there be toast
                },
                complete: () => {
                    this.bannerUploadLoading.set(false);
                    input.value = '';
                }
            })
        }
    }

}
