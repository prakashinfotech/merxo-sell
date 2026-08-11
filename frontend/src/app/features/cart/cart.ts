import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { DialogService } from '../../core/dialog/dialog.service';
import { Router } from '@angular/router';
import { CartService, CartItemDto } from '../../core/cart/cart.service';
import { CurrencyService } from '../../core/currency/currency.service';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-cart',
  templateUrl: './cart.html',
  styleUrls: ['./cart.scss'],
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class Cart implements OnInit {
  items$: Observable<CartItemDto[]>;
  total$: Observable<number>;
  isItemsCollapsed = false;

  // Delivery & Pickup Options
  showPickupModal = false;

  deliveryOptions = [
    {
      id: 'standard',
      title: 'Standard Home Delivery',
      subtitle: '5–10 Business Days',
      price: 'FREE',
      icon: 'fa-truck',
      desc: 'Delivered directly to your shipping address.'
    },
    {
      id: 'express',
      title: 'Express Priority Shipping',
      subtitle: '2–3 Business Days',
      price: 'CA$ 9.99',
      icon: 'fa-bolt',
      desc: 'Fast priority dispatch with direct courier tracking.'
    },
    {
      id: 'pickup',
      title: 'In-Store / Hub Pickup',
      subtitle: 'Ready in 2 Hours',
      price: 'FREE',
      icon: 'fa-store',
      desc: 'Collect at your nearest MerxoSell Express Hub.'
    }
  ];

  get selectedDeliveryMode(): 'standard' | 'express' | 'pickup' {
    return this.cartService.currentDeliveryMode;
  }

  get shippingAmountCAD(): number {
    return this.cartService.shippingAmountCAD;
  }

  getFinalTotal(subTotal: number | null): number {
    return (subTotal ?? 0) + this.shippingAmountCAD;
  }

  toggleCollapse(): void {
    this.isItemsCollapsed = !this.isItemsCollapsed;
    this.cdr.markForCheck();
  }

  openPickupModal(): void {
    this.showPickupModal = true;
    this.cdr.markForCheck();
  }

  closePickupModal(): void {
    this.showPickupModal = false;
    this.cdr.markForCheck();
  }

  selectDeliveryMode(mode: string): void {
    const validMode = mode as 'standard' | 'express' | 'pickup';
    this.cartService.setDeliveryMode(validMode);
    this.cdr.markForCheck();
  }

  confirmDeliveryOption(): void {
    this.showPickupModal = false;
    this.cdr.markForCheck();
  }

  constructor(
    public cartService: CartService,
    public currencyService: CurrencyService,
    private cdr: ChangeDetectorRef,
    private dialog: DialogService,
    private router: Router
  ) {
    this.items$ = this.cartService.items$;
    this.total$ = this.cartService.total$;
  }

  ngOnInit(): void {
    this.cartService.fetchCart().subscribe(() => this.cdr.markForCheck());
  }

  updateQuantity(itemId: number, currentQty: number, delta: number): void {
    const newQty = currentQty + delta;
    if (newQty < 1) return;
    
    this.cartService.updateItem(itemId, { quantity: newQty }).subscribe(() => {
      this.cdr.markForCheck();
    });
  }

  removeItem(itemId: number): void {
    this.dialog.ask('Remove Item', 'Remove this item from your cart?', true)
      .subscribe(confirmed => {
        if (!confirmed) return;
        
        this.cartService.removeItem(itemId).subscribe(() => {
          this.cdr.markForCheck();
        });
      });
  }

  goToCheckout(): void {
    this.router.navigate(['/cart/checkout']);
  }
}
