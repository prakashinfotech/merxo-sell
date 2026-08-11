import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '../../shared/shared-module';
import { Coupons } from './coupons';

const routes: Routes = [{ path: '', component: Coupons }];

@NgModule({
  declarations: [Coupons],
  imports: [SharedModule, RouterModule.forChild(routes)]
})
export class CouponsModule {}
