import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Header } from './components/header/header';
import { Footer } from './components/footer/footer';
import { CurrencySelector } from './components/currency-selector/currency-selector';
import { AppCurrencyPipe } from './pipes/app-currency.pipe';
import { ProductCard } from './components/product-card/product-card';
import { CartDrawer } from './components/cart-drawer/cart-drawer';
import { SkeletonCard } from './components/skeleton-card/skeleton-card';
import { FilterBar } from './components/filter-bar/filter-bar';
import { FilterSidebar } from './components/filter-sidebar/filter-sidebar';
import { ProductGrid } from './components/product-grid/product-grid';
import { ConfirmDialog } from './components/confirm-dialog/confirm-dialog';
import { Pagination } from './components/pagination/pagination';
import { ToastHost } from './components/toast-host/toast-host';
import { NotFound } from './components/not-found/not-found';
import { ServerError } from './components/server-error/server-error';
import { LoadingOverlay } from './components/loading-overlay/loading-overlay';
import { HeroBanner } from './components/offer-banner/hero-banner/hero-banner';
import { MidPromoBanner } from './components/offer-banner/mid-promo-banner/mid-promo-banner';
import { StripBanner } from './components/offer-banner/strip-banner/strip-banner';
import { MatDialogModule } from '@angular/material/dialog';

@NgModule({
  declarations: [
    Header, Footer, CurrencySelector, AppCurrencyPipe,
    ProductCard, CartDrawer, SkeletonCard, FilterBar, FilterSidebar, ProductGrid, ConfirmDialog,
    Pagination, ToastHost, NotFound, ServerError, LoadingOverlay,
    HeroBanner, MidPromoBanner, StripBanner,
  ],
  imports: [CommonModule, RouterModule, MatDialogModule],
  exports: [
    Header, Footer, CurrencySelector, AppCurrencyPipe,
    ProductCard, CartDrawer, SkeletonCard, FilterBar, FilterSidebar, ProductGrid, ConfirmDialog,
    Pagination, ToastHost, NotFound, ServerError, LoadingOverlay,
    HeroBanner, MidPromoBanner, StripBanner,
    CommonModule, RouterModule, MatDialogModule,
  ],
})
export class SharedModule {}

