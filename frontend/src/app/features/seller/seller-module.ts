import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MaterialDropdownModule } from '../../shared/material/material-dropdown.module';
import { MaterialDatepickerModule } from '../../shared/material/material-datepicker.module';
import { SharedModule } from '../../shared/shared-module';
import { SellerRoutingModule } from './seller-routing-module';
import { SellerShell } from './seller-shell/seller-shell';
import { SellerDashboard } from './seller-dashboard/seller-dashboard';
import { SellerProducts } from './seller-products/seller-products';
import { SellerProductForm } from './seller-product-form/seller-product-form';
import { SellerOrders } from './seller-orders/seller-orders';
import { SellerProfile } from './seller-profile/seller-profile';
import { SellerReviews } from './seller-reviews/seller-reviews';

@NgModule({
  declarations: [
    SellerShell,
    SellerDashboard,
    SellerProducts,
    SellerProductForm,
    SellerOrders,
    SellerProfile,
    SellerReviews
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    MaterialDropdownModule,
    MaterialDatepickerModule,
    SharedModule,
    SellerRoutingModule,
  ],
})
export class SellerModule { }
