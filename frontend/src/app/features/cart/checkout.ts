import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { combineLatest, of } from 'rxjs';
import { switchMap, take, tap, catchError } from 'rxjs/operators';
import { ProfileService, AddressDto, AddressCreateUpdateDto, PaymentMethodDto } from '../../core/profile/profile.service';
import { CartService, CartItemDto } from '../../core/cart/cart.service';
import {
  OrderService, CreateOrderDto, CreateOrderItemDto,
} from '../../core/order/order.service';
import { CurrencyService } from '../../core/currency/currency.service';
import { CouponService, CouponSummaryDto, CouponValidationResultDto } from '../../core/coupon/coupon.service';
import { RazorpayPaymentService } from '../../core/payment/razorpay.service';

@Component({
  selector: 'app-checkout',
  templateUrl: './checkout.html',
  styleUrls: ['./checkout.scss'],
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Checkout implements OnInit {
  addresses: AddressDto[] = [];
  paymentMethods: PaymentMethodDto[] = [];
  selectedAddressId: number | null = null;
  selectedPaymentId: string | number = 'razorpay';
  loading = true;
  submitting = false;
  error = '';
  couponCode = '';
  couponMessage = '';
  couponError = '';
  appliedCoupon: CouponValidationResultDto | null = null;
  availableCoupons: CouponSummaryDto[] = [];

  // Inline new-address form state
  showNewAddressForm = false;
  savingAddress = false;
  newAddressError = '';
  newAddress: AddressCreateUpdateDto = this.blankAddress();

  private blankAddress(): AddressCreateUpdateDto {
    return {
      fullName: '', addressLine1: '', addressLine2: '', city: '',
      state: '', postalCode: '', country: 'CA',
      phone: '', label: 'Home', isDefault: this.addresses.length === 0,
    };
  }

  toggleNewAddressForm(): void {
    this.showNewAddressForm = !this.showNewAddressForm;
    this.newAddressError = '';
    if (this.showNewAddressForm) this.newAddress = this.blankAddress();
    this.cdr.markForCheck();
  }

  saveNewAddress(): void {
    this.newAddressError = '';
    const a = this.newAddress;
    if (!a.fullName || !a.addressLine1 || !a.city || !a.state || !a.postalCode || !a.country) {
      this.newAddressError = 'Please fill in every required field.';
      this.cdr.markForCheck();
      return;
    }

    this.savingAddress = true;
    this.cdr.markForCheck();

    this.profileService.addAddress(a).subscribe({
      next: created => {
        this.addresses = [...this.addresses, created];
        this.selectedAddressId = created.addressId;
        this.showNewAddressForm = false;
        this.savingAddress = false;
        this.cdr.markForCheck();
      },
      error: err => {
        this.newAddressError = err?.error?.error || 'Could not save address. Check the postal code format.';
        this.savingAddress = false;
        this.cdr.markForCheck();
      }
    });
  }

  constructor(
    private profileService: ProfileService,
    public  cartService: CartService,
    private orderService: OrderService,
    private couponService: CouponService,
    public  currencyService: CurrencyService,
    private razorpayService: RazorpayPaymentService,
    private router: Router,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    // Load addresses + ensure cart is fetched from server so the latest items
    // are visible if the buyer added them in another tab.
    this.profileService.getProfile().pipe(
      tap(profile => {
        this.addresses = profile.addresses;
        this.paymentMethods = profile.paymentMethods || [];
        
        const defaultAddr = this.addresses.find(a => a.isDefault);
        this.selectedAddressId = defaultAddr?.addressId
          ?? this.addresses[0]?.addressId
          ?? null;
      }),
      switchMap(() => this.cartService.fetchCart().pipe(catchError(() => of(null)))),
    ).subscribe({
      next: () => {
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.loading = false;
        this.cdr.markForCheck();
      },
    });

    // Redirect back to cart if it ends up empty after the fetch.
    this.cartService.items$.pipe(take(1)).subscribe(items => {
      if (!this.loading && items.length === 0) {
        this.router.navigate(['/cart']);
      }
    });

    this.couponService.getAvailable().subscribe({
      next: rows => { this.availableCoupons = rows; this.cdr.markForCheck(); },
      error: () => undefined,
    });
  }

  selectAddress(id: number): void {
    this.selectedAddressId = id;
    this.cdr.markForCheck();
  }

  selectPayment(id: string | number): void {
    this.selectedPaymentId = id;
    this.cdr.markForCheck();
  }

  applyCoupon(): void {
    this.couponError = '';
    this.couponMessage = '';
    this.appliedCoupon = null;
    const code = this.couponCode.trim();
    if (!code) {
      this.couponError = 'Enter a coupon code.';
      this.cdr.markForCheck();
      return;
    }

    this.cartService.total$.pipe(take(1)).subscribe(total => {
      this.couponService.apply(code, total).subscribe({
        next: result => {
          this.appliedCoupon = result;
          this.couponCode = result.couponCode || code.toUpperCase();
          this.couponMessage = result.message || 'Coupon applied.';
          this.cdr.markForCheck();
        },
        error: err => {
          this.couponError = err?.error?.message || err?.error?.Message || 'Coupon is not valid for this order.';
          this.cdr.markForCheck();
        }
      });
    });
  }

  removeCoupon(): void {
    this.appliedCoupon = null;
    this.couponCode = '';
    this.couponMessage = '';
    this.couponError = '';
    this.cdr.markForCheck();
  }

  placeOrder(): void {
    if (!this.selectedAddressId) {
      this.error = 'Please select a shipping address.';
      this.cdr.markForCheck();
      return;
    }

    this.submitting = true;
    this.error = '';
    this.cdr.markForCheck();

    // Snapshot cart items + total in one tick so the payload is consistent.
    combineLatest([
      this.cartService.items$.pipe(take(1)),
      this.cartService.total$.pipe(take(1)),
    ]).subscribe(([items, total]) => {
      if (!items.length) {
        this.submitting = false;
        this.error = 'Your cart is empty.';
        this.cdr.markForCheck();
        return;
      }

      const orderItems: CreateOrderItemDto[] = items.map((i: CartItemDto) => ({
        productId: i.productId,
        variantId: i.variantId,
        quantity:  i.quantity,
      }));

      const shippingFee = this.cartService.shippingAmountCAD;
      const payableTotal = Math.max(0, total + shippingFee - (this.appliedCoupon?.discountAmount || 0));

      const selectedAddress = this.addresses.find(a => a.addressId === this.selectedAddressId);

      const createOrderDto: CreateOrderDto = {
        addressId:       this.selectedAddressId!,
        currencyCode:    this.currencyService.currentCurrency,
        displayTotal:    payableTotal,
        shippingAmount:  shippingFee,
        discountAmount:  this.appliedCoupon?.discountAmount || 0,
        couponCode:      this.appliedCoupon?.couponCode,
        items:           orderItems,
      };

      if (this.selectedPaymentId === 'razorpay') {
        // Trigger Razorpay Payment Flow
        this.razorpayService.createOrder({
          amount: payableTotal,
          currency: this.currencyService.currentCurrency || 'INR',
          receipt: `rcpt_order_${Date.now()}`
        }).subscribe({
          next: (rzpOrder) => {
            this.razorpayService.openRazorpayCheckout(
              rzpOrder,
              'suthary980@gmail.com',
              selectedAddress?.fullName || 'Customer',
              selectedAddress?.phone || ''
            ).then(payResponse => {
              // Verify Payment Signature
              this.razorpayService.verifyPayment({
                razorpayOrderId: payResponse.razorpay_order_id,
                razorpayPaymentId: payResponse.razorpay_payment_id,
                razorpaySignature: payResponse.razorpay_signature
              }).subscribe({
                next: () => {
                  const finalDto: CreateOrderDto = {
                    ...createOrderDto,
                    paymentMethod: 'Razorpay',
                    paymentTransactionId: payResponse.razorpay_payment_id
                  };
                  this.finalizeOrderCreation(finalDto);
                },
                error: (vErr) => {
                  this.submitting = false;
                  this.error = vErr?.error?.error || 'Payment verification failed. Please try again.';
                  this.cdr.markForCheck();
                }
              });
            }).catch(err => {
              this.submitting = false;
              this.error = err?.message || 'Payment was not completed.';
              this.cdr.markForCheck();
            });
          },
          error: (err) => {
            this.submitting = false;
            this.error = err?.error?.error || 'Could not initialize Razorpay payment. Please try again.';
            this.cdr.markForCheck();
          }
        });
      } else {
        // Direct COD flow
        const codDto: CreateOrderDto = {
          ...createOrderDto,
          paymentMethod: 'COD',
          paymentTransactionId: undefined
        };
        this.finalizeOrderCreation(codDto);
      }
    });
  }

  private finalizeOrderCreation(dto: CreateOrderDto): void {
    this.orderService.createOrder(dto).pipe(
      switchMap(order =>
        this.cartService.clearCart().pipe(
          catchError(() => of(null)),
          tap(() => order),
        )),
    ).subscribe({
      next: () => {
        this.submitting = false;
        this.router.navigate(['/orders']);
      },
      error: err => {
        this.submitting = false;
        this.error = err?.error?.error
          ?? 'Failed to place order. Please review your cart and try again.';
        this.cdr.markForCheck();
      },
    });
  }
}
