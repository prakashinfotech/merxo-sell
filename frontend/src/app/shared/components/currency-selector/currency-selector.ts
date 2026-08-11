import { Component, ChangeDetectionStrategy, HostListener, ChangeDetectorRef } from '@angular/core';
import { CurrencyService } from '../../../core/currency/currency.service';

@Component({
  selector: 'app-currency-selector',
  templateUrl: './currency-selector.html',
  styleUrls: ['./currency-selector.css'],
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CurrencySelector {
  isOpen = false;
  readonly currencies$: typeof this.cs.rates$;
  readonly selected$:   typeof this.cs.selectedCurrency$;

  constructor(readonly cs: CurrencyService, private cdr: ChangeDetectorRef) {
    this.currencies$ = cs.rates$;
    this.selected$   = cs.selectedCurrency$;
  }

  toggleDropdown(): void {
    this.isOpen = !this.isOpen;
    this.cdr.markForCheck();
  }

  selectCurrency(code: string): void {
    this.cs.setUserCurrency(code);
    this.isOpen = false;
    this.cdr.markForCheck();
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    if (!target.closest('.currency-selector')) {
      this.isOpen = false;
      this.cdr.markForCheck();
    }
  }
}
