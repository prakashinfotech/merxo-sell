import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { Router } from '@angular/router';
import { CurrencyService } from '../../../core/currency/currency.service';
import { CartService } from '../../../core/cart/cart.service';
import { CartDrawerService } from '../../../core/cart/cart-drawer.service';
import { AuthService } from '../../../core/auth/auth.service';
import { ProductListItem } from '../../models/product.models';

@Component({
  selector: 'app-product-card',
  templateUrl: './product-card.html',
  styleUrl: './product-card.scss',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductCard {
  @Input({ required: true }) product!: ProductListItem;
  @Output() editProduct  = new EventEmitter<ProductListItem>();
  @Output() deleteProduct = new EventEmitter<ProductListItem>();

  added   = false;
  wishlist = false;

  onImgError(event: Event): void {
    const img = event.target as HTMLImageElement;
    if (img) {
      img.onerror = null;
      img.src = '/assets/images/white-tshirt.png';
    }
  }

  toggleWishlist(event: Event): void {
    event.preventDefault();
    event.stopPropagation();
    this.wishlist = !this.wishlist;
  }

  constructor(
    readonly cs: CurrencyService,
    readonly auth: AuthService,
    private cart: CartService,
    private cartDrawer: CartDrawerService,
    private router: Router
  ) {}

  get isAdmin(): boolean { return this.auth.isAdmin(); }

  onAddToCart(event: Event): void {
    event.preventDefault();
    event.stopPropagation();

    // Anonymous shoppers (or non-Buyers) are bounced to login. The intended
    // item is queued so it gets added to the cart automatically once they
    // sign in.
    if (!this.auth.isLoggedIn() || !this.auth.isBuyer()) {
      this.router.navigate(['/auth/login'], { queryParams: { returnUrl: `/products/${this.product.productId}` } });
      return;
    }

    this.cart.addItem({
      productId: this.product.productId,
      quantity: 1,
    }).subscribe({
      next: () => {
        this.added = true;
        this.cartDrawer.open();
        setTimeout(() => { this.added = false; }, 1500);
      },
      error: () => {
        this.added = false;
      },
    });
  }

  onEdit(event: Event): void {
    event.preventDefault();
    event.stopPropagation();
    this.editProduct.emit(this.product);
  }

  onDelete(event: Event): void {
    event.preventDefault();
    event.stopPropagation();
    this.deleteProduct.emit(this.product);
  }
}
