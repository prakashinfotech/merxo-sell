import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SellerShell } from './seller-shell/seller-shell';
import { SellerDashboard } from './seller-dashboard/seller-dashboard';
import { SellerProducts } from './seller-products/seller-products';
import { SellerProductForm } from './seller-product-form/seller-product-form';
import { SellerOrders } from './seller-orders/seller-orders';
import { SellerProfile } from './seller-profile/seller-profile';
import { SellerReviews } from './seller-reviews/seller-reviews';
import { SellerGuard } from '../../core/auth/guards/seller.guard';

const routes: Routes = [
  {
    path: '',
    component: SellerShell,
    canActivate: [SellerGuard],
    children: [
      { path: 'dashboard', component: SellerDashboard },
      { path: 'products', component: SellerProducts },
      { path: 'products/new', component: SellerProductForm },
      { path: 'products/:id/edit', component: SellerProductForm },
      { path: 'orders', component: SellerOrders },
      { path: 'reviews', component: SellerReviews },
      { path: 'profile', component: SellerProfile },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class SellerRoutingModule { }
