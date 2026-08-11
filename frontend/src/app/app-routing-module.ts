import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './core/auth/auth.guard';
import { AdminGuard } from './core/auth/admin.guard';
import { SellerGuard } from './core/auth/guards/seller.guard';
import { BuyerGuard } from './core/auth/guards/buyer.guard';
import { NotFound } from './shared/components/not-found/not-found';
import { ServerError } from './shared/components/server-error/server-error';

const routes: Routes = [
  {
    path: '',
    loadChildren: () => import('./features/home/home-module').then(m => m.HomeModule)
  },
  {
    path: 'products',
    loadChildren: () => import('./features/products/products-module').then(m => m.ProductsModule)
  },
  {
    path: 'cart',
    loadChildren: () => import('./features/cart/cart-module').then(m => m.CartModule)
  },
  {
    path: 'coupons',
    loadChildren: () => import('./features/coupons/coupons-module').then(m => m.CouponsModule)
  },
  {
    path: 'orders',
    canActivate: [BuyerGuard],
    loadChildren: () => import('./features/orders/orders-module').then(m => m.OrdersModule)
  },
  {
    path: 'profile',
    canActivate: [BuyerGuard],
    loadChildren: () => import('./features/profile/profile-module').then(m => m.ProfileModule)
  },
  {
    path: 'admin',
    canActivate: [AdminGuard],
    loadChildren: () => import('./features/admin/admin-module').then(m => m.AdminModule)
  },
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth-module').then(m => m.AuthModule)
  },
  {
    path: 'seller',
    canActivate: [SellerGuard],
    loadChildren: () => import('./features/seller/seller-module').then(m => m.SellerModule)
  },
  { path: '404', component: NotFound },
  { path: '500', component: ServerError },
  { path: '**', component: NotFound }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
