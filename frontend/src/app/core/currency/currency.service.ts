import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject } from 'rxjs';
import { AuthService } from '../auth/auth.service';
import { environment } from '../../../environments/environment';

export interface CurrencyRate {
  currencyCode: string;
  currencyName: string;
  rateToCad:    number;
  symbol:       string;
  lastUpdated:  string;
}

const CAD_DEFAULT: CurrencyRate = {
  currencyCode: 'CAD',
  currencyName: 'Canadian Dollar',
  rateToCad:    1,
  symbol:       '$',
  lastUpdated:  new Date().toISOString()
};

const STORAGE_KEY = 'tc_currency_code';

@Injectable({ providedIn: 'root' })
export class CurrencyService {
  readonly rates$            = new BehaviorSubject<CurrencyRate[]>([]);
  readonly selectedCurrency$ = new BehaviorSubject<CurrencyRate>(CAD_DEFAULT);

  constructor(private http: HttpClient, private auth: AuthService) {}

  get currentCurrency(): string {
    return this.selectedCurrency$.value.currencyCode;
  }

  initialize(): void {
    this.http.get<CurrencyRate[]>(`${environment.apiUrl}/currencies`).subscribe({
      next: rates => {
        this.rates$.next(rates);
        const savedCode = localStorage.getItem(STORAGE_KEY);
        const targetCode = savedCode
          ?? (this.auth.isLoggedIn() ? this.auth.getPreferredCurrency() : null)
          ?? 'CAD';
        const found = rates.find(r => r.currencyCode === targetCode);
        if (found) this.selectedCurrency$.next(found);
      },
      error: () => {}
    });
  }

  selectCurrency(code: string): void {
    const rate = this.rates$.value.find(r => r.currencyCode === code);
    if (!rate) return;
    this.selectedCurrency$.next(rate);
    localStorage.setItem(STORAGE_KEY, code);
  }

  setUserCurrency(code: string): void {
    this.selectCurrency(code);
    if (this.auth.isLoggedIn()) {
      this.http
        .put(`${environment.apiUrl}/profile/currency`, { currencyCode: code })
        .subscribe({ error: () => {} });
    }
  }

  convert(amountCAD: number, targetCode: string): number {
    const rate = this.rates$.value.find(r => r.currencyCode === targetCode);
    if (!rate || rate.rateToCad === 0) return amountCAD;
    return amountCAD * rate.rateToCad;
  }

  /** Format a CAD amount based on the currently selected currency. */
  format(amountCAD: number | null | undefined): string {
    if (amountCAD == null) return '...';
    const rate = this.selectedCurrency$.value;
    const value = amountCAD * rate.rateToCad;
    
    return new Intl.NumberFormat('en-CA', {
      style: 'currency',
      currency: rate.currencyCode,
      currencyDisplay: 'symbol'
    }).format(value);
  }
}
