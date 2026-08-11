import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { SharedModule } from '../../shared/shared-module';
import { MaterialDropdownModule } from '../../shared/material/material-dropdown.module';
import { MaterialDatepickerModule } from '../../shared/material/material-datepicker.module';
import { Admin } from './admin';
import { AdminDashboard } from './dashboard/admin-dashboard';
import { AdminProducts } from './products/admin-products';
import { Sellers } from './sellers/sellers';
import { Orders } from './orders/orders';
import { Users } from './users/users';
import { AdminCustomers } from './customers/admin-customers';
import { AdminCategories } from './categories/admin-categories';
import { Currencies } from './currencies/currencies';
import { AdminReviews } from './reviews/admin-reviews';
import { AdminCoupons } from './coupons/admin-coupons';
import { ModerationProducts } from './moderation/moderation-products';
import { ModerationReviews } from './moderation/moderation-reviews';
import { AdminBanners } from './banners/admin-banners';

const routes: Routes = [
  {
    path: '',
    component: Admin,
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: AdminDashboard },
      { path: 'products', component: AdminProducts },
      { path: 'categories', component: AdminCategories },
      { path: 'sellers', component: Sellers },
      { path: 'customers', component: AdminCustomers },
      { path: 'orders', component: Orders },
      { path: 'users', component: Users },
      { path: 'currencies', component: Currencies },
      { path: 'reviews', component: AdminReviews },
      { path: 'coupons', component: AdminCoupons },
      { path: 'moderation/products', component: ModerationProducts },
      { path: 'moderation/reviews',  component: ModerationReviews  },
      { path: 'banners',             component: AdminBanners        },
    ],
  },
];

@NgModule({
  declarations: [
    Admin,
    AdminDashboard,
    AdminProducts,
    AdminCategories,
    Sellers,
    AdminCustomers,
    Orders,
    Users,
    Currencies,
    AdminReviews,
    AdminCoupons,
    ModerationProducts,
    ModerationReviews,
    AdminBanners,
  ],
  imports: [
    CommonModule, FormsModule, SharedModule,
    MaterialDropdownModule, MaterialDatepickerModule,
    RouterModule.forChild(routes),
  ],
})
export class AdminModule {}
