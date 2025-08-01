export interface User {
    walletAddress: string;
    username: string;
    pfpUrl: string;
    bannerUrl: string;
    bio: string;
    website: string;
    ageVerified: boolean;
    adultContentEnabled: boolean;
    joined: Date;
}

export interface UpdateUsernameRequest {
    username: string;
}

export interface UpdateUserBioRequest {
    userBio: string;
}

export interface UpdateUserWebsiteRequest {
    userWebsite: string;
}
