import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CouponService, CouponSummaryDto } from '../../core/coupon/coupon.service';
import { CurrencyService } from '../../core/currency/currency.service';

@Component({
  selector: 'app-coupons',
  templateUrl: './coupons.html',
  styleUrls: ['./coupons.scss'],
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class Coupons implements OnInit {
  coupons: CouponSummaryDto[] = [];
  loading = true;

  constructor(
    private couponsApi: CouponService,
    public currencyService: CurrencyService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.couponsApi.getAvailable().subscribe({
      next: rows => { this.coupons = rows; this.loading = false; this.cdr.markForCheck(); },
      error: () => { this.loading = false; this.cdr.markForCheck(); }
    });
  }

  discount(coupon: CouponSummaryDto): string {
    return coupon.discountType === 'Percentage'
      ? `${coupon.discountValue}% OFF`
      : `${coupon.discountValue} OFF`;
  }
}
