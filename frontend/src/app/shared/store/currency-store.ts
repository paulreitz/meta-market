import { inject } from '@angular/core';
import { signalStore, withState, withMethods, patchState } from '@ngrx/signals';
import { tap } from 'rxjs';
import { CurrencyService } from '../services/currency.service';

interface CurrencyState {
    currencies: string[];
}

export const CurrencyStore = signalStore(
    { providedIn: 'root' },
    withState<CurrencyState>({ currencies: [] }),
    withMethods((store) => {
        const currencyService = inject(CurrencyService);
        return {
            loadCurrencies() {
                return currencyService.getSupportedCurrencies().pipe(
                    tap((currencies: string[]) => patchState(store, { currencies }))
                );
            },
        };
    })
);
