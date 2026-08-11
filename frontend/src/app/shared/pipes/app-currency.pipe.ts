import { Pipe, PipeTransform } from '@angular/core';
import { CurrencyRate, CurrencyService } from '../../core/currency/currency.service';

@Pipe({ name: 'appCurrency', standalone: false })
export class AppCurrencyPipe implements PipeTransform {
  constructor(private cs: CurrencyService) {}

  /**
   * Converts a CAD amount to the active display currency.
   *
   * Accepts an optional `currency` argument so templates can pass
   * `(selectedCurrency$ | async)` and trigger re-evaluation on every
   * currency change while keeping the pipe pure:
   *
   *   {{ product.basePrice | appCurrency }}                         — static
   *   {{ product.basePrice | appCurrency:(cs.selectedCurrency$ | async) }} — reactive
   *
   * Returns empty string for null / undefined input.
   */
  transform(valueCAD: number | null | undefined, currency?: CurrencyRate | null): string {
    if (valueCAD == null) return '';

    const rate     = currency ?? this.cs.selectedCurrency$.value;
    const converted = valueCAD * rate.rateToCad;
    return this.format(converted, rate);
  }

  private format(amount: number, rate: CurrencyRate): string {
    // INR and other high-denomination currencies look better without decimals
    const decimals = ['INR', 'JPY', 'KRW'].includes(rate.currencyCode) ? 0 : 2;
    const formatted = amount.toLocaleString('en-CA', {
      minimumFractionDigits: decimals,
      maximumFractionDigits: decimals
    });

    // CAD gets the "CA$" prefix to distinguish from USD/AUD; others get code suffix
    if (rate.currencyCode === 'CAD') {
      return `CA$${formatted}`;
    }
    return `${rate.symbol}${formatted} ${rate.currencyCode}`;
  }
}
