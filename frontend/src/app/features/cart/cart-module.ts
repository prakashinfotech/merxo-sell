import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '../../shared/shared-module';
import { FormsModule } from '@angular/forms';
import { Cart } from './cart';
import { Checkout } from './checkout';

const routes: Routes = [
  { path: '', component: Cart },
  { path: 'checkout', component: Checkout }
];

@NgModule({
  declarations: [Cart, Checkout],
  imports: [SharedModule, FormsModule, RouterModule.forChild(routes)]
})
export class CartModule {}
