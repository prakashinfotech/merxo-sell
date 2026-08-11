import { ChangeDetectionStrategy, Component } from '@angular/core';
import { Router } from '@angular/router';
import { CartService } from '../../../core/cart/cart.service';
import { CartDrawerService } from '../../../core/cart/cart-drawer.service';
import { CurrencyService } from '../../../core/currency/currency.service';

@Component({
  selector: 'app-cart-drawer',
  templateUrl: './cart-drawer.html',
  styleUrls: ['./cart-drawer.scss'],
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CartDrawer {
  constructor(
    readonly cart: CartService,
    readonly drawer: CartDrawerService,
    readonly currency: CurrencyService,
    private router: Router,
  ) {}

  checkout(): void {
    this.drawer.close();
    this.router.navigate(['/cart/checkout']);
  }

  viewCart(): void {
    this.drawer.close();
    this.router.navigate(['/cart']);
  }
}
