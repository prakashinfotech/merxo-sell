import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared-module';
import { Orders } from './orders';

const routes: Routes = [{ path: '', component: Orders }];

@NgModule({
  declarations: [Orders],
  imports: [SharedModule, FormsModule, RouterModule.forChild(routes)],
})
export class OrdersModule {}
