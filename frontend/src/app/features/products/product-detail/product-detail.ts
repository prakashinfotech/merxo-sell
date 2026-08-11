import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CurrencyService } from '../../../core/currency/currency.service';
import { CartService } from '../../../core/cart/cart.service';
import { CartDrawerService } from '../../../core/cart/cart-drawer.service';
import { AuthService } from '../../../core/auth/auth.service';
import {
  ProductDetail as ProductDetailModel,
  ProductImage,
  ProductVariant
} from '../../../shared/models/product.models';
import { ProductService } from '../product.service';
import { BrowsingHistoryService } from '../../../core/browsing-history/browsing-history.service';
import { ReviewDto, ReviewService } from '../../../core/review/review.service';

@Component({
  selector: 'app-product-detail',
  templateUrl: './product-detail.html',
  styleUrl: './product-detail.scss',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductDetail implements OnInit {
  product: ProductDetailModel | null = null;
  loading = true;
  error   = false;

  selectedVariant: ProductVariant | null = null;
  filteredImages: ProductImage[] = [];
  quantity = 1;
  cartAdded = false;
  reviews: ReviewDto[] = [];
  loadingReviews = false;
  deliveryInput = '';
  deliveryEstimate = '';
  deliveryError = '';

  constructor(
    private route:          ActivatedRoute,
    private router:         Router,
    private productService: ProductService,
    readonly cs:            CurrencyService,
    readonly auth:          AuthService,
    private cart:           CartService,
    private cartDrawer:     CartDrawerService,
    private history:        BrowsingHistoryService,
    private reviewsApi:     ReviewService,
    private cdr:            ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.productService.getById(id).subscribe({
      next:  p  => {
        this.product = p;
        this.filteredImages = p.images ?? [];
        // Ensure the page starts at the top when a new product is loaded.
        window.scrollTo({ top: 0, behavior: 'smooth' });
        this.loading = false;
        this.history.record(p.productId).subscribe({ error: () => undefined });
        this.loadReviews(p.productId);
        this.cdr.markForCheck();
      },
      error: () => { this.error   = true; this.loading = false; this.cdr.markForCheck(); }
    });
  }

  get isAdmin(): boolean { return this.auth.isAdmin(); }
  get canCheckDelivery(): boolean { return this.auth.isLoggedIn() && this.auth.isBuyer(); }

  get displayPrice(): number {
    const base = this.product!.isOnSale && this.product!.salePrice != null
      ? this.product!.salePrice
      : this.product!.basePrice;
    const delta = this.selectedVariant?.priceDelta ?? this.selectedVariant?.priceAdjustment ?? 0;
    return base + delta;
  }

  /** Stock + cap shown next to the qty stepper — variant-aware. */
  get effectiveStock(): number {
    if (!this.product) return 0;
    if (this.selectedVariant) return this.selectedVariant.stock;
    return this.product.availableStock;
  }

  get currentSku(): string | null {
    return this.selectedVariant?.sku ?? null;
  }

  onVariantSelect(variant: ProductVariant | null): void {
    this.selectedVariant = variant;

    // Swap the gallery: show variant images if any are mapped.
    // When a variant is selected, we ONLY show images for that variant.
    // If no specific variant images exist, we show the fallback (all).
    const all = this.product?.images ?? [];
    if (variant && variant.imageIds && variant.imageIds.length > 0) {
      const ids = new Set(variant.imageIds);
      this.filteredImages = all.filter(i => ids.has(i.imageId));
    } else if (variant && variant.color) {
      // Fall back to images tagged with any variant matching this colour.
      const sameColorVariantIds = new Set(
        (this.product?.variants ?? [])
          .filter(v => v.color === variant.color)
          .map(v => v.variantId)
      );
      const variantImgs = all.filter(i => i.variantIds?.some(vid => sameColorVariantIds.has(vid)));
      this.filteredImages = variantImgs.length > 0 ? variantImgs : all;
    } else {
      this.filteredImages = all;
    }

    // Clamp quantity to the new effective stock so the user can't over-order.
    if (this.quantity > this.effectiveStock && this.effectiveStock > 0) {
      this.quantity = this.effectiveStock;
    }
    this.cdr.markForCheck();
  }

  decrement(): void { if (this.quantity > 1) this.quantity--; }
  increment(): void { this.quantity++; }

  addToCart(): void {
    if (!this.product) return;

    // Anonymous + non-buyer roles: send to login.
    if (!this.auth.isLoggedIn() || !this.auth.isBuyer()) {
      this.router.navigate(['/auth/login'], { queryParams: { returnUrl: `/products/${this.product.productId}` } });
      return;
    }

    this.cart.addItem({
      productId: this.product.productId,
      variantId: this.selectedVariant?.variantId,
      quantity: this.quantity
    }).subscribe({
      next: () => {
        this.cartAdded = true;
        this.cartDrawer.open();
        setTimeout(() => { this.cartAdded = false; this.cdr.markForCheck(); }, 2000);
        this.cdr.markForCheck();
      },
      error: () => { this.cdr.markForCheck(); },
    });
  }

  checkDelivery(): void {
    const value = this.deliveryInput.trim();
    this.deliveryEstimate = '';
    this.deliveryError = '';

    if (value.length < 4) {
      this.deliveryError = 'Enter a valid pincode or address.';
      this.cdr.markForCheck();
      return;
    }

    const seed = [...value].reduce((sum, ch) => sum + ch.charCodeAt(0), 0);
    const minDays = 2 + (seed % 2);
    const maxDays = minDays + 3;
    const start = new Date();
    const end = new Date();
    start.setDate(start.getDate() + minDays);
    end.setDate(end.getDate() + maxDays);
    this.deliveryEstimate = `Expected delivery ${start.toLocaleDateString()} - ${end.toLocaleDateString()}`;
    this.cdr.markForCheck();
  }

  stars(rating: number): number[] {
    return Array.from({ length: 5 }, (_, i) => i + 1).filter(i => i <= rating);
  }

  emptyStars(rating: number): number[] {
    return Array.from({ length: 5 }, (_, i) => i + 1).filter(i => i > rating);
  }

  getStarPercentage(star: number): number {
    if (!this.product?.ratingSummary || this.product.ratingSummary.totalCount === 0) return 0;
    const count = this.product.ratingSummary.distribution[star] || 0;
    return Math.round((count / this.product.ratingSummary.totalCount) * 100);
  }

  getStarCount(star: number): number {
    return this.product?.ratingSummary?.distribution[star] || 0;
  }

  private loadReviews(productId: number): void {
    this.loadingReviews = true;
    this.reviewsApi.getProductReviews(productId).subscribe({
      next: rows => {
        this.reviews = rows;
        this.loadingReviews = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.loadingReviews = false;
        this.cdr.markForCheck();
      }
    });
  }
}
