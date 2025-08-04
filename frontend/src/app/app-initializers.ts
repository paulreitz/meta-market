import { inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { CurrencyStore } from './shared/store/currency-store';

export const initializeCurrencies = () => {
    const store = inject(CurrencyStore);
    return firstValueFrom(store.loadCurrencies());
}
